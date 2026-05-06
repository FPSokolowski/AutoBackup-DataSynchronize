using ABDS.Core.Hashing;

namespace ABDS.Core.Sync;

public sealed class HashBelowSizeComparison : IFileComparisonStrategy
{
    private readonly long _thresholdBytes;
    private readonly IHashCache _cache;

    public HashBelowSizeComparison(int thresholdMb, IHashCache cache)
    {
        _thresholdBytes = thresholdMb * 1024L * 1024L;
        _cache = cache;
    }

    public bool AreDifferent(FileInfo source, FileInfo target)
    {
        if (!target.Exists)
            return true;

        if (source.Length != target.Length)
            return true;

        if (source.LastWriteTimeUtc != target.LastWriteTimeUtc)
            return true;

        if (source.Length <= _thresholdBytes)
        {
            var s = Sha256Hasher.GetOrComputeHex(source, _cache);
            var t = Sha256Hasher.GetOrComputeHex(target, _cache);
            return !StringComparer.OrdinalIgnoreCase.Equals(s, t);
        }

        return false;
    }
}