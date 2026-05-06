using System.Diagnostics;
using System.Reflection;

namespace ABDS.Setup;

internal static class Program
{
    [STAThread]
    private static int Main()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "ABDS-Setup-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            ExtractResource("install.ps1", Path.Combine(tempDir, "install.ps1"));
            ExtractResource("payload.zip", Path.Combine(tempDir, "payload.zip"));

            var installScript = Path.Combine(tempDir, "install.ps1");
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{installScript}\"",
                UseShellExecute = true,
                Verb = "runas"
            });

            process?.WaitForExit();
            return process?.ExitCode ?? 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.ToString(),
                "ABDS Setup",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return 1;
        }
        finally
        {
            try
            {
                Directory.Delete(tempDir, recursive: true);
            }
            catch
            {
                // Best-effort cleanup.
            }
        }
    }

    private static void ExtractResource(string logicalName, string targetPath)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(logicalName)
            ?? throw new InvalidOperationException($"Missing embedded resource: {logicalName}");
        using var file = File.Create(targetPath);
        stream.CopyTo(file);
    }
}
