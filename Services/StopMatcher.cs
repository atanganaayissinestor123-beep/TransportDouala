using System.Globalization;
using System.Text;
using TransportDouala.Models;

namespace TransportDouala.Services;

public static class StopMatcher
{
    public static string Norm(string s)
    {
        var d = s.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in d)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
        return sb.ToString();
    }

    /// <summary>Index de l'arrêt (égalité exacte d'abord, puis "contient"), -1 sinon.</summary>
    public static int Find(IReadOnlyList<LineStop> stops, string query)
    {
        var n = Norm(query);
        if (n.Length == 0) return -1;
        for (int i = 0; i < stops.Count; i++) if (Norm(stops[i].Name) == n) return i;
        if (n.Length >= 3)
            for (int i = 0; i < stops.Count; i++) if (Norm(stops[i].Name).Contains(n)) return i;
        return -1;
    }
}
