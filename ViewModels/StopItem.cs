using TransportDouala.Services;

namespace TransportDouala.ViewModels;

/// <summary>Ligne de la frise des arrêts (Détail ligne).</summary>
public class StopItem
{
    readonly bool _first, _last, _endpoint, _dimmed;
    public StopItem(string name, bool first, bool last, bool endpoint, bool dimmed)
    { Name = name; _first = first; _last = last; _endpoint = endpoint; _dimmed = dimmed; }

    public string Name { get; }
    public Thickness LineMargin => _first ? new Thickness(0, 22, 0, 0) : _last ? new Thickness(0, 0, 0, 22) : new Thickness(0);
    public Color DotFill => _endpoint ? Palette.Primary : Palette.White;
    public Brush DotStroke => _dimmed ? Palette.MutedBrush : Palette.PrimaryBrush;
    public Color NameColor => _dimmed ? Palette.TextMuted : Palette.TextMain;
    public FontAttributes NameAttributes => _endpoint ? FontAttributes.Bold : FontAttributes.None;
}
