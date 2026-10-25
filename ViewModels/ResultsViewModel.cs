using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using TransportDouala.Models;
using TransportDouala.Services;

namespace TransportDouala.ViewModels;

public partial class ResultsViewModel : BaseViewModel, IQueryAttributable
{
    readonly ITransportService _transport;
    readonly FavoritesService _fav;
    readonly LocationService _location;
    string _from = "", _to = "";
    bool _loaded;

    public ResultsViewModel(ITransportService transport, FavoritesService fav, LocationService location)
    { _transport = transport; _fav = fav; _location = location; }

    public ObservableCollection<ResultItemViewModel> Items { get; } = new();
    [ObservableProperty] private string summary = "";
    [ObservableProperty] private bool isEmpty;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        var from = query.TryGetValue("from", out var f) ? f?.ToString() ?? "" : "";
        var to = query.TryGetValue("to", out var t) ? t?.ToString() ?? "" : "";
        if (to.Length == 0 || (_loaded && from == _from && to == _to)) return;   // pas de rechargement au retour
        _from = from; _to = to; _loaded = true;
        Summary = $"{(string.IsNullOrWhiteSpace(from) ? "Ma position" : from)} → {to}";
        _ = LoadAsync();
    }

    async Task LoadAsync()
    {
        IsBusy = true; IsEmpty = false; Items.Clear();
        try
        {
            GeoPoint? pos = null;
            if (string.IsNullOrWhiteSpace(_from)) pos = (await _location.GetPositionAsync()).Point;
            var results = await _transport.SearchRoutesAsync(_from, _to, pos);
            foreach (var r in results) Items.Add(new ResultItemViewModel(r, _fav));
        }
        catch { /* l'état vide s'affiche */ }
        IsEmpty = Items.Count == 0;
        IsBusy = false;
    }
}
