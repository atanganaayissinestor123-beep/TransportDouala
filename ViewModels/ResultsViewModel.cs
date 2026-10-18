using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using TransportDouala.Services;

namespace TransportDouala.ViewModels;

public partial class ResultsViewModel : BaseViewModel, IQueryAttributable
{
    readonly ITransportService _transport;
    readonly FavoritesService _fav;
    string _from = "", _to = "";
    bool _loaded;

    public ResultsViewModel(ITransportService transport, FavoritesService fav) { _transport = transport; _fav = fav; }

    public ObservableCollection<ResultItemViewModel> Items { get; } = new();
    [ObservableProperty] private string summary = "";
    [ObservableProperty] private bool isEmpty;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        var from = query.TryGetValue("from", out var f) ? f?.ToString() ?? "" : "";
        var to = query.TryGetValue("to", out var t) ? t?.ToString() ?? "" : "";
        if (to.Length == 0 || (_loaded && from == _from && to == _to)) return; // évite de recharger au retour
        _from = from; _to = to; _loaded = true;
        Summary = $"{(string.IsNullOrWhiteSpace(from) ? "Ma position" : from)} → {to}";
        _ = LoadAsync();
    }

    async Task LoadAsync()
    {
        IsBusy = true; IsEmpty = false; Items.Clear();
        var results = await _transport.SearchRoutesAsync(_from, _to);
        foreach (var r in results) Items.Add(new ResultItemViewModel(r, _fav));
        IsEmpty = Items.Count == 0;
        IsBusy = false;
    }
}
