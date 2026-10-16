using System;
using System.Collections.Generic;
using System.IO;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Eldoria.EditorTools
{
    // Native render lookdev on the historical playable Bastion I source, not on
    // new meshes and never on the canonical or public scene.
    public static class Bastion1UnityEnvironmentReview
    {
        const string Original = "Assets/Eldoria/ProductionSlice/Runtime/Valoria.unity";
        const string Candidate = "Assets/Eldoria/ArtTests/Bastion1Assembly/Bastion1UnityEnvironmentCandidate.unity";
        const string SavedMaterialFolder = "Assets/Eldoria/ArtTests/Bastion1Assembly/GeneratedUnityEnvironment";
        const string Evidence = "Bastion1UnityEnvironmentAB";
        static readonly Dictionary<Material,Material> MaterialCopies = new Dictionary<Material,Material>();

        [MenuItem("Eldoria/Art/Review Bastion I Unity-only lighting and terrain")]
        public static void Capture()
        {
            try
            {
                if(!File.Exists(Original)) throw new FileNotFoundException("Native playable source missing",Original);
                Directory.CreateDirectory(Evidence);
                var scene=EditorSceneManager.OpenScene(Original,OpenSceneMode.Single);
                var presentation=UnityEngine.Object.FindFirstObjectByType<ValoriaParcelPresentation>();
                if(presentation==null||!presentation.MeshMode||presentation.ProductionCamera==null||
                   presentation.Bindings==null||presentation.Bindings.Length<2||
                   presentation.Bindings[0].Ground==null||
                   presentation.Bindings[0].Construction==null||
                   presentation.Bindings[0].Built==null||
                   presentation.Bindings[1].Built==null)
                    throw new Exception("Existing Bastion I and II playable state must remain intact");
                var camera=presentation.ProductionCamera;
                var pos=camera.transform.position;
                var rotation=camera.transform.rotation;
                float size=camera.orthographicSize;
                CaptureThree(camera,"before",size);

                // A deliberately focused Unity-only hypothesis, not a proven improvement.
                // Never alter glTF original materials: clone and persist new instances.
                var touched=new List<string>();
                foreach(var renderer in UnityEngine.Object.FindObjectsByType<Renderer>(
                    FindObjectsInactive.Include,FindObjectsSortMode.None))
                {
                    var values=renderer.sharedMaterials;
                    bool modified=false;
                    for(int i=0;i<values.Length;i++)
                    {
                        var source=values[i];
                        if(source==null)continue;
                        var name=source.name.ToLowerInvariant();
                        if(!(name.Contains("family soil portable") ||
                             name.Contains("terrace earth olive grass")))continue;
                        if(!MaterialCopies.TryGetValue(source,out var copy))
                        {
                            copy=new Material(source){name="BastionI_UnityOnlyWarmEarth_"+MaterialCopies.Count};
                            Color c=copy.GetColor("_BaseColor");
                            copy.SetColor("_BaseColor",new Color(
                                Mathf.Clamp01(c.r*1.13f),Mathf.Clamp01(c.g*1.12f),
                                Mathf.Clamp01(c.b*0.95f),c.a));
                            MaterialCopies.Add(source,copy);
                            touched.Add(source.name);
                        }
                        values[i]=copy;modified=true;
                    }
                    if(modified)renderer.sharedMaterials=values;
                }
                if(touched.Count==0)throw new Exception("No existing Unity terrain material matched the source");
                var sun=FindDirectionalLight();
                if(sun==null)throw new Exception("Existing sun missing; no substitute geometry or new scene light created");
                sun.color=new Color(1f,.89f,.77f);
                sun.intensity=1.57f;
                sun.shadows=LightShadows.Soft;
                RenderSettings.ambientMode=AmbientMode.Trilight;
                RenderSettings.ambientSkyColor=new Color(.46f,.53f,.63f);
                RenderSettings.ambientEquatorColor=new Color(.49f,.48f,.41f);
                RenderSettings.ambientGroundColor=new Color(.39f,.34f,.28f);
                // Keep source depth falloff nearly unchanged; older high-fog
                // experiments washed the bastion out and were visually rejected.
                RenderSettings.fog=true;
                RenderSettings.fogMode=FogMode.ExponentialSquared;
                RenderSettings.fogDensity=.0024f;
                RenderSettings.fogColor=new Color(.35f,.42f,.49f);

                if(Vector3.Distance(pos,camera.transform.position)>.0001f||
                    Quaternion.Angle(rotation,camera.transform.rotation)>.0001f||
                    Mathf.Abs(size-camera.orthographicSize)>.0001f)
                    throw new Exception("Review changed locked strategic camera");
                PersistMaterialCopies();
                if(!EditorSceneManager.SaveScene(scene,Candidate,true))
                    throw new Exception("Isolated render candidate scene could not be saved");
                CaptureThree(camera,"after",size);
                string names=String.Join(",",touched.ConvertAll(s=>"\"" + s.Replace("\"","") + "\""));
                File.WriteAllText(Path.Combine(Evidence,"review.json"),
                    "{\n  \"schema\": \"eldoria-bastion-unity-environment-ab-v1\",\n"+
                    "  \"source_unchanged\": true,\n  \"new_meshes\": 0,\n"+
                    "  \"gameplay_changes\": false,\n  \"art_approved\": false,\n"+
                    "  \"terrain_materials\": ["+names+"],\n"+
                    "  \"candidate\": \""+Candidate+"\"\n}\n");
                Debug.Log("BASTION1_UNITY_ENVIRONMENT_AB_COMPLETE materials="+MaterialCopies.Count+
                    " same_camera=true source_unchanged=true art_approved=false");
                EditorApplication.Exit(0);
            }
            catch(Exception ex)
            {
                Debug.LogError("BASTION1_UNITY_ENVIRONMENT_AB_FAILED "+ex);
                EditorApplication.Exit(1);
            }
        }
        static Light FindDirectionalLight()
        {
            foreach(var light in UnityEngine.Object.FindObjectsByType<Light>(
                FindObjectsInactive.Include,FindObjectsSortMode.None))
                if(light.type==LightType.Directional && light.gameObject.name.Contains("afternoon sun"))
                    return light;
            return null;
        }
        static void CaptureThree(Camera camera,string stage,float homeSize)
        {
            Bastion1ReferenceCapture.Render(camera,Path.GetFullPath(Evidence+"/"+stage+"-landscape.png"),1440,1000);
            Bastion1ReferenceCapture.Render(camera,Path.GetFullPath(Evidence+"/"+stage+"-mobile.png"),780,520);
            camera.orthographicSize=76f;
            try
            {
                Bastion1ReferenceCapture.Render(camera,Path.GetFullPath(Evidence+"/"+stage+"-portrait.png"),390,844);
            }
            finally {camera.orthographicSize=homeSize;}
        }
        static void PersistMaterialCopies()
        {
            Directory.CreateDirectory(SavedMaterialFolder);
            int id=0;
            foreach(var pair in MaterialCopies)
            {
                string path=SavedMaterialFolder+"/material_"+id.ToString("D3")+".mat";
                if(AssetDatabase.LoadMainAssetAtPath(path)!=null)AssetDatabase.DeleteAsset(path);
                AssetDatabase.CreateAsset(pair.Value,path);
                id++;
            }
            AssetDatabase.SaveAssets();
        }
    }
}
