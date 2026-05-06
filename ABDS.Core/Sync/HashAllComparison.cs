using ABDS.Core.Hashing;

namespace ABDS.Core.Sync;

public sealed class HashAllComparison : IFileComparisonStrategy
{
    private readonly IHashCache _cache;

    public HashAllComparison(IHashCache cache)
    {
        _cache = cache;
    }

    public bool AreDifferent(FileInfo source, FileInfo target)
    {
        if (!target.Exists)
            return true;

        if (source.Length != target.Length)
            return true;

        var s = Sha256Hasher.GetOrComputeHex(source, _cache);
        var t = Sha256Hasher.GetOrComputeHex(target, _cache);
        return !StringComparer.OrdinalIgnoreCase.Equals(s, t);
    }
}