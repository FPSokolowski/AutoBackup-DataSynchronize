using ABDS.Core.Sync;

public sealed class MetadataOnlyComparison : IFileComparisonStrategy
{
    public bool AreDifferent(FileInfo source, FileInfo target)
    {
        if (!target.Exists)
            return true;

        return source.Length != target.Length ||
               source.LastWriteTimeUtc != target.LastWriteTimeUtc ||
               source.Name != target.Name;
    }
}