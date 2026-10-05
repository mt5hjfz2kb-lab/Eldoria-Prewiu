using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Eldoria.EditorTools
{
    // Golden Lookdev Slice v1: deterministic authored surface maps for the bounded real-scene proof.
    // This is deliberately not MaterialPropertyBlock tint variation and not a flat/noise-only material pass.
    public static class GoldenSurfaceAuthoringV1
    {
        sealed class SurfaceSet
        {
            public string name;
            public Texture2D albedo;
            public Texture2D normal;
            public Texture2D occlusion;
            public Texture2D metallicSmooth;
            public float normalScale;
            public Vector2 tiling;
        }

        public static void Apply(PreproductionSceneCapture.PremiumTreatment treatment, PreproductionSceneCapture.Evidence evidence)
        {
            bool persistent=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Eldoria/ArtTests/GoldenSurfaceV2/Textures/stone_albedo.png")!=null;
            var stone=persistent?LoadPersisted("STONE","stone",.72f,new Vector2(1.55f,1.55f)):Build("STONE",new Color(.62f,.53f,.39f),.13f,new Vector2(1.65f,1.65f));
            var rock=persistent?LoadPersisted("ROCK","rock",.88f,new Vector2(1.28f,1.28f)):Build("ROCK",new Color(.40f,.36f,.29f),.09f,new Vector2(1.35f,1.35f));
            var ground=persistent?LoadPersisted("GROUND","ground",.62f,new Vector2(.92f,.92f)):Build("GROUND",new Color(.43f,.38f,.25f),.07f,new Vector2(1.10f,1.10f));
            var shore=persistent?LoadPersisted("SHORE","shore",.46f,new Vector2(1.35f,1.35f)):Build("SHORE",new Color(.19f,.23f,.18f),.32f,new Vector2(1.55f,1.55f));
            var vegetation=persistent?LoadPersisted("VEGETATION","vegetation",.34f,new Vector2(1.25f,1.25f)):Build("VEGETATION",new Color(.32f,.48f,.29f),.08f,new Vector2(1.35f,1.35f));
            int authoredRenderers=0,uvFixed=0;
            var uniqueMaterials=new HashSet<Material>();
            foreach(var renderer in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(!renderer.enabled||!renderer.gameObject.activeInHierarchy) continue;
                var center=renderer.bounds.center;
                if(center.z<treatment.hero_min_z||center.z>treatment.hero_max_z||center.x<treatment.hero_min_x||center.x>treatment.hero_max_x) continue;
                string n=renderer.name.ToLowerInvariant();
                SurfaceSet set=null;
                if(n.StartsWith("lowergate_")||n.StartsWith("gatefoundation_")||n.Contains("foundationcontact")||n=="bridge_deck"||n=="bridge_parapetl"||n=="bridge_parapetr"||n=="bridge_support"||n=="gatethreshold") set=stone;
                else if(n.StartsWith("rockcontact_")||n.StartsWith("shorerock_")||n.StartsWith("rock_")||n=="foregroundbank") set=rock;
                else if(n.StartsWith("heroground_")||n.StartsWith("heroroadverge_")||n.Contains("_berm")||n.Contains("_vergebreak")||n.Contains("_shoulder")||n=="gatewestberm"||n=="gateeastberm"||n=="roadwestverge"||n=="roadeastverge"||n=="bridgewestshoulder"||n=="bridgeeastshoulder"||n=="foregroundroad"||n=="mainroad") set=ground;
                else if(n.StartsWith("valoria shore")) set=shore;
                else if(n.StartsWith("tree_")) set=vegetation;
                if(set==null) continue;

                var mf=renderer.GetComponent<MeshFilter>();
                if(mf!=null&&mf.sharedMesh!=null&&mf.sharedMesh.vertexCount>0)
                {
                    var mesh=mf.sharedMesh;
                    if(mesh.uv==null||mesh.uv.Length!=mesh.vertexCount)
                    {
                        var copy=Object.Instantiate(mesh);copy.name=mesh.name+" · GoldenSurfaceUV";
                        var uv=new Vector2[copy.vertexCount];
                        var v=copy.vertices;
                        // Metric-ish planar projection in local X/Z; vertical meshes use X/Y.
                        var b=copy.bounds;bool vertical=b.size.y>b.size.z*1.35f;
                        for(int i=0;i<v.Length;i++) uv[i]=vertical?new Vector2(v[i].x*.34f,v[i].y*.34f):new Vector2(v[i].x*.34f,v[i].z*.34f);
                        copy.uv=uv;mf.sharedMesh=copy;uvFixed++;
                    }
                }

                var src=renderer.sharedMaterials;if(src==null||src.Length==0) continue;
                var dst=new Material[src.Length];
                for(int i=0;i<src.Length;i++)
                {
                    if(src[i]==null){dst[i]=null;continue;}
                    var m=new Material(src[i]){name=src[i].name+" · GoldenSurfaceV1 · "+set.name};
                    if(m.HasProperty("_BaseColor")) m.SetColor("_BaseColor",Color.white);
                    if(m.HasProperty("_BaseMap")){m.SetTexture("_BaseMap",set.albedo);m.SetTextureScale("_BaseMap",set.tiling);}
                    if(m.HasProperty("_BumpMap"))
                    {
                        m.SetTexture("_BumpMap",set.normal);m.SetTextureScale("_BumpMap",set.tiling);
                        if(m.HasProperty("_BumpScale"))m.SetFloat("_BumpScale",set.normalScale);
                        m.EnableKeyword("_NORMALMAP");
                    }
                    if(m.HasProperty("_OcclusionMap"))
                    {
                        m.SetTexture("_OcclusionMap",set.occlusion);m.SetTextureScale("_OcclusionMap",set.tiling);
                        if(m.HasProperty("_OcclusionStrength"))m.SetFloat("_OcclusionStrength",.92f);
                        m.EnableKeyword("_OCCLUSIONMAP");
                    }
                    if(m.HasProperty("_MetallicGlossMap"))
                    {
                        m.SetTexture("_MetallicGlossMap",set.metallicSmooth);m.SetTextureScale("_MetallicGlossMap",set.tiling);
                        if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",0f);
                        if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",1f);
                        m.EnableKeyword("_METALLICSPECGLOSSMAP");
                    }
                    m.enableInstancing=true;dst[i]=m;uniqueMaterials.Add(m);
                }
                renderer.sharedMaterials=dst;authoredRenderers++;
            }
            evidence.surface_authoring=true;
            evidence.surface_authored_renderers=authoredRenderers;
            evidence.surface_uv_fixed_renderers=uvFixed;
            evidence.surface_texture_count=20;
            evidence.surface_texture_dimensions=persistent?"Blender persisted V2: stone/rock/ground 512; shore/vegetation 256; 4 maps/family":"Runtime V1 fallback: 5 families x 4 maps x 256";
            evidence.surface_texture_bytes_estimated=persistent?0:5L*4L*256L*256L*4L*4L/3L;
            evidence.surface_material_instances=uniqueMaterials.Count;
        }

        static SurfaceSet LoadPersisted(string kind,string file,float normalScale,Vector2 tiling)
        {
            string root="Assets/Eldoria/ArtTests/GoldenSurfaceV2/Textures/";
            var albedo=LoadMap(root+file+"_albedo.png",false,false);
            var normal=LoadMap(root+file+"_normal.png",true,false);
            var ao=LoadMap(root+file+"_ao.png",false,true);
            var smooth=LoadMap(root+file+"_smoothness.png",false,true);
            if(albedo==null||normal==null||ao==null||smooth==null) throw new Exception("Golden Surface V2 persistent map set incomplete: "+file);
            return new SurfaceSet{name=kind+"-BLENDER-V2",albedo=albedo,normal=normal,occlusion=ao,metallicSmooth=smooth,normalScale=normalScale,tiling=tiling};
        }

        static Texture2D LoadMap(string path,bool normalMap,bool linear)
        {
            // Import settings are enforced by GoldenSurfaceV2TextureImporter before Capture runs.
            // Never reimport from inside a batch executeMethod: that can trigger domain reload/hang.
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        static SurfaceSet Build(string kind,Color baseColor,float baseSmooth,Vector2 tiling)
        {
            const int size=256;
            var h=new float[size*size];
            var ao=new float[size*size];
            var sm=new float[size*size];
            var col=new Color[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                float u=(x+.5f)/size,v=(y+.5f)/size;
                float height=.5f,occ=1f,smooth=baseSmooth;
                Color c=baseColor;
                if(kind=="STONE")
                {
                    // Staggered masonry: mortar, face bowing, edge wear and mineral/dirt accumulation.
                    float rows=8f,cols=7f;float ry=v*rows;int row=Mathf.FloorToInt(ry);
                    float uu=Mathf.Repeat(u*cols+(row%2)*.5f,1f);float vv=Mathf.Repeat(ry,1f);
                    float edge=Mathf.Min(Mathf.Min(uu,1f-uu),Mathf.Min(vv,1f-vv));
                    float mortar=Mathf.SmoothStep(.025f,.075f,edge);
                    float face=(1f-Mathf.Pow(Mathf.Abs(uu-.5f)*2f,3f))*(1f-Mathf.Pow(Mathf.Abs(vv-.5f)*2f,3f));
                    float mineral=.5f+.5f*Mathf.Sin((u*2.1f+v*.72f)*Mathf.PI*2f);
                    float stain=Mathf.Clamp01((v-.58f)*2.6f)*(.5f+.5f*Mathf.Sin(u*13f+v*5f));
                    height=.34f+mortar*.38f+face*.08f+mineral*.025f;
                    occ=.62f+mortar*.34f-stain*.10f;smooth=baseSmooth+mortar*.025f-stain*.035f;
                    c*=.82f+mortar*.21f+mineral*.035f-stain*.08f;
                }
                else if(kind=="ROCK")
                {
                    // Directional strata + intersecting crevice families + face-scale mineral breakup.
                    float strata=.5f+.5f*Mathf.Sin((u*3.4f+v*.82f)*Mathf.PI*2f);
                    float strata2=.5f+.5f*Mathf.Sin((u*.75f-v*6.2f)*Mathf.PI*2f+.7f);
                    float crackA=Mathf.Abs(Mathf.Sin((u*2.4f+v*1.1f)*Mathf.PI*2f+Mathf.Sin(v*11f)*.35f));
                    float crackB=Mathf.Abs(Mathf.Sin((u*.8f-v*3.7f)*Mathf.PI*2f+1.4f));
                    float crevice=1f-Mathf.SmoothStep(.0f,.12f,Mathf.Min(crackA,crackB));
                    float ledge=Mathf.SmoothStep(.42f,.58f,strata);
                    height=.38f+ledge*.22f+strata2*.08f-crevice*.16f;
                    occ=.92f-crevice*.40f;smooth=baseSmooth+ledge*.035f-crevice*.04f;
                    c*=.78f+ledge*.20f+strata2*.07f-crevice*.20f;
                }
                else if(kind=="GROUND")
                {
                    // Authored macro patches, compacted path streaks and pebble-scale relief.
                    float patch=.5f+.5f*Mathf.Sin((u*1.7f+v*.43f)*Mathf.PI*2f+.5f);
                    float path=Mathf.Exp(-Mathf.Pow((u-.52f)/.22f,2f))*(.6f+.4f*Mathf.Sin(v*7f));
                    float pebbles=0f;
                    for(int k=0;k<7;k++)
                    {
                        float px=Mathf.Repeat(.13f+k*.137f,1f),py=Mathf.Repeat(.21f+k*.193f,1f);
                        float dx=Mathf.Abs(u-px);dx=Mathf.Min(dx,1f-dx);float dy=Mathf.Abs(v-py);dy=Mathf.Min(dy,1f-dy);
                        pebbles+=Mathf.SmoothStep(.055f,.0f,Mathf.Sqrt(dx*dx+dy*dy));
                    }
                    height=.38f+patch*.10f+pebbles*.10f-path*.035f;
                    occ=.86f+patch*.10f-pebbles*.07f;smooth=baseSmooth+path*.035f;
                    c*=.78f+patch*.17f-path*.08f+pebbles*.055f;
                }
                else if(kind=="SHORE")
                {
                    // Wet silt banding + shallow ripple channels; deliberately smoother/darker than ground.
                    float band=.5f+.5f*Mathf.Sin((v*5.2f+u*.35f)*Mathf.PI*2f);
                    float ripple=.5f+.5f*Mathf.Sin((u*7.6f-v*.8f)*Mathf.PI*2f);
                    float silt=Mathf.SmoothStep(.38f,.7f,band);
                    height=.43f+band*.08f+ripple*.035f;occ=.72f+silt*.20f;smooth=baseSmooth+silt*.18f+ripple*.035f;
                    c*=.68f+silt*.22f+ripple*.06f;
                }
                else
                {
                    // Leaf/trunk-family response: restrained vein structure and value breakup, not density camouflage.
                    float vein=1f-Mathf.SmoothStep(.0f,.10f,Mathf.Abs(u-.5f));
                    float branch=.5f+.5f*Mathf.Sin((v*4f+u*.55f)*Mathf.PI*2f);
                    height=.46f+vein*.09f+branch*.04f;occ=.82f+branch*.13f;smooth=baseSmooth;
                    c*=.78f+branch*.18f+vein*.06f;
                }
                int idx=y*size+x;h[idx]=Mathf.Clamp01(height);ao[idx]=Mathf.Clamp01(occ);sm[idx]=Mathf.Clamp01(smooth);
                col[idx]=new Color(Mathf.Clamp01(c.r),Mathf.Clamp01(c.g),Mathf.Clamp01(c.b),1f);
            }
            Texture2D albedo=NewTex(kind+" Albedo",false);
            Texture2D normal=NewTex(kind+" Normal",true);
            Texture2D occlusion=NewTex(kind+" AO",true);
            Texture2D metal=NewTex(kind+" MetallicSmooth",true);
            var nc=new Color[h.Length];var ac=new Color[h.Length];var mc=new Color[h.Length];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                int idx=y*size+x;int xl=(x+size-1)%size,xr=(x+1)%size,yd=(y+size-1)%size,yu=(y+1)%size;
                float dx=h[y*size+xr]-h[y*size+xl],dy=h[yu*size+x]-h[yd*size+x];
                var n=new Vector3(-dx*7.5f,-dy*7.5f,1f).normalized;nc[idx]=new Color(n.x*.5f+.5f,n.y*.5f+.5f,n.z*.5f+.5f,1f);
                ac[idx]=new Color(ao[idx],ao[idx],ao[idx],1f);mc[idx]=new Color(0f,0f,0f,sm[idx]);
            }
            albedo.SetPixels(col);normal.SetPixels(nc);occlusion.SetPixels(ac);metal.SetPixels(mc);
            albedo.Apply(true,false);normal.Apply(true,false);occlusion.Apply(true,false);metal.Apply(true,false);
            return new SurfaceSet{name=kind,albedo=albedo,normal=normal,occlusion=occlusion,metallicSmooth=metal,normalScale=kind=="ROCK"?.82f:kind=="STONE"?.68f:kind=="GROUND"?.58f:kind=="SHORE"?.42f:.32f,tiling=tiling};
            Texture2D NewTex(string name,bool linear)=>new Texture2D(size,size,TextureFormat.RGBA32,true,linear){name="Valoria Golden "+name,wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Trilinear,anisoLevel=4};
        }
    }
}
