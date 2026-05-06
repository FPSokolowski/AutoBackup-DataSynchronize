namespace ABDS.Core.Sync;

public sealed record SyncPlanItem(string SourceFile, string TargetFile, long Bytes);

public static class SyncPlanner
{
    public static List<SyncPlanItem> BuildPlan(
    string sourceRoot,
    string targetRoot,
    IFileComparisonStrategy strategy)
    {
        var plan = new List<SyncPlanItem>();

        if (!Directory.Exists(sourceRoot))
            return plan;

        foreach (var srcFilePath in Directory.EnumerateFiles(
                     sourceRoot, "*", SearchOption.AllDirectories))
        {
            var rel = Path.GetRelativePath(sourceRoot, srcFilePath);
            var dstFilePath = Path.Combine(targetRoot, rel);

            var sInfo = new FileInfo(srcFilePath);
            var dInfo = new FileInfo(dstFilePath);

            if (strategy.AreDifferent(sInfo, dInfo))
            {
                plan.Add(new SyncPlanItem(
                    srcFilePath,
                    dstFilePath,
                    sInfo.Length));
            }
        }

        return plan;
    }
}