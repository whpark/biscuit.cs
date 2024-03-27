using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using CV = OpenCvSharp;

namespace Biscuit.winform {
	public partial class MatView_SettingsDlg : Form {

		public xMatView.sSettings m_settings = new();

		public MatView_SettingsDlg(xMatView.sSettings settings) {
			m_settings = settings;

			InitializeComponent();

			ui_cmbZoomIn.Items.AddRange(Enum.GetNames(typeof(xMatView.eZOOM_IN)));
			ui_cmbZoomOut.Items.AddRange(Enum.GetNames(typeof(xMatView.eZOOM_OUT)));

			SyncData(false, ref m_settings);
		}

		string GetColorName(CV.Vec3b cr) {
			return $"r{cr[0]}, g{cr[1]}, b{cr[2]}";
		}

		public void SyncData(bool bSave, ref xMatView.sSettings settings) {
			if (bSave) {
				settings.bZoomLock					= ui_chkZoomLock.Checked;
				settings.bPanningLock				= ui_chkPanningLock.Checked;
				settings.bExtendedPanning			= ui_chkExtendedPanning.Checked;
				settings.bKeyboardNavigation		= ui_chkKeyboardNavigation.Checked;
				settings.bDrawPixelValue			= ui_chkDrawPixelValue.Checked;
				settings.bPyrImageDown 				= ui_chkPyrImageDown.Checked;

				settings.dPanningSpeed				= (double)ui_spinImagePanningSpeed.Value;
				settings.nScrollMargin				= (int)ui_spinScrollMargin.Value;
				settings.tsScroll                   = (uint)ui_spinScrollDuration.Value;
				settings.eZoomIn					= (xMatView.eZOOM_IN)ui_cmbZoomIn.SelectedIndex;
				settings.eZoomOut					= (xMatView.eZOOM_OUT)ui_cmbZoomOut.SelectedIndex;
			}
			else {
				ui_chkZoomLock.Checked				= settings.bZoomLock;
				ui_chkPanningLock.Checked			= settings.bPanningLock;
				ui_chkExtendedPanning.Checked		= settings.bExtendedPanning;
				ui_chkKeyboardNavigation.Checked	= settings.bKeyboardNavigation;
				ui_chkDrawPixelValue.Checked		= settings.bDrawPixelValue;
				ui_chkPyrImageDown.Checked			= settings.bPyrImageDown;

				ui_spinImagePanningSpeed.Value		= (decimal)settings.dPanningSpeed;
				ui_spinScrollMargin.Value			= settings.nScrollMargin;
				ui_spinScrollDuration.Value			= settings.tsScroll;
				ui_cmbZoomIn.SelectedIndex			= (int)settings.eZoomIn;
				ui_cmbZoomOut.SelectedIndex			= (int)settings.eZoomOut;
				ui_txtBackgroundColor.Text          = GetColorName(settings.crBackground);
			}
		}

		private void ui_btnOK_Click(object sender, EventArgs e) {
			SyncData(true, ref m_settings);
			DialogResult = DialogResult.OK;
		}

		private void ui_btnCancel_Click(object sender, EventArgs e) {
			DialogResult = DialogResult.Cancel;
		}

		private void ui_btnBackgroundColor_Click(object sender, EventArgs e) {
			colorDialog1.Color = Color.FromArgb(255, m_settings.crBackground[0], m_settings.crBackground[1], m_settings.crBackground[2]);
			if (colorDialog1.ShowDialog() == DialogResult.OK) {
				m_settings.crBackground[0] = colorDialog1.Color.R;
				m_settings.crBackground[1] = colorDialog1.Color.G;
				m_settings.crBackground[2] = colorDialog1.Color.B;
				ui_txtBackgroundColor.Text = GetColorName(m_settings.crBackground);
			}
		}
	}
}
