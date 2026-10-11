using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GLTFast;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Eldoria.EditorTools
{
    // Isolated library staging: never touches the published gameplay scene.
    public static class ValoriaLibraryV1Proof
    {
        const string Input = "Assets/Eldoria/Resources/Valoria/ProductionArt/LibraryV1";
        const string Stage = "Assets/Eldoria/ArtTests/ValoriaLibraryV1";
        const string Original = "Assets/Eldoria/ArtTests/ValoriaGatehouseV1/ValoriaVisualSliceV1.unity";
        static readonly string[] Models = { "Valoria_Gate_LibraryV1", "Valoria_Wall_LibraryV1", "Valoria_Tower_LibraryV1" };

        public static async void Build()
        {
            try
            {
                Directory.CreateDirectory("ValoriaLibraryV1Proof");
                Directory.CreateDirectory(Stage);
                EditorSceneManager.OpenScene(Original, OpenSceneMode.Single);
                var camera = UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).FirstOrDefault();
                if (camera == null) throw new Exception("Canonical comparison camera missing");
                var scene = SceneManager.GetActiveScene();
                var group = new GameObject("Valoria Library V1 isolated composition");
                var originals = new System.Collections.Generic.List<GameObject>();
                for (int i=0;i<Models.Length;i++)
                {
                    string file = Path.GetFullPath(Input + "/" + Models[i] + ".glb");
                    if (!File.Exists(file)) throw new FileNotFoundException(file);
                    var import = new GltfImport(null, new UninterruptedDeferAgent());
                    if (!await import.LoadFile(file)) throw new Exception("GLB load failed: " + Models[i]);
                    var item = new GameObject(Models[i]);
                    item.transform.SetParent(group.transform, false);
                    item.transform.localPosition = new Vector3((i-1)*5.5f, 0, 0);
                    if (!await import.InstantiateMainSceneAsync(item.transform)) throw new Exception("GLB instantiate failed");
                    if (!item.GetComponentsInChildren<MeshFilter>().Any()) throw new Exception("No mesh in " + Models[i]);
                    originals.Add(item);
                }
                // Stage a separately saved scene; no gameplay systems or canonical visual guard are changed.
                // Persist imported runtime mesh and URP material assets before saving prefabs.
                int assetNumber=0;
                foreach(var item in originals)
                {
                    foreach(var mf in item.GetComponentsInChildren<MeshFilter>(true))
                    {
                        if(mf.sharedMesh==null || AssetDatabase.Contains(mf.sharedMesh))continue;
                        var copy=UnityEngine.Object.Instantiate(mf.sharedMesh);
                        copy.name=Models[originals.IndexOf(item)]+" mesh "+assetNumber;
                        string path=Stage+"/mesh-"+assetNumber+++".asset";
                        AssetDatabase.CreateAsset(copy,path);
                        mf.sharedMesh=copy;
                    }
                    foreach(var renderer in item.GetComponentsInChildren<Renderer>(true))
                    {
                        var mats=renderer.sharedMaterials;
                        for(int i=0;i<mats.Length;i++)
                        {
                            if(mats[i]==null || AssetDatabase.Contains(mats[i]))continue;
                            Shader lit=Shader.Find("Universal Render Pipeline/Lit");
                            if(lit==null)throw new Exception("URP Lit shader missing");
                            var mat=new Material(lit);
                            mat.name="Valoria Library persistent material";
                            var source=mats[i];
                            if(source.HasProperty("baseColorFactor"))mat.SetColor("_BaseColor",source.GetColor("baseColorFactor"));
                            else if(source.HasProperty("_BaseColor"))mat.SetColor("_BaseColor",source.GetColor("_BaseColor"));
                            else if(source.HasProperty("_Color"))mat.SetColor("_BaseColor",source.GetColor("_Color"));
                            string p=Stage+"/material-"+assetNumber+++".mat";
                            AssetDatabase.CreateAsset(mat,p);
                            mats[i]=mat;
                        }
                        renderer.sharedMaterials=mats;
                    }
                    PrefabUtility.SaveAsPrefabAsset(item,Stage+"/"+item.name+".prefab");
                }
                AssetDatabase.SaveAssets();
                EditorSceneManager.SaveScene(scene,Stage+"/ValoriaLibraryV1Stage.unity");
                File.WriteAllText("ValoriaLibraryV1Proof/metrics.json",
                    "{\"models\":3,\"persisted_asset_count\":"+assetNumber+",\"art_pass\":false,\"mobile_tested\":false}");
                EditorApplication.Exit(0);
            }
            catch(Exception ex){ Debug.LogException(ex); EditorApplication.Exit(1);}
        }

        public static void Reopen()
        {
            try
            {
                EditorSceneManager.OpenScene(Stage+"/ValoriaLibraryV1Stage.unity",OpenSceneMode.Single);
                foreach(var name in Models)
                    if(GameObject.Find(name)==null)throw new Exception("Missing staged asset "+name);
                foreach(var mf in UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                    if(mf.transform.root.name=="Valoria Library V1 isolated composition" && mf.sharedMesh!=null && !AssetDatabase.Contains(mf.sharedMesh))
                        throw new Exception("Nonpersistent geometry "+mf.name);
                EditorApplication.Exit(0);
            }
            catch(Exception ex){ Debug.LogException(ex); EditorApplication.Exit(1);}
        }
    }
}
