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
        [Serializable] sealed class Fault {public float ax,az,bx,bz,radius;}
        [Serializable] sealed class Region {public Crossing[] bridges; public Fault[] faults;}
        const string Art="World4X/Illustrated/";
        readonly List<UnityEngine.Object> owned=new List<UnityEngine.Object>();
        Vector3 home;
        Transform surface;
        RegionOneModularArt kit;
        ValoriaIntegratedWorld host;
        readonly List<(Transform target,Renderer art,string id)> campaign=new List<(Transform,Renderer,string)>();
        Renderer villageMill,villageGarrison,villageGranary;
        Renderer CityDetail(string key,float x,float z,float width)
        {
            var foot=home+new Vector3(x,Ground(x,z)+.16f,z);
            return kit.Place(surface,"ValoriaModular25D/"+key,width,foot);
        }
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
            ground.SetTexture("_Region",Texture("RegionColour"));ground.SetTexture("_Weights",Texture("SurfaceWeights"));
            foreach(string k in new[]{"Grass","Earth","Slate","Water"})ground.SetTexture("_"+k,Texture(k));
            // Extend only the painted mist beyond the known region. Keep the
            // backing immediately below the lowest canonical height. The former
            // fixed y=-90 plane was safe, but the inclined panorama projected
            // that 90 m vertical gap beside the eastern relief as a second,
            // wavy dark silhouette. GeographicBounds comes from the unchanged
            // query export; the backing remains non-interactive and below every
            // playable point.
            var fogMesh=new Mesh{name="Mist beyond the mapped region"};
            const float horizon=6500f;
            float fogHeight=RegionOneBlenderLandscape.GeographicBounds(Vector3.zero).min.y-.5f;
            fogMesh.vertices=new[]{new Vector3(-horizon,fogHeight,-horizon),new Vector3(-horizon,fogHeight,horizon),
                new Vector3(horizon,fogHeight,horizon),new Vector3(horizon,fogHeight,-horizon)};
            // Keep mist variations aligned with the geographic UVs used by the relief.
            var fogUv=new Vector2[4];var corners=fogMesh.vertices;
            for(int i=0;i<corners.Length;i++) fogUv[i]=new Vector2(corners[i].x/1820f+.5f,corners[i].z/3600f+.5f);
            fogMesh.uv=fogUv;fogMesh.triangles=new[]{0,1,2,0,2,3};fogMesh.RecalculateBounds();
            var fogMaterial=new Material(ground);fogMaterial.SetFloat("_BeyondMap",1f);
            owned.Add(fogMaterial);Draw(fogMesh,fogMaterial);
            // At panoramic zoom both the relief edge and its backing must meet
            // the exact camera haze used by the imported continuation. Earlier
            // builds left the illustrated backing dark on the eastern coast.
            var look=transform.GetComponentInChildren<RegionOneLookDevelopment>(true);
            if(!look)throw new InvalidOperationException("Region 1 look development must exist before illustrated relief");
            look.RegisterPeripheralMaterial(ground);
            look.RegisterPeripheralMaterial(fogMaterial);
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
            // The city landmark meets its approaches across the existing bridge.
            // These fixed physical widths recede naturally at regional zoom.
            CityDetail("RecoveredProps/PozoPiedra",-10,-4,3.8f);
            CityDetail("RecoveredProps/ArboledaAntigua",-20,17,7.3f);
            CityDetail("RecoveredProps/MatojosFlores",13,12,3.2f);
            villageMill=CityDetail("sawmill_i",-16,7,10.4f);
            villageGarrison=CityDetail("Progression/barracks",17,16,10.6f);
            villageGranary=CityDetail("Progression/granary",-4,20,9.4f);
            villageMill.enabled=villageGarrison.enabled=villageGranary.enabled=false;
            var planting=Plant();
            while(planting.MoveNext())yield return null;
            var layout=JsonUtility.FromJson<Region>(Resources.Load<TextAsset>("World4X/Blender/production-region").text);
            foreach(var b in layout.bridges)Decal("Bridge",b.x,b.z,b.width*1.35f,b.length,0,.7f);
            foreach(var f in layout.faults){
                float dx=f.bx-f.ax,dz=f.bz-f.az,len=Mathf.Sqrt(dx*dx+dz*dz);
                // The long axis of the painted opening angles up from left to right.
                float angle=Mathf.Atan2(dz,dx)*Mathf.Rad2Deg-27f;
                Decal("Fault",(f.ax+f.bx)*.5f,(f.az+f.bz)*.5f,len*1.55f,len*.96f,angle,.24f);
            }
        }
        // Spatial batches keep woodland and rocks visible in the same geography at every scale.
        System.Collections.IEnumerator Plant()
        {
            var slice=new RegionOneFrameSlice(10);
            var rng=new System.Random(27132);
            var mats=new Dictionary<string,Material>();
            string[] names={"Fir","Oak","Grove","Crag","Ridge"};
            foreach(string k in names){
                var material=new Material(Shader.Find("Eldoria/World4X/LocalCanopy")){mainTexture=Texture(k)};
                mats.Add(k,material);owned.Add(material);
            }
            var groves=new Dictionary<Vector2Int,List<Vector3>>();
            var positions=Resources.Load<TextAsset>(Art+"ForestPositions");
            using(var reader=new System.IO.BinaryReader(new System.IO.MemoryStream(positions.bytes))){
                int count=reader.ReadInt32();
                for(int i=0;i<count;i++){
                    var tree=new Vector3(reader.ReadSingle(),0,reader.ReadSingle());
                    tree.y=Mathf.Clamp(reader.ReadSingle()*4.6f,4f,11.5f);
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
                    batches[pick<9?"Fir":pick<12?"Oak":"Grove"].Add(foot);
                    cityTrees[foot]=tree.y;
                }
                for(int n=0;n<85;n++){
                    float x=bx+(float)rng.NextDouble()*182,z=bz+(float)rng.NextDouble()*180;
                    if(RegionOneBlenderLandscape.Water(x,z)>.08f||RegionOneBlenderLandscape.Shore(x,z)<2f||RegionOneBlenderLandscape.Road(x,z)>.22f)continue;
                    float forest=RegionOneBlenderLandscape.Forest(x,z);
                    float patch=Mathf.PerlinNoise(x*.036f+31,z*.036f+90);
                    bool tree=rng.NextDouble()<forest*.18f || patch>.67f&&rng.NextDouble()<.14f;
                    if(!tree && rng.NextDouble()>(patch>.67f?.095:.012))continue;
                    if(Mathf.Abs(x)<15&&Mathf.Abs(z)<22)continue;
                    int type=tree?(patch>.67f?2:rng.Next(2)):(patch>.59f?4:3);
                    batches[names[type]].Add(new Vector3(x,Ground(x,z)+.16f,z));
                }
                foreach(var pair in batches){
                    if(pair.Value.Count==0)continue;
                    var v=new List<Vector3>();var uv=new List<Vector2>();var offsets=new List<Vector2>();var triangles=new List<int>();
                    var tex=mats[pair.Key].mainTexture;
                    foreach(var foot in pair.Value){
                        float width=(pair.Key=="Ridge"?20f:pair.Key=="Crag"?7f:13f)*(.72f+(float)rng.NextDouble()*.62f);
                        if(cityTrees.TryGetValue(foot,out float crownWidth))width=crownWidth;
                        float h=width*tex.height/tex.width;int start=v.Count;
                        foreach(var corner in new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up}){
                            v.Add(foot);uv.Add(corner);offsets.Add(new Vector2((corner.x-.5f)*width,(corner.y-.035f)*h));
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
            owned.Add(material);Draw(mesh,material);
        }
        void Draw(Mesh mesh,Material mat)
        {
            var go=new GameObject(mesh.name);go.layer=29;go.transform.SetParent(surface,false);
            go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=mat;
            renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;owned.Add(mesh);
        }
        readonly Dictionary<string,Material> decals=new Dictionary<string,Material>();
        void Decal(string name,float x,float z,float width,float depth,float angle,float lift)
        {
            if(!decals.TryGetValue(name,out var mat)){
                mat=new Material(Shader.Find("Eldoria/World4X/LocalCanopy"));mat.mainTexture=Texture(name);
                owned.Add(mat);decals.Add(name,mat);
            }
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
            var mesh=new Mesh{name="Painted "+name,vertices=v,uv=uv,triangles=tri};mesh.RecalculateBounds();Draw(mesh,mat);
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
                var art=kit.Place(surface,RegionOneModularArt.Illustration(id),id=="fissure-valoria"?12:9,target.transform.position);
                campaign.Add((target.transform,art,id));
                if(id=="fissure-valoria")campaign.Add((target.transform,kit.Place(surface,"SealedFault",12,target.transform.position),"closing-scar"));
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
            if(villageMill){
                villageMill.enabled=state.SawmillLevel>0;
                villageGarrison.enabled=state.BarracksLevel>0;
                villageGranary.enabled=(state.ChapterThree?.GranaryLevel??0)>0;
            }
            bool visible=!RegionOneResourcePlacement.IsPanoramic(host.WorldCamera.orthographicSize,host.WorldNavigation.globalZoom);
            foreach(var entry in campaign){
                var pos=entry.target.position;
                pos.y=home.y+Ground(pos.x-home.x,pos.z-home.z)+.3f;
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

