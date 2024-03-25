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
			flowLayoutPanel1 = new FlowLayoutPanel();
			ui_chkShowToolbar = new CheckBox();
			ui_cmbZoomMode = new ComboBox();
			ui_edtZoom = new TextBox();
			label1 = new Label();
			ui_pnlToolbar = new Panel();
			ui_txtInfo = new TextBox();
			ui_btnSettings = new Button();
			ui_btnZoomFit = new Button();
			ui_btnZoomOut = new Button();
			ui_btnZoomIn = new Button();
			ui_scrollbarHorz = new HScrollBar();
			ui_scrollbarVert = new VScrollBar();
			ui_gl = new SharpGL.OpenGLControl();
			ui_pnlToolbar.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)ui_gl).BeginInit();
			SuspendLayout();
			// 
			// flowLayoutPanel1
			// 
			flowLayoutPanel1.AutoSize = true;
			flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			flowLayoutPanel1.Location = new Point(0, 0);
			flowLayoutPanel1.Name = "flowLayoutPanel1";
			flowLayoutPanel1.Size = new Size(0, 0);
			flowLayoutPanel1.TabIndex = 0;
			// 
			// ui_chkShowToolbar
			// 
			ui_chkShowToolbar.AutoSize = true;
			ui_chkShowToolbar.Checked = true;
			ui_chkShowToolbar.CheckState = CheckState.Checked;
			ui_chkShowToolbar.Location = new Point(5, 11);
			ui_chkShowToolbar.Name = "ui_chkShowToolbar";
			ui_chkShowToolbar.Size = new Size(18, 17);
			ui_chkShowToolbar.TabIndex = 0;
			ui_chkShowToolbar.UseVisualStyleBackColor = true;
			// 
			// ui_cmbZoomMode
			// 
			ui_cmbZoomMode.DropDownStyle = ComboBoxStyle.DropDownList;
			ui_cmbZoomMode.FormattingEnabled = true;
			ui_cmbZoomMode.Location = new Point(33, 5);
			ui_cmbZoomMode.Name = "ui_cmbZoomMode";
			ui_cmbZoomMode.Size = new Size(174, 28);
			ui_cmbZoomMode.TabIndex = 1;
			ui_cmbZoomMode.SelectedIndexChanged += ui_cmbZoomMode_SelectedIndexChanged;
			// 
			// ui_edtZoom
			// 
			ui_edtZoom.Location = new Point(213, 5);
			ui_edtZoom.Name = "ui_edtZoom";
			ui_edtZoom.Size = new Size(128, 27);
			ui_edtZoom.TabIndex = 2;
			ui_edtZoom.TextAlign = HorizontalAlignment.Right;
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new Point(347, 8);
			label1.Name = "label1";
			label1.Size = new Size(21, 20);
			label1.TabIndex = 3;
			label1.Text = "%";
			// 
			// ui_pnlToolbar
			// 
			ui_pnlToolbar.Controls.Add(ui_txtInfo);
			ui_pnlToolbar.Controls.Add(ui_btnSettings);
			ui_pnlToolbar.Controls.Add(ui_btnZoomFit);
			ui_pnlToolbar.Controls.Add(ui_btnZoomOut);
			ui_pnlToolbar.Controls.Add(ui_btnZoomIn);
			ui_pnlToolbar.Controls.Add(ui_chkShowToolbar);
			ui_pnlToolbar.Controls.Add(flowLayoutPanel1);
			ui_pnlToolbar.Controls.Add(ui_cmbZoomMode);
			ui_pnlToolbar.Controls.Add(label1);
			ui_pnlToolbar.Controls.Add(ui_edtZoom);
			ui_pnlToolbar.Dock = DockStyle.Top;
			ui_pnlToolbar.Location = new Point(0, 0);
			ui_pnlToolbar.Name = "ui_pnlToolbar";
			ui_pnlToolbar.Size = new Size(833, 38);
			ui_pnlToolbar.TabIndex = 0;
			// 
			// ui_txtInfo
			// 
			ui_txtInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			ui_txtInfo.Location = new Point(530, 5);
			ui_txtInfo.Name = "ui_txtInfo";
			ui_txtInfo.Size = new Size(300, 27);
			ui_txtInfo.TabIndex = 5;
			// 
			// ui_btnSettings
			// 
			ui_btnSettings.Location = new Point(486, 4);
			ui_btnSettings.Name = "ui_btnSettings";
			ui_btnSettings.Size = new Size(34, 29);
			ui_btnSettings.TabIndex = 4;
			ui_btnSettings.Text = "...";
			ui_btnSettings.UseVisualStyleBackColor = true;
			// 
			// ui_btnZoomFit
			// 
			ui_btnZoomFit.Location = new Point(446, 4);
			ui_btnZoomFit.Name = "ui_btnZoomFit";
			ui_btnZoomFit.Size = new Size(34, 29);
			ui_btnZoomFit.TabIndex = 4;
			ui_btnZoomFit.Text = "/";
			ui_btnZoomFit.UseVisualStyleBackColor = true;
			// 
			// ui_btnZoomOut
			// 
			ui_btnZoomOut.Location = new Point(409, 4);
			ui_btnZoomOut.Name = "ui_btnZoomOut";
			ui_btnZoomOut.Size = new Size(34, 29);
			ui_btnZoomOut.TabIndex = 4;
			ui_btnZoomOut.Text = "-";
			ui_btnZoomOut.UseVisualStyleBackColor = true;
			// 
			// ui_btnZoomIn
			// 
			ui_btnZoomIn.Location = new Point(371, 4);
			ui_btnZoomIn.Name = "ui_btnZoomIn";
			ui_btnZoomIn.Size = new Size(34, 29);
			ui_btnZoomIn.TabIndex = 4;
			ui_btnZoomIn.Text = "+";
			ui_btnZoomIn.UseVisualStyleBackColor = true;
			// 
			// ui_scrollbarHorz
			// 
			ui_scrollbarHorz.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			ui_scrollbarHorz.Location = new Point(0, 217);
			ui_scrollbarHorz.Name = "ui_scrollbarHorz";
			ui_scrollbarHorz.Size = new Size(807, 26);
			ui_scrollbarHorz.TabIndex = 1;
			// 
			// ui_scrollbarVert
			// 
			ui_scrollbarVert.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
			ui_scrollbarVert.Location = new Point(807, 41);
			ui_scrollbarVert.Name = "ui_scrollbarVert";
			ui_scrollbarVert.Size = new Size(26, 176);
			ui_scrollbarVert.TabIndex = 2;
			// 
			// ui_gl
			// 
			ui_gl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			ui_gl.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			ui_gl.DrawFPS = false;
			ui_gl.FrameRate = 0;
			ui_gl.Location = new Point(4, 41);
			ui_gl.Margin = new Padding(4, 5, 4, 5);
			ui_gl.Name = "ui_gl";
			ui_gl.OpenGLVersion = SharpGL.Version.OpenGLVersion.OpenGL4_0;
			ui_gl.RenderContextType = SharpGL.RenderContextType.FBO;
			ui_gl.RenderTrigger = SharpGL.RenderTrigger.TimerBased;
			ui_gl.Size = new Size(799, 171);
			ui_gl.TabIndex = 3;
			// 
			// xMatView
			// 
			AutoScaleDimensions = new SizeF(8F, 20F);
			AutoScaleMode = AutoScaleMode.Font;
			Controls.Add(ui_gl);
			Controls.Add(ui_scrollbarVert);
			Controls.Add(ui_scrollbarHorz);
			Controls.Add(ui_pnlToolbar);
			Name = "xMatView";
			Size = new Size(833, 243);
			Load += MatView_Load;
			ui_pnlToolbar.ResumeLayout(false);
			ui_pnlToolbar.PerformLayout();
			((System.ComponentModel.ISupportInitialize)ui_gl).EndInit();
			ResumeLayout(false);
		}

		#endregion

		private FlowLayoutPanel flowLayoutPanel1;
		private CheckBox ui_chkShowToolbar;
		private ComboBox ui_cmbZoomMode;
		private Panel ui_pnlToolbar;
		private HScrollBar ui_scrollbarHorz;
		private VScrollBar ui_scrollbarVert;
		private TextBox ui_edtZoom;
		private Label label1;
		private Button ui_btnZoomIn;
		private Button ui_btnZoomOut;
		private TextBox ui_txtInfo;
		private Button ui_btnSettings;
		private Button ui_btnZoomFit;
		private SharpGL.OpenGLControl ui_gl;
	}
}
