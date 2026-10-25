namespace TransportDouala.Models;

public record GeoPoint(double Lat, double Lon);

public record LineStop(string Name, double Lat, double Lon, int Minutes);

/// <summary>Une ligne de bus ou de taxi (issue du GTFS WhereIsMyTransport).</summary>
public sealed class BusLine
{
    public BusLine(int id, string type, string code, string routeName, int baseFare, IReadOnlyList<LineStop> stops)
    { Id = id; Type = type; Code = code; RouteName = routeName; BaseFare = baseFare; Stops = stops; AverageFare = baseFare; }

    public int Id { get; }
    public string Type { get; }              // "bus" ou "taxi"
    public string Code { get; }              // ex : "Taxi 5"
    public string RouteName { get; }         // ex : "Ndokoti → Dalip Akwa"
    public int BaseFare { get; }             // tarif du jeu de données
    public int AverageFare { get; set; }     // tarif moyen recalculé avec les signalements
    public IReadOnlyList<LineStop> Stops { get; }
    public string Name => Code;
    public string From => Stops[0].Name;
    public string To => Stops[^1].Name;
    public bool IsTaxi => Type == "taxi";
}

public record RouteOption(BusLine Line, string FromLabel, string ToLabel, string FromStop, string ToStop,
                          int Minutes, int StopCount, int Fare, int WalkMeters = 0)
{
    public string Key => $"{Line.Id}|{FromLabel}|{ToLabel}|{FromStop}|{ToStop}";
}

public record NearbyLine(int Id, string Code, string RouteName, string FareText);

public record NearbyStop(string Name, string Info, int LineId, double Lat, double Lon, IReadOnlyList<NearbyLine> Lines);

public record FareReport(int LineId, string Direction, int Fare, DateTime Date, string Comment);

public record PositionResult(GeoPoint Point, bool IsReal, string Note, double? Accuracy = null);
