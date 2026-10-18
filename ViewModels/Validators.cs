using System.Text.RegularExpressions;

namespace TransportDouala.ViewModels;

public static class Validators
{
    static readonly Regex EmailRx = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    public static bool IsEmail(string s) => EmailRx.IsMatch(s.Trim());
}
