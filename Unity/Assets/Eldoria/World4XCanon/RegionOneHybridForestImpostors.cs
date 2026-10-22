using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eldoria.World4XCanon
{
    /// <summary>
    /// Genuine hybrid 2.5D: top-down painted conifer canopies in 3D terrain XYZ,
    /// grouped into three meshes / one alpha material. Only visual LODs.
    /// They are NOT POIs, resources, camps or world cities. No physics.
    /// No web sprites, paid assets, Tripo or alternate Unity scene.
    /// </summary>
    public static class RegionOneHybridForestImpostors
    {
        public const int Revision = 1;
        public sealed class Output
        {
            public GameObject root;
            public readonly List<Mesh> meshes=new List<Mesh>();
            public Texture2D atlas;
            public Material material;
            public int trees, triangles;
        }

        static float Random01(int a,int b,int c)
        {
            unchecked
            {
                uint v=(uint)a*1664525u+(uint)b*1013904223u+(uint)c*2246822519u;
                v^=v>>16;v*=2246822519u;v^=v>>13;v*=3266489917u;v^=v>>16;
                return (v&65535u)/65535f;
            }
        }

        static Texture2D PaintConiferAtlas()
        {
            // Strategic overview is nearly top-down. Paint a canopy seen
            // from ABOVE rather than an upright tree, which becomes a black
            // 2-pixel line when projected by the 74-degree world camera.
            const int size=96;
            var pixels=new Color32[size*size];
            for(int y=0;y<size;y++)
                for(int x=0;x<size;x++)
                {
                    float dx=(x-size*.50f)/(size*.47f);
                    float dy=(y-size*.50f)/(size*.47f);
                    float a=Mathf.Atan2(dy,dx);
                    float radius=Mathf.Sqrt(dx*dx+dy*dy);
                    float jitter=Random01(x,y,39);
                    float edge=1f+.13f*Mathf.Sin(a*7f)+
                                  .10f*Mathf.Sin(a*13f+1.7f)+
                                  .09f*(jitter-.5f);
                    if(radius>edge)
                    {
                        pixels[y*size+x]=new Color32(0,0,0,0);
                        continue;
                    }
                    // Painterly needle patches, lighter on the northwest
                    // facing canopy and with a darker grounded perimeter.
                    float branches=Mathf.Sin(a*9f+radius*12f)*.10f+
                        Mathf.Sin(a*15f-radius*7f)*.06f;
                    float light=Mathf.Clamp01(.47f-.21f*dx+.20f*dy+
                        branches+.24f*(jitter-.5f));
                    float edgeShade=Mathf.SmoothStep(0f,1f,
                        Mathf.Clamp01((radius-.55f)/.45f));
                    light*=1f-.32f*edgeShade;
                    byte rr=(byte)Mathf.Clamp(Mathf.RoundToInt(
                        Mathf.Lerp(22f,91f,light)),0,255);
                    byte gg=(byte)Mathf.Clamp(Mathf.RoundToInt(
                        Mathf.Lerp(65f,151f,light)),0,255);
                    byte bb=(byte)Mathf.Clamp(Mathf.RoundToInt(
                        Mathf.Lerp(27f,77f,light)),0,255);
                    byte alpha=(byte)Mathf.RoundToInt(255f*
                        Mathf.Clamp01((edge-radius)*13f));
                    pixels[y*size+x]=new Color32(rr,gg,bb,alpha);
                }
            var atlas=new Texture2D(size,size,TextureFormat.RGBA32,true,false)
                {name="Region 1 painterly aerial evergreen crowns"};
            atlas.SetPixels32(pixels);
            atlas.Apply(true,true);
            atlas.wrapMode=TextureWrapMode.Clamp;
            atlas.filterMode=FilterMode.Bilinear;
            return atlas;
        }

        public static Output Build(Transform parent,Vector3 origin,Vector2[] centres,
            Func<float,float,float> elevation,Func<float,float,bool> woodland,
            Func<float,float,bool> water,Func<float,float,bool> roads,
            Func<float,float,bool> coast)
        {
            var shader=Shader.Find("Sprites/Default");
            if(!shader)
            {
                Debug.LogWarning("REGION1_HYBRID_FOREST_SPRITE_SHADER_MISSING: fallback procedural 3D batches");
                return null;
            }
            var result=new Output();
            result.atlas=PaintConiferAtlas();
            result.material=new Material(shader)
                {name="Region1 2.5D aerial woodland canopy material"};
            if(result.material.HasProperty("_MainTex"))
                result.material.SetTexture("_MainTex",result.atlas);
            result.material.enableInstancing=true;
            result.root=new GameObject("Region1 · batched distant organic forest canopies");
            result.root.transform.SetParent(parent,true);
            for(int band=0;band<3;band++)
            {
                var vertices=new List<Vector3>();
                var uvs=new List<Vector2>();
                var colors=new List<Color32>();
                var triangles=new List<int>();
                for(int cluster=band;cluster<centres.Length;cluster+=3)
                {
                    var centre=centres[cluster];
                    const int count=105;
                    for(int k=0;k<count;k++)
                    {
                        float angle=k*2.3999632f+cluster*.93f;
                        float radius=Mathf.Sqrt((k+.45f)/count);
                        float x=centre.x+Mathf.Cos(angle)*radius*(49f+(cluster%4)*11f);
                        float z=centre.y+Mathf.Sin(angle)*radius*(38f+(cluster%3)*10f);
                        // Exact same geographic forest definition as 2D texture.
                        // Keep genuine interaction corridor and river paths free.
                        if(!woodland(x,z)||water(x,z)||roads(x,z)||coast(x,z)||
                            x*x+z*z<95f*95f)continue;
                        float width=6.0f+Random01(cluster,k,277)*5.2f;
                        float rotation=(cluster*43f+k*17f)*Mathf.Deg2Rad;
                        float groundY=elevation(x,z)+.25f;
                        var p=new Vector3(origin.x+x,groundY,origin.z+z);
                        var tint=new Color32((byte)(217+Random01(cluster,k,111)*37f),
                            (byte)(216+Random01(cluster,k,141)*39f),
                            (byte)(203+Random01(cluster,k,191)*52f),255);
                        // Two horizontal textured canopy planes at slightly
                        // different heights create light parallax/shading from
                        // the true 3D camera without tall black billboard
                        // silhouettes or hundreds of per-tree renderers.
                        for(int layer=0;layer<2;layer++)
                        {
                            float direction=rotation+(layer==0?0f:.22f);
                            float scale=(layer==0?1f:.67f);
                            Vector3 right=new Vector3(Mathf.Cos(direction),0f,
                                Mathf.Sin(direction))*(width*scale*.5f);
                            Vector3 forward=new Vector3(-right.z,0f,right.x);
                            Vector3 centre=p+Vector3.up*(layer==0?.15f:1.10f);
                            int start=vertices.Count;
                            vertices.Add(centre-right-forward);
                            vertices.Add(centre+right-forward);
                            vertices.Add(centre+right+forward);
                            vertices.Add(centre-right+forward);
                            uvs.Add(new Vector2(0,0));
                            uvs.Add(new Vector2(1,0));
                            uvs.Add(new Vector2(1,1));
                            uvs.Add(new Vector2(0,1));
                            for(int i=0;i<4;i++)colors.Add(tint);
                            // Upward-facing winding is essential with
                            // the overhead orthographic strategy camera.
                            triangles.Add(start);
                            triangles.Add(start+2);
                            triangles.Add(start+1);
                            triangles.Add(start);
                            triangles.Add(start+3);
                            triangles.Add(start+2);
                        }
                        result.trees++;
                    }
                }
                if(vertices.Count==0)continue;
                var mesh=new Mesh{
                    name="Region1 2.5D evergreen batched LOD "+band,
                    vertices=vertices.ToArray(),uv=uvs.ToArray(),
                    colors32=colors.ToArray(),triangles=triangles.ToArray()
                };
                mesh.RecalculateBounds();
                result.triangles+=triangles.Count/3;
                result.meshes.Add(mesh);
                var unit=new GameObject("Region1 2.5D forest band "+band);
                unit.transform.SetParent(result.root.transform,true);
                unit.AddComponent<MeshFilter>().sharedMesh=mesh;
                var renderer=unit.AddComponent<MeshRenderer>();
                renderer.sharedMaterial=result.material;
                renderer.shadowCastingMode=ShadowCastingMode.Off;
                renderer.receiveShadows=false;
            }
            result.root.SetActive(false);
            Debug.Log("REGION1_HYBRID_IMPOSTORS_BATCHED trees="+result.trees+
                      " meshes="+result.meshes.Count+" triangles="+result.triangles+
                      " textures=1 gameplay_changes=0");
            return result;
        }
    }
}
