using System.IO.Pipes;

namespace ABDS.DesktopHost;

internal static class Program
{
    private const string MutexName = "ABDS.DesktopHost.SingleInstance";
    private const string PipeName = "ABDS_DESKTOP_HOST_PIPE_V1";

    [STAThread]
    private static void Main()
    {
        var runId = MainForm.ReadRunId(Environment.GetCommandLineArgs());
        using var mutex = new Mutex(initiallyOwned: true, MutexName, out var createdNew);
        if (!createdNew)
        {
            TrySendExistingInstanceCommand(runId);
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm(PipeName, runId));
    }

    private static void TrySendExistingInstanceCommand(string? runId)
    {
        try
        {
            using var pipe = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
            pipe.Connect(1000);
            using var writer = new StreamWriter(pipe) { AutoFlush = true };
            writer.WriteLine(string.IsNullOrWhiteSpace(runId) ? "show" : $"show|{runId}");
        }
        catch
        {
            // If the existing instance is still starting, a second launch can fail quietly.
        }
    }
}
