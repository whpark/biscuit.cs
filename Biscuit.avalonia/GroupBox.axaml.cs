using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace Biscuit.avalonia
{

	public partial class xGroupBox : HeaderedContentControl
	{
		//public static readonly StyledProperty<IControlTemplate> HeaderProperty
		//	= AvaloniaProperty.Register<GroupBox, IControlTemplate>(nameof(Header));

		//public object Header { get; set; }
		//public object Content { get; set; }
		public xGroupBox()
		{
			InitializeComponent();
		}
	}

}	// Biscuit.avalonia
