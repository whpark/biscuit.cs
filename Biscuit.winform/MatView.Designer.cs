
namespace Biscuit.winform {
	partial class xMatView {
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

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			ui_chkShowToolbar = new CheckBox();
			ui_cmbZoomMode = new ComboBox();
			ui_edtZoom = new TextBox();
			ui_txtInfo = new TextBox();
			ui_btnSettings = new Button();
			ui_btnZoomFit = new Button();
			ui_btnZoomOut = new Button();
			ui_btnZoomIn = new Button();
			ui_scrollbarHorz = new HScrollBar();
			ui_scrollbarVert = new VScrollBar();
			panelToolbox = new Panel();
			ui_picture = new PictureBox();
			panelToolbox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)ui_picture).BeginInit();
			SuspendLayout();
			// 
			// ui_chkShowToolbar
			// 
			ui_chkShowToolbar.Checked = true;
			ui_chkShowToolbar.CheckState = CheckState.Checked;
			ui_chkShowToolbar.Location = new Point(3, 3);
			ui_chkShowToolbar.Name = "ui_chkShowToolbar";
			ui_chkShowToolbar.Size = new Size(18, 28);
			ui_chkShowToolbar.TabIndex = 0;
			ui_chkShowToolbar.UseVisualStyleBackColor = true;
			// 
			// ui_cmbZoomMode
			// 
			ui_cmbZoomMode.DropDownStyle = ComboBoxStyle.DropDownList;
			ui_cmbZoomMode.FormattingEnabled = true;
			ui_cmbZoomMode.Location = new Point(27, 3);
			ui_cmbZoomMode.Name = "ui_cmbZoomMode";
			ui_cmbZoomMode.Size = new Size(174, 28);
			ui_cmbZoomMode.TabIndex = 1;
			ui_cmbZoomMode.SelectedIndexChanged += ui_cmbZoomMode_SelectedIndexChanged;
			// 
			// ui_edtZoom
			// 
			ui_edtZoom.Location = new Point(207, 3);
			ui_edtZoom.Name = "ui_edtZoom";
			ui_edtZoom.Size = new Size(79, 27);
			ui_edtZoom.TabIndex = 2;
			ui_edtZoom.TextAlign = HorizontalAlignment.Right;
			// 
			// ui_txtInfo
			// 
			ui_txtInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			ui_txtInfo.Location = new Point(453, 3);
			ui_txtInfo.Name = "ui_txtInfo";
			ui_txtInfo.Size = new Size(59, 27);
			ui_txtInfo.TabIndex = 5;
			// 
			// ui_btnSettings
			// 
			ui_btnSettings.Location = new Point(413, 3);
			ui_btnSettings.Name = "ui_btnSettings";
			ui_btnSettings.Size = new Size(34, 29);
			ui_btnSettings.TabIndex = 4;
			ui_btnSettings.Text = "...";
			ui_btnSettings.UseVisualStyleBackColor = true;
			// 
			// ui_btnZoomFit
			// 
			ui_btnZoomFit.Location = new Point(373, 3);
			ui_btnZoomFit.Name = "ui_btnZoomFit";
			ui_btnZoomFit.Size = new Size(34, 29);
			ui_btnZoomFit.TabIndex = 4;
			ui_btnZoomFit.Text = "/";
			ui_btnZoomFit.UseVisualStyleBackColor = true;
			// 
			// ui_btnZoomOut
			// 
			ui_btnZoomOut.Location = new Point(333, 3);
			ui_btnZoomOut.Name = "ui_btnZoomOut";
			ui_btnZoomOut.Size = new Size(34, 29);
			ui_btnZoomOut.TabIndex = 4;
			ui_btnZoomOut.Text = "-";
			ui_btnZoomOut.UseVisualStyleBackColor = true;
			// 
			// ui_btnZoomIn
			// 
			ui_btnZoomIn.Location = new Point(293, 3);
			ui_btnZoomIn.Name = "ui_btnZoomIn";
			ui_btnZoomIn.Size = new Size(34, 29);
			ui_btnZoomIn.TabIndex = 4;
			ui_btnZoomIn.Text = "+";
			ui_btnZoomIn.UseVisualStyleBackColor = true;
			// 
			// ui_scrollbarHorz
			// 
			ui_scrollbarHorz.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			ui_scrollbarHorz.Cursor = Cursors.SizeWE;
			ui_scrollbarHorz.Location = new Point(0, 123);
			ui_scrollbarHorz.Name = "ui_scrollbarHorz";
			ui_scrollbarHorz.ScaleScrollBarForDpiChange = false;
			ui_scrollbarHorz.Size = new Size(489, 16);
			ui_scrollbarHorz.TabIndex = 1;
			ui_scrollbarHorz.Scroll += ui_scrollbarHorz_Scroll;
			// 
			// ui_scrollbarVert
			// 
			ui_scrollbarVert.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
			ui_scrollbarVert.Location = new Point(503, 33);
			ui_scrollbarVert.Name = "ui_scrollbarVert";
			ui_scrollbarVert.ScaleScrollBarForDpiChange = false;
			ui_scrollbarVert.Size = new Size(12, 79);
			ui_scrollbarVert.TabIndex = 2;
			ui_scrollbarVert.Scroll += ui_scrollbarVert_Scroll;
			// 
			// panelToolbox
			// 
			panelToolbox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			panelToolbox.Controls.Add(ui_btnSettings);
			panelToolbox.Controls.Add(ui_btnZoomFit);
			panelToolbox.Controls.Add(ui_btnZoomOut);
			panelToolbox.Controls.Add(ui_btnZoomIn);
			panelToolbox.Controls.Add(ui_edtZoom);
			panelToolbox.Controls.Add(ui_cmbZoomMode);
			panelToolbox.Controls.Add(ui_chkShowToolbar);
			panelToolbox.Controls.Add(ui_txtInfo);
			panelToolbox.Location = new Point(0, 0);
			panelToolbox.Name = "panelToolbox";
			panelToolbox.Size = new Size(515, 37);
			panelToolbox.TabIndex = 6;
			// 
			// ui_picture
			// 
			ui_picture.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			ui_picture.Location = new Point(0, 37);
			ui_picture.Name = "ui_picture";
			ui_picture.Size = new Size(502, 85);
			ui_picture.TabIndex = 7;
			ui_picture.TabStop = false;
			ui_picture.Paint += ui_picture_Paint;
			ui_picture.MouseDown += ui_picture_MouseDown;
			ui_picture.MouseLeave += ui_picture_MouseLeave;
			ui_picture.MouseMove += ui_picture_MouseMove;
			ui_picture.MouseUp += ui_picture_MouseUp;
			ui_picture.Resize += ui_picture_Resize;
			ui_picture.MouseWheel += ui_picture_MouseWheel;
			// 
			// xMatView
			// 
			AutoScaleDimensions = new SizeF(8F, 20F);
			AutoScaleMode = AutoScaleMode.Font;
			Controls.Add(ui_scrollbarVert);
			Controls.Add(ui_scrollbarHorz);
			Controls.Add(ui_picture);
			Controls.Add(panelToolbox);
			Name = "xMatView";
			Size = new Size(515, 139);
			Load += MatView_Load;
			Resize += xMatView_Resize;
			panelToolbox.ResumeLayout(false);
			panelToolbox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)ui_picture).EndInit();
			ResumeLayout(false);
		}

		#endregion
		private CheckBox ui_chkShowToolbar;
		private ComboBox ui_cmbZoomMode;
		private HScrollBar ui_scrollbarHorz;
		private VScrollBar ui_scrollbarVert;
		private TextBox ui_edtZoom;
		private Button ui_btnZoomIn;
		private Button ui_btnZoomOut;
		private TextBox ui_txtInfo;
		private Button ui_btnSettings;
		private Button ui_btnZoomFit;
		private Panel panelToolbox;
		private PictureBox ui_picture;
	}
}
