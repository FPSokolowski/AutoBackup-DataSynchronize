using ABDS.App.ViewModels;

namespace ABDS.App.Views;

public partial class ActionsPage : ContentPage
{
    public ActionsPage(ActionsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}