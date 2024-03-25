using System;
using System.Collections;
using System.Collections.Generic;
using Avalonia.Controls;
using CV = OpenCvSharp;
using Biscuit;

namespace Tester.avalonia.Views {
	public partial class MainWindow : Window {
		public MainWindow() {
			InitializeComponent();

			// Set Working Directory
			var dir = System.IO.Directory.GetCurrentDirectory();
			var r = new System.Text.RegularExpressions.Regex(@"\\bin(\\(x86|x64))?\\(Debug|Release)\\net\d\.\d(-windows)?\z");
			var m = r.Match(dir);
			if (m.Success) {
				dir = dir.Substring(0, m.Index);
				System.IO.Directory.SetCurrentDirectory(dir);
			}

			//CV.Mat img = CV.Mat.Zeros(800, 600, CV.MatType.CV_8UC3);
			//var m = img.GetGenericIndexer<CV.Vec3b>();
			//for (int y = 0; y < img.Rows; y++)
			//{
			//	for (int x = 0; x < img.Cols; x++)
			//	{
			//		m[y, x] = new CV.Vec3b((byte)(x * 255 / img.Cols), (byte)(y * 255 / img.Rows), (byte)(y * 255 * 255 / img.Rows / img.Cols));
			//	}
			//}

			Biscuit.xImageHelper il = new ();
			il.LoadImage("../test_resource/1000x1000-4bpp.bmp");
			CV.Mat img  = il.GetImage(false);

			ui_view.Image = img;
		}
	}
}