using UnityEngine;

namespace Eldoria.Experimentos.ValoriaModular25D
{
    // Projects the building's own alpha silhouette onto the ground.
    // Geometry and UVs update only when the canonical construction sprite changes.
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class Modular25DArchitecturalShadow : MonoBehaviour
    {
        SpriteRenderer source;
        Sprite previous;
        GameObject shadow;
        Mesh mesh;
        Material material;

        void OnEnable()
        {
            source=GetComponent<SpriteRenderer>();
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

        void Refresh()
        {
            if(source==null||source.sprite==null||source.sprite==previous)return;
            previous=source.sprite;
            var bounds=previous.bounds;
            float floor=bounds.min.y+.08f;
            float height=bounds.size.y;
            float left=bounds.min.x;
            float right=bounds.max.x;
            mesh.Clear();
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

        void OnDisable()
        {
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
