namespace TransportDouala.Services;

// Authentification SIMULÉE (en mémoire). Sera remplacée par SQLite + mot de passe haché.
public class AuthService
{
    readonly Dictionary<string, (string Name, string Password)> _users = new(StringComparer.OrdinalIgnoreCase)
    {
        ["demo@transportdouala.cm"] = ("Utilisateur Démo", "Douala2026"),
    };
    public string? CurrentUserName { get; private set; }

    public async Task<bool> LoginAsync(string email, string password)
    {
        await Task.Delay(600);
        if (_users.TryGetValue(email, out var u) && u.Password == password) { CurrentUserName = u.Name; return true; }
        return false;
    }

    public async Task<bool> RegisterAsync(string name, string email, string password)
    {
        await Task.Delay(600);
        if (_users.ContainsKey(email)) return false;
        _users[email] = (name, password);
        CurrentUserName = name;
        return true;
    }
}
