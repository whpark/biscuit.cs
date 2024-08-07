﻿using System;
using Biscuit;

public class Program {
	public static void Main() {
		TestProfile();
		TestStrToInt();
	}

	public static void TestProfile() {
		//xLazyProfile profile = new xLazyProfile();
		//var section = profile["section"];
		//section.SetItemValue("key", "value");
		//profile.DeleteSectionsIf((key, section) => key == "section");

		//section = profile["section1"];
		//section.SetItemValue("key1", "value1");

		//profile.Save("Test.ini");

		xLazyProfile profile = new();
		profile.Load("Test.cfg");

		profile["Test"].SetItemValue("LogSetup", false);
		profile.Save("Test2.cfg");
	}

	public static void TestStrToInt() {
		if (misc.StrToIntBase("0x123X", 0) != 0x123)
			throw new Exception();
		if (misc.StrToIntBase("0b1001", 0) != 9)
			throw new Exception();
		if (misc.StrToIntBase("1234", 0) != 1234)
			throw new Exception();
		if (misc.StrToUIntBase("0x123X", 0) != 0x123)
			throw new Exception();
		if (misc.StrToUIntBase("0b1001", 0) != 9)
			throw new Exception();
		if (misc.StrToUIntBase("1234", 0) != 1234)
			throw new Exception();
	}
}