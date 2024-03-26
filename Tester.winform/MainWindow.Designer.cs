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
			panelBottom = new System.Windows.Forms.Panel();
			ui_view = new Biscuit.winform.xMatView();
			panelBottom.SuspendLayout();
			SuspendLayout();
			// 
			// panelBottom
			// 
			panelBottom.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			panelBottom.Controls.Add(ui_view);
			panelBottom.Location = new System.Drawing.Point(11, 12);
			panelBottom.Name = "panelBottom";
			panelBottom.Size = new System.Drawing.Size(752, 374);
			panelBottom.TabIndex = 2;
			// 
			// ui_view
			// 
			ui_view.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			ui_view.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			ui_view.Location = new System.Drawing.Point(3, 3);
			ui_view.Name = "ui_view";
			ui_view.Settings = sSettings1;
			ui_view.Size = new System.Drawing.Size(746, 371);
			ui_view.TabIndex = 0;
			// 
			// MainWindow
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			ClientSize = new System.Drawing.Size(776, 398);
			Controls.Add(panelBottom);
			Name = "MainWindow";
			Text = "Test - MainWindow";
			panelBottom.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion

		private System.Windows.Forms.Panel panelBottom;
		private Biscuit.winform.xMatView ui_view;
	}
}
