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
using CV = OpenCvSharp;
using Tmds.DBus.Protocol;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualBasic.FileIO;
using OpenCvSharp.Detail;

//using Avalonia.Threading;
//using Avalonia.VisualTree;
//using Avalonia.Utilities;

namespace Biscuit.MatView.avalonia;

public class xRenderer : Control
{
	//private Avalonia.Threading.DispatcherTimer _timerDraw;

	CV.Mat? m_img;
	Avalonia.Rect? m_rectSelected;

	public CV.Mat? Image {
		get => m_img;
		set {
			m_img = value;
		}
	}
	public CV.Rect2d m_rectTarget { get; set; } = new CV.Rect2d(0, 0, 0, 0);
	public Avalonia.Rect? SelectedRect
	{
		get => m_rectSelected;
		set
		{
			m_rectSelected = value;
			InvalidateVisual();
		}
	}

	public xRenderer()
	{
		//_timerDraw = new Avalonia.Threading.DispatcherTimer();
		//_timerDraw.Interval = TimeSpan.FromMilliseconds(1000 / 60);
	}

	public override void Render(DrawingContext context)
	{
		if (m_img == null)
		{
			context.DrawRectangle(Brushes.DarkGray, null, Bounds);
			return;
		}
		context.DrawRectangle(Brushes.Black, null, Bounds);

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
			context.DrawImage(bmp,
				new Avalonia.Rect(0, 0, m_img.Cols, m_img.Rows),
				new Avalonia.Rect(m_rectTarget.X, m_rectTarget.Y, m_rectTarget.Width, m_rectTarget.Height));
		}

		if (m_rectSelected != null)
		{
			context.DrawRectangle(null, new Pen(Brushes.Red, 2), m_rectSelected.Value);
		}
	}

}
