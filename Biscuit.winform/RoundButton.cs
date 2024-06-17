using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biscuit.winform {
	public class RoundButton : System.Windows.Forms.Button {
		public int RadiusX { get; set; }
		public int RadiusY { get; set; }
		public RoundButton() {
			RadiusX = RadiusY = 30;
			BackColor = System.Drawing.Color.Gray;
			FlatStyle = FlatStyle.Flat;
			FlatAppearance.BorderSize = 0;
		}

		protected override void OnPaint(System.Windows.Forms.PaintEventArgs e) {
			System.Drawing.Drawing2D.GraphicsPath grPath = new System.Drawing.Drawing2D.GraphicsPath();
			if (RadiusX == 0 && RadiusY == 0)
				grPath.AddEllipse(0, 0, ClientSize.Width, ClientSize.Height);
			else {
				var w = ClientSize.Width;
				var h = ClientSize.Height;
				var rx = Math.Min(w/2, RadiusX);
				var ry = Math.Min(h/2, RadiusY);
				var dx = rx*2;
				var dy = ry*2;
				grPath.AddLine(rx, 0, w - rx, 0);
				grPath.AddArc(w - dx, 0, dx, dy, 270, 90);
				grPath.AddLine(w, ry, w, h - ry);
				grPath.AddArc(w - dx, h - dy, dx, dy, 0, 90);
				grPath.AddLine(w - rx, h, rx, h);
				grPath.AddArc(0, h - dy, dx, dy, 90, 90);
				grPath.AddLine(0, h - ry, 0, ry);
				grPath.AddArc(0, 0, dx, dy, 180, 90);
			}
			this.Region = new System.Drawing.Region(grPath);
			base.OnPaint(e);
		}
	}
}
