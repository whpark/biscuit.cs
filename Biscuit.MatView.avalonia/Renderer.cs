using Biscuit;

using System;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Timers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;	// WriteableBitmap
using Avalonia.Platform;
using Microsoft.CodeAnalysis.Text;
using OpenCvSharp;
using static Biscuit.MatView.avalonia.xRenderer;
using CV = OpenCvSharp;
using Tmds.DBus.Protocol;

//using Avalonia.Threading;
//using Avalonia.VisualTree;
//using Avalonia.Utilities;

namespace Biscuit.MatView.avalonia;

public class xRenderer : Control
{
	private Avalonia.Threading.DispatcherTimer _timerDraw;

	public OpenCvSharp.Mat Image {
		get => m_img;
		set {
			m_img = value;
			Init();
		}
	}

	public eZOOM ZoomMode
	{
		get => m_option.eZOOM;
		set => InitZoom(value);
	}

	public struct sScrollGeometry
	{
		CV.Rect2d rectClient, rectImageScreen, rectScrollRange;
	};

	private CV.Mat m_imgOriginal;  // original image
	private CV.Mat m_img;

	public enum eZOOM : int { none = -1, one2one, fit2window, fit2width, fit2height, mouse_wheel_locked, free }
	public enum eZOOM_IN : int { nearest, linear, bicubic, lanczos4/* EDSR, ESPCN, FSRCNN, LapSRN */}
	public enum eZOOM_OUT : int { nearest, area, }

	public struct sOption
	{
		public eZOOM eZOOM = eZOOM.fit2window;          // zoom mode
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

	sOption m_option;

	private CV.Rect2d _rectSource = new CV.Rect2d(0, 0, 800, 600);
	private CV.Rect2d _rectDest = new CV.Rect2d(0, 0, 800, 600);

	public xRenderer()
	{
		_timerDraw = new Avalonia.Threading.DispatcherTimer();
		_timerDraw.Interval = TimeSpan.FromMilliseconds(1000 / 60);
	}

	public void Init()
	{
		eZOOM eZoom = m_option.eZOOM;
		if (eZoom == eZOOM.none)
			eZoom = eZOOM.fit2window;
		InitZoom(eZoom);
	}

	protected void ReserveDraw()
	{
		_timerDraw.Stop();
		_timerDraw.Start();
	}

	protected void InitZoom(eZOOM eZoom)
	{
		ReserveDraw();

		m_option.eZOOM = eZoom;
		if (m_img == null)
			return;

		switch (m_option.eZOOM)
		{
			case eZOOM.one2one:
				_rectSource = new CV.Rect2d(0, 0, m_img.Cols, m_img.Rows);
				_rectDest = new CV.Rect2d(0, 0, m_img.Cols, m_img.Rows);
				break;
			case eZOOM.fit2window:
				_rectSource = new CV.Rect2d(0, 0, m_img.Cols, m_img.Rows);
				break;
		}
	}

	public override void Render(DrawingContext context)
	{
		_timerDraw.Stop();

		if (m_img == null)
		{
			context.DrawRectangle(Brushes.DarkGray, null, Bounds);
			return;
		}

		var ePixelFormat = m_img.Channels() switch
		{
			1 => PixelFormats.Gray8,
			3 => PixelFormats.Bgr24,
			4 => PixelFormats.Bgra8888,
			_ => throw new Exception("Unsupported image format"),
		};
		using (WriteableBitmap bmp = new WriteableBitmap(
			ePixelFormat,
			m_img.Channels() == 4 ? AlphaFormat.Unpremul : AlphaFormat.Opaque,
			m_img.Data,
			new PixelSize(m_img.Cols, m_img.Rows),
			new Avalonia.Vector(96, 96),
			(int)m_img.Step()))
		{
			context.DrawImage(bmp, new Avalonia.Rect(0, 0, m_img.Cols, m_img.Rows), Bounds);
		}
	}

	public struct sMouseOperation {
		public bool bInSelectionMode = false;
		public bool bRectSelected = false;
		public CV.Point2d? ptAnchor;		// Anchor Point in Screen Coordinate
		public CV.Point2d ptOffset0;		// offset backup
		public CV.Point2d ptSel0, ptSel1;		// Selection Point in Image Coordinate

		public sMouseOperation()
		{
		}

