using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TransportDouala.Services;

namespace TransportDouala.ViewModels;

public partial class SearchViewModel : BaseViewModel, IQueryAttributable
{
    readonly ITransportService _transport;
    bool _suppress;

    public SearchViewModel(ITransportService transport) => _transport = transport;

    public ObservableCollection<string> DepartureSuggestions { get; } = new();
    public ObservableCollection<string> DestinationSuggestions { get; } = new();
    public ObservableCollection<string> Recent { get; } = new();

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SwapCommand))] private string departure = "";
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SearchCommand))] private string destination = "";
    [ObservableProperty, NotifyPropertyChangedFor(nameof(HasError))] private string errorMessage = "";
    [ObservableProperty] private bool hasDepartureSuggestions;
    [ObservableProperty] private bool hasDestinationSuggestions;
    [ObservableProperty] private bool hasRecent;
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public void Refresh()
    {
        Recent.Clear();
        foreach (var r in _transport.GetRecentDestinations()) Recent.Add(r);
        HasRecent = Recent.Count > 0;
    }

    partial void OnDepartureChanged(string value)
    {
        ErrorMessage = "";
        if (_suppress) return;
        HasDepartureSuggestions = Fill(DepartureSuggestions, value);
    }

    partial void OnDestinationChanged(string value)
    {
        ErrorMessage = "";
        if (_suppress) return;
        HasDestinationSuggestions = Fill(DestinationSuggestions, value);
    }

    bool Fill(ObservableCollection<string> target, string text)
    {
        target.Clear();
        foreach (var s in _transport.SuggestStops(text.Trim()))
            if (!string.Equals(s, text.Trim(), StringComparison.OrdinalIgnoreCase)) target.Add(s);
        return target.Count > 0;
    }

    void ClearSuggestions()
    {
        DepartureSuggestions.Clear(); DestinationSuggestions.Clear();
        HasDepartureSuggestions = false; HasDestinationSuggestions = false;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("destination", out var d) && !string.IsNullOrWhiteSpace(d?.ToString()))
            Destination = d!.ToString()!;
    }

    bool CanSearch() => !string.IsNullOrWhiteSpace(Destination);
    bool CanSwap() => !string.IsNullOrWhiteSpace(Departure);

    [RelayCommand(CanExecute = nameof(CanSearch))]
    private async Task Search()
    {
        var to = _transport.ResolveStop(Destination);
        if (to is null)
        {
            ErrorMessage = $"Destination introuvable : « {Destination.Trim()} ». Choisissez un arrêt dans les suggestions.";
            return;
        }
        var from = "";
        if (!string.IsNullOrWhiteSpace(Departure))
        {
            var f = _transport.ResolveStop(Departure);
            if (f is null) { ErrorMessage = $"Départ introuvable : « {Departure.Trim()} ». Choisissez un arrêt dans les suggestions."; return; }
            from = f;
        }
        ClearSuggestions();
        _transport.AddRecentDestination(to);
        await Shell.Current.GoToAsync("results", new Dictionary<string, object> { ["from"] = from, ["to"] = to });
    }

    [RelayCommand(CanExecute = nameof(CanSwap))]
    private void Swap()
    {
        _suppress = true;
        (Departure, Destination) = (Destination, Departure);
        _suppress = false;
        ClearSuggestions();
    }

    [RelayCommand]
    private void UseCurrentPosition()
    {
        _suppress = true; Departure = ""; _suppress = false;
        HasDepartureSuggestions = false; DepartureSuggestions.Clear();
    }

    [RelayCommand]
    private void PickDeparture(string name)
    {
        _suppress = true; Departure = name; _suppress = false;
        DepartureSuggestions.Clear(); HasDepartureSuggestions = false;
    }

    [RelayCommand]
    private void PickDestination(string name)
    {
        _suppress = true; Destination = name; _suppress = false;
        DestinationSuggestions.Clear(); HasDestinationSuggestions = false;
    }

    [RelayCommand]
    private void PickRecent(string name) => PickDestination(name);
}
