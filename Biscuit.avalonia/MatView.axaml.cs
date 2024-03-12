using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Remote.Protocol.Input;
using Avalonia.Rendering;
using Avalonia.Threading;
using ReactiveUI;
using CV = OpenCvSharp;

namespace Biscuit.avalonia
{

	public partial class xMatView : UserControl
	{
		public CV.Mat Image
		{
			get => m_img;
			set
			{
				m_img = value;
				Init();
			}
		}

		public eZOOM ZoomMode
		{
			get => m_option.eZoom;
			set => InitZoom(value);
		}

		public struct sScrollGeometry
		{
			public CV.Rect2d rectClient, rectImageScreen, rectScrollRange;
		};

		private CV.Mat m_img;

		public enum eZOOM : int { none = -1, one2one, fit2window, fit2width, fit2height, mouse_wheel_locked, free }
		public enum eZOOM_IN : int { nearest, linear, bicubic, lanczos4/* EDSR, ESPCN, FSRCNN, LapSRN */}
		public enum eZOOM_OUT : int { nearest, area, }

		public struct sOption
		{
			public eZOOM eZoom = eZOOM.fit2window;          // zoom mode
			public bool bZoomLock = false;                  // if eZoom is NOT free, zoom is locked
			public bool bPanningLock = true;                // image can be moved only eZoom == free
			public bool bExtendedPanning = true;            // image can move out of screen
			public bool bKeyboardNavigation = false;        // page up/down, scroll down to next blank of image
			public bool bDrawPixelValue = true;             // draw pixel value on image
			public bool bPyrImageDown = true;               // build cache image for down sampling. (such as mipmap)
			public double dPanningSpeed = 2.0;              // Image Panning Speed. 1.0 is same with mouse move.
			public double nScrollMargin = 5;                // bExtendedPanning, px margin to scroll
			public int tsScroll = 250;                      // Smooth Scroll. duration
			public eZOOM_IN eZoomIn = eZOOM_IN.nearest;
			public eZOOM_OUT eZoomOut = eZOOM_OUT.area;
			public Vector3 crBackground = new Vector3(0, 0, 0); // rgb	//{161, 114, 230}

			public sOption()
			{
			}
		};

		sOption m_option = new sOption();

		protected Timer m_timerDraw;

		private CV.Rect2d _rectSource = new CV.Rect2d(0, 0, 800, 600);
		private CV.Rect2d _rectDest = new CV.Rect2d(0, 0, 800, 600);

		static readonly double[] dZoomLevels = new double[] {
		1.0/8192.0, 1.0/4096, 1.0/2048, 1.0/1024,
		1.0/512, 1.0/256, 1.0/128, 1.0/64, 1.0/32, 1.0/16, 1.0/8, 1.0/4, 1.0/2,
		3.0/4, 1, 1.25, 1.5, 1.75, 2, 2.5, 3, 4, 5, 6, 7, 8, 9, 10,
		12.5, 15, 17.5, 20, 25, 30, 35, 40, 45, 50, 60, 70, 80, 90, 100,
		125, 150, 175, 200, 250, 300, 350, 400, 450, 500,
		600, 700, 800, 900, 1_000,
	};

		public xMatView()
		{
			InitializeComponent();

			//ui_renderer.PointerPressed += OnMousePressed;
			m_timerDraw = new Timer(OnTimerDraw);
		}

		public void Init()
		{
			eZOOM eZoom = m_option.eZoom;
			if (eZoom == eZOOM.none)
				eZoom = eZOOM.fit2window;
			InitZoom(eZoom);
		}

		protected override void OnSizeChanged(SizeChangedEventArgs e)
		{
			base.OnSizeChanged(e);

			UpdateCT();
		}

		protected void ReserveDraw()
		{
			m_timerDraw.Change(1000 / 60, 1000 / 60);
			//if (m_img == null || m_img.Empty())
			//	return;
		}

