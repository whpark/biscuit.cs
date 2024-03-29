using System.Diagnostics;
using Biscuit;
using CV = OpenCvSharp;
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
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace Biscuit.winform {

	public partial class xMatView : UserControl {

		public CV.Mat? Image {
			get => m_img;
		}
		public RegistryKey? Reg { get; set; }
		public string RegKey { get; set; } = "MatView";    // Registry Key for saving settings

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

		//----------------------------------------------------------
		private CV.Mat? m_img;

		//----------------------------------------------------------
		public struct sSettings {
			public bool bZoomLock { get; set; } = false;                                 // if eZoom is NOT free, zoom is locked
			public bool bPanningLock { get; set; } = true;                               // image can be moved only eZoom == free
			public bool bExtendedPanning { get; set; } = true;                           // image can move out of screen
			public bool bKeyboardNavigation { get; set; } = false;                       // page up/down, scroll down to next blank of image
			public bool bDrawPixelValue { get; set; } = true;                            // draw pixel value on image
			public bool bPyrImageDown { get; set; } = true;                              // build cache image for down sampling. (such as mipmap)
			public double dPanningSpeed { get; set; } = 2.0;                             // Image Panning Speed. 1.0 is same with mouse move.
			public int nScrollMargin { get; set; } = 5;                                  // bExtendedPanning, px margin to scroll
			public uint tsScroll { get; set; } = 250;                                    // Smooth Scroll. duration (in ms)
			public eZOOM_IN eZoomIn { get; set; } = eZOOM_IN.nearest;
			public eZOOM_OUT eZoomOut { get; set; } = eZOOM_OUT.area;

			[JsonIgnore]
			public CV.Vec3b crBackground = new CV.Vec3b(161, 114, 230);                // rgb
			public Byte crBackgroundR { get => crBackground[2]; set { crBackground[2] = (Byte)value; } }
			public Byte crBackgroundG { get => crBackground[1]; set { crBackground[1] = (Byte)value; } }
			public Byte crBackgroundB { get => crBackground[0]; set { crBackground[0] = (Byte)value; } }

			public sSettings() { }
		};
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

		/// <summary>
		/// thumbnail pyramid
		/// </summary>
		struct sPyramid {

			bool bStop = false;

			public Mutex mtx = new();
			public List<CV.Mat> imagesThumbnail = new();
			Thread? threadPyramidMaker = null;

			public sPyramid() { }
			public bool StartMakePyramids(CV.Mat src, UInt64 minImageArea = 3000*2000) {
				var self = this;
				if (self.threadPyramidMaker is not null) {
					self.bStop = true;
					self.threadPyramidMaker.Join();
					self.threadPyramidMaker = null;
				}

				if (src is null || src.Empty())
					return false;

				threadPyramidMaker = new Thread(() => {
					CV.Mat img = src;
					{
						self.mtx.WaitOne();
						self.imagesThumbnail.Add(img);
						self.mtx.ReleaseMutex();
					}
					while (!self.bStop && ((UInt64)img.Cols*(UInt64)img.Rows > minImageArea)) {
						CV.Mat imgPyr = new();
						CV.Cv2.PyrDown(img, imgPyr);
						{
							self.mtx.WaitOne();
							self.imagesThumbnail.Insert(0, imgPyr);
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
			public CV.Point2d ptOffset0 = new();         // offset backup
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

		//-------------------------------------------------------------------------------------------------------------------------
		// Zoom Level
		private static readonly double[] s_dZoomLevels = {
			1.0/8192, 1.0/4096, 1.0/2048, 1.0/1024,
			1.0/512, 1.0/256, 1.0/128, 1.0/64, 1.0/32, 1.0/16, 1.0/8, 1.0/4.0, 1.0/2.0,
			3.0/4, 1, 1.25, 1.5, 1.75, 2, 2.5, 3, 4, 5, 6, 7, 8, 9, 10,
			12.5, 15, 17.5, 20, 25, 30, 35, 40, 45, 50, 60, 70, 80, 90, 100,
			125.0, 150, 175, 200, 250, 300, 350, 400, 450, 500,
			600, 700, 800, 900, 1_000
		};

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

			m_pyramid.StartMakePyramids(m_img);

			m_mouse.Clear();
			m_smooth_scroll.Clear();

			if (eZoomMode != eZOOM.none) {
				ui_cmbZoomMode.SelectedIndex = (int)eZoomMode;
				SetZoomMode(eZoomMode, false);
			}
			else {
				UpdateCT(bCenter);
				UpdateScrollBars();
				//m_view->Refresh();
				UpdateView();
			}

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
			ui_cmbZoomMode.SelectedIndex = (int)m_eZoom;

		}
		CV.Rect GetViewRect() {
			var r = ui_picture.DisplayRectangle;
			//return new CV.Rect(r.X, r.Y, r.Width, r.Height);
			return new CV.Rect(0, 0, r.Width, r.Height);
		}

		bool UpdateCT(bool bCenter = false, eZOOM eZoom = eZOOM.none) {
			if (m_img is null || m_img.Empty())
				return false;
			if (eZoom == eZOOM.none)
				eZoom = m_eZoom;

			if (ui_cmbZoomMode.Items.Count > (int)eZoom) {
				m_eZoom = eZoom;
				if (ui_cmbZoomMode.SelectedIndex != (int)eZoom)
					ui_cmbZoomMode.SelectedIndex = (int)eZoom;
			}

			CV.Rect rectClient = GetViewRect();
			CV.Size sizeClient = rectClient.Size;

			// scale
			double dScale = -1.0;
			switch (eZoom) {
			case eZOOM.one2one:
				dScale = 1.0;
				break;
			case eZOOM.fit2window:
				dScale = Math.Min((double)sizeClient.Width / m_img.Cols, (double)sizeClient.Height / m_img.Rows);
				break;
			case eZOOM.fit2width:
				dScale = (double)sizeClient.Width / m_img.Cols;
				break;
			case eZOOM.fit2height:
				dScale = (double)sizeClient.Height / m_img.Rows;
				break;
				//case free:			dScale = m_ctScreenFromImage.m_scale; break;
			}
			if (dScale > 0)
				m_ctScreenFromImage.Scale = dScale;

			// constraints. make image put on the center of the screen
			if (bCenter || eZoom switch { eZOOM.fit2window => true, eZOOM.fit2width => true, eZOOM.fit2height => true, _ => false }) {

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
			}
			else {
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
				rectScrollRange.Width += rectImageScreen.Width + rectClient.Width;
				rectScrollRange.Height += rectImageScreen.Height + rectClient.Height;
				int nScrollMarginX = Math.Min(m_settings.nScrollMargin, rectImageScreen.Width);
				int nScrollMarginY = Math.Min(m_settings.nScrollMargin, rectImageScreen.Height);
				misc.DeflateRect(ref rectScrollRange, nScrollMarginX, nScrollMarginY);
			}
			else {
				rectScrollRange |= rectImageScreen;
			}
			return new sScrollGeometry(rectClient, rectImageScreen, rectScrollRange);
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
					sbHorz.Maximum = Math.Max(0, rectScrollRange.Width/* - rectClient.Width*/);
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
					sbVert.Maximum = Math.Max(0, rectScrollRange.Height/* - rectClient.Height*/);
					sbVert.LargeChange = rectClient.Height;
					sbVert.Value = p;
				}
			}

			double dScale = double.TryParse(ui_edtZoom.Text.TrimEnd('%'), out dScale) ? dScale : 100.0;
			if (m_ctScreenFromImage.Scale != dScale*0.01) {
				//m_bSkipSpinZoomEvent = true;
				ui_edtZoom.Text = $"{m_ctScreenFromImage.Scale*100.0:#,###.##} %";
				//m_bSkipSpinZoomEvent = false;
			}

			return true;
		}

		void UpdateView() {
			ui_picture.Invalidate();
		}

		static CV.Scalar[] GetValue<T>(IntPtr ptr, int depth, int channel, int col0, int col1) where T : unmanaged {
			CV.Scalar [] v = new CV.Scalar[col1-col0];
			int el0 = Marshal.SizeOf<T>();
			int el = channel*el0;
			IntPtr offset = ptr + col0 * el;
			for (int x = col0; x < col1; x++) {
				for (int i = 0; i < channel; ++i, offset += el0) {
					T value = Marshal.PtrToStructure<T>(offset);
					v[x-col0][i] = Convert.ToDouble(value);
				}
			}
			return v;
		}

		public static CV.Scalar[]? GetMatValue(IntPtr ptr, int depth, int channel, int col0, int col1) {
			switch (depth) {
			case CV.MatType.CV_8U:
				return GetValue<Byte>(ptr, depth, channel, col0, col1);
			case CV.MatType.CV_8S:
				return GetValue<SByte>(ptr, depth, channel, col0, col1);
			case CV.MatType.CV_16U:
				return GetValue<UInt16>(ptr, depth, channel, col0, col1);
			case CV.MatType.CV_16S:
				return GetValue<Int16>(ptr, depth, channel, col0, col1);
			case CV.MatType.CV_32S:
				return GetValue<Int32>(ptr, depth, channel, col0, col1);
			case CV.MatType.CV_32F:
				return GetValue<float>(ptr, depth, channel, col0, col1);
			case CV.MatType.CV_64F:
				return GetValue<double>(ptr, depth, channel, col0, col1);
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

			if (ctCanvasFromImage.Scale < ((nChannel+1.0)*minTextHeight))
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
						string str = $"{v[ch],3}";
						canvas.PutText(str, new CV.Point(pt.X, pt.Y+(ch+1)*heightFont*40), CV.HersheyFonts.HersheyDuplex, heightFont, crText, 1, CV.LineTypes.Link8, false);
					}
				}
			}
			//auto t1 = stdc::steady_clock::now();
			//auto dur = stdc::duration_cast<stdc::milliseconds>(t1-t0).count();
			//OutputDebugString(std::format(L"{} ms\n", dur).c_str());

			return true;
		}

		//-------------------------------------------------------------------------------------------------------------------------
		// Operations
		public bool SetZoomMode(eZOOM eZoomMode, bool bCenter = true) {
			if ((int)eZoomMode < 0 || (int)eZoomMode >= ui_cmbZoomMode.Items.Count)
				return false;

			if (ui_cmbZoomMode.SelectedIndex != (int)eZoomMode) {
				ui_cmbZoomMode.SelectedIndex = (int)eZoomMode;
				//// will be re-entered
				//return true;
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

		public bool LoadSettings() {
			if (RegKey == "" || Reg is null)
				return false;
			try {
				var reg = Reg.CreateSubKey(RegKey);
				if (reg.GetValue("Settings", "") is string json) {
					if (json != "")
						m_settings = JsonSerializer.Deserialize<sSettings>(json);
				}
				else {
					return false;
				}

			}
			catch (Exception e) {
				Debug.WriteLine(e.Message);
				return false;
			}
			return true;
		}
		public bool SaveSettings() {
			if (RegKey == "" || Reg is null)
				return false;
			try {
				string json = JsonSerializer.Serialize<sSettings>(m_settings);
				var reg = Reg.CreateSubKey(RegKey);
				reg.SetValue("Settings", json);
			}
			catch (Exception e) {
				Debug.WriteLine(e.Message);
				return false;
			}
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
			if (ui_cmbZoomMode.Items.Count == 0)
				return;
			SetZoomMode((eZOOM)ui_cmbZoomMode.SelectedIndex, false);
		}

		private void ui_picture_Paint(object sender, PaintEventArgs evt) {
			if (m_img is null || m_img.Empty())
				return;

			// Client Rect
			var rectClient = ui_picture.DisplayRectangle;
			if (rectClient.IsEmpty)
				return;

			var g = evt.Graphics;
			var brushBkgnd = new SolidBrush(Color.FromArgb(255, m_settings.crBackground[0], m_settings.crBackground[1], m_settings.crBackground[2]));
			g.FillRectangle(brushBkgnd, rectClient);

			Debug.WriteLine($"rectClient:{rectClient.Width}, {rectClient.Height}, clip: {g.ClipBounds.Width}, {g.ClipBounds.Height}");
			Debug.WriteLine($"control Width: {ui_picture.Width}, Height: {ui_picture.Height}");

			//==================
			// Get Image - ROI
			var ct = m_ctScreenFromImage;

			// Position
			CV.Rect rectImage = misc.Floor(misc.MakeRectFromPts(
				ct.TransI(new CV.Point2d(rectClient.X, rectClient.Y)), ct.TransI(new CV.Point2d(rectClient.Right, rectClient.Bottom))));
			rectImage = misc.NormalizeRect(rectImage);
			rectImage.Width += 2;
			rectImage.Height += 2;
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
			//img = m_settings.crBackground;
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
							imgPyr = imgThumbnail;
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
				PixelFormat pixelFormat = img.Type() == CV.MatType.CV_8UC1 ? PixelFormat.Format8bppIndexed :
					img.Type() == CV.MatType.CV_8UC3 ? PixelFormat.Format24bppRgb:
					img.Type() == CV.MatType.CV_8UC4 ? PixelFormat.Format32bppArgb : PixelFormat.Undefined;
				if (pixelFormat == PixelFormat.Undefined)
					return;
				Bitmap bmp = new Bitmap(rcTarget.Width, img.Rows, (int)img.Step(), pixelFormat, img.Data);
				g.DrawImageUnscaled(bmp, rectTarget.Left, rectTarget.Top);

				// Draw Selection Rect
				if (m_mouse.bInSelectionMode || m_mouse.bRectSelected) {
					CV.Rect rectSelected = misc.Floor(misc.MakeRectFromPts(m_ctScreenFromImage.Trans(m_mouse.ptSel0), m_ctScreenFromImage.Trans(m_mouse.ptSel1)));
					rectSelected = misc.NormalizeRect(rectSelected);
					rectSelected = rectSelected.Intersect(Rect.FromLTRB(rectClient.Left, rectClient.Top, rectClient.Right, rectClient.Bottom));
					if (!misc.IsRectEmpty(rectSelected)) {
						Pen penOuter = new Pen(Color.Gray, 5);
						Pen pen = new Pen(Color.Red, 1);
						g.DrawRectangle(penOuter, rectSelected.Left, rectSelected.Top, rectSelected.Width, rectSelected.Height);
						g.DrawRectangle(pen, rectSelected.Left, rectSelected.Top, rectSelected.Width, rectSelected.Height);
					}
				}

			}

		}

		private void ui_picture_Resize(object sender, EventArgs e) {
			UpdateCT();
			UpdateScrollBars();
			//UpdateView();
			ui_picture.Invalidate();
		}

		private void xMatView_Resize(object sender, EventArgs e) {
			ui_picture.Width = this.Width - ui_picture.Left - ui_scrollbarVert.Width;
			ui_picture.Height = this.Height - ui_picture.Top - ui_scrollbarHorz.Height;
		}

		private void ui_scrollbarHorz_Scroll(object sender, ScrollEventArgs e) {
			if (m_img is null || m_img.Empty())
				return;
			var s = GetScrollGeometry();
			var rectClient = s.rectClient;
			var rectImageScreen = s.rectImageScreen;
			var rectScrollRange = s.rectScrollRange;

			int range = rectScrollRange.Width;
			int page =  rectClient.Width;
			var pos = misc.Clamp(e.NewValue, 0, range);
			m_ctScreenFromImage.m_offset.X = -pos;
			if (m_settings.bExtendedPanning)
				m_ctScreenFromImage.m_offset.X += rectScrollRange.Width - Math.Max(0, rectImageScreen.Width - m_settings.nScrollMargin) - rectClient.Width;
			if (m_eZoom == eZOOM.fit2window)
				SetZoomMode(eZOOM.free, false);
			UpdateScrollBars();
			UpdateView();
		}

		private void ui_scrollbarVert_Scroll(object sender, ScrollEventArgs e) {
			if (m_img is null || m_img.Empty())
				return;
			var s = GetScrollGeometry();
			var rectClient = s.rectClient;
			var rectImageScreen = s.rectImageScreen;
			var rectScrollRange = s.rectScrollRange;

			int range = rectScrollRange.Height;
			int page =  rectClient.Height;
			var pos = misc.Clamp(e.NewValue, 0, range);
			m_ctScreenFromImage.m_offset.Y = -pos;
			if (m_settings.bExtendedPanning)
				m_ctScreenFromImage.m_offset.Y += rectScrollRange.Height - Math.Max(0, rectImageScreen.Height - m_settings.nScrollMargin) - rectClient.Height;
			if (m_eZoom == eZOOM.fit2window)
				SetZoomMode(eZOOM.free, false);
			UpdateScrollBars();
			UpdateView();
		}

		private void ui_picture_MouseDown(object sender, MouseEventArgs e) {
			if (m_img is null || m_img.Empty())
				return;
			CV.Point ptView = new CV.Point(e.Location.X, e.Location.Y);
			if (e.Button == MouseButtons.Left) {
				if (m_settings.bPanningLock && (m_eZoom == eZOOM.fit2window))
					return;
				//if (ui_picture.Capture)
				//	return;
				//ui_picture.Capture = true;
				m_mouse.ptAnchor = ptView;
				m_mouse.ptOffset0 = m_ctScreenFromImage.m_offset;
			}
			else if (e.Button == MouseButtons.Right) {
				if (m_mouse.bInSelectionMode) {
					m_mouse.bInSelectionMode = false;
					m_mouse.bRectSelected = true;
					var pt = m_ctScreenFromImage.TransI(ptView);
					m_mouse.ptSel1.X = misc.Clamp<int>((int)pt.X, 0, m_img.Cols);
					m_mouse.ptSel1.Y = misc.Clamp<int>((int)pt.Y, 0, m_img.Rows);
				}
				else {
					m_mouse.bRectSelected = false;
					m_mouse.bInSelectionMode = true;
					var pt = m_ctScreenFromImage.TransI(ptView);
					m_mouse.ptSel0.X = misc.Clamp<int>((int)pt.X, 0, m_img.Cols);
					m_mouse.ptSel0.Y = misc.Clamp<int>((int)pt.Y, 0, m_img.Rows);
					m_mouse.ptSel1 = m_mouse.ptSel0;
				}
				UpdateView();
			}
		}

		private void ui_picture_MouseUp(object sender, MouseEventArgs e) {
			if (m_img is null || m_img.Empty())
				return;
			if (e.Button == MouseButtons.Left) {
				//if (!ui_picture.Capture) {
				//	return;
				//}
				//ui_picture.Capture = false;
				m_mouse.ptAnchor = null;
			}
			else if (e.Button == MouseButtons.Right) {
			}
		}

		private void ui_picture_MouseMove(object sender, MouseEventArgs e) {
			if (m_img is null || m_img.Empty())
				return;
			CV.Point2d ptView = new CV.Point2d(e.Location.X, e.Location.Y);
			if (m_mouse.ptAnchor is not null) {
				CV.Point ptAnchor = m_mouse.ptAnchor ?? new CV.Point(0, 0);
				if (!m_settings.bPanningLock) {
					switch (m_eZoom) {
					case eZOOM.one2one:
						break;
					case eZOOM.mouse_wheel_locked:
						break;
					case eZOOM.free:
						break;
					default:
						m_eZoom = eZOOM.free;
						ui_cmbZoomMode.SelectedIndex = (int)m_eZoom;
						break;
					}
				}
				double dPanningSpeed = m_mouse.bInSelectionMode ? 1.0 : m_settings.dPanningSpeed;
				CV.Point2d ptOffset = (ptView - ptAnchor) * dPanningSpeed;
				if (m_eZoom == eZOOM.fit2width)
					ptOffset.X = 0;
				if (m_eZoom == eZOOM.fit2height)
					ptOffset.Y = 0;
				m_ctScreenFromImage.m_offset = m_mouse.ptOffset0 + ptOffset;
				UpdateCT();
				UpdateScrollBars();
				UpdateView();
			}

			// Selection Mode
			if (m_mouse.bInSelectionMode) {
				var pt = m_ctScreenFromImage.TransI(ptView);
				m_mouse.ptSel1.X = misc.Clamp<int>((int)pt.X, 0, m_img.Cols);
				m_mouse.ptSel1.Y = misc.Clamp<int>((int)pt.Y, 0, m_img.Rows);
				UpdateView();
			}

			// status
			UpdateInfo(ptView);

		}

		private void UpdateInfo(Point2d? ptView) {
			if (m_img is null || m_img.Empty())
				return;	

			StringBuilder status = new();

			if (!m_img.Empty())
				status.Append($"(w{m_img.Cols:#,###} h{m_img.Rows:#,###}) ");

			// print ptImage.x and ptImage.y with thousand comma separated
			if (ptView is Point2d pt) {
				var ptImage = misc.Floor(m_ctScreenFromImage.TransI(pt));
				status.Append($"[x{ptImage.X:#,###} y{ptImage.Y:#,###}]");

				// image value
				int nChannel = m_img.Channels();
				if (ptImage.X >= 0 && ptImage.X < m_img.Cols && ptImage.Y >= 0 && ptImage.Y < m_img.Rows) {
					int depth = m_img.Depth();
					CV.Scalar []? cr = GetMatValue(m_img.Ptr(ptImage.Y), depth, nChannel, ptImage.X, ptImage.X+1);
					if (cr is not null) {
						status.Append($" [{cr[0][0]}");
						for (int i = 1; i < nChannel; i++)
							status.Append($",{cr[0][i]}");
						status.Append("]");
					}
				}
			}

			// Selection
			if (m_mouse.bInSelectionMode || m_mouse.bRectSelected) {
				CV.Point2d size = m_mouse.ptSel1 - m_mouse.ptSel0;
				status.Append($" (x{m_mouse.ptSel0.X:#,###} y{m_mouse.ptSel0.Y:#,###} w{Math.Abs(size.X)} h{Math.Abs(size.Y)})");
			}

			string str = status.ToString();
			if (str != ui_txtInfo.Text) {
				ui_txtInfo.Text = str;
				ui_txtInfo.SelectionStart = 0;
				ui_txtInfo.SelectionLength = 0;
			}
		}

		private void ui_picture_MouseLeave(object sender, EventArgs e) {
			UpdateInfo(null);
		}

		private void ui_picture_MouseWheel(object sender, MouseEventArgs e) {
			if (m_img is null || m_img.Empty())
				return;
			if ((m_eZoom == eZOOM.mouse_wheel_locked) || (m_settings.bZoomLock && m_eZoom != eZOOM.free)) {
				return;
			}
			ZoomInOut(e.Delta, new CV.Point(e.Location.X, e.Location.Y), false);
		}

		private void ui_btnZoomIn_Click(object sender, EventArgs e) {
			ZoomInOut(100, misc.CenterPoint(GetViewRect()), false);
		}

		private void ui_btnZoomOut_Click(object sender, EventArgs e) {
			ZoomInOut(-100, misc.CenterPoint(GetViewRect()), false);
		}

		private void ui_btnZoomFit_Click(object sender, EventArgs e) {
			SetZoomMode(eZOOM.fit2window, true);
		}

		private void ui_btnSettings_Click(object sender, EventArgs e) {
			MatView_SettingsDlg dlg = new(m_settings);
			if (dlg.ShowDialog() == DialogResult.OK) {
				m_settings = dlg.m_settings;
				SaveSettings();

				UpdateCT(false, eZOOM.none);
				UpdateScrollBars();
				UpdateView();
			}
		}
	}

}
