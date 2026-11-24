using System.Collections.Generic;
using UnityEngine;

namespace Eldoria.Experimentos.ValoriaModular25D
{
    // Projects the building's own alpha silhouette onto the ground.
    // Geometry and UVs update only when the canonical construction sprite changes.
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class Modular25DArchitecturalShadow : MonoBehaviour
    {
        // Normalized height of the masonry at five stations across each image.
        // Interpolate the ground plane through arch openings so the passage
        // casts shade behind the arch without a false sill across the road.
        static readonly Dictionary<string,float[]> frontageSills = new Dictionary<string,float[]> {
            {"AceraZapateros",new[]{.01f,.01f,.05f,.25f,.53f}},
            {"CasaDelGremio",new[]{.39f,.25f,.07f,.15f,.39f}},
            {"ManzanaDelAdarve",new[]{.36f,.25f,.05f,.04f,.24f}},
            {"PortalesDeOficios",new[]{.12f,.10f,.085f,.07f,.16f}},
            {"CalleDelPostigo",new[]{.15f,.13f,.085f,.045f,.18f}},
            {"ViviendaDelPostigo",new[]{.14f,.055f,.028f,.11f,.22f}},
            {"ToneleriaDelPostigo",new[]{.125f,.025f,.073f,.103f,.23f}},
            {"CasasDelPeso",new[]{.127f,.099f,.074f,.031f,.17f}}
        };

        public static void AttachToUnshadowedFrontage(SpriteRenderer building)
        {
            if(frontageSills.ContainsKey(building.sprite.name))
                building.gameObject.AddComponent<Modular25DArchitecturalShadow>();
        }

        SpriteRenderer source;
        Sprite previous;
        GameObject shadow;
        Mesh mesh;
        Material material;

        Material originalMaterial, elevation;
        Vector2 streetAxis = Vector2.right;

        void OnEnable()
        {
            source=GetComponent<SpriteRenderer>();
            if (source.sprite != null)
            {
                if (source.sprite.name == "PortalesDeOficios") streetAxis = ValoriaUrbanPlan.PortalesAxis;
                if (streetAxis != Vector2.right)
                {
                    elevation = new Material(Resources.Load<Shader>("ValoriaModular25D/StreetElevation"));
                    elevation.SetVector("_StreetAxis",new Vector4(streetAxis.x,streetAxis.y,0,0));
                    originalMaterial = source.sharedMaterial;
                    source.sharedMaterial = elevation;
                    var bounds = source.sprite.bounds;
                    bounds.Expand(new Vector3(0,2f*Mathf.Abs(streetAxis.y)*bounds.extents.x,0));
                    source.localBounds = bounds;
                }
            }
            shadow=new GameObject("Projected architectural ground shadow");
            shadow.transform.SetParent(transform,false);
            mesh=new Mesh { name="Ground projection of authored building silhouette" };
            shadow.AddComponent<MeshFilter>().sharedMesh=mesh;
            material=new Material(Resources.Load<Shader>("ValoriaModular25D/ArchitecturalShadow"));
            var renderer=shadow.AddComponent<MeshRenderer>();
            renderer.sharedMaterial=material;
            renderer.sortingOrder=-3900;
            Refresh();
        }

        void LateUpdate() => Refresh();

        public void Refresh()
        {
            if(source==null||source.sprite==null||source.sprite==previous)return;
            previous=source.sprite;
            var bounds=previous.bounds;
            float floor=bounds.min.y+.08f;
            float height=bounds.size.y;
            float left=bounds.min.x;
            float right=bounds.max.x;
            mesh.Clear();
            if(frontageSills.TryGetValue(previous.name,out var sills))
            {
                BuildStreetProjection(bounds,sills);
                material.SetTexture("_ShadowTex",previous.texture);
                return;
            }
            mesh.vertices=new[] {
                new Vector3(left,floor,0),new Vector3(right,floor,0),
                new Vector3(right+height*.32f,floor-height*.18f,0),
                new Vector3(left+height*.32f,floor-height*.18f,0)
            };
            Rect pixels=previous.textureRect;
            float w=previous.texture.width,h=previous.texture.height;
            mesh.uv=new[] {
                new Vector2(pixels.xMin/w,pixels.yMin/h),
                new Vector2(pixels.xMax/w,pixels.yMin/h),
                new Vector2(pixels.xMax/w,pixels.yMax/h),
                new Vector2(pixels.xMin/w,pixels.yMax/h)
            };
            var near=new Color(.10f,.13f,.19f,.22f);
            var far=new Color(.10f,.13f,.19f,.03f);
            mesh.colors=new[]{near,near,far,far};
            mesh.triangles=new[]{0,1,2,0,2,3};
            mesh.RecalculateBounds();
            material.SetTexture("_ShadowTex",previous.texture);
        }

        void BuildStreetProjection(Bounds bounds, float[] sills)
        {
            Rect uv=previous.textureRect;
            float w=previous.texture.width,h=previous.texture.height;
            int count=sills.Length;
            var vertices=new Vector3[count*2];
            var texCoords=new Vector2[count*2];
            var colors=new Color[count*2];
            var triangles=new int[(count-1)*6];
            for(int i=0;i<count;i++)
            {
                float t=i/(float)(count-1);
                float x=Mathf.Lerp(bounds.min.x,bounds.max.x,t);
                float contactY=bounds.min.y+sills[i]*bounds.size.y;
                for(int row=0;row<2;row++)
                {
                    float lift=(row-sills[i])*bounds.size.y;
                    int n=2*i+row;
                    vertices[n]=new Vector3(streetAxis.x*x+.32f*lift,contactY+streetAxis.y*x-.18f*lift,0f);
                    texCoords[n]=new Vector2((uv.xMin+t*uv.width)/w,(uv.yMin+row*uv.height)/h);
                    colors[n]=new Color(.10f,.13f,.19f,row==0 ? .22f : .03f);
                }
                if(i==count-1)continue;
                int v=2*i,q=6*i;
                triangles[q]=v;triangles[q+1]=v+2;triangles[q+2]=v+3;
                triangles[q+3]=v;triangles[q+4]=v+3;triangles[q+5]=v+1;
            }
            mesh.vertices=vertices;mesh.uv=texCoords;mesh.colors=colors;
            mesh.triangles=triangles;mesh.RecalculateBounds();
        }

        void OnDisable()
        {
            if (elevation)
            {
                source.sharedMaterial = originalMaterial;
                source.ResetLocalBounds();
                if (UnityEngine.Application.isPlaying) Destroy(elevation);
                else DestroyImmediate(elevation);
                elevation = null;
            }
            if(UnityEngine.Application.isPlaying)
            {
                Destroy(shadow);Destroy(mesh);Destroy(material);
            }
            else
            {
                DestroyImmediate(shadow);DestroyImmediate(mesh);DestroyImmediate(material);
            }
            previous=null;
        }
    }
}
