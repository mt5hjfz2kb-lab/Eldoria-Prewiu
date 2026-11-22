using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eldoria.World4XCanon
{
    // Decorative camera-facing planes live alongside the canonical meshes. Only the
    // latter supply spatial queries, reachability, picking and generation.
    public sealed class RegionOneModularArt : MonoBehaviour
    {
        public static bool Enabled => !UnityEngine.Application.isEditor ||
            Environment.GetEnvironmentVariable("ELDORIA_WORLD_ART_BASELINE") != "1";
        const string Root = "World4X/Illustrated/";
        readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();
        readonly Dictionary<(string,int), Mesh> shapes = new Dictionary<(string,int), Mesh>();

        public static System.Collections.IEnumerator Install(GameObject artwork,Eldoria.ReferenceClean.ValoriaIntegratedWorld host)
        {
            var art=artwork.AddComponent<RegionOneModularArt>();
            // Geometry, transforms and names remain available for the physical map.
            foreach(var renderer in artwork.GetComponentsInChildren<Renderer>(true))
                renderer.forceRenderingOff=true;
            var build=RegionOneOriginalLandscape.Install(artwork.transform.parent,host,art);
            while(build.MoveNext())yield return null;
        }

        static string ResourcePath(string key)
        {
            return key.StartsWith("World4X/",StringComparison.Ordinal) ||
                key.StartsWith("ValoriaModular25D/",StringComparison.Ordinal) ? key : Root+key;
        }

        // The approved city scene uses its own registration. When those paintings
        // are reused on geographic billboards, register their authored ground
        // contacts without changing the city scene's existing sprite imports.
        internal static Vector2 WorldPivot(string path, Sprite sprite)
        {
            switch(path)
            {
                case "ValoriaModular25D/sawmill_i":
                case "ValoriaModular25D/sawmill_i_hd": return new Vector2(.5f,.07f);
                case "ValoriaModular25D/Progression/barracks": return new Vector2(.5f,.104863f);
                case "ValoriaModular25D/Progression/granary": return new Vector2(.5f,.072251f);
                case "ValoriaModular25D/RecoveredProps/ArbolAntiguo": return new Vector2(.5f,.09f);
                case "ValoriaModular25D/RecoveredProps/ArboledaAntigua":
                case "ValoriaModular25D/RecoveredProps/MatojosFlores":
                case "ValoriaModular25D/RecoveredProps/PozoPiedra": return new Vector2(.5f,.08f);
                default: return sprite ? sprite.pivot / sprite.rect.size : new Vector2(.5f,.07f);
            }
        }

        public static string Illustration(string id)
        {
            if(string.IsNullOrEmpty(id))return null;
            if(id=="forest-valoria"||id.StartsWith("region1:wood:",StringComparison.Ordinal))return "Timber";
            if(id=="quarry-valoria"||id.StartsWith("region1:stone:",StringComparison.Ordinal))return "Quarry";
            switch(id){
                case "hunt-wolf":return "Wolf";
                case "hunt-boar":return "Boar";
                case "meat-valoria":return "HunterCache";
                case "ash-stalker-valoria":return "AshStalker";
                case "fissure-valoria":return "Fault";
                case "corrupt-scout":return "Camp";
                case "engendro-valoria":return "Spawn";
                case "old-watch-ruin":return "Ruin";
                default:return null;
            }
        }
        public MeshRenderer Place(Transform parent, string key, float width, Vector3 foot)
        {
            if (!materials.TryGetValue(key, out var material))
            {
                var texture = Resources.Load<Texture2D>(ResourcePath(key));
                if (!texture) throw new InvalidOperationException("Missing modular world art " + key);
                material = new Material(Shader.Find("Eldoria/World4X/LocalCanopy")) {mainTexture=texture, name="Valoria · "+key};
                // City alpha silhouettes must retain the same softly
                // painted pixels in the world (shader legacy default stays 0).
                material.SetFloat("_SoftEdges",1f);
                // Illustrations replace GLB geometry, but the canonical relief
                // still supplies height and z-buffering. Shift only view depth,
                // never their geographic foot or colliders. The near city and
                // worksites have stronger relief than tiny forest props.
                material.SetFloat("_CameraFacingDepth",14f);
                materials.Add(key, material);
            }
            int size = Mathf.Max(1, Mathf.RoundToInt(width*4));
            if (!shapes.TryGetValue((key,size), out var mesh))
            {

                // The city sprite owns its actual painted footprint. Reuse
                // the original pixel-space pivot rather than the old generic
                // 4.2% baseline which made granaries, mills and groves float.
                var sprite=Resources.Load<Sprite>(ResourcePath(key));
                float w=size/4f;
                float h=w*(sprite ? sprite.rect.height/sprite.rect.width
                                  : material.mainTexture.height/(float)material.mainTexture.width);
                Vector2 pivot=WorldPivot(ResourcePath(key),sprite);
                var uv = new[]{Vector2.zero, Vector2.right, Vector2.one, Vector2.up};
                var offsets = new Vector2[4];
                for(int i=0;i<4;i++) offsets[i]=Vector2.Scale(uv[i]-pivot,new Vector2(w,h));
                mesh = new Mesh {name=key, vertices=new Vector3[4], uv=uv, uv2=offsets,
                    triangles=new[]{0,1,2,0,2,3}, bounds=new Bounds(Vector3.zero,Vector3.one*(2f*Mathf.Max(w,h)))};
                shapes.Add((key,size),mesh);
            }
            var go = new GameObject("Valoria art · "+key);
            go.layer=29;
            go.transform.SetParent(parent,false);
            go.transform.position=foot;
            go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial=material;
            renderer.shadowCastingMode=ShadowCastingMode.Off;
            renderer.receiveShadows=false;
            return renderer;
        }

        Material soilMaterial;
        readonly List<Mesh> yards=new List<Mesh>();
        public void SettleYard(Transform pad,Func<Vector3,float> ground)
        {
            var mesh=pad.GetComponent<MeshFilter>().sharedMesh;
            var vertices=mesh.vertices;
            for(int i=0;i<vertices.Length;i++){
                var world=pad.TransformPoint(vertices[i]);
                world.y=ground(world)+.08f;
                vertices[i]=pad.InverseTransformPoint(world);
            }
            mesh.vertices=vertices;mesh.RecalculateBounds();
        }
        void AddYard(Transform site,float width)
        {
            if(!soilMaterial)soilMaterial=new Material(Shader.Find("Eldoria/Region1/Workyard")){mainTexture=Resources.Load<Texture2D>(Root+"Earth")};
            const int n=6;var v=new Vector3[(n+1)*(n+1)];var uv=new Vector2[v.Length];var tris=new int[n*n*6];int t=0;
            for(int j=0;j<=n;j++)for(int i=0;i<=n;i++){
                int k=j*(n+1)+i;uv[k]=new Vector2(i/(float)n,j/(float)n);
                v[k]=new Vector3((uv[k].x-.5f)*width,0,(uv[k].y-.5f)*width*.59f);
                if(i==n||j==n)continue;
                tris[t++]=k;tris[t++]=k+n+1;tris[t++]=k+1;
                tris[t++]=k+1;tris[t++]=k+n+1;tris[t++]=k+n+2;
            }
            var mesh=new Mesh{name="Worn earth at work site",vertices=v,uv=uv,triangles=tris};mesh.RecalculateBounds();yards.Add(mesh);
            var pad=new GameObject("Ground at site");pad.layer=29;pad.transform.SetParent(site,false);
            pad.AddComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=pad.AddComponent<MeshRenderer>();renderer.sharedMaterial=soilMaterial;renderer.shadowCastingMode=ShadowCastingMode.Off;
        }

        public GameObject WorkSite(string id, Transform parent)
        {
            var site=new GameObject("Valoria work site "+id);
            site.transform.SetParent(parent,false);
            var anchor=site.transform.position;
            bool wood=id=="forest-valoria" || id.StartsWith("region1:wood:",StringComparison.Ordinal);
            AddYard(site.transform,wood?34f:39f);
            Place(site.transform,wood?"Timber":"Quarry",wood?25f:27.5f,anchor);
            return site;
        }
        void OnDestroy()
        {
            if(soilMaterial)Destroy(soilMaterial);
            foreach(var mesh in yards)Destroy(mesh);
            foreach(var shape in shapes.Values) Destroy(shape);
            foreach(var material in materials.Values) Destroy(material);
        }
    }
}

