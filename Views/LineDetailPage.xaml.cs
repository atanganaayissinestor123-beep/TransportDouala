using TransportDouala.ViewModels;

namespace TransportDouala.Views;

public partial class LineDetailPage : ContentPage
{
    
    public LineDetailPage(LineDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
