using TransportDouala.ViewModels;

namespace TransportDouala.Views;

public partial class SearchPage : ContentPage
{
    readonly SearchViewModel _vm;

    public SearchPage(SearchViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        _vm = vm;
    }

    void OnRecentTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject { BindingContext: string name }) _vm.PickRecentCommand.Execute(name);
    }
}
