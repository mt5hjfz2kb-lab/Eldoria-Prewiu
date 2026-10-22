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
            // The overhead radial crown A/B was too coin-like. Author a
            // conifer side-silhouette painted into oblique ground-oriented
            // 2.5D impostors: broad serrated lower boughs, narrow tapered top.
            // It stays one alpha atlas and is not a gameplay object or POI.
            const int size=96;
            // Two painted conifer profiles in ONE shared 192x96 atlas.
            // No additional materials or renderer batches; avoid repeating
            // the same triangular shape across every distant woodland.
            const int atlasWidth=size*2;
            var pixels=new Color32[atlasWidth*size];
            for(int species=0;species<2;species++)
              for(int y=0;y<size;y++)
                for(int x=0;x<size;x++)
                {
                    float dx=(x-size*.50f)/(size*.48f);
                    float dy=(y-size*.49f)/(size*.48f);
                    float t=Mathf.Clamp01((dy+.94f)/1.88f);
                    float jitter=Random01(x,y,39);
                    // Sawtooth boughs and asymmetrical needles break the
                    // circular green-dot silhouette even at overview zoom.
                    float boughs=.11f*Mathf.Sin(t*36f+dx*1.7f)+
                                  .047f*Mathf.Sin(t*72f-dx*3f);
                    float profile=species==0
                        ? (.91f*(1f-t)+.065f)
                        : (.73f*(1f-t)+.16f+.085f*Mathf.Sin(t*5.5f));
                    float width=profile*
                                (.93f+.11f*Mathf.Sin(t*17f+1.4f));
                    float edge=width+boughs*(1f-.38f*t);
                    float within=edge-Mathf.Abs(dx);
                    if(dy<-.93f||dy>.95f||within<=0f)
                    {
                        pixels[y*atlasWidth+species*size+x]=new Color32(0,0,0,0);
                        continue;
                    }
                    // Branch tiers and left/right directional light suggest
                    // evergreen volume without distant per-tree meshes.
                    float needles=Mathf.Sin(t*44f+dx*6f)*.12f+
                        Mathf.Sin(t*73f-dx*9f)*.06f;
                    float light=Mathf.Clamp01(.46f-.20f*dx+
                        .17f*t+needles+.16f*(jitter-.5f)+
                        (species==1?.055f:0f));
                    light*=1f-.28f*Mathf.SmoothStep(0f,1f,
                        Mathf.Clamp01((.22f-within)/.22f));
                    byte rr=(byte)Mathf.Clamp(Mathf.RoundToInt(
                        Mathf.Lerp(20f,91f,light)),0,255);
                    byte gg=(byte)Mathf.Clamp(Mathf.RoundToInt(
                        Mathf.Lerp(63f,154f,light)),0,255);
                    byte bb=(byte)Mathf.Clamp(Mathf.RoundToInt(
                        Mathf.Lerp(24f,80f,light)),0,255);
                    byte alpha=(byte)Mathf.RoundToInt(255f*
                        Mathf.Clamp01(within*15f)*
                        Mathf.Clamp01((.97f-dy)*17f));
                    pixels[y*atlasWidth+species*size+x]=new Color32(rr,gg,bb,alpha);
                }
            var atlas=new Texture2D(atlasWidth,size,TextureFormat.RGBA32,true,false)
                {name="Region 1 painted dual-profile organic conifers / single atlas"};
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
                        // Slight spatial domain warp breaks repeated circular
                        // grove outlines while the original woodland/water/
                        // road masks remain final placement authority.
                        float seedJitter=Random01(cluster,k,913)-.5f;
                        x+=6f*Mathf.Sin(z*.040f+cluster*.69f)+seedJitter*5f;
                        z+=5f*Mathf.Cos(x*.037f-cluster*.43f)-seedJitter*4f;
                        // Exact same geographic forest definition as 2D texture.
                        // Keep genuine interaction corridor and river paths free.
                        if(!woodland(x,z)||water(x,z)||roads(x,z)||coast(x,z)||
                            x*x+z*z<95f*95f)continue;
                        int variety=(cluster*13+k*7)&1;
                        float width=(6.0f+Random01(cluster,k,277)*5.2f)*
                            (variety==0?.91f:1.12f);
                        float rotation=(Random01(cluster,k,313)-.5f)*.26f;
                        float groundY=elevation(x,z)+.25f;
                        var p=new Vector3(origin.x+x,groundY,origin.z+z);
                        var tint=new Color32((byte)(217+Random01(cluster,k,111)*37f),
                            (byte)(216+Random01(cluster,k,141)*39f),
                            (byte)(203+Random01(cluster,k,191)*52f),255);
                        // Two staggered tapered 2.5D impostor layers produce
                        // depth while preserving a side-like spruce silhouette.
                        // Avoid the rejected three concentric circular crowns.
                        for(int layer=0;layer<2;layer++)
                        {
                            float direction=rotation+layer*.055f;
                            float scale=layer==0?1f:.63f;
                            Vector3 right=new Vector3(Mathf.Cos(direction),0f,
                                Mathf.Sin(direction))*(width*scale*.5f);
                            Vector3 forward=new Vector3(-right.z,0f,right.x);
                            float canopyLift=layer==0?.15f:2.85f;
                            Vector3 canopyCentre=p+Vector3.up*canopyLift;
                            int start=vertices.Count;
                            vertices.Add(canopyCentre-right-forward);
                            vertices.Add(canopyCentre+right-forward);
                            vertices.Add(canopyCentre+right+forward);
                            vertices.Add(canopyCentre-right+forward);
                            // Each tree uses one of two atlas cells, with
                            // tiny insets to avoid mipmap seams at the join.
                            float uMin=variety*.5f+.006f;
                            float uMax=(variety+1)*.5f-.006f;
                            uvs.Add(new Vector2(uMin,0));
                            uvs.Add(new Vector2(uMax,0));
                            uvs.Add(new Vector2(uMax,1));
                            uvs.Add(new Vector2(uMin,1));
                            // Dark undergrowth, mid-green needles, sunlit tips.
                            // The painted silhouette remains transparent and
                            // consistent with the forest underlay at all zooms.
                            float layerLight=layer==0?.81f:1.06f;
                            var crownTint=new Color32(
                                (byte)Mathf.Clamp(Mathf.RoundToInt(tint.r*layerLight),0,255),
                                (byte)Mathf.Clamp(Mathf.RoundToInt(tint.g*layerLight),0,255),
                                (byte)Mathf.Clamp(Mathf.RoundToInt(tint.b*layerLight),0,255),255);
                            for(int i=0;i<4;i++)colors.Add(crownTint);
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
