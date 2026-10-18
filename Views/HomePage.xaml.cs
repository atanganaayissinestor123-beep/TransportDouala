using TransportDouala.ViewModels;

namespace TransportDouala.Views;

public partial class HomePage : ContentPage
{
    readonly HomeViewModel _vm;
    public HomePage(HomeViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        _vm = vm;
    }

    void OnStopTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject { BindingContext: TransportDouala.Models.NearbyStop s })
            _vm.OpenStopCommand.Execute(s.LineId);
    }

    void OnLocateTapped(object? sender, TappedEventArgs e) => MapView.Invalidate();
}
