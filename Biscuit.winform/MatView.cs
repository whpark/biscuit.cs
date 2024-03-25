using System.Diagnostics;
using SharpGL;
using Biscuit;
using CV = OpenCvSharp;
using SharpGL.SceneGraph.Assets;
using System.Drawing.Imaging;
using OpenCvSharp;
using static System.Windows.Forms.DataFormats;
using System.Text.Json.Serialization;
using System;
using Biscuit.winform;
using Microsoft.VisualBasic.Devices;
using System.IO.Compression;
using System.Text.Json;
using Microsoft.Win32;
using SharpGL.SceneGraph;
using System.Runtime.InteropServices;
using System.Text;

namespace Biscuit.winform {
	public partial class xMatView : UserControl {

		private CV.Mat? m_img;
		public CV.Mat? Image {
			get => m_img;
		}

		//----------------------------------------------------------
		public enum eZOOM { none = -1, one2one, fit2window, fit2width, fit2height, mouse_wheel_locked, free };  // lock : 
		public enum eZOOM_IN { nearest, linear, bicubic, lanczos4/* EDSR, ESPCN, FSRCNN, LapSRN */};
		public enum eZOOM_OUT { nearest, area, };

		static Dictionary<eZOOM_IN, CV.InterpolationFlags> s_mapZoomInInterpolation = new () {
			{eZOOM_IN.nearest, CV.InterpolationFlags.Nearest},
			{eZOOM_IN.linear, CV.InterpolationFlags.Linear},
			{eZOOM_IN.bicubic, CV.InterpolationFlags.Cubic},
			{eZOOM_IN.lanczos4, CV.InterpolationFlags.Lanczos4},
		};
		static Dictionary<eZOOM_OUT, CV.InterpolationFlags> s_mapZoomOutInterpolation = new () {
			{eZOOM_OUT.nearest, CV.InterpolationFlags.Nearest},
			{eZOOM_OUT.area, CV.InterpolationFlags.Area},
		};

		[JsonConverter(typeof(JsonStringEnumConverter))]
		public struct sSettings {
			public bool bZoomLock = false;                                 // if eZoom is NOT free, zoom is locked
			public bool bPanningLock = true;                               // image can be moved only eZoom == free
			public bool bExtendedPanning = true;                           // image can move out of screen
			public bool bKeyboardNavigation = false;                       // page up/down, scroll down to next blank of image
			public bool bDrawPixelValue = true;                            // draw pixel value on image
			public bool bPyrImageDown = true;                              // build cache image for down sampling. (such as mipmap)
			public double dPanningSpeed = 2.0;                             // Image Panning Speed. 1.0 is same with mouse move.
			public int nScrollMargin = 5;                                  // bExtendedPanning, px margin to scroll
			public uint tsScroll = 250;                                    // Smooth Scroll. duration (in ms)
			public eZOOM_IN eZoomIn = eZOOM_IN.nearest;
			public eZOOM_OUT eZoomOut = eZOOM_OUT.area;
			public CV.Vec3b crBackground = new CV.Vec3b(161, 114, 230);    // rgb

			public sSettings() { }
		};
		string m_strRegKey = "";    // Registry Key for saving settings
		sSettings m_settings = new sSettings();

		struct sScrollGeometry {
			public CV.Rect rectClient, rectImageScreen, rectScrollRange;

			public sScrollGeometry() {
				rectClient = new();
				rectImageScreen = new();
				rectScrollRange = new();
			}
			public sScrollGeometry(CV.Rect rectClient, CV.Rect rectImageScreen, CV.Rect rectScrollRange) {
				this.rectClient = rectClient;
				this.rectImageScreen = rectImageScreen;
				this.rectScrollRange = rectScrollRange;
			}

		};

		CV.Mat m_imgDisplay = new();  // image for screen
		struct sPyramid {

			bool bStop = false;

			public Mutex mtx = new();
			public List<CV.Mat> imagesThumbnail = new();
			Thread? threadPyramidMaker = null;

			public sPyramid() { }

			public bool StartMakePyramids(CV.Mat src, UInt64 minImageArea = 3000*2000) {
				if (threadPyramidMaker is not null) {
					bStop = true;
					threadPyramidMaker.Join();
					threadPyramidMaker = null;
				}

				if (src is null || src.Empty())
					return false;

				sPyramid self = this;
				threadPyramidMaker = new Thread(() => {
					CV.Mat img = src;
					while (!self.bStop && ((UInt64)img.Cols*(UInt64)img.Rows > minImageArea)) {
						CV.Mat imgPyr = new();
						CV.Cv2.PyrDown(img, imgPyr);
						{
							self.mtx.WaitOne();
							self.imagesThumbnail.Add(imgPyr);
							img = imgPyr;
							self.mtx.ReleaseMutex();
						}
					}
					self.threadPyramidMaker = null;
				});
				threadPyramidMaker.Start();

				return true;
			}
		}
		sPyramid m_pyramid = new();

		struct sMouseOperation {
			public bool bInSelectionMode = false;
			public bool bRectSelected = false;
			public CV.Point? ptAnchor = null;           // Anchor Point in Screen Coordinate
			public CV.Point ptOffset0 = new();         // offset backup
			public CV.Point2d ptSel0 = new(), ptSel1 = new();    // Selection Point in Image Coordinate

			public sMouseOperation() { }

			public void Clear() {
				bInSelectionMode = false;
				bRectSelected = false;
				ptAnchor = null;
				ptOffset0 = default;
				ptSel0 = ptSel1 = default;
			}
		};
		sMouseOperation m_mouse = new();

		struct sSmoothScroll {
			CV.Point2d pt0 = new(), pt1 = new();
			DateTime t0 = new (), t1 = new();

			public sSmoothScroll() { }
			public void Clear() {
				pt0 = pt1 = default;
				t0 = t1 = default;
			}
		}
		sSmoothScroll m_smooth_scroll = new();

		eZOOM m_eZoom = eZOOM.fit2window;
		xCoordTrans2d m_ctScreenFromImage = new();

