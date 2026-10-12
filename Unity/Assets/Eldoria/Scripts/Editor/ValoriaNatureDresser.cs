using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Eldoria.EditorTools
{
    /// <summary>
    /// Reuses already installed Eldoria NatureStarterKit prefabs for staging,
    /// rather than generating new placeholder trees or changing the source GLB.
    /// Editor-only lookdev; asset licenses and deployment must be verified before production use.
    /// </summary>
    public static class ValoriaNatureDresser
    {
        static readonly string[] Bushes={
            "Assets/NatureStarterKit2/Nature/bush01.prefab",
            "Assets/NatureStarterKit2/Nature/bush02.prefab",
            "Assets/NatureStarterKit2/Nature/bush03.prefab",
            "Assets/NatureStarterKit2/Nature/bush04.prefab"
        };
        static readonly string[] Trees={
            "Assets/NatureStarterKit2/Nature/tree01.prefab",
            "Assets/NatureStarterKit2/Nature/tree02.prefab"
        };

        public static GameObject Dress(Bounds bounds,string piece)
        {
            var result = new GameObject("Valoria authored nature staging / "+piece);
            float spreadX=Mathf.Max(3f,bounds.extents.x+1.8f);
            float spreadZ=Mathf.Max(2.8f,bounds.extents.z+2f);
            var placements = new List<Vector3> {
                new Vector3(-spreadX,0,-spreadZ*.6f),
                new Vector3(spreadX,0,spreadZ*.45f),
                new Vector3(-spreadX*.65f,0,spreadZ),
                new Vector3(spreadX*.75f,0,-spreadZ)
            };
            int n=piece=="defensive_props"?2:piece=="stone_path_platform"?5:piece=="cliff_rocks"?7:4;
            for(int i=0;i<n;i++)
            {
                var pref=Bushes[i%Bushes.Length];
                var x=i<placements.Count?placements[i]:new Vector3(
                    Mathf.Sin(i*2.13f)*spreadX,0,Mathf.Cos(i*2.78f)*spreadZ);
                float h=(piece=="cliff_rocks"?1.1f:piece=="stone_path_platform"?.60f:.8f);
                Spawn(pref,result.transform,new Vector3(bounds.center.x+x.x,bounds.min.y-.06f,bounds.center.z+x.z),h,30f*i);
            }
            if(piece=="cliff_rocks"||piece=="stone_path_platform")
            {
                for(int i=0;i<2;i++)
                {
                    float height=piece=="cliff_rocks"?3.8f:3.0f;
                    Spawn(Trees[i%Trees.Length],result.transform,
                        new Vector3(bounds.center.x+(i==0?-1:1)*spreadX*1.55f,
                        bounds.min.y-.08f,bounds.center.z+spreadZ*(i==0?.35f:.70f)),height,23f+i*52f);
                }
            }
            return result;
        }

        static void Spawn(string path,Transform root,Vector3 position,float targetHeight,float angle)
        {
            var source=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if(source==null) {Debug.LogWarning("Nature asset missing: "+path); return;}
            var instance=PrefabUtility.InstantiatePrefab(source) as GameObject;
            if(instance==null)return;
            instance.name="Existing nature prefab: "+source.name;
            instance.transform.SetParent(root,false);
            instance.transform.position=position;
            instance.transform.rotation=Quaternion.Euler(0f,angle,0f);
            var renderers=instance.GetComponentsInChildren<Renderer>(true);
            if(renderers.Length==0){UnityEngine.Object.DestroyImmediate(instance);return;}
            Bounds all=renderers[0].bounds;
            foreach(var r in renderers)all.Encapsulate(r.bounds);
            float ratio=Mathf.Clamp(targetHeight/Mathf.Max(.01f,all.size.y),.05f,20f);
            instance.transform.localScale*=ratio;
            // Ground the actual visible foliage mesh, not its prefab pivot.
            Bounds grounded=renderers[0].bounds;
            foreach(var r in renderers)grounded.Encapsulate(r.bounds);
            instance.transform.position+=Vector3.up*(position.y-grounded.min.y);
            // Material conversion preserves each authored prefab's existing diffuse texture.
            foreach(var r in renderers)
            {
                var originals=r.sharedMaterials;
                var result=new Material[originals.Length];
                for(int i=0;i<originals.Length;i++)
                {
                    var old=originals[i];
                    if(old==null)continue;
                    var shader=Shader.Find("Universal Render Pipeline/Lit");
                    if(shader==null)continue;
                    var m=new Material(shader){name="ValoriaNatureURP_"+old.name};
                    if(old.HasProperty("_MainTex"))m.SetTexture("_BaseMap",old.mainTexture);
                    if(old.HasProperty("_Color"))m.SetColor("_BaseColor",old.color);
                    m.SetFloat("_Smoothness",.06f);
                    var leafName=old.name.ToLowerInvariant();
                    if(leafName.Contains("leaf")||leafName.Contains("foliage")||leafName.Contains("bush")||
                       leafName.Contains("branch")||leafName.Contains("tree"))
                    {
                        m.SetFloat("_AlphaClip",1f);
                        m.SetFloat("_Cutoff",.35f);
                        m.EnableKeyword("_ALPHATEST_ON");
                        m.SetFloat("_Cull",0f);
                    }
                    result[i]=m;
                }
                r.sharedMaterials=result;
            }
        }
    }
}