		protected void OnTimerDraw(object? state)
		{
			m_timerDraw.Change(Timeout.Infinite, Timeout.Infinite);
			Dispatcher.UIThread.InvokeAsync(() =>
			{
				if (ui_renderer != null)
				{
					ui_renderer.InvalidateVisual();
				}
			});
		}

		protected void InitZoom(eZOOM eZoom)
		{
			m_option.eZoom = eZoom;

			UpdateCT();
		}

		public struct sMouseOperation
		{
			public bool bInSelectionMode = false;
			public bool bRectSelected = false;
			public CV.Point2d? ptAnchor;        // Anchor Point in Screen Coordinate
			public CV.Point2d ptOffset0;        // offset backup
			public CV.Point2d ptSel0, ptSel1;       // Selection Point in Image Coordinate

			public sMouseOperation()
			{
			}

			public void Clear()
			{
				bInSelectionMode = false;
				bRectSelected = false;
				ptAnchor = null;
				ptOffset0 = new CV.Point2d();
				ptSel0 = new CV.Point2d();
				ptSel1 = new CV.Point2d();
			}
		}
		protected sMouseOperation m_mouse;

		xCoordTransScaleShift m_ctScreenFromImage = new xCoordTransScaleShift();

		public bool SetZoomMode(eZOOM eZoomMode, bool bCenter = true)
		{
			return true;
		}

		public CV.Rect2d GetSelectionRect()
		{
			CV.Rect2d rect = new();
			if (!m_mouse.bRectSelected)
				return rect;
			rect.X = Math.Floor(m_mouse.ptSel0.X);
			rect.Y = Math.Floor(m_mouse.ptSel0.Y);
			rect.Width = Math.Floor(m_mouse.ptSel1.X - m_mouse.ptSel0.X);
			rect.Height = Math.Floor(m_mouse.ptSel1.Y - m_mouse.ptSel0.Y);

			return rect;
		}

		public CV.Point2d[]? GetSelectionPoints()
		{
			if (!m_mouse.bRectSelected)
				return null;
			return new CV.Point2d[] { m_mouse.ptSel0, m_mouse.ptSel1 };
		}

		public void SetSelectionRect(CV.Rect2d rect)
		{
			m_mouse.bRectSelected = true;
			m_mouse.ptSel0 = new CV.Point2d(rect.X, rect.Y);
			m_mouse.ptSel1 = new CV.Point2d(rect.X + rect.Width, rect.Y + rect.Height);
			// todo: update selection rect

		}

		public void ClearSelectionRect()
		{
			m_mouse.bRectSelected = false;
			// todo: update selection rect
		}

		string m_strCookie = "";

		public Func<bool, string, sOption, bool> m_fnSyncOption;
		public bool LoadOption()
		{
			return m_fnSyncOption != null && m_fnSyncOption(false, m_strCookie, m_option) && SetOption(m_option, false);
		}

		bool SaveOption()
		{
			return m_fnSyncOption != null && m_fnSyncOption(true, m_strCookie, m_option);
		}
		public sOption GetOption()
		{
			return m_option;
		}
		public bool SetOption(sOption option, bool bStore = true)
		{
			if (!m_option.Equals(option))
				m_option = option;

			UpdateCT(false, eZOOM.none);
			UpdateScrollBars();
			// todo: update view
			//if (ui->view)
			//	ui->view->update();

			if (bStore)
			{
				return SaveOption();
			}
			return true;

		}

		public bool ShowToolBar(bool bShow)
		{
			return true;
		}

		public bool IsToolBarShown()
		{
			return true;
		}

		//virtual void OnClose(wxCloseEvent& event) override;

		public CV.Rect2d GetViewRect()
		{
			CV.Rect2d rect = new CV.Rect2d();
			if (ui_renderer != null)
			{
				rect.Width = ui_renderer.Bounds.Right - ui_renderer.Bounds.Left;
				rect.Height = ui_renderer.Bounds.Bottom - ui_renderer.Bounds.Top;
			}
			return rect;
		}

