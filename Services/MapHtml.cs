using System.Text.Json;
using System.Text.Json.Serialization;
using TransportDouala.Models;

namespace TransportDouala.Services;

public class MapUserDto { public double Lat { get; set; } public double Lon { get; set; } public double Accuracy { get; set; } }
public class MapStopDto
{
    public int I { get; set; }
    public string Name { get; set; } = "";
    public string Info { get; set; } = "";
    public double Lat { get; set; }
    public double Lon { get; set; }
}
public class MapPayload
{
    public MapUserDto? User { get; set; }
    public List<MapStopDto> Stops { get; set; } = new();
    public int Selected { get; set; }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(MapPayload))]
internal partial class MapJsonContext : JsonSerializerContext { }

/// <summary>Assemble la page de carte (MapLibre embarqué) et prépare les données envoyées au JavaScript.</summary>
public static class MapHtml
{
    // Style de carte OpenFreeMap : gratuit, sans clé d'API ni inscription (cartes vectorielles, données OpenStreetMap).
    // Autres styles du même service : "bright", "positron". Pour un autre fournisseur, changer cette ligne
    // (certains exigent une clé d'API).
    const string StyleUrl = "https://tiles.openfreemap.org/styles/liberty";
    // Origine https de la page de carte (adresse du dépôt du projet).
    public const string BaseUrl = "https://github.com/atanganaayissinestor123-beep/TransportDouala";
    static string? _html;

    static async Task<string> ReadAsync(string name)
    {
        using var s = await FileSystem.OpenAppPackageFileAsync(name);
        using var r = new StreamReader(s);
        return await r.ReadToEndAsync();
    }

    public static async Task<string> BuildAsync()
    {
        if (_html != null) return _html;
        var template = await ReadAsync("map/map.html");
        var css = await ReadAsync("map/maplibre-gl.css");
        var js = await ReadAsync("map/maplibre-gl.js");
        return _html = template.Replace("{{MAPLIBRE_CSS}}", css).Replace("{{MAPLIBRE_JS}}", js).Replace("{{STYLE_URL}}", StyleUrl);
    }

    public static string Payload(PositionResult? pos, IReadOnlyList<NearbyStop> stops, int selected)
    {
        var p = new MapPayload
        {
            User = pos is null ? null : new MapUserDto { Lat = pos.Point.Lat, Lon = pos.Point.Lon, Accuracy = pos.Accuracy ?? 0 },
            Selected = selected,
            Stops = stops.Select((s, i) => new MapStopDto { I = i, Name = s.Name, Info = s.Info, Lat = s.Lat, Lon = s.Lon }).ToList()
        };
        return JsonSerializer.Serialize(p, MapJsonContext.Default.MapPayload);
    }
}
