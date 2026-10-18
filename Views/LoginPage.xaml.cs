using TransportDouala.ViewModels;

namespace TransportDouala.Views;

public partial class LoginPage : ContentPage
{
    
    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
