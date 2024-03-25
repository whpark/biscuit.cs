using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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


		}

	}
}
