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
			roundButton1 = new Biscuit.winform.xRoundButton();
			ui_btnRotate = new System.Windows.Forms.Button();
			ui_btnLoad = new System.Windows.Forms.Button();
			ui_btnTF = new System.Windows.Forms.Button();
			SuspendLayout();
			// 
			// ui_view
			// 
			ui_view.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			ui_view.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			ui_view.Location = new System.Drawing.Point(12, 59);
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
			ui_view.Size = new System.Drawing.Size(568, 239);
			ui_view.TabIndex = 0;
			// 
			// roundButton1
			// 
			roundButton1.BackColor = System.Drawing.Color.Gray;
			roundButton1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			roundButton1.Location = new System.Drawing.Point(12, 12);
			roundButton1.Name = "roundButton1";
			roundButton1.RadiusX = 30;
			roundButton1.RadiusY = 30;
			roundButton1.Size = new System.Drawing.Size(109, 36);
			roundButton1.TabIndex = 1;
			roundButton1.Text = "roundButton1";
			roundButton1.UseVisualStyleBackColor = true;
			// 
			// ui_btnRotate
			// 
			ui_btnRotate.Location = new System.Drawing.Point(285, 31);
			ui_btnRotate.Name = "ui_btnRotate";
			ui_btnRotate.Size = new System.Drawing.Size(75, 23);
			ui_btnRotate.TabIndex = 2;
			ui_btnRotate.Text = "Rotate";
			ui_btnRotate.UseVisualStyleBackColor = true;
			ui_btnRotate.Click += ui_btnRotate_Click;
			// 
			// ui_btnLoad
			// 
			ui_btnLoad.Location = new System.Drawing.Point(174, 31);
			ui_btnLoad.Name = "ui_btnLoad";
			ui_btnLoad.Size = new System.Drawing.Size(75, 23);
			ui_btnLoad.TabIndex = 2;
			ui_btnLoad.Text = "Load";
			ui_btnLoad.UseVisualStyleBackColor = true;
			ui_btnLoad.Click += ui_btnLoad_Click;
			// 
			// ui_btnTF
			// 
			ui_btnTF.Location = new System.Drawing.Point(366, 31);
			ui_btnTF.Name = "ui_btnTF";
			ui_btnTF.Size = new System.Drawing.Size(75, 23);
			ui_btnTF.TabIndex = 2;
			ui_btnTF.Text = "t && flip";
			ui_btnTF.UseVisualStyleBackColor = true;
			ui_btnTF.Click += ui_btnTF_Click;
			// 
			// MainWindow
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			ClientSize = new System.Drawing.Size(592, 309);
			Controls.Add(ui_btnLoad);
			Controls.Add(ui_btnTF);
			Controls.Add(ui_btnRotate);
			Controls.Add(roundButton1);
			Controls.Add(ui_view);
			Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			Name = "MainWindow";
			Text = "Test - MainWindow";
			ResumeLayout(false);
		}

		#endregion

		private Biscuit.winform.xMatView ui_view;
		private Biscuit.winform.xRoundButton roundButton1;
		private System.Windows.Forms.Button ui_btnRotate;
		private System.Windows.Forms.Button ui_btnLoad;
		private System.Windows.Forms.Button ui_btnTF;
	}
}