		// ClientRect, ImageRect, ScrollRange
		public sScrollGeometry GetScrollGeometry()
		{
			CV.Rect2d rectClient = GetViewRect();
			CV.Point2d pt0 = m_ctScreenFromImage.Trans(new CV.Point2d(0, 0));
			CV.Point2d pt1 = m_img switch { null => pt0, _ => m_ctScreenFromImage.Trans(new CV.Point2d(m_img.Cols, m_img.Rows)) };
			CV.Rect2d rectImageScreen = new(pt0.X, pt0.Y, pt1.X - pt0.X, pt1.Y - pt0.Y);
			rectImageScreen = misc.NormalizeRect(rectImageScreen);

			CV.Rect2d rectScrollRange = rectClient;
			if (m_option.bExtendedPanning)
			{
				rectScrollRange.X -= rectImageScreen.Width;
				rectScrollRange.Y -= rectImageScreen.Height;
				rectScrollRange.Width += rectClient.Width;
				rectScrollRange.Height += rectClient.Height;

				double nScrollMarginX = Math.Min(m_option.nScrollMargin, rectImageScreen.Width);
				double nScrollMarginY = Math.Min(m_option.nScrollMargin, rectImageScreen.Height);
				misc.DeflateRect(ref rectScrollRange, nScrollMarginX, nScrollMarginY);
			}
			else
			{
				rectScrollRange |= rectImageScreen;
			}
			sScrollGeometry g = new();
			g.rectClient = rectClient;
			g.rectImageScreen = rectImageScreen;
			g.rectScrollRange = rectScrollRange;
			return g;
		}

