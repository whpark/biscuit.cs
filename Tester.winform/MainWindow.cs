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

			var mat = new CV.Mat(65536, 65536, CV.MatType.CV_8UC3, new CV.Scalar(0, 0, 0));
			mat[mat.Rows/3*0, mat.Rows*1/3, mat.Cols*0/3, mat.Cols*1/3].SetTo(new CV.Scalar(255, 0, 0));
			mat[mat.Rows/3*1, mat.Rows*2/3, mat.Cols*1/3, mat.Cols*2/3].SetTo(new CV.Scalar(0, 255, 0));
			mat[mat.Rows/3*2, mat.Rows*3/3, mat.Cols*2/3, mat.Cols*3/3].SetTo(new CV.Scalar(0, 0, 255));
			//for (int y = 0; y < mat.Rows; y++) {
			//	for (int x = 0; x < mat.Cols; x++) {
			//		ref Vec3b v = ref mat.At<CV.Vec3b>(y, x);
			//		v[0] = (Byte)(y & 0xFF);
			//		v[1] = (Byte)(x & 0xFF);
			//		v[2] = (Byte)((x + y) & 0xFF);
			//	}
			//}
			ui_view.SetImage(mat);
		}

	}
}
