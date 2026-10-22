using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eldoria.World4XCanon
{
    /// <summary>
    /// Genuine hybrid 2.5D: painted conifer cards stand in 3D terrain XYZ,
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
            const int width=64,height=96;
            var pixels=new Color32[width*height];
            for(int y=0;y<height;y++)
            {
                float t=(y-18f)/73f;
                for(int x=0;x<width;x++)
                {
                    float dx=x-32f;
                    float sw=Random01(x,y,31);
                    var color=new Color32(0,0,0,0);
                    bool trunk=y>=5 && y<38 && Mathf.Abs(dx)<3.25f;
                    if(trunk)
                    {
                        float shade=sw*.22f;
                        color=new Color32((byte)(77+shade*44),
                            (byte)(57+shade*21),(byte)(33+shade*12),255);
                    }
                    if(t>=0f && t<=1f)
                    {
                        // Branch crowns layered at three natural intervals.
                        // Uneven jagged needles rather than triangular 3D cones.
                        float widthAtHeight=(1f-t)*27f+2.5f;
                        float sweep=2.3f*Mathf.Sin(y*.21f)+.7f*Mathf.Sin(y*.49f);
                        float tier=Mathf.Abs(Mathf.Sin((t*3.6f+.12f)*Mathf.PI));
                        float outline=widthAtHeight*(.65f+.35f*tier);
                        outline+=1.8f*(sw-.5f);
                        if(Mathf.Abs(dx-sweep)<=outline)
                        {
                            float light=Mathf.Clamp01(.43f + (32f-x)*.011f
                                            + .27f*(sw-.5f)+.14f*tier);
                            byte red=(byte)Mathf.RoundToInt(Mathf.Lerp(24f,71f,light));
                            byte green=(byte)Mathf.RoundToInt(Mathf.Lerp(62f,132f,light));
                            byte blue=(byte)Mathf.RoundToInt(Mathf.Lerp(30f,66f,light));
                            color=new Color32(red,green,blue,255);
                        }
                    }
                    pixels[y*width+x]=color;
                }
            }
            var atlas=new Texture2D(width,height,TextureFormat.RGBA32,true,false)
                {name="Region1 painterly evergreen alpha silhouette"};
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
                {name="Region1 2.5D shared painted conifer sheet"};
            if(result.material.HasProperty("_MainTex"))
                result.material.SetTexture("_MainTex",result.atlas);
            result.material.enableInstancing=true;
            result.root=new GameObject("Region1 · hybrid forest painterly 2.5D LOD");
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
                        float height=5.5f+Random01(cluster,k,131)*5.0f;
                        float width=2.2f+Random01(cluster,k,277)*2.0f;
                        float rotation=(cluster*43f+k*17f)*Mathf.Deg2Rad;
                        float groundY=elevation(x,z)+.16f;
                        var p=new Vector3(origin.x+x,groundY,origin.z+z);
                        var tint=new Color32((byte)(217+Random01(cluster,k,111)*37f),
                            (byte)(216+Random01(cluster,k,141)*39f),
                            (byte)(203+Random01(cluster,k,191)*52f),255);
                        // Crossed upright planes: parallax when camera pans or
                        // tilts; no radial billboarding updates per frame.
                        for(int card=0;card<2;card++)
                        {
                            float direction=rotation+card*Mathf.PI*.5f;
                            Vector3 right=new Vector3(Mathf.Cos(direction),0,
                                Mathf.Sin(direction))*(width*.5f);
                            int start=vertices.Count;
                            vertices.Add(p-right);
                            vertices.Add(p+right);
                            vertices.Add(p+right+Vector3.up*height);
                            vertices.Add(p-right+Vector3.up*height);
                            uvs.Add(new Vector2(0,0));
                            uvs.Add(new Vector2(1,0));
                            uvs.Add(new Vector2(1,1));
                            uvs.Add(new Vector2(0,1));
                            for(int i=0;i<4;i++)colors.Add(tint);
                            triangles.Add(start);
                            triangles.Add(start+1);
                            triangles.Add(start+2);
                            triangles.Add(start);
                            triangles.Add(start+2);
                            triangles.Add(start+3);
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
