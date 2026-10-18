using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TransportDouala.Services;

namespace TransportDouala.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    readonly AuthService _auth;
    public LoginViewModel(AuthService auth) => _auth = auth;

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(LoginCommand))] private string email = "";
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(LoginCommand))] private string password = "";
    [ObservableProperty] private string emailError = "";
    [ObservableProperty, NotifyPropertyChangedFor(nameof(HasError))] private string errorMessage = "";

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public string ButtonText => IsBusy ? "Connexion..." : "Se connecter";

    partial void OnEmailChanged(string value) { EmailError = ""; ErrorMessage = ""; }
    partial void OnPasswordChanged(string value) => ErrorMessage = "";

    bool CanLogin() => !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrEmpty(Password);

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task Login()
    {
        if (IsBusy) return;
        if (!Validators.IsEmail(Email)) { EmailError = "Adresse e-mail invalide."; return; }
        IsBusy = true; OnPropertyChanged(nameof(ButtonText));
        var ok = await _auth.LoginAsync(Email.Trim(), Password);
        IsBusy = false; OnPropertyChanged(nameof(ButtonText));
        if (ok) { Password = ""; await Shell.Current.GoToAsync("//main/home"); }
        else ErrorMessage = "E-mail ou mot de passe incorrect.";
    }

    [RelayCommand] private Task GoRegister() => Shell.Current.GoToAsync("register");

    [RelayCommand]
    private Task ForgotPassword() => Shell.Current.DisplayAlertAsync("Mot de passe oublié",
        "La réinitialisation sera disponible dans une prochaine version.", "OK");
}
