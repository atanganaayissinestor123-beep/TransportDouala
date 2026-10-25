using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TransportDouala.Services;

namespace TransportDouala.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    readonly AuthService _auth;
    readonly AppInitializer _init;

    public LoginViewModel(AuthService auth, AppInitializer init) { _auth = auth; _init = init; }

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(LoginCommand))] private string email = "";
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(LoginCommand))] private string password = "";
    [ObservableProperty] private string emailError = "";
    [ObservableProperty, NotifyPropertyChangedFor(nameof(HasError))] private string errorMessage = "";

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public string ButtonText => IsBusy ? "Connexion..." : "Se connecter";

    partial void OnEmailChanged(string value) { EmailError = ""; ErrorMessage = ""; }
    partial void OnPasswordChanged(string value) => ErrorMessage = "";

    bool CanLogin() => !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrEmpty(Password);

    /// <summary>Au lancement : prépare la base puis rouvre la session précédente si elle existe.</summary>
    public async Task TryRestoreAsync()
    {
        try
        {
            await _init.EnsureAsync();
            if (_auth.TryRestoreSession()) await Shell.Current.GoToAsync("//main/home");
        }
        catch (Exception ex) { ErrorMessage = "Initialisation de la base impossible : " + ex.Message; }
    }

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task Login()
    {
        if (IsBusy) return;
        if (!Validators.IsEmail(Email)) { EmailError = "Adresse e-mail invalide."; return; }
        IsBusy = true; OnPropertyChanged(nameof(ButtonText));
        try
        {
            await _init.EnsureAsync();
            var ok = await _auth.LoginAsync(Email, Password);
            if (ok) { Email = ""; Password = ""; await Shell.Current.GoToAsync("//main/home"); }
            else ErrorMessage = "E-mail ou mot de passe incorrect.";
        }
        catch (Exception ex) { ErrorMessage = "Erreur : " + ex.Message; }
        finally { IsBusy = false; OnPropertyChanged(nameof(ButtonText)); }
    }

    [RelayCommand] private Task GoRegister() => Shell.Current.GoToAsync("register");

    [RelayCommand]
    private Task ForgotPassword() => Shell.Current.DisplayAlertAsync("Mot de passe oublié",
        "La réinitialisation sera disponible dans une prochaine version.", "OK");
}
