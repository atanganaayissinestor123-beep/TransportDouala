using Microsoft.Extensions.Logging;
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

        // Services (en mémoire pour l'instant ; SQLite viendra remplacer les implémentations)
        builder.Services.AddSingleton<ITransportService, MockTransportService>();
        builder.Services.AddSingleton<FavoritesService>();
        builder.Services.AddSingleton<AuthService>();

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
#endif
#if DEBUG
        builder.Logging.AddDebug();
 #endif
        return builder.Build();
    }
}
