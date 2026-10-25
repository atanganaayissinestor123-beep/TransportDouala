using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TransportDouala.Models;
using TransportDouala.Services;

namespace TransportDouala.ViewModels;

public partial class ResultItemViewModel : ObservableObject
{
    readonly FavoritesService _fav;
    readonly Action<ResultItemViewModel>? _onChanged;

    public ResultItemViewModel(RouteOption option, FavoritesService fav, bool showRoute = false,
                               Action<ResultItemViewModel>? onChanged = null)
    {
        Option = option; _fav = fav; ShowRoute = showRoute; _onChanged = onChanged;
        isFavorite = fav.IsRoute(option);
    }

    public RouteOption Option { get; }
    public bool ShowRoute { get; }
    public string RouteLabel => $"{Option.FromLabel} → {Option.ToLabel}";
    public string LineTitle => $"{(Option.Line.IsTaxi ? "🚕" : "🚌")}  {Option.Line.Name}";
    public string DurationText => $"{Option.Minutes} min";
    public string StopsText => Option.WalkMeters > 0 ? $"{Option.StopCount} arrêts · départ à {Option.WalkMeters} m" : $"{Option.StopCount} arrêts";
    public string FareText => $"{Option.Fare} FCFA";
    public string StarGlyph => IsFavorite ? "★" : "☆";

    [ObservableProperty, NotifyPropertyChangedFor(nameof(StarGlyph))] private bool isFavorite;

    [RelayCommand]
    private void ToggleFavorite()
    {
        IsFavorite = _fav.ToggleRoute(Option);
        _onChanged?.Invoke(this);
    }

    [RelayCommand]
    private Task OpenLine() => Nav.OpenLine(Option.Line.Id, Option.FromStop, Option.ToStop);
}
