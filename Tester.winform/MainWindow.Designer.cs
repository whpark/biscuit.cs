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
			SuspendLayout();
			// 
			// ui_view
			// 
			ui_view.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			ui_view.Dock = System.Windows.Forms.DockStyle.Fill;
			ui_view.Location = new System.Drawing.Point(0, 0);
			ui_view.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			ui_view.Name = "ui_view";
			ui_view.Settings = sSettings1;
			ui_view.Size = new System.Drawing.Size(464, 113);
			ui_view.TabIndex = 0;
			// 
			// MainWindow
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			ClientSize = new System.Drawing.Size(464, 113);
			Controls.Add(ui_view);
			Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			Name = "MainWindow";
			Text = "Test - MainWindow";
			ResumeLayout(false);
		}

		#endregion

		private Biscuit.winform.xMatView ui_view;
	}
}
