using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TransportDouala.Models;
using TransportDouala.Services;

namespace TransportDouala.ViewModels;

public partial class HomeViewModel : BaseViewModel
{
    readonly ITransportService _transport;
    readonly LocationService _location;
    readonly AuthService _auth;
    GeoPoint? _last;

    public HomeViewModel(ITransportService transport, LocationService location, AuthService auth)
    { _transport = transport; _location = location; _auth = auth; }

    public ObservableCollection<NearbyStop> NearbyStops { get; } = new();
    public ObservableCollection<NearbyLine> SelectedLines { get; } = new();

    /// <summary>Position, arrêts proches et index de l'arrêt sélectionné : pour redessiner la carte.</summary>
    public event Action<PositionResult?, IReadOnlyList<NearbyStop>, int>? MapChanged;
    /// <summary>Un arrêt a été sélectionné (liste ou carte) : pour le mettre en évidence sur la carte.</summary>
    public event Action<int>? SelectionChanged;
    /// <summary>L'utilisateur a touché sa propre position (carte ou pastille).</summary>
    public event Action? MeSelected;
    /// <summary>Mode « placer ma position » activé ou désactivé (pour la carte).</summary>
    public event Action<bool>? PickModeChanged;

    [ObservableProperty] private string query = "";
    [ObservableProperty] private string locationNote = "Localisation en cours...";
    [ObservableProperty] private bool showNoNearby;
    [ObservableProperty] private bool hasSelectedStop;
    [ObservableProperty] private string selectedName = "";
    [ObservableProperty] private string selectedInfo = "";
    [ObservableProperty] private int selectedIndex = -1;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(PickButtonText))] private bool isPicking;
    [ObservableProperty] private bool isManualPosition;
    public string PickButtonText => IsPicking ? "Touchez la carte pour placer votre position" : "📍 Corriger ma position";
    [ObservableProperty] private bool isMeSelected;
    [ObservableProperty] private string positionText = "";
    [ObservableProperty] private string nearestText = "";

    public async Task StartAsync()
    {
        await RefreshAsync(false);
        _location.PositionChanged -= OnPositionChanged;
        _location.PositionChanged += OnPositionChanged;
        if (_location.Current is { IsReal: true }) await _location.StartTrackingAsync();
    }

    public void Stop()
    {
        _location.PositionChanged -= OnPositionChanged;
        _location.StopTracking();
    }

    public async Task RefreshAsync(bool force)
    {
        IsBusy = true;
        var pos = await _location.GetPositionAsync(force);
        Apply(pos);
        IsBusy = false;
    }

    void OnPositionChanged(object? sender, PositionResult pos) =>
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (_last is null || Geo.Distance(_last, pos.Point) > 40) Apply(pos);   // évite de redessiner pour de petits mouvements
        });

    void Apply(PositionResult pos)
    {
        _last = pos.Point;
        var previous = HasSelectedStop ? SelectedName : null;
        var keepMe = IsMeSelected;
        var near = _transport.GetNearbyStops(pos.Point);
        NearbyStops.Clear();
        foreach (var n in near) NearbyStops.Add(n);
        ShowNoNearby = near.Count == 0;
        LocationNote = pos.Note;
        IsManualPosition = _location.IsManual;

        PositionText = $"{pos.Point.Lat:F5}, {pos.Point.Lon:F5}";
        NearestText = near.Count == 0 ? "Aucun arrêt à moins de 2 km."
                                      : $"Arrêt le plus proche : {near[0].Name} ({near[0].Info.Split('·')[0].Trim()})";

        int idx = previous is null ? 0 : near.ToList().FindIndex(s => s.Name == previous);
        SetSelection(near.Count == 0 ? -1 : Math.Max(0, idx));
        if (keepMe) { SetSelection(-1); IsMeSelected = true; }
        MapChanged?.Invoke(pos, near, SelectedIndex);
    }

    void SetSelection(int index)
    {
        SelectedLines.Clear();
        IsMeSelected = false;
        if (index < 0 || index >= NearbyStops.Count)
        {
            SelectedIndex = -1; HasSelectedStop = false; SelectedName = ""; SelectedInfo = "";
            return;
        }
        var s = NearbyStops[index];
        SelectedIndex = index; HasSelectedStop = true; SelectedName = s.Name; SelectedInfo = s.Info;
        foreach (var l in s.Lines) SelectedLines.Add(l);
    }

    /// <summary>Sélectionne un arrêt depuis la liste ou la carte.</summary>
    public void SelectStop(int index)
    {
        if (index < 0 || index >= NearbyStops.Count) return;
        SetSelection(index);
        SelectionChanged?.Invoke(index);
    }

    /// <summary>Sélectionne la position de l'utilisateur (fiche « Votre position »).</summary>
    public void SelectMe()
    {
        SetSelection(-1);
        IsMeSelected = true;
        MeSelected?.Invoke();
    }

    [RelayCommand]
    private void TogglePick()
    {
        IsPicking = !IsPicking;
        PickModeChanged?.Invoke(IsPicking);
    }

    /// <summary>Fixe la position à la main (toucher sur la carte ou « Je suis à cet arrêt »).</summary>
    public void SetManualPosition(double lat, double lon)
    {
        IsPicking = false;
        PickModeChanged?.Invoke(false);
        Apply(_location.SetManual(new GeoPoint(lat, lon)));
    }

    [RelayCommand]
    private void UseSelectedStopAsPosition()
    {
        if (SelectedIndex < 0 || SelectedIndex >= NearbyStops.Count) return;
        var s = NearbyStops[SelectedIndex];
        SetManualPosition(s.Lat, s.Lon);
    }

    [RelayCommand]
    private async Task UseAutoPosition()
    {
        _location.ClearManual();
        IsPicking = false;
        PickModeChanged?.Invoke(false);
        await RefreshAsync(true);
        if (_location.Current is { IsReal: true }) await _location.StartTrackingAsync();
    }

    [RelayCommand]
    private Task SearchFromHere() => Shell.Current.GoToAsync("//main/search");

    [RelayCommand]
    private Task Search() => Shell.Current.GoToAsync("//main/search",
        new Dictionary<string, object> { ["destination"] = Query.Trim() });

    [RelayCommand]
    private Task OpenLine(int lineId) => Nav.OpenLine(lineId);

    [RelayCommand]
    private Task GoToSelectedStop() => Shell.Current.GoToAsync("//main/search",
        new Dictionary<string, object> { ["destination"] = SelectedName });

    [RelayCommand]
    private async Task Logout()
    {
        Stop();
        _auth.Logout();
        await Shell.Current.GoToAsync("//login");
    }
}