		uint [] m_textures = new uint[2];


		//-------------------------------------------------------------------------------------------------------------------------
		public xMatView() {
			InitializeComponent();
		}

		public bool SetImage(CV.Mat img, bool bCenter = true, eZOOM eZoomMode = eZOOM.none, bool bCopy = false) {
			//wxBusyCursor wc;

			// original image
			if (bCopy) {
				m_img = new();
				img.CopyTo(m_img);
			}
			else
				m_img = img;

			m_pyramid.StartMakePyramids(img);

			// check (opengl) texture format
			var aImageFormat = GetGLImageFormatType(m_img.Type());
			if (aImageFormat is null)
				return false;
			if (aImageFormat?.eColorType == 0)
				return false;

			m_mouse.Clear();
			m_smooth_scroll.Clear();

			if (eZoomMode != eZOOM.none) {
				ui_cmbZoomMode.SelectedIndex = (int)eZoomMode;
				//wxCommandEvent evt;
				//OnCombobox_ZoomMode(evt);
			} else {
				//UpdateCT(bCenter);
				//UpdateScrollBars();
				//m_view->Refresh();
				//m_view->Update();
			}


			return true;
		}


		class sGLData {
			public static string vertexShaderSource = 
				@"
				#version 330 core
				layout(location = 0) in vec2 inPosition;
				layout(location = 1) in vec2 inTexCoord;
				out vec2 TexCoord;
				void main() {
					gl_Position = vec4(inPosition, 0.0, 1.0);
					TexCoord = inTexCoord;
				}
				";
			public static string fragmentShaderSource =
				@"
				#version 330 core
				in vec2 TexCoord;
				out vec4 FragColor;
				uniform sampler2D textureSampler;
				void main() {
					FragColor = texture(textureSampler, TexCoord);
				}
				";
			public uint shaderProgram = 0;
			public uint [] VAO = { 0 };
			public uint [] VBO = { 0 };

			public sGLData() {

			}
		}
		sGLData m_glData = new();

		private bool InitializeGL() {
			var gl = ui_gl.OpenGL;

			// Vertex Shader Source
			// Fragment Shader Source

			if (m_glData.shaderProgram == 0) {

				gl.GenVertexArrays(m_glData.VAO.Length, m_glData.VAO);
				gl.BindVertexArray(m_glData.VAO[0]);

				// Create Vertex Buffer Object (VBO)
				gl.GenBuffers(1, m_glData.VBO);
				gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, m_glData.VBO[0]);

				// Define vertices and texture coordinates for a quad
				float [] vertices = {
					// Position    // Texture Coordinates
					0.0f, 1.0f,  0.0f, 0.0f,
					1.0f, 0.0f,  1.0f, 0.0f,
					1.0f, 1.0f,  1.0f, 1.0f,
					0.0f, 1.0f,  0.0f, 1.0f,
				};
				gl.BufferData(OpenGL.GL_ARRAY_BUFFER, vertices, OpenGL.GL_STATIC_DRAW);

				// Create and compile the vertex shader
				var vertexShader = gl.CreateShader(OpenGL.GL_VERTEX_SHADER);
				gl.ShaderSource(vertexShader, sGLData.vertexShaderSource);
				gl.CompileShader(vertexShader);

				// Create and compile the fragment shader
				uint fragmentShader = gl.CreateShader(OpenGL.GL_FRAGMENT_SHADER);
				gl.ShaderSource(fragmentShader, sGLData.fragmentShaderSource);
				gl.CompileShader(fragmentShader);

				// Create and link the shader program
				m_glData.shaderProgram = gl.CreateProgram();
				gl.AttachShader(m_glData.shaderProgram, vertexShader);
				gl.AttachShader(m_glData.shaderProgram, fragmentShader);
			}
			if (m_glData.shaderProgram != 0) {
				gl.LinkProgram(m_glData.shaderProgram);
				gl.UseProgram(m_glData.shaderProgram);

				gl.Uniform1(gl.GetUniformLocation(m_glData.shaderProgram, "textureSampler"), 0);
			}

			// Specify vertex attribute data
			gl.VertexAttribPointer(0, 2, OpenGL.GL_FLOAT, false, 4 * sizeof(float), IntPtr.Zero);
			gl.EnableVertexAttribArray(0);
			gl.VertexAttribPointer(1, 2, OpenGL.GL_FLOAT, false, 4 * sizeof(float), new IntPtr(2 * sizeof(float)));
			gl.EnableVertexAttribArray(1);

			return true;
		}


		private bool Setup() {
			var img = m_img;
			if (img is null)
				return false;

			var gl = ui_gl.OpenGL;
			gl.LoadIdentity();

			gl.Enable(OpenGL.GL_TEXTURE_2D);

			gl.GenTextures(1, m_textures);
			gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[0]);
			uint pixelFormat = OpenGL.GL_LUMINANCE;
			uint bytesPerPixel = 1;
			if (img.Type() == CV.MatType.CV_8UC1) {
				pixelFormat = OpenGL.GL_LUMINANCE;
				bytesPerPixel = 1;
			}
			else if (img.Type() == CV.MatType.CV_8UC3) {
				pixelFormat = OpenGL.GL_BGR;
				bytesPerPixel = 3;
			}
			else if (img.Type() == CV.MatType.CV_8UC4) {
				pixelFormat = OpenGL.GL_BGRA;
				bytesPerPixel = 4;
			}
			if ((img.Step() % 4) != 0)
				gl.PixelStore(OpenGL.GL_UNPACK_ALIGNMENT, 1);
			else
				gl.PixelStore(OpenGL.GL_UNPACK_ALIGNMENT, 4);

			gl.TexImage2D(OpenGL.GL_TEXTURE_2D, 0, bytesPerPixel, img.Cols, img.Rows, 0, pixelFormat, OpenGL.GL_UNSIGNED_BYTE, img.Ptr());

