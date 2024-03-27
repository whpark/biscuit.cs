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
			tableLayoutPanel = new TableLayoutPanel();
			ui_btnOK = new Button();
			ui_btnCancel = new Button();
			SuspendLayout();
			// 
			// tableLayoutPanel
			// 
			tableLayoutPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			tableLayoutPanel.ColumnCount = 2;
			tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
			tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
			tableLayoutPanel.Location = new Point(12, 12);
			tableLayoutPanel.Name = "tableLayoutPanel";
			tableLayoutPanel.RowCount = 13;
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
			tableLayoutPanel.Size = new Size(301, 262);
			tableLayoutPanel.TabIndex = 0;
			// 
			// ui_btnOK
			// 
			ui_btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			ui_btnOK.Location = new Point(157, 290);
			ui_btnOK.Name = "ui_btnOK";
			ui_btnOK.Size = new Size(75, 23);
			ui_btnOK.TabIndex = 1;
			ui_btnOK.Text = "OK";
			ui_btnOK.UseVisualStyleBackColor = true;
			// 
			// ui_btnCancel
			// 
			ui_btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			ui_btnCancel.Location = new Point(238, 290);
			ui_btnCancel.Name = "ui_btnCancel";
			ui_btnCancel.Size = new Size(75, 23);
			ui_btnCancel.TabIndex = 2;
			ui_btnCancel.Text = "Cancel";
			ui_btnCancel.UseVisualStyleBackColor = true;
			// 
			// MatView_SettingsDlg
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(325, 325);
			Controls.Add(tableLayoutPanel);
			Controls.Add(ui_btnCancel);
			Controls.Add(ui_btnOK);
			Name = "MatView_SettingsDlg";
			Text = "MatView_SettingsDlg";
			ResumeLayout(false);
		}

		#endregion

		private TableLayoutPanel tableLayoutPanel;
		private Button ui_btnOK;
		private Button ui_btnCancel;
	}
}