		public bool UpdateCT(bool bCenter = false, eZOOM eZoom = eZOOM.none)
		{
			if (m_img is null || m_img.Empty())
				return false;

			if (eZoom == eZOOM.none)
				eZoom = m_option.eZoom;

			ui_cmbZoomMode.SelectedIndex = (int)eZoom;

			CV.Rect2d rectClient = GetViewRect();
			CV.Size2d sizeClient = rectClient.Size;

			// scale
			int cols = m_img == null ? 1 : m_img.Cols;
			int rows = m_img == null ? 1 : m_img.Rows;
			double dScale = eZoom switch
			{
				eZOOM.one2one => 1.0,
				eZOOM.fit2window => Math.Min(sizeClient.Width / cols, sizeClient.Height / rows),
				eZOOM.fit2width => sizeClient.Width / cols,
				eZOOM.fit2height => sizeClient.Height / rows,
				_ => (double)ui_spinZoom.Value,
			};
			if (dScale > 0)
				m_ctScreenFromImage.m_scale = dScale;

			// constraints. make image put on the center of the screen
			if (bCenter || (eZoom == eZOOM.fit2window) || (eZoom == eZOOM.fit2width) || (eZoom == eZOOM.fit2height))
			{
				var ct2 = m_ctScreenFromImage;
				ct2.m_origin.X = cols / 2;
				ct2.m_origin.Y = rows / 2;
				ct2.m_offset.X = rectClient.X + rectClient.Width / 2;
				ct2.m_offset.Y = rectClient.Y + rectClient.Height / 2;

				CV.Point2d ptOrigin = new CV.Point2d(0.0, 0.0);
				CV.Point2d ptLT = ct2.Trans(ptOrigin);

				if (bCenter || eZoom == eZOOM.fit2window)
				{
					m_ctScreenFromImage.m_origin = new CV.Point2d(0.0, 0.0);
					m_ctScreenFromImage.m_offset = ptLT;
				}
				else if (eZoom == eZOOM.fit2width)
				{
					m_ctScreenFromImage.m_origin.X = 0;
					m_ctScreenFromImage.m_offset.X = ptLT.X;
				}
				else if (eZoom == eZOOM.fit2height)
				{
					m_ctScreenFromImage.m_origin.Y = 0;
					m_ctScreenFromImage.m_offset.Y = ptLT.Y;
				}
			}

			// panning constraints.
			CV.Point2d pt0 = m_ctScreenFromImage.Trans(new CV.Point2d(0.0, 0.0));
			CV.Point2d pt1 = m_ctScreenFromImage.Trans(new CV.Point2d(cols, rows));
			CV.Rect2d rectImageScreen = new CV.Rect2d(pt0.X, pt0.Y, pt1.X - pt0.X, pt1.Y - pt0.Y);
			misc.NormalizeRect(rectImageScreen);
			if (m_option.bExtendedPanning)
			{
				double marginX = Math.Min(m_option.nScrollMargin, rectImageScreen.Width);
				double marginY = Math.Min(m_option.nScrollMargin, rectImageScreen.Height);
				// make any part of image stays inside the screen
				if (rectImageScreen.Right < rectClient.Left)
				{
					m_ctScreenFromImage.m_offset.X += rectClient.Left - rectImageScreen.Right + marginX;
				}
				else if (rectImageScreen.Left > rectClient.Right)
				{
					m_ctScreenFromImage.m_offset.X += rectClient.Right - rectImageScreen.Left - marginX;
				}
				if (rectImageScreen.Bottom < rectClient.Top)
				{
					m_ctScreenFromImage.m_offset.Y += rectClient.Top - rectImageScreen.Bottom + marginY;
				}
				else if (rectImageScreen.Top > rectClient.Bottom)
				{
					m_ctScreenFromImage.m_offset.Y += rectClient.Bottom - rectImageScreen.Top - marginY;
				}
			}
			else
			{
				// default panning. make image stays inside the screen
				if (rectImageScreen.Width <= rectClient.Width)
				{
					var pt = m_ctScreenFromImage.Trans(new CV.Point2d(cols / 2, 0));
					m_ctScreenFromImage.m_offset.X += rectClient.X + rectClient.Width / 2 - pt.X;
				}
				if (rectImageScreen.Width > rectClient.Width)
				{
					if (rectImageScreen.Left > rectClient.Left)
						m_ctScreenFromImage.m_offset.X += rectClient.Left - rectImageScreen.Left;
					else if (rectImageScreen.Right < rectClient.Right)
						m_ctScreenFromImage.m_offset.X += rectClient.Right - rectImageScreen.Right;
				}
				if (rectImageScreen.Height <= rectClient.Height)
				{
					var pt = m_ctScreenFromImage.Trans(new CV.Point2d(0, rows / 2));
					m_ctScreenFromImage.m_offset.Y += rectClient.Height / 2 - pt.Y;
				}
				if (rectImageScreen.Height > rectClient.Height)
				{
					if (rectImageScreen.Top > rectClient.Top)
						m_ctScreenFromImage.m_offset.Y += rectClient.Top - rectImageScreen.Top;
					else if (rectImageScreen.Bottom < rectClient.Bottom)
						m_ctScreenFromImage.m_offset.Y += rectClient.Bottom - rectImageScreen.Bottom;
				}
			}

			//pt0 = m_ctScreenFromImage.Trans(new CV.Point2d());
			//pt1 = m_ctScreenFromImage.Trans(new CV.Point2d(cols, rows));
			if (pt0.X < 0)
				pt0.X = 0;
			if (pt0.Y < 0)
				pt0.Y = 0;
			if (pt1.X > ui_renderer.Width)
				pt1.X = ui_renderer.Width;
			if (pt1.Y > ui_renderer.Height)
				pt1.Y = ui_renderer.Height;

			var ptSource0 = m_ctScreenFromImage.TransI(pt0);
			var ptSource1 = m_ctScreenFromImage.TransI(pt1);
			var roi = misc.GetSafeROI(new CV.Rect2d(ptSource0.X, ptSource0.Y, ptSource1.X - ptSource0.X, ptSource1.Y - ptSource0.Y), new CV.Size(cols, rows));
			ui_renderer.m_rectTarget = new CV.Rect2d(pt0.X, pt0.Y, pt1.X - pt0.X, pt1.Y - pt0.Y);
			ui_renderer.Image = misc.IsRectEmpty(roi) ? null : Image.SubMat(roi);

			ReserveDraw();

			return true;
		}

