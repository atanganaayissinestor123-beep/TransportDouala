using System.Text.Json;
using System.Text.Json.Serialization;

namespace TransportDouala.Data;

public class TransportJson
{
    public string Source { get; set; } = "";
    public List<JsonLine> Lines { get; set; } = new();
}
public class JsonLine
{
    public string Id { get; set; } = "";
    public string Type { get; set; } = "";
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public int Fare { get; set; }
    public List<JsonStop> Stops { get; set; } = new();
}
public class JsonStop
{
    [JsonPropertyName("n")] public string Name { get; set; } = "";
    [JsonPropertyName("lat")] public double Lat { get; set; }
    [JsonPropertyName("lon")] public double Lon { get; set; }
    [JsonPropertyName("m")] public int Minutes { get; set; }
}

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(TransportJson))]
internal partial class AppJsonContext : JsonSerializerContext { }

/// <summary>Importe transport_data.json dans SQLite au premier lancement (ou quand DataVersion change).</summary>
public static class DataImporter
{
    const int DataVersion = 1;   // à incrémenter si transport_data.json est mis à jour
    const string PrefKey = "data_version";

    public static async Task ImportIfNeededAsync(AppDatabase db)
    {
        var hasData = db.Run(c => c.Table<LineEntity>().Count()) > 0;
        if (hasData && Preferences.Default.Get(PrefKey, 0) == DataVersion) return;

        using var stream = await FileSystem.OpenAppPackageFileAsync("transport_data.json");
        var data = await JsonSerializer.DeserializeAsync(stream, AppJsonContext.Default.TransportJson)
                   ?? throw new InvalidOperationException("transport_data.json est vide ou invalide.");

        db.Run(c => c.RunInTransaction(() =>
        {
            c.DeleteAll<LineStopEntity>();
            c.DeleteAll<LineEntity>();
            var stops = new List<LineStopEntity>();
            foreach (var l in data.Lines)
            {
                // "b0007" -> 10007 ; "t0005" -> 20005 (identifiants stables entre deux imports)
                int id = (l.Id[0] == 'b' ? 10000 : 20000) + int.Parse(l.Id[1..]);
                c.Insert(new LineEntity { Id = id, Type = l.Type, Code = l.Code, Name = l.Name, BaseFare = l.Fare });
                for (int i = 0; i < l.Stops.Count; i++)
                {
                    var s = l.Stops[i];
                    stops.Add(new LineStopEntity { LineId = id, Seq = i, Name = s.Name, Lat = s.Lat, Lon = s.Lon, Minutes = s.Minutes });
                }
            }
            c.InsertAll(stops);
        }));
        Preferences.Default.Set(PrefKey, DataVersion);
    }
}
