using ABDS.App.ViewModels;

namespace ABDS.App.Views;

public partial class StatusPage : ContentPage
{
    public StatusPage(StatusViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        Loaded += async (_, _) => await vm.RefreshAsync();
    }
}