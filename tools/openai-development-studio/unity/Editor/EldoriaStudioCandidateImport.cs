// Install only into a disposable Unity 6000.3 project; never modify production scene.
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class EldoriaStudioCandidateImport
{
    const string Root = "Assets/EldoriaStudioCandidate";
    public static void Run()
    {
        var source = Environment.GetEnvironmentVariable("ELDORIA_STUDIO_SPRITE");
        var proof = Environment.GetEnvironmentVariable("ELDORIA_STUDIO_UNITY_PROOF");
        if (string.IsNullOrWhiteSpace(source) || !File.Exists(source) ||
            string.IsNullOrWhiteSpace(proof) || File.Exists(proof))
            throw new InvalidOperationException("Explicit verified source and new proof path required");
        if (AssetDatabase.IsValidFolder(Root))
            throw new InvalidOperationException("Candidate root already exists; no overwrite");
        AssetDatabase.CreateFolder("Assets", "EldoriaStudioCandidate");
        string path = Root + "/Candidate.png";
        File.Copy(source, Path.Combine(Application.dataPath, "EldoriaStudioCandidate", "Candidate.png"), false);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) throw new InvalidOperationException("TextureImporter missing");
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.spritePixelsPerUnit = 100;
        importer.maxTextureSize = 1024;
        importer.SaveAndReimport();
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null || sprite.rect.width <= 0 || sprite.rect.height <= 0)
            throw new InvalidOperationException("Sprite failed native import");
        var go = new GameObject("EldoriaStudioCandidate_NOT_APPROVED");
        try
        {
            go.AddComponent<SpriteRenderer>().sprite = sprite;
            string prefabPath = Root + "/Candidate.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            if (prefab == null || prefab.GetComponent<SpriteRenderer>().sprite == null)
                throw new InvalidOperationException("Prefab verification failed");
            AssetDatabase.SaveAssets();
            var result = new Proof { unityVersion = Application.unityVersion, spriteWidth = sprite.rect.width,
                spriteHeight = sprite.rect.height, prefabPath = prefabPath, artApproved = false,
                importedIntoGame = false };
            File.WriteAllText(proof, JsonUtility.ToJson(result, true));
            Debug.Log("STUDIO_NATIVE_IMPORT_PASS: isolated sprite and prefab; no scene change");
        }
        finally { UnityEngine.Object.DestroyImmediate(go); }
    }
    public static void RunShader()
    {
        var source = Environment.GetEnvironmentVariable("ELDORIA_STUDIO_SHADER");
        var proof = Environment.GetEnvironmentVariable("ELDORIA_STUDIO_UNITY_PROOF");
        if (string.IsNullOrWhiteSpace(source) || !File.Exists(source) ||
            string.IsNullOrWhiteSpace(proof) || File.Exists(proof))
            throw new InvalidOperationException("Explicit reviewed shader and new proof path required");
        if (AssetDatabase.IsValidFolder(Root))
            throw new InvalidOperationException("Candidate root already exists; no overwrite");
        AssetDatabase.CreateFolder("Assets", "EldoriaStudioCandidate");
        var path = Root + "/DistrictGroundCandidate.shader";
        File.Copy(source, Path.Combine(Application.dataPath, "EldoriaStudioCandidate", "DistrictGroundCandidate.shader"), false);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
        if (shader == null || ShaderUtil.ShaderHasError(shader))
            throw new InvalidOperationException("Native shader import reported errors");
        var material = new Material(shader);
        try {
            if (!material.HasProperty("_Soil") || !material.HasProperty("_MainTex"))
                throw new InvalidOperationException("Expected ground material properties missing");
        } finally { UnityEngine.Object.DestroyImmediate(material); }
        File.WriteAllText(proof, JsonUtility.ToJson(new ShaderProof {
            unityVersion = Application.unityVersion, shaderName = shader.name,
            importWithoutReportedErrors = true, artApproved = false,
            gameRendered = false, importedIntoGame = false
        }, true));
        Debug.Log("STUDIO_NATIVE_SHADER_IMPORT_PASS: isolated import, not a render or art approval");
    }
    [Serializable] class ShaderProof {
        public string unityVersion, shaderName;
        public bool importWithoutReportedErrors, artApproved, gameRendered, importedIntoGame;
    }
    [Serializable] class Proof {
        public string unityVersion, prefabPath;
        public float spriteWidth, spriteHeight;
        public bool artApproved, importedIntoGame;
    }
}
