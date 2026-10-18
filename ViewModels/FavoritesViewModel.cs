using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using TransportDouala.Services;

namespace TransportDouala.ViewModels;

public class FavoriteLineItem
{
    public FavoriteLineItem(int id, string name, string direction, string fare, Action<FavoriteLineItem> onRemove)
    {
        Name = name; Direction = direction; FareText = $"Tarif moyen : {fare}";
        OpenCommand = new AsyncRelayCommand(() => Nav.OpenLine(id));
        RemoveCommand = new RelayCommand(() => onRemove(this));
        Id = id;
    }
    public int Id { get; }
    public string Name { get; }
    public string Direction { get; }
    public string FareText { get; }
    public ICommand OpenCommand { get; }
    public ICommand RemoveCommand { get; }
}

public partial class FavoritesViewModel : BaseViewModel
{
    readonly ITransportService _transport;
    readonly FavoritesService _fav;

    public FavoritesViewModel(ITransportService transport, FavoritesService fav) { _transport = transport; _fav = fav; }

    public ObservableCollection<FavoriteLineItem> Lines { get; } = new();
    public ObservableCollection<ResultItemViewModel> Routes { get; } = new();

    [ObservableProperty] private bool hasLines;
    [ObservableProperty] private bool hasRoutes;
    [ObservableProperty] private bool isEmpty;

    public void Refresh()
    {
        Lines.Clear(); Routes.Clear();
        foreach (var id in _fav.LineIds.OrderBy(i => i))
            if (_transport.GetLine(id) is { } l)
                Lines.Add(new FavoriteLineItem(l.Id, l.Name, $"{l.From} → {l.To}", $"{l.AverageFare} FCFA", RemoveLine));
        foreach (var r in _fav.Routes)
            Routes.Add(new ResultItemViewModel(r, _fav, showRoute: true, onChanged: OnRouteChanged));
        UpdateFlags();
    }

    void RemoveLine(FavoriteLineItem item) { _fav.ToggleLine(item.Id); Lines.Remove(item); UpdateFlags(); }
    void OnRouteChanged(ResultItemViewModel item) { if (!item.IsFavorite) Routes.Remove(item); UpdateFlags(); }

    void UpdateFlags()
    {
        HasLines = Lines.Count > 0; HasRoutes = Routes.Count > 0; IsEmpty = !HasLines && !HasRoutes;
    }
}
