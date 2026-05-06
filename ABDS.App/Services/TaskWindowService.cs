using ABDS.App.ViewModels;
using ABDS.App.Views;

namespace ABDS.App.Services;

public sealed class TaskWindowService(IServiceProvider sp)
{
    public async Task ShowRunAsync(string runId)
    {
        var page = sp.GetRequiredService<TaskWindowPage>();
        var vm = (TaskWindowViewModel)page.BindingContext;

        await vm.StartAsync(runId);
        await Shell.Current.Navigation.PushModalAsync(new NavigationPage(page));
    }
}