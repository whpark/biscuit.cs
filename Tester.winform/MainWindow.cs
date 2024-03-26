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

			var mat = new CV.Mat(512, 512, CV.MatType.CV_8UC3, new CV.Scalar(0, 0, 0));
			mat[mat.Rows/3*0, mat.Rows*1/3, 0, mat.Cols].SetTo(new CV.Scalar(255, 0, 0));
			mat[mat.Rows/3*1, mat.Rows*2/3, 0, mat.Cols].SetTo(new CV.Scalar(0, 255, 0));
			mat[mat.Rows/3*2, mat.Rows*3/3, 0, mat.Cols].SetTo(new CV.Scalar(0, 0, 255));
			ui_view.SetImage(mat);
		}

	}
}
