using TransportDouala.ViewModels;

namespace TransportDouala.Views;

public partial class ResultsPage : ContentPage
{
    
    public ResultsPage(ResultsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
