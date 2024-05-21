using Biscuit;
using Biscuit.winform;
using Microsoft.Win32;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CV = OpenCvSharp;

namespace Tester.winform {
	public partial class MainWindow : Form {

		public MainWindow() {
			// Set Working Directory
			var dir = System.IO.Directory.GetCurrentDirectory();
			var r = new System.Text.RegularExpressions.Regex(@"\\bin(\\(x86|x64))?\\(Debug|Release)\\net\d\.\d(-windows)?\z");
			var m = r.Match(dir);
			if (m.Success) {
				dir = dir.Substring(0, m.Index);
				System.IO.Directory.SetCurrentDirectory(dir);
			}

			InitializeComponent();

			ui_view.Reg = Registry.CurrentUser.CreateSubKey("Software\\Biscuit.cs\\Biscuit_Tester_Winform");
			ui_view.RegKey = "MainView";
			ui_view.LoadSettings();

			////var mat = new CV.Mat(65536, 65536, CV.MatType.CV_8UC3, new CV.Scalar(0, 0, 0));
			//var mat = new CV.Mat(600, 800, CV.MatType.CV_8UC1, new CV.Scalar(0, 0, 0));
			////mat[mat.Rows/3*0, mat.Rows*1/3, mat.Cols*0/3, mat.Cols*1/3].SetTo(new CV.Scalar(255, 0, 0));
			////mat[mat.Rows/3*1, mat.Rows*2/3, mat.Cols*1/3, mat.Cols*2/3].SetTo(new CV.Scalar(0, 255, 0));
			////mat[mat.Rows/3*2, mat.Rows*3/3, mat.Cols*2/3, mat.Cols*3/3].SetTo(new CV.Scalar(0, 0, 255));
			////for (int y = 0; y < mat.Rows; y++) {
			////	for (int x = 0; x < mat.Cols; x++) {
			////		ref Vec3b v = ref mat.At<CV.Vec3b>(y, x);
			////		v[0] = (Byte)(y & 0xFF);
			////		v[1] = (Byte)(x & 0xFF);
			////		v[2] = (Byte)((x + y) & 0xFF);
			////	}
			////}
			//for (int y = 0; y < mat.Rows; y++) {
			//	for (int x = 0; x < mat.Cols; x++) {
			//		mat.At<Byte>(y, x) = (Byte)((x + y) & 0xFF);
			//	}
			//}
			//CV.Mat palette = CV.Mat.Zeros(256, 1, CV.MatType.CV_8UC3);
			//for (int i = 0; i < 256; i++) {
			//	palette.At<CV.Vec3b>(i, 0) = new CV.Vec3b((byte)0, (byte)i, (byte)i);
			//}

			xImageHelper imgHelder = new();
			imgHelder.LoadImage(@"D:\Project\APS\1024x1600-1bpp.bmp");
			CV.Mat mat = imgHelder.GetIndexImage();
			CV.Mat palette = imgHelder.GetPalette();
			//ui_view.SetImage(mat, true, xMatView.eZOOM.fit2window, false);
			//ui_view.SetPalette(palette);
		}

	}
}
