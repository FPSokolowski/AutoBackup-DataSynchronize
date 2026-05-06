namespace ABDS.Core.Sync;

public interface IFileComparisonStrategy
{
    bool AreDifferent(FileInfo source, FileInfo target);
}