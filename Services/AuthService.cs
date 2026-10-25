using System.Security.Cryptography;
using TransportDouala.Data;

namespace TransportDouala.Services;

/// <summary>Comptes locaux dans SQLite. Mot de passe haché (PBKDF2-SHA256 + sel), jamais stocké en clair.</summary>
public class AuthService
{
    const string SessionKey = "user_id";
    const int Iterations = 100_000;
    readonly AppDatabase _db;

    public AuthService(AppDatabase db) => _db = db;

    public int CurrentUserId { get; private set; }
    public string CurrentUserName { get; private set; } = "";
    public bool IsLoggedIn => CurrentUserId > 0;

    static string Hash(string password, byte[] salt) =>
        Convert.ToBase64String(Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, 32));

    public Task<bool> LoginAsync(string email, string password) => Task.Run(() =>
    {
        var e = email.Trim().ToLowerInvariant();
        var u = _db.Run(c => c.Table<UserEntity>().Where(x => x.Email == e).FirstOrDefault());
        if (u is null) return false;
        var computed = Hash(password, Convert.FromBase64String(u.Salt));
        if (!CryptographicOperations.FixedTimeEquals(
                System.Text.Encoding.UTF8.GetBytes(computed), System.Text.Encoding.UTF8.GetBytes(u.PasswordHash)))
            return false;
        SetSession(u);
        return true;
    });

    public Task<bool> RegisterAsync(string name, string email, string password) => Task.Run(() =>
    {
        var e = email.Trim().ToLowerInvariant();
        if (_db.Run(c => c.Table<UserEntity>().Where(x => x.Email == e).Count()) > 0) return false;
        var salt = RandomNumberGenerator.GetBytes(16);
        var u = new UserEntity
        {
            FullName = name.Trim(), Email = e, Salt = Convert.ToBase64String(salt),
            PasswordHash = Hash(password, salt), CreatedAt = DateTime.UtcNow
        };
        _db.Run(c => c.Insert(u));          // renseigne u.Id
        SetSession(u);
        return true;
    });

    public bool TryRestoreSession()
    {
        var id = Preferences.Default.Get(SessionKey, 0);
        if (id <= 0) return false;
        var u = _db.Run(c => c.Table<UserEntity>().Where(x => x.Id == id).FirstOrDefault());
        if (u is null) { Logout(); return false; }
        CurrentUserId = u.Id; CurrentUserName = u.FullName;
        return true;
    }

    public void Logout()
    {
        CurrentUserId = 0; CurrentUserName = "";
        Preferences.Default.Remove(SessionKey);
    }

    void SetSession(UserEntity u)
    {
        CurrentUserId = u.Id; CurrentUserName = u.FullName;
        Preferences.Default.Set(SessionKey, u.Id);
    }
}
