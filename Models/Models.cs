namespace TransportDouala.Models;

public record LineStop(string Name, int Minutes);

public record BusLine(int Id, int AverageFare, IReadOnlyList<LineStop> Stops)
{
    public string Name => $"Ligne {Id}";
    public string From => Stops[0].Name;
    public string To => Stops[^1].Name;
}

public record RouteOption(BusLine Line, string FromLabel, string ToLabel, string FromStop, string ToStop,
                          int Minutes, int StopCount, int Fare);

public record NearbyStop(string Name, string Info, int LineId);

public record FareReport(int LineId, string Direction, int Fare, DateTime Date, string Comment);
