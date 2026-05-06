using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using ABDS.Core.Models;

#pragma warning disable SYSLIB0014

namespace ABDS.Core.Destinations;

public static class DestinationProbe
{
    public static async Task<DestinationProbeResult> ProbeAsync(
        string location,
        bool writeTest,
        CancellationToken ct = default)
    {
        var kind = Classify(location);
        try
        {
            if (kind == DestinationKind.Ftp)
                return await ProbeFtpAsync(location, writeTest, ct);

            return await ProbeFileSystemAsync(location, kind, writeTest, ct);
        }
        catch (Exception ex)
        {
            return new DestinationProbeResult
            {
                Location = location,
                Kind = kind,
                Available = false,
                Writable = false,
                Status = "Unavailable",
                ErrorCode = ex.GetType().Name,
                ErrorMessage = ex.Message,
                DiagnosticDetails = ex.ToString()
            };
        }
    }

    public static DestinationKind Classify(string location)
    {
        if (string.IsNullOrWhiteSpace(location))
            return DestinationKind.Unknown;

        if (Uri.TryCreate(location, UriKind.Absolute, out var uri) &&
            (uri.Scheme.Equals("ftp", StringComparison.OrdinalIgnoreCase) ||
             uri.Scheme.Equals("ftps", StringComparison.OrdinalIgnoreCase)))
            return DestinationKind.Ftp;

        if (location.StartsWith(@"\\", StringComparison.Ordinal))
            return DestinationKind.NetworkShare;

        try
        {
            var root = Path.GetPathRoot(Path.GetFullPath(location));
            if (!string.IsNullOrWhiteSpace(root))
            {
                var drive = new DriveInfo(root);
                if (drive.DriveType == DriveType.Removable)
                    return DestinationKind.RemovableDevice;
            }
        }
        catch
        {
            // Fall through to local; the probe will report the concrete failure.
        }

        return DestinationKind.LocalPath;
    }

    public static DestinationIdentity BuildIdentity(string location)
    {
        var kind = Classify(location);
        if (kind == DestinationKind.Ftp && Uri.TryCreate(location, UriKind.Absolute, out var uri))
        {
            var normalizedPath = uri.AbsolutePath.TrimEnd('/');
            return new DestinationIdentity
            {
                Fingerprint = Hash($"ftp|{uri.Scheme}|{uri.Host}|{uri.Port}|{normalizedPath}".ToLowerInvariant()),
                UriHost = uri.Host,
                UriPath = normalizedPath
            };
        }

        var fullPath = Path.GetFullPath(location);
        var root = Path.GetPathRoot(fullPath);
        if (string.IsNullOrWhiteSpace(root))
            return new DestinationIdentity { Fingerprint = Hash(fullPath.ToLowerInvariant()) };

        var drive = new DriveInfo(root);
        var serial = OperatingSystem.IsWindows() ? TryGetVolumeSerial(root) : null;
        var label = Safe(() => drive.IsReady ? drive.VolumeLabel : null);
        var format = Safe(() => drive.IsReady ? drive.DriveFormat : null);
        var totalSize = Safe(() => drive.IsReady ? drive.TotalSize : (long?)null);
        var fingerprintMaterial = string.Join("|", "fs", root, serial, label, format, totalSize?.ToString() ?? "");

        return new DestinationIdentity
        {
            Fingerprint = Hash(fingerprintMaterial.ToLowerInvariant()),
            RootPath = root,
            VolumeLabel = label,
            VolumeSerial = serial,
            DriveType = drive.DriveType.ToString(),
            DriveFormat = format,
            TotalSizeBytes = totalSize
        };
    }

