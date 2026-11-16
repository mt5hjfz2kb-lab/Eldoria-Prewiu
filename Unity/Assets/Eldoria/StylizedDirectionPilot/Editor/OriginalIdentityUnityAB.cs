#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using GLTFast;
using GLTFast.Loading;
using Eldoria.ReferenceClean;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Eldoria.StylizedDirectionPilot.Editor
{
    /// <summary>
    /// Real Unity 6 / URP A/B, using a whole new Blender-authored GLB (B)
    /// versus separately staged current production B5 prefabs (A).
    /// This does NOT touch gameplay or replace any production prefab.
    /// A/B is in the original integrated camera, not an artist-created render.
    /// </summary>
    public static class OriginalIdentityUnityAB
    {
        const string Scene = "Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
        const string Import = "StagedOriginalBastionV/valoria-original-bastion-v.glb";
        static readonly string[] OldIds={"bastion-v","aserradero-v","cuartel-v","granero-v","cantera-v"};
        static readonly float[] Angles={235f,313f,231f,306f,318f};
        [Serializable] sealed class Proof
        {
            public string kind="NATIVE_UNITY_EDITOR_A_B_EXISTING_B5_VERSUS_FULLY_ORIGINAL_NEW_GLB";
            public string unityScene=Scene,artSourceSha,gitSourceSha;
            public int stagedLegacyPrefabs,importedOriginalMeshRenderers;
            public bool originalModelSourceImported=false; // referring to the new B
            public bool originalPrefabModified=false,gameplayStateModified=false;
            public bool sceneSerialized=false,artApproved=false,iphonePhysicalTested=false;
            public bool actualRuntimeHUDValidated=false;
        }
        public static async void Capture()
        {
            var result=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../OriginalIdentityUnityProof"));
            Directory.CreateDirectory(result);
            GameObject originalRoot=null,newRoot=null;
            var disabledNodes=new Dictionary<GameObject,bool>();
            var priorRenderers=new Dictionary<Renderer,bool>();
            var terrains=new Dictionary<Terrain,bool>();
            var camera=null as Camera;
            Vector3 cameraPos=Vector3.zero;
            Quaternion cameraRot=Quaternion.identity;
            float cameraOrtho=0;
            try
            {
                var fullGlb=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../"+Import));
                if(!File.Exists(fullGlb) || new FileInfo(fullGlb).Length<120000)
                    throw new FileNotFoundException("No valid Blender-generated ORIGINAL GLB was staged",fullGlb);
                EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);
                var city=Object.FindFirstObjectByType<ValoriaReferenceCleanGameplay>(FindObjectsInactive.Include);
                if(!city||!city.PlayerCamera||!city.BastionTwo||!city.Parcels)
                    throw new InvalidOperationException("Canonical playable city camera and build anchors absent");
                camera=city.PlayerCamera;
                cameraPos=camera.transform.position;cameraRot=camera.transform.rotation;
                cameraOrtho=camera.orthographicSize;
                if(!camera.orthographic)throw new InvalidOperationException("Expected real strategy ortho camera");
                // Original B5 old assets are *comparison only*: no color/geometry edits.
                var b2=city.BastionTwo;
                var saw=city.Parcels.LeftBuilt;var barr=city.Parcels.RightBuilt;
                foreach(var node in new[]{b2,saw,barr})
                {
                    disabledNodes[node]=node.activeSelf;
                    node.SetActive(false);
                }
                var keep=Bounds(b2),work=Bounds(saw),army=Bounds(barr);
                var anchors=new[]{
                    new Vector3(keep.center.x,keep.min.y,keep.center.z),
                    new Vector3(work.center.x,work.min.y,work.center.z),
                    new Vector3(army.center.x,army.min.y,army.center.z),
                    new Vector3(-19f,.56f,9f),new Vector3(19f,.56f,9f)
                };
                var widths=new[]{
                    Mathf.Max(keep.size.x,keep.size.z),
                    Mathf.Max(work.size.x,work.size.z),
                    Mathf.Max(army.size.x,army.size.z),
                    10.4f,11f
                };
                originalRoot=new GameObject("A / Existing production Bastion V prefabs only");
                originalRoot.hideFlags=HideFlags.DontSaveInEditor|HideFlags.DontSaveInBuild;
                int matched=0;
                for(int i=0;i<OldIds.Length;i++)
                {
                    var asset=Resources.Load<GameObject>("BastionIVV/Models/"+OldIds[i]);
                    if(!asset)throw new FileNotFoundException("Original stage V comparison prefab absent",OldIds[i]);
                    var model=Object.Instantiate(asset,originalRoot.transform);
                    ActivateFirstLOD(model);
                    Fit(model,anchors[i],widths[i]);
                    var b=Bounds(model);
                    model.transform.RotateAround(new Vector3(b.center.x,b.min.y,b.center.z),Vector3.up,Angles[i]);
                    if(i==0)
                    {
                        var gate=model.transform.TransformPoint(new Vector3(-.327f,-.39831066f,.286f));
                        model.transform.position+=new Vector3(keep.center.x-gate.x,0,keep.min.z+.5f-gate.z);
                    }
                    matched++;
                }
                Screen(camera,result,"A-main-bastion-v");

                // B replaces ALL visible model/terrain renderers with the new authored scene.
                // Unchanged camera angle/position/scale and scene lighting give honest A/B.
                foreach(var renderer in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                {
                    if(!renderer)continue;
                    priorRenderers[renderer]=renderer.enabled;
                    renderer.enabled=false;
                }
                foreach(var terr in Object.FindObjectsByType<Terrain>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                {
                    terrains[terr]=terr.drawHeightmap;
                    terr.drawHeightmap=false;
                }
                originalRoot.SetActive(false);
                newRoot=new GameObject("B / ORIGINAL new Eldoria stylized city");
                newRoot.hideFlags=HideFlags.DontSaveInEditor|HideFlags.DontSaveInBuild;
                var loader=new GltfImport(null,new UninterruptedDeferAgent());
                try
                {
                    if(!await loader.Load(new Uri(fullGlb).AbsoluteUri))
                        throw new InvalidOperationException("glTFast could not import the NEW source GLB");
                    if(!await loader.InstantiateMainSceneAsync(newRoot.transform))
                        throw new InvalidOperationException("glTFast could not instantiate NEW source geometry");
                }
                finally {loader.Dispose();}
                var renderers=newRoot.GetComponentsInChildren<Renderer>(true);
                if(renderers.Length<50)throw new InvalidOperationException("New city GLB contains insufficient physical meshes");
                var artBounds=Bounds(newRoot);
                float width=Mathf.Max(artBounds.size.x,artBounds.size.z);
                if(width<65 || width>160)throw new InvalidOperationException("Original artwork has unexpected world scale "+width);
                // All geometry is from the new GLB. Align its river-valley centre to the
                // authentic city centre without moving the camera or staging any old props.
                var mainFocus=new Vector3(keep.center.x,keep.min.y,keep.center.z);
                var artFoot=new Vector3(artBounds.center.x,artBounds.min.y,artBounds.center.z);
                newRoot.transform.position+=mainFocus-artFoot;
                Screen(camera,result,"B-completely-new-bastion-v");
                var evidence=new Proof{
                    artSourceSha=Environment.GetEnvironmentVariable("ORIGINAL_ART_SHA")??"artifact-unpinned",
                    gitSourceSha=Environment.GetEnvironmentVariable("GITHUB_SHA")??"local-unpinned",
                    stagedLegacyPrefabs=matched,importedOriginalMeshRenderers=renderers.Length
                };
                File.WriteAllText(Path.Combine(result,"native-original-bastion-v-proof.json"),
                    JsonUtility.ToJson(evidence,true));
                Debug.Log("ORIGINAL_ART_BASTION_V_UNITY_A_B_CAPTURE_OK");
                EditorApplication.Exit(0);
            }
            catch(Exception e)
            {
                File.WriteAllText(Path.Combine(result,"native-error.txt"),e.ToString());
                Debug.LogException(e);
                EditorApplication.Exit(1);
            }
            finally
            {
                // The source scene is never serialized or saved.
                if(newRoot)Object.DestroyImmediate(newRoot);
                if(originalRoot)Object.DestroyImmediate(originalRoot);
                foreach(var pair in priorRenderers)if(pair.Key)pair.Key.enabled=pair.Value;
                foreach(var pair in terrains)if(pair.Key)pair.Key.drawHeightmap=pair.Value;
                foreach(var pair in disabledNodes)if(pair.Key)pair.Key.SetActive(pair.Value);
                if(camera){camera.transform.SetPositionAndRotation(cameraPos,cameraRot);camera.orthographicSize=cameraOrtho;}
            }
        }
        static void ActivateFirstLOD(GameObject go)
        {
            go.SetActive(true);
            foreach(var r in go.GetComponentsInChildren<Renderer>(true))
            {
                for(var t=r.transform;t&&t!=go.transform.parent;t=t.parent)t.gameObject.SetActive(true);
                r.enabled=true;
            }
            foreach(var group in go.GetComponentsInChildren<LODGroup>(true))
            {
                var lv=group.GetLODs();
                if(lv.Length==0)continue;
                var active=new HashSet<Renderer>(lv[0].renderers.Where(r=>r));
                group.enabled=false;
                foreach(var r in group.GetComponentsInChildren<Renderer>(true))r.enabled=active.Contains(r);
            }
            foreach(var collider in go.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(collider);
        }
        static Bounds Bounds(GameObject go)
        {
            var rs=go.GetComponentsInChildren<Renderer>(true).Where(r=>r).ToArray();
            if(rs.Length==0)throw new InvalidOperationException("3D source has no renderer: "+go.name);
            var b=rs[0].bounds;
            foreach(var r in rs.Skip(1))b.Encapsulate(r.bounds);
            return b;
        }
        static void Fit(GameObject root,Vector3 point,float width)
        {
            root.transform.localPosition=Vector3.zero;
            root.transform.localScale=Vector3.one;
            root.transform.localRotation=Quaternion.identity;
            var b=Bounds(root);
            root.transform.localScale*=width/Mathf.Max(.01f,Mathf.Max(b.size.x,b.size.z));
            b=Bounds(root);
            root.transform.position+=point-new Vector3(b.center.x,b.min.y,b.center.z);
        }
        static void Screen(Camera camera,string destination,string prefix)
        {
            Capture(camera,Path.Combine(destination,prefix+"-1600x900.png"),1600,900);
            Capture(camera,Path.Combine(destination,prefix+"-390x844.png"),390,844);
        }
        static void Capture(Camera camera,string file,int width,int height)
        {
            var target=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);
            var tex=new Texture2D(width,height,TextureFormat.RGB24,false);
            var old=RenderTexture.active;var current=camera.targetTexture;
            var aspect=camera.aspect;bool enabled=camera.enabled;
            try
            {
                camera.enabled=true;camera.aspect=width/(float)height;
                camera.targetTexture=target;camera.Render();RenderTexture.active=target;
                tex.ReadPixels(new Rect(0,0,width,height),0,0);
                tex.Apply(false,false);
                var bytes=tex.EncodeToPNG();
                if(bytes.Length<30000)throw new InvalidOperationException("Blank real Unity camera capture: "+file);
                File.WriteAllBytes(file,bytes);
            }
            finally
            {
                camera.targetTexture=current;camera.aspect=aspect;camera.enabled=enabled;
                RenderTexture.active=old;target.Release();
                Object.DestroyImmediate(target);Object.DestroyImmediate(tex);
            }
        }
    }
}
#endif
