#if UNITY_EDITOR
using System;
using Eldoria.Experiments.Nocturne;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Eldoria.Experiments.Nocturne.Editor
{
    /// <summary>
    /// Deterministic editor-time assembly. The shipping scene holds real imported
    /// Blender meshes, physical UI sprites and serialized callbacks. This is not
    /// a hidden procedural code-built world at runtime.
    /// </summary>
    public static class NocturneSceneBuilder
    {
        private const string Root = "Assets/Eldoria/Experiments/NocturneRebuild";
        private const string ScenePath = Root + "/Scenes/Nocturne_Experiment.unity";
        private const string Models = Root + "/Models/";
        private const string Ui = Root + "/UI/";
        private static Font font;
        private static Sprite panel, goldButton, softButton, dock;
        private static readonly Color Ivory = H("#E4D9C3");
        private static readonly Color TextMuted = H("#ABA99F");
        private static readonly Color Gold = H("#DBB477");
        private static readonly Color CanvasDark = H("#181C22");
        private static Color H(string s)
        {
            if (!ColorUtility.TryParseHtmlString(s, out var c)) throw new Exception(s);
            return c;
        }

        [MenuItem("Eldoria/Experiments/NOCTURNE/Build independent scene")]
        public static void Build()
        {
            if (EditorApplication.isPlaying)
                throw new InvalidOperationException("Stop Play Mode before authoring Nocturne.");
            ValidatePhysicalAssets();
            EnsureFolder(Root + "/Scenes");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            // This is the experiment's own scene; no production scene is edited.
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            // Scene path determines the persisted scene name.

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = H("#74818C");
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = H("#4B5360");
            RenderSettings.fogStartDistance = 37f;
            RenderSettings.fogEndDistance = 86f;

            var sceneCamera = new GameObject("Player • isometric gameplay camera");
            var cam=sceneCamera.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 14f;
            cam.nearClipPlane = .1f;
            cam.farClipPlane = 105;
            cam.backgroundColor=H("#424B57");
            cam.clearFlags=CameraClearFlags.SolidColor;
            sceneCamera.tag="MainCamera";
            cam.transform.position=new Vector3(17.5f,19.8f,22.5f);
            cam.transform.LookAt(new Vector3(0,2.05f,0));
            cam.allowHDR=false;
            cam.allowMSAA=true;

            var keyObj=new GameObject("Warm dusk • one directional light");
            var light=keyObj.AddComponent<Light>();
            light.type=LightType.Directional;
            light.color=H("#F8DDBA");
            light.intensity=1.1f;
            light.shadows=LightShadows.Soft;
            light.shadowStrength=.63f;
            light.shadowBias=.055f;
            light.transform.rotation=Quaternion.Euler(49,-34,0);
            var city=Group("NOCTURNE / authored city");
            var world=Group("NOCTURNE / authored frontier");
            WorldModel(city,"CityGround",Vector3.zero,"Environment • meadow + worked road");
            WorldModel(world,"WorldGround",Vector3.zero,"Environment • forest + geological breach");
            // Trees and rock clusters remain physical GLB prefabs, not generated
            // mesh primitives. Intentionally sparse for mobile graphics budgets.
            SceneKit(city,
                new Vector3(-12,0,-2),new Vector3(-11,0,-9),
                new Vector3(12,0,-3),new Vector3(12,0,-9),
                new Vector3(-4,0,-10),new Vector3(6,0,-10));
            SceneKit(world,
                new Vector3(-13,0,-6),new Vector3(-11,0,8),
                new Vector3(-6,0,10),new Vector3(13,0,8),
                new Vector3(15,0,-6),new Vector3(-1,0,-13));

            Transform bastionAnchor=Anchor(city,"Bastion / one active stage",Vector3.zero);
            var plotAnchor=Anchor(city,"Sawmill anchor",new Vector3(-8.5f,0,3.5f));
            var barracksAnchor=Anchor(city,"Barracks anchor",new Vector3(8.5f,0,3.5f));
            var granaryAnchor=Anchor(city,"Granary anchor",new Vector3(-8.8f,0,-5.2f));
            var stoneAnchor=Anchor(city,"Stoneworks anchor",new Vector3(8.9f,0,-5.7f));

            var runtimeObject = new GameObject("NOCTURNE / authentic I–V player state and presentation");
            var runtime=runtimeObject.AddComponent<NocturneRuntime>();
            runtime.PlayCamera=cam;
            runtime.CityRoot=city;
            runtime.WorldRoot=world;
            runtime.BastionAnchor=bastionAnchor;
            runtime.BuildingAnchors=new[] {plotAnchor,barracksAnchor,granaryAnchor,stoneAnchor};
            runtime.BastionModels=new GameObject[5];
            for (int i=0;i<5;i++)
                runtime.BastionModels[i]=Model("Bastion_"+(i+1).ToString("D2"));
            runtime.BuildingModels=new[]{
                Model("Sawmill"),Model("Barracks"),Model("Granary"),Model("Stoneworks")};
            runtime.UnbuiltPlot=Model("ConstructionPlot");

            var canvas=MakeCanvas(runtime);
            BuildHeader(runtime,canvas);
            BuildNavigation(runtime,canvas);
            BuildSelectionPanel(runtime,canvas);
            BuildBuildingTabs(runtime,canvas);
            BuildWorldChoices(runtime,canvas);
            BuildEventSystem();
            var responsive=canvas.gameObject.AddComponent<NocturneResponsiveLayout>();
            responsive.PlayCamera=cam;
            responsive.Scaler=canvas.GetComponent<CanvasScaler>();
            responsive.ContextPanel=runtime.ContextPanel.GetComponent<RectTransform>();
            responsive.WorldChoices=runtime.WorldChoicesPanel.GetComponent<RectTransform>();
            responsive.CityList=runtime.BuildingTabs[0].transform.parent as RectTransform;

            // Scenes are saved with their real models, material source refs,
            // serialized UI anchors, and gameplay runtime component.
            world.SetActive(false);
            bool success=EditorSceneManager.SaveScene(scene,ScenePath);
            if (!success || AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath)==null)
                throw new InvalidOperationException("NOCTURNE scene was not actually written");
            AssetDatabase.SaveAssets();
            Debug.Log("NOCTURNE_SCENE_AUTHORED " + ScenePath +
                " ; 5 imported Bastions ; 4 building models ; 3 ground/plot models ; 4 UI sprites." +
                " Gameplay: original LocalGateway and isolated save.");
        }

        private static void ValidatePhysicalAssets()
        {
            for(int i=1;i<=5;i++) Model("Bastion_"+i.ToString("D2"));
            foreach(var n in new[]{
                "Sawmill","Barracks","Granary","Stoneworks",
                "CityGround","WorldGround","ConstructionPlot","Pine","Rock"})
                Model(n);
            panel=Sprite("NocturnePanel");
            goldButton=Sprite("NocturnePrimary");
            softButton=Sprite("NocturneSecondary");
            dock=Sprite("NocturneDock");
            font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (!font)
                throw new InvalidOperationException("No built-in fallback font. Add a licensed font asset and update authoring.");
        }

        private static GameObject Model(string name)
        {
            string file=Models+name+".glb";
            AssetDatabase.ImportAsset(file,ImportAssetOptions.ForceSynchronousImport);
            GameObject prefab=AssetDatabase.LoadAssetAtPath<GameObject>(file);
            if (!prefab)
                throw new InvalidOperationException("GLB NOT IMPORTED AS PREFAB. Check glTFast version: "+file);
            return prefab;
        }
        private static Sprite Sprite(string name)
        {
            string path=Ui+name+".png";
            var textureImporter=AssetImporter.GetAtPath(path) as TextureImporter;
            if(textureImporter==null)
            {
                AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                textureImporter=AssetImporter.GetAtPath(path) as TextureImporter;
            }
            if(textureImporter==null)
                throw new InvalidOperationException("Authored UI PNG missing: "+path);
            if(textureImporter.textureType!=TextureImporterType.Sprite ||
               textureImporter.spriteBorder!=new Vector4(22,22,22,22))
            {
                textureImporter.textureType=TextureImporterType.Sprite;
                textureImporter.spriteImportMode=SpriteImportMode.Single;
                textureImporter.spriteBorder=new Vector4(22,22,22,22);
                textureImporter.mipmapEnabled=false;
                textureImporter.alphaIsTransparency=true;
                textureImporter.textureCompression=TextureImporterCompression.Compressed;
                textureImporter.SaveAndReimport();
            }
            var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if(!sprite) throw new InvalidOperationException("UI PNG could not become Sprite: "+path);
            return sprite;
        }
        private static GameObject Group(string name)
        {
            var g=new GameObject(name);
            return g;
        }
        private static Transform Anchor(GameObject parent,string name,Vector3 pos)
        {
            var go=new GameObject(name);
            go.transform.SetParent(parent.transform,false);
            go.transform.localPosition=pos;
            return go.transform;
        }
        private static GameObject WorldModel(GameObject parent,string model,Vector3 loc,string name)
        {
            GameObject prefab=Model(model);
            var go=PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if(!go) throw new InvalidOperationException("Could not instantiate GLB "+model);
            go.name=name;
            go.transform.SetParent(parent.transform,false);
            go.transform.localPosition=loc;
            go.transform.localRotation=Quaternion.identity;
            return go;
        }
        private static void SceneKit(GameObject root, params Vector3[] pines)
        {
            for(int i=0;i<pines.Length;i++)
            {
                var tree=WorldModel(root,"Pine",pines[i],"Pine / "+i.ToString("D2"));
                tree.transform.localScale=Vector3.one*(.87f+.11f*(i%4));
                tree.transform.localRotation=Quaternion.Euler(0,54*i,0);
            }
            for(int i=0;i<4;i++)
            {
                Vector3 pos=new Vector3(i%2==0 ? -9.5f : 10.6f,0,i<2 ? -8.7f : 8.8f);
                var rock=WorldModel(root,"Rock",pos,"Rock group / "+i);
                rock.transform.localScale=Vector3.one*(.8f+.11f*i);
            }
        }
        private static Canvas MakeCanvas(NocturneRuntime r)
        {
            var go=new GameObject("NOCTURNE / authored graphical canvas",
                typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            var canvas=go.GetComponent<Canvas>();
            canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder=100;
            var scaler=go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(390,844);
            scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight=0;
            return canvas;
        }
        private static RectTransform Rect(GameObject parent,string name,
            Vector2 anchorMin, Vector2 anchorMax,
            Vector2 pivot,Vector2 anchoredPos,Vector2 size)
        {
            var go=new GameObject(name,typeof(RectTransform));
            var r=go.GetComponent<RectTransform>();
            r.SetParent(parent.transform,false);
            r.anchorMin=anchorMin;r.anchorMax=anchorMax;r.pivot=pivot;
            r.anchoredPosition=anchoredPos;r.sizeDelta=size;
            return r;
        }
        private static Image Artwork(RectTransform r,Sprite texture,Color tint)
        {
            var image=r.gameObject.AddComponent<Image>();
            image.sprite=texture;
            image.type=Image.Type.Sliced;
            image.color=tint;
            return image;
        }
        private static Text Label(RectTransform parent,string name,string caption,
            int size,TextAnchor alignment,Color color,
            Vector2 min,Vector2 max,Vector2 pos,Vector2 delta)
        {
            var r=Rect(parent.gameObject,name,min,max,new Vector2(.5f,.5f),pos,delta);
            var t=r.gameObject.AddComponent<Text>();
            t.font=font;t.fontSize=size;t.color=color;
            t.text=caption;t.alignment=alignment;
            t.horizontalOverflow=HorizontalWrapMode.Wrap;
            t.verticalOverflow=VerticalWrapMode.Truncate;
            t.supportRichText=false;
            t.raycastTarget=false;
            return t;
        }
        private static Button Button(RectTransform parent,string name,string label,
            Sprite tex,Vector2 aMin,Vector2 aMax,Vector2 pos,Vector2 delta,
            out Text text, int textSize=14)
        {
            var r=Rect(parent.gameObject,name,aMin,aMax,new Vector2(.5f,.5f),pos,delta);
            var image=Artwork(r,tex,Color.white);
            var b=r.gameObject.AddComponent<Button>();
            b.targetGraphic=image;
            var c=b.colors;c.normalColor=Color.white;c.highlightedColor=H("#FFF3D9");
            c.pressedColor=H("#C4A16A");c.disabledColor=H("#55545A");b.colors=c;
            text=Label(r,"Inscription",label,textSize,TextAnchor.MiddleCenter,Ivory,
                Vector2.zero,Vector2.one,Vector2.zero,new Vector2(-14,-10));
            return b;
        }
        private static void BuildHeader(NocturneRuntime r,Canvas canvas)
        {
            RectTransform baseRect=Rect(canvas.gameObject,"TOP / embossed slate resource rail",
                new Vector2(0,1),new Vector2(1,1),new Vector2(.5f,1),Vector2.zero,
                new Vector2(0,110));
            Artwork(baseRect,dock,Color.white);
            r.RealmTitle=Label(baseRect,"Emblem · region & Bastion","VALORIA  ·  BASTIÓN I",19,
                TextAnchor.MiddleLeft,Gold,new Vector2(0,1),new Vector2(1,1),
                new Vector2(0,-28),new Vector2(-36,36));
            r.Resources=Label(baseRect,"Resource ledgers","MADERA 230    PIEDRA 150    COMIDA 0",14,
                TextAnchor.MiddleCenter,Ivory,new Vector2(0,1),new Vector2(1,1),
                new Vector2(0,-65),new Vector2(-24,36));
            r.Chapter=Label(baseRect,"Campaign progress","CAMPAÑA I–V   ·   PODER 0",12,
                TextAnchor.MiddleCenter,TextMuted,new Vector2(0,1),new Vector2(1,1),
                new Vector2(0,-94),new Vector2(-24,23));

            RectTransform msg=Rect(canvas.gameObject,"Message / outcome ribbon",
                new Vector2(0,1),new Vector2(1,1),new Vector2(.5f,1),
                new Vector2(0,-114),new Vector2(-24,45));
            Artwork(msg,softButton,Color.white);
            r.Feedback=Label(msg,"Contextual narrative feedback","Selecciona un edificio",12,
                TextAnchor.MiddleCenter,Ivory,Vector2.zero,Vector2.one,Vector2.zero,
                new Vector2(-15,-10));
        }
        private static void BuildNavigation(NocturneRuntime r,Canvas canvas)
        {
            RectTransform baseRect=Rect(canvas.gameObject,"BOTTOM / navigation metal rail",
                new Vector2(0,0),new Vector2(1,0),new Vector2(.5f,0),
                Vector2.zero,new Vector2(0,97));
            Artwork(baseRect,dock,Color.white);
            string[] labels={ "CIUDAD", "FRONTERA", "HÉROES", "BUZÓN" };
            Action[] callbacks={r.ShowCity,r.ShowWorld,r.ShowHeroes,r.ShowMailbox};
            for(int i=0;i<4;i++)
            {
                var button=Button(baseRect,"Medallion / "+labels[i],labels[i],
                    i==0?goldButton:softButton,
                    new Vector2(i/4f,0.5f),new Vector2((i+1)/4f,.5f),
                    new Vector2(0,-1),new Vector2(-6,58),out Text caption,12);
                button.onClick.AddListener(()=>callbacks[GetIndex(button,baseRect)]());
                if(i==0)r.CityNavText=caption;
                else if(i==1)r.WorldNavText=caption;
            }
        }
        private static int GetIndex(Button child,RectTransform baseRect)
        {
            // Button's stable sibling order is authored once in scene.
            return Mathf.Clamp(child.transform.GetSiblingIndex(),0,3);
        }
        private static void BuildSelectionPanel(NocturneRuntime r,Canvas canvas)
        {
            RectTransform holder=Rect(canvas.gameObject,"CITY / construction contextual dossier",
                new Vector2(0,0),new Vector2(1,0),new Vector2(.5f,0),
                new Vector2(0,111),new Vector2(-18,184));
            Artwork(holder,panel,Color.white);
            r.ContextPanel=holder.gameObject;r.ContextPanelTransform=holder;
            r.SelectionTitle=Label(holder,"Selected building header","BASTIÓN I",17,
                TextAnchor.MiddleLeft,Gold,new Vector2(0,1),new Vector2(1,1),
                new Vector2(0,-30),new Vector2(-45,38));
            r.SelectionBody=Label(holder,"Authentic objectives","OBJETIVOS PENDIENTES",12,
                TextAnchor.UpperLeft,Ivory,new Vector2(0,1),new Vector2(1,1),
                new Vector2(0,-76),new Vector2(-44,62));
            r.PrimaryButton=Button(holder,"PRIMARY / real gateway command","MEJORAR BASTIÓN",
                goldButton,new Vector2(.25f,0),new Vector2(.25f,0),
                new Vector2(0,30),new Vector2(148,45),out var primary,13);
            r.PrimaryText=primary;
            r.SecondaryButton=Button(holder,"SECONDARY / real gateway command","PREPARAR MARCHA",
                softButton,new Vector2(.75f,0),new Vector2(.75f,0),
                new Vector2(0,30),new Vector2(148,45),out var secondary,12);
            r.SecondaryText=secondary;
        }
        private static void BuildBuildingTabs(NocturneRuntime r,Canvas canvas)
        {
            RectTransform rail=Rect(canvas.gameObject,"CITY / five building tabs",
                new Vector2(0,1),new Vector2(0,1),new Vector2(0,1),
                new Vector2(10,-183),new Vector2(139,238));
            Artwork(rail,panel,Color.white);
            string[] labels={ "BASTIÓN","ASERRADERO","CUARTEL","GRANERO","CANTERA" };
            for(int i=0;i<5;i++)
            {
                var b=Button(rail,"Select "+labels[i],labels[i],i==0?goldButton:softButton,
                    new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(0,-31-i*42),
                    new Vector2(119,37),out Text _,11);
                r.BuildingTabs[i]=b;
            }
        }
        private static void BuildWorldChoices(NocturneRuntime r,Canvas canvas)
        {
            RectTransform holder=Rect(canvas.gameObject,"FRONTIER / eight genuine destinations",
                new Vector2(0,0),new Vector2(1,0),new Vector2(.5f,0),
                new Vector2(0,111),new Vector2(-18,232));
            Artwork(holder,panel,Color.white);
            r.WorldChoicesPanel=holder.gameObject;
            var title=Label(holder,"Frontier header","DESPLIEGUE · FRONTERA DE VALORIA",
                14,TextAnchor.MiddleCenter,Gold,
                new Vector2(0,1),new Vector2(1,1),new Vector2(0,-26),
                new Vector2(-32,37));
            for(int i=0;i<8;i++)
            {
                int row=i/2,col=i%2;
                float x=col==0?.25f:.75f;
                float top=69+row*42;
                r.TargetButtons[i]=Button(holder,"Travel command "+i,"",
                    softButton,new Vector2(x,1),new Vector2(x,1),
                    new Vector2(0,-top),new Vector2(150,36),out Text _,12);
            }
        }
        private static void BuildEventSystem()
        {
            var ev=new GameObject("EventSystem • new Input System");
            ev.AddComponent<EventSystem>();
            ev.AddComponent<InputSystemUIInputModule>();
        }
        private static void EnsureFolder(string path)
        {
            if(AssetDatabase.IsValidFolder(path))return;
            var tokens=path.Split('/');
            for(int i=1;i<tokens.Length;i++)
            {
                string parent=String.Join("/",tokens,0,i);
                string dest=parent+"/"+tokens[i];
                if(!AssetDatabase.IsValidFolder(dest))
                    AssetDatabase.CreateFolder(parent,tokens[i]);
            }
        }
    }


}
#endif
