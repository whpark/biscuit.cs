﻿using Biscuit;

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
	private Avalonia.Threading.DispatcherTimer _timerDraw;

	CV.Mat m_img;

	public CV.Mat Image {
		get => m_img;
		set {
			m_img = value;
			//Init();
		}
	}


	public xRenderer()
	{
		_timerDraw = new Avalonia.Threading.DispatcherTimer();
		_timerDraw.Interval = TimeSpan.FromMilliseconds(1000 / 60);
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

}
