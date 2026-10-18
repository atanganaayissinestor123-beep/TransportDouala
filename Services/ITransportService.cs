using TransportDouala.Models;

namespace TransportDouala.Services;

/// <summary>Contrat d'accès aux données. Implémentation actuelle : simulée. Future : SQLite.</summary>
public interface ITransportService
{
    IReadOnlyList<BusLine> GetLines();
    BusLine? GetLine(int id);
    IReadOnlyList<string> RecentDestinations { get; }
    IReadOnlyList<NearbyStop> GetNearbyStops();
    string? ResolveStop(string query);
    Task<IReadOnlyList<RouteOption>> SearchRoutesAsync(string? from, string to);
    Task<bool> AddFareReportAsync(FareReport report);
}
