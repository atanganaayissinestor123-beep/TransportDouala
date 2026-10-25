using TransportDouala.Data;
using TransportDouala.Models;

namespace TransportDouala.Services;

/// <summary>Favoris persistants (par utilisateur) dans SQLite.</summary>
public class FavoritesService
{
    readonly AppDatabase _db;
    readonly ITransportService _transport;
    readonly AuthService _auth;

    public FavoritesService(AppDatabase db, ITransportService transport, AuthService auth)
    { _db = db; _transport = transport; _auth = auth; }

    int Uid => _auth.CurrentUserId;

    public IReadOnlyCollection<int> LineIds
    {
        get { var uid = Uid; return _db.Run(c => c.Table<FavoriteLineEntity>().Where(f => f.UserId == uid).ToList()).Select(f => f.LineId).ToList(); }
    }

    public bool IsLine(int id)
    {
        var uid = Uid;
        return _db.Run(c => c.Table<FavoriteLineEntity>().Where(f => f.UserId == uid && f.LineId == id).Count()) > 0;
    }

    public bool ToggleLine(int id)
    {
        var uid = Uid;
        return _db.Run(c =>
        {
            var existing = c.Table<FavoriteLineEntity>().Where(f => f.UserId == uid && f.LineId == id).FirstOrDefault();
            if (existing != null) { c.Delete(existing); return false; }
            c.Insert(new FavoriteLineEntity { UserId = uid, LineId = id });
            return true;
        });
    }

    public IReadOnlyList<RouteOption> Routes
    {
        get
        {
            var uid = Uid;
            var rows = _db.Run(c => c.Table<FavoriteRouteEntity>().Where(f => f.UserId == uid).ToList());
            var list = new List<RouteOption>();
            foreach (var r in rows)
                if (_transport.GetLine(r.LineId) is { } line)
                    list.Add(new RouteOption(line, r.FromLabel, r.ToLabel, r.FromStop, r.ToStop, r.Minutes, r.StopCount, line.AverageFare));
            return list;
        }
    }

    public bool IsRoute(RouteOption o)
    {
        var uid = Uid; var key = o.Key;
        return _db.Run(c => c.Table<FavoriteRouteEntity>().Where(f => f.UserId == uid && f.Key == key).Count()) > 0;
    }

    public bool ToggleRoute(RouteOption o)
    {
        var uid = Uid; var key = o.Key;
        return _db.Run(c =>
        {
            var existing = c.Table<FavoriteRouteEntity>().Where(f => f.UserId == uid && f.Key == key).FirstOrDefault();
            if (existing != null) { c.Delete(existing); return false; }
            c.Insert(new FavoriteRouteEntity
            {
                UserId = uid, Key = key, LineId = o.Line.Id, FromLabel = o.FromLabel, ToLabel = o.ToLabel,
                FromStop = o.FromStop, ToStop = o.ToStop, Minutes = o.Minutes, StopCount = o.StopCount
            });
            return true;
        });
    }
}