		bool UpdateScrollBars()
		{
			if (m_img == null || m_img.Empty())
				return false;

			var g = GetScrollGeometry();
			var rectClient = g.rectClient;
			var rectImageScreen = g.rectImageScreen;
			var rectScrollRange = g.rectScrollRange;

			var sbH = ui_scrollbarH;
			if (sbH.Visibility == ScrollBarVisibility.Visible)
			{
				double p = m_option.bExtendedPanning
					? rectScrollRange.Width - (m_ctScreenFromImage.m_offset.X + Math.Max(0.0, rectImageScreen.Width - m_option.nScrollMargin) + rectClient.Width)
					: -m_ctScreenFromImage.m_offset.X;
				if (rectScrollRange.Width > 0)
				{
					p = misc.Clamp(p, 0, rectScrollRange.Width);
					sbH.Minimum = 0;
					sbH.Maximum = Math.Max(0, rectScrollRange.Width - rectClient.Width);
					sbH.LargeChange = rectClient.Width;
					sbH.Value = p;
				}
			}

			var sbV = ui_scrollbarV;
			if (sbV.Visibility == ScrollBarVisibility.Visible)
			{
				double p = m_option.bExtendedPanning
					? rectScrollRange.Height - (m_ctScreenFromImage.m_offset.Y + Math.Max(0, rectImageScreen.Height - m_option.nScrollMargin) + rectClient.Height)
					: -m_ctScreenFromImage.m_offset.Y;
				if (rectScrollRange.Height > 0)
				{
					p = misc.Clamp(p, 0, rectScrollRange.Height);
					sbV.Minimum = 0;
					sbV.Maximum = Math.Max(0, rectScrollRange.Height - rectClient.Height);
					sbV.LargeChange = rectClient.Height;
					sbV.Value = p;
				}
			}

			if (m_ctScreenFromImage.m_scale != (double)ui_spinZoom.Value)
			{
				ui_spinZoom.Value = (decimal)m_ctScreenFromImage.m_scale;
			}

			return true;
		}

		bool ZoomInOut(double step, CV.Point2d ptAnchor, bool bCenter)
		{
			var scale = m_ctScreenFromImage.m_scale;
			if (step > 0)
			{
				foreach (var dZoom in dZoomLevels)
				{
					if (dZoom > scale)
					{
						scale = dZoom;
						break;
					}
				}
			}
			else
			{
				foreach (var dZoom in dZoomLevels.Reverse())
				{
					if (dZoom < scale)
					{
						scale = dZoom;
						break;
					}
				}
			}
			return SetZoom(scale, ptAnchor, bCenter);
		}

		bool SetZoom(double scale, CV.Point2d ptAnchor, bool bCenter)
		{
			if (m_img == null || m_img.Empty())
				return false;
			// Backup Image Position
			CV.Point2d ptImage = m_ctScreenFromImage.TransI(ptAnchor);
			// Get Scroll Amount
			if (m_ctScreenFromImage.m_scale == scale)
				return true;
			else
				m_ctScreenFromImage.m_scale = scale;

			var rectClient = GetViewRect();
			var dMinZoom = Math.Min(rectClient.Width / 4.0 / m_img.Cols, rectClient.Height / 4.0 / m_img.Rows);
			dMinZoom = Math.Min(dMinZoom, 0.5);
			m_ctScreenFromImage.m_scale = misc.Clamp(m_ctScreenFromImage.m_scale, dMinZoom, 1_000.0);
			m_ctScreenFromImage.m_offset += ptAnchor - m_ctScreenFromImage.Trans(ptImage);
			// Anchor point
			var eZoom = m_option.eZoom;// m_eZoom;
			if (eZoom != eZOOM.mouse_wheel_locked)
				eZoom = eZOOM.free;
			ui_cmbZoomMode.SelectedIndex = (int)eZoom;
			//OnCmbZoomMode_currentIndexChanged(std::to_underlying(eZoom));
			UpdateCT(bCenter);
			UpdateScrollBars();

			ReserveDraw();

			return true;
		}

