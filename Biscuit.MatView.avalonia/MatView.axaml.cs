using System.Drawing;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;

using CV = OpenCvSharp;

namespace Biscuit.MatView.avalonia;

public partial class xMatView : UserControl
{
    public CV.Mat Image {
        get => ui_renderer.Image;
        set => ui_renderer.Image = value;
    }

    public xMatView()
    {
		InitializeComponent();
    }

}