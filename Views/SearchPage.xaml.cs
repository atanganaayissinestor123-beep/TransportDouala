using TransportDouala.ViewModels;

namespace TransportDouala.Views;

public partial class SearchPage : ContentPage
{
    readonly SearchViewModel _vm;

    public SearchPage(SearchViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.Refresh();
    }

    void OnDepartureSuggestionTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject { BindingContext: string name }) _vm.PickDepartureCommand.Execute(name);
    }

    void OnDestinationSuggestionTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject { BindingContext: string name }) _vm.PickDestinationCommand.Execute(name);
    }

    void OnRecentTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject { BindingContext: string name }) _vm.PickRecentCommand.Execute(name);
    }
}
