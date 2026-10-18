using System.Windows.Input;
using TransportDouala.Services;

namespace TransportDouala.Controls;

/// <summary>Champ de formulaire réutilisable : libellé, saisie, message d'erreur, mot de passe, suffixe ou action.</summary>
public partial class FormField : ContentView
{
    bool _hidden;

    public FormField() { InitializeComponent(); }

    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(FormField), "",
        propertyChanged: (b, o, n) => ((FormField)b).TitleLabel.Text = (string)n);
    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(FormField), "",
        propertyChanged: (b, o, n) => ((FormField)b).Input.Placeholder = (string)n);
    public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(FormField), "",
        BindingMode.TwoWay, propertyChanged: (b, o, n) =>
        {
            var f = (FormField)b;
            if (f.Input.Text != (string)n) f.Input.Text = (string)n;
        });
    public static readonly BindableProperty ErrorTextProperty = BindableProperty.Create(nameof(ErrorText), typeof(string), typeof(FormField), "",
        propertyChanged: (b, o, n) =>
        {
            var f = (FormField)b; var s = (string?)n ?? "";
            f.ErrorLabel.Text = s; f.ErrorLabel.IsVisible = s.Length > 0;
            f.FieldBorder.Stroke = s.Length > 0 ? Palette.DangerBrush : Palette.BorderBrush;
        });
    public static readonly BindableProperty IsPasswordProperty = BindableProperty.Create(nameof(IsPassword), typeof(bool), typeof(FormField), false,
        propertyChanged: (b, o, n) => { var f = (FormField)b; f._hidden = (bool)n; f.Input.IsPassword = (bool)n; f.UpdateTrail(); });
    public static readonly BindableProperty SuffixProperty = BindableProperty.Create(nameof(Suffix), typeof(string), typeof(FormField), "",
        propertyChanged: (b, o, n) => ((FormField)b).UpdateTrail());
    public static readonly BindableProperty ActionTextProperty = BindableProperty.Create(nameof(ActionText), typeof(string), typeof(FormField), "",
        propertyChanged: (b, o, n) => ((FormField)b).UpdateTrail());
    public static readonly BindableProperty ActionCommandProperty = BindableProperty.Create(nameof(ActionCommand), typeof(ICommand), typeof(FormField), null);
    public static readonly BindableProperty InputKeyboardProperty = BindableProperty.Create(nameof(InputKeyboard), typeof(Keyboard), typeof(FormField), Keyboard.Default,
        propertyChanged: (b, o, n) => ((FormField)b).Input.Keyboard = (Keyboard)n);

    public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public string Placeholder { get => (string)GetValue(PlaceholderProperty); set => SetValue(PlaceholderProperty, value); }
    public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
    public string ErrorText { get => (string)GetValue(ErrorTextProperty); set => SetValue(ErrorTextProperty, value); }
    public bool IsPassword { get => (bool)GetValue(IsPasswordProperty); set => SetValue(IsPasswordProperty, value); }
    public string Suffix { get => (string)GetValue(SuffixProperty); set => SetValue(SuffixProperty, value); }
    public string ActionText { get => (string)GetValue(ActionTextProperty); set => SetValue(ActionTextProperty, value); }
    public ICommand? ActionCommand { get => (ICommand?)GetValue(ActionCommandProperty); set => SetValue(ActionCommandProperty, value); }
    public Keyboard InputKeyboard { get => (Keyboard)GetValue(InputKeyboardProperty); set => SetValue(InputKeyboardProperty, value); }

    void OnInputChanged(object? sender, TextChangedEventArgs e) => Text = e.NewTextValue ?? "";

    void UpdateTrail()
    {
        if (IsPassword) { TrailLabel.Text = _hidden ? "Afficher" : "Masquer"; TrailLabel.TextColor = Palette.Primary; }
        else if (!string.IsNullOrEmpty(ActionText)) { TrailLabel.Text = ActionText; TrailLabel.TextColor = Palette.Primary; }
        else { TrailLabel.Text = Suffix; TrailLabel.TextColor = Palette.TextMuted; }
        TrailLabel.IsVisible = !string.IsNullOrEmpty(TrailLabel.Text);
    }

    void OnTrailTapped(object? sender, TappedEventArgs e)
    {
        if (IsPassword) { _hidden = !_hidden; Input.IsPassword = _hidden; UpdateTrail(); }
        else if (ActionCommand?.CanExecute(null) == true) ActionCommand.Execute(null);
    }
}