		protected void OnZoomMode(object sender, SelectionChangedEventArgs e)
		{
			if (ui_cmbZoomMode == null)
				return;

			if (ui_cmbZoomMode.SelectedIndex < 0)
				return;
			var eZoom = (eZOOM)ui_cmbZoomMode.SelectedIndex;
			InitZoom(eZoom);
		}

		protected void OnZoomValueChanged(object sender, NumericUpDownValueChangedEventArgs e)
		{
			if (ui_spinZoom == null)
				return;

			if (ui_spinZoom.Value <= 0)
				return;

			if (m_option.eZoom != eZOOM.free)
				m_option.eZoom = eZOOM.free;
			UpdateCT();
		}

		protected void OnMousePressed(object sender, PointerPressedEventArgs e)
		{
			if (m_img == null || m_img.Empty())
				return;

			var Point = e.GetCurrentPoint(sender as Control);
			var ptView = new CV.Point2d(Point.Position.X, Point.Position.Y);
			if (Point.Properties.IsLeftButtonPressed)
			{
				if (m_option.bPanningLock && (m_option.eZoom == eZOOM.fit2window))
					return;
				//if (e.Pointer.Captured != null)
				//	return;
				//e.Pointer.Capture(sender as Control);
				m_mouse.ptAnchor = ptView;
				m_mouse.ptOffset0 = m_ctScreenFromImage.m_offset;
			}
			else if (Point.Properties.IsRightButtonPressed)
			{
				if (m_mouse.bInSelectionMode)
				{
					m_mouse.bInSelectionMode = false;
					m_mouse.bRectSelected = true;
					var pt = m_ctScreenFromImage.TransI(ptView);
					m_mouse.ptSel1.X = misc.Clamp(pt.X, 0, m_img.Cols);
					m_mouse.ptSel1.Y = misc.Clamp(pt.Y, 0, m_img.Rows);
				}
				else
				{
					m_mouse.bRectSelected = false;
					m_mouse.bInSelectionMode = true;
					var pt = m_ctScreenFromImage.TransI(ptView);
					m_mouse.ptSel0.X = misc.Clamp(pt.X, 0, m_img.Cols);
					m_mouse.ptSel0.Y = misc.Clamp(pt.Y, 0, m_img.Rows);
					m_mouse.ptSel1 = m_mouse.ptSel0;
				}
			}

			UpdateCT();
		}