    private static async Task<DestinationProbeResult> ProbeFileSystemAsync(
        string location,
        DestinationKind kind,
        bool writeTest,
        CancellationToken ct)
    {
        var fullPath = Path.GetFullPath(location);
        var identity = BuildIdentity(fullPath);
        Directory.CreateDirectory(fullPath);

        if (!writeTest)
        {
            return new DestinationProbeResult
            {
                Location = location,
                Kind = kind,
                Available = true,
                Writable = true,
                Identity = identity,
                Status = "Available"
            };
        }

        var probePath = Path.Combine(fullPath, $".abds_probe_{Guid.NewGuid():N}.tmp");
        var expected = $"ABDS probe {Guid.NewGuid():N}";

        await File.WriteAllTextAsync(probePath, expected, ct);
        var actual = await File.ReadAllTextAsync(probePath, ct);
        File.Delete(probePath);

        if (!StringComparer.Ordinal.Equals(expected, actual))
            throw new IOException("Probe readback mismatch.");

        return new DestinationProbeResult
        {
            Location = location,
            Kind = kind,
            Available = true,
            Writable = true,
            Identity = identity,
            Status = "Available"
        };
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("SYSLIB", "SYSLIB0014", Justification = "FTP support is kept for user-configured legacy NAS targets.")]
    private static async Task<DestinationProbeResult> ProbeFtpAsync(string location, bool writeTest, CancellationToken ct)
    {
        var identity = BuildIdentity(location);
        if (!writeTest)
        {
            return new DestinationProbeResult
            {
                Location = location,
                Kind = DestinationKind.Ftp,
                Available = true,
                Writable = true,
                Identity = identity,
                Status = "Available"
            };
        }

        var content = Encoding.UTF8.GetBytes($"ABDS probe {Guid.NewGuid():N}");
        var fileUri = CombineFtpUri(location, $".abds_probe_{Guid.NewGuid():N}.tmp");

        await FtpUploadAsync(fileUri, content, ct);
        var downloaded = await FtpDownloadAsync(fileUri, ct);
        await FtpDeleteAsync(fileUri, ct);

        if (!content.SequenceEqual(downloaded))
            throw new IOException("FTP probe readback mismatch.");

        return new DestinationProbeResult
        {
            Location = location,
            Kind = DestinationKind.Ftp,
            Available = true,
            Writable = true,
            Identity = identity,
            Status = "Available"
        };
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("SYSLIB", "SYSLIB0014", Justification = "FTP support is kept for user-configured legacy NAS targets.")]
    private static async Task FtpUploadAsync(Uri uri, byte[] content, CancellationToken ct)
    {
        var request = CreateFtpRequest(uri, WebRequestMethods.Ftp.UploadFile);
        request.ContentLength = content.Length;
        await using var stream = await request.GetRequestStreamAsync();
        await stream.WriteAsync(content, ct);
        using var response = (FtpWebResponse)await request.GetResponseAsync();
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("SYSLIB", "SYSLIB0014", Justification = "FTP support is kept for user-configured legacy NAS targets.")]
    private static async Task<byte[]> FtpDownloadAsync(Uri uri, CancellationToken ct)
    {
        var request = CreateFtpRequest(uri, WebRequestMethods.Ftp.DownloadFile);
        using var response = (FtpWebResponse)await request.GetResponseAsync();
        await using var stream = response.GetResponseStream();
        using var memory = new MemoryStream();
        if (stream is not null)
            await stream.CopyToAsync(memory, ct);
        return memory.ToArray();
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("SYSLIB", "SYSLIB0014", Justification = "FTP support is kept for user-configured legacy NAS targets.")]
    private static async Task FtpDeleteAsync(Uri uri, CancellationToken ct)
    {
        var request = CreateFtpRequest(uri, WebRequestMethods.Ftp.DeleteFile);
        using var response = (FtpWebResponse)await request.GetResponseAsync();
        await Task.CompletedTask;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("SYSLIB", "SYSLIB0014", Justification = "FTP support is kept for user-configured legacy NAS targets.")]
    private static FtpWebRequest CreateFtpRequest(Uri uri, string method)
    {
        var request = (FtpWebRequest)WebRequest.Create(uri);
        request.Method = method;
        request.UseBinary = true;
        request.KeepAlive = false;
        request.EnableSsl = uri.Scheme.Equals("ftps", StringComparison.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(uri.UserInfo))
        {
            var parts = uri.UserInfo.Split(':', 2);
            request.Credentials = new NetworkCredential(
                Uri.UnescapeDataString(parts[0]),
                parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : "");
        }

        return request;
    }

    private static Uri CombineFtpUri(string baseLocation, string fileName)
    {
        var separator = baseLocation.EndsWith("/", StringComparison.Ordinal) ? "" : "/";
        return new Uri(baseLocation + separator + Uri.EscapeDataString(fileName));
    }

    private static string? TryGetVolumeSerial(string root)
    {
        var volumeName = new StringBuilder(261);
        var fileSystemName = new StringBuilder(261);
        return GetVolumeInformation(
            root,
            volumeName,
            volumeName.Capacity,
            out var serial,
            out _,
            out _,
            fileSystemName,
            fileSystemName.Capacity)
            ? serial.ToString("X8")
            : null;
    }

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool GetVolumeInformation(
        string lpRootPathName,
        StringBuilder lpVolumeNameBuffer,
        int nVolumeNameSize,
        out uint lpVolumeSerialNumber,
        out uint lpMaximumComponentLength,
        out uint lpFileSystemFlags,
        StringBuilder lpFileSystemNameBuffer,
        int nFileSystemNameSize);

    private static T? Safe<T>(Func<T?> read)
    {
        try { return read(); }
        catch { return default; }
    }

    private static string Hash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }
}
