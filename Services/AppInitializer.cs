using TransportDouala.Data;

namespace TransportDouala.Services;

/// <summary>Ouvre la base, importe les données au premier lancement, charge le cache. Exécuté une seule fois.</summary>
public class AppInitializer
{
    readonly AppDatabase _db;
    readonly TransportService _transport;
    Task? _task;

    public AppInitializer(AppDatabase db, TransportService transport) { _db = db; _transport = transport; }

    public Task EnsureAsync() => _task ??= RunAsync();

    async Task RunAsync()
    {
        try
        {
            await Task.Run(_db.Initialize);
            await DataImporter.ImportIfNeededAsync(_db);
            await _transport.LoadAsync();
        }
        catch { _task = null; throw; }   // permet de réessayer
    }
}
