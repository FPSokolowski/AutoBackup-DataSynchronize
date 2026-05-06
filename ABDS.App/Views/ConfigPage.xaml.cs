
using ABDS.App.ViewModels;

namespace ABDS.App.Views;

public partial class ConfigPage : ContentPage
{
    public ConfigPage()
    {
        InitializeComponent();
        BindingContext = new SyncConfigViewModel();
    }
}