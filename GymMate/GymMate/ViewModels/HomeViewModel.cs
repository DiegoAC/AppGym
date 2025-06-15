using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace GymMate.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    [ObservableProperty]
    private int count;

    [ICommand]
    private void IncrementCount()
    {
        Count++;
    }
}
