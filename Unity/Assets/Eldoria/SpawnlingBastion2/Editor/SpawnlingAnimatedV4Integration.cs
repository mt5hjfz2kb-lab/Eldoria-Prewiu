#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Eldoria.ReferenceClean;
using Eldoria.Presentation;
using Eldoria.World4XPrototype;
using GLTFast;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;

namespace Eldoria.SpawnlingBastion2.Editor
{
    // Explicit reviewed integration only; no InitializeOnLoad or automatic scene writes.
    public static class SpawnlingAnimatedV4Integration
    {
        const string SourceSha="eef82ab48b8e512a39b3f87b5b282f5bc493ec119a7e21c2149a2437c1448128";
        const string ScenePath="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
        const string ApprovalPath="../docs/evidence/spawnling-animation-v4-review-20261021/unity-approved.json";
        const string Output="../docs/evidence/valoria-reference-clean/spawnling-v4-integration";
        [Serializable] class Approval { public bool approved;public string source_sha256;public string reviewer; }
        [Serializable] class Report {
            public string status,source_sha256,scene_before,scene_after,clip;
            public int bones,near_triangles,far_triangles,hotspots,colliders,new_textures,reused_textures;
            public float duration;
            public bool original_hotspots_preserved,gameplay_unchanged,canonical_save_untouched;
        }
        static void Need(bool ok,string message){if(!ok)throw new InvalidOperationException("V4 integration: "+message);}
        static string Hash(string path){using(var h=SHA256.Create())return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(path))).Replace("-","").ToLowerInvariant();}
        static string PixelHash(Texture2D texture){using(var h=SHA256.Create())return BitConverter.ToString(h.ComputeHash(texture.GetRawTextureData())).Replace("-","");}
        static Bounds BoundsOf(Renderer[] renderers){
            Need(renderers.Length>0,"Renderer set empty");var b=renderers[0].bounds;
            foreach(var r in renderers.Skip(1))b.Encapsulate(r.bounds);return b;
        }
        static Texture2D CopyTexture(Texture texture){
            bool linear=!UnityEngine.Experimental.Rendering.GraphicsFormatUtility.IsSRGBFormat(texture.graphicsFormat);
            var rt=RenderTexture.GetTemporary(texture.width,texture.height,0,RenderTextureFormat.ARGB32,
                linear?RenderTextureReadWrite.Linear:RenderTextureReadWrite.sRGB);
            var previous=RenderTexture.active;
            try {
                Graphics.Blit(texture,rt);RenderTexture.active=rt;
                var copy=new Texture2D(texture.width,texture.height,TextureFormat.RGBA32,true,linear);
                copy.wrapMode=texture.wrapMode;copy.filterMode=FilterMode.Trilinear;copy.anisoLevel=2;
                copy.ReadPixels(new Rect(0,0,texture.width,texture.height),0,0);copy.Apply(true,false);return copy;
            } finally {RenderTexture.active=previous;RenderTexture.ReleaseTemporary(rt);}
        }
        static void PersistSkin(GameObject root,string folder,Renderer[] retained,ref int added,ref int reused)
        {
            int n=0;var meshes=new Dictionary<Mesh,Mesh>();
            foreach(var skin in root.GetComponentsInChildren<SkinnedMeshRenderer>(true)) {
                var source=skin.sharedMesh;Need(source&&source.bindposes.Length==skin.bones.Length,"Skin bindposes invalid");
                if(!meshes.TryGetValue(source,out var saved)) {
                    saved=Object.Instantiate(source);saved.name="Spawnling V4 / retained skin";
                    AssetDatabase.CreateAsset(saved,folder+"/skin-"+n+++".asset");meshes.Add(source,saved);
                }
                skin.sharedMesh=saved;
            }
            var originalTextures=retained.SelectMany(r=>r.sharedMaterials).Where(m=>m)
                .SelectMany(m=>m.GetTexturePropertyNames().Select(p=>m.GetTexture(p))).Where(t=>t).Distinct().ToArray();
            var reusable=new Dictionary<string,Texture>();
            foreach(var texture in originalTextures) {
                var copy=CopyTexture(texture);
                try {string key=texture.width+"x"+texture.height+":"+copy.graphicsFormat+":"+PixelHash(copy);
                    if(!reusable.ContainsKey(key))reusable.Add(key,texture);}
                finally {Object.DestroyImmediate(copy);}
            }
            var textures=new Dictionary<Texture,Texture>();var materials=new Dictionary<Material,Material>();
            foreach(var renderer in root.GetComponentsInChildren<Renderer>(true)) {
                var assigned=renderer.sharedMaterials;
                for(int i=0;i<assigned.Length;i++) {
                    var source=assigned[i];Need(source&&source.shader&&!source.shader.name.Contains("InternalError"),"Material shader invalid");
                    if(!materials.TryGetValue(source,out var saved)) {
                        saved=new Material(source){name="Spawnling V4 / "+source.name};
                        foreach(string property in source.GetTexturePropertyNames()) {
                            var texture=source.GetTexture(property);if(!texture)continue;
                            if(!textures.TryGetValue(texture,out var stored)) {
                                var copy=CopyTexture(texture);
                                string key=texture.width+"x"+texture.height+":"+copy.graphicsFormat+":"+PixelHash(copy);
                                if(reusable.TryGetValue(key,out stored)){Object.DestroyImmediate(copy);reused++;}
                                else {stored=copy;AssetDatabase.CreateAsset(copy,folder+"/texture-"+n+++".asset");added++;}
                                textures.Add(texture,stored);
                            }
                            saved.SetTexture(property,stored);
                        }
                        AssetDatabase.CreateAsset(saved,folder+"/material-"+n+++".mat");materials.Add(source,saved);
                    }
                    assigned[i]=saved;
                }
                renderer.sharedMaterials=assigned;renderer.lightmapIndex=-1;
                renderer.shadowCastingMode=ShadowCastingMode.On;renderer.receiveShadows=true;
                GameObjectUtility.SetStaticEditorFlags(renderer.gameObject,0);
            }
        }
        static Task Frame() {
            var ready=new TaskCompletionSource<bool>();int count=0;
            void Tick(){EditorApplication.QueuePlayerLoopUpdate();if(++count<3)return;EditorApplication.update-=Tick;ready.SetResult(true);}
            EditorApplication.update+=Tick;EditorApplication.QueuePlayerLoopUpdate();return ready.Task;
        }
        static void Capture(Camera camera,Bounds bounds,string filename,float zoom,int width,int height){
            var position=camera.transform.position;var size=camera.orthographicSize;var aspect=camera.aspect;
            var target=camera.targetTexture;var previous=RenderTexture.active;
            var rt=RenderTexture.GetTemporary(width,height,24,RenderTextureFormat.ARGB32);
            var image=new Texture2D(width,height,TextureFormat.RGB24,false);
            try {
                camera.orthographicSize=zoom;camera.aspect=width/(float)height;
                camera.transform.position=bounds.center-camera.transform.forward*90f;camera.targetTexture=rt;
                camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();
                File.WriteAllBytes(Path.Combine(Output,filename),image.EncodeToPNG());
            } finally {
                camera.targetTexture=target;camera.orthographicSize=size;camera.aspect=aspect;camera.transform.position=position;
                RenderTexture.active=previous;RenderTexture.ReleaseTemporary(rt);Object.DestroyImmediate(image);
            }
        }
        public static async void Integrate()
        {
            Directory.CreateDirectory(Output);
            try {
                Need(File.Exists(ApprovalPath),"Independent actual Unity image acceptance missing");
                var approval=JsonUtility.FromJson<Approval>(File.ReadAllText(ApprovalPath));
                Need(approval.approved&&approval.source_sha256==SourceSha&&!string.IsNullOrEmpty(approval.reviewer),
                    "Pinned V4 source not independently accepted");
                string input=Environment.GetEnvironmentVariable("ELDORIA_V4_INTEGRATION_INPUT");
                Need(File.Exists(input)&&Hash(input)==SourceSha,"Reviewed GLB bytes mismatch");
                string before=Hash(ScenePath);
                var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
                var host=Object.FindFirstObjectByType<ValoriaIntegratedWorld>();
                Need(host&&host.WorldRoot&&host.WorldCamera,"Canonical world missing");
                var actor=host.WorldRoot.GetComponentsInChildren<SpawnlingEncounterVisual>(true).Single();
                Need(actor&&actor.ModelRoot&&actor.MotionPivot&&!actor.SkeletalAnimation,"Existing static actor missing or V4 already installed");
                var lod=actor.GetComponent<LODGroup>();Need(lod,"Existing mobile LOD group missing");
                var levels=lod.GetLODs();Need(levels.Length==2,"Retain original near/far LOD contract");
                var oldNear=levels[0].renderers;var oldFar=levels[1].renderers;
                Need(oldNear.All(r=>r)&&oldFar.All(r=>r),"Existing LOD renderers missing");
                var oldBounds=BoundsOf(oldNear);
                var spots=host.WorldRoot.GetComponentsInChildren<WorldHotspot>(true);
                var identities=spots.Select(h=>h.Id+"="+h.transform.position.ToString("R")).OrderBy(x=>x).ToArray();
                int colliders=host.WorldRoot.GetComponentsInChildren<Collider>(true).Length;
                bool wasActive=host.WorldRoot.activeSelf;host.WorldRoot.SetActive(true);
                Capture(host.WorldCamera,oldBounds,"before-local.png",8,960,720);
                string folder="Assets/Eldoria/SpawnlingBastion2/Generated/AnimatedV4-"+Environment.GetEnvironmentVariable("GITHUB_RUN_ID");
                Need(!Directory.Exists(folder),"Never overwrite earlier animation resources");
                Directory.CreateDirectory(folder);AssetDatabase.Refresh();
                var root=new GameObject("NEAR / reviewed V4 genuine skin");
                root.transform.SetParent(actor.MotionPivot,false);
                Animation animation;AnimationClip savedClip;int added=0,reused=0;
                using(var importer=new GltfImport(null,new UninterruptedDeferAgent())) {
                    Need(await importer.Load(new Uri(Path.GetFullPath(input)).AbsoluteUri,
                        new ImportSettings{AnimationMethod=AnimationMethod.Legacy,GenerateMipMaps=true}),"Reviewed skin import failed");
                    Need(await importer.InstantiateMainSceneAsync(root.transform),"Reviewed skin hierarchy missing");
                    var skins=root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                    Need(skins.Length==3&&skins.SelectMany(r=>r.bones).Distinct().Count()==12,"Pinned skin/bone counts changed");
                    Need(root.GetComponentsInChildren<Collider>(true).Length==0&&root.GetComponentsInChildren<Camera>(true).Length==0,
                        "Visual must not introduce physics or cameras");
                    var clips=importer.GetAnimationClips();Need(clips!=null&&clips.Length==1,"Expected one reviewed clip");
                    animation=root.GetComponentsInChildren<Animation>(true).Single();animation.Stop();animation.playAutomatically=false;
                    savedClip=Object.Instantiate(clips[0]);savedClip.name="Eldoria_Spawnling_ExpressiveClawSwipe_V4";
                    savedClip.legacy=true;savedClip.wrapMode=WrapMode.Once;
                    AssetDatabase.CreateAsset(savedClip,folder+"/ExpressiveClawSwipeV4.anim");
                    foreach(string name in animation.Cast<AnimationState>().Select(a=>a.name).ToArray())animation.RemoveClip(name);
                    animation.AddClip(savedClip,savedClip.name);animation.clip=savedClip;animation.wrapMode=WrapMode.Once;
                    savedClip.SampleAnimation(animation.gameObject,1f/24f);
                    PersistSkin(root,folder,oldNear.Concat(oldFar).ToArray(),ref added,ref reused);
                }
                var renderers=root.GetComponentsInChildren<Renderer>(true);
                var bounds=BoundsOf(renderers);Need(bounds.size.y>.2f,"Imported source height implausible");
                root.transform.localScale*=oldBounds.size.y/bounds.size.y;
                bounds=BoundsOf(renderers);root.transform.position+=new Vector3(oldBounds.center.x-bounds.center.x,
                    oldBounds.min.y-bounds.min.y,oldBounds.center.z-bounds.center.z);
                // Cache the full real clip envelope so near-camera skin culling cannot cut off a swipe.
                var skinRenderers=root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                foreach(var skin in skinRenderers) {
                    Bounds envelope=default;bool first=true;
                    foreach(float t in new[]{1f/24f,19f/24f,27f/24f,34f/24f,65f/24f}) {
                        savedClip.SampleAnimation(animation.gameObject,t);var baked=new Mesh();
                        skin.BakeMesh(baked);
                        if(first){envelope=baked.bounds;first=false;}else envelope.Encapsulate(baked.bounds);
                        Object.DestroyImmediate(baked);
                    }
                    skin.localBounds=envelope;skin.updateWhenOffscreen=false;
                }
                savedClip.SampleAnimation(animation.gameObject,1f/24f);
                foreach(var old in oldNear)old.enabled=false;
                levels[0].renderers=renderers;lod.SetLODs(levels);lod.RecalculateBounds();
                actor.SkeletalAnimation=animation;actor.SwipeClip=savedClip.name;
                int nearTriangles=skinRenderers.Sum(r=>r.sharedMesh.triangles.Length/3);
                int farTriangles=oldFar.Sum(r=>r.GetComponent<MeshFilter>().sharedMesh.triangles.Length/3);
                Need(nearTriangles==12040&&farTriangles<=3300,"Mobile LOD budgets changed");
                await Frame();Capture(host.WorldCamera,oldBounds,"after-rest.png",8,960,720);
                savedClip.SampleAnimation(animation.gameObject,27f/24f);
                await Frame();Capture(host.WorldCamera,oldBounds,"after-swipe.png",8,960,720);
                Need(Hash(Path.Combine(Output,"after-rest.png"))!=Hash(Path.Combine(Output,"after-swipe.png")),
                    "Actual canonical camera does not show changing skin");
                savedClip.SampleAnimation(animation.gameObject,65f/24f);
                await Frame();Capture(host.WorldCamera,oldBounds,"after-return.png",8,960,720);
                Capture(host.WorldCamera,oldBounds,"after-regional.png",32,1600,900);
                Need(identities.SequenceEqual(host.WorldRoot.GetComponentsInChildren<WorldHotspot>(true)
                    .Select(h=>h.Id+"="+h.transform.position.ToString("R")).OrderBy(x=>x)),"Original gameplay anchors changed");
                Need(host.WorldRoot.GetComponentsInChildren<Collider>(true).Length==colliders,"Original physics changed");
                host.WorldRoot.SetActive(wasActive);
                AssetDatabase.SaveAssets();Need(EditorSceneManager.SaveScene(scene,ScenePath),"Canonical save failed");
                EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
                var reopened=Object.FindObjectsByType<SpawnlingEncounterVisual>(FindObjectsInactive.Include,FindObjectsSortMode.None).Single();
                Need(reopened.SkeletalAnimation&&reopened.SkeletalAnimation.GetClip(reopened.SwipeClip)&&
                    reopened.SkeletalAnimation.GetComponentsInChildren<SkinnedMeshRenderer>(true).All(r=>r.sharedMesh&&AssetDatabase.Contains(r.sharedMesh)),
                    "Reopened skin/clip resources were not persisted");
                var report=new Report{status="V4_CANONICAL_SKIN_INTEGRATED_GAMEPLAY_QA_REQUIRED",
                    source_sha256=SourceSha,scene_before=before,scene_after=Hash(ScenePath),clip=savedClip.name,duration=savedClip.length,
                    bones=12,near_triangles=nearTriangles,far_triangles=farTriangles,hotspots=spots.Length,colliders=colliders,
                    new_textures=added,reused_textures=reused,original_hotspots_preserved=true,gameplay_unchanged=true,canonical_save_untouched=true};
                File.WriteAllText(Path.Combine(Output,"integration.json"),JsonUtility.ToJson(report,true));
                Debug.Log("ELDORIA_SPAWNLING_V4_ANIMATED_INTEGRATION_COMPLETE");
                EditorApplication.Exit(0);
            } catch(Exception error) {
                File.WriteAllText(Path.Combine(Output,"error.txt"),error.ToString());Debug.LogException(error);EditorApplication.Exit(1);
            }
        }
    }
}
#endif
