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

			Mutex mtx = new();
			List<CV.Mat> imagesThumbnail = new();
			Thread? threadPyramidMaker = null;

			public sPyramid() { }

			public bool StartMakePyramids(CV.Mat src, UInt64 minImageArea = 3000*2000) {
				if (threadPyramidMaker is not null) {
					bStop = true;
					threadPyramidMaker.Join();
					threadPyramidMaker = null;
				}

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

		bool UpdateCT(bool bCenter, eZOOM eZoom) {

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
				m_ctScreenFromImage.m_mat *= (dScale / m_ctScreenFromImage.m_mat.Determinant());

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
					m_ctScreenFromImage.m_offset.y += rectClient.Bottom - rectImageScreen.Top - marginY;
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
			if (m_ctScreenFromImage.m_mat.Determinant() != dScale*0.01) {
				//m_bSkipSpinZoomEvent = true;
				ui_edtZoom.Text = $"{m_ctScreenFromImage.m_mat.Determinant()*100.0}";
				//m_bSkipSpinZoomEvent = false;
			}

			return true;
		}

		void UpdateView() {
			ui_gl.Invalidate();
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
			Debug.WriteLine("ui_gl_OpenGLDraw event triggered");

			var gl = ui_gl.OpenGL;

			gl.Viewport(0, 0, ui_gl.ClientRectangle.Width, ui_gl.ClientRectangle.Height);

			gl.Enable(OpenGL.GL_TEXTURE_2D);

			gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
			gl.Viewport(0, 0, gl.RenderContextProvider.Width, gl.RenderContextProvider.Height);
			gl.LoadIdentity();
			//gl.Translate(0.0f, 0.0f, -5.0f);
			gl.Ortho2D(gl.RenderContextProvider.Width, gl.RenderContextProvider.Height, 0, 0);
			gl.MatrixMode(OpenGL.GL_MODELVIEW);

			//gl.Rotate(rotation, 0.0f, 1.0f, 0.0f);
			//rotation += 1.0f;

			gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[0]);
			//uint pixelFormat = OpenGL.GL_LUMINANCE;
			//if (img.Type() == CV.MatType.CV_8UC1) {
			//	pixelFormat = OpenGL.GL_LUMINANCE;
			//}
			//else if (img.Type() == CV.MatType.CV_8UC3) {
			//	pixelFormat = OpenGL.GL_BGR;
			//}
			//else if (img.Type() == CV.MatType.CV_8UC4) {
			//	pixelFormat = OpenGL.GL_BGRA;
			//}
			//gl.TexImage2D(OpenGL.GL_TEXTURE_2D, 0, pixelFormat, img.Cols, img.Rows, 0, pixelFormat, OpenGL.GL_UNSIGNED_BYTE, img.Data);

			gl.Begin(OpenGL.GL_QUADS);

			var rect = ui_gl.ClientRectangle;

			//gl.TexCoord(0.0f, 0.0f); gl.Vertex(rect.Left, rect.Bottom);
			//gl.TexCoord(1.0f, 0.0f); gl.Vertex(rect.Right, rect.Bottom);
			//gl.TexCoord(1.0f, 1.0f); gl.Vertex(rect.Right, rect.Top);
			//gl.TexCoord(0.0f, 1.0f); gl.Vertex(rect.Left, rect.Top);
			gl.TexCoord(0.0f, 0.0f); gl.Vertex(0.0f, 0.0f);
			gl.TexCoord(1.0f, 0.0f); gl.Vertex(10.0f, 0.0f);
			gl.TexCoord(1.0f, 1.0f); gl.Vertex(10.0f, 10.0f);
			gl.TexCoord(0.0f, 1.0f); gl.Vertex(0.0f, 10.0f);

			gl.End();

			gl.Flush();

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

			public sImageFormat(uint eColorType, uint eFormat, uint ePixelType) {
				this.eColorType = eColorType;
				this.eFormat = eFormat;
				this.ePixelType = ePixelType;
			}
		}
		private static Dictionary<int, sImageFormat> m_mapImageFormat = new() {
			{ CV.MatType.CV_8UC1,   new sImageFormat(1, OpenGL.GL_LUMINANCE,    OpenGL.GL_UNSIGNED_BYTE) },
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
			if (m_mapImageFormat.ContainsKey(type))
				return m_mapImageFormat[type];
			else
				return null;
		}

	}


}
