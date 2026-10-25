using TransportDouala.Drawing;
using TransportDouala.Models;
using TransportDouala.Services;
using TransportDouala.ViewModels;

namespace TransportDouala.Views;

public partial class HomePage : ContentPage
{
    readonly HomeViewModel _vm;
    readonly MapDrawable _fallback = new();      // carte schématique hors connexion
    bool _webReady, _webLoaded;
    string? _pendingPayload;

    public HomePage(HomeViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
        OfflineMap.Drawable = _fallback;

        _vm.MapChanged += async (user, stops, selected) =>
        {
            _fallback.User = user?.Point; _fallback.Stops = stops; OfflineMap.Invalidate();
            _pendingPayload = MapHtml.Payload(user, stops, selected);
            await PushAsync();
        };
        _vm.PickModeChanged += async on =>
        {
            if (_webReady) await WebMap.EvaluateJavaScriptAsync($"setPickMode({(on ? "true" : "false")})");
        };
        _vm.MeSelected += async () =>
        {
            if (_webReady) await WebMap.EvaluateJavaScriptAsync("selectMe(true)");
        };
        _vm.SelectionChanged += async index =>
        {
            if (_webReady) await WebMap.EvaluateJavaScriptAsync($"selectStop({index}, true)");
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        bool online = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
        WebMap.IsVisible = online; OfflineMap.IsVisible = !online;
        if (online && !_webLoaded)
        {
            _webLoaded = true;
            WebMap.Source = new HtmlWebViewSource { Html = await MapHtml.BuildAsync(), BaseUrl = MapHtml.BaseUrl };
        }
        await _vm.StartAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.Stop();
    }

    async Task PushAsync()
    {
        if (_webReady && _pendingPayload != null) await WebMap.EvaluateJavaScriptAsync($"setData({_pendingPayload})");
    }

    async void OnWebNavigated(object? sender, WebNavigatedEventArgs e)
    {
        if (e.Result != WebNavigationResult.Success) return;
        _webReady = true;
        await PushAsync();
    }

    // Les clics sur la carte arrivent sous la forme tdapp://stop/N
    void OnWebNavigating(object? sender, WebNavigatingEventArgs e)
    {
        const string prefix = "tdapp://stop/";
        if (!e.Url.StartsWith("tdapp://")) return;
        e.Cancel = true;
        if (e.Url.StartsWith("tdapp://me")) { _vm.SelectMe(); return; }
        if (e.Url.StartsWith("tdapp://pos/"))
        {
            var parts = e.Url["tdapp://pos/".Length..].Split('/');
            if (parts.Length >= 2
                && double.TryParse(parts[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lat)
                && double.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lon))
                _vm.SetManualPosition(lat, lon);
            return;
        }
        if (e.Url.StartsWith("tdapp://nowebgl"))        // WebGL indisponible : on bascule sur le schéma
        {
            _webReady = false; WebMap.IsVisible = false; OfflineMap.IsVisible = true;
            return;
        }
        if (e.Url.StartsWith(prefix) && int.TryParse(e.Url[prefix.Length..].TrimEnd('/'), out var i)) _vm.SelectStop(i);
    }

    void OnStopTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject { BindingContext: NearbyStop s })
            _vm.SelectStop(_vm.NearbyStops.IndexOf(s));
    }

    void OnLineTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject { BindingContext: NearbyLine l }) _vm.OpenLineCommand.Execute(l.Id);
    }

    void OnMeChipTapped(object? sender, TappedEventArgs e) => _vm.SelectMe();

    async void OnLocateTapped(object? sender, TappedEventArgs e)
    {
        if (!_vm.IsManualPosition) await _vm.RefreshAsync(true);   // position choisie à la main : on recentre seulement
        if (_webReady) await WebMap.EvaluateJavaScriptAsync("recenter()");
    }
}
