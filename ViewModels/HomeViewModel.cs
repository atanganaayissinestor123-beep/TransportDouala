using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TransportDouala.Models;
using TransportDouala.Services;

namespace TransportDouala.ViewModels;

public partial class HomeViewModel : BaseViewModel
{
    public HomeViewModel(ITransportService transport)
    {
        NearbyStops = transport.GetNearbyStops();
    }

    public IReadOnlyList<NearbyStop> NearbyStops { get; }
    public bool HasNearby => NearbyStops.Count > 0;
    public bool HasNoNearby => !HasNearby;

    [ObservableProperty] private string query = "";

    [RelayCommand]
    private Task Search() => Shell.Current.GoToAsync("//main/search",
        new Dictionary<string, object> { ["destination"] = Query.Trim() });

    [RelayCommand]
    private Task OpenStop(int lineId) => Nav.OpenLine(lineId);
}
