using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;
using TransportDouala.Models;

namespace TransportDouala.Services;

/// <summary>Position réelle (permission Android) avec repli sur une position de démonstration à Douala.</summary>
public class LocationService
{
    // Carrefour Ndokoti (données GTFS) : position de démonstration
    public static readonly GeoPoint Demo = new(4.041668, 9.742386);
    const double MaxDistanceFromDouala = 40_000;

    PositionResult? _cache;
    DateTime _cacheAt;
    bool _tracking;
    GeoPoint? _manual;     // position choisie à la main sur la carte (prioritaire sur le GPS)

    public bool IsManual => _manual != null;

    public PositionResult? Current => _cache;
    public event EventHandler<PositionResult>? PositionChanged;

    static PositionResult Fallback(string note) => new(Demo, false, note);

    PositionResult ManualResult() => new(_manual!, false, "Position choisie manuellement sur la carte.");

    /// <summary>Fixe la position à la main (utile sur PC sans GPS, ou si le GPS est imprécis).</summary>
    public PositionResult SetManual(GeoPoint p)
    {
        _manual = p;
        StopTracking();
        var r = ManualResult();
        Store(r);
        return r;
    }

    /// <summary>Revient à la position automatique (GPS / réseau).</summary>
    public void ClearManual() { _manual = null; _cache = null; }

    public async Task<PositionResult> GetPositionAsync(bool force = false)
    {
        if (_manual != null) return ManualResult();
        if (!force && _cache != null && DateTime.UtcNow - _cacheAt < TimeSpan.FromSeconds(60)) return _cache;
        var r = await AcquireAsync();
        Store(r);
        return r;
    }

    void Store(PositionResult r) { _cache = r; _cacheAt = DateTime.UtcNow; }

    async Task<PositionResult> AcquireAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
                return Fallback("Permission de localisation refusée : position de démonstration (Ndokoti).");

            var loc = await Geolocation.Default.GetLocationAsync(
                new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(15)))
                ?? await Geolocation.Default.GetLastKnownLocationAsync();
            if (loc is null) return Fallback("Position indisponible : position de démonstration (Ndokoti).");
            return FromLocation(loc.Latitude, loc.Longitude, loc.Accuracy);
        }
        catch (FeatureNotEnabledException) { return Fallback("Localisation désactivée sur l'appareil : position de démonstration (Ndokoti)."); }
        catch (Exception) { return Fallback("Localisation impossible : position de démonstration (Ndokoti)."); }
    }

    static PositionResult FromLocation(double lat, double lon, double? accuracy)
    {
        var p = new GeoPoint(lat, lon);
        if (Geo.Distance(p, Demo) > MaxDistanceFromDouala)
            return Fallback("Vous êtes hors de Douala : position de démonstration (Ndokoti).");
        var note = accuracy switch
        {
            null => "Position réelle, mise à jour en direct.",
            > 300 => $"Position approximative (précision ± {Math.Round(accuracy.Value / 10) * 10:0} m, réseau Wi-Fi ou IP). " +
                     "Un téléphone avec GPS est plus précis ; vous pouvez aussi corriger la position sur la carte.",
            _ => $"Position réelle (précision ± {Math.Round(accuracy.Value):0} m), mise à jour en direct."
        };
        return new PositionResult(p, true, note, accuracy);
    }

    // ---- Suivi en direct (uniquement quand l'écran Accueil est affiché, pour économiser la batterie) ----
    public async Task StartTrackingAsync()
    {
        if (_tracking) return;
        try
        {
            var ok = await Geolocation.Default.StartListeningForegroundAsync(
                new GeolocationListeningRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10)));
            if (!ok) return;
            Geolocation.Default.LocationChanged += OnLocationChanged;
            _tracking = true;
        }
        catch { /* le suivi est facultatif */ }
    }

    public void StopTracking()
    {
        if (!_tracking) return;
        Geolocation.Default.LocationChanged -= OnLocationChanged;
        try { Geolocation.Default.StopListeningForeground(); } catch { }
        _tracking = false;
    }

    void OnLocationChanged(object? sender, GeolocationLocationChangedEventArgs e)
    {
        if (_manual != null) return;
        var r = FromLocation(e.Location.Latitude, e.Location.Longitude, e.Location.Accuracy);
        Store(r);
        PositionChanged?.Invoke(this, r);
    }
}
