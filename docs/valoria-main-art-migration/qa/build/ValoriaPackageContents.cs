#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public sealed class ValoriaPackageContents : IPostprocessBuildWithReport
{
    public int callbackOrder => int.MaxValue;

    [Serializable] sealed class Entry
    {
        public string container, source, guid;
        public long bytes;
    }

    [Serializable] sealed class Inventory
    {
        public string source, target, unity;
        public Entry[] entries;
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        string path = Environment.GetEnvironmentVariable("VALORIA_QA_PACKED_CONTENTS");
        if (string.IsNullOrEmpty(path)) return;
        var inventory = new Inventory {
            source = Environment.GetEnvironmentVariable("GITHUB_SHA"),
            target = report.summary.platform.ToString(),
            unity = UnityEngine.Application.unityVersion,
            entries = report.packedAssets.SelectMany(pack => pack.contents.Select(entry => new Entry {
                container = pack.shortPath, source = entry.sourceAssetPath,
                guid = entry.sourceAssetGUID.ToString(), bytes = checked((long)entry.packedSize)
            })).OrderBy(entry => entry.container,StringComparer.Ordinal)
               .ThenBy(entry => entry.source,StringComparer.Ordinal).ToArray()
        };
        if (inventory.entries.Length == 0) throw new InvalidOperationException("Unity BuildReport contains no packed asset rows.");
        using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
        using (var writer = new StreamWriter(stream)) writer.Write(JsonUtility.ToJson(inventory));
    }
}
#endif
