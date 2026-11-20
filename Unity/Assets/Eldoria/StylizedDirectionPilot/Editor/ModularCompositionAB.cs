#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eldoria.ReferenceClean;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Eldoria.StylizedDirectionPilot.Editor
{
    /// <summary>
    /// One bounded macro A/B: existing world geometry + existing modular ruins,
    /// rocks and forest, composed in the genuine serialized Valoria scene.
    /// Nothing serialized, no player state loaded, no gameplay colliders.
    /// This is a production hypothesis, NOT a shipping scene nor a mobile test.
    /// </summary>
    public static class ModularCompositionAB
    {
        const string Scene="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
        static readonly List<string> Audit=new List<string>();
        public static void Capture()
        {
            string output=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../StylizedDirectionPilotCaptures"));
            Directory.CreateDirectory(output);
            try
            {
                EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);
                var city=Object.FindFirstObjectByType<ValoriaReferenceCleanGameplay>(FindObjectsInactive.Include);
                if(!city||!city.PlayerCamera)throw new InvalidOperationException("Canonical city camera is missing");
                city.Apply(new Eldoria.Domain.PlayerState{BastionLevel=2,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true});
                var terrain=Object.FindFirstObjectByType<Terrain>(FindObjectsInactive.Include);
                if(!terrain)throw new InvalidOperationException("Actual Unity Terrain missing");
                var camera=city.PlayerCamera;
                var originalIds=Audit.Select(x=>x).ToArray();
                Save(camera,Path.Combine(output,"A-composition-baseline-1600x900.png"),1600,900);
                Save(camera,Path.Combine(output,"A-composition-baseline-390x844.png"),390,844);
                using(var composed=new TemporaryDiorama(city,terrain,camera))
                {
                    if(composed.AddedCount<15)throw new InvalidOperationException("Diorama additions not substantial");
                    Save(camera,Path.Combine(output,"D-modular-environment-1600x900.png"),1600,900);
                    Save(camera,Path.Combine(output,"D-modular-environment-390x844.png"),390,844);
                    File.WriteAllText(Path.Combine(output,"D-modular-composition-evidence.json"),JsonUtility.ToJson(
                        new Evidence{kind="UNITY_EDITOR_REAL_SCENE_MODULAR_COMPOSITION_NOT_GAMEPLAY",
                            sourceSha=Environment.GetEnvironmentVariable("GITHUB_SHA")??"local",addedMeshes=composed.AddedCount,
                            artApproved=false,mobileValidated=false},true));
                }
                File.WriteAllLines(Path.Combine(output,"D-modular-composition-audit.txt"),Audit);
                if(new FileInfo(Path.Combine(output,"D-modular-environment-1600x900.png")).Length<60000)
                    throw new InvalidOperationException("Diorama screenshot blank");
                Debug.Log("ELDORIA_MODULAR_COMPOSITION_REAL_UNITY_EDITOR_RENDER_COMPLETE");
                EditorApplication.Exit(0);
            }
            catch(Exception e)
            {
                File.WriteAllText(Path.Combine(output,"D-modular-composition-error.txt"),e.ToString());
                Debug.LogException(e);
                EditorApplication.Exit(1);
            }
        }
        [Serializable]
        sealed class Evidence
        {
            public string kind,sourceSha;
            public int addedMeshes;
            public bool artApproved,mobileValidated;
        }
        sealed class TemporaryDiorama : IDisposable
        {
            readonly Terrain terrain;
            readonly Camera cam;
            readonly GameObject root;
            readonly Light[] lights;
            readonly Color ambient;
            readonly AmbientMode mode;
            readonly Dictionary<Light,Color> originalLightColors=new Dictionary<Light,Color>();
            readonly Dictionary<Light,float> originalIntensities=new Dictionary<Light,float>();
            public int AddedCount {get;private set;}
            public TemporaryDiorama(ValoriaReferenceCleanGameplay city,Terrain terr,Camera camera)
            {
                terrain=terr;cam=camera;
                root=new GameObject("Art direction temporary macro composition / never serialize");
                root.hideFlags=HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                ambient=RenderSettings.ambientLight;mode=RenderSettings.ambientMode;
                RenderSettings.ambientMode=AmbientMode.Flat;
                RenderSettings.ambientLight=new Color(.61f,.66f,.66f);
                lights=Object.FindObjectsByType<Light>(FindObjectsSortMode.None)
                    .Where(l=>l&&l.gameObject.scene==city.gameObject.scene&&l.type==LightType.Directional).ToArray();
                foreach(var l in lights)
                {
                    originalLightColors[l]=l.color;originalIntensities[l]=l.intensity;
                    l.color=new Color(1f,.89f,.77f);l.intensity*=.87f;
                }
                BuildLandscape();
            }
            void BuildLandscape()
            {
                var all=Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None);
                // Choose copies already designed for this project's own world.
                var tree=all.FirstOrDefault(t=>t&&(t.name.StartsWith("Existing Holotna tree.")||t.name.StartsWith("Forest retained Valoria pine"))&&
                    t.GetComponentInChildren<MeshRenderer>(true)!=null);
                var rock=all.FirstOrDefault(t=>t&&(t.name.StartsWith("Existing Holotna rock.")||t.name.StartsWith("Rock01_LOD0"))&&
                    t.GetComponentInChildren<MeshRenderer>(true)!=null);
                var ruin=all.FirstOrDefault(t=>t&&(t.name=="Arch_Gothic"||t.name.StartsWith("Ruin_-22_46"))&&
                    t.GetComponentInChildren<MeshRenderer>(true)!=null);
                if(tree==null||rock==null)throw new InvalidOperationException("Retained city vegetation/rock donors missing");
                // Border clusters: open Bastion, pathways and future building area untouched.
                var clusters=new[]{new Vector2(.07f,.84f),new Vector2(.19f,.80f),
                    new Vector2(.80f,.88f),new Vector2(.92f,.80f),
                    new Vector2(.10f,.53f),new Vector2(.88f,.54f),
                    new Vector2(.84f,.34f)};
                int index=0;
                foreach(var centre in clusters)
                {
                    int n=centre.y<.40f?3:4;
                    for(int k=0;k<n;k++)
                    {
                        float dx=((k%3)-1)*.028f+(k%2)*.008f;
                        float dy=((k/3)-.5f)*.028f;
                        Place(tree,new Vector2(Mathf.Clamp01(centre.x+dx),Mathf.Clamp01(centre.y+dy)),
                            .67f + ((index*7)%11)*.043f,index*73f,"fir "+index);
                        index++;
                    }
                }
                // Rock/boulder groupings connect land to shoreline and paths;
                // every placement reuses actual authored rock textures.
                var banks=new[]{new Vector2(.12f,.34f),new Vector2(.25f,.23f),
                    new Vector2(.77f,.13f),new Vector2(.89f,.19f),
                    new Vector2(.66f,.63f),new Vector2(.25f,.57f)};
                for(int cluster=0;cluster<banks.Length;cluster++)
                {
                    for(int i=0;i<3;i++)
                    {
                        Vector2 at=banks[cluster]+new Vector2(i*.014f-.016f,(i%2)*.019f-.013f);
                        Place(rock,at,.65f+i*.17f,cluster*46f+i*75f,"rock "+cluster+"-"+i);
                    }
                }
                // Signature ancient landmark: one existing sculpted arch/ruin,
                // never the rejected legacy Bastion nor a new housing district.
                if(ruin!=null)Place(ruin,new Vector2(.32f,.86f),1.43f,18f,"distant ancient ruin");
            }
            void Place(Transform source,Vector2 viewport,float scale,float angle,string role)
            {
                if(!source)return;
                var point=AtGround(viewport);
                if(!terrain.terrainData.bounds.Contains(terrain.transform.InverseTransformPoint(point)))
                    return;
                var replica=Object.Instantiate(source.gameObject);
                replica.name="ART PILOT ONLY / "+role+" / "+source.name;
                replica.transform.SetParent(root.transform,true);
                replica.SetActive(true);
                replica.transform.rotation=source.rotation*Quaternion.Euler(0f,angle,0f);
                replica.transform.localScale=Vector3.Scale(source.lossyScale,Vector3.one*scale);
                replica.transform.position=point;
                foreach(var component in replica.GetComponentsInChildren<MonoBehaviour>(true))
                    component.enabled=false;
                foreach(var collider in replica.GetComponentsInChildren<Collider>(true))
                    collider.enabled=false;
                foreach(var renderer in replica.GetComponentsInChildren<Renderer>(true))
                {
                    renderer.enabled=true;
                    renderer.shadowCastingMode=ShadowCastingMode.On;
                }
                var renderers=replica.GetComponentsInChildren<Renderer>();
                if(renderers.Length>0)
                {
                    float low=renderers.Min(r=>r.bounds.min.y);
                    replica.transform.position+=Vector3.up*(point.y-low);
                    AddedCount+=renderers.Length;
                }
                Audit.Add(role+" source="+source.name+" XYZ="+replica.transform.position+" scale="+scale);
            }
            Vector3 AtGround(Vector2 view)
            {
                var ray=cam.ViewportPointToRay(new Vector3(view.x,view.y,0));
                var plane=new Plane(Vector3.up,terrain.transform.position+Vector3.up*3f);
                if(!plane.Raycast(ray,out float distance))throw new InvalidOperationException("World camera cannot see grass plane");
                var hit=ray.GetPoint(distance);
                return new Vector3(hit.x,terrain.SampleHeight(hit)+terrain.transform.position.y,hit.z);
            }
            public void Dispose()
            {
                foreach(var l in lights)if(l)
                {
                    l.color=originalLightColors[l];l.intensity=originalIntensities[l];
                }
                RenderSettings.ambientMode=mode;RenderSettings.ambientLight=ambient;
                if(root)Object.DestroyImmediate(root);
            }
        }
        static void Save(Camera camera,string path,int w,int h)
        {
            var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32);
            var tex=new Texture2D(w,h,TextureFormat.RGB24,false);
            var old=camera.targetTexture;
            var active=RenderTexture.active;
            bool wasEnabled=camera.enabled;
            float aspect=camera.aspect,size=camera.orthographicSize;
            try
            {
                camera.enabled=true;
                camera.aspect=w/(float)h;
                if(h>w)camera.orthographicSize=size*1.18f;
                camera.targetTexture=rt;
                camera.Render();
                RenderTexture.active=rt;
                tex.ReadPixels(new Rect(0,0,w,h),0,0);tex.Apply(false,false);
                var bytes=tex.EncodeToPNG();
                if(bytes.Length<30000)throw new InvalidOperationException("Empty Unity city proof "+path);
                File.WriteAllBytes(path,bytes);
            }
            finally
            {
                camera.targetTexture=old;camera.aspect=aspect;camera.orthographicSize=size;camera.enabled=wasEnabled;
                RenderTexture.active=active;rt.Release();Object.DestroyImmediate(rt);Object.DestroyImmediate(tex);
            }
        }
    }
}
#endif