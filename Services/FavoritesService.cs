using TransportDouala.Models;

namespace TransportDouala.Services;

public class FavoritesService
{
    readonly HashSet<int> _lines = new() { 5, 12 };
    readonly List<RouteOption> _routes = new();

    public FavoritesService(ITransportService transport)
    {
        var l5 = transport.GetLine(5)!;
        _routes.Add(new RouteOption(l5, "Ma position", "Akwa", "Ndokoti", "Akwa", 25, 2, 300));
    }

    public IReadOnlyCollection<int> LineIds => _lines;
    public IReadOnlyList<RouteOption> Routes => _routes;

    public bool IsLine(int id) => _lines.Contains(id);
    public bool ToggleLine(int id) { if (!_lines.Remove(id)) { _lines.Add(id); return true; } return false; }

    public bool IsRoute(RouteOption o) => _routes.Contains(o);
    public bool ToggleRoute(RouteOption o) { if (!_routes.Remove(o)) { _routes.Add(o); return true; } return false; }
}
