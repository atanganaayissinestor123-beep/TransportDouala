using TransportDouala.ViewModels;

namespace TransportDouala.Views;

public partial class ReportPage : ContentPage
{
    
    public ReportPage(ReportViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
