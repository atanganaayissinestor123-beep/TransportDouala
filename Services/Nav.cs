namespace TransportDouala.Services;

public static class Nav
{
    public static Task OpenLine(int lineId, string? fromStop = null, string? toStop = null) =>
        Shell.Current.GoToAsync("line", new Dictionary<string, object>
        {
            ["lineId"] = lineId, ["from"] = fromStop ?? "", ["to"] = toStop ?? ""
        });
}
