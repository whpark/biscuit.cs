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
using CV = OpenCvSharp;

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
		get => _option.eZOOM;
		set => InitZoom(value);
	}

	public struct S_SCROLL_GEOMETRY
	{
		Avalonia.Rect rectClient, rectImageScreen, rectScrollRange;
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
		public int nScrollMargin = 5;                   // bExtendedPanning, px margin to scroll
		public int tsScroll = 250;                      // Smooth Scroll. duration
		public eZOOM_IN eZoomIn = eZOOM_IN.nearest;
		public eZOOM_OUT eZoomOut = eZOOM_OUT.area;
		public Vector3 crBackground = new Vector3(0, 0, 0); // rgb	//{161, 114, 230}

		public sOption()
		{
		}
	};

	sOption _option;

	private Rect _rectSource = new Rect(0, 0, 800, 600);
	private Rect _rectDest = new Rect(0, 0, 800, 600);

	public xRenderer()
	{
		_timerDraw = new Avalonia.Threading.DispatcherTimer();
		_timerDraw.Interval = TimeSpan.FromMilliseconds(1000 / 60);
	}

	public void Init()
	{
		InitZoom(eZOOM.fit2window);
	}

	protected void ReserveDraw()
	{
		_timerDraw.Stop();
		_timerDraw.Start();
	}

	protected void InitZoom(eZOOM eZoom)
	{
		ReserveDraw();

		_option.eZOOM = eZoom;
		if (m_img == null)
			return;

		switch (_option.eZOOM)
		{
			case eZOOM.one2one:
				_rectSource = new Rect(0, 0, m_img.Cols, m_img.Rows);
				_rectDest = new Rect(0, 0, m_img.Cols, m_img.Rows);
				break;
			case eZOOM.fit2window:
				_rectSource = new Rect(0, 0, m_img.Cols, m_img.Rows);
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
			context.DrawImage(bmp, new Rect(0, 0, m_img.Cols, m_img.Rows), Bounds);
		}
	}

	public struct sMouseOperation {
		public bool bInSelectionMode = false;
		public bool bRectSelected = false;
		Avalonia.PixelPoint? ptAnchor;		// Anchor Point in Screen Coordinate
		Avalonia.PixelPoint ptOffset0;		// offset backup
		Avalonia.Point ptSel0, ptSel1;		// Selection Point in Image Coordinate

		public sMouseOperation()
		{
		}

		public void Clear()
		{
			bInSelectionMode = false;
			bRectSelected = false ;
			ptAnchor = null;
			ptOffset0 = new PixelPoint();
			ptSel0 = new Point();
			ptSel1 = new Point();
		}
	};

	protected sMouseOperation m_mouse;
}
