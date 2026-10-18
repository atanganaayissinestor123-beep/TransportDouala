using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TransportDouala.Services;

namespace TransportDouala.ViewModels;

public partial class LineDetailViewModel : BaseViewModel, IQueryAttributable
{
    readonly ITransportService _transport;
    readonly FavoritesService _fav;
    int _lineId;

    public LineDetailViewModel(ITransportService transport, FavoritesService fav) { _transport = transport; _fav = fav; }

    [ObservableProperty] private string title = "";
    [ObservableProperty] private string subtitle = "";
    [ObservableProperty] private string fareText = "";
    [ObservableProperty] private string stopsHeader = "";
    [ObservableProperty] private string directionText = "";
    [ObservableProperty] private List<StopItem> stops = new();
    [ObservableProperty] private bool isNotFound;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(StarGlyph))] private bool isFavorite;
    public string StarGlyph => IsFavorite ? "★" : "☆";
    public bool IsFound => !IsNotFound;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!query.TryGetValue("lineId", out var v)) return;
        var from = query.TryGetValue("from", out var f) ? f?.ToString() ?? "" : "";
        var to = query.TryGetValue("to", out var t) ? t?.ToString() ?? "" : "";
        Load(Convert.ToInt32(v), from, to);
    }

    void Load(int id, string from, string to)
    {
        var line = _transport.GetLine(id);
        IsNotFound = line is null;
        OnPropertyChanged(nameof(IsFound));
        if (line is null) return;
        _lineId = id;

        var list = line.Stops.ToList();
        int i = from.Length > 0 ? StopMatcher.Find(list, from) : -1;
        int j = to.Length > 0 ? StopMatcher.Find(list, to) : -1;
        bool trip = i >= 0 && j >= 0 && i != j;
        if (trip && i > j) { list.Reverse(); i = list.Count - 1 - i; j = list.Count - 1 - j; } // sens de la recherche
        int lo = Math.Min(i, j), hi = Math.Max(i, j), n = list.Count;

        Stops = list.Select((s, k) => new StopItem(s.Name, k == 0, k == n - 1,
            endpoint: trip ? (k == i || k == j) : (k == 0 || k == n - 1),
            dimmed: trip && (k < lo || k > hi))).ToList();

        Title = line.Name;
        Subtitle = $"{list[0].Name} → {list[^1].Name}";
        DirectionText = $"Sens : {list[0].Name} vers {list[^1].Name}";
        StopsHeader = $"{n} arrêts au total";
        FareText = $"{line.AverageFare} FCFA";
        IsFavorite = _fav.IsLine(id);
    }

    [RelayCommand] private void ToggleFavorite() => IsFavorite = _fav.ToggleLine(_lineId);

    [RelayCommand]
    private Task ReportFare() => Shell.Current.GoToAsync("//main/report",
        new Dictionary<string, object> { ["lineId"] = _lineId, ["direction"] = Subtitle });
}
