// Copied only into an isolated validation project's Assets/Editor directory.
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class EldoriaMetadataNativeVerification
{
    [Serializable] public class Entry
    {
        public string path;
        public string guid;
        public string meta_sha256;
        public string disposition;
    }
    [Serializable] public class Manifest { public Entry[] entries; }
    [Serializable] public class Row
    {
        public string path;
        public string expected_guid;
        public string actual_guid;
        public bool bytes_preserved;
        public bool folder;
        public int objects;
        public bool scene_asset;
    }
    [Serializable] public class Report
    {
        public string unity_version;
        public string candidate_sha;
        public string state;
        public Row[] entries;
        public string[] failures;
        public int enabled_scenes;
        public int missing_scripts;
        public string company_name;
        public string product_name;
        public string validation_mode;
        public bool scene_content_verified;
    }

    static string Argument(string key)
    {
        var args = Environment.GetCommandLineArgs();
        for (int i = 0; i + 1 < args.Length; i++) if (args[i] == key) return args[i + 1];
        throw new ArgumentException("Required argument: " + key);
    }

    static string Hash(string file)
    {
        using (var sha = SHA256.Create())
        using (var stream = File.OpenRead(file))
            return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
    }

    public static void Run()
    {
        string output = Argument("-metadataReport");
        var failures = new List<string>();
        var rows = new List<Row>();
        var report = new Report { unity_version = UnityEngine.Application.unityVersion,
            candidate_sha = Argument("-metadataCandidate"), validation_mode = Argument("-metadataMode") };
        try
        {
            report.company_name = UnityEngine.Application.companyName;
            report.product_name = UnityEngine.Application.productName;
            if (report.company_name != Argument("-metadataCompany") || report.product_name != Argument("-metadataProduct"))
                throw new Exception("Test persistence identity not isolated");
            string root = Directory.GetParent(UnityEngine.Application.dataPath).FullName;
            var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(Argument("-metadataManifest")));
            if (manifest == null || manifest.entries == null) throw new Exception("Recovery manifest is empty");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            foreach (var entry in manifest.entries)
            {
                if (entry.disposition != "restored") continue;
                if (!entry.path.StartsWith("Unity/Assets/", StringComparison.Ordinal) ||
                    !entry.path.EndsWith(".meta", StringComparison.Ordinal) || entry.path.Contains(".."))
                    throw new Exception("Unsafe manifest path: " + entry.path);
                string meta = entry.path.Substring("Unity/".Length);
                string asset = meta.Substring(0, meta.Length - ".meta".Length);
                string full = Path.Combine(root, meta);
                var row = new Row { path = asset, expected_guid = entry.guid,
                    actual_guid = AssetDatabase.AssetPathToGUID(asset),
                    folder = AssetDatabase.IsValidFolder(asset),
                    bytes_preserved = File.Exists(full) && Hash(full) == entry.meta_sha256 };
                if (!string.Equals(row.actual_guid, entry.guid, StringComparison.OrdinalIgnoreCase))
                    failures.Add("GUID mismatch: " + asset);
                if (!row.folder)
                {
                    // Scene assets are Editor references, not the object array
                    // of a prefab/model. Do not open or instantiate a scene here.
                    row.scene_asset = asset.EndsWith(".unity", StringComparison.OrdinalIgnoreCase);
                    if (row.scene_asset)
                        row.objects = AssetDatabase.LoadAssetAtPath<SceneAsset>(asset) == null ? 0 : 1;
                    else
                    {
                        var objects = AssetDatabase.LoadAllAssetsAtPath(asset);
                        row.objects = objects == null ? 0 : objects.Length;
                    }
                    if (row.objects == 0) failures.Add("Imported asset not loadable: " + asset);
                }
                if (!row.bytes_preserved) failures.Add("Unity rewrote original metadata: " + asset);
                rows.Add(row);
            }
            if (rows.Count == 0) failures.Add("No restored metadata was verified");
            if (report.validation_mode != "identities_only" && report.validation_mode != "full")
                throw new Exception("Unknown validation mode");
            if (report.validation_mode == "full") foreach (var scene in EditorBuildSettings.scenes)
            {
                if (!scene.enabled) continue;
                report.enabled_scenes++;
                var opened = EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);
                foreach (var go in opened.GetRootGameObjects())
                    foreach (var transform in go.GetComponentsInChildren<Transform>(true))
                        report.missing_scripts += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject);
            }
            report.scene_content_verified = report.validation_mode == "full";
            if (report.missing_scripts != 0) failures.Add("Enabled scenes contain missing MonoBehaviours");
        }
        catch (Exception e) { failures.Add(e.ToString()); }
        report.entries = rows.ToArray();
        report.failures = failures.ToArray();
        report.state = failures.Count == 0 ? "PASS" : "FAIL";
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        File.WriteAllText(output, JsonUtility.ToJson(report, true));
        if (failures.Count != 0) throw new Exception("Metadata native verification failed: " + string.Join("; ", failures));
        Debug.Log("ELDORIA_METADATA_NATIVE_PASS entries=" + rows.Count + " scenes=" + report.enabled_scenes);
    }
}