		protected void OnMouseMoved(object sender, PointerEventArgs e)
		{
			if (m_img == null || m_img.Empty())
				return;
			var Point = e.GetCurrentPoint(sender as Control);
			var ptView = new CV.Point2d(Point.Position.X, Point.Position.Y);

			if (m_mouse.ptAnchor is CV.Point2d ptAnchor)
			{
				if (!m_option.bPanningLock)
				{
					switch (m_option.eZoom)
					{
						case eZOOM.one2one: break;
						case eZOOM.mouse_wheel_locked: break;
						case eZOOM.free: break;
						default:
							m_option.eZoom = eZOOM.free;
							ui_cmbZoomMode.SelectedIndex = (int)m_option.eZoom;
							break;
					}
				}
				double dPanningSpeed = m_mouse.bInSelectionMode ? 1.0 : m_option.dPanningSpeed;
				CV.Point2d ptOffset = (ptView - ptAnchor) * dPanningSpeed;
				if (m_option.eZoom == eZOOM.fit2width)
					ptOffset.X = 0;
				if (m_option.eZoom == eZOOM.fit2height)
					ptOffset.Y = 0;
				m_ctScreenFromImage.m_offset = m_mouse.ptOffset0 + ptOffset;
				UpdateScrollBars();
			}

			// Selection Mode
			if (m_mouse.bInSelectionMode)
			{
				var pt = m_ctScreenFromImage.TransI(ptView);
				m_mouse.ptSel1.X = misc.Clamp(pt.X, 0, m_img.Cols);
				m_mouse.ptSel1.Y = misc.Clamp(pt.Y, 0, m_img.Rows);

				var pt0 = m_ctScreenFromImage.Trans(m_mouse.ptSel0);
				var pt1 = m_ctScreenFromImage.Trans(m_mouse.ptSel1);

				var r = misc.NormalizeRect(new CV.Rect2d(pt0.X, pt0.Y, pt1.X - pt0.X, pt1.Y - pt0.Y));
				ui_renderer.SelectedRect = new Rect(r.X, r.Y, r.Width, r.Height);
			}

			// status
			{
				var ptImagef = m_ctScreenFromImage.TransI(ptView);
				CV.Point ptImage = new((int)ptImagef.X, (int)ptImagef.Y);
				StringBuilder sb = new();

				// Current Position
				{
					// print ptImage.x and ptImage.y with thousand comma separated
					if (m_img != null && !m_img.Empty())
						sb.Append($"(w{m_img.Cols,3} h{m_img.Rows,3}) ");
					sb.Append($"[x{ptImage.X,3} y{ptImage.Y,3}]");
				}

				// image value
				if (m_img != null && !m_img.Empty())
				{
					int n = m_img.Channels();
					if (misc.PtInRect(new CV.Rect(0, 0, m_img.Cols, m_img.Rows), ptImage))
					{
						int depth = m_img.Depth();
						var buffer = m_img.Ptr(ptImage.Y);
						CV.Scalar cr = misc.GetMatValue(m_img.Ptr(ptImage.Y), depth, n, ptImage.Y, ptImage.X);
						sb.Append($" [{cr[0],3}");
						for (int i = 1; i < n; i++)
						{
							sb.Append($",{cr[i]}");
						}
						sb.Append("]");
					}
					else
					{
						sb.AppendFormat(" [{0, 3}", ' ');
						for (int i = 1; i < n; i++)
							sb.AppendFormat(" {0, 3}", ' ');
						sb.Append("]");
					}
				}

				// Selection
				if (m_mouse.bInSelectionMode || m_mouse.bRectSelected)
				{
					var size = m_mouse.ptSel1 - m_mouse.ptSel0;
					sb.AppendFormat(" (x{0} y{1} w{2} h{3})",
						(int)m_mouse.ptSel0.X, (int)m_mouse.ptSel0.Y,
						(int)Math.Abs(size.X), (int)Math.Abs(size.Y));
				}

				string str = sb.ToString();
				if (ui_edtInfo.Text != str)
				{
					ui_edtInfo.Text = str;
				}
			}

			UpdateCT();
		}

		protected void OnMouseReleased(object sender, PointerReleasedEventArgs e)
		{
			if (m_img == null || m_img.Empty())
				return;

			var Point = e.GetCurrentPoint(sender as Control);
			var ptView = new CV.Point2d(Point.Position.X, Point.Position.Y);

			switch (e.InitialPressMouseButton)
			{
				case Avalonia.Input.MouseButton.Left:
					//if (e.Pointer.Captured != this)
					//	return;
					//e.Pointer.Capture(null);
					m_mouse.ptAnchor = null;
					break;

				case Avalonia.Input.MouseButton.Right:
					//ui_renderer.SelectedRect = null;
					break;
			}
		}

	}


}	// Biscuit.avalonia
