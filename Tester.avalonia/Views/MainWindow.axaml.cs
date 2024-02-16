using Avalonia.Controls;
using CV = OpenCvSharp;

namespace Tester.avalonia.Views
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			CV.Mat img = CV.Mat.Zeros(800, 600, CV.MatType.CV_8UC3);
			var m = img.GetGenericIndexer<CV.Vec3b>();
			for (int y = 0; y < img.Rows; y++)
			{
				for (int x = 0; x < img.Cols; x++)
				{
					m[y, x] = new CV.Vec3b((byte)(x * 255 / img.Cols), (byte)(y * 255 / img.Rows), (byte)(y * 255 * 255 / img.Rows / img.Cols));
				}
			}

			ui_view.Image = img;

			Biscuit.xLazyProfile profile = new ();
			profile.Load("z:\\Downloads\\test.cfg");

			profile.Save("z:\\Downloads\\test2.cfg");
		}
	}
}