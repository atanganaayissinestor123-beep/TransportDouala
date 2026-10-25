using TransportDouala.Models;

namespace TransportDouala.Drawing;

/// <summary>Carte schématique centrée sur l'utilisateur : arrêts proches projetés en mètres. Aucune image, aucun réseau.</summary>
public class MapDrawable : IDrawable
{
    public GeoPoint? User { get; set; }
    public IReadOnlyList<NearbyStop> Stops { get; set; } = Array.Empty<NearbyStop>();

    public void Draw(ICanvas c, RectF r)
    {
        float W = r.Width, H = r.Height;
        c.FillColor = Color.FromArgb("#EAF0F4"); c.FillRectangle(r);

        // quadrillage discret (repère visuel, pas de vraies rues)
        c.StrokeColor = Colors.White; c.StrokeSize = 10;
        for (int k = 1; k <= 3; k++)
        {
            c.DrawLine(W * k / 4, 0, W * k / 4, H);
            c.DrawLine(0, H * k / 4, W, H * k / 4);
        }

        if (User is null)
        {
            c.FontColor = Color.FromArgb("#6B7487"); c.FontSize = 14;
            c.DrawString("Localisation en cours...", 0, 0, W, H, HorizontalAlignment.Center, VerticalAlignment.Center);
            return;
        }

        double lat0 = User.Lat, lon0 = User.Lon;
        double kx = 111320 * Math.Cos(lat0 * Math.PI / 180), ky = 110540;
        var pts = Stops.Select(s => (s, x: (s.Lon - lon0) * kx, y: (s.Lat - lat0) * ky)).ToList();
        double maxX = Math.Max(150, pts.Select(p => Math.Abs(p.x)).DefaultIfEmpty(0).Max());
        double maxY = Math.Max(150, pts.Select(p => Math.Abs(p.y)).DefaultIfEmpty(0).Max());
        const float pad = 36;
        double scale = Math.Min((W / 2 - pad) / maxX, (H / 2 - pad) / maxY);
        var me = new PointF(W / 2, H / 2);

        c.FontSize = 12;
        foreach (var (s, x, y) in pts)
        {
            var p = new PointF((float)(W / 2 + x * scale), (float)(H / 2 - y * scale));
            c.StrokeColor = Color.FromArgb("#A9B4C7"); c.StrokeSize = 2; c.StrokeDashPattern = new float[] { 4, 4 };
            c.DrawLine(me, p); c.StrokeDashPattern = null;
            c.FillColor = Colors.White; c.FillCircle(p, 6);
            c.StrokeColor = Color.FromArgb("#17407A"); c.StrokeSize = 2.5f; c.DrawCircle(p, 6);

            var name = s.Name.Length > 22 ? s.Name[..21] + "…" : s.Name;
            c.FontColor = Color.FromArgb("#111B2E");
            if (p.X + 10 + 150 > W) c.DrawString(name, p.X - 160, p.Y - 8, 150, 16, HorizontalAlignment.Right, VerticalAlignment.Center);
            else c.DrawString(name, p.X + 10, p.Y - 8, 150, 16, HorizontalAlignment.Left, VerticalAlignment.Center);
        }

        c.StrokeColor = Color.FromArgb("#17407A"); c.StrokeSize = 1.5f; c.DrawCircle(me, 18);
        c.FillColor = Color.FromArgb("#17407A"); c.FillCircle(me, 7);
    }
}
