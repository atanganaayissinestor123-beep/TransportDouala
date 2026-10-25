using TransportDouala.ViewModels;

namespace TransportDouala.Views;

public partial class LoginPage : ContentPage
{
    readonly LoginViewModel _vm;

    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Task.Delay(100);              // laisse le Shell finir son initialisation
        await _vm.TryRestoreAsync();
    }
}
