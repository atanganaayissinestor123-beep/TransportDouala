using Microsoft.Extensions.Logging;
using TransportDouala.Data;
using TransportDouala.Services;
using TransportDouala.ViewModels;
using TransportDouala.Views;

namespace TransportDouala;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();   // polices système (Roboto) : aucune police embarquée

        // Données et services
        builder.Services.AddSingleton<AppDatabase>();
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<TransportService>();
        builder.Services.AddSingleton<ITransportService>(sp => sp.GetRequiredService<TransportService>());
        builder.Services.AddSingleton<FavoritesService>();
        builder.Services.AddSingleton<LocationService>();
        builder.Services.AddSingleton<AppInitializer>();

        // ViewModels + Pages
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<SearchViewModel>();
        builder.Services.AddTransient<ResultsViewModel>();
        builder.Services.AddTransient<LineDetailViewModel>();
        builder.Services.AddTransient<ReportViewModel>();
        builder.Services.AddTransient<FavoritesViewModel>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<SearchPage>();
        builder.Services.AddTransient<ResultsPage>();
        builder.Services.AddTransient<LineDetailPage>();
        builder.Services.AddTransient<ReportPage>();
        builder.Services.AddTransient<FavoritesPage>();

#if ANDROID
        // Supprime le soulignement Android : les champs sont encadrés par un Border
        Action<Android.Widget.EditText> clear = e =>
            e.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline", (h, v) => clear(h.PlatformView));
        Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("NoUnderline", (h, v) => clear(h.PlatformView));
        Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("NoUnderline", (h, v) => clear(h.PlatformView));
        Microsoft.Maui.Handlers.DatePickerHandler.Mapper.AppendToMapping("NoUnderline", (h, v) => clear(h.PlatformView));

        // La carte garde le geste : glisser un doigt dessus déplace la carte et ne fait pas défiler la page
        Microsoft.Maui.Handlers.WebViewHandler.Mapper.AppendToMapping("MapTouch", (h, v) =>
        {
            h.PlatformView.Touch += (s, e) =>
            {
                var action = e.Event?.Action;
                if (action == Android.Views.MotionEventActions.Down || action == Android.Views.MotionEventActions.Move)
                    h.PlatformView.Parent?.RequestDisallowInterceptTouchEvent(true);
                e.Handled = false;
            };
        });
#endif
#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}
