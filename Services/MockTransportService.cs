using TransportDouala.Models;

namespace TransportDouala.Services;

// Données ILLUSTRATIVES (à remplacer par la base locale puis des données vérifiées).
public class MockTransportService : ITransportService
{
    const string DemoPosition = "Ndokoti"; // position simulée de l'utilisateur

    readonly List<BusLine> _lines = new()
    {
        new(5, 300, new LineStop[] { new("Akwa", 0), new("Ndokoti", 25), new("Makèpè", 35), new("Bonamoussadi", 45) }),
        new(8, 250, new LineStop[] { new("Bonamoussadi", 0), new("Makèpè", 10), new("Ndokoti", 20), new("Deido", 32) }),
        new(12, 300, new LineStop[] { new("Deido", 0), new("Ndokoti", 10), new("Bali", 25), new("Akwa Nord", 36), new("Akwa", 42) }),
    };
    readonly List<FareReport> _reports = new();

    public IReadOnlyList<string> RecentDestinations { get; } = new[] { "Akwa", "Bonamoussadi", "Deido" };

    public IReadOnlyList<BusLine> GetLines() => _lines;
    public BusLine? GetLine(int id) => _lines.FirstOrDefault(l => l.Id == id);

    public IReadOnlyList<NearbyStop> GetNearbyStops() => new NearbyStop[]
    {
        new("Carrefour Bonamoussadi", "350 m · 3 lignes", 5),
        new("Rond-point Deido", "650 m · 5 lignes", 12),
    };

    public string? ResolveStop(string query)
    {
        var n = StopMatcher.Norm(query);
        if (n.Length == 0) return null;
        foreach (var l in _lines)
            foreach (var s in l.Stops) if (StopMatcher.Norm(s.Name) == n) return s.Name;
        if (n.Length >= 3)
            foreach (var l in _lines)
                foreach (var s in l.Stops) if (StopMatcher.Norm(s.Name).Contains(n)) return s.Name;
        return null;
    }

    public async Task<IReadOnlyList<RouteOption>> SearchRoutesAsync(string? from, string to)
    {
        await Task.Delay(500); // simule le chargement
        bool fromPosition = string.IsNullOrWhiteSpace(from);
        var fromQuery = fromPosition ? DemoPosition : from!;
        var result = new List<RouteOption>();
        foreach (var l in _lines)
        {
            int i = StopMatcher.Find(l.Stops, fromQuery), j = StopMatcher.Find(l.Stops, to);
            if (i < 0 || j < 0 || i == j) continue;
            result.Add(new RouteOption(l,
                fromPosition ? "Ma position" : l.Stops[i].Name, l.Stops[j].Name,
                l.Stops[i].Name, l.Stops[j].Name,
                Math.Abs(l.Stops[j].Minutes - l.Stops[i].Minutes), Math.Abs(i - j) + 1, l.AverageFare));
        }
        return result.OrderBy(r => r.Minutes).ToList();
    }

    public async Task<bool> AddFareReportAsync(FareReport report)
    {
        await Task.Delay(600);
        _reports.Add(report);
        return true;
    }
}
