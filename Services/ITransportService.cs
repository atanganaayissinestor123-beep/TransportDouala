using TransportDouala.Models;

namespace TransportDouala.Services;

public interface ITransportService
{
    IReadOnlyList<BusLine> GetLines();
    BusLine? GetLine(int id);

    /// <summary>Suggestions d'arrêts (autocomplétion) pour un texte saisi.</summary>
    IReadOnlyList<string> SuggestStops(string query, int max = 8);
    /// <summary>Nom d'arrêt officiel correspondant au texte saisi, ou null.</summary>
    string? ResolveStop(string query);

    IReadOnlyList<NearbyStop> GetNearbyStops(GeoPoint position, int max = 6);
    Task<IReadOnlyList<RouteOption>> SearchRoutesAsync(string? from, string to, GeoPoint? position);

    IReadOnlyList<string> GetRecentDestinations();
    void AddRecentDestination(string name);

    Task<bool> AddFareReportAsync(FareReport report);
}
