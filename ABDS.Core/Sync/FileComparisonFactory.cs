using ABDS.Core.Hashing;
using ABDS.Core.Models;

namespace ABDS.Core.Sync;

public static class FileComparisonFactory
{
    public static IFileComparisonStrategy Create(
        SyncComparisonMode mode,
        int thresholdMb,
        IHashCache cache)
    {
        return mode switch
        {
            SyncComparisonMode.MetadataOnly =>
                new MetadataOnlyComparison(),

            SyncComparisonMode.HashBelowSizeMb =>
                new HashBelowSizeComparison(thresholdMb, cache),

            SyncComparisonMode.HashAll =>
                new HashAllComparison(cache),

            _ => new MetadataOnlyComparison()
        };
    }
}