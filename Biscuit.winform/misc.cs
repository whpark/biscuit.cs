﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Biscuit.winform {
	public static class gMisc {

		public static string GetAppPath() {	// auto generated by copilot
			return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
		}

		public static bool LoadWindowPos(RegistryKey reg, Form form) {
			if (form is null)
				return false;
			string? str = reg.GetValue("Rect", "") as string;
			string[] strs = str?.Split(',');
            if (strs is not null && strs.Length >= 4) {
				form.Left	= Biscuit.misc.StrToInt(strs[0]) ?? 0;
				form.Top	= Biscuit.misc.StrToInt(strs[1]) ?? 0;
				form.Width	= Biscuit.misc.StrToInt(strs[2]) ?? 100;
				form.Height	= Biscuit.misc.StrToInt(strs[3]) ?? 100;
			}

			bool bMaximized = (int)reg.GetValue("Maximized", 0) != 0;
			form.WindowState = bMaximized ? FormWindowState.Maximized : FormWindowState.Normal;

			return true;
		}

		public static bool SaveWindowPos(RegistryKey reg, Form form) {
			if (form is null)
				return false;
			bool bMaximized = (form.WindowState == FormWindowState.Maximized);
			reg.SetValue("Maximized", bMaximized ? 1 : 0);
			if (!bMaximized) {
				string str = $"{form.Left},{form.Top},{form.Width},{form.Height}";
				reg.SetValue("Rect", str);
			}
			return true;
		}
	}
}
