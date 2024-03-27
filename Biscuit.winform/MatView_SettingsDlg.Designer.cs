namespace Biscuit.winform {
	partial class MatView_SettingsDlg {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
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
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			ui_btnOK = new Button();
			ui_btnCancel = new Button();
			ui_chkZoomLock = new CheckBox();
			ui_chkPanningLock = new CheckBox();
			ui_chkExtendedPanning = new CheckBox();
			ui_chkDrawPixelValue = new CheckBox();
			ui_chkPyrImageDown = new CheckBox();
			label1 = new Label();
			ui_spinImagePanningSpeed = new NumericUpDown();
			label2 = new Label();
			ui_spinScrollMargin = new NumericUpDown();
			label3 = new Label();
			ui_spinScrollDuration = new NumericUpDown();
			ui_cmbZoomIn = new ComboBox();
			ui_cmbZoomOut = new ComboBox();
			label4 = new Label();
			label5 = new Label();
			ui_btnBackgroundColor = new Button();
			label6 = new Label();
			label7 = new Label();
			ui_chkKeyboardNavigation = new CheckBox();
			label8 = new Label();
			ui_txtBackgroundColor = new TextBox();
			colorDialog1 = new ColorDialog();
			((System.ComponentModel.ISupportInitialize)ui_spinImagePanningSpeed).BeginInit();
			((System.ComponentModel.ISupportInitialize)ui_spinScrollMargin).BeginInit();
			((System.ComponentModel.ISupportInitialize)ui_spinScrollDuration).BeginInit();
			SuspendLayout();
			// 
			// ui_btnOK
			// 
			ui_btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			ui_btnOK.Location = new Point(120, 354);
			ui_btnOK.Name = "ui_btnOK";
			ui_btnOK.Size = new Size(75, 23);
			ui_btnOK.TabIndex = 100;
			ui_btnOK.Text = "OK";
			ui_btnOK.UseVisualStyleBackColor = true;
			ui_btnOK.Click += ui_btnOK_Click;
			// 
			// ui_btnCancel
			// 
			ui_btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			ui_btnCancel.Location = new Point(201, 354);
			ui_btnCancel.Name = "ui_btnCancel";
			ui_btnCancel.Size = new Size(75, 23);
			ui_btnCancel.TabIndex = 101;
			ui_btnCancel.Text = "Cancel";
			ui_btnCancel.UseVisualStyleBackColor = true;
			ui_btnCancel.Click += ui_btnCancel_Click;
			// 
			// ui_chkZoomLock
			// 
			ui_chkZoomLock.AutoSize = true;
			ui_chkZoomLock.Location = new Point(12, 12);
			ui_chkZoomLock.Name = "ui_chkZoomLock";
			ui_chkZoomLock.Size = new Size(188, 19);
			ui_chkZoomLock.TabIndex = 0;
			ui_chkZoomLock.Text = "Zoom Lock (No Mouse Wheel)";
			ui_chkZoomLock.UseVisualStyleBackColor = true;
			// 
			// ui_chkPanningLock
			// 
			ui_chkPanningLock.AutoSize = true;
			ui_chkPanningLock.Location = new Point(12, 37);
			ui_chkPanningLock.Name = "ui_chkPanningLock";
			ui_chkPanningLock.Size = new Size(98, 19);
			ui_chkPanningLock.TabIndex = 1;
			ui_chkPanningLock.Text = "Panning Lock";
			ui_chkPanningLock.UseVisualStyleBackColor = true;
			// 
			// ui_chkExtendedPanning
			// 
			ui_chkExtendedPanning.AutoSize = true;
			ui_chkExtendedPanning.Location = new Point(12, 62);
			ui_chkExtendedPanning.Name = "ui_chkExtendedPanning";
			ui_chkExtendedPanning.Size = new Size(122, 19);
			ui_chkExtendedPanning.TabIndex = 2;
			ui_chkExtendedPanning.Text = "Extended Panning";
			ui_chkExtendedPanning.UseVisualStyleBackColor = true;
			// 
			// ui_chkDrawPixelValue
			// 
			ui_chkDrawPixelValue.AutoSize = true;
			ui_chkDrawPixelValue.Location = new Point(12, 112);
			ui_chkDrawPixelValue.Name = "ui_chkDrawPixelValue";
			ui_chkDrawPixelValue.Size = new Size(114, 19);
			ui_chkDrawPixelValue.TabIndex = 4;
			ui_chkDrawPixelValue.Text = "Show Pixel Value";
			ui_chkDrawPixelValue.UseVisualStyleBackColor = true;
			// 
			// ui_chkPyrImageDown
			// 
			ui_chkPyrImageDown.AutoSize = true;
			ui_chkPyrImageDown.Location = new Point(12, 137);
			ui_chkPyrImageDown.Name = "ui_chkPyrImageDown";
			ui_chkPyrImageDown.Size = new Size(209, 19);
			ui_chkPyrImageDown.TabIndex = 5;
			ui_chkPyrImageDown.Text = "Build image Pyramid (fast resizing)";
			ui_chkPyrImageDown.UseVisualStyleBackColor = true;
			// 
			// label1
			// 
			label1.Location = new Point(12, 164);
			label1.Name = "label1";
			label1.Size = new Size(130, 15);
			label1.TabIndex = 6;
			label1.Text = "Image Panning Speed :";
			label1.TextAlign = ContentAlignment.MiddleRight;
			// 
			// ui_spinImagePanningSpeed
			// 
			ui_spinImagePanningSpeed.DecimalPlaces = 2;
			ui_spinImagePanningSpeed.Increment = new decimal(new int[] { 25, 0, 0, 131072 });
			ui_spinImagePanningSpeed.Location = new Point(152, 162);
			ui_spinImagePanningSpeed.Name = "ui_spinImagePanningSpeed";
			ui_spinImagePanningSpeed.Size = new Size(57, 23);
			ui_spinImagePanningSpeed.TabIndex = 7;
			ui_spinImagePanningSpeed.TextAlign = HorizontalAlignment.Right;
			// 
			// label2
			// 
			label2.Location = new Point(12, 193);
			label2.Name = "label2";
			label2.Size = new Size(130, 15);
			label2.TabIndex = 8;
			label2.Text = "Scroll Margin :";
			label2.TextAlign = ContentAlignment.MiddleRight;
			// 
			// ui_spinScrollMargin
			// 
			ui_spinScrollMargin.Location = new Point(153, 191);
			ui_spinScrollMargin.Name = "ui_spinScrollMargin";
			ui_spinScrollMargin.Size = new Size(57, 23);
			ui_spinScrollMargin.TabIndex = 8;
			ui_spinScrollMargin.TextAlign = HorizontalAlignment.Right;
			// 
			// label3
			// 
			label3.Enabled = false;
			label3.Location = new Point(12, 222);
			label3.Name = "label3";
			label3.Size = new Size(130, 15);
			label3.TabIndex = 9;
			label3.Text = "Scroll Duration :";
			label3.TextAlign = ContentAlignment.MiddleRight;
			// 
			// ui_spinScrollDuration
			// 
			ui_spinScrollDuration.Enabled = false;
			ui_spinScrollDuration.Location = new Point(153, 220);
			ui_spinScrollDuration.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
			ui_spinScrollDuration.Name = "ui_spinScrollDuration";
			ui_spinScrollDuration.Size = new Size(57, 23);
			ui_spinScrollDuration.TabIndex = 10;
			ui_spinScrollDuration.TextAlign = HorizontalAlignment.Right;
			// 
			// ui_cmbZoomIn
			// 
			ui_cmbZoomIn.DropDownStyle = ComboBoxStyle.DropDownList;
			ui_cmbZoomIn.FormattingEnabled = true;
			ui_cmbZoomIn.Location = new Point(152, 249);
			ui_cmbZoomIn.Name = "ui_cmbZoomIn";
			ui_cmbZoomIn.Size = new Size(121, 23);
			ui_cmbZoomIn.TabIndex = 11;
			// 
			// ui_cmbZoomOut
			// 
			ui_cmbZoomOut.DropDownStyle = ComboBoxStyle.DropDownList;
			ui_cmbZoomOut.FormattingEnabled = true;
			ui_cmbZoomOut.Location = new Point(152, 278);
			ui_cmbZoomOut.Name = "ui_cmbZoomOut";
			ui_cmbZoomOut.Size = new Size(121, 23);
			ui_cmbZoomOut.TabIndex = 12;
			// 
			// label4
			// 
			label4.Location = new Point(12, 252);
			label4.Name = "label4";
			label4.Size = new Size(130, 15);
			label4.TabIndex = 11;
			label4.Text = "Zoom In :";
			label4.TextAlign = ContentAlignment.MiddleRight;
			// 
			// label5
			// 
			label5.Location = new Point(12, 281);
			label5.Name = "label5";
			label5.Size = new Size(130, 15);
			label5.TabIndex = 12;
			label5.Text = "Zoom Out :";
			label5.TextAlign = ContentAlignment.MiddleRight;
			// 
			// ui_btnBackgroundColor
			// 
			ui_btnBackgroundColor.Location = new Point(12, 302);
			ui_btnBackgroundColor.Name = "ui_btnBackgroundColor";
			ui_btnBackgroundColor.Size = new Size(130, 30);
			ui_btnBackgroundColor.TabIndex = 13;
			ui_btnBackgroundColor.Text = "Background Color :";
			ui_btnBackgroundColor.UseVisualStyleBackColor = true;
			ui_btnBackgroundColor.Click += ui_btnBackgroundColor_Click;
			// 
			// label6
			// 
			label6.AutoSize = true;
			label6.Location = new Point(215, 164);
			label6.Name = "label6";
			label6.Size = new Size(13, 15);
			label6.TabIndex = 5;
			label6.Text = "x";
			label6.TextAlign = ContentAlignment.MiddleRight;
			// 
			// label7
			// 
			label7.AutoSize = true;
			label7.Location = new Point(215, 193);
			label7.Name = "label7";
			label7.Size = new Size(20, 15);
			label7.TabIndex = 5;
			label7.Text = "px";
			label7.TextAlign = ContentAlignment.MiddleRight;
			// 
			// ui_chkKeyboardNavigation
			// 
			ui_chkKeyboardNavigation.AutoSize = true;
			ui_chkKeyboardNavigation.Enabled = false;
			ui_chkKeyboardNavigation.Location = new Point(12, 87);
			ui_chkKeyboardNavigation.Name = "ui_chkKeyboardNavigation";
			ui_chkKeyboardNavigation.Size = new Size(137, 19);
			ui_chkKeyboardNavigation.TabIndex = 3;
			ui_chkKeyboardNavigation.Text = "Keyboard Navigation";
			ui_chkKeyboardNavigation.UseVisualStyleBackColor = true;
			// 
			// label8
			// 
			label8.AutoSize = true;
			label8.Enabled = false;
			label8.Location = new Point(215, 222);
			label8.Name = "label8";
			label8.Size = new Size(35, 15);
			label8.TabIndex = 10;
			label8.Text = "msec";
			label8.TextAlign = ContentAlignment.MiddleRight;
			// 
			// ui_txtBackgroundColor
			// 
			ui_txtBackgroundColor.Location = new Point(152, 307);
			ui_txtBackgroundColor.Name = "ui_txtBackgroundColor";
			ui_txtBackgroundColor.ReadOnly = true;
			ui_txtBackgroundColor.Size = new Size(121, 23);
			ui_txtBackgroundColor.TabIndex = 14;
			// 
			// MatView_SettingsDlg
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(288, 389);
			Controls.Add(ui_txtBackgroundColor);
			Controls.Add(ui_btnBackgroundColor);
			Controls.Add(ui_cmbZoomOut);
			Controls.Add(ui_cmbZoomIn);
			Controls.Add(ui_spinScrollDuration);
			Controls.Add(ui_spinScrollMargin);
			Controls.Add(label5);
			Controls.Add(label4);
			Controls.Add(label3);
			Controls.Add(ui_spinImagePanningSpeed);
			Controls.Add(label2);
			Controls.Add(label8);
			Controls.Add(label7);
			Controls.Add(label6);
			Controls.Add(label1);
			Controls.Add(ui_chkZoomLock);
			Controls.Add(ui_chkPanningLock);
			Controls.Add(ui_chkKeyboardNavigation);
			Controls.Add(ui_chkExtendedPanning);
			Controls.Add(ui_chkDrawPixelValue);
			Controls.Add(ui_btnCancel);
			Controls.Add(ui_chkPyrImageDown);
			Controls.Add(ui_btnOK);
			Name = "MatView_SettingsDlg";
			Text = "MatView_SettingsDlg";
			((System.ComponentModel.ISupportInitialize)ui_spinImagePanningSpeed).EndInit();
			((System.ComponentModel.ISupportInitialize)ui_spinScrollMargin).EndInit();
			((System.ComponentModel.ISupportInitialize)ui_spinScrollDuration).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		private Button ui_btnOK;
		private Button ui_btnCancel;
		private CheckBox ui_chkZoomLock;
		private CheckBox ui_chkPanningLock;
		private CheckBox ui_chkExtendedPanning;
		private CheckBox ui_chkPyrImageDown;
		private CheckBox ui_chkDrawPixelValue;
		private Label label1;
		private NumericUpDown ui_spinImagePanningSpeed;
		private Label label2;
		private NumericUpDown ui_spinScrollMargin;
		private Label label3;
		private NumericUpDown ui_spinScrollDuration;
		private ComboBox ui_cmbZoomIn;
		private ComboBox ui_cmbZoomOut;
		private Label label4;
		private Label label5;
		private Button ui_btnBackgroundColor;
		private Label label6;
		private Label label7;
		private CheckBox ui_chkKeyboardNavigation;
		private Label label8;
		private TextBox ui_txtBackgroundColor;
		private ColorDialog colorDialog1;
	}
}