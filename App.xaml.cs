namespace TransportDouala;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        UserAppTheme = AppTheme.Light; // thème unique : sobriété et cohérence
    }

    protected override Window CreateWindow(IActivationState? activationState) => new Window(new AppShell());
}
