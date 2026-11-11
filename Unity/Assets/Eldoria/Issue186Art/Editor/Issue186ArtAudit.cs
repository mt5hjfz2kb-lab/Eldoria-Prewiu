#if UNITY_EDITOR
using System;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;
using Eldoria.Domain;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.Issue186Art.Editor
{
    // Read-only import proof and preview; no scene/saves/builds are modified.
    public static class Issue186ArtAudit
    {
        const string Dir = "Assets/Eldoria/Issue186Art/Resources/Issue186ResourceArt/";
        const string Manifest = "Assets/Eldoria/Issue186Art/issue186_manifest.json";
        [Serializable] sealed class Artifact
        {
            public string file, key, sha256, guid;
            public int width, height, bytes;
        }
        [Serializable] sealed class Package
        {
            public string version, status;
            public int assetCount;
            public Artifact[] artifacts;
        }
        static void Need(bool ok, string error)
        {
            if (!ok) throw new InvalidOperationException("ISSUE186_IMPORT_FAIL: "+error);
        }
        static string Hash(string file)
        {
            using (var sha = SHA256.Create())
            using (var stream = File.OpenRead(file))
                return BitConverter.ToString(sha.ComputeHash(stream))
                    .Replace("-", "").ToLowerInvariant();
        }
        public static void VerifyImportedArt()
        {
            Need(File.Exists(Manifest), "Missing original source manifest");
            var data = JsonUtility.FromJson<Package>(File.ReadAllText(Manifest));
            Need(data != null && data.artifacts != null &&
                 data.artifacts.Length == 16 && data.assetCount == 16,
                 "Expected 12 packages, 2 badges, hand and halo");
            var names = new HashSet<string>(StringComparer.Ordinal);
            var guids = new HashSet<string>(StringComparer.Ordinal);
            int packages = 0, badges = 0, guides = 0;
            foreach (var a in data.artifacts)
            {
                Need(a != null && !string.IsNullOrEmpty(a.file) &&
                     a.file.EndsWith(".png", StringComparison.OrdinalIgnoreCase) &&
                     !a.file.Contains("/") && !a.file.Contains("\\") &&
                     names.Add(a.file), "Filename duplicate or invalid");
                string path = Dir + a.file;
                Need(File.Exists(path), "Missing art: "+path);
                Need(Hash(path) == a.sha256, "Unexpected original PNG bytes: "+a.file);
                var sprite = Resources.Load<Sprite>(a.key);
                Need(sprite, "Missing imported Unity Sprite: "+a.key);
                Need(Mathf.Abs(sprite.rect.width - a.width) < .01f &&
                     Mathf.Abs(sprite.rect.height - a.height) < .01f,
                     "Sprite dimensions incorrect: "+a.file);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                Need(importer && importer.textureType == TextureImporterType.Sprite &&
                     importer.spriteImportMode == SpriteImportMode.Single &&
                     importer.alphaIsTransparency && !importer.mipmapEnabled,
                     "Missing transparent UI Sprite import settings: "+a.file);
                string guid = AssetDatabase.AssetPathToGUID(path);
                Need(!string.IsNullOrEmpty(guid) && guids.Add(guid) &&
                     guid == a.guid && AssetDatabase.GUIDToAssetPath(guid) == path,
                     "Unity GUID mismatch/duplicate: "+a.file);
                if (a.file.StartsWith("pack_", StringComparison.Ordinal)) packages++;
                else if (a.file.StartsWith("badge_", StringComparison.Ordinal)) badges++;
                else if (a.file.StartsWith("tutorial_", StringComparison.Ordinal)) guides++;
            }
            Need(packages == 12 && badges == 2 && guides == 2,
                "Unexpected identity or number of imported assets");
            foreach (var kind in new [] {"wood", "stone", "food"})
            foreach (var amount in new [] {100, 1000, 10000, 100000})
                Need(Eldoria.Issue186Art.Issue186ArtKit.PacketSprite(kind, amount),
                    "Resources.Load cannot find "+kind+":"+amount);

            Debug.Log("ISSUE186_UNITY_ART_IMPORT_PASS: "+
                data.assetCount+" real textures, 16 hashes, 16 unique GUIDs, "+
                "12 actual Sprite paths. THIS IS NOT visual/gameplay approval.");
        }

        [MenuItem("Eldoria/Issue 186/Verificar 16 sprites importados")]
        static void VerifyFromMenu() => VerifyImportedArt();

        [MenuItem("Eldoria/Issue 186/Abrir vista de recursos")]
        static void OpenWindow()
        {
            VerifyImportedArt();
            var window = EditorWindow.GetWindow<Issue186ArtPreview>("Eldoria · Recursos");
            window.minSize = new Vector2(390, 600);
            window.Show();
        }
    }

    // Genuine Unity Editor texture preview, not a captured game screenshot.
    // A later scene probe must still certify actual mobile Game View and touches.
    public sealed class Issue186ArtPreview : EditorWindow
    {
        Vector2 scroll;
        readonly string[] kinds = {"wood", "stone", "food"};
        readonly int[] amounts = {100, 1000, 10000, 100000};
        void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.LabelField("Eldoria · paquetes genéricos",
                EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Arte real importado: el importe es texto dinámico. "+
                "Este panel no modifica la partida ni concede recompensas.",
                MessageType.Info);
            int columns = position.width < 690 ? 2 : 4;
            for(int k=0;k<kinds.Length;k++)
            {
                EditorGUILayout.LabelField(kinds[k].ToUpperInvariant(),
                    EditorStyles.boldLabel);
                for(int i=0;i<4;i+=columns)
                {
                    EditorGUILayout.BeginHorizontal();
                    for(int j=i;j<Math.Min(4,i+columns);j++)
                    {
                        int amount=amounts[j];
                        var sprite=Eldoria.Issue186Art.Issue186ArtKit.PacketSprite(
                            kinds[k],amount);
                        EditorGUILayout.BeginVertical(GUILayout.Width(columns==2?150:170));
                        Rect rect=GUILayoutUtility.GetRect(
                            columns==2?145f:165f, columns==2?145f:165f);
                        if(sprite)
                            GUI.DrawTexture(rect,sprite.texture,ScaleMode.ScaleToFit,true);
                        EditorGUILayout.LabelField(
                            ResourceRewardRules.TierLabel(amount),
                            EditorStyles.boldLabel);
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndScrollView();
        }
    }
}
#endif
