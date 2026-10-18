using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TransportDouala.Services;

namespace TransportDouala.ViewModels;

public partial class RegisterViewModel : BaseViewModel
{
    readonly AuthService _auth;
    public RegisterViewModel(AuthService auth) => _auth = auth;

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(RegisterCommand))] private string fullName = "";
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(RegisterCommand))] private string email = "";
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(RegisterCommand))] private string password = "";
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(RegisterCommand))] private string confirm = "";
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(RegisterCommand))] private bool accepted;

    [ObservableProperty] private string nameError = "";
    [ObservableProperty] private string emailError = "";
    [ObservableProperty] private string passwordError = "";
    [ObservableProperty] private string confirmError = "";

    public string ButtonText => IsBusy ? "Création..." : "Créer mon compte";

    partial void OnFullNameChanged(string value) => NameError = "";
    partial void OnEmailChanged(string value) => EmailError = "";
    partial void OnPasswordChanged(string value) => PasswordError = "";
    partial void OnConfirmChanged(string value) => ConfirmError = "";

    bool CanRegister() => !string.IsNullOrWhiteSpace(FullName) && !string.IsNullOrWhiteSpace(Email)
                          && !string.IsNullOrEmpty(Password) && !string.IsNullOrEmpty(Confirm) && Accepted;

    [RelayCommand(CanExecute = nameof(CanRegister))]
    private async Task Register()
    {
        if (IsBusy) return;
        NameError = FullName.Trim().Length < 2 ? "Entrez votre nom et prénom." : "";
        EmailError = Validators.IsEmail(Email) ? "" : "Adresse e-mail invalide.";
        PasswordError = Password.Length < 8 ? "8 caractères minimum." : "";
        ConfirmError = Password == Confirm ? "" : "Les mots de passe ne correspondent pas.";
        if (NameError + EmailError + PasswordError + ConfirmError != "") return;

        IsBusy = true; OnPropertyChanged(nameof(ButtonText));
        var ok = await _auth.RegisterAsync(FullName.Trim(), Email.Trim(), Password);
        IsBusy = false; OnPropertyChanged(nameof(ButtonText));
        if (!ok) { EmailError = "Un compte existe déjà avec cette adresse."; return; }
        await Shell.Current.GoToAsync("//main/home");
    }
}
