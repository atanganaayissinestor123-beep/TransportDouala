using TransportDouala.Models;

namespace TransportDouala.Services;

public static class Geo
{
    public static double Distance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000;
        double p1 = lat1 * Math.PI / 180, p2 = lat2 * Math.PI / 180;
        double dp = p2 - p1, dl = (lon2 - lon1) * Math.PI / 180;
        double h = Math.Sin(dp / 2) * Math.Sin(dp / 2) + Math.Cos(p1) * Math.Cos(p2) * Math.Sin(dl / 2) * Math.Sin(dl / 2);
        return 2 * R * Math.Asin(Math.Sqrt(h));
    }
    public static double Distance(GeoPoint a, GeoPoint b) => Distance(a.Lat, a.Lon, b.Lat, b.Lon);
}
