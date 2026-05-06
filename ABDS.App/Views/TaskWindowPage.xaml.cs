using ABDS.App.ViewModels;

namespace ABDS.App.Views;

public partial class TaskWindowPage : ContentPage
{
    private readonly TaskWindowViewModel _vm;

    public TaskWindowPage(TaskWindowViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.Stop();
    }
}