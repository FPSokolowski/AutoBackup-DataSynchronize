using System.Security.Cryptography;

namespace ABDS.Core.Hashing;

public static class Sha256Hasher
{
    public static string ComputeHex(string filePath)
    {
        using var sha = SHA256.Create();
        using var fs = File.OpenRead(filePath);
        var hash = sha.ComputeHash(fs);
        return Convert.ToHexString(hash); // uppercase HEX
    }

    public static string GetOrComputeHex(FileInfo fi, IHashCache cache)
    {
        var key = new HashCacheKey(fi.FullName, fi.Length, fi.LastWriteTimeUtc.Ticks);

        if (cache.TryGet(key, out var hex))
            return hex;

        hex = ComputeHex(fi.FullName);
        cache.Set(key, hex);
        return hex;
    }
}