			//  Specify linear filtering.
			gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR);
			gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);

			return true;
		}

		private void MatView_Load(object sender, EventArgs e) {
			InitializeGL();

			// Zoom Mode
			var names = Enum.GetNames(typeof(eZOOM));
			var eZooms = typeof(eZOOM).GetEnumValues();
			for (int i = 0; i < names.Length; i++) {
				if ((int)(eZooms.GetValue(i)??-1) < 0)
					continue;
				ui_cmbZoomMode.Items.Add(names[i]);
			}

			// Init OpenGL
			var gl = ui_gl.OpenGL;
			gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

			ui_gl.OpenGLDraw += ui_gl_OpenGLDraw;
		}

		CV.Rect GetViewRect() {
			var r = ui_gl.ClientRectangle;
			//return new CV.Rect(r.X, r.Y, r.Width, r.Height);
			return new CV.Rect(0, 0, r.Width, r.Height);
		}

		bool UpdateCT(bool bCenter = false, eZOOM eZoom = eZOOM.none) {

			if (m_img is null || m_img.Empty())
				return false;
			if (eZoom == eZOOM.none)
				eZoom = m_eZoom;

			ui_cmbZoomMode.SelectedIndex = (int)eZoom;

			CV.Rect rectClient = GetViewRect();
			CV.Size sizeClient = rectClient.Size;

			// scale
			double dScale = -1.0;
			switch (eZoom) {
			case eZOOM.one2one:		dScale = 1.0; break;
			case eZOOM.fit2window:	dScale = Math.Min((double)sizeClient.Width / m_img.Cols, (double)sizeClient.Height / m_img.Rows); break;
			case eZOOM.fit2width:	dScale = (double)sizeClient.Width / m_img.Cols; break;
			case eZOOM.fit2height:	dScale = (double)sizeClient.Height / m_img.Rows; break;
			//case free:			dScale = m_ctScreenFromImage.m_scale; break;
			}
			if (dScale > 0)
				m_ctScreenFromImage.Scale = dScale;

			// constraints. make image put on the center of the screen
			if ( bCenter || eZoom switch { eZOOM.fit2window => true, eZOOM.fit2width => true, eZOOM.fit2height => true, _ => false } ) {

				xCoordTrans2d ct2 = new xCoordTrans2d(m_ctScreenFromImage); // copy
				ct2.m_origin.X = m_img.Cols/2.0;
				ct2.m_origin.Y = m_img.Rows/2.0;
				ct2.m_offset.X = rectClient.X + rectClient.Width/2.0;
				ct2.m_offset.Y = rectClient.Y + rectClient.Height/2.0;

				CV.Point2d ptOrigin = new();
				CV.Point ptLT = misc.Floor(ct2.Trans(ptOrigin));

				if (bCenter || eZoom == eZOOM.fit2window) {
					m_ctScreenFromImage.m_origin = new(0.0, 0.0);
					m_ctScreenFromImage.m_offset = ptLT;
				}
				else if (eZoom == eZOOM.fit2width) {
					m_ctScreenFromImage.m_origin.X = 0;
					m_ctScreenFromImage.m_offset.X = ptLT.X;
				}
				else if (eZoom == eZOOM.fit2height) {
					m_ctScreenFromImage.m_origin.Y = 0;
					m_ctScreenFromImage.m_offset.Y = ptLT.Y;
				}
			}

			// panning constraints.
			CV.Point2d pt0 = m_ctScreenFromImage.Trans(new CV.Point2d(0,0));
			CV.Point2d pt1 = m_ctScreenFromImage.Trans(new CV.Point2d(m_img.Cols, m_img.Rows));
			CV.Rect rectImageScreen = new(misc.Floor(pt0), misc.Floor(new CV.Size2d(pt1.X - pt0.X, pt1.Y - pt0.Y)));
			rectImageScreen = misc.NormalizeRect(rectImageScreen);

			if (m_settings.bExtendedPanning) {
				int marginX = Math.Min(m_settings.nScrollMargin, rectImageScreen.Width);
				int marginY = Math.Min(m_settings.nScrollMargin, rectImageScreen.Height);
				// make any part of image stays inside the screen
				if (rectImageScreen.Right < rectClient.Left) {
					m_ctScreenFromImage.m_offset.X += rectClient.Left - rectImageScreen.Right + marginX;
				}
				else if (rectImageScreen.Left > rectClient.Right) {
					m_ctScreenFromImage.m_offset.X += rectClient.Right - rectImageScreen.Left - marginX;
				}
				if (rectImageScreen.Bottom < rectClient.Top) {
					m_ctScreenFromImage.m_offset.Y += rectClient.Top - rectImageScreen.Bottom + marginY;
				}
				else if (rectImageScreen.Top > rectClient.Bottom) {
					m_ctScreenFromImage.m_offset.Y += rectClient.Bottom - rectImageScreen.Top - marginY;
				}
			} else {
				// default panning. make image stays inside the screen
				if (rectImageScreen.Width <= rectClient.Width) {
					var pt = m_ctScreenFromImage.Trans(new CV.Point2d(m_img.Cols/2.0, 0.0));
					m_ctScreenFromImage.m_offset.X += misc.CenterPoint(rectClient).X - pt.X;
				}
				if (rectImageScreen.Width > rectClient.Width) {
					if (rectImageScreen.Left > rectClient.Left)
						m_ctScreenFromImage.m_offset.X += rectClient.Left - rectImageScreen.Left;
					else if (rectImageScreen.Right < rectClient.Right)
						m_ctScreenFromImage.m_offset.X += rectClient.Right - rectImageScreen.Right;
				}
				if (rectImageScreen.Height <= rectClient.Height) {
					var pt = m_ctScreenFromImage.Trans(new CV.Point2d(0, m_img.Rows/2.0));
					m_ctScreenFromImage.m_offset.Y += rectClient.Height / 2 - pt.Y;
				}
				if (rectImageScreen.Height > rectClient.Height) {
					if (rectImageScreen.Top > rectClient.Top)
						m_ctScreenFromImage.m_offset.Y += rectClient.Top - rectImageScreen.Top;
					else if (rectImageScreen.Bottom < rectClient.Bottom)
						m_ctScreenFromImage.m_offset.Y += rectClient.Bottom - rectImageScreen.Bottom;
				}
			}

			return true;
		}

		sScrollGeometry GetScrollGeometry() {
			CV.Rect rectClient = GetViewRect();
			var pt0 = m_ctScreenFromImage.Trans(new CV.Point2d(0,0));
			var pt1 = m_ctScreenFromImage.Trans(new CV.Point2d(m_img.Cols, m_img.Rows));
			CV.Rect rectImageScreen = CV.Rect.FromLTRB((int)pt0.X, (int)pt0.Y, (int)pt1.X, (int)pt1.Y);
			rectImageScreen = misc.NormalizeRect(rectImageScreen);
			CV.Rect rectScrollRange = rectClient;
			if (m_settings.bExtendedPanning) {
				rectScrollRange.X -= rectImageScreen.Width;
				rectScrollRange.Y -= rectImageScreen.Height;
				rectScrollRange.Width += rectClient.Width;
				rectScrollRange.Height += rectClient.Height;
				int nScrollMarginX = Math.Min(m_settings.nScrollMargin, rectImageScreen.Width);
				int nScrollMarginY = Math.Min(m_settings.nScrollMargin, rectImageScreen.Height);
				misc.DeflateRect(ref rectScrollRange, nScrollMarginX, nScrollMarginY);
			}
			else {
				rectScrollRange |= rectImageScreen;
			}
			return new sScrollGeometry( rectClient, rectImageScreen, rectScrollRange);
		}

		bool UpdateScrollBars() {
			if (m_img is null || m_img.Empty())
				return false;
			var s = GetScrollGeometry();
			var rectClient = s.rectClient;
			var rectImageScreen = s.rectImageScreen;
			var rectScrollRange = s.rectScrollRange;

			var sbHorz = ui_scrollbarHorz;
			if (sbHorz.Visible) {
				double p0 = m_settings.bExtendedPanning
				? rectScrollRange.Width - (m_ctScreenFromImage.m_offset.X + Math.Max(0, rectImageScreen.Width - m_settings.nScrollMargin) + rectClient.Width)
				: -m_ctScreenFromImage.m_offset.X;
				int p = (int)p0;
				if (rectScrollRange.Width > 0) {
					p = misc.Clamp<int>(p, 0, rectScrollRange.Width);
					sbHorz.Minimum = 0;
					sbHorz.Maximum = Math.Max(0, rectScrollRange.Width - rectClient.Width);
					sbHorz.LargeChange = rectClient.Width;
					sbHorz.Value = p;
				}
			}

			var sbVert = ui_scrollbarVert;
			if (sbVert.Visible) {
				var p0 = m_settings.bExtendedPanning
				? rectScrollRange.Height - (m_ctScreenFromImage.m_offset.Y + Math.Max(0, rectImageScreen.Height - m_settings.nScrollMargin) + rectClient.Height)
				: -m_ctScreenFromImage.m_offset.Y;
				int p = (int)p0;
				if (rectScrollRange.Height > 0) {
					p = misc.Clamp(p, 0, rectScrollRange.Height);
					sbVert.Minimum = 0;
					sbVert.Maximum = Math.Max(0, rectScrollRange.Height-rectClient.Height);
					sbVert.LargeChange = rectClient.Height;
					sbVert.Value = p;
				}
			}

			double dScale = double.TryParse(ui_edtZoom.Text, out dScale) ? dScale : 100.0;
			if (m_ctScreenFromImage.Scale != dScale*0.01) {
				//m_bSkipSpinZoomEvent = true;
				ui_edtZoom.Text = $"{m_ctScreenFromImage.Scale*100.0}";
				//m_bSkipSpinZoomEvent = false;
			}

			return true;
		}

		void UpdateView() {
			ui_gl.Invalidate();
		}

		bool PutMatAsTexture(uint textureID, CV.Mat img, int width, CV.Rect rect, CV.Rect rectClient) {
			if (textureID == 0 || (img is null) || img.Empty() || !img.IsContinuous())
				return false;

			if (m_glData.shaderProgram == 0 || m_glData.VAO is null || m_glData.VBO is null) {
				return false;
				//return PutMatAsTexture(textureID, img, width, rect);
			}

			if (rectClient.Width <= 0 || rectClient.Height <= 0)
				return false;

			var gl = ui_gl.OpenGL;

			gl.BindTexture(OpenGL.GL_TEXTURE_2D, textureID);
			if ( (img.Step()%4) != 0 )
				gl.PixelStore(OpenGL.GL_UNPACK_ALIGNMENT, 1);
			else
				gl.PixelStore(OpenGL.GL_UNPACK_ALIGNMENT, 4);

			// Create the texture
			sImageFormat f = GetGLImageFormatType(img.Type())??default;
			if (f.eColorType == 0)
				return false;

			gl.TexImage2D(OpenGL.GL_TEXTURE_2D, 0, f.eColorType, img.Cols, img.Rows, 0, f.eFormat, f.ePixelType, img.Data);

			gl.Uniform1(gl.GetUniformLocation(m_glData.shaderProgram, "textureSampler"), 0);

			gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_NEAREST);
			gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_NEAREST);
			// Set texture clamping method
			gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_CLAMP_TO_BORDER);
			gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_CLAMP_TO_BORDER);

			// patch
			gl.BindVertexArray(m_glData.VAO[0]);
			gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, m_glData.VBO[0]);

			CV.Rect2f rc = CV.Rect2f.FromLTRB((float)rect.Left/(float)rectClient.Width, (float)rect.Top/(float)rectClient.Height,
				(float)rect.Right/(float)rectClient.Width, (float)rect.Bottom/(float)rectClient.Height);
			rc.Width = rc.X + rc.Width*2 - 1;
			rc.Height = rc.Y + rc.Height*2 - 1;
			rc.X = rc.X*2 - 1;
			rc.Y = rc.Y*2 - 1;

			// Define vertices and texture coordinates for a quad
			var r = (float)width/img.Cols;
			float [] vertices = {
				// Position				// Texture Coordinates
				//-r, -1.0f,			rc.right, rc.top,
				//r, -1.0f,				rc.left, rc.top,
				//r, 1.0f,				rc.left, rc.bottom,
				//-r, 1.0f,				rc.right, rc.bottom,
				rc.Left,  -rc.Top, 		0, 0,
				rc.Right, -rc.Top, 		r, 0,
				rc.Right, -rc.Bottom,	r, 1,
				rc.Left,  -rc.Bottom,	0, 1,
			};

			gl.BufferData(OpenGL.GL_ARRAY_BUFFER, vertices, OpenGL.GL_STATIC_DRAW);
			gl.DrawArrays(OpenGL.GL_TRIANGLE_FAN, 0, 4);

			gl.BindVertexArray(0);

			//glBegin(GL_QUADS);
			//glTexCoord2f(0.f, 0.f);	glVertex2i(rect.left,   rect.top);
			//glTexCoord2f(0.f, 1.f);	glVertex2i(rect.left,   rect.bottom);
			//glTexCoord2f(r, 1.f);	glVertex2i(rect.right,  rect.bottom);
			//glTexCoord2f(r, 0.f);	glVertex2i(rect.right,  rect.top);
			//glEnd();

			return true;
		}

		static CV.Scalar [] GetValue<T>(IntPtr ptr, int depth, int channel, int col0, int col1) where T : unmanaged {
			CV.Scalar [] v = new CV.Scalar[col1-col0];
			int el = channel*Marshal.SizeOf<T>();
			IntPtr offset = ptr + col0 * el;
			for (int x = col0; x < col1; x++) {
				for (int i = 0; i < channel; ++i, offset += el) {
					T value = Marshal.PtrToStructure<T>(offset);
					v[i] = Convert.ToDouble(value);
				}
			}
			return v;
		}

		public static CV.Scalar []? GetMatValue(IntPtr ptr, int depth, int channel, int col0, int col1) {
			switch (depth) {
			case CV.MatType.CV_8U:	return GetValue<Byte>(ptr, depth, channel, col0, col1);
			case CV.MatType.CV_8S:	return GetValue<SByte>(ptr, depth, channel, col0, col1);
			case CV.MatType.CV_16U:	return GetValue<UInt16>(ptr, depth, channel, col0, col1);
			case CV.MatType.CV_16S:	return GetValue<Int16>(ptr, depth, channel, col0, col1);
			case CV.MatType.CV_32S:	return GetValue<Int32>(ptr, depth, channel, col0, col1);
			case CV.MatType.CV_32F:	return GetValue<float>(ptr, depth, channel, col0, col1);
			case CV.MatType.CV_64F:	return GetValue<double>(ptr, depth, channel, col0, col1);
			}

			return null;
		}

		//! @brief Draw gridlines and pixel value of Mat to canvas.
		public static bool DrawPixelValue(ref CV.Mat canvas, in CV.Mat imgOriginal, in CV.Rect roi, in xCoordTrans2d ctCanvasFromImage, in double minTextHeight) {

			// Draw Grid / pixel value
			if (ctCanvasFromImage.Scale < 4)
				return false;
			CV.Scalar cr = new (127, 127, 127, 255);
			// grid - horizontal
			for (int y = roi.Y, y1 = roi.Y+roi.Height; y < y1; y++) {
				CV.Point2d pt0 = ctCanvasFromImage.Trans(new CV.Point2d(roi.X, y));
				CV.Point2d pt1 = ctCanvasFromImage.Trans(new CV.Point2d(roi.X+roi.Width, y));
				canvas.Line(misc.Floor(pt0), misc.Floor(pt1), cr);
			}
			// grid - vertical
			for (int x = roi.X, x1 = roi.X+ roi.Width; x < x1; x++) {
				CV.Point2d pt0 = ctCanvasFromImage.Trans(new CV.Point2d(x, roi.Y));
				CV.Point2d pt1 = ctCanvasFromImage.Trans(new CV.Point2d(x, roi.Y+roi.Height));
				canvas.Line(misc.Floor(pt0), misc.Floor(pt1), cr);
			}

			// Pixel Value
			int nChannel = imgOriginal.Channels();
			int depth = imgOriginal.Depth();

			if ( ctCanvasFromImage.Scale < ((nChannel+1.0)*minTextHeight) )
				return false;
			double heightFont = misc.Clamp(ctCanvasFromImage.Scale/(nChannel+1.0), 1.0, 40.0) / 40.0;
			//auto t0 = stdc::steady_clock::now();
			for (int y = roi.Y, y1 = roi.Y+roi.Height; y < y1; y++) {
				IntPtr ptr = imgOriginal.Ptr(y);
				int x1 = roi.X+roi.Width;
				//#pragma omp parallel for --------- little improvement
				CV.Scalar[]? vs = GetMatValue(ptr, depth, nChannel, roi.X, x1);
				if (vs is null)
					continue;
				for (int i = 0; i < vs.Length; i++) {
					var v = vs[i];
					CV.Point2d pt = ctCanvasFromImage.Trans(new CV.Point2d(roi.X+i, y));
					//auto p = SkPoint::Make(pt.x, pt.y);
					double avg = (v[0] + v[1] + v[2]) / nChannel;
					CV.Scalar crText = (avg > 128) ? new CV.Scalar(0, 0, 0, 255) : new CV.Scalar(255, 255, 255, 255);
					for (int ch = 0; ch < nChannel; ch++) {
						string str = $"{v[ch]:3}";
						canvas.PutText(str, new CV.Point(pt.X, pt.Y+(ch+1)*heightFont*40), CV.HersheyFonts.HersheyDuplex, heightFont, crText, 1, CV.LineTypes.Link8, false);
					}
				}
			}
			//auto t1 = stdc::steady_clock::now();
			//auto dur = stdc::duration_cast<stdc::milliseconds>(t1-t0).count();
			//OutputDebugString(std::format(L"{} ms\n", dur).c_str());

			return true;
		}


		void PaintGL(SharpGL.OpenGL gl) {
			if (m_img is null || m_img.Empty() || gl is null)
				return;

			//================
			// openGL

			// Client Rect
			CV.Rect rectClient = GetViewRect();
			CV.Size sizeView = rectClient.Size;
			gl.Viewport(0, 0, sizeView.Width, sizeView.Height);

			gl.MatrixMode(OpenGL.GL_PROJECTION);     // Make a simple 2D projection on the entire window
			gl.LoadIdentity();
			gl.Ortho(0.0, sizeView.Width, sizeView.Height, 0.0, 0.0, 100.0);

			gl.MatrixMode(OpenGL.GL_MODELVIEW);    // Set the matrix mode to object modeling
			CV.Scalar cr = new();
			cr[0] = m_settings.crBackground[0];
			cr[1] = m_settings.crBackground[1];
			cr[2] = m_settings.crBackground[2];

			gl.ClearColor((float)(cr[0]/255.0), (float)(cr[1]/255.0), (float)(cr[2]/255.0), 1.0f);
			gl.ClearDepth(0.0f);
			gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT); // Clear the window

			//auto* context = view->context();
			//if (!context)
			//	return;
			//auto* surface = view->context()->surface();

			//gtl::xFinalAction faSwapBuffer{[&]{
			//	//OutputDebugStringA("Flush\n");
			//	glFlush();
			//	context->swapBuffers(surface);
			//}};

			////event.Skip();
			//if (!view or !context)
			//	return;

			if (misc.IsRectEmpty(rectClient))
				return;

			//==================
			// Get Image - ROI
			var ct = m_ctScreenFromImage;

			// Position
			CV.Rect rectImage = misc.Floor(misc.MakeRectFromPts(ct.TransI(rectClient.TopLeft), ct.TransI(rectClient.BottomRight)));
			rectImage = misc.NormalizeRect(rectImage);
			misc.InflateRect(ref rectImage, 1, 1);
			if (rectImage.X < 0)
				rectImage.X = 0;
			if (rectImage.Y < 0)
				rectImage.Y = 0;
			if (rectImage.Right >= m_img.Cols)
				rectImage.Width = m_img.Cols - rectImage.X;
			if (rectImage.Bottom >= m_img.Rows)
				rectImage.Height = m_img.Rows - rectImage.Y;
			if (misc.IsRectEmpty(rectImage))
				return;

			CV.Rect roi = rectImage;
			CV.Rect rectTarget = misc.Floor(misc.MakeRectFromPts(ct.Trans(rectImage.TopLeft), ct.Trans(rectImage.BottomRight)));
			rectTarget = misc.NormalizeRect(rectTarget);
			if (rectTarget.Width == 0)
				rectTarget.Width = 1;
			if (rectTarget.Height == 0)
				rectTarget.Height = 1;
			if (!misc.IsROI_Valid(roi, m_img.Size()))
				return;

			// img (roi)
			CV.Rect rcTarget = new CV.Rect(new CV.Point(), new CV.Size(rectTarget.Width, rectTarget.Height));
			CV.Rect rcTargetC = rcTarget;   // 4 byte align
			if ((rcTarget.Width*m_img.ElemSize()) % 4 != 0)
				rcTargetC.Width = misc.AdjustAlign32(rcTargetC.Width);
			// check target image size
			if ((UInt64)rcTargetC.Width * (UInt64)rcTargetC.Height > (UInt64)(1ul *1024*1024*1024))
				return;

			CV.Mat img = new CV.Mat(rcTargetC.Size, m_img.Type());
			//img = m_option.crBackground;
			var eInterpolation = CV.InterpolationFlags.Linear;
			try {
				if (ct.Scale < 1.0) {
					if (s_mapZoomOutInterpolation.ContainsKey(m_settings.eZoomOut)) {
						eInterpolation = s_mapZoomOutInterpolation[m_settings.eZoomOut];
					}

					// resize from pyramid image
					double dScale = ct.Scale;
					CV.Size2d size = new (dScale*m_img.Cols, dScale*m_img.Rows);
					CV.Mat? imgPyr = null;
					//{
					//	auto t = std::chrono::steady_clock::now();
					//	OutputDebugStringA(std::format("=======================\n1 {}\n", std::chrono::duration_cast<std::chrono::milliseconds>(t-t0)).c_str());
					//	t0 = t;
					//}
					{
						m_pyramid.mtx.WaitOne();
						foreach (CV.Mat imgThumbnail in m_pyramid.imagesThumbnail) {
							if (imgThumbnail.Cols < size.Width)
								continue;
							imgPyr = img;
							break;
						}
						m_pyramid.mtx.ReleaseMutex();
					}
					//{
					//	auto t = std::chrono::steady_clock::now();
					//	OutputDebugStringA(std::format("=======================\n2 {}\n", std::chrono::duration_cast<std::chrono::milliseconds>(t-t0)).c_str());
					//	t0 = t;
					//}
					if (imgPyr is not null) {

						// show wait cursor
						Cursor.Current = Cursors.WaitCursor;

						double scaleP = m_img.Cols < m_img.Rows ? (double)imgPyr.Rows / m_img.Rows : (double)imgPyr.Cols / m_img.Cols;
						double scale = imgPyr.Cols < imgPyr.Rows ? (double)size.Height / imgPyr.Rows : (double)size.Width / imgPyr.Cols;
						CV.Rect roiP = roi;
						roiP.X = (int)(scaleP * roiP.X);
						roiP.Y = (int)(scaleP * roiP.Y);
						roiP.Width = (int)(scaleP * roiP.Width);
						roiP.Height = (int)(scaleP * roiP.Height);
						roiP = misc.GetSafeROI(roiP, imgPyr.Size());
						if (!misc.IsRectEmpty(roiP)) {
							CV.Mat imgSrc = imgPyr[roiP];
							imgSrc.Resize(rcTarget.Size, 0.0, 0.0, eInterpolation).CopyTo(img[rcTarget]);
						}
					}
				}
				else if (ct.Scale > 1.0) {
					if (s_mapZoomInInterpolation.ContainsKey(m_settings.eZoomIn)) {
						eInterpolation = s_mapZoomInInterpolation[m_settings.eZoomIn];
					}
					CV.Mat imgSrc = m_img[roi];
					imgSrc.Resize(rcTarget.Size, 0.0, 0.0, eInterpolation).CopyTo(img[rcTarget]);
				}
				else {
					m_img[roi].CopyTo(img[rcTarget]);
				}
			}
			catch (Exception e) {
				Debug.WriteLine($"Exception:{e}\n");
			}
			//catch (...) {
			//	//OutputDebugStringA("cv::.......\n");
			//}

			if (!img.Empty()) {
				if (m_settings.bDrawPixelValue) {
					xCoordTrans2d ctCanvas = new xCoordTrans2d(m_ctScreenFromImage);
					ctCanvas.m_offset -= m_ctScreenFromImage.Trans(roi.TopLeft);
					DrawPixelValue(ref img, m_img, roi, ctCanvas, 8);   // DPI...
				}

				gl.Enable(OpenGL.GL_BLEND);
				gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);

				gl.Enable(OpenGL.GL_TEXTURE_2D);
				uint [] textures = new uint[2]{ 0, 0 };
				gl.GenTextures(textures.Length, textures);
				if (textures[0] == 0) {
					Debug.WriteLine("glGenTextures failed");
					return;
				}

				//gtl::xFinalAction finalAction([&] {
				//	glDisable(OpenGL.GL_TEXTURE_2D);
				//	glDeleteTextures(std::size(textures), textures);
				//});

				// Use the shader program
				if (m_glData.shaderProgram != 0) {
					gl.UseProgram(m_glData.shaderProgram);
				}

				PutMatAsTexture(textures[0], img, rcTarget.Width, rectTarget, rectClient);

				// Draw Selection Rect
				if (m_mouse.bInSelectionMode || m_mouse.bRectSelected) {
					CV.Rect rect = misc.Floor(misc.MakeRectFromPts(m_ctScreenFromImage.Trans(m_mouse.ptSel0), m_ctScreenFromImage.Trans(m_mouse.ptSel1)));
					rect = misc.NormalizeRect(rect);
					rect &= rectClient;
					if (!misc.IsRectEmpty(rect)) {
						CV.Mat rectangle = new(16, 16, CV.MatType.CV_8UC4, new CV.Scalar(255, 255, 127, 128));
						PutMatAsTexture(textures[1], rectangle, rectangle.Cols, rect, rectClient);
					}
				}

				gl.Disable(OpenGL.GL_TEXTURE_2D);
				gl.DeleteTextures(textures.Length, textures);
			}

			gl.Flush();
		}


		//-------------------------------------------------------------------------------------------------------------------------
		// Operations
		public bool SetZoomMode(eZOOM eZoomMode, bool bCenter = true) {
			if ((int)eZoomMode < 0 || (int)eZoomMode >= ui_cmbZoomMode.Items.Count)
				return false;

			if (ui_cmbZoomMode.SelectedIndex != (int)eZoomMode) {
				ui_cmbZoomMode.SelectedIndex = (int)eZoomMode;
				// will be re-entered
				return true;
			}

			UpdateCT(bCenter, eZoomMode);
			UpdateScrollBars();
			UpdateView();

			return true;
		}

		public bool ZoomInOut(double step, CV.Point ptAnchor, bool bCenter) {
			var scale = m_ctScreenFromImage.Scale;
			if (step > 0) {
				foreach (var dZoom in s_dZoomLevels) {
					if (dZoom > scale) {
						scale = dZoom;
						break;
					}
				}
			}
			else {
				foreach (var dZoom in s_dZoomLevels.Reverse()) {
					if (dZoom < scale) {
						scale = dZoom;
						break;
					}
				}
			}
			return SetZoom(scale, ptAnchor, bCenter);
		}

		public bool SetZoom(double scale, CV.Point ptAnchor, bool bCenter) {
			if (m_img is null || m_img.Empty())
				return false;

			// Backup Image Position
			CV.Point2d ptImage = m_ctScreenFromImage.TransI(ptAnchor);
			// Get Scroll Amount
			if (m_ctScreenFromImage.Scale == scale)
				return true;
			else
				m_ctScreenFromImage.Scale = scale;

			var rectClient = GetViewRect();
			var dMinZoom = Math.Min(rectClient.Width/4.0 / m_img.Cols, rectClient.Height/4.0 / m_img.Rows);
			dMinZoom = Math.Min(dMinZoom, 0.5);
			m_ctScreenFromImage.Scale = misc.Clamp(m_ctScreenFromImage.Scale, dMinZoom, 1.0e3);
			m_ctScreenFromImage.m_offset += ptAnchor - m_ctScreenFromImage.Trans(ptImage);
			// Anchor point
			var eZoom = m_eZoom;
			if (eZoom != eZOOM.mouse_wheel_locked)
				eZoom = eZOOM.free;
			ui_cmbZoomMode.SelectedIndex = (int)eZoom;
			//OnCmbZoomMode_currentIndexChanged(std::to_underlying(eZoom));
			UpdateCT(bCenter);
			UpdateScrollBars();
			UpdateView();

			return true;
		}


		public CV.Rect? GetSelectionRect() {
			if (!m_mouse.bRectSelected)
				return null;
			CV.Rect rect = misc.MakeRectFromPts(misc.Floor(m_mouse.ptSel0), misc.Floor(m_mouse.ptSel1));
			rect = misc.NormalizeRect(rect);
			return rect;
		}

		public void SetSelectionRect(CV.Rect rect) {
			m_mouse.bRectSelected = true;
			m_mouse.ptSel0 = rect.TopLeft;
			m_mouse.ptSel1 = rect.BottomRight;
			UpdateView();
		}
		public void ClearSelectionRect() {
			m_mouse.bRectSelected = false;
			UpdateView();
		}

		public bool LoadOption() {
			if (m_strRegKey == "")
				return false;
			if (Registry.GetValue(m_strRegKey, "Settings", "") is string json) {
				m_settings = JsonSerializer.Deserialize<sSettings>(json);
			} else {
				return false;
			}
			return true;
		}
		public bool SaveOption() {
			if (m_strRegKey == "")
				return false;
			string json = JsonSerializer.Serialize(m_settings);
			Registry.SetValue(m_strRegKey, "Settings", json);
			return true;
		}

		public sSettings Settings {
			get => m_settings;
			set {
				m_settings = value;

				m_pyramid.StartMakePyramids(m_img);

				UpdateCT(false, eZOOM.none);
				UpdateScrollBars();
				UpdateView();
			}
		}



		private void ui_cmbZoomMode_SelectedIndexChanged(object sender, EventArgs e) {
			if (sender is xMatView view) {
				return;
			}
			SetZoomMode((eZOOM)ui_cmbZoomMode.SelectedIndex, false);
		}



		private void ui_gl_OpenGLDraw(object sender, SharpGL.RenderEventArgs args) {
			PaintGL(ui_gl.OpenGL);
		}



		//=========================================================================================================================
		// Zoom Level
		private static readonly double[] s_dZoomLevels = {
			1.0/8192, 1.0/4096, 1.0/2048, 1.0/1024,
			1.0/512, 1.0/256, 1.0/128, 1.0/64, 1.0/32, 1.0/16, 1.0/8, 1.0/4.0, 1.0/2.0,
			3.0/4, 1, 1.25, 1.5, 1.75, 2, 2.5, 3, 4, 5, 6, 7, 8, 9, 10,
			12.5, 15, 17.5, 20, 25, 30, 35, 40, 45, 50, 60, 70, 80, 90, 100,
			125.0, 150, 175, 200, 250, 300, 350, 400, 450, 500,
			600, 700, 800, 900, 1_000
			//1250, 1500, 1750, 2000, 2500, 3000, 3500, 4000, 4500, 5000,
			//6000, 7000, 8000, 9000, 10000,
			//12500, 15000, 17500, 20000, 25000, 30000, 35000, 40000, 45000, 50000,
			//60000, 70000, 80000, 90000, 100000,
			//125000, 150000, 175000, 200000, 250000, 300000, 350000, 400000, 450000, 500000,
			//600000, 700000, 800000, 900000, 1000000,
			//1250000, 1500000, 1750000, 2000000, 2500000, 3000000, 3500000, 4000000, 4500000, 5000000,
			//6000000, 7000000, 8000000, 9000000, 10000000,
			//12500000, 15000000, 17500000, 20000000, 25000000, 30000000, 35000000, 40000000, 45000000, 50000000,
			//60000000, 70000000, 80000000, 90000000
		};

		//=========================================================================================================================
		public struct sImageFormat {
			public uint eColorType, eFormat, ePixelType;

			public sImageFormat() {
				eColorType = eFormat = ePixelType = 0;
			}

			public sImageFormat(uint eColorType, uint eFormat, uint ePixelType) {
				this.eColorType = eColorType;
				this.eFormat = eFormat;
				this.ePixelType = ePixelType;
			}
		}
		private static Dictionary<int, sImageFormat> s_mapImageFormat = new() {
			{ CV.MatType.CV_8UC1,   new sImageFormat(1, OpenGL.GL_LUMINANCE,    OpenGL.GL_UNSIGNED_BYTE) },
			{ CV.MatType.CV_8UC3,   new sImageFormat(3, OpenGL.GL_RGB,          OpenGL.GL_UNSIGNED_BYTE) },
			{ CV.MatType.CV_8UC4,   new sImageFormat(4, OpenGL.GL_RGBA,         OpenGL.GL_UNSIGNED_BYTE) },
			{ CV.MatType.CV_16UC1,  new sImageFormat(1, OpenGL.GL_LUMINANCE,    OpenGL.GL_UNSIGNED_SHORT) },
			{ CV.MatType.CV_16UC3,  new sImageFormat(3, OpenGL.GL_RGB,          OpenGL.GL_UNSIGNED_SHORT) },
			{ CV.MatType.CV_16UC4,  new sImageFormat(4, OpenGL.GL_RGBA,         OpenGL.GL_UNSIGNED_SHORT) },
			{ CV.MatType.CV_16SC1,  new sImageFormat(1, OpenGL.GL_LUMINANCE,    OpenGL.GL_SHORT) },
			{ CV.MatType.CV_16SC3,  new sImageFormat(3, OpenGL.GL_RGB,          OpenGL.GL_SHORT) },
			{ CV.MatType.CV_16SC4,  new sImageFormat(4, OpenGL.GL_RGBA,         OpenGL.GL_SHORT) },
			{ CV.MatType.CV_32SC1,  new sImageFormat(1, OpenGL.GL_LUMINANCE,    OpenGL.GL_INT) },
			{ CV.MatType.CV_32SC3,  new sImageFormat(3, OpenGL.GL_RGB,          OpenGL.GL_INT) },
			{ CV.MatType.CV_32SC4,  new sImageFormat(4, OpenGL.GL_RGBA,         OpenGL.GL_INT) },
			{ CV.MatType.CV_32FC1,  new sImageFormat(1, OpenGL.GL_LUMINANCE,    OpenGL.GL_FLOAT) },
			{ CV.MatType.CV_32FC3,  new sImageFormat(3, OpenGL.GL_RGB,          OpenGL.GL_FLOAT) },
			{ CV.MatType.CV_32FC4,  new sImageFormat(4, OpenGL.GL_RGBA,         OpenGL.GL_FLOAT) }
		};
		public static sImageFormat? GetGLImageFormatType(int type) {
			if (s_mapImageFormat is null)
				return null;
			if (s_mapImageFormat.ContainsKey(type))
				return s_mapImageFormat[type];
			else
				return null;
		}

	}


}
