using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TransportDouala.Services;

namespace TransportDouala.ViewModels;

public partial class SearchViewModel : BaseViewModel, IQueryAttributable
{
    readonly ITransportService _transport;
    public SearchViewModel(ITransportService transport)
    {
        _transport = transport;
        Recent = transport.RecentDestinations;
    }

    public IReadOnlyList<string> Recent { get; }

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SwapCommand))] private string departure = "";
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SearchCommand))] private string destination = "";
    [ObservableProperty, NotifyPropertyChangedFor(nameof(HasError))] private string errorMessage = "";
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    partial void OnDepartureChanged(string value) => ErrorMessage = "";
    partial void OnDestinationChanged(string value) => ErrorMessage = "";

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
            ErrorMessage = $"Destination introuvable : « {Destination.Trim()} ». Essayez Akwa, Deido, Ndokoti ou Bonamoussadi.";
            return;
        }
        var from = "";
        if (!string.IsNullOrWhiteSpace(Departure))
        {
            var f = _transport.ResolveStop(Departure);
            if (f is null) { ErrorMessage = $"Départ introuvable : « {Departure.Trim()} »."; return; }
            from = f;
        }
        await Shell.Current.GoToAsync("results", new Dictionary<string, object> { ["from"] = from, ["to"] = to });
    }

    [RelayCommand(CanExecute = nameof(CanSwap))]
    private void Swap() => (Departure, Destination) = (Destination, Departure);

    [RelayCommand] private void UseCurrentPosition() => Departure = "";
    [RelayCommand] private void PickRecent(string name) => Destination = name;
}
