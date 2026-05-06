using ABDS.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ABDS.App.ViewModels;

public partial class SyncConfigViewModel : ObservableObject
{
    public List<SyncComparisonMode> Modes { get; } =
        Enum.GetValues<SyncComparisonMode>().ToList();

    [ObservableProperty]
    private SyncComparisonMode selectedMode = SyncComparisonMode.HashBelowSizeMb;

    [ObservableProperty]
    private int hashThresholdMb = 20;

    public bool IsThresholdEditable =>
        SelectedMode == SyncComparisonMode.HashBelowSizeMb;

    public string SelectedModeDescription => SelectedMode switch
    {
        SyncComparisonMode.MetadataOnly =>
            "Porównywanie tylko nazwy, daty ostatniej modyfikacji i długości pliku.",

        SyncComparisonMode.HashBelowSizeMb =>
            "Kompromis między czasem wykonywania zadań a pewnością wykrycia zmian.",

        SyncComparisonMode.HashAll =>
            "Największa pewność wykrycia wszystkich zmian, wolniejsze wykonywanie zadań.",

        _ => ""
    };

    partial void OnSelectedModeChanged(SyncComparisonMode value)
    {
        OnPropertyChanged(nameof(IsThresholdEditable));
        OnPropertyChanged(nameof(SelectedModeDescription));
    }
}