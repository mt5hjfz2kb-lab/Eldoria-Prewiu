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
    public static class HeroDistrictIntegrationGate
    {
        const string Folder="HeroDistrictIntegrationCaptures";
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

            int suppressed=HideLegacyBastionVisuals();
            if(suppressed<1)throw new Exception("No legacy Bastion visual renderers were suppressed.");
            var hero=PlaceGeneratedHero();

            string baselineSignature=ValoriaVisualFormulaGate.CollisionSignature();
            string beforeMetrics=MetricsJson();

            Save(camera,Folder+"/before-19.png",19f,1280,720);
            Save(camera,Folder+"/before-12.png",12f,1280,720);
            Save(camera,Folder+"/before-9.png",9f,1280,720);
            Save(camera,Folder+"/before-mobile.png",12f,390,844);

            var district=BuildHeroDistrictIntegration();
            if(district==null)throw new Exception("Hero district integration root missing.");
            DisableAllGameplayOnVisuals(district);
            string afterSignature=ValoriaVisualFormulaGate.CollisionSignature();
            if(afterSignature!=baselineSignature)
                throw new Exception("Hero district integration altered gameplay collider/hotspot signature.");

            string afterMetrics=MetricsJson();
            Save(camera,Folder+"/after-19.png",19f,1280,720);
            Save(camera,Folder+"/after-12.png",12f,1280,720);
            Save(camera,Folder+"/after-9.png",9f,1280,720);
            Save(camera,Folder+"/after-mobile.png",12f,390,844);

            var heroBounds=BoundsOf(hero);
            var districtBounds=BoundsOf(district);
            File.WriteAllText(Folder+"/hero-district-evidence.json",
                "{\n"+
                "  \"phase\": \"VALORIA_HERO_DISTRICT_INTEGRATION_V1\",\n"+
                "  \"branch\": \"visual-proof/hero-district-integration-v1\",\n"+
                "  \"source_optimized_sha256\": \"afb6cee6ae572b0879650f18285b32798e263c17359a158ffe2bdd03fb62ad5c\",\n"+
                "  \"tripo_credits_this_proof\": 0,\n"+
                "  \"same_scene_before_after\": true,\n"+
                "  \"collider_hotspot_signature_equal\": true,\n"+
                "  \"generated_asset_colliders_enabled\": false,\n"+
                "  \"generated_asset_hotspots_added\": false,\n"+
                "  \"legacy_bastion_renderers_suppressed\": "+suppressed+",\n"+
                "  \"before_metrics\": "+beforeMetrics+",\n"+
                "  \"after_metrics\": "+afterMetrics+",\n"+
                "  \"hero_bounds_center\": ["+F(heroBounds.center.x)+","+F(heroBounds.center.y)+","+F(heroBounds.center.z)+"],\n"+
                "  \"hero_bounds_size\": ["+F(heroBounds.size.x)+","+F(heroBounds.size.y)+","+F(heroBounds.size.z)+"],\n"+
                "  \"district_bounds_center\": ["+F(districtBounds.center.x)+","+F(districtBounds.center.y)+","+F(districtBounds.center.z)+"],\n"+
                "  \"district_bounds_size\": ["+F(districtBounds.size.x)+","+F(districtBounds.size.y)+","+F(districtBounds.size.z)+"]\n"+
                "}\n");

            EditorApplication.Exit(0);
        }

        static GameObject BuildHeroDistrictIntegration()
        {
            var root=new GameObject("HERO DISTRICT INTEGRATION v1 · visual only");
            var art=ValoriaExternalAssetLibrary.Load();

            // Surface continuity first: keep certified physical stair/landing topology exactly where it is,
            // but make their visible faces share one restrained Eldoria stone response.
            var stairMat=ValoriaKit.PbrSurfaceMaterial(
                art!=null?art.ValoriaStoneSurface:null,
                new Color(.66f,.64f,.59f,1f),new Vector2(2.4f,1.5f),.025f,.90f);
            var groundMat=ValoriaKit.PbrSurfaceMaterial(
                art!=null?art.ValoriaCobbleSurface:null,
                new Color(.72f,.69f,.61f,1f),new Vector2(3.4f,3.4f),.028f,.78f);

            foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                var n=HierarchyName(r.transform);
                if(n.Contains("VPD · vertical stair",StringComparison.OrdinalIgnoreCase))
                    r.sharedMaterial=stairMat;
                else if(n.Contains("VPD · GroundKit L1 landing",StringComparison.OrdinalIgnoreCase) ||
                        n.Contains("VPD · GroundKit L1 west terrace",StringComparison.OrdinalIgnoreCase) ||
                        n.Contains("VPD · GroundKit L1 east terrace",StringComparison.OrdinalIgnoreCase))
                    r.sharedMaterial=groundMat;
            }

            // Seat the fused Hero Bastion into the authored mountain instead of leaving a clean model/base seam.
            if(art!=null&&art.SlavicFlatRock!=null)
            {
                AddPiece(root,ValoriaKit.BenchmarkPieceModulated("HeroDistrict · seat rock west A",art.SlavicFlatRock,
                    new Vector3(-5.15f,2.18f,8.15f),3.65f,1.75f,Quaternion.Euler(0,-34f,0),new Color(.53f,.55f,.52f,1f)));
                AddPiece(root,ValoriaKit.BenchmarkPieceModulated("HeroDistrict · seat rock west B",art.SlavicFlatRock,
                    new Vector3(-3.65f,2.28f,10.20f),3.20f,1.50f,Quaternion.Euler(0,48f,0),new Color(.50f,.52f,.50f,1f)));
                AddPiece(root,ValoriaKit.BenchmarkPieceModulated("HeroDistrict · seat rock east A",art.SlavicFlatRock,
                    new Vector3(5.10f,2.18f,8.30f),3.60f,1.70f,Quaternion.Euler(0,30f,0),new Color(.53f,.55f,.52f,1f)));
                AddPiece(root,ValoriaKit.BenchmarkPieceModulated("HeroDistrict · seat rock east B",art.SlavicFlatRock,
                    new Vector3(3.75f,2.25f,10.30f),3.10f,1.45f,Quaternion.Euler(0,-52f,0),new Color(.50f,.52f,.50f,1f)));
                AddPiece(root,ValoriaKit.BenchmarkPieceModulated("HeroDistrict · stair rock west",art.SlavicFlatRock,
                    new Vector3(-2.42f,.82f,3.62f),2.25f,1.20f,Quaternion.Euler(0,-18f,0),new Color(.48f,.50f,.47f,1f)));
                AddPiece(root,ValoriaKit.BenchmarkPieceModulated("HeroDistrict · stair rock east",art.SlavicFlatRock,
                    new Vector3(2.45f,.84f,3.76f),2.20f,1.18f,Quaternion.Euler(0,21f,0),new Color(.48f,.50f,.47f,1f)));
            }

            // Retaining faces make the landing read as masonry cut into rock, not a floating flat platform.
            if(art!=null&&art.SlavicStoneFence!=null)
            {
                foreach(var data in new[]{
                    new[]{-4.65f,1.46f,5.00f,-4f},new[]{-2.55f,1.52f,4.92f,2f},
                    new[]{2.55f,1.52f,4.92f,-2f},new[]{4.65f,1.46f,5.00f,4f}})
                {
                    AddPiece(root,ValoriaKit.BenchmarkPieceModulated("HeroDistrict · retaining masonry",art.SlavicStoneFence,
                        new Vector3(data[0],data[1],data[2]),2.35f,1.35f,Quaternion.Euler(0,data[3],0),
                        new Color(.74f,.73f,.68f,1f)));
                }
            }

            // The right upper dwelling is the one immediate procedural building that competes directly
            // with the hero asset. Replace only its renderer after an authored visual replacement exists.
            GameObject authoredHouse=null;
            if(art!=null&&art.SlavicHouse!=null)
                authoredHouse=ValoriaKit.BenchmarkPieceModulated("HeroDistrict · east upper residence",art.SlavicHouse,
                    new Vector3(5.15f,2.89f,7.15f),3.45f,3.55f,Quaternion.Euler(0,188f,0),
                    new Color(.76f,.73f,.67f,1f));
            if(authoredHouse!=null)
            {
                AddPiece(root,authoredHouse);
                HideVisualFamily("VPD · upper dwelling");
            }

            // Restrained inhabited warmth: local only, no global exposure trick.
            AddWarmLight(root,"HeroDistrict · landing warmth",new Vector3(0f,3.35f,5.45f),.42f,4.0f);
            AddWarmLight(root,"HeroDistrict · east hearth",new Vector3(4.65f,3.75f,6.35f),.26f,2.6f);

            return root;
        }

        static void AddWarmLight(GameObject root,string name,Vector3 p,float intensity,float range)
        {
            var go=new GameObject(name);go.transform.SetParent(root.transform,false);go.transform.position=p;
            var l=go.AddComponent<Light>();l.type=LightType.Point;l.color=new Color(1f,.60f,.32f);
            l.intensity=intensity;l.range=range;l.shadows=LightShadows.None;
        }

        static void AddPiece(GameObject root,GameObject go)
        {
            if(go!=null)go.transform.SetParent(root.transform,true);
        }

        static void HideVisualFamily(string family)
        {
            foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled)continue;
                if(HierarchyName(r.transform).Contains(family,StringComparison.OrdinalIgnoreCase))r.enabled=false;
            }
        }

        static string HierarchyName(Transform t)
        {
            string s="";
            for(var p=t;p!=null;p=p.parent)s=p.name+"/"+s;
            return s;
        }

        static void DisableAllGameplayOnVisuals(GameObject root)
        {
            foreach(var c in root.GetComponentsInChildren<Collider>(true))c.enabled=false;
            foreach(var h in root.GetComponentsInChildren<WorldHotspot>(true))UnityEngine.Object.DestroyImmediate(h);
            foreach(var b in root.GetComponentsInChildren<MonoBehaviour>(true))
                if(!(b is WorldHotspot))b.enabled=false;
            Physics.SyncTransforms();
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
                    { legacy=true; break; }
                }
                if(!legacy)continue;
                r.enabled=false;count++;
            }
            return count;
        }

        static GameObject PlaceGeneratedHero()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var source=AssetDatabase.LoadAssetAtPath<GameObject>(AssetPath);
            if(source==null)throw new Exception("Generated Hero Bastion GLB missing: "+AssetPath);
            var go=UnityEngine.Object.Instantiate(source);
            go.name="Valoria · Generated Hero Bastion v1 · hero district proof";
            go.transform.rotation=Quaternion.Euler(0f,180f,0f);
            DisableAllGameplayOnVisuals(go);

            var bounds=BoundsOf(go);
            float span=Mathf.Max(bounds.size.x,bounds.size.z);
            if(span<=.001f||bounds.size.y<=.001f)throw new Exception("Generated Hero Bastion bounds invalid.");
            float scale=Mathf.Min(12.8f/span,10.2f/bounds.size.y);
            go.transform.localScale*=scale;
            bounds=BoundsOf(go);
            go.transform.position+=new Vector3(-bounds.center.x,2.52f-bounds.min.y,8.75f-bounds.center.z);
            FitGeneratedHeroSurface(go);
            return go;
        }

        static void FitGeneratedHeroSurface(GameObject go)
        {
            var cache=new Dictionary<int,Material>();
            foreach(var r in go.GetComponentsInChildren<Renderer>(true))
            {
                var srcs=r.sharedMaterials;var dst=new Material[srcs.Length];
                for(int i=0;i<srcs.Length;i++)
                {
                    var src=srcs[i];if(src==null){dst[i]=null;continue;}
                    if(cache.TryGetValue(src.GetInstanceID(),out var cached)){dst[i]=cached;continue;}
                    Texture baseMap=null,normal=null,mask=null;
                    foreach(string p in new[]{"_BaseMap","_MainTex","_BaseColorTexture","baseColorTexture","_Texture"})
                        if(src.HasProperty(p)&&src.GetTexture(p)!=null){baseMap=src.GetTexture(p);break;}
                    foreach(string p in new[]{"_BumpMap","_NormalMap","normalTexture"})
                        if(src.HasProperty(p)&&src.GetTexture(p)!=null){normal=src.GetTexture(p);break;}
                    foreach(string p in new[]{"_MaskMap","_MetallicGlossMap","_OcclusionMap"})
                        if(src.HasProperty(p)&&src.GetTexture(p)!=null){mask=src.GetTexture(p);break;}
                    var shader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");
                    var m=new Material(shader){name="Valoria fitted · "+src.name};
                    if(baseMap!=null)m.SetTexture("_BaseMap",baseMap);
                    if(normal!=null){m.SetTexture("_BumpMap",normal);m.EnableKeyword("_NORMALMAP");if(m.HasProperty("_BumpScale"))m.SetFloat("_BumpScale",1f);}
                    if(mask!=null&&m.HasProperty("_OcclusionMap"))m.SetTexture("_OcclusionMap",mask);
                    if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",new Color(.72f,.69f,.64f,1f));
                    if(m.HasProperty("_Color"))m.SetColor("_Color",new Color(.72f,.69f,.64f,1f));
                    if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",0f);
                    if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",.03f);
                    if(m.HasProperty("_OcclusionStrength"))m.SetFloat("_OcclusionStrength",1f);
                    cache[src.GetInstanceID()]=m;dst[i]=m;
                }
                r.sharedMaterials=dst;
            }
        }

        static Bounds BoundsOf(GameObject go)
        {
            var rs=go.GetComponentsInChildren<Renderer>(true);
            if(rs.Length==0)return new Bounds(go.transform.position,Vector3.zero);
            var b=rs[0].bounds;for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);return b;
        }

        static string MetricsJson()
        {
            long triangles=0;int renderers=0,lights=0;var materials=new HashSet<int>();
            foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                renderers++;foreach(var m in r.sharedMaterials)if(m!=null)materials.Add(m.GetInstanceID());
                var mf=r.GetComponent<MeshFilter>();if(mf!=null&&mf.sharedMesh!=null)triangles+=mf.sharedMesh.triangles.LongLength/3;
                var sk=r as SkinnedMeshRenderer;if(sk!=null&&sk.sharedMesh!=null)triangles+=sk.sharedMesh.triangles.LongLength/3;
            }
            foreach(var l in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
                if(l!=null&&l.enabled&&l.gameObject.activeInHierarchy)lights++;
            return "{\"triangles\":"+triangles+",\"renderers\":"+renderers+",\"materials\":"+materials.Count+",\"lights\":"+lights+"}";
        }

        static void Save(Camera camera,string path,float zoom,int width,int height)
        {
            camera.transform.position=CameraPosition;camera.transform.LookAt(CameraTarget);
            camera.orthographic=true;camera.orthographicSize=zoom;
            var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);
            var tex=new Texture2D(width,height,TextureFormat.RGB24,false);
            camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
            tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());
            camera.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(rt);
        }

        static string F(float v)=>v.ToString("0.###",System.Globalization.CultureInfo.InvariantCulture);
    }
}
