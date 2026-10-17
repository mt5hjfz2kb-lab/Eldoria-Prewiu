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
        private int authenticSourceInstances;
        public int AuthenticSourceInstances => authenticSourceInstances;

        private bool Rich => artDirection == ArtDirection.DetailedMedieval;
        private bool Bright => artDirection == ArtDirection.ColorfulCastle;

        private Color Stone => Rich ? RGB(0.74f, 0.70f, 0.62f) : Bright ? RGB(0.81f, 0.78f, 0.69f) : RGB(0.66f, 0.65f, 0.60f);
        private Color Trim => Rich ? RGB(0.84f, 0.79f, 0.65f) : Bright ? RGB(0.95f, 0.88f, 0.67f) : RGB(0.76f, 0.73f, 0.63f);
        private Color DarkStone => Rich ? RGB(0.48f, 0.47f, 0.49f) : Bright ? RGB(0.61f, 0.61f, 0.63f) : RGB(0.45f, 0.49f, 0.53f);
        private Color Roof => Rich ? RGB(0.20f, 0.28f, 0.40f) : Bright ? RGB(0.26f, 0.49f, 0.74f) : RGB(0.28f, 0.34f, 0.46f);
        private Color Wood => Rich ? RGB(0.31f, 0.23f, 0.18f) : Bright ? RGB(0.57f, 0.38f, 0.24f) : RGB(0.39f, 0.29f, 0.21f);
        private Color Green => Rich ? RGB(0.29f, 0.39f, 0.30f) : Bright ? RGB(0.40f, 0.65f, 0.37f) : RGB(0.32f, 0.48f, 0.32f);
        private Color Grass => Rich ? RGB(0.39f, 0.48f, 0.32f) : Bright ? RGB(0.36f, 0.58f, 0.33f) : RGB(0.41f, 0.56f, 0.35f);
        private Color Gold => RGB(0.94f, 0.69f, 0.24f);
        private Color Blue => Rich ? RGB(0.11f, 0.23f, 0.46f) : Bright ? RGB(0.17f, 0.40f, 0.76f) : RGB(0.13f, 0.32f, 0.53f);
        private static Color RGB(float r, float g, float b) => new Color(r, g, b, 1f);

        private void OnEnable()
        {
            if (transform.Find(Container) != null) return;
#if UNITY_EDITOR
            if (!UnityEngine.Application.isPlaying)
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
                if (UnityEngine.Application.isPlaying) Destroy(existing.gameObject);
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
            authenticSourceInstances = 0;
            var holder = new GameObject(Container);
            // Recreate on every open. Generated meshes/materials do not pollute the Git repo
            // or accidentally become permanent/canonical Unity art.
            holder.hideFlags = HideFlags.DontSaveInEditor;
            holder.transform.SetParent(transform, false);
            built = holder.transform;
            CameraAndLighting();
            Terrain();
            if (Rich)
            {
                BuildAshenHighlands();
            }
            else if (Bright)
            {
                BuildSunlitRingTown();
            }
            else
            {
                RiverAndBridge();
                CentralRoad();
                LowerGate();
                UpperBastion();
                WorkDistricts();
                EnrichStrategyValley();
            }
            VegetationAndRocks();
            if (!Rich && !Bright) RuinsAndLife();
            Debug.Log("[ELDORIA THREE LOOKS] Built isolated " + artDirection +
                " scene; generated GameObjects=" + objectCount +
                " | genuine source model instances=" + authenticSourceInstances +
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
            if (collider != null) { if (UnityEngine.Application.isPlaying) Destroy(collider); else DestroyImmediate(collider); }
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
            if (relative.StartsWith("ValoriaHero/"))
                path = "Assets/Eldoria/Resources/Valoria/HeroBastionGenerated/" +
                       relative.Substring("ValoriaHero/".Length);
            if (relative.StartsWith("ValoriaStoneV2/"))
                path = "Assets/Eldoria/Resources/Valoria/ProductionArt/StoneDefensiveFamilyV2/" +
                       relative.Substring("ValoriaStoneV2/".Length);
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
                if (UnityEngine.Application.isPlaying) Destroy(inst);
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
            if (Bright && relative.StartsWith("Kenney/castle/"))
            {
                Color tint = relative.Contains("roof") ? RGB(.48f,.64f,.98f) :
                             relative.Contains("wall") ? RGB(.90f,.83f,.69f) :
                             RGB(.94f,.86f,.76f);
                foreach (var renderer in renderers)
                {
                    var block = new MaterialPropertyBlock();
                    renderer.GetPropertyBlock(block);
                    block.SetColor("_BaseColor",tint);
                    renderer.SetPropertyBlock(block);
                }
            }
            objectCount += renderers.Length;
            authenticSourceInstances++;
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
            c.backgroundColor = Rich ? RGB(.43f,.50f,.60f) : Bright ? RGB(.53f,.74f,.88f) : RGB(.69f,.79f,.83f);
            var sun = new GameObject("Afternoon sun / shadows");
            sun.transform.SetParent(built, false);
            sun.transform.rotation = Quaternion.Euler(46f, -38f, 0f);
            var light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = RGB(1f,.94f,.82f);
            light.intensity = Rich ? 1.15f : Bright ? 1.05f : 1.22f;
            light.shadows = LightShadows.Soft;
            light.shadowStrength = .65f;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = Rich ? RGB(.35f,.39f,.44f) : Bright ? RGB(.41f,.43f,.40f) : RGB(.48f,.51f,.50f);
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
            var terrainMaterial=new Material(Mat(Color.white));
            terrainMaterial.name="Isolated terrain painting for " + artDirection;
            terrainMaterial.hideFlags=HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            var texture=GroundTexture();
            if (terrainMaterial.HasProperty("_BaseMap")) terrainMaterial.SetTexture("_BaseMap",texture);
            if (terrainMaterial.HasProperty("_MainTex")) terrainMaterial.SetTexture("_MainTex",texture);
            go.AddComponent<MeshRenderer>().sharedMaterial=terrainMaterial;
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
            Cuboid("Creek flowing west to east",-0f,-1.48f,-30f,96f,.12f,8.2f,water);
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
            // In the Quaternius route the upper keep MUST be recognizable as a
            // source-art strategy building, not a huge untextured placeholder box.
            bool sourceKeep = artDirection == ArtDirection.FantasyStrategy &&
                Vendor("QuaterniusRTS/TownCenter_FirstAge_Level3.gltf",0,z+2f,19f,19f);
            if(!sourceKeep)
            {
                Cuboid("Bastion main hall",0,y+5.6f,z+2f,width*.63f,11.1f,depth*.63f,Stone);
                Cuboid("Bastion upper stone cornice",0,y+11.5f,z+2f,width*.67f,.9f,depth*.67f,Trim);
            }
            else
            {
                var marker = new GameObject("Bastion main hall");
                marker.transform.SetParent(built,false);
                marker.transform.position = new Vector3(0,y+6,z+2f);
            }
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
            bool hexagonal = prefix.StartsWith("BASTION", StringComparison.Ordinal);
            string family = hexagonal ? "tower-hexagon" : "tower-square";
            float baseOffset = foundation - H(x, z);
            bool genuineKenney = Bright && Vendor("Kenney/castle/" + family + "-base.glb",
                                      x,z,radius*2f,height*.45f,baseOffset);
            if (genuineKenney)
            {
                // Actual same-author tower modules, stacked at intentional world heights.
                Vendor("Kenney/castle/" + (hexagonal ? "tower-hexagon-mid" : "tower-square-mid-windows") +
                       ".glb",x,z,radius*2f,height*.48f,baseOffset+height*.42f);
                Vendor("Kenney/castle/" + (hexagonal ? "tower-hexagon-roof" : "tower-square-top-roof") +
                       ".glb",x,z,radius*2.35f,height*.24f,baseOffset+height*.86f);
            }
            else
                Shape(Rich?PrimitiveType.Cylinder:PrimitiveType.Cube,prefix+" tower masonry",
                      new Vector3(x,center,z),
                      Rich?new Vector3(radius,height*.5f,radius):
                           new Vector3(radius*2f,height,radius*2f),Stone);
            if (!genuineKenney)
            {
                Shape(PrimitiveType.Cylinder,prefix+" parapet rim",
                      new Vector3(x,foundation+height+.12f,z),
                      new Vector3(radius*1.15f,.26f,radius*1.15f),Trim);
                Cone(prefix+" stylized roof",new Vector3(x,foundation+height+.34f,z),
                     radius*(Bright?1.55f:1.2f),Rich?4f:Bright?3.9f:2.6f,Roof);
            }
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
            // Keep focus on the reachable town rather than huge white cone mountains.
            for(int i=0;i<6;i++)
            {
                float x=-48+i*19.2f,z=57f;
                Cone("Distant alpine silhouette",new Vector3(x,H(x,52)-2f,z),
                     Rich ? 13f : 9f,Rich ? 19f : 12f,DarkStone);
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


        private Texture2D GroundTexture()
        {
            const int side=256;
            var texture=new Texture2D(side,side,TextureFormat.RGBA32,false,false);
            texture.name="Eldoria isolated palette • terrain";
            texture.wrapMode=TextureWrapMode.Repeat;
            texture.filterMode=FilterMode.Bilinear;
            texture.hideFlags=HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            Color low=Rich?RGB(.33f,.37f,.34f):Bright?RGB(.32f,.56f,.31f):RGB(.34f,.51f,.31f);
            Color high=Rich?RGB(.59f,.59f,.54f):Bright?RGB(.53f,.72f,.42f):RGB(.52f,.62f,.41f);
            Color soil=Rich?RGB(.37f,.37f,.36f):Bright?RGB(.59f,.53f,.36f):RGB(.55f,.52f,.36f);
            var colors=new Color[side*side];
            for(int y=0;y<side;y++)for(int x=0;x<side;x++)
            {
                float u=(float)x/side, v=(float)y/side;
                float broad=Mathf.PerlinNoise(u*4.2f+23.7f,v*4.2f+8.9f);
                float fine=Mathf.PerlinNoise(u*19.7f+8.1f,v*19.7f+62.3f);
                float grain=Mathf.PerlinNoise(u*74f+7.6f,v*74f+15.2f);
                Color c=Color.Lerp(low,high,Mathf.Clamp01(.12f+broad*.68f+fine*.2f));
                float exposed=Mathf.PerlinNoise(u*9f+17f,v*9f+38f);
                c=Color.Lerp(c,soil,Mathf.SmoothStep(.48f,.74f,exposed)*(Rich?.42f:.27f));
                float brightness=.88f+.19f*fine+.06f*grain;
                colors[y*side+x]=new Color(c.r*brightness,c.g*brightness,c.b*brightness,1);
            }
            texture.SetPixels(colors);
            texture.Apply(false,false);
            return texture;
        }

        // DIFFERENT GEOGRAPHIES AND BUILDING GRAMMARS.
        // These are actual editor-created mesh objects for Unity previews, not mock-up images.
        // Never touch the canon world, state machine, timeline or progression graph.
        private void Walkway(string name, Vector3 start, Vector3 end, float width, Color color, int subdivisions=24)
        {
            for (int i=0;i<subdivisions;i++)
            {
                float t0=(float)i/subdivisions, t1=(float)(i+1)/subdivisions;
                Vector3 a=Vector3.Lerp(start,end,t0), b=Vector3.Lerp(start,end,t1);
                a.y=H(a.x,a.z)+.23f; b.y=H(b.x,b.z)+.23f;
                Vector3 mid=(a+b)*.5f;
                var stone=Shape(PrimitiveType.Cube,name+" "+i,mid,
                    new Vector3(width,.22f,Vector3.Distance(a,b)*1.065f),
                    (i%5==0)?Stone:color);
                stone.transform.rotation=Quaternion.LookRotation((b-a).normalized,Vector3.up);
            }
        }

        private void Curtain(string name,Vector2 a, Vector2 b,float height,float thick,Color color)
        {
            Vector2 delta=b-a;
            Vector2 mid=(a+b)*.5f;
            float y=(H(a.x,a.y)+H(b.x,b.y))*.5f;
            var go=Cuboid(name,mid.x,y+height*.5f,mid.y,delta.magnitude,height,thick,color);
            go.transform.rotation=Quaternion.Euler(0,-Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg,0);
            // Clamping the footing to a continuous terrain mesh; corner pilasters conceal slopes.
            for(int i=0;i<=4;i++)
            {
                Vector2 p=Vector2.Lerp(a,b,i*.25f);
                float h=H(p.x,p.y);
                Shape(PrimitiveType.Cube,name+" stepped stone foot",new Vector3(p.x,h+.45f,p.y),
                    new Vector3(thick*1.22f,.9f,thick*1.22f),DarkStone);
            }
        }

        private void CarvedCliff(float x,float z,float width,float height,float depth,Color color)
        {
            float y=H(x,z);
            var piece=Cuboid("Alpine rock escarpment",x,y+height*.42f,z,width,height,depth,color);
            piece.transform.rotation=Quaternion.Euler(0,0,((int)(x+z)%3-1)*4f);
            for (int i=0;i<3;i++)
                Cuboid("Broken rock layer",x-width*.30f+i*width*.28f,y+height*.76f,z-depth*.42f,
                    width*.24f,.7f,depth*.32f,i%2==0?DarkStone:Stone);
        }

        private void SetBanner(float x,float z,float h,Color banner)
        {
            float y=H(x,z);
            Cuboid("Reconstruction flagstaff",x,y+h*.55f,z,.16f,h,.16f,Wood);
            Cuboid("Valoria blue-and-gold banner",x+.61f,y+h-.85f,z,1.12f,1.55f,.12f,banner);
            Cuboid("Heraldic sun crest",x+.61f,y+h-.65f,z-.07f,.24f,.50f,.10f,Gold);
        }

        private void RidgeTower(float x,float z,float height,float r,Color masonry,Color roof)
        {
            float y=H(x,z);
            Shape(PrimitiveType.Cylinder,"Ridge watchtower • masonry",new Vector3(x,y+height*.5f,z),
                new Vector3(r,height*.5f,r),masonry);
            Shape(PrimitiveType.Cylinder,"Ridge machicolation",new Vector3(x,y+height-.18f,z),
                new Vector3(r*1.17f,.33f,r*1.17f),Stone);
            Cone("Slate conical tower cap",new Vector3(x,y+height+.12f,z),r*1.36f,r*1.10f,roof);
            for(int k=0;k<4;k++)
            {
                float a=k*Mathf.PI*.5f;
                Cuboid("Vertical arrow embrasure",x+Mathf.Sin(a)*r,y+height*.64f,z+Mathf.Cos(a)*r,
                    .20f,1.8f,.22f,RGB(.15f,.16f,.19f));
            }
        }

        private void HighlandHall(float x,float z,float w,float length,int seed)
        {
            float y=H(x,z);
            Color wall=seed%2==0?RGB(.49f,.49f,.50f):RGB(.40f,.41f,.44f);
            Cuboid("Highland masonry civilian hall",x,y+3.8f,z,w,7.6f,length,wall);
            for (int side=-1;side<=1;side+=2)
            {
                var roof=Cuboid("Steep slate hall roofing",x+side*w*.26f,y+8.8f,z,
                    w*.58f,.52f,length+1f,Roof);
                roof.transform.rotation=Quaternion.Euler(0,0,side*47f);
                Cuboid("Buttressed hall stone",x+side*w*.44f,y+2.9f,z,
                    .7f,5.8f,length*.92f,DarkStone);
            }
            Cuboid("Recessed oak entrance",x,y+1.6f,z-length*.51f,1.25f,3.2f,.22f,Wood);
            for(int i=-1;i<=1;i+=2)
                Cuboid("Warm amber slit lantern",x+i*w*.30f,y+4.9f,z-length*.52f,.50f,1.05f,.12f,Gold);
        }

        private void BuildAshenHighlands()
        {
            // An asymmetric defensive capital perched on a cold mountain ridge.
            // No river-axis plan: zig-zag ascent, stronghold wall, stepped military district.
            Color cliff=RGB(.36f,.38f,.41f);
            Color masonry=RGB(.45f,.47f,.51f);
            Color trim=RGB(.66f,.64f,.61f);
            Color slate=RGB(.19f,.26f,.33f);
            Color path=RGB(.57f,.55f,.54f);
            for(int i=0;i<10;i++)
            {
                float x=(i<5?-47f:36f)+(i%5)*3.4f;
                float z=4f+(i%5)*7.1f;
                CarvedCliff(x,z,5f+(i%3)*2f,7f+(i%4)*2f,7f,cliff);
            }
            Walkway("Serpentine military ascent",new Vector3(34,0,-39),new Vector3(16,0,-15),4.1f,path);
            Walkway("Mountain switchback lower",new Vector3(16,0,-15),new Vector3(-15,0,5),4.2f,path);
            Walkway("Mountain switchback upper",new Vector3(-15,0,5),new Vector3(-6,0,29),4.8f,path);
            // Staggered curtain walls with a large open entry facing the southern approach.
            Curtain("Heavy east defensive curtain",new Vector2(26,-5),new Vector2(28,19),9f,2.7f,masonry);
            Curtain("Heavy upper defensive curtain",new Vector2(28,19),new Vector2(20,42),10f,2.8f,masonry);
            Curtain("North fortress rampart",new Vector2(20,42),new Vector2(-22,42),11f,2.8f,masonry);
            Curtain("West high rampart",new Vector2(-22,42),new Vector2(-27,18),9f,2.8f,masonry);
            RidgeTower(27f,-5f,17f,3.3f,masonry,slate);
            RidgeTower(28f,19f,19f,3.7f,masonry,slate);
            RidgeTower(20f,42f,22f,3.2f,masonry,slate);
            RidgeTower(-22f,42f,20f,3.2f,masonry,slate);
            RidgeTower(-27f,18f,16f,3.0f,masonry,slate);
            float gy=H(13f,-5f);
            bool sculptedGate=Vendor("ValoriaStoneV2/Valoria_MainGate_StoneDefV2.glb",13,-5,13f,14.2f);
            if (!sculptedGate)
            {
                Cuboid("MOUNTAIN GATEHOUSE",13f,gy+7.2f,-5f,13.2f,14.4f,7.2f,masonry);
                Cuboid("Heavy slate gatehouse roof",13f,gy+14.6f,-5f,14f,1.25f,8f,slate);
                Cuboid("Mountain gate opening",13f,gy+3f,-8.7f,5f,6f,.38f,Wood);
                for(int i=-2;i<=2;i++)
                    Cuboid("Iron gate bar",13f+i*.87f,gy+3f,-8.95f,.19f,5.8f,.21f,DarkStone);
            }
            for(int side=-1;side<=1;side+=2)
                Cuboid("Gate entrance buttress",13f+side*5.5f,gy+6f,-9.0f,1.6f,12f,2.2f,trim);
            SetBanner(8,-8,13f,Blue);
            // Dominant tall keep, timber halls, ground-level courtyard and stepped foundations.
            float kx=-5f,kz=30f,ky=H(kx,kz);
            Cuboid("BASTION I granite plinth",kx,ky+1.15f,kz,22f,2.3f,19f,cliff);
            bool hero=Vendor("ValoriaHero/Valoria_HeroBastion_v1.glb",kx,kz,20f,24f);
            if (!hero)
            {
                Cuboid("Bastion main hall",kx,ky+10.5f,kz,15.5f,17.2f,13f,masonry);
                Cuboid("Inner high stone cornice",kx,ky+19.7f,kz,16.4f,1.2f,14.1f,trim);
                Cuboid("High keep secondary tower",kx-3.2f,ky+23f,kz+2f,8f,8f,8f,masonry);
                Cuboid("Keep dark ridgeline",kx-3.2f,ky+27.9f,kz+2f,10f,1.15f,9.9f,slate);
            }
            else
            {
                var marker=new GameObject("Bastion main hall");
                marker.transform.SetParent(built,false);
                marker.transform.position=new Vector3(kx,ky+10,kz);
            }
            if (!hero) for(int i=0;i<5;i++)
            {
                Cuboid("Heavy stone buttress",kx-7f+i*3.5f,ky+7.9f,kz-6.92f,.95f,14.8f,1.5f,trim);
                if(i>0&&i<4)
                    Cuboid("Keep narrow murder hole",kx-7f+i*3.5f,ky+12f,kz-7.77f,.36f,2.35f,.21f,RGB(.10f,.12f,.16f));
            }
            for(int i=-1;i<=1;i+=2)
            {
                RidgeTower(kx+i*10.2f,kz-5f,20f,3.1f,masonry,slate);
                RidgeTower(kx+i*10.2f,kz+7.5f,16f,2.8f,masonry,slate);
            }
            Cuboid("Bastion oak gate",kx,ky+3.6f,kz-7.17f,3.2f,7f,.40f,Wood);
            Cuboid("Bastion blue crest",kx,ky+11.0f,kz-7.26f,3.2f,3.0f,.18f,Blue);
            SetBanner(-12f,34f,16f,Blue);
            // Two civilian production functions retained as physical scene locations.
            if (!Vendor("EldoriaProduction/Valoria_Workshop_v1.glb",28,3,8,7))
                HighlandHall(28,3,7f,8f,0);
            if (!Vendor("EldoriaProduction/Valoria_CivicHouse_v1.glb",-18,-3,8,7))
                HighlandHall(-18,-3,7.4f,7f,1);
            HighlandHall(20f,26f,8f,8f,3);
            HighlandHall(-29f,3f,7.5f,7f,4);
            HighlandHall(-13f,14f,6.8f,7f,5);
            for(int i=0;i<8;i++)
            {
                float x=-12f+i*2.1f,z=18f;
                Cuboid("Rebuilding stockpile",x,H(x,z)+.55f,z,1.6f,1.1f,1.4f,i%3==0?Wood:DarkStone);
            }
        }

        private void SunlitTower(float x,float z,float height,float width)
        {
            float y=H(x,z);
            bool ok=Vendor("Kenney/castle/tower-square-base.glb",x,z,width,height*.38f);
            ok=Vendor("Kenney/castle/tower-square-mid.glb",x,z,width,height*.42f,height*.30f)||ok;
            ok=Vendor("Kenney/castle/tower-square-top-roof.glb",x,z,width*1.22f,height*.30f,height*.75f)||ok;
            if(!ok)
            {
                Cuboid("Colored tower base",x,y+height*.45f,z,width,height*.9f,width,Stone);
                Cone("Blue roof cap",new Vector3(x,y+height*.93f,z),width*.88f,height*.35f,Roof);
            }
        }

        private void BuildSunlitRingTown()
        {
            // Luminous hill town with a circular plaza and compact blue-roof Bastion,
            // instead of an axial walled medieval fortress / dark ridge.
            Color pale=RGB(.89f,.76f,.53f), blue=RGB(.21f,.54f,.86f);
            Color lawn=RGB(.31f,.62f,.39f), lake=RGB(.21f,.66f,.79f);
            float px=15f,pz=-2f,py=H(px,pz);
            Shape(PrimitiveType.Cylinder,"Civic plaza ochre paved circle",
                new Vector3(px,py+.12f,pz),new Vector3(15,.16f,15),pale);
            Shape(PrimitiveType.Cylinder,"Civic plaza outer border",
                new Vector3(px,py+.16f,pz),new Vector3(12.5f,.14f,12.5f),Stone);
            Shape(PrimitiveType.Cylinder,"Civic plaza cobbled center",
                new Vector3(px,py+.24f,pz),new Vector3(10.1f,.09f,10.1f),Trim);
            if(!Vendor("Kenney/fantasy-town/fountain-square.glb",px,pz,5.1f,4.2f))
            {
                Shape(PrimitiveType.Cylinder,"Ornamental central fountain",new Vector3(px,py+.7f,pz),
                     new Vector3(2.7f,.62f,2.7f),blue);
            }
            // Oval lagoon and sand quays are deliberately WEST, not a crossing along a central road.
            float wx=-31f,wz=-23f,waterY=H(wx,wz);
            Shape(PrimitiveType.Cylinder,"Golden lake beach",new Vector3(wx,waterY+.11f,wz),
                new Vector3(19f,.10f,15f),pale);
            Shape(PrimitiveType.Cylinder,"Turquoise mountain lagoon",new Vector3(wx,waterY+.20f,wz),
                new Vector3(16.8f,.095f,12.9f),lake);
            Shape(PrimitiveType.Cylinder,"Lagoon shallow rim",new Vector3(-17f,H(-17,-21)+.20f,-21),
                new Vector3(5.0f,.1f,6f),RGB(.40f,.77f,.75f));
            // Beautiful radial trade town with garden blocks and no monumental southern barbican.
            for(int i=0;i<9;i++)
            {
                float angle=(i*40f+20f)*Mathf.Deg2Rad;
                float x=px+Mathf.Cos(angle)*24.5f,z=pz+Mathf.Sin(angle)*24.5f;
                if(x< -10 && z>9)continue;
                House(x,z,i%2==0?4.2f:5.1f,i+1,"COLORFUL LIVELY TOWN HOUSE");
                if(i%2==0)SetBanner(x+4,z,3.8f,blue);
            }
            Walkway("Garden road from market",new Vector3(14,0,-32),new Vector3(px,0,-11),4.8f,pale);
            Walkway("Garden road toward Bastion",new Vector3(px-11,0,pz+6),new Vector3(-21,0,20),4.5f,pale);
            for(int i=0;i<11;i++)
            {
                float a=i*2*Mathf.PI/11f;
                float x=px+Mathf.Cos(a)*11.3f,z=pz+Mathf.Sin(a)*11.3f;
                Shape(PrimitiveType.Cube,"Civic flower and hedgerow",new Vector3(x,H(x,z)+.54f,z),
                    new Vector3(1.3f,1.05f,1.3f),i%2==0?Green:lawn);
            }
            // A compact, bright hill Bastion assembled from the genuine Kenney modular family.
            float bx=-23f,bz=23f,by=H(bx,bz);
            Shape(PrimitiveType.Cylinder,"BASTION I circular natural hilltop terrace",
                new Vector3(bx,by+1f,bz),new Vector3(13.8f,1.25f,13.8f),pale);
            Shape(PrimitiveType.Cylinder,"Bastion I inner landscaped terrace",
                new Vector3(bx,by+2.18f,bz),new Vector3(10.8f,.10f,10.8f),Grass);
            for(int j=0;j<7;j++)
            {
                float a=j*(Mathf.PI*2f/7f);
                float x=bx+Mathf.Cos(a)*11.6f,z=bz+Mathf.Sin(a)*11.6f;
                SunlitTower(x,z,10f,3.9f);
                int k=(j+1)%7;
                float na=k*Mathf.PI*2f/7f;
                Vector2 from=new Vector2(x,z),to=new Vector2(bx+Mathf.Cos(na)*11.6f,bz+Mathf.Sin(na)*11.6f);
                if(j!=4)Curtain("Low bright garden curtain",from,to,3.7f,1.1f,Stone);
            }
            if(!Vendor("Kenney/castle/tower-square.glb",bx,bz,9.5f,16.3f))
                Cuboid("Bright compact keep",bx,by+8.2f,bz,9f,15f,9f,Stone);
            // Marker retained for native capture contract.
            var marker=new GameObject("Bastion main hall");
            marker.transform.SetParent(built,false);
            marker.transform.position=new Vector3(bx,by+8,bz);
            SetBanner(bx-5,bz+1,9f,Blue);
            if(!Vendor("Kenney/castle/gate.glb",bx-2,bz-11,5.7f,6.4f))
                Cuboid("Low bright Bastion gate",bx-2,by+3,bz-12,5f,6f,1.1f,Wood);
            Walkway("Bastion hill footpath",new Vector3(-21,0,12),new Vector3(-21,0,20),4f,pale,18);
            // Civil development reuses the known barracks/lumber/logistic roles.
            House(34,17,6.2f,2,"CUARTEL ROLE: BLUE ROOFS");
            House(38,-16,6.5f,1,"ASERRADERO ROLE: WARM TIMBER");
            Vendor("Kenney/fantasy-town/cart.glb",29f,-12f,4f,2.8f);
            Vendor("Kenney/castle/bridge-straight.glb",-9f,-25f,5f,3f);
            for(int i=0;i<8;i++)
            {
                float x=36f+(i%4)*1.6f,z=15f+(i/4)*2f;
                Cuboid("Sawmill stacked cut timber",x,H(x,z)+.25f,z,1.3f,.48f,2.7f,Wood);
            }
        }

        private void EnrichStrategyValley()
        {
            // Stable Quaternius RTS is a growing valley settlement, visually different
            // from cold mountain barracks and compact circular Kenney town.
            Vendor("QuaterniusRTS/Farm_SecondAge_Level3.gltf",-33,-11,11f,7f);
            Vendor("QuaterniusRTS/Houses_SecondAge_2_Level3.gltf",-21,29,9f,8f);
            Vendor("QuaterniusRTS/Market_FirstAge_Level3.gltf",23,-18,9f,7f);
            Vendor("QuaterniusRTS/TownCenter_FirstAge_Level3.gltf",0,33,12f,11f);
            for(int i=0;i<8;i++)
            {
                float x=-40f+i*1.7f,z=-18f;
                Cuboid("Terraced farm plot",x,H(x,z)+.12f,z,1.38f,.17f,8.2f,
                    (i%2==0)?RGB(.65f,.59f,.29f):RGB(.74f,.66f,.35f));
            }
            for(int i=0;i<5;i++)
            {
                float x=-40+i*4.1f,z=3f;
                Shape(PrimitiveType.Sphere,"Apple orchard foliage",new Vector3(x,H(x,z)+2.1f,z),
                    new Vector3(2.9f,3.7f,2.9f),RGB(.28f,.52f,.31f));
            }
        }

        private void OnGUI()
        {
            if (!UnityEngine.Application.isPlaying || !showTrialTitle) return;
            var name=artDirection==ArtDirection.FantasyStrategy?"01 / FANTASIA 3D ESTRATEGIA":
                     artDirection==ArtDirection.DetailedMedieval?"02 / MEDIEVAL ESTILIZADO DETALLADO":
                     "03 / MEDIEVAL COLORIDO";
            GUI.Box(new Rect(12,12,420,58),
                "ELDORIA — VALORIA • " + name + "\n" +
                "PRUEBA ARTISTICA / NO GAMEPLAY CERTIFICADO");
        }
    }
}
