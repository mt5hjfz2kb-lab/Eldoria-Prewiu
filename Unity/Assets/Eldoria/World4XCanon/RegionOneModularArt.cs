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

        public static RegionOneModularArt Install(GameObject artwork,Eldoria.ReferenceClean.ValoriaIntegratedWorld host)
        {
            var art=artwork.AddComponent<RegionOneModularArt>();
            // Geometry, transforms and names remain available for the physical map.
            foreach(var renderer in artwork.GetComponentsInChildren<Renderer>(true))
                renderer.forceRenderingOff=true;
            RegionOneOriginalLandscape.Install(artwork.transform.parent,host,art);
            return art;
        }

        static string ResourcePath(string key)
        {
            return key.StartsWith("World4X/",StringComparison.Ordinal) ? key : Root+key;
        }

        public MeshRenderer Place(Transform parent, string key, float width, Vector3 foot)
        {
            if (!materials.TryGetValue(key, out var material))
            {
                var texture = Resources.Load<Texture2D>(ResourcePath(key));
                if (!texture) throw new InvalidOperationException("Missing modular world art " + key);
                material = new Material(Shader.Find("Eldoria/World4X/LocalCanopy")) {mainTexture=texture, name="Valoria · "+key};
                materials.Add(key, material);
            }
            int size = Mathf.Max(1, Mathf.RoundToInt(width*4));
            if (!shapes.TryGetValue((key,size), out var mesh))
            {

                float w = size/4f, h = w*material.mainTexture.height/material.mainTexture.width;
                // Camera-facing art is clipped by the walkable surface below its anchor.
                // The visible base of each isolated PNG meets the ground here.
                Vector2 pivot = new Vector2(.5f, key.StartsWith("World4X/") ? .07f : .042f);
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

        public GameObject WorkSite(string id, Transform parent)
        {
            var site=new GameObject("Valoria work site "+id);
            site.transform.SetParent(parent,false);
            var anchor=site.transform.position;
            bool wood=id=="forest-valoria" || id.StartsWith("region1:wood:",StringComparison.Ordinal);
            Place(site.transform,wood?"Timber":"Quarry",wood?25f:27.5f,anchor);
            return site;
        }
        void OnDestroy()
        {
            foreach(var shape in shapes.Values) Destroy(shape);
            foreach(var material in materials.Values) Destroy(material);
        }
    }
}
