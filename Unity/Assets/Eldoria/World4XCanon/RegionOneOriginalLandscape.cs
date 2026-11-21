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
        float Ground(float x,float z)=>RegionOneBlenderLandscape.GeographicHeight(x,z);
        static Texture2D Texture(string name)=>Resources.Load<Texture2D>(Art+name);
        public static void Install(Transform parent,ValoriaIntegratedWorld host,RegionOneModularArt kit)
        {
            var service=parent.gameObject.AddComponent<RegionOneOriginalLandscape>();
            service.home=host.WorldHome;service.kit=kit;service.host=host;
            service.surface=new GameObject("Illustrated Region 1").transform;
            service.surface.SetParent(parent,false);service.surface.position=service.home;
            service.Build();
            foreach(string id in new[]{"corrupt-scout","engendro-valoria"}){
                var target=host.FindTarget(id);
                if(!target)continue;
                var visual=kit.Place(target.transform,id=="corrupt-scout"?"Camp":"Spawn",id=="corrupt-scout"?15:13,target.transform.position);
                service.campaign.Add((target.transform,visual,id));
            }
        }
        void Build()
        {
            var ground=new Material(Shader.Find("Eldoria/Region1/Illustrated ground"));
            owned.Add(ground);
            ground.SetTexture("_Region",Texture("RegionColour"));ground.SetTexture("_Weights",Texture("SurfaceWeights"));
            foreach(string k in new[]{"Grass","Earth","Slate","Water"})ground.SetTexture("_"+k,Texture(k));
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
            }
            Plant();
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
        void Plant()
        {
            var rng=new System.Random(27132);
            var mats=new Dictionary<string,Material>();
            string[] names={"Fir","Oak","Grove","Crag","Ridge"};
            foreach(string k in names){
                var material=new Material(Shader.Find("Eldoria/World4X/LocalCanopy")){mainTexture=Texture(k)};
                mats.Add(k,material);owned.Add(material);
            }
            for(int bz=-1800;bz<1800;bz+=180)for(int bx=-910;bx<910;bx+=182){
                var batches=new Dictionary<string,List<Vector3>>();
                foreach(var k in names)batches[k]=new List<Vector3>();
                for(int n=0;n<115;n++){
                    float x=bx+(float)rng.NextDouble()*182,z=bz+(float)rng.NextDouble()*180;
                    if(RegionOneBlenderLandscape.Water(x,z)>.08f||RegionOneBlenderLandscape.Shore(x,z)<2f||RegionOneBlenderLandscape.Road(x,z)>.22f)continue;
                    float forest=RegionOneBlenderLandscape.Forest(x,z);
                    float patch=Mathf.PerlinNoise(x*.036f+31,z*.036f+90);
                    bool tree=rng.NextDouble()<forest*.92f || patch>.64f&&rng.NextDouble()<.32f;
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
            }
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
        Eldoria.Domain.PlayerState snapshot;
        float nextRead;
        void LateUpdate()
        {
            if(!host||!host.Ready||!host.InWorld||campaign.Count==0)return;
            if(snapshot==null||Time.unscaledTime>=nextRead){snapshot=host.City.Gateway.Snapshot();nextRead=Time.unscaledTime+.25f;}
            var state=snapshot;
            bool visible=!RegionOneResourcePlacement.IsPanoramic(host.WorldCamera.orthographicSize,host.WorldNavigation.globalZoom);
            foreach(var entry in campaign){
                var pos=entry.target.position;
                pos.y=home.y+Ground(pos.x-home.x,pos.z-home.z)+.3f;
                entry.art.transform.position=pos;
                entry.art.enabled=visible && (entry.id=="corrupt-scout"?!state.ScoutDefeated:!state.EngendroDefeated);
            }
        }
        void OnDestroy(){foreach(var item in owned)if(item)Destroy(item);}
    }
}
