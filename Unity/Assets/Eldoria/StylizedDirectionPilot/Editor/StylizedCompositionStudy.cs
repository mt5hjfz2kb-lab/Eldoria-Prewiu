#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using Eldoria.ReferenceClean;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Eldoria.StylizedDirectionPilot.Editor
{
    /// <summary>
    /// Art-direction proof, not a gameplay change. Makes a disposable, modular
    /// density/composition study using existing imported Unity prefabs.
    /// Never touches a source mesh/prefab, target scene file, player save or HUD.
    /// </summary>
    public sealed class StylizedCompositionStudy : IDisposable
    {
        readonly GameObject root;
        readonly List<string> sources=new List<string>();
        public int Count {get;private set;}
        public IList<string> Sources=>sources.AsReadOnly();

        static readonly string Base="Assets/EmaceArt/Slavic World Free/Prefabs/";
        static readonly string[] Trees = {
            "Nature/Tree/EA03_Nature_Tree_01b_PRE.prefab",
            "Nature/Tree/EA03_Nature_Tree_02b_PRE.prefab",
            "Nature/Tree/EA03_Nature_Tree_03b_PRE.prefab",
            "Nature/Tree/EA03_Nature_Tree_05a_PRE.prefab"
        };
        static readonly string[] Stones={
            "Environment/Rock/EA03_EnvRock_01a_PRE.prefab",
            "Environment/Rock/EA03_Environment_Rock_Flat_04c_PRE.prefab",
            "Environment/Rock/EA03_Environment_Rock_Big_Head_01a_PRE.prefab"
        };

        StylizedCompositionStudy()
        {
            root=new GameObject("ART PROOF / TEMP MODULAR ENVIRONMENT / NOT IN SCENE");
            root.hideFlags=HideFlags.DontSaveInEditor|HideFlags.DontSaveInBuild;
        }

        static float Rand(int n,int k)
        {
            float f=Mathf.Sin((n+1)*12.9898f+(k+1)*78.233f)*43758.5453f;
            return f-Mathf.Floor(f);
        }

        static Vector3 Ground(Camera camera,float nx,float ny)
        {
            var ray=camera.ViewportPointToRay(new Vector3(nx,ny,0f));
            var plane=new Plane(Vector3.up,Vector3.zero);
            if(!plane.Raycast(ray,out float distance))
                throw new InvalidOperationException("Isometric player camera cannot project onto real ground");
            return ray.GetPoint(distance);
        }

        static Bounds BoundsOf(GameObject go)
        {
            var all=go.GetComponentsInChildren<Renderer>(true);
            if(all.Length==0)throw new InvalidOperationException("Existing kit prefab has no mesh: "+go.name);
            var b=all[0].bounds;
            foreach(var r in all)b.Encapsulate(r.bounds);
            return b;
        }

        bool Place(Camera cam,string path,float nx,float ny,float height,float yaw,int index,string role)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if(!prefab)return false;
            var go=Object.Instantiate(prefab);
            if(!go)return false;
            go.name="ART PROOF / "+role+"/"+index+" / "+prefab.name;
            go.transform.SetParent(root.transform,true);
            go.transform.rotation=Quaternion.Euler(0,yaw,0);
            var b=BoundsOf(go);
            float nativeHeight=Mathf.Max(.01f,b.size.y);
            float scale=Mathf.Clamp(height/nativeHeight,.3f,2.5f);
            go.transform.localScale*=scale;
            go.transform.position=Ground(cam,nx,ny);
            b=BoundsOf(go);
            go.transform.position+=Vector3.up*(.05f-b.min.y);
            // This is a composition preview only; no new selectable nodes.
            foreach(var collider in go.GetComponentsInChildren<Collider>(true))collider.enabled=false;
            foreach(var mb in go.GetComponentsInChildren<MonoBehaviour>(true))mb.enabled=false;
            foreach(var r in go.GetComponentsInChildren<Renderer>(true))
                r.gameObject.layer=0;
            Count++;
            sources.Add(role+","+index+","+path+","+nx.ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+","+ny.ToString("F3",System.Globalization.CultureInfo.InvariantCulture));
            return true;
        }

        void Forest(Camera cam,float cx,float cy,float dx,float dy,int n,int seed)
        {
            for(int i=0;i<n;i++)
            {
                float theta=i*2.39996323f+seed;
                float rr=Mathf.Sqrt((i+.5f)/n);
                float x=cx+Mathf.Cos(theta)*dx*rr*(.83f+.25f*Rand(i,seed));
                float y=cy+Mathf.Sin(theta)*dy*rr*(.82f+.27f*Rand(i,seed+9));
                if(x<.01f||x>.99f||y<.05f||y>.98f)continue;
                string source=Base+Trees[(i+seed)%Trees.Length];
                float height=2.6f+Rand(i,seed+22)*2.0f;
                Place(cam,source,x,y,height,Rand(i,seed+80)*360,i,"forest");
            }
        }
        void Rocks(Camera cam,float cx,float cy,float rx,float ry,int n,int seed)
        {
            for(int i=0;i<n;i++)
            {
                float theta=i*2.39996323f+seed;
                float rr=Mathf.Sqrt((i+.37f)/n);
                float x=cx+Mathf.Cos(theta)*rx*rr;
                float y=cy+Mathf.Sin(theta)*ry*rr;
                if(x<.01f||x>.99f||y<.03f||y>.97f)continue;
                Place(cam,Base+Stones[(i+seed)%Stones.Length],x,y,
                    .55f+Rand(i,seed+14)*1.25f,Rand(i,seed+53)*360,i,"geology");
            }
        }

        public static StylizedCompositionStudy Build(ValoriaReferenceCleanGameplay city,Camera camera)
        {
            if(!city||!camera)throw new ArgumentNullException("City and camera required");
            var study=new StylizedCompositionStudy();
            try
            {
                // Substantial, deliberate silhouette clusters, not evenly spaced props.
                // Free corridor and Bastion middle preserve interaction clarity.
                study.Forest(camera,.10f,.86f,.12f,.11f,12,11);
                study.Forest(camera,.29f,.91f,.10f,.075f,11,19);
                study.Forest(camera,.81f,.88f,.15f,.09f,17,31);
                study.Forest(camera,.92f,.63f,.08f,.14f,10,47);
                study.Forest(camera,.13f,.44f,.09f,.12f,8,67);
                study.Forest(camera,.87f,.14f,.11f,.09f,12,83);
                study.Rocks(camera,.69f,.74f,.11f,.075f,15,21);
                study.Rocks(camera,.90f,.43f,.07f,.10f,10,34);
                study.Rocks(camera,.10f,.35f,.07f,.08f,8,44);

                // Ancient wall fragments can frame the lost empire but must not
                // become new production buildings or touch existing districts.
                var ruin="Assets/Eldoria/Resources/Valoria/Stone_Wall.prefab";
                study.Place(camera,ruin,.74f,.84f,4.5f,19,1,"ancient-ruin");
                study.Place(camera,ruin,.84f,.80f,3.7f,72,2,"ancient-ruin");
                study.Place(camera,ruin,.20f,.80f,2.6f,145,3,"ancient-ruin");

                if(study.Count<45)
                    throw new InvalidOperationException("Too few authentic reusable kit props for scene proof: "+study.Count);
                return study;
            }
            catch {study.Dispose(); throw;}
        }

        public void Dispose()
        {
            if(root)Object.DestroyImmediate(root);
        }
    }
}
#endif
