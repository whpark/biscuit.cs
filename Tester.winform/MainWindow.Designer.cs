namespace Tester.winform {
	partial class MainWindow {
		/// <summary>
		///  Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///  Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			Biscuit.winform.xMatView.sSettings sSettings1 = new Biscuit.winform.xMatView.sSettings();
			ui_view = new Biscuit.winform.xMatView();
			roundButton1 = new Biscuit.winform.RoundButton();
			SuspendLayout();
			// 
			// ui_view
			// 
			ui_view.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			ui_view.Location = new System.Drawing.Point(32, 73);
			ui_view.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			ui_view.Name = "ui_view";
			ui_view.Reg = null;
			ui_view.RegKey = "MatView";
			sSettings1.bDrawPixelValue = true;
			sSettings1.bExtendedPanning = true;
			sSettings1.bKeyboardNavigation = false;
			sSettings1.bPanningLock = true;
			sSettings1.bPyrImageDown = true;
			sSettings1.bZoomLock = false;
			sSettings1.crBackgroundB = 161;
			sSettings1.crBackgroundG = 114;
			sSettings1.crBackgroundR = 230;
			sSettings1.dPanningSpeed = 2D;
			sSettings1.eZoomIn = Biscuit.winform.xMatView.eZOOM_IN.nearest;
			sSettings1.eZoomOut = Biscuit.winform.xMatView.eZOOM_OUT.area;
			sSettings1.nScrollMargin = 5;
			sSettings1.tsScroll = 250U;
			ui_view.Settings = sSettings1;
			ui_view.Size = new System.Drawing.Size(464, 113);
			ui_view.TabIndex = 0;
			// 
			// roundButton1
			// 
			roundButton1.Location = new System.Drawing.Point(12, 12);
			roundButton1.Name = "roundButton1";
			roundButton1.Size = new System.Drawing.Size(109, 36);
			roundButton1.TabIndex = 1;
			roundButton1.Text = "roundButton1";
			roundButton1.UseVisualStyleBackColor = true;
			// 
			// MainWindow
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			ClientSize = new System.Drawing.Size(592, 309);
			Controls.Add(roundButton1);
			Controls.Add(ui_view);
			Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			Name = "MainWindow";
			Text = "Test - MainWindow";
			ResumeLayout(false);
		}

		#endregion

		private Biscuit.winform.xMatView ui_view;
		private Biscuit.winform.RoundButton roundButton1;
	}
}
