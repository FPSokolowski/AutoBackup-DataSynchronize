namespace ABDS.Service;

public sealed record AbdsPaths(string RootDir)
{
    public string ConfigPath => Path.Combine(RootDir, "config.json");
    public string StatePath => Path.Combine(RootDir, "state.json");
    public string DumpsDir => Path.Combine(RootDir, "Dumps");
    public string HashCachePath => Path.Combine(RootDir, "hashcache.json");

    public static AbdsPaths Default()
        => new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ABDS"));
}