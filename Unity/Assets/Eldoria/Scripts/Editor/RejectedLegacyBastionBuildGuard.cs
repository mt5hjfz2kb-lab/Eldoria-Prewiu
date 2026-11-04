#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Eldoria.Editor
{
    // An archived rejection must never become an implicit Resources dependency.
    public sealed class RejectedLegacyBastionBuildGuard : IPreprocessBuildWithReport
    {
        const string Model = "Valoria_HeroBastion_v1";
        const string Guid = "fa36c6abf91adc7778caeac12130f567";
        public int callbackOrder => -10000;
        public void OnPreprocessBuild(BuildReport report) { Validate(); }

        static bool Rejected(string text) =>
            text.IndexOf(Model,StringComparison.OrdinalIgnoreCase)>=0 ||
            text.IndexOf(Guid,StringComparison.OrdinalIgnoreCase)>=0 ||
            text.IndexOf("HeroBastionGenerated",StringComparison.OrdinalIgnoreCase)>=0;

        public static string Validate()
        {
            var files=Directory.GetFiles(UnityEngine.Application.dataPath,"*",SearchOption.AllDirectories);
            foreach(var file in files)
            {
                var path=file.Replace('\\','/');
                bool shipping=path.Contains("/Resources/")||path.Contains("/StreamingAssets/");
                bool metadata=path.EndsWith(".meta",StringComparison.OrdinalIgnoreCase);
                string meta=metadata?File.ReadAllText(file):"";
                // Unity recreates metadata for an empty historical directory.
                // A folder GUID is not model content. The actual rejected model
                // GUID and every payload filename remain forbidden.
                bool folderMetadata=metadata&&meta.Contains("folderAsset: yes");
                if(shipping&&((!folderMetadata&&Rejected(Path.GetFileName(path)))||
                    (metadata&&meta.IndexOf(Guid,StringComparison.OrdinalIgnoreCase)>=0)))
                    throw new BuildFailedException("Owner-vetoed Bastion remains in automatic build content: "+path);
                // Runtime loaders remain prohibited even if the archive is absent.
                if(path.EndsWith(".cs",StringComparison.OrdinalIgnoreCase)&&!path.Contains("/Editor/")&&
                    Rejected(File.ReadAllText(file)))
                    throw new BuildFailedException("Owner-vetoed Bastion referenced by runtime source: "+path);
            }
            var scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray();
            if(scenes.Length==0)throw new BuildFailedException("No enabled scene to audit for the Bastion veto.");
            var dependencies=AssetDatabase.GetDependencies(scenes,true);
            foreach(var asset in dependencies)
            {
                if(Rejected(asset))throw new BuildFailedException("Rejected Bastion is a scene dependency: "+asset);
                if((asset.EndsWith(".unity")||asset.EndsWith(".prefab"))&&File.Exists(asset)&&Rejected(File.ReadAllText(asset)))
                    throw new BuildFailedException("Rejected Bastion GUID in active serialized dependency: "+asset);
            }
            return "{\"status\":\"PASS\",\"enabled_scenes\":"+scenes.Length+
                ",\"dependencies_checked\":"+dependencies.Length+",\"rejected_shipping_assets\":0,\"rejected_runtime_loaders\":0}";
        }
    }
}
#endif
