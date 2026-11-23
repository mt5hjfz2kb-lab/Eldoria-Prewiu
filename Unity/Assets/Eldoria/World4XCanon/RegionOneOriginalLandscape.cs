using System;
using System.Collections.Generic;
using Eldoria.ReferenceClean;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eldoria.World4XCanon
{
    // Only the height, land cover and crossing coordinates come from the physical map.
    // These meshes and illustrated textures are display-only and have no colliders.
    public sealed class RegionOneOriginalLandscape : MonoBehaviour
    {
        [Serializable] sealed class Crossing {public float x,z,width,length;}
        [Serializable] sealed class Region {public Crossing[] bridges;}
        const string Art="World4X/Illustrated/";
        readonly List<UnityEngine.Object> owned=new List<UnityEngine.Object>();
        Vector3 home;
        Transform surface;
        RegionOneModularArt kit;
        ValoriaIntegratedWorld host;
        RegionOneLookDevelopment look;
        readonly List<(Transform target,Renderer art,string id)> campaign=new List<(Transform,Renderer,string)>();
        float Ground(float x,float z)=>RegionOneBlenderLandscape.GeographicHeight(x,z);
        static Texture2D Texture(string name)=>Resources.Load<Texture2D>(Art+name);
        public static System.Collections.IEnumerator Install(Transform parent,ValoriaIntegratedWorld host,RegionOneModularArt kit)
        {
            var service=parent.gameObject.AddComponent<RegionOneOriginalLandscape>();
            service.home=host.WorldHome;service.kit=kit;service.host=host;
            service.surface=new GameObject("Illustrated Region 1").transform;
            service.surface.SetParent(parent,false);service.surface.position=service.home;
            var build=service.Build();
            while(build.MoveNext())yield return null;
            foreach(string id in new[]{"corrupt-scout","engendro-valoria"}){
                var target=host.FindTarget(id);
                if(!target)continue;
                var visual=kit.Place(target.transform,id=="corrupt-scout"?"Camp":"Spawn",id=="corrupt-scout"?15:13,target.transform.position);
                service.campaign.Add((target.transform,visual,id));
            }
        }
        System.Collections.IEnumerator Build()
        {
            var ground=new Material(Shader.Find("Eldoria/Region1/Illustrated ground"));
            owned.Add(ground);
            ground.SetTexture("_NearFeatures",Texture("NearFeatures"));
            ground.SetTexture("_FaultPigment",Texture("FaultPigment"));
            ground.SetTexture("_Region",Texture("RegionColour"));ground.SetTexture("_Weights",Texture("SurfaceWeights"));
            foreach(string k in new[]{"Grass","Earth","Slate","Water"})ground.SetTexture("_"+k,Texture(k));
            // One material on the actual canonical relief. The city paint
            // replaces the old full-map pigment; nothing is placed over it.
            var cityMeadow=Resources.Load<Texture2D>("ValoriaModular25D/BastionMeadow");
            var cityEarth=Resources.Load<Texture2D>("ValoriaModular25D/HaulersEarth");
            var cityCobble=Resources.Load<Texture2D>("ValoriaModular25D/OrganicCobbleRoad");
            if(!cityMeadow || !cityEarth || !cityCobble)
                throw new InvalidOperationException("REGION1_MODULAR_ART_MISSING: approved Bastion I meadow and both road textures are required");
            foreach(var texture in new[]{cityMeadow,cityEarth,cityCobble}){
                texture.wrapMode=TextureWrapMode.Repeat;
                texture.filterMode=FilterMode.Trilinear;
            }
            ground.SetTexture("_CityMeadow",cityMeadow);
            ground.SetTexture("_CityEarth",cityEarth);
            ground.SetTexture("_CityCobble",cityCobble);
            ground.SetFloat("_CityFinish",1f);
            Debug.Log("REGION1_MODULAR_GROUND_REPLACED same_city_meadow=1 same_city_earth=1 same_city_cobble=1");
            var hinterland = new Material(ground);
            hinterland.SetFloat("_BeyondMap", 1f);
            owned.Add(hinterland);
            look=transform.GetComponentInChildren<RegionOneLookDevelopment>(true);
            if(!look)throw new InvalidOperationException("Region 1 look development must exist before illustrated relief");
            look.RegisterPeripheralMaterial(ground);
            look.RegisterPeripheralMaterial(hinterland);
            var slice=new RegionOneFrameSlice(14);
            const int columns=228,rows=450,tile=24;
            for(int z=0;z<rows;z+=tile)for(int x=0;x<columns;x+=tile){
                int width=Mathf.Min(tile,columns-x),depth=Mathf.Min(tile,rows-z);
                var v=new Vector3[(width+1)*(depth+1)];var uv=new Vector2[v.Length];
                var tris=new int[width*depth*6];int t=0;
                for(int j=0;j<=depth;j++)for(int i=0;i<=width;i++){
                    int index=j*(width+1)+i;
                    var coord=new Vector2((x+i)/(float)columns,(z+j)/(float)rows);
                    float px=(coord.x-.5f)*1820,pz=(coord.y-.5f)*3600;
                    uv[index]=coord;v[index]=new Vector3(px,Ground(px,pz),pz);
                    if(i==width||j==depth)continue;
                    tris[t++]=index;tris[t++]=index+width+1;tris[t++]=index+1;
                    tris[t++]=index+1;tris[t++]=index+width+1;tris[t++]=index+width+2;
                }
                var mesh=new Mesh{name="Painted regional relief",vertices=v,uv=uv,triangles=tris};
                mesh.RecalculateBounds();Draw(mesh,ground);
                if(ValoriaIntegratedWorld.P0LazyWorldProbe&&slice.ShouldYield()){yield return null;slice.Resume();}
            }
            Draw(BuildHinterland(), hinterland);
            var planting=Plant();
            while(planting.MoveNext())yield return null;
            var layout=JsonUtility.FromJson<Region>(Resources.Load<TextAsset>("World4X/Blender/production-region").text);
            foreach(var b in layout.bridges)
            {
                // The short entrance painted for the old fixed capital is purely scenic.
                // Keep both regional crossings and the complete underlying navigation map.
                if(Mathf.Abs(b.x-3.3f)<.1f && Mathf.Abs(b.z+18.5f)<.1f)continue;
                Decal("Bridge",b.x,b.z,b.width*1.35f,b.length,0,.7f);
            }
        }
        // A single, open-centred strip continues the surveyed relief. The inner
        // samples coincide with the outermost tile vertices on all four sides.
        Mesh BuildHinterland()
        {
            float[] distances = { 0, 20, 50, 95, 155, 240, 360, 540, 800, 1200, 1850, 2900, 4500, 6800 };
            var vertices = new List<Vector3>();
            var uv = new List<Vector2>();
            var triangles = new List<int>();
            void Bank(Vector2 start, Vector2 end, int steps)
            {
                int first = vertices.Count;
                foreach (float distance in distances)
                {
                    for (int j = 0; j <= steps; j++)
                    {
                        Vector2 shore = Vector2.Lerp(start, end, j / (float)steps);
                        float x = shore.x * (1 + distance / 910f);
                        float z = shore.y * (1 + distance / 1800f);
                        float basin = Mathf.PerlinNoise(x * .0014f + 32, z * .0018f + 19);
                        float fold = 1 - Mathf.Abs(2 * Mathf.PerlinNoise(x * .0024f + z * .0007f, z * .0031f + 46) - 1);
                        float hills = 12 + basin * basin * 145 + fold * fold * 83;
                        float coast = Mathf.SmoothStep(0, 1, Mathf.Clamp01((shore.x - 600) / 310));
                        float height = Mathf.Lerp(Ground(shore.x, shore.y), hills * (1 - coast),
                            1 - Mathf.Exp(-distance / 265));
                        vertices.Add(new Vector3(x, height, z));
                        uv.Add(new Vector2(x / 1820 + .5f, z / 3600 + .5f));
                    }
                }
                int stride = steps + 1;
                for (int k = 1; k < distances.Length; k++)
                for (int j = 0; j < steps; j++)
                {
                    int n = first + k * stride + j;
                    triangles.Add(n); triangles.Add(n - stride); triangles.Add(n + 1);
                    triangles.Add(n + 1); triangles.Add(n - stride); triangles.Add(n - stride + 1);
                }
            }
            Bank(new Vector2(-910,-1800),new Vector2(910,-1800),228);
            Bank(new Vector2(910,-1800),new Vector2(910,1800),450);
            Bank(new Vector2(910,1800),new Vector2(-910,1800),228);
            Bank(new Vector2(-910,1800),new Vector2(-910,-1800),450);
            var mesh = new Mesh { name = "Uncharted regional foothills" };
            mesh.SetVertices(vertices); mesh.SetUVs(0,uv); mesh.SetTriangles(triangles,0);
            mesh.RecalculateBounds();
            return mesh;
        }
        // Spatial batches keep woodland and rocks visible in the same geography at every scale.
        System.Collections.IEnumerator Plant()
        {
            var slice=new RegionOneFrameSlice(10);
            var rng=new System.Random(27132);
            var mats=new Dictionary<string,Material>();
            var plantedPivots=new Dictionary<string,float>();
            // Quality review of ACTUAL PNGs found pine_0/1 to be low-poly
            // green clay silhouettes, unlike the illustrated Bastion I canopy.
            // Replace those images entirely with painterly Fir/Grove sprites,
            // retaining the city-approved ancient trees and the canonical
            // forest positions. One mesh/material per family, no additive art.
            string[] names={"CityOak","CityGrove","PaintedFir","PaintedForestGrove","Crag","Ridge"};
            foreach(string k in names){
                string source=k=="CityOak"?"ValoriaModular25D/RecoveredProps/ArbolAntiguo"
                    :k=="CityGrove"?"ValoriaModular25D/RecoveredProps/ArboledaAntigua"
                    :k=="PaintedFir"?Art+"Fir"
                    :k=="PaintedForestGrove"?Art+"Grove"
                    :Art+k;
                var sourceArt=Resources.Load<Texture2D>(source);
                if(!sourceArt)throw new InvalidOperationException("REGION1_MODULAR_FOREST_MISSING "+source);
                var material=new Material(Shader.Find("Eldoria/World4X/LocalCanopy")){
                    mainTexture=sourceArt,name="Region 1 modular | "+k};
                material.SetFloat("_SoftEdges",1f);
                // The foot remains at the certified forest coordinate while
                // the illustrated crown renders in front of adjacent relief.
                material.SetFloat("_CameraFacingDepth",9f);
                mats.Add(k,material);owned.Add(material);look.RegisterPeripheralMaterial(material);
                var approved=Resources.Load<Sprite>(source);
                plantedPivots[k]=approved?RegionOneModularArt.WorldPivot(source,approved).y:.035f;
            }
            var groves=new Dictionary<Vector2Int,List<Vector3>>();
            var positions=Resources.Load<TextAsset>(Art+"ForestPositions");
            using(var reader=new System.IO.BinaryReader(new System.IO.MemoryStream(positions.bytes))){
                int count=reader.ReadInt32();
                for(int i=0;i<count;i++){
                    var tree=new Vector3(reader.ReadSingle(),0,reader.ReadSingle());
                    tree.y=Mathf.Clamp(reader.ReadSingle()*4.6f,4f,11.5f);
                    // Preserve recognisable groves and open the approach to water and roads.
                    // Disrupt repeated planting rows inside each existing forest stand.
                    float gap = Mathf.PerlinNoise(tree.x*.041f+11,tree.z*.032f+5);
                    float drift = Mathf.PerlinNoise(tree.z*.026f+17,tree.x*.037f+3);
                    tree.x=Mathf.Clamp(tree.x+(gap-.5f)*9,-909.9f,909.9f);
                    tree.z=Mathf.Clamp(tree.z+(drift-.5f)*8,-1799.9f,1799.9f);
                    float stand=Mathf.PerlinNoise(tree.x*.016f+7,tree.z*.016f+41);
                    float fringe=Mathf.PerlinNoise(tree.x*.051f+21,tree.z*.046f+9);
                    if(stand+.34f*(fringe-.5f)<.43f || RegionOneBlenderLandscape.Shore(tree.x,tree.z)<4f ||
                       RegionOneBlenderLandscape.Road(tree.x,tree.z)>.15f) continue;
                    var cell=new Vector2Int(Mathf.FloorToInt((tree.x+910f)/182f),Mathf.FloorToInt((tree.z+1800f)/180f));
                    if(!groves.TryGetValue(cell,out var clump)){clump=new List<Vector3>();groves.Add(cell,clump);}
                    clump.Add(tree);
                }
            }
            PlantBanksAndWoodland(groves);
            for(int bz=-1800;bz<1800;bz+=180)for(int bx=-910;bx<910;bx+=182){
                var batches=new Dictionary<string,List<Vector3>>();
                foreach(var k in names)batches[k]=new List<Vector3>();
                var cityTrees=new Dictionary<Vector3,float>();
                var tileId=new Vector2Int((bx+910)/182,(bz+1800)/180);
                if(groves.TryGetValue(tileId,out var clump))foreach(var tree in clump){
                    float px=tree.x,pz=tree.z;
                    var foot=new Vector3(px,Ground(px,pz)+.12f,pz);
                    int pick=Mathf.Abs(Mathf.RoundToInt(px*3.1f+pz*7.7f))%13;
                    float stand=Mathf.PerlinNoise(px*.009f+8,pz*.009f+3);
                    string family=stand>.55f ? (pick<9?"PaintedFir":"PaintedForestGrove") :
                        (pick<8?"CityOak":pick<12?"CityGrove":"PaintedForestGrove");
                    batches[family].Add(foot);
                    float core=Mathf.InverseLerp(.36f,.69f,Mathf.PerlinNoise(px*.016f+7,pz*.016f+41));
                    // Grove art depicts several crowns; sizing it like a single fir
                    // compressed groups into rows of identical miniature silhouettes.
                    float breadth=family=="CityGrove" || family=="PaintedForestGrove" ? 1.52f : 1.08f;
                    cityTrees[foot]=tree.y*breadth*Mathf.Lerp(.68f,1.34f,core)*Mathf.Lerp(.9f,1.1f,stand);
                }
                // Rock outcrops punctuate clearings. All trees come from the authored stands.
                for(int n=0;n<40;n++){
                    float x=bx+(float)rng.NextDouble()*182,z=bz+(float)rng.NextDouble()*180;
                    if(RegionOneBlenderLandscape.Shore(x,z)<4f || RegionOneBlenderLandscape.Water(x,z)>.05f ||
                       RegionOneBlenderLandscape.Road(x,z)>.15f || x*x+z*z<500f) continue;
                    if(Mathf.PerlinNoise(x*.036f+31,z*.036f+90)<.67f)continue;
                    batches[rng.Next(6)==0?"Ridge":"Crag"].Add(new Vector3(x,Ground(x,z)+.16f,z));
                }
                foreach(var pair in batches){
                    if(pair.Value.Count==0)continue;
                    var v=new List<Vector3>();var uv=new List<Vector2>();var offsets=new List<Vector2>();var triangles=new List<int>();
                    var tex=mats[pair.Key].mainTexture;
                    foreach(var foot in pair.Value){
                        float width=(pair.Key=="Ridge"?20f:pair.Key=="Crag"?7f:
                            pair.Key=="CityGrove"?20.5f:15.3f)*(.72f+(float)rng.NextDouble()*.62f);
                        if(cityTrees.TryGetValue(foot,out float crownWidth))width=crownWidth;
                        float h=width*tex.height/tex.width;int start=v.Count;
                        bool mirror=cityTrees.ContainsKey(foot) && Mathf.Sin(foot.x*.91f+foot.z*.43f)>0;
                        foreach(var corner in new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up}){
                            v.Add(foot);uv.Add(new Vector2(mirror?1f-corner.x:corner.x,corner.y));
                            offsets.Add(new Vector2((corner.x-.5f)*width,
                                (corner.y-plantedPivots[pair.Key])*h));
                        }
                        triangles.AddRange(new[]{start,start+1,start+2,start,start+2,start+3});
                    }
                    var mesh=new Mesh{name="Illustrated "+pair.Key};
                    mesh.SetVertices(v);mesh.SetUVs(0,uv);mesh.SetUVs(1,offsets);mesh.SetTriangles(triangles,0);
                    var bounds=mesh.bounds;bounds.Expand(60);mesh.bounds=bounds;
                    Draw(mesh,mats[pair.Key]);
                }
                if(ValoriaIntegratedWorld.P0LazyWorldProbe&&slice.ShouldYield()){yield return null;slice.Resume();}
            }
        }
        // A single batched foreground material shares the painted light across
        // reeds, slate and fallen wood. Its geometry never enters spatial queries.
        void PlantBanksAndWoodland(Dictionary<Vector2Int,List<Vector3>> groves)
        {
            var v=new List<Vector3>(); var uv=new List<Vector2>();
            var offsets=new List<Vector2>(); var triangles=new List<int>();
            void Place(float x,float z,int tile,float width)
            {
                int first=v.Count;
                var foot=new Vector3(x,Ground(x,z)+.12f,z);
                for(int i=0;i<4;i++)
                {
                    float u=(i==1||i==2)?1:0, h=i>=2?1:0;
                    v.Add(foot); uv.Add(new Vector2((tile+u)*.25f,h));
                    offsets.Add(new Vector2((u-.5f)*width,(h-.035f)*width));
                }
                triangles.Add(first);triangles.Add(first+1);triangles.Add(first+2);
                triangles.Add(first);triangles.Add(first+2);triangles.Add(first+3);
            }
            var sites=Resources.Load<TextAsset>(Art+"BankPositions");
            using(var reader=new System.IO.BinaryReader(new System.IO.MemoryStream(sites.bytes)))
            {
                int count=reader.ReadInt32();
                for(int n=0;n<count;n++)
                    Place(reader.ReadSingle(),reader.ReadSingle(),reader.ReadByte(),reader.ReadSingle());
            }
            var rng=new System.Random(4427);
            foreach(var group in groves.Values)foreach(var tree in group)
            {
                if(tree.x < -200 || tree.x > 216 || tree.z < -148 || tree.z > 364 || rng.Next(4)!=0)continue;
                float x=tree.x+(float)rng.NextDouble()*4f-2f,z=tree.z-2.6f;
                if(x*x+z*z<15f*15f || RegionOneBlenderLandscape.Water(x,z)>.01f ||
                   RegionOneBlenderLandscape.Road(x,z)>.12f)continue;
                Place(x,z,2,Mathf.Clamp(tree.y*.72f,2.4f,5.8f));
            }
            var mesh=new Mesh{name="Painted riverbanks and woodland margins"};
            mesh.SetVertices(v);mesh.SetUVs(0,uv);mesh.SetUVs(1,offsets);mesh.SetTriangles(triangles,0);
            var bounds=mesh.bounds;bounds.Expand(16f);mesh.bounds=bounds;
            var material=new Material(Shader.Find("Eldoria/World4X/LocalCanopy")){mainTexture=Texture("MarginAtlas")};
            material.SetFloat("_SoftEdges",1f);
            owned.Add(material);look.RegisterPeripheralMaterial(material);Draw(mesh,material);
        }
        MeshRenderer Draw(Mesh mesh,Material mat)
        {
            var go=new GameObject(mesh.name);go.layer=29;go.transform.SetParent(surface,false);
            go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=mat;
            renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;owned.Add(mesh);
            return renderer;
        }
        readonly Dictionary<string,Material> decals=new Dictionary<string,Material>();
        Material DecalMaterial(string name)
        {
            if(!decals.TryGetValue(name,out var mat)){
                mat=new Material(Shader.Find("Eldoria/World4X/LocalCanopy"));mat.mainTexture=Texture(name);
                mat.SetFloat("_SoftEdges",1f);
                if(name=="Fault")mat.SetFloat("_FaultLight",1f);
                owned.Add(mat);look.RegisterPeripheralMaterial(mat);decals.Add(name,mat);
            }
            return mat;
        }
        MeshRenderer Decal(string name,float x,float z,float width,float depth,float angle,float lift)
        {
            var mat=DecalMaterial(name);
            const int n=12;
            var v=new Vector3[(n+1)*(n+1)];var uv=new Vector2[v.Length];var tri=new int[n*n*6];int t=0;
            var rotate=Quaternion.Euler(0,-angle,0);
            for(int j=0;j<=n;j++)for(int i=0;i<=n;i++){
                int k=j*(n+1)+i;uv[k]=new Vector2(i/(float)n,j/(float)n);
                var offset=rotate*new Vector3((uv[k].x-.5f)*width,0,(uv[k].y-.5f)*depth);
                float px=x+offset.x,pz=z+offset.z;
                v[k]=new Vector3(px,Ground(px,pz)+lift,pz);
                if(i==n||j==n)continue;
                tri[t++]=k;tri[t++]=k+n+1;tri[t++]=k+1;
                tri[t++]=k+1;tri[t++]=k+n+1;tri[t++]=k+n+2;
            }
            var mesh=new Mesh{name="Painted "+name,vertices=v,uv=uv,triangles=tri};mesh.RecalculateBounds();return Draw(mesh,mat);
        }
        static readonly string[] ChapterIds={"hunt-wolf","hunt-boar","meat-valoria","ash-stalker-valoria","fissure-valoria"};
        readonly HashSet<string> illustratedChapter=new HashSet<string>();
        void BindChapter()
        {
            foreach(var id in ChapterIds){
                if(illustratedChapter.Contains(id))continue;
                var target=host.FindTarget(id);
                if(!target)continue;
                foreach(var donor in target.GetComponentsInChildren<Renderer>(true))donor.forceRenderingOff=true;
                if(id=="fissure-valoria"){
                    var local=target.transform.position-home;
                    var open=Decal("Fault",local.x,local.z,18,8,19,.32f);
                    var sealedScar=Decal("SealedFault",local.x,local.z,16,8,19,.32f);
                    campaign.Add((target.transform,open,id));
                    campaign.Add((target.transform,sealedScar,"closing-scar"));
                }else{
                    var art=kit.Place(surface,RegionOneModularArt.Illustration(id),9,target.transform.position);
                    campaign.Add((target.transform,art,id));
                }
                illustratedChapter.Add(id);
            }
        }
        Eldoria.Domain.PlayerState snapshot;
        float nextRead;
        void LateUpdate()
        {
            if(!host||!host.Ready||!host.InWorld||campaign.Count==0)return;
            if(snapshot==null||Time.unscaledTime>=nextRead){snapshot=host.City.Gateway.Snapshot();nextRead=Time.unscaledTime+.25f;if(snapshot.BastionLevel>=3&&illustratedChapter.Count<ChapterIds.Length)BindChapter();}
            var state=snapshot;
            bool visible=!RegionOneResourcePlacement.IsPanoramic(host.WorldCamera.orthographicSize,host.WorldNavigation.globalZoom);
            foreach(var entry in campaign){
                var pos=entry.target.position;
                pos.y=home.y+Ground(pos.x-home.x,pos.z-home.z)+.3f;
                if(entry.id!="fissure-valoria" && entry.id!="closing-scar")
                    entry.art.transform.position=pos;
                bool available=entry.target.gameObject.activeInHierarchy;
                if(entry.id=="corrupt-scout")available&=!state.ScoutDefeated;
                if(entry.id=="engendro-valoria")available&=!state.EngendroDefeated;
                if(entry.id=="fissure-valoria")available&=!state.ChapterThree.FissureClosed;
                if(entry.id=="closing-scar")available&=state.ChapterThree.FissureClosed;
                entry.art.enabled=visible&&available;
            }
        }
        void OnDestroy(){foreach(var item in owned)if(item)Destroy(item);}
    }
}

