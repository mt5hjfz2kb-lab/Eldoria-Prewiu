using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eldoria.ArtTests.ThreeVisualStyles
{
    /// <summary>
    /// Three SELF-CONTAINED, isolated Unity 6 look-development scenes. No canonical
    /// world/PlayerState/LocalGateway changes; no simulated game progression.
    /// Only art, topology, camera, lighting, and a non-canonical Bastion hotspot.
    /// Geometry is generated when the scene opens, also in Edit mode.
    /// </summary>
    [ExecuteAlways]
    public sealed class ValoriaThreeStylesScene : MonoBehaviour
    {
        public enum ArtDirection { FantasyStrategy = 0, DetailedMedieval = 1, ColorfulCastle = 2 }
        public ArtDirection artDirection;
        [SerializeField] private bool showTrialTitle = true;
        private const string Container = "__LOOK_DEV_GENERATED_NOT_SAVED";
        private Transform built;
        private readonly Dictionary<Color32, Material> palette = new Dictionary<Color32, Material>();
        private Mesh conicalMesh;
        private int objectCount;

        private bool Rich => artDirection == ArtDirection.DetailedMedieval;
        private bool Bright => artDirection == ArtDirection.ColorfulCastle;

        private Color Stone => Rich ? RGB(0.74f, 0.70f, 0.62f) : Bright ? RGB(0.81f, 0.78f, 0.69f) : RGB(0.66f, 0.65f, 0.60f);
        private Color Trim => Rich ? RGB(0.84f, 0.79f, 0.65f) : Bright ? RGB(0.95f, 0.88f, 0.67f) : RGB(0.76f, 0.73f, 0.63f);
        private Color DarkStone => Rich ? RGB(0.48f, 0.47f, 0.49f) : Bright ? RGB(0.61f, 0.61f, 0.63f) : RGB(0.45f, 0.49f, 0.53f);
        private Color Roof => Rich ? RGB(0.20f, 0.28f, 0.40f) : Bright ? RGB(0.26f, 0.49f, 0.74f) : RGB(0.28f, 0.34f, 0.46f);
        private Color Wood => Rich ? RGB(0.31f, 0.23f, 0.18f) : Bright ? RGB(0.57f, 0.38f, 0.24f) : RGB(0.39f, 0.29f, 0.21f);
        private Color Green => Rich ? RGB(0.29f, 0.39f, 0.30f) : Bright ? RGB(0.40f, 0.65f, 0.37f) : RGB(0.32f, 0.48f, 0.32f);
        private Color Grass => Rich ? RGB(0.39f, 0.48f, 0.32f) : Bright ? RGB(0.52f, 0.72f, 0.42f) : RGB(0.41f, 0.56f, 0.35f);
        private Color Gold => RGB(0.94f, 0.69f, 0.24f);
        private Color Blue => Rich ? RGB(0.11f, 0.23f, 0.46f) : Bright ? RGB(0.17f, 0.40f, 0.76f) : RGB(0.13f, 0.32f, 0.53f);
        private static Color RGB(float r, float g, float b) => new Color(r, g, b, 1f);

        private void OnEnable()
        {
            if (transform.Find(Container) != null) return;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    if (this != null && gameObject.scene.IsValid() && transform.Find(Container) == null)
                        Build();
                };
                return;
            }
#endif
            Build();
        }

        [ContextMenu("Regenerate isolated Valoria trial")]
        public void Regenerate()
        {
            Transform existing = transform.Find(Container);
            if (existing != null)
            {
                if (Application.isPlaying) Destroy(existing.gameObject);
                else DestroyImmediate(existing.gameObject);
            }
            palette.Clear();
            conicalMesh = null;
            Build();
        }

        public void Build()
        {
            if (transform.Find(Container) != null) return;
            objectCount = 0;
            var holder = new GameObject(Container);
            // Recreate on every open. Generated meshes/materials do not pollute the Git repo
            // or accidentally become permanent/canonical Unity art.
            holder.hideFlags = HideFlags.DontSaveInEditor;
            holder.transform.SetParent(transform, false);
            built = holder.transform;
            CameraAndLighting();
            Terrain();
            RiverAndBridge();
            CentralRoad();
            LowerGate();
            UpperBastion();
            WorkDistricts();
            VegetationAndRocks();
            RuinsAndLife();
            Debug.Log("[ELDORIA THREE LOOKS] Built isolated " + artDirection +
                " scene; generated GameObjects=" + objectCount +
                " | no production game files modified.");
        }

        private Material Mat(Color color)
        {
            var key = (Color32)color;
            if (palette.TryGetValue(key, out var cached) && cached != null) return cached;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            var result = new Material(shader) { name = "Preview_" + key };
            result.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            if (result.HasProperty("_BaseColor")) result.SetColor("_BaseColor", color);
            if (result.HasProperty("_Color")) result.SetColor("_Color", color);
            if (result.HasProperty("_Smoothness")) result.SetFloat("_Smoothness", .16f);
            palette.Add(key, result);
            return result;
        }

        private GameObject Shape(PrimitiveType type, string name, Vector3 pos, Vector3 scale, Color c, Transform parent = null)
        {
            var g = GameObject.CreatePrimitive(type);
            g.name = name;
            g.transform.SetParent(parent == null ? built : parent, false);
            g.transform.localPosition = pos;
            g.transform.localScale = scale;
            g.GetComponent<Renderer>().sharedMaterial = Mat(c);
            var collider = g.GetComponent<Collider>();
            if (collider != null) { if (Application.isPlaying) Destroy(collider); else DestroyImmediate(collider); }
            objectCount++;
            return g;
        }

        private GameObject Cuboid(string n, float x, float y, float z, float sx, float sy, float sz, Color c)
            => Shape(PrimitiveType.Cube, n, new Vector3(x, y, z), new Vector3(sx, sy, sz), c);

        // Real creator-authored CC0 meshes; imported through Eldoria's existing glTFast.
        // NEVER call editor APIs in a player build. Runtime look-dev uses safe
        // procedural fallbacks; editor bake creates persistent native references.
        private bool Vendor(string relative, float x, float z, float width, float height,
                            float yOffset = 0f)
        {
#if UNITY_EDITOR
            string path = "Assets/Eldoria/ArtTests/ThreeVisualStyles/Vendor/" + relative;
            if (relative.StartsWith("EldoriaProduction/"))
                path = "Assets/Eldoria/Resources/Valoria/ProductionArt/StarterFamily/" +
                       relative.Substring("EldoriaProduction/".Length);
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) return false;
            var inst = UnityEditor.PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (inst == null) return false;
            inst.name = "AUTHENTIC SOURCE — " + System.IO.Path.GetFileNameWithoutExtension(path);
            inst.transform.SetParent(built, false);
            inst.transform.position = new Vector3(x, H(x, z), z);
            var renderers = inst.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                if (Application.isPlaying) Destroy(inst);
                else DestroyImmediate(inst);
                return false;
            }
            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
            float horizontal = Mathf.Max(.01f, b.size.x, b.size.z);
            float vertical = Mathf.Max(.01f, b.size.y);
            float factor = Mathf.Min(width / horizontal, height / vertical);
            if (float.IsNaN(factor) || float.IsInfinity(factor) || factor <= 0) return false;
            inst.transform.localScale *= factor;
            // Recalculate world bounds after applying uniform scale.
            b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
            inst.transform.position += new Vector3(x - b.center.x,
                H(x, z) + yOffset - b.min.y, z - b.center.z);
            objectCount += renderers.Length;
            return true;
