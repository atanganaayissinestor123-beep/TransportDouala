using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TransportDouala.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty] private bool isBusy;

    [RelayCommand]
    private Task GoBack() => Shell.Current.GoToAsync("..");
}
