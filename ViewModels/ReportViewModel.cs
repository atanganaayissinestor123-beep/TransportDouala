using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TransportDouala.Models;
using TransportDouala.Services;

namespace TransportDouala.ViewModels;

public partial class ReportViewModel : BaseViewModel, IQueryAttributable
{
    readonly ITransportService _transport;

    public ReportViewModel(ITransportService transport)
    {
        _transport = transport;
        LineNames = transport.GetLines().Select(l => l.Name).ToList();
        SelectedLineName = LineNames.FirstOrDefault();
    }

    public List<string> LineNames { get; }
    public DateTime Today => DateTime.Today;

    [ObservableProperty] private string? selectedLineName;
    [ObservableProperty] private List<string> directions = new();
    [ObservableProperty] private string? selectedDirection;
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SubmitCommand))] private string fareText = "";
    [ObservableProperty] private DateTime reportDate = DateTime.Today;
    [ObservableProperty] private string comment = "";
    [ObservableProperty] private string fareError = "";
    [ObservableProperty, NotifyPropertyChangedFor(nameof(HasSuccess))] private string successMessage = "";
    [ObservableProperty, NotifyPropertyChangedFor(nameof(HasFailure))] private string failureMessage = "";

    public bool HasSuccess => !string.IsNullOrEmpty(SuccessMessage);
    public bool HasFailure => !string.IsNullOrEmpty(FailureMessage);
    public string ButtonText => IsBusy ? "Envoi..." : "Envoyer le signalement";

    BusLine? CurrentLine() =>
        SelectedLineName is { Length: > 6 } n && int.TryParse(n[6..], out var id) ? _transport.GetLine(id) : null;

    partial void OnSelectedLineNameChanged(string? value)
    {
        var line = CurrentLine();
        Directions = line is null ? new() : new() { $"{line.From} → {line.To}", $"{line.To} → {line.From}" };
        SelectedDirection = Directions.FirstOrDefault();
    }

    partial void OnFareTextChanged(string value) { FareError = ""; SuccessMessage = ""; FailureMessage = ""; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!query.TryGetValue("lineId", out var v)) return;
        SelectedLineName = $"Ligne {Convert.ToInt32(v)}";
        if (query.TryGetValue("direction", out var d) && d?.ToString() is { } dir && Directions.Contains(dir))
            SelectedDirection = dir;
        SuccessMessage = ""; FailureMessage = "";
    }

    bool CanSubmit() => !string.IsNullOrWhiteSpace(FareText);

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private async Task Submit()
    {
        if (IsBusy) return;
        SuccessMessage = ""; FailureMessage = "";
        if (!int.TryParse(FareText.Trim(), out var fare) || fare < 50 || fare > 2000)
        { FareError = "Entrez un tarif valide (entre 50 et 2000 FCFA)."; return; }
        if (ReportDate.Date > DateTime.Today)
        { FailureMessage = "La date ne peut pas être dans le futur."; return; }
        var line = CurrentLine();
        if (line is null || SelectedDirection is null) { FailureMessage = "Choisissez une ligne et un trajet."; return; }

        IsBusy = true; OnPropertyChanged(nameof(ButtonText));
        var ok = await _transport.AddFareReportAsync(new FareReport(line.Id, SelectedDirection, fare, ReportDate, Comment.Trim()));
        IsBusy = false; OnPropertyChanged(nameof(ButtonText));

        if (ok)
        {
            FareText = ""; Comment = ""; ReportDate = DateTime.Today;
            SuccessMessage = "Merci ! Votre signalement a été enregistré.";
        }
        else FailureMessage = "L'envoi a échoué. Réessayez.";
    }
}