#else
            return false;
#endif
        }

        private void Cone(string n, Vector3 p, float radius, float height, Color c, int sides = 8)
        {
            if (conicalMesh == null)
            {
                int count = 12;
                var vs = new Vector3[count + 2];
                var tris = new int[count * 6];
                vs[0] = Vector3.up;
                vs[count + 1] = Vector3.zero;
                for (int i = 0; i < count; i++)
                    vs[i + 1] = new Vector3(Mathf.Cos(i * 2f * Mathf.PI / count), 0f, Mathf.Sin(i * 2f * Mathf.PI / count));
                for (int i = 0; i < count; i++)
                {
                    int next = (i + 1) % count;
                    tris[3*i] = 0; tris[3*i+1] = next + 1; tris[3*i+2] = i + 1;
                    int o = count * 3 + i * 3;
                    tris[o] = count + 1; tris[o+1] = i + 1; tris[o+2] = next + 1;
                }
                conicalMesh = new Mesh { name = "ReusableCone12Sides" };
                conicalMesh.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                conicalMesh.vertices = vs;
                conicalMesh.triangles = tris;
                conicalMesh.RecalculateNormals();
                conicalMesh.RecalculateBounds();
            }
            var go = new GameObject(n);
            go.transform.SetParent(built, false);
            go.transform.localPosition = p;
            go.transform.localScale = new Vector3(radius, height, radius);
            go.AddComponent<MeshFilter>().sharedMesh = conicalMesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = Mat(c);
            objectCount++;
        }

        private float Elevation(float x, float z)
        {
            float side = Mathf.Pow(Mathf.Clamp01((Mathf.Abs(x) - 20f) / 30f), 1.7f) *
                         (Rich ? 22f : Bright ? 10f : 15f);
            float north = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(4f, 26f, z)) *
                          (Rich ? 13f : Bright ? 4.6f : 6.2f);
            float valley = -.24f * Mathf.Cos(x * .20f) * Mathf.Sin(z * .12f);
            float stream = -3f * Mathf.Exp(-Mathf.Pow((z + 30f) / 4.2f, 2f));
            float shoulders = Rich ? Mathf.Pow(Mathf.Clamp01((Mathf.Abs(x) - 33f) / 20f), 2f) * 9f : 0f;
            return side + north + valley + stream + shoulders;
        }
        private float H(float x, float z) => Elevation(x, z);

        private void CameraAndLighting()
        {
            var cam = new GameObject("REVIEW CAMERA - select Game tab (orthographic, not canonical runtime)");
            cam.transform.SetParent(built, false);
            cam.transform.position = new Vector3(55f, Rich ? 77f : 67f, -72f);
            cam.transform.LookAt(new Vector3(0f, Rich ? 9f : 4f, 0f));
            var c = cam.AddComponent<Camera>();
            c.tag = "MainCamera";
            c.orthographic = true;
            c.orthographicSize = Rich ? 46f : 42f;
            c.nearClipPlane = .1f;
            c.farClipPlane = 300f;
            c.clearFlags = CameraClearFlags.SolidColor;
            c.backgroundColor = Rich ? RGB(.67f,.74f,.80f) : Bright ? RGB(.61f,.81f,.94f) : RGB(.69f,.79f,.83f);
            var sun = new GameObject("Afternoon sun / shadows");
            sun.transform.SetParent(built, false);
            sun.transform.rotation = Quaternion.Euler(46f, -38f, 0f);
            var light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = RGB(1f,.94f,.82f);
            light.intensity = Rich ? 1.35f : 1.5f;
            light.shadows = LightShadows.Soft;
            light.shadowStrength = .65f;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = Rich ? RGB(.44f,.48f,.56f) : RGB(.59f,.61f,.59f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = c.backgroundColor;
            RenderSettings.fogStartDistance = Rich ? 105f : 112f;
            RenderSettings.fogEndDistance = Rich ? 220f : 240f;
        }

        private void Terrain()
        {
            const int segments = 72;
            const float size = 112f;
            int stride = segments + 1;
            var v = new Vector3[stride * stride];
            var uv = new Vector2[v.Length];
            int[] index = new int[segments * segments * 6];
            for (int iz = 0; iz <= segments; iz++)
                for (int ix = 0; ix <= segments; ix++)
                {
                    float x = -size * .5f + size * ix / segments;
                    float z = -size * .5f + size * iz / segments;
                    int j = iz * stride + ix;
                    v[j] = new Vector3(x, H(x,z), z);
                    uv[j] = new Vector2(x * .07f,z * .07f);
                }
            for (int iz = 0; iz < segments; iz++)
                for (int ix = 0; ix < segments; ix++)
                {
                    int a=iz*stride+ix, b=a+1, d=a+stride, e=d+1;
                    int j=(iz*segments+ix)*6;
                    index[j]=a;index[j+1]=d;index[j+2]=b;
                    index[j+3]=b;index[j+4]=d;index[j+5]=e;
                }
            var m = new Mesh { name = "ContinuousSculptedLandmass" };
            m.indexFormat = IndexFormat.UInt32;
            m.vertices=v; m.uv=uv; m.triangles=index;
            m.RecalculateNormals();
            var go = new GameObject("Continuous terrain (not disconnected cubes)");
            go.transform.SetParent(built,false);
            go.AddComponent<MeshFilter>().sharedMesh=m;
            go.AddComponent<MeshRenderer>().sharedMaterial=Mat(Grass);
            objectCount++;
            // Selective natural bands around the main plateau help show playable space.
            for (int i=0;i<38;i++)
            {
                float x = -19f + (i % 19) * 2f, z = 36f + (i / 19) * 2f;
                if (Mathf.Abs(x)<11f) continue;
                Shape(PrimitiveType.Sphere,"Hill boulder",new Vector3(x,H(x,z)+.20f,z),
                     new Vector3(1.4f,.6f,1.25f),i%3==0?DarkStone:Stone);
            }
        }

        private void RiverAndBridge()
        {
            var water=Rich?RGB(.19f,.43f,.53f):Bright?RGB(.19f,.64f,.82f):RGB(.22f,.48f,.61f);
            Cuboid("Creek flowing west to east",-0f,-1.48f,-30f,106f,.12f,8.2f,water);
            for(int i=0;i<32;i++)
            {
                float x=-48+i*3.1f;
                if(Mathf.Abs(x)<5f)continue;
                foreach(int side in new[]{-1,1})
                {
                    float z=-30f+side*5f;
                    Shape(PrimitiveType.Sphere,"Riverbank stone",new Vector3(x,H(x,z)+.2f,z),
                         new Vector3(1.6f,.9f,1.15f),i%3==0?Stone:DarkStone);
                }
            }
            // The deck joins the approach on either side of the actual water channel.
            float deckHeight=H(0,-36)+.42f;
            Cuboid("STONE BRIDGE across stream",0,deckHeight,-30,9f,1.2f,12.4f,Stone);
            for(int side=-1;side<=1;side+=2)
            {
                Cuboid("Bridge parapet",side*4.2f,deckHeight+1f,-30,.6f,1.8f,12.1f,DarkStone);
                for(int t=0;t<5;t++)
                    Cuboid("Parapet merlon",side*4.2f,deckHeight+2.05f,-35+t*2.5f,.82f,.55f,.8f,Trim);
            }
        }

        private void CentralRoad()
        {
            for (int i=0;i<59;i++)
            {
                float z=-50+i*1.48f;
                float y=H(0,z)+.17f;
                Cuboid("Road flagstone",0,y,z,5.5f,.17f,1.45f,
                       (i%7==0)?Trim:(Rich?Stone:DarkStone));
                if(i%3==0)
                {
                    Cuboid("Central seam",-.6f,y+.10f,z,.12f,.03f,1.36f,Trim);
                    Cuboid("Road border",-2.8f,y+.06f,z,.25f,.25f,1.42f,Stone);
                    Cuboid("Road border",2.8f,y+.06f,z,.25f,.25f,1.42f,Stone);
                }
            }
        }

        private void LowerGate()
        {
            float z=-13f, y=H(0,z);
            for(int side=-1;side<=1;side+=2)
            {
                float x=side*5.5f;
                Tower(x,z,y,Bright?8.3f:Rich?14f:10.8f,Rich?3.4f:2.95f,"LOWER GATE");
                Cuboid("Defensive wing",side*13.3f,y+3f,z,11f,6f,2.5f,Stone);
                Battlements(side*13.3f,y+6.1f,z,11f,true);
            }
            Cuboid("Gate lintel",0,y+9f,z,6.8f,3f,2.8f,Stone);
            bool gateModel = Bright
                ? Vendor("Kenney/castle/gate.glb", 0, z-1.7f, 5.7f, 6.9f)
                : Rich && Vendor("EldoriaProduction/Valoria_MainGate_v1.glb", 0, z-1.7f, 6f, 8.7f);
            if (!gateModel)
                Cuboid("Raised wooden portcullis",0,y+4f,z-.28f,5f,6.4f,.32f,Wood);
            for(int i=0;i<6;i++)
                Cuboid("Portcullis vertical timber",-2.15f+i*.85f,y+4f,z-.57f,.16f,6.4f,.28f,DarkStone);
            Banner(-5.5f,y+8f,z-1.95f);
            Banner(5.5f,y+8f,z-1.95f);
            for(int s=-1;s<=1;s+=2)Flame(s*3.6f,y+3.6f,z-2f);
        }

        private void UpperBastion()
        {
            float z=32f, y=H(0,z);
            float width=Rich?24f:Bright?18f:21f;
            float depth=Rich?18f:Bright?16f:16f;
            Cuboid("BASTION I — fortified footprint, not Bastion II",0,y+.36f,z,width+2f,.7f,depth+2f,DarkStone);
            Cuboid("Bastion main hall",0,y+5.6f,z+2f,width*.63f,11.1f,depth*.63f,Stone);
            Cuboid("Bastion upper stone cornice",0,y+11.5f,z+2f,width*.67f,.9f,depth*.67f,Trim);
            for(int s=-1;s<=1;s+=2)for(int t=-1;t<=1;t+=2)
            {
                float tx=s*(width*.5f),tz=z+t*(depth*.5f);
                Tower(tx,tz,y,Rich?21f:Bright?13f:16f,Rich?3.1f:2.75f,"BASTION CORNER");
                if(t==-1) Banner(tx,y+(Rich?12f:9f),tz-2.9f);
            }
            Cuboid("Bastion south-facing wall",0,y+5.9f,z-depth*.5f,width,11.2f,2.1f,Stone);
            Cuboid("Bastion north wall",0,y+5.9f,z+depth*.5f,width,11.2f,2.1f,Stone);
            for(int side=-1;side<=1;side+=2)Cuboid("Bastion flank wall",side*width*.5f,y+5.6f,z,1.8f,10.7f,depth,Stone);
            Cuboid("Vaulted entrance reveal",0,y+3.8f,z-depth*.5f-1.13f,3.4f,7.5f,.34f,Wood);
            Cuboid("Grand bastion crest",0,y+10.6f,z-depth*.5f-1.3f,4.5f,2.1f,.44f,Blue);
            Battlements(0,y+11.8f,z-depth*.5f,width,true);
            if(Rich)
            {
                for(int i=-2;i<=2;i++)
                {
                    Cuboid("Historic buttress",i*3.4f,y+3.8f,z-depth*.5f-1.2f,.85f,7.6f,1.5f,Trim);
                    Cuboid("Bas-relief window",i*3.4f,y+8f,z-depth*.5f-1.4f,.7f,1.6f,.28f,DarkStone);
                }
                Tower(0,z+2f,y+10.9f,15f,4.5f,"CENTRAL KEEP TOWER");
                Banner(0,y+22f,z-depth*.5f-1.6f);
            }
            else if (Bright)
            {
                Tower(0,z+2f,y+10f,7f,3.4f,"BLUE CENTRAL KEEP");
                for(int i=-2;i<=2;i++)
                    Cuboid("Colored battlement tile",i*2.7f,y+12f,z-depth*.5f-1.2f,1.9f,1f,.5f,Trim);
            }
        }

        private void Tower(float x,float z,float foundation,float height,float radius,string prefix)
        {
            float center=foundation+height*.5f;
            bool genuineKenney = Bright && Vendor("Kenney/castle/tower-square-base.glb",
                                      x,z,radius*2f,height*.85f);
            if (!genuineKenney)
                Shape(Rich?PrimitiveType.Cylinder:PrimitiveType.Cube,prefix+" tower masonry",
                      new Vector3(x,center,z),
                      Rich?new Vector3(radius,height*.5f,radius):
                           new Vector3(radius*2f,height,radius*2f),Stone);
            Shape(PrimitiveType.Cylinder,prefix+" parapet rim",
                  new Vector3(x,foundation+height+.12f,z),
                  new Vector3(radius*1.15f,.26f,radius*1.15f),Trim);
            Cone(prefix+" stylized roof",new Vector3(x,foundation+height+.34f,z),
                 radius*(Bright?1.55f:1.2f),Rich?4f:Bright?3.9f:2.6f,Roof);
            Cuboid(prefix+" arrow slit",x,foundation+height*.61f,z-radius-0.09f,.3f,1.3f,.15f,DarkStone);
            if(Rich)
            {
                for(int k=0;k<4;k++)
                {
                    float a=k*Mathf.PI*.5f;
                    Cuboid("Stone defensive corbel",x+Mathf.Sin(a)*radius*.91f,
                           foundation+height-.9f,z+Mathf.Cos(a)*radius*.91f,
                           .8f,.75f,.85f,Trim);
                }
            }
        }

        private void Battlements(float x,float y,float z,float span,bool alongX)
        {
            int count=Mathf.Max(4,Mathf.RoundToInt(span/2.3f));
            for(int i=0;i<count;i++)
            {
                float p=-span*.5f+(i+.5f)*span/count;
                Cuboid("Merlon",alongX?x+p:x,y+.6f,alongX?z:z+p,1.0f,1.15f,1.1f,Trim);
            }
        }

        private void House(float x,float z,float size,int seed,string label)
        {
            float y=H(x,z);
            float w=size*(seed%2==0?1.1f:.85f), h=size*(Rich?1.1f:.88f);
            var wallColor=seed%3==0?Trim:(seed%3==1?Stone:RGB(.70f,.59f,.43f));
            Cuboid(label+" lower walls",x,y+h*.5f,z,w,h,w*.83f,wallColor);
            Cuboid(label+" stone footing",x,y+.23f,z,w+.55f,.46f,w*1.02f,DarkStone);
            for(int s=-1;s<=1;s+=2)
            {
                var roofPart=Cuboid(label+" sloping roof",x+s*w*.28f,y+h+size*.22f,z,
                                    w*.65f,.26f,w*1.04f,Roof);
                roofPart.transform.localRotation=Quaternion.Euler(0,0,s*32f);
            }
            Cuboid("Building doorway",x,y+1.05f,z-w*.43f-.06f,.8f,2.1f,.16f,Wood);
            for(int s=-1;s<=1;s+=2)
            {
                Cuboid("Framed lit window",x+s*w*.29f,y+h*.65f,z-w*.43f-.075f,.62f,.85f,.16f,Gold);
                Cuboid("Shutter",x+s*w*.29f+.4f,y+h*.65f,z-w*.43f-.1f,.13f,1.04f,.21f,Wood);
            }
            if(Rich)
            {
                for(int s=-1;s<=1;s+=2)
                    Cuboid("Timber-frame structural post",x+s*(w*.45f),y+h*.55f,z-w*.44f,.22f,h*.86f,.25f,Wood);
                Cuboid("Roof chimney",x+w*.28f,y+h+1.25f,z+.55f,.75f,2.45f,.85f,DarkStone);
            }
        }

        private void WorkDistricts()
        {
            if (!(artDirection == ArtDirection.FantasyStrategy &&
                  Vendor("QuaterniusRTS/Storage_FirstAge_Leve3.gltf",-17f,6f,9f,8f)))
                House(-17f,6f,5.8f,1,"ASERRADERO (art-only location)");
            if (!(artDirection == ArtDirection.FantasyStrategy &&
                  Vendor("QuaterniusRTS/Barracks_FirstAge_Level3.gltf",17f,6f,11f,9f)))
                House(17f,6f,6.3f,2,"CUARTEL (art-only location)");
            if (!(artDirection == ArtDirection.FantasyStrategy &&
                  Vendor("QuaterniusRTS/Houses_SecondAge_1_Level3.gltf",-22f,19f,7f,7f)) &&
                !(Rich && Vendor("EldoriaProduction/Valoria_CivicHouse_v1.glb",-22f,19f,7f,7f)))
                House(-22f,19f,4.5f,3,"Civilian house under repair");
            if (!(artDirection == ArtDirection.FantasyStrategy &&
                  Vendor("QuaterniusRTS/Market_FirstAge_Level3.gltf",22f,21f,9f,7f)) &&
                !(Rich && Vendor("EldoriaProduction/Valoria_Workshop_v1.glb",22f,21f,8f,7f)))
                House(22f,21f,4.4f,4,"Stone mason workshop");
            if (artDirection == ArtDirection.FantasyStrategy)
            {
                Vendor("QuaterniusRTS/Houses_SecondAge_2_Level3.gltf",-27f,29f,8f,8f);
                Vendor("QuaterniusRTS/TownCenter_FirstAge_Level3.gltf",23f,35f,10f,9f);
                Vendor("QuaterniusRTS/Windmill_FirstAge.gltf",-34f,-2f,8f,11f);
            }
            if (Bright)
            {
                Vendor("Kenney/fantasy-town/fountain-square.glb",-17f,-15f,5f,4f);
                Vendor("Kenney/fantasy-town/cart.glb",13f,-26f,4f,3f);
                Vendor("Kenney/castle/bridge-straight.glb",10f,-30f,5f,3f);
            }
            if(Rich)
            {
                House(-29f,10f,4.7f,5,"Upper artisan house");
                House(28f,12f,4.6f,6,"Storage granary");
                House(-13f,25f,3.5f,7,"Civic annex");
            }
            if(Bright)
            {
                House(-12f,22f,3.8f,7,"Carpenter");
                House(13f,20f,3.9f,8,"Gardener");
            }
            // Rebuild and farming clues: derived from the web slice's economy concept.
            float farmY=H(-26,-2)+.17f;
            Cuboid("Golden cultivation field",-27f,farmY,-2f,10f,.22f,11f,RGB(.74f,.64f,.27f));
            for(int i=0;i<8;i++)
                Cuboid("Wheat row",-31+i*1.1f,farmY+.19f,-2, .22f,.35f,9.7f,RGB(.89f,.73f,.35f));
            for(int j=0;j<4;j++)
            {
                float x=22+j*2.2f,z=-2f;
                Cuboid("Timber drying rack",x,H(x,z)+1.5f,z,.34f,3f,4.5f,Wood);
                Cuboid("Wood stock",x,H(x,z)+.31f,z+1.1f,1.5f,.55f,1.1f,Wood);
            }
        }

        private void VegetationAndRocks()
        {
            var rng=new System.Random(artDirection==ArtDirection.FantasyStrategy?71:artDirection==ArtDirection.DetailedMedieval?177:317);
            for(int i=0;i<(Rich?115:Bright?80:90);i++)
            {
                float x=(float)(rng.NextDouble()*102-51);
                float z=(float)(rng.NextDouble()*99-49);
                if(Mathf.Abs(x)<12f && z>-25f && z<43f)continue;
                if(Mathf.Abs(x)<6f && z<-24f)continue;
                float y=H(x,z);
                float height=2.5f+(float)rng.NextDouble()*3.7f;
                if (Bright && i < 18 && Vendor("Kenney/castle/tree-small.glb",x,z,height*1.5f,height*1.8f))
                    continue;
                Shape(PrimitiveType.Cylinder,"Pine trunk",new Vector3(x,y+height*.27f,z),
                      new Vector3(.25f,height*.29f,.25f),Wood);
                for(int t=0;t<2;t++)
                    Cone("Conifer canopy",new Vector3(x,y+height*.26f+t*height*.25f,z),
                         height*(Bright?.30f:.27f)*(1f-t*.18f),
                         height*.54f,Green*(t==0?1f:1.12f));
            }
            for(int i=0;i<130;i++)
            {
                float x=(float)(rng.NextDouble()*103-51),z=(float)(rng.NextDouble()*102-51);
                if(Mathf.Abs(x)<6 && z>-35 && z<40)continue;
                float y=H(x,z);
                float r=.5f+(float)rng.NextDouble()*1.5f;
                var rock=Shape(PrimitiveType.Sphere,"Rock scatter",new Vector3(x,y+.08f,z),
                               new Vector3(r*1.6f,r*.8f,r*1.2f),i%3==0?DarkStone:Stone);
                rock.transform.localRotation=Quaternion.Euler(0,(float)rng.NextDouble()*170f,13f);
            }
            // A few silhouettes in the distance make the geography believable.
            for(int i=0;i<8;i++)
            {
                float x=-53+i*15.2f,z=56f;
                Cone("Distant mountain ridgeline",new Vector3(x,H(x,52)+1f,z),
                     12f+(i%3)*4f,23f+i%3*4f,DarkStone);
            }
        }

        private void RuinsAndLife()
        {
            for(int k=0;k<7;k++)
            {
                float x= k%2==0 ? -33f+k*.8f : 28f+k*.7f;
                float z= k<3 ? -17f+k*6f : 3f+k*6f;
                float y=H(x,z);
                Cuboid("Recovered wall stump",x,y+1.1f,z,2f,2.2f,.9f,DarkStone);
                Cuboid("Rubble at ruined district",x+1.2f,y+.15f,z+1.2f,2.6f,.3f,2.2f,Stone);
            }
            for(int k=0;k<14;k++)
            {
                float z=-38+k*5.0f, x=(k%2==0?8.5f:-9.5f);
                float y=H(x,z);
                if(z>-10f && z<12f && k%3==0)continue;
                Cuboid("Valoria blue banner pole",x,y+2.6f,z,.13f,5.3f,.13f,Wood);
                Cuboid("Valoria pennant",x+.65f,y+4.7f,z,1.2f,.9f,.1f,Blue);
            }
        }

        private void Banner(float x,float y,float z)
        {
            Cuboid("Valoria blue banner",x,y,z,1.25f,2.8f,.14f,Blue);
            Cuboid("Gold heraldic device",x,y,z-.12f,.34f,.75f,.09f,Gold);
        }
        private void Flame(float x,float y,float z)
        {
            Shape(PrimitiveType.Cylinder,"Brazier",new Vector3(x,y,z),new Vector3(.44f,.31f,.44f),DarkStone);
            Cone("Warm fire",new Vector3(x,y+.27f,z),.32f,.9f,Gold);
        }

        private void OnGUI()
        {
            if (!Application.isPlaying || !showTrialTitle) return;
            var name=artDirection==ArtDirection.FantasyStrategy?"01 / FANTASIA 3D ESTRATEGIA":
                     artDirection==ArtDirection.DetailedMedieval?"02 / MEDIEVAL ESTILIZADO DETALLADO":
                     "03 / MEDIEVAL COLORIDO";
            GUI.Box(new Rect(12,12,420,58),
                "ELDORIA — VALORIA • " + name + "\n" +
                "PRUEBA ARTISTICA / NO GAMEPLAY CERTIFICADO");
        }
    }
}