		public void Clear()
		{
			bInSelectionMode = false;
			bRectSelected = false ;
			ptAnchor = null;
			ptOffset0 = new CV.Point2d();
			ptSel0 = new CV.Point2d();
			ptSel1 = new CV.Point2d();
		}
	}
	protected sMouseOperation m_mouse;

	xCoordTrans2d m_ctScreenFromImage = new xCoordTrans2d();

	public bool SetZoomMode(eZOOM eZoomMode, bool bCenter = true)
	{
		return true;
	}

	public CV.Rect2d GetSelectionRect() {
		CV.Rect2d rect = new ();
		if (!m_mouse.bRectSelected)
			return rect;
		rect.X = Math.Floor(m_mouse.ptSel0.X);
		rect.Y = Math.Floor(m_mouse.ptSel0.Y);
		rect.Width = Math.Floor(m_mouse.ptSel1.X - m_mouse.ptSel0.X);
		rect.Height = Math.Floor(m_mouse.ptSel1.Y - m_mouse.ptSel0.Y);
		
		return rect;
	}

	public CV.Point2d[]? GetSelectionPoints() {
		if (!m_mouse.bRectSelected)
			return null;
		return new CV.Point2d[] { m_mouse.ptSel0, m_mouse.ptSel1 };
	}

	public void SetSelectionRect(CV.Rect2d rect) {
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
	public bool LoadOption() {
		return m_fnSyncOption != null && m_fnSyncOption(false, m_strCookie, m_option) && SetOption(m_option, false);
	}

	bool SaveOption() {
		return m_fnSyncOption != null && m_fnSyncOption(true, m_strCookie, m_option);
	}
	public sOption GetOption() {
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
		CV.Rect2d rect = new CV.Rect2d(0, 0, Bounds.Right, Bounds.Bottom);
		return rect;
	}

	// ClientRect, ImageRect, ScrollRange
	public sScrollGeometry GetScrollGeometry()
	{
		CV.Rect2d rectClient = GetViewRect();
		CV.Point2d pt0 = m_ctScreenFromImage.Trans(new CV.Point2d(0, 0));
		CV.Point2d pt1 = m_ctScreenFromImage.Trans(new CV.Point2d(m_img.Cols, m_img.Rows));
		CV.Rect2d rectImageScreen = new(pt0.X, pt0.Y, pt1.X-pt0.X, pt1.Y-pt0.Y);
		rectImageScreen = NormalizeRect(rectImageScreen);

		CV.Rect2d rectScrollRange = rectClient;
		if (m_option.bExtendedPanning)
		{
			rectScrollRange.X -= rectImageScreen.Width;
			rectScrollRange.Y -= rectImageScreen.Height;
			rectScrollRange.Width += rectClient.Width;
			rectScrollRange.Height += rectClient.Height;

			double nScrollMarginX = Math.Min(m_option.nScrollMargin, rectImageScreen.Width);
			double nScrollMarginY = Math.Min(m_option.nScrollMargin, rectImageScreen.Height);
			DeflateRect(ref rectScrollRange, nScrollMarginX, nScrollMarginY);
		}
		else
		{
			rectScrollRange |= rectImageScreen;
		}
		return { rectClient, rectImageScreen, rectScrollRange};

	}
	public bool UpdateCT(bool bCenter = false, eZOOM eZoom = eZOOM::none)
	{

	}
	bool UpdateScrollBars();
	bool ZoomInOut(double step, xPoint2i ptAnchor, bool bCenter);
	bool SetZoom(double scale, xPoint2i ptAnchor, bool bCenter);
	bool ScrollTo(xPoint2d pt, std::chrono::milliseconds tsScroll = -1ms);
	bool Scroll(xPoint2d delta, std::chrono::milliseconds tsScroll = -1ms);
	void PurgeScroll(bool bUpdate = true);
	bool KeyboardNavigate(int key, bool ctrl = false, bool alt = false, bool shift = false);

	public CV.Rect2d NormalizeRect(CV.Rect2d rect)
	{
		CV.Rect2d rect_ = rect;
		if (rect.Width < 0)
		{
			rect.X += rect.Width;
			rect.Width = -rect.Width;
		}
		if (rect.Height < 0)
		{
			rect.Y += rect.Height;
			rect.Height = -rect.Height;
		}
		return rect_;
	}

	public void DeflateRect(ref CV.Rect2d rect, double dx, double dy)
	{
		rect.X += dx;
		rect.Y += dy;
		rect.Width -= dx * 2;
		rect.Height -= dy * 2;
	}

}
