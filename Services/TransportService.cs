using TransportDouala.Data;
using TransportDouala.Models;

namespace TransportDouala.Services;

/// <summary>Lignes, arrêts, recherche d'itinéraire, arrêts proches et tarifs moyens. Données chargées de SQLite en mémoire.</summary>
public class TransportService : ITransportService
{
    const int PriorWeight = 2;           // poids du tarif du jeu de données face aux signalements
    const double StopRadius = 250;       // tolérance (m) pour rattacher un lieu à un arrêt d'une ligne
    const double WalkRadius = 1000;      // distance max (m) entre la position et le premier arrêt

    sealed class Cluster { public string Name = ""; public double Lat, Lon; public HashSet<int> LineIds = new(); }

    readonly AppDatabase _db;
    readonly AuthService _auth;
    List<BusLine> _lines = new();
    Dictionary<int, BusLine> _byId = new();
    Dictionary<string, string> _canonical = new();            // nom normalisé -> nom affiché
    Dictionary<string, int> _nameCount = new();               // popularité d'un nom
    Dictionary<string, List<GeoPoint>> _points = new();       // nom normalisé -> coordonnées
    List<Cluster> _clusters = new();

    public TransportService(AppDatabase db, AuthService auth) { _db = db; _auth = auth; }

    // ---------- Chargement ----------
    public Task LoadAsync() => Task.Run(() =>
    {
        var (lineRows, stopRows) = _db.Run(c => (
            c.Table<LineEntity>().ToList(),
            c.Table<LineStopEntity>().OrderBy(s => s.LineId).ThenBy(s => s.Seq).ToList()));

        var stopsBy = stopRows.GroupBy(s => s.LineId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<LineStop>)g.Select(s => new LineStop(s.Name, s.Lat, s.Lon, s.Minutes)).ToList());

        var lines = lineRows.Where(l => stopsBy.ContainsKey(l.Id))
            .OrderBy(l => l.Type == "bus" ? 0 : 1).ThenBy(l => l.Id)
            .Select(l => new BusLine(l.Id, l.Type, l.Code, l.Name, l.BaseFare, stopsBy[l.Id])).ToList();

        var canonical = new Dictionary<string, string>();
        var count = new Dictionary<string, int>();
        var points = new Dictionary<string, List<GeoPoint>>();
        var seen = new HashSet<string>();
        var clusters = new List<Cluster>();

        foreach (var l in lines)
            foreach (var s in l.Stops)
            {
                var n = StopMatcher.Norm(s.Name);
                canonical.TryAdd(n, s.Name);
                count[n] = count.GetValueOrDefault(n) + 1;
                if (seen.Add($"{n}|{s.Lat:F5}|{s.Lon:F5}"))
                {
                    if (!points.TryGetValue(n, out var list)) points[n] = list = new();
                    list.Add(new GeoPoint(s.Lat, s.Lon));
                }
                // regroupe les arrêts situés à moins de 80 m (même lieu, lignes différentes)
                Cluster? cl = null;
                foreach (var c in clusters)
                    if (Math.Abs(c.Lat - s.Lat) < 0.0008 && Math.Abs(c.Lon - s.Lon) < 0.0008 && Geo.Distance(c.Lat, c.Lon, s.Lat, s.Lon) < 80)
                    { cl = c; break; }
                if (cl is null) clusters.Add(cl = new Cluster { Name = s.Name, Lat = s.Lat, Lon = s.Lon });
                cl.LineIds.Add(l.Id);
            }

        _lines = lines; _byId = lines.ToDictionary(l => l.Id);
        _canonical = canonical; _nameCount = count; _points = points; _clusters = clusters;
        RefreshFares();
    });

    // ---------- Lignes ----------
    public IReadOnlyList<BusLine> GetLines() => _lines;
    public BusLine? GetLine(int id) => _byId.GetValueOrDefault(id);

    // ---------- Autocomplétion ----------
    public IReadOnlyList<string> SuggestStops(string query, int max = 8)
    {
        var q = StopMatcher.Norm(query);
        if (q.Length < 2) return Array.Empty<string>();
        return _canonical
            .Select(kv => new { kv.Key, kv.Value, Score = kv.Key == q ? 0 : kv.Key.StartsWith(q) ? 1 : kv.Key.Contains(" " + q) ? 2 : kv.Key.Contains(q) ? 3 : 9 })
            .Where(x => x.Score < 9)
            .OrderBy(x => x.Score).ThenByDescending(x => _nameCount.GetValueOrDefault(x.Key)).ThenBy(x => x.Value.Length)
            .Take(max).Select(x => x.Value).ToList();
    }

    public string? ResolveStop(string query)
    {
        var q = StopMatcher.Norm(query);
        if (q.Length == 0) return null;
        if (_canonical.TryGetValue(q, out var exact)) return exact;
        return q.Length >= 3 ? SuggestStops(query, 1).FirstOrDefault() : null;
    }

    // ---------- Arrêts à proximité ----------
    public IReadOnlyList<NearbyStop> GetNearbyStops(GeoPoint p, int max = 6)
    {
        string Fmt(double m) => m < 1000 ? $"{Math.Round(m / 10) * 10:0} m" : $"{m / 1000:0.0} km";
        return _clusters
            .Select(c => (c, d: Geo.Distance(p.Lat, p.Lon, c.Lat, c.Lon)))
            .Where(x => x.d <= 2000).OrderBy(x => x.d).Take(max)
            .Select(x => new NearbyStop(x.c.Name,
                $"{Fmt(x.d)} · {x.c.LineIds.Count} ligne{(x.c.LineIds.Count > 1 ? "s" : "")}",
                x.c.LineIds.Min(), x.c.Lat, x.c.Lon,
                x.c.LineIds.OrderBy(id => id).Take(8).Where(id => _byId.ContainsKey(id))
                    .Select(id => new NearbyLine(id, _byId[id].Name, _byId[id].RouteName, $"{_byId[id].AverageFare} FCFA"))
                    .ToList()))
            .ToList();
    }

    // ---------- Recherche d'itinéraire ----------
    static int Nearest(BusLine line, IReadOnlyList<GeoPoint> targets, double radius, out double dist)
    {
        int best = -1; dist = double.MaxValue;
        for (int i = 0; i < line.Stops.Count; i++)
            foreach (var t in targets)
            {
                var d = Geo.Distance(line.Stops[i].Lat, line.Stops[i].Lon, t.Lat, t.Lon);
                if (d < dist) { dist = d; best = i; }
            }
        return dist <= radius ? best : -1;
    }

    public Task<IReadOnlyList<RouteOption>> SearchRoutesAsync(string? from, string to, GeoPoint? position) => Task.Run(() =>
    {
        IReadOnlyList<RouteOption> none = Array.Empty<RouteOption>();
        var toName = ResolveStop(to);
        if (toName is null || !_points.TryGetValue(StopMatcher.Norm(toName), out var toPts)) return none;

        bool fromPosition = string.IsNullOrWhiteSpace(from);
        IReadOnlyList<GeoPoint> fromPts; string fromLabel; double fromRadius;
        if (fromPosition)
        {
            if (position is null) return none;
            fromPts = new[] { position }; fromLabel = "Ma position"; fromRadius = WalkRadius;
        }
        else
        {
            var fromName = ResolveStop(from!);
            if (fromName is null || !_points.TryGetValue(StopMatcher.Norm(fromName), out var fp)) return none;
            fromPts = fp; fromLabel = fromName; fromRadius = StopRadius;
        }

        var result = new List<(double score, RouteOption opt)>();
        foreach (var l in _lines)
        {
            int i = Nearest(l, fromPts, fromRadius, out var dFrom);
            int j = Nearest(l, toPts, StopRadius, out _);
            if (i < 0 || j < 0 || i == j) continue;
            if (i > j && !l.IsTaxi) continue;                  // bus : sens unique ; taxi : sens inverse accepté
            int minutes = Math.Max(1, Math.Abs(l.Stops[j].Minutes - l.Stops[i].Minutes));
            int walk = fromPosition ? (int)Math.Round(dFrom / 10) * 10 : 0;
            result.Add((minutes + walk / 80.0, new RouteOption(l, fromLabel, toName, l.Stops[i].Name, l.Stops[j].Name,
                minutes, Math.Abs(i - j) + 1, l.AverageFare, walk)));
        }
        return (IReadOnlyList<RouteOption>)result.OrderBy(r => r.score).Take(15).Select(r => r.opt).ToList();
    });

    // ---------- Destinations récentes (par utilisateur) ----------
    public IReadOnlyList<string> GetRecentDestinations()
    {
        var uid = _auth.CurrentUserId;
        return _db.Run(c => c.Table<RecentSearchEntity>().Where(r => r.UserId == uid).OrderByDescending(r => r.SearchedAt).ToList())
            .Select(r => r.Name).Distinct().Take(5).ToList();
    }

    public void AddRecentDestination(string name)
    {
        var uid = _auth.CurrentUserId;
        _db.Run(c =>
        {
            c.Execute("DELETE FROM recent_searches WHERE UserId = ? AND Name = ?", uid, name);
            c.Insert(new RecentSearchEntity { UserId = uid, Name = name, SearchedAt = DateTime.UtcNow });
            c.Execute("DELETE FROM recent_searches WHERE UserId = ? AND Id NOT IN " +
                      "(SELECT Id FROM recent_searches WHERE UserId = ? ORDER BY SearchedAt DESC LIMIT 20)", uid, uid);
        });
    }

    // ---------- Signalements et tarif moyen ----------
    public Task<bool> AddFareReportAsync(FareReport r) => Task.Run(() =>
    {
        try
        {
            var uid = _auth.CurrentUserId;
            _db.Run(c => c.Insert(new FareReportEntity
            {
                UserId = uid, LineId = r.LineId, Direction = r.Direction, Fare = r.Fare,
                Date = r.Date, Comment = r.Comment, CreatedAt = DateTime.UtcNow
            }));
            RefreshFares();
            return true;
        }
        catch { return false; }
    });

    /// <summary>Tarif moyen = moyenne pondérée (tarif du jeu de données x2 + signalements des 12 derniers mois), arrondie à 5 FCFA.</summary>
    void RefreshFares()
    {
        var cutoff = DateTime.Today.AddDays(-365);
        var reports = _db.Run(c => c.Table<FareReportEntity>().Where(r => r.Date >= cutoff).ToList());
        var agg = reports.GroupBy(r => r.LineId).ToDictionary(g => g.Key, g => new { Count = g.Count(), Sum = g.Sum(r => r.Fare) });
        foreach (var l in _lines)
        {
            double avg = agg.TryGetValue(l.Id, out var a)
                ? (l.BaseFare * PriorWeight + a.Sum) / (double)(PriorWeight + a.Count)
                : l.BaseFare;
            l.AverageFare = (int)(Math.Round(avg / 5.0) * 5);
        }
    }
}
