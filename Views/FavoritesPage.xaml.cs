using TransportDouala.ViewModels;

namespace TransportDouala.Views;

public partial class FavoritesPage : ContentPage
{
    readonly FavoritesViewModel _vm;
    public FavoritesPage(FavoritesViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        _vm = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.Refresh();
    }
}
