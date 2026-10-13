using System.Collections.Generic;
using UnityEngine;

namespace Eldoria.World4XPrototype
{
    /// <summary>
    /// Independent proof of a camera-driven chunk renderer. Attach to an EMPTY test scene.
    /// No writes to Valoria, Region 1, player state or published game.
    /// Placeholder colors only; this is not production artwork.
    /// </summary>
    public sealed class ZoneOnePreview : MonoBehaviour
    {
        [Range(1,3)] public int chunkRadius=1;
        [Range(7,80)] public float zoom=28f;
        public float panSpeed=24f;
        readonly ZoneOneWorld world=new ZoneOneWorld();
        readonly Dictionary<Vector2Int,GameObject> loaded=new Dictionary<Vector2Int,GameObject>();
        readonly List<Vector2Int> removal=new List<Vector2Int>();
        readonly HashSet<Vector2Int> needed=new HashSet<Vector2Int>();
        Camera view;
        Transform cameraPivot;
        Material material;
        Vector3 target;
        void Start()
        {
            var shader=Shader.Find("Universal Render Pipeline/Unlit");
            if(shader==null)shader=Shader.Find("Unlit/Color");
            material=new Material(shader);
            material.enableInstancing=true;
            var pivot=new GameObject("Zone 1 camera pivot");
            pivot.transform.SetParent(transform,false);
            cameraPivot=pivot.transform;
            var cameraGo=new GameObject("Zone 1 preview camera");
            cameraGo.transform.SetParent(cameraPivot,false);
            view=cameraGo.AddComponent<Camera>();
            view.orthographic=true;
            view.clearFlags=CameraClearFlags.SolidColor;
            view.backgroundColor=new Color(.19f,.23f,.18f);
            cameraGo.transform.localPosition=new Vector3(0,50,-25);
            cameraGo.transform.localRotation=Quaternion.Euler(64,0,0);
            target=world.ToWorld(64,86);
            UpdateView();
        }
        void Update()
        {
            // Keyboard pan for desktop test; touch pan for device.
            float dx=Input.GetAxisRaw("Horizontal"), dz=Input.GetAxisRaw("Vertical");
            target+=new Vector3(dx,0,dz)*panSpeed*Time.deltaTime;
            if(Input.touchCount==1&&Input.GetTouch(0).phase==TouchPhase.Moved)
            {
                var delta=Input.GetTouch(0).deltaPosition;
                target+=new Vector3(-delta.x,0,-delta.y)*zoom*.0025f;
            }
            if(Input.touchCount==2)
            {
                var a=Input.GetTouch(0);var b=Input.GetTouch(1);
                float old=Vector2.Distance(a.position-a.deltaPosition,b.position-b.deltaPosition);
                float now=Vector2.Distance(a.position,b.position);
                zoom=Mathf.Clamp(zoom+(old-now)*.045f,7,80);
            }
            zoom=Mathf.Clamp(zoom-Input.mouseScrollDelta.y*2f,7,80);
            float limit=ZoneOneWorld.Size*ZoneOneWorld.CellWorldSize*.5f;
            target.x=Mathf.Clamp(target.x,-limit,limit);
            target.z=Mathf.Clamp(target.z,-limit,limit);
            UpdateView();
        }
        void UpdateView()
        {
            cameraPivot.position=target;
            view.orthographicSize=zoom;
            needed.Clear();
            foreach(var key in world.VisibleChunks(target,chunkRadius))
            {
                needed.Add(key);
                if(!loaded.ContainsKey(key))loaded[key]=BuildChunk(key.x,key.y);
            }
            removal.Clear();
            foreach(var kv in loaded)if(!needed.Contains(kv.Key))removal.Add(kv.Key);
            foreach(var key in removal){Destroy(loaded[key]);loaded.Remove(key);}
        }
        static Color Tint(ZoneTerrain t)
        {
            switch(t)
            {
                case ZoneTerrain.Breach:return new Color(.29f,.11f,.35f);
                case ZoneTerrain.Sea:return new Color(.12f,.30f,.43f);
                case ZoneTerrain.Forest:return new Color(.19f,.31f,.19f);
                case ZoneTerrain.Mountain:return new Color(.38f,.37f,.32f);
                default:return new Color(.45f,.48f,.30f);
            }
        }
        static Color FeatureTint(ZoneFeature f)
        {
            switch(f)
            {
                case ZoneFeature.City:return new Color(.20f,.48f,.82f);
                case ZoneFeature.EnemyCamp:return new Color(.75f,.17f,.13f);
                case ZoneFeature.Ruin:return new Color(.55f,.50f,.45f);
                case ZoneFeature.Wood:return new Color(.11f,.24f,.11f);
                case ZoneFeature.Stone:return new Color(.75f,.76f,.70f);
                case ZoneFeature.Food:return new Color(.83f,.69f,.21f);
                default:return Color.white;
            }
        }
        GameObject BuildChunk(int cx,int cz)
        {
            var vertices=new List<Vector3>();
            var triangles=new List<int>();
            var colors=new List<Color>();
            foreach(var c in world.GetChunk(cx,cz))
            {
                float half=ZoneOneWorld.CellWorldSize*.5f;
                Vector3 p=world.ToWorld(c.X,c.Z);
                AddQuad(p,half,Tint(c.Terrain),vertices,triangles,colors);
                if(c.Feature!=ZoneFeature.None)
                    AddQuad(p+Vector3.up*.03f,half*.48f,FeatureTint(c.Feature),vertices,triangles,colors);
            }
            var mesh=new Mesh { indexFormat=UnityEngine.Rendering.IndexFormat.UInt32 };
            mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.SetColors(colors);mesh.RecalculateNormals();
            var go=new GameObject("Zone 1 chunk "+cx+","+cz);
            go.transform.SetParent(transform,false);
            var filter=go.AddComponent<MeshFilter>();filter.sharedMesh=mesh;
            var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;
            return go;
        }
        static void AddQuad(Vector3 p,float h,Color color,List<Vector3> v,List<int> t,List<Color> c)
        {
            int i=v.Count;
            v.Add(p+new Vector3(-h,0,-h));v.Add(p+new Vector3(-h,0,h));
            v.Add(p+new Vector3(h,0,h));v.Add(p+new Vector3(h,0,-h));
            t.Add(i);t.Add(i+1);t.Add(i+2);t.Add(i);t.Add(i+2);t.Add(i+3);
            for(int n=0;n<4;n++)c.Add(color);
        }
        void OnDestroy()
        {
            foreach(var kv in loaded)if(kv.Value!=null)Destroy(kv.Value);
            if(material!=null)Destroy(material);
        }
    }
}
