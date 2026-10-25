using SQLite;

namespace TransportDouala.Data;

/// <summary>Accès SQLite local (un seul fichier dans le dossier privé de l'application).</summary>
public class AppDatabase
{
    readonly object _gate = new();
    SQLiteConnection? _conn;

    public static string DbPath => Path.Combine(FileSystem.AppDataDirectory, "transportdouala.db3");

    public void Initialize()
    {
        lock (_gate)
        {
            if (_conn != null) return;
            var c = new SQLiteConnection(DbPath,
                SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex);
            c.CreateTable<UserEntity>();
            c.CreateTable<LineEntity>();
            c.CreateTable<LineStopEntity>();
            c.CreateTable<FareReportEntity>();
            c.CreateTable<FavoriteLineEntity>();
            c.CreateTable<FavoriteRouteEntity>();
            c.CreateTable<RecentSearchEntity>();
            _conn = c;
        }
    }

    public T Run<T>(Func<SQLiteConnection, T> action)
    {
        lock (_gate)
        {
            if (_conn == null) Initialize();
            return action(_conn!);
        }
    }

    public void Run(Action<SQLiteConnection> action) => Run<object?>(c => { action(c); return null; });
}
