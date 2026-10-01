using System;
using System.Collections.Generic;
using System.IO;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Eldoria.EditorTools
{
    // trigger: integrated-proof-v1
    public static class HeroBastionIntegratedGate
    {
        const string Folder="HeroBastionIntegratedCaptures";
        const string AssetPath="Assets/Resources/Valoria/HeroBastionGenerated/Valoria_HeroBastion_v1.glb";
        static readonly Vector3 CameraPosition=new Vector3(18.2f,14.6f,-25.8f);
        static readonly Vector3 CameraTarget=new Vector3(0f,3.65f,7.25f);

        public static void Capture()
        {
            UnityEditor.ShaderUtil.allowAsyncCompilation=false;
            SceneSetup.SetupRenderPipeline();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            ProductionVisualIntegration.ResetVisualCachesForGate();
            VisualWorld.VisualIntegrationEnabled=true;
            VisualWorld.Create(true,new PlayerState{
                BastionLevel=3,
                SawmillLevel=1,
                BarracksLevel=1,
                CorruptionDiscovered=true
            });

            var camera=Camera.main;
            if(camera==null)throw new Exception("Valoria camera missing.");
            Directory.CreateDirectory(Folder);

            string baseline=ValoriaVisualFormulaGate.CollisionSignature();
            string beforeMetrics=MetricsJson();

            Save(camera,Folder+"/before-19.png",19f,1280,720);
            Save(camera,Folder+"/before-12.png",12f,1280,720);
            Save(camera,Folder+"/before-9.png",9f,1280,720);
            Save(camera,Folder+"/before-mobile.png",12f,390,844);

            int suppressed=HideLegacyBastionVisuals();
            if(suppressed<1)throw new Exception("No legacy Bastion visual renderers were suppressed.");

            var hero=PlaceGeneratedHero();
            string afterMetrics=MetricsJson();

            string afterSignature=ValoriaVisualFormulaGate.CollisionSignature();
            if(afterSignature!=baseline)
                throw new Exception("Integrated Hero Bastion proof altered gameplay collider/hotspot signature.");

            Save(camera,Folder+"/after-19.png",19f,1280,720);
            Save(camera,Folder+"/after-12.png",12f,1280,720);
            Save(camera,Folder+"/after-9.png",9f,1280,720);
            Save(camera,Folder+"/after-mobile.png",12f,390,844);

            var renderers=hero.GetComponentsInChildren<Renderer>(true);
            Bounds bounds=renderers[0].bounds;
            for(int i=1;i<renderers.Length;i++)bounds.Encapsulate(renderers[i].bounds);

            File.WriteAllText(Folder+"/integrated-evidence.json",
                "{\n"+
                "  \"phase\": \"HERO_BASTION_INTEGRATED_PROOF_V1\",\n"+
                "  \"branch\": \"visual-proof/hero-bastion-integrated-v1\",\n"+
                "  \"source_raw_sha256\": \"fd859fe52b45dbdd3224f7f607897c1d36f326dc0ff2632b0a0f2a4b14348ebf\",\n"+
                "  \"source_optimized_sha256\": \"afb6cee6ae572b0879650f18285b32798e263c17359a158ffe2bdd03fb62ad5c\",\n"+
                "  \"same_scene_before_after\": true,\n"+
                "  \"collider_hotspot_signature_equal\": true,\n"+
                "  \"legacy_bastion_renderers_suppressed\": "+suppressed+",\n"+
                "  \"generated_asset_colliders_enabled\": false,\n"+
                "  \"generated_asset_hotspots_added\": false,\n"+
                "  \"tripo_credits_this_proof\": 0,\n"+
                "  \"before_metrics\": "+beforeMetrics+",\n"+
                "  \"after_metrics\": "+afterMetrics+",\n"+
                "  \"hero_bounds_center\": ["+F(bounds.center.x)+","+F(bounds.center.y)+","+F(bounds.center.z)+"],\n"+
                "  \"hero_bounds_size\": ["+F(bounds.size.x)+","+F(bounds.size.y)+","+F(bounds.size.z)+"]\n"+
                "}\n");

            EditorApplication.Exit(0);
        }

        static int HideLegacyBastionVisuals()
        {
            int count=0;
            foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                bool legacy=false;
                for(var t=r.transform;t!=null;t=t.parent)
                {
                    if(t.name.StartsWith("Bastion ·",StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(t.name,"Bastion",StringComparison.OrdinalIgnoreCase) ||
                       t.name.StartsWith("Valoria · Bastion hero",StringComparison.OrdinalIgnoreCase) ||
                       t.name.StartsWith("Valoria · rescued hero flank",StringComparison.OrdinalIgnoreCase))
                    {
                        legacy=true;
                        break;
                    }
                }
                if(!legacy)continue;
                r.enabled=false;
                count++;
            }
            return count;
        }

        static GameObject PlaceGeneratedHero()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var source=AssetDatabase.LoadAssetAtPath<GameObject>(AssetPath);
            if(source==null)throw new Exception("Generated Hero Bastion GLB missing: "+AssetPath);

            var go=UnityEngine.Object.Instantiate(source);
            go.name="Valoria · Generated Hero Bastion v1 · integrated proof";
            go.transform.rotation=Quaternion.Euler(0f,180f,0f);

            foreach(var c in go.GetComponentsInChildren<Collider>(true))c.enabled=false;
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))
                UnityEngine.Object.DestroyImmediate(h);

            var renderers=go.GetComponentsInChildren<Renderer>(true);
            if(renderers.Length==0)throw new Exception("Generated Hero Bastion has no renderers.");

            Bounds bounds=renderers[0].bounds;
            for(int i=1;i<renderers.Length;i++)bounds.Encapsulate(renderers[i].bounds);
            float span=Mathf.Max(bounds.size.x,bounds.size.z);
            if(span<=.001f||bounds.size.y<=.001f)throw new Exception("Generated Hero Bastion bounds invalid.");

            // Preserve Tripo proportions; only uniformly fit the authored Bastion envelope.
            const float targetSpan=12.8f;
            const float targetHeight=10.2f;
            float scale=Mathf.Min(targetSpan/span,targetHeight/bounds.size.y);
            go.transform.localScale*=scale;

            renderers=go.GetComponentsInChildren<Renderer>(true);
            bounds=renderers[0].bounds;
            for(int i=1;i<renderers.Length;i++)bounds.Encapsulate(renderers[i].bounds);

            // L1 landing is y≈2.55; front access must face the real central stair (-Z).
            Vector3 targetCenter=new Vector3(0f,0f,8.75f);
            float groundY=2.52f;
            go.transform.position+=new Vector3(
                targetCenter.x-bounds.center.x,
                groundY-bounds.min.y,
                targetCenter.z-bounds.center.z);

            // Do not replace imported Tripo materials/textures. The point of this proof is
            // to judge the generated asset in the real frame, not a generic material override.
            go.transform.SetParent(new GameObject("HERO BASTION INTEGRATED PROOF · visual only").transform,true);
            return go;
        }

        static string MetricsJson()
        {
            long triangles=0;
            int renderers=0,lights=0;
            var materials=new HashSet<int>();
            foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                renderers++;
                foreach(var m in r.sharedMaterials)if(m!=null)materials.Add(m.GetInstanceID());
                var mf=r.GetComponent<MeshFilter>();
                if(mf!=null&&mf.sharedMesh!=null)triangles+=mf.sharedMesh.triangles.LongLength/3;
                var sk=r as SkinnedMeshRenderer;
                if(sk!=null&&sk.sharedMesh!=null)triangles+=sk.sharedMesh.triangles.LongLength/3;
            }
            foreach(var l in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
                if(l!=null&&l.enabled&&l.gameObject.activeInHierarchy)lights++;
            return "{\"triangles\":"+triangles+",\"renderers\":"+renderers+",\"materials\":"+materials.Count+",\"lights\":"+lights+"}";
        }

        static void Save(Camera camera,string path,float zoom,int width,int height)
        {
            camera.transform.position=CameraPosition;
            camera.transform.LookAt(CameraTarget);
            camera.orthographic=true;
            camera.orthographicSize=zoom;
            var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);
            var tex=new Texture2D(width,height,TextureFormat.RGB24,false);
            camera.targetTexture=rt;
            camera.Render();
            RenderTexture.active=rt;
            tex.ReadPixels(new Rect(0,0,width,height),0,0);
            tex.Apply();
            File.WriteAllBytes(path,tex.EncodeToPNG());
            camera.targetTexture=null;
            RenderTexture.active=null;
            UnityEngine.Object.DestroyImmediate(tex);
            UnityEngine.Object.DestroyImmediate(rt);
        }

        static string F(float v)=>v.ToString("0.###",System.Globalization.CultureInfo.InvariantCulture);
    }
}
