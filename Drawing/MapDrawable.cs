namespace TransportDouala.Drawing;

/// <summary>Carte schématique dessinée en vectoriel : aucune image, aucune requête réseau.</summary>
public class MapDrawable : IDrawable
{
    public void Draw(ICanvas c, RectF r)
    {
        float W = r.Width, H = r.Height;
        PointF P(float x, float y) => new(W * x, H * y);

        c.FillColor = Color.FromArgb("#EAF0F4"); c.FillRectangle(r);
        c.FillColor = Color.FromArgb("#DCE7EA"); c.FillEllipse(-W * 0.12f, H * 0.80f, W * 0.34f, H * 0.5f);
        c.FillColor = Color.FromArgb("#E1E7EF");
        c.FillRoundedRectangle(W * 0.74f, H * 0.5f, W * 0.15f, H * 0.14f, 6);

        // rues
        c.StrokeColor = Colors.White; c.StrokeSize = 14; c.StrokeLineCap = LineCap.Butt;
        c.DrawLine(0, H * 0.40f, W, H * 0.40f);
        c.DrawLine(0, H * 0.74f, W, H * 0.74f);
        c.DrawLine(W * 0.12f, 0, W * 0.12f, H);
        c.DrawLine(W * 0.62f, 0, W * 0.62f, H);

        // lignes
        c.StrokeSize = 4; c.StrokeLineCap = LineCap.Round;
        c.StrokeColor = Color.FromArgb("#2A62B0");
        var bona = P(0.17f, 0.20f); var deido = P(0.5f, 0.42f); var akwa = P(0.82f, 0.76f);
        c.DrawLine(bona, deido); c.DrawLine(deido, akwa);
        c.StrokeColor = Color.FromArgb("#A9B4C7");
        var wouri = P(0.16f, 0.90f); var ndokoti = P(0.80f, 0.14f);
        c.DrawLine(wouri, deido); c.DrawLine(deido, ndokoti);

        // arrêts
        foreach (var p in new[] { bona, deido, akwa, ndokoti })
        {
            c.FillColor = Colors.White; c.FillCircle(p, 6);
            c.StrokeColor = Color.FromArgb("#17407A"); c.StrokeSize = 2.5f; c.DrawCircle(p, 6);
        }
        c.FontColor = Color.FromArgb("#111B2E"); c.FontSize = 12;
        c.DrawString("Bonamoussadi", bona.X + 10, bona.Y - 14, HorizontalAlignment.Left);
        c.DrawString("Deido", deido.X - 8, deido.Y + 22, HorizontalAlignment.Left);
        c.DrawString("Akwa", akwa.X - 12, akwa.Y + 20, HorizontalAlignment.Left);
        c.DrawString("Ndokoti", ndokoti.X - 52, ndokoti.Y - 22, HorizontalAlignment.Left);
        c.FontColor = Color.FromArgb("#6B7487");
        c.DrawString("Wouri", W * 0.04f, H * 0.90f, HorizontalAlignment.Left);

        // position simulée de l'utilisateur
        var me = P(0.30f, 0.62f);
        c.StrokeColor = Color.FromArgb("#17407A"); c.StrokeSize = 1.5f; c.DrawCircle(me, 18);
        c.FillColor = Color.FromArgb("#17407A"); c.FillCircle(me, 7);
    }
}
