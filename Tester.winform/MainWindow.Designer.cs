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
			panelBottom = new System.Windows.Forms.Panel();
			matView1 = new Biscuit.winform.xMatView();
			panelBottom.SuspendLayout();
			SuspendLayout();
			// 
			// panelBottom
			// 
			panelBottom.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			panelBottom.Controls.Add(matView1);
			panelBottom.Location = new System.Drawing.Point(12, 12);
			panelBottom.Name = "panelBottom";
			panelBottom.Size = new System.Drawing.Size(811, 427);
			panelBottom.TabIndex = 2;
			// 
			// matView1
			// 
			matView1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			matView1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			matView1.Location = new System.Drawing.Point(0, 0);
			matView1.Name = "matView1";
			matView1.Size = new System.Drawing.Size(808, 424);
			matView1.TabIndex = 0;
			// 
			// MainWindow
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			ClientSize = new System.Drawing.Size(835, 451);
			Controls.Add(panelBottom);
			Name = "MainWindow";
			Text = "Test - MainWindow";
			panelBottom.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion

		private System.Windows.Forms.Panel panelBottom;
		private Biscuit.winform.xMatView matView1;
	}
}
