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
        public const int Revision = 4;
        public sealed class Output
        {
            public GameObject root;
            public readonly List<Mesh> meshes=new List<Mesh>();
            public Texture2D atlas;
            public bool ownsAtlas;
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
            // Three hand-painted evergreen profiles in ONE shared 288x96 atlas.
            // The third is a wider asymmetric cedar/evergreen canopy, not
            // a new tree object or paid asset. Keep the same three batches.
            const int atlasWidth=size*3;
            var pixels=new Color32[atlasWidth*size];
            for(int species=0;species<3;species++)
              for(int y=0;y<size;y++)
                for(int x=0;x<size;x++)
                {
                    float dx=(x-size*.50f)/(size*.48f);
                    float dy=(y-size*.49f)/(size*.48f);
                    float t=Mathf.Clamp01((dy+.94f)/1.88f);
                    float jitter=Random01(x,y,39+species*191);
                    // Sawtooth boughs and asymmetrical needles break the
                    // circular green-dot silhouette even at overview zoom.
                    float tierFrequency=species==2?27f:36f;
                    float boughs=(species==2?.16f:.11f)*
                                  Mathf.Sin(t*tierFrequency+dx*1.7f)+
                                  .047f*Mathf.Sin(t*(species==2?52f:72f)-dx*3f);
                    float profile=species==0
                        ? (.91f*(1f-t)+.065f)
                        : species==1
                        ? (.73f*(1f-t)+.16f+.085f*Mathf.Sin(t*5.5f))
                        : (.85f*Mathf.Pow(1f-t,.68f)+.08f+
                           .045f*Mathf.Sin(t*12f+dx*3f));
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
                        (species==1?.055f:species==2?.035f:0f));
                    light*=1f-.28f*Mathf.SmoothStep(0f,1f,
                        Mathf.Clamp01((.22f-within)/.22f));
                    byte rr=(byte)Mathf.Clamp(Mathf.RoundToInt(
                        Mathf.Lerp(species==2?29f:20f,species==2?106f:91f,light)),0,255);
                    byte gg=(byte)Mathf.Clamp(Mathf.RoundToInt(
                        Mathf.Lerp(species==2?72f:63f,species==2?142f:154f,light)),0,255);
                    byte bb=(byte)Mathf.Clamp(Mathf.RoundToInt(
                        Mathf.Lerp(species==2?31f:24f,species==2?82f:80f,light)),0,255);
                    byte alpha=(byte)Mathf.RoundToInt(255f*
                        Mathf.Clamp01(within*15f)*
                        Mathf.Clamp01((.97f-dy)*17f));
                    pixels[y*atlasWidth+species*size+x]=new Color32(rr,gg,bb,alpha);
                }
            var atlas=new Texture2D(atlasWidth,size,TextureFormat.RGBA32,true,false)
                {name="Region 1 painted triple-profile organic evergreens / single atlas"};
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
            result.atlas=Resources.Load<Texture2D>("World4X/RegionOnePaintedForest");
            result.ownsAtlas=!result.atlas;
            if(!result.atlas) result.atlas=PaintConiferAtlas();
            Debug.Log("REGION1_FOREST_PAINTED="+(!result.ownsAtlas));
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
                int connectors=RegionOneCanonicalLandscape.ForestCorridorPairs.Length/2;
                for(int cluster=band;cluster<centres.Length+connectors;cluster+=3)
                {
                    bool isCorridor=cluster>=centres.Length;
                    int link=(cluster-centres.Length)*2;
                    var centre=isCorridor
                        ? (centres[RegionOneCanonicalLandscape.ForestCorridorPairs[link]]+
                           centres[RegionOneCanonicalLandscape.ForestCorridorPairs[link+1]])*.5f
                        : centres[cluster];
                    int count=isCorridor ? 24 : RegionOneAuthoredComposition.Groves[cluster].stems;
                    for(int k=0;k<count;k++)
                    {
                        float angle=k*2.3999632f+cluster*.93f;
                        float radius=Mathf.Sqrt((k+.45f)/count);
                        float x,z;
                        if(isCorridor)
                        {
                            // Populate the newly connected organic strips
                            // with the SAME atlas and three existing batches.
                            Vector2 start=centres[RegionOneCanonicalLandscape.ForestCorridorPairs[link]];
                            Vector2 end=centres[RegionOneCanonicalLandscape.ForestCorridorPairs[link+1]];
                            Vector2 segment=end-start;
                            float t=Random01(cluster,k,431);
                            float lateral=(Random01(cluster,k,613)-.5f)*33f;
                            float length=Mathf.Max(.01f,segment.magnitude);
                            x=Mathf.Lerp(start.x,end.x,t)-segment.y/length*lateral;
                            z=Mathf.Lerp(start.y,end.y,t)+segment.x/length*lateral;
                        }
                        else
                        {
                            var grove=RegionOneAuthoredComposition.Groves[cluster];
                            float u=Mathf.Cos(angle)*radius*grove.width*1.22f;
                            float v=Mathf.Sin(angle)*radius*grove.depth*1.22f;
                            x=centre.x+Mathf.Cos(grove.angle)*u-Mathf.Sin(grove.angle)*v;
                            z=centre.y+Mathf.Sin(grove.angle)*u+Mathf.Cos(grove.angle)*v;
                        }
                        // Slight spatial domain warp breaks repeated circular
                        // grove outlines while the original woodland/water/
                        // road masks remain final placement authority.
                        float seedJitter=Random01(cluster,k,913)-.5f;
                        x+=1.5f*Mathf.Sin(z*.040f+cluster*.69f)+seedJitter*2f;
                        z+=1.5f*Mathf.Cos(x*.037f-cluster*.43f)-seedJitter*2f;
                        // Exact same geographic forest definition as 2D texture.
                        // Keep genuine interaction corridor and river paths free.
                        if(!woodland(x,z)||water(x,z)||roads(x,z)||coast(x,z)||
                            x*x+z*z<95f*95f)continue;
                        // Nonalternating deterministic species distribution avoids
                        // a checkerboard pattern across close-set trees.
                        int variety=Mathf.Min(2,(int)(Random01(cluster,k,829)*3f));
                        float width=(8.0f+Random01(cluster,k,277)*5.8f)*
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
                            float scale=1f;
                            Vector3 right=new Vector3(Mathf.Cos(direction),0f,
                                Mathf.Sin(direction))*(width*scale*.5f);
                            Vector3 along=new Vector3(-Mathf.Sin(direction),0f,Mathf.Cos(direction));
                            // Ground-contact shadow plus a raised, inclined tree
                            // card. Formerly BOTH cards lay flat on the terrain,
                            // making entire groves read as stacked stickers.
                            Vector3 forward=layer==0
                                ? along*(width*.43f)
                                : (along*.82f+Vector3.up*.57f)*(width*.68f);
                            Vector3 canopyCentre=layer==0
                                ? p+along*(width*.17f)+Vector3.up*.08f
                                : p+forward+Vector3.up*.12f;
                            int start=vertices.Count;
                            vertices.Add(canopyCentre-right-forward);
                            vertices.Add(canopyCentre+right-forward);
                            vertices.Add(canopyCentre+right+forward);
                            vertices.Add(canopyCentre-right+forward);
                            // Each tree uses one of three atlas cells. Keep a thin
                            // gutter to avoid mipmap bleeding at tile joins.
                            float uMin=variety/3f+.004f;
                            float uMax=(variety+1)/3f-.004f;
                            uvs.Add(new Vector2(uMin,0));
                            uvs.Add(new Vector2(uMax,0));
                            uvs.Add(new Vector2(uMax,1));
                            uvs.Add(new Vector2(uMin,1));
                            // Dark undergrowth, mid-green needles, sunlit tips.
                            // The painted silhouette remains transparent and
                            // consistent with the forest underlay at all zooms.
                            float layerLight=layer==0?.27f:1.03f;
                            var crownTint=new Color32(
                                (byte)Mathf.Clamp(Mathf.RoundToInt(tint.r*layerLight),0,255),
                                (byte)Mathf.Clamp(Mathf.RoundToInt(tint.g*layerLight),0,255),
                                (byte)Mathf.Clamp(Mathf.RoundToInt(tint.b*layerLight),0,255),
                                (byte)(layer==0?72:255));
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
