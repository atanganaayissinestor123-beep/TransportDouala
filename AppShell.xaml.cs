using TransportDouala.Views;

namespace TransportDouala;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        // Pages "poussées" (empilées au-dessus des onglets)
        Routing.RegisterRoute("register", typeof(RegisterPage));
        Routing.RegisterRoute("results", typeof(ResultsPage));
        Routing.RegisterRoute("line", typeof(LineDetailPage));
    }
}
