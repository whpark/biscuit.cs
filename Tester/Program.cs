using System;
using Biscuit;

public class Program
{
	public static void Main()
	{
		TestProfile();
	}

	public static void TestProfile()
	{
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
}