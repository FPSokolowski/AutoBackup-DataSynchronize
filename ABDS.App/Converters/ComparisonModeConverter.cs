using System.Globalization;
using ABDS.Core.Models;

namespace ABDS.App.Converters;

public class ComparisonModeConverter : IValueConverter
{
    public object Convert(object value, Type targetType,
        object parameter, CultureInfo culture)
    {
        return (SyncComparisonMode)value switch
        {
            SyncComparisonMode.MetadataOnly =>
                "Porównywanie tylko metadanych",

            SyncComparisonMode.HashBelowSizeMb =>
                "Hash plików poniżej X MB",

            SyncComparisonMode.HashAll =>
                "Hash wszystkich plików",

            _ => value.ToString()
        };
    }

    public object ConvertBack(object value, Type targetType,
        object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}