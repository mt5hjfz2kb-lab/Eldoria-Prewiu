using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Eldoria.Presentation
{
    /// <summary>
    /// Runtime implementation of the owner-approved Eldoria HUD reference.
    /// Functional controls remain owned by SlicePresenter; this component reshapes and
    /// decorates them into the reference composition and adds visual-only reference chrome.
    /// </summary>
    public sealed class ReferenceUiArtPass : MonoBehaviour
    {
        static readonly Color Panel = new Color(.025f,.035f,.043f,.91f);
        static readonly Color PanelDeep = new Color(.012f,.018f,.023f,.97f);
        static readonly Color Bronze = new Color(.53f,.38f,.19f,1f);
        static readonly Color BronzeDark = new Color(.18f,.11f,.05f,1f);
        static readonly Color Gold = new Color(.82f,.65f,.31f,1f);
        static readonly Color GoldSoft = new Color(.98f,.85f,.58f,1f);
        static readonly Color Ink = new Color(.055f,.050f,.038f,1f);
        static readonly Color Disabled = new Color(.17f,.18f,.19f,.94f);
        static Sprite portraitSprite;

        readonly Dictionary<string, GameObject> labels = new Dictionary<string, GameObject>();
        RectTransform safe;
        Camera sceneCamera;
        bool lastLandscape;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            SceneManager.sceneLoaded -= OnLoaded;
            SceneManager.sceneLoaded += OnLoaded;
            Attach();
        }

        static void OnLoaded(Scene scene, LoadSceneMode mode) => Attach();

        static void Attach()
        {
            if (Object.FindFirstObjectByType<ReferenceUiArtPass>() != null) return;
            var go = new GameObject("Reference UI art pass", typeof(ReferenceUiArtPass));
            DontDestroyOnLoad(go);
        }

        IEnumerator Start()
        {
            while (true)
            {
                yield return null;
                DecorateCurrentHud();
                yield return new WaitForSecondsRealtime(.20f);
            }
        }

        void LateUpdate()
        {
            var canvas = GameObject.Find("Eldoria HUD");
            if (canvas == null) return;
            safe = canvas.transform.Find("Safe area") as RectTransform;
            if (safe == null) return;
            sceneCamera = GameObject.Find("Isometric camera")?.GetComponent<Camera>() ?? Camera.main;
            UpdateWorldLabels();
        }

        void DecorateCurrentHud()
        {
            var canvas = GameObject.Find("Eldoria HUD");
            if (canvas == null) return;
            safe = canvas.transform.Find("Safe area") as RectTransform;
            if (safe == null) return;

            Frame(GameObject.Find("Reference topbar"), false, 2f);
            Frame(GameObject.Find("Quest panel"), true, 2f);
            Frame(GameObject.Find("Bottom navigation"), false, 2f);
            Frame(GameObject.Find("Building interaction panel"), true, 3f);

            var buttons = canvas.GetComponentsInChildren<Button>(true);
            foreach (var button in buttons) StyleButton(button);

            var texts = canvas.GetComponentsInChildren<Text>(true);
            foreach (var text in texts) StyleText(text);

            EnsureReferenceChrome();
            ApplyReferenceLayout();
        }

        void EnsureReferenceChrome()
        {
            if (safe == null) return;
            if (safe.Find("Reference portrait") == null) CreatePortrait();
            if (safe.Find("Reference VIP") == null) CreateVip();
            if (safe.Find("Reference quest subtitle") == null) CreateQuestSubtitle();
            if (safe.Find("Reference left actions") == null) CreateLeftActions();
            if (safe.Find("Reference chat") == null) CreateChat();
            if (safe.Find("Reference top menu") == null) CreateTopMenu();
            if (safe.Find("Reference extra nav") == null) CreateExtraNav();
            EnsureWorldLabel("Bastion · target","Bastión","⬡");
            EnsureWorldLabel("Aserradero · target","Aserradero","⚒");
            EnsureWorldLabel("Cuartel · target","Cuartel","⚔");
            EnsureWorldLabel("Granero · target","Granero","✥");
        }

        void CreatePortrait()
        {
            var go = new GameObject("Reference portrait", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(safe,false);
            var rt=go.GetComponent<RectTransform>();
            rt.anchorMin=rt.anchorMax=new Vector2(0,1);rt.pivot=new Vector2(0,1);rt.sizeDelta=new Vector2(78,78);
            var img=go.GetComponent<Image>();img.color=Color.white;img.raycastTarget=false;
            if (portraitSprite == null)
            {
                var tex=Resources.Load<Texture2D>("UI/aldric_reference_portrait");
                if(tex!=null)portraitSprite=Sprite.Create(tex,new Rect(0,0,tex.width,tex.height),new Vector2(.5f,.5f),100f);
            }
            img.sprite=portraitSprite;
            var outline=go.AddComponent<Outline>();outline.effectColor=Gold;outline.effectDistance=new Vector2(2,-2);
            Badge(go.transform,"12",new Vector2(.05f,.05f),new Vector2(19,19),new Vector2(8,7),new Color(.04f,.14f,.24f,.98f));
            Badge(go.transform,"12",new Vector2(.32f,.05f),new Vector2(19,19),new Vector2(7,7),new Color(.17f,.11f,.035f,.98f));
        }

        void CreateVip()
        {
            var go=PanelObject("Reference VIP",safe,new Vector2(92,26));
            var rt=go.GetComponent<RectTransform>();rt.anchorMin=rt.anchorMax=new Vector2(0,1);rt.pivot=new Vector2(0,1);
            var text=MakeText("VIP text",go.transform,11,GoldSoft,TextAnchor.MiddleCenter);
            text.text="♜  VIP 2   ▲";Stretch(text.rectTransform,5f);
            Frame(go,false,1.5f);
        }

        void CreateQuestSubtitle()
        {
            var quest=GameObject.Find("Quest panel"); if(quest==null)return;
            var text=MakeText("Reference quest subtitle",quest.transform,11,Color.white,TextAnchor.MiddleLeft);
            text.text="Más allá de las Murallas";
            var rt=text.rectTransform;rt.anchorMin=rt.anchorMax=new Vector2(0,1);rt.pivot=new Vector2(0,1);
            rt.anchoredPosition=new Vector2(13,-42);rt.sizeDelta=new Vector2(274,26);
            var line=MakeText("Reference quest checks",quest.transform,9,new Color(.89f,.90f,.89f,1),TextAnchor.UpperLeft);
            line.text="□  Sigue el objetivo actual\n✓  Reconstruye Valoria\n□  Asegura la frontera";
            var lr=line.rectTransform;lr.anchorMin=lr.anchorMax=new Vector2(0,1);lr.pivot=new Vector2(0,1);
            lr.anchoredPosition=new Vector2(13,-69);lr.sizeDelta=new Vector2(274,70);
        }

        void CreateLeftActions()
        {
            var root=new GameObject("Reference left actions",typeof(RectTransform));root.transform.SetParent(safe,false);
            var rr=root.GetComponent<RectTransform>();rr.anchorMin=rr.anchorMax=new Vector2(0,1);rr.pivot=new Vector2(0,1);rr.sizeDelta=new Vector2(68,210);
            ActionMedallion(root.transform,"Construcción","⚒","0/2",0);
            ActionMedallion(root.transform,"Investigación","⚗","0/1",1);
            ActionMedallion(root.transform,"Población","♟","0/1",2);
        }

        void CreateChat()
        {
            var go=PanelObject("Reference chat",safe,new Vector2(360,72));
            var rt=go.GetComponent<RectTransform>();rt.anchorMin=rt.anchorMax=new Vector2(0,0);rt.pivot=new Vector2(0,0);
            var text=MakeText("Chat copy",go.transform,9,new Color(.95f,.95f,.93f,1),TextAnchor.MiddleLeft);
            text.text="●   [VAL] Nareth: ¡Bienvenidos!\n     [VAL] Lyra: Listos para la marcha\n     [VAL] Aldric: El reino se levanta";
            Stretch(text.rectTransform,9f);
            go.GetComponent<Image>().color=new Color(.015f,.020f,.024f,.72f);
        }

        void CreateTopMenu()
        {
            var root=new GameObject("Reference top menu",typeof(RectTransform));root.transform.SetParent(safe,false);
            var rt=root.GetComponent<RectTransform>();rt.anchorMin=rt.anchorMax=new Vector2(1,1);rt.pivot=new Vector2(1,1);rt.sizeDelta=new Vector2(150,42);
            MenuChip(root.transform,"Social","♟",0);
            MenuChip(root.transform,"Correo","✉",1);
            MenuChip(root.transform,"Menú","☰",2);
        }

        void CreateExtraNav()
        {
            var root=new GameObject("Reference extra nav",typeof(RectTransform));root.transform.SetParent(safe,false);
            var rt=root.GetComponent<RectTransform>();rt.anchorMin=rt.anchorMax=new Vector2(1,0);rt.pivot=new Vector2(1,0);rt.sizeDelta=new Vector2(320,76);
            NavMedallion(root.transform,"Ejército","⚔",0);
            NavMedallion(root.transform,"Alianza","🔒",1);
        }

        void ApplyReferenceLayout()
        {
            if(safe==null)return;
            bool landscape=safe.rect.width>safe.rect.height*1.08f;
            if(landscape!=lastLandscape)lastLandscape=landscape;

            if(landscape) ApplyLandscape();
            else ApplyPortrait();
        }

        void ApplyLandscape()
        {
            float w=safe.rect.width;

            var top=GameObject.Find("Reference topbar")?.GetComponent<RectTransform>();
            if(top!=null)
            {
                top.anchorMin=new Vector2(0,1);top.anchorMax=new Vector2(1,1);top.pivot=new Vector2(.5f,1);
                top.sizeDelta=new Vector2(0,52);top.anchoredPosition=Vector2.zero;
                var g=top.GetComponent<HorizontalLayoutGroup>();if(g!=null)g.enabled=false;
                Place(top,"Realm crest chip",new Vector2(88,-7),new Vector2(28,38),new Vector2(0,1));
                Place(top,"Heading",new Vector2(0,0),new Vector2(1,1),new Vector2(0,1));
                Place(top,"Power chip",new Vector2(118,-5),new Vector2(154,42),new Vector2(0,1));
                Place(top,"Wood resource chip",new Vector2(w-610,-5),new Vector2(145,42),new Vector2(0,1));
                Place(top,"Stone resource chip",new Vector2(w-458,-5),new Vector2(145,42),new Vector2(0,1));
            }

            var portrait=safe.Find("Reference portrait") as RectTransform;
            if(portrait!=null){portrait.anchoredPosition=new Vector2(8,-5);portrait.sizeDelta=new Vector2(80,80);}
            var vip=safe.Find("Reference VIP") as RectTransform;
            if(vip!=null){vip.anchoredPosition=new Vector2(98,-54);vip.sizeDelta=new Vector2(92,26);}

            var quest=GameObject.Find("Quest panel")?.GetComponent<RectTransform>();
            if(quest!=null)
            {
                quest.anchorMin=quest.anchorMax=new Vector2(0,1);quest.pivot=new Vector2(0,1);
                quest.anchoredPosition=new Vector2(14,-98);quest.sizeDelta=new Vector2(306,164);
                var v=quest.GetComponent<VerticalLayoutGroup>();if(v!=null)v.enabled=false;
                var kicker=quest.transform.Find("Quest kicker")?.GetComponent<Text>();
                if(kicker!=null)
                {
                    kicker.text="❓   Capítulo I                                  3/5   ›";
                    kicker.fontSize=11;kicker.alignment=TextAnchor.MiddleLeft;
                    var rt=kicker.rectTransform;rt.anchorMin=rt.anchorMax=new Vector2(0,1);rt.pivot=new Vector2(0,1);rt.anchoredPosition=new Vector2(10,-8);rt.sizeDelta=new Vector2(286,28);
                }
                var obj=quest.transform.Find("Objective")?.GetComponent<Text>();
                if(obj!=null)
                {
                    obj.fontSize=8;obj.color=new Color(.82f,.84f,.84f,1);obj.alignment=TextAnchor.LowerLeft;
                    var rt=obj.rectTransform;rt.anchorMin=rt.anchorMax=new Vector2(0,0);rt.pivot=new Vector2(0,0);rt.anchoredPosition=new Vector2(13,7);rt.sizeDelta=new Vector2(274,30);
                }
            }

            var left=safe.Find("Reference left actions") as RectTransform;
            if(left!=null)left.anchoredPosition=new Vector2(16,-292);

            var chat=safe.Find("Reference chat") as RectTransform;
            if(chat!=null){chat.anchoredPosition=new Vector2(120,9);chat.sizeDelta=new Vector2(Mathf.Min(380,w*.31f),72);}

            var menu=safe.Find("Reference top menu") as RectTransform;
            if(menu!=null)menu.anchoredPosition=new Vector2(-8,-5);

            var dock=GameObject.Find("World objective dock")?.GetComponent<RectTransform>();
            if(dock!=null)
            {
                dock.GetComponent<Image>().color=new Color(0,0,0,0);
                var frame=dock.Find("ReferenceArtFrame");if(frame!=null)frame.gameObject.SetActive(false);
                dock.anchorMin=dock.anchorMax=new Vector2(0,1);dock.pivot=new Vector2(0,1);
                dock.anchoredPosition=new Vector2(277,-101);dock.sizeDelta=new Vector2(38,28);
                var vg=dock.GetComponent<VerticalLayoutGroup>();if(vg!=null)vg.enabled=false;
                foreach(Transform child in dock)child.gameObject.SetActive(false);
                var primary=GameObject.Find("CONTINUAR");
                if(primary!=null)
                {
                    primary.SetActive(true);
                    var pr=primary.GetComponent<RectTransform>();pr.anchorMin=Vector2.zero;pr.anchorMax=Vector2.one;pr.offsetMin=pr.offsetMax=Vector2.zero;
                    primary.GetComponent<Image>().color=new Color(.82f,.65f,.31f,.10f);
                    var t=primary.GetComponentInChildren<Text>();if(t!=null){t.text="›";t.fontSize=18;t.color=GoldSoft;}
                }
            }

            LayoutBottomNavigation(w);
            var extra=safe.Find("Reference extra nav") as RectTransform;
            if(extra!=null){extra.gameObject.SetActive(true);extra.anchoredPosition=new Vector2(-530,7);}
        }

        void ApplyPortrait()
        {
            var top=GameObject.Find("Reference topbar")?.GetComponent<RectTransform>();
            if(top!=null)
            {
                top.anchorMin=new Vector2(0,1);top.anchorMax=new Vector2(1,1);top.pivot=new Vector2(.5f,1);top.sizeDelta=new Vector2(0,68);
                var g=top.GetComponent<HorizontalLayoutGroup>();if(g!=null)g.enabled=true;
            }
            var portrait=safe.Find("Reference portrait") as RectTransform;if(portrait!=null){portrait.anchoredPosition=new Vector2(8,-72);portrait.sizeDelta=new Vector2(58,58);}
            var vip=safe.Find("Reference VIP") as RectTransform;if(vip!=null){vip.anchoredPosition=new Vector2(71,-81);vip.sizeDelta=new Vector2(82,24);}
            var quest=GameObject.Find("Quest panel")?.GetComponent<RectTransform>();
            if(quest!=null){quest.anchoredPosition=new Vector2(10,-142);quest.sizeDelta=new Vector2(Mathf.Min(300,safe.rect.width-20),150);}
            var left=safe.Find("Reference left actions") as RectTransform;if(left!=null)left.anchoredPosition=new Vector2(8,-310);
            var chat=safe.Find("Reference chat") as RectTransform;if(chat!=null){chat.anchoredPosition=new Vector2(8,82);chat.sizeDelta=new Vector2(Mathf.Min(270,safe.rect.width-16),68);}
            var menu=safe.Find("Reference top menu") as RectTransform;if(menu!=null)menu.anchoredPosition=new Vector2(-5,-72);

            var dock=GameObject.Find("World objective dock")?.GetComponent<RectTransform>();
            if(dock!=null)
            {
                dock.gameObject.SetActive(true);
                dock.anchorMin=dock.anchorMax=new Vector2(.5f,0);dock.pivot=new Vector2(.5f,0);dock.sizeDelta=new Vector2(Mathf.Min(360,safe.rect.width-20),94);dock.anchoredPosition=new Vector2(0,78);
                var vg=dock.GetComponent<VerticalLayoutGroup>();if(vg!=null)vg.enabled=true;
                dock.GetComponent<Image>().color=Panel;
                foreach(Transform child in dock)child.gameObject.SetActive(true);
            }
            var nav=GameObject.Find("Bottom navigation")?.GetComponent<RectTransform>();
            if(nav!=null)
            {
                nav.anchorMin=new Vector2(0,0);nav.anchorMax=new Vector2(1,0);nav.pivot=new Vector2(.5f,0);nav.sizeDelta=new Vector2(0,68);nav.anchoredPosition=Vector2.zero;
                var hg=nav.GetComponent<HorizontalLayoutGroup>();if(hg!=null)hg.enabled=true;
            }
            var extra=safe.Find("Reference extra nav");if(extra!=null)extra.gameObject.SetActive(false);
        }

        void LayoutBottomNavigation(float width)
        {
            var nav=GameObject.Find("Bottom navigation")?.GetComponent<RectTransform>();if(nav==null)return;
            nav.anchorMin=new Vector2(0,0);nav.anchorMax=new Vector2(1,0);nav.pivot=new Vector2(.5f,0);nav.sizeDelta=new Vector2(0,86);nav.anchoredPosition=Vector2.zero;
            var hg=nav.GetComponent<HorizontalLayoutGroup>();if(hg!=null)hg.enabled=false;
            nav.GetComponent<Image>().color=new Color(.01f,.015f,.018f,.72f);

            var world=nav.Find("MUNDO")?.GetComponent<RectTransform>();
            var city=nav.Find("CIUDAD")?.GetComponent<RectTransform>();
            var heroes=nav.Find("HÉROES")?.GetComponent<RectTransform>();
            var chest=nav.Find("ARCÓN")?.GetComponent<RectTransform>();
            var codex=nav.Find("CÓDICE")?.GetComponent<RectTransform>();
            if(world!=null)PlaceBottom(world,24,8,76,74,"⌖\nMundo");
            if(heroes!=null)PlaceBottom(heroes,width-610,8,74,74,"♞\nHéroes");
            if(codex!=null)PlaceBottom(codex,width-450,8,74,74,"▤\nMisiones");
            if(chest!=null)PlaceBottom(chest,width-370,8,74,74,"▣\nInventario");
            if(city!=null)
            {
                PlaceBottom(city,width-112,4,104,80,"♜\nBastión");
                var im=city.GetComponent<Image>();if(im!=null)im.color=new Color(.025f,.055f,.075f,.98f);
            }
        }

        void UpdateWorldLabels()
        {
            if(safe==null||sceneCamera==null)return;
            bool landscape=safe.rect.width>safe.rect.height*1.08f;
            foreach(var kv in labels)
            {
                var label=kv.Value;if(label==null)continue;
                var target=GameObject.Find(kv.Key);
                bool show=landscape&&target!=null&&target.activeInHierarchy;
                label.SetActive(show);
                if(!show)continue;
                var col=target.GetComponent<Collider>();
                var world=col!=null?col.bounds.center+Vector3.up*(col.bounds.extents.y+1.0f):target.transform.position+Vector3.up*2f;
                var screen=sceneCamera.WorldToScreenPoint(world);
                if(screen.z<=0){label.SetActive(false);continue;}
                if(RectTransformUtility.ScreenPointToLocalPointInRectangle(safe,screen,null,out var local))
                    label.GetComponent<RectTransform>().anchoredPosition=local;
            }
        }

        void EnsureWorldLabel(string targetName,string title,string icon)
        {
            if(labels.ContainsKey(targetName)&&labels[targetName]!=null)return;
            var existing=safe.Find("Reference label "+title);
            GameObject go;
            if(existing!=null)go=existing.gameObject;
            else
            {
                go=PanelObject("Reference label "+title,safe,new Vector2(title=="Bastión"?142:128,38));
                go.GetComponent<RectTransform>().anchorMin=go.GetComponent<RectTransform>().anchorMax=new Vector2(.5f,.5f);
                go.GetComponent<RectTransform>().pivot=new Vector2(.5f,0);
                var text=MakeText("Label",go.transform,9,Color.white,TextAnchor.MiddleCenter);
                text.text=icon+"   "+title+"   ▲\nNv. 1";
                Stretch(text.rectTransform,3f);
                Frame(go,true,1.5f);
            }
            labels[targetName]=go;
        }

        static void PlaceBottom(RectTransform rt,float x,float y,float w,float h,string text)
        {
            rt.anchorMin=rt.anchorMax=new Vector2(0,0);rt.pivot=new Vector2(0,0);rt.anchoredPosition=new Vector2(x,y);rt.sizeDelta=new Vector2(w,h);
            var t=rt.GetComponentInChildren<Text>();if(t!=null){t.text=text;t.fontSize=9;t.lineSpacing=.82f;t.alignment=TextAnchor.MiddleCenter;}
            var img=rt.GetComponent<Image>();if(img!=null){img.color=new Color(.025f,.033f,.038f,.97f);var o=img.GetComponent<Outline>()??img.gameObject.AddComponent<Outline>();o.effectColor=Bronze;o.effectDistance=new Vector2(2,-2);}
        }

        static void Place(RectTransform parent,string childName,Vector2 pos,Vector2 size,Vector2 anchor)
        {
            var rt=parent.Find(childName) as RectTransform;if(rt==null)return;
            rt.anchorMin=rt.anchorMax=anchor;rt.pivot=anchor;rt.anchoredPosition=pos;rt.sizeDelta=size;
        }

        static GameObject PanelObject(string name,Transform parent,Vector2 size)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);
            go.GetComponent<RectTransform>().sizeDelta=size;go.GetComponent<Image>().color=Panel;go.GetComponent<Image>().raycastTarget=false;return go;
        }

        static Text MakeText(string name,Transform parent,int size,Color color,TextAnchor align)
        {
            var t=new GameObject(name,typeof(RectTransform),typeof(Text)).GetComponent<Text>();
            t.transform.SetParent(parent,false);t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.fontSize=size;t.color=color;t.alignment=align;
            t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;t.raycastTarget=false;return t;
        }

        static void ActionMedallion(Transform parent,string name,string icon,string count,int index)
        {
            var go=PanelObject(name,parent,new Vector2(54,54));var rt=go.GetComponent<RectTransform>();
            rt.anchorMin=rt.anchorMax=new Vector2(0,1);rt.pivot=new Vector2(0,1);rt.anchoredPosition=new Vector2(0,-index*68f);
            var t=MakeText("Icon",go.transform,18,GoldSoft,TextAnchor.MiddleCenter);t.text=icon+"\n<size=8>"+count+"</size>";Stretch(t.rectTransform,2);
            var o=go.AddComponent<Outline>();o.effectColor=Bronze;o.effectDistance=new Vector2(2,-2);
        }

        static void MenuChip(Transform parent,string name,string glyph,int index)
        {
            var go=PanelObject(name,parent,new Vector2(42,38));var rt=go.GetComponent<RectTransform>();
            rt.anchorMin=rt.anchorMax=new Vector2(0,1);rt.pivot=new Vector2(0,1);rt.anchoredPosition=new Vector2(index*48,0);
            var t=MakeText("Glyph",go.transform,18,new Color(.88f,.84f,.74f),TextAnchor.MiddleCenter);t.text=glyph;Stretch(t.rectTransform,1);
        }

        static void NavMedallion(Transform parent,string label,string glyph,int index)
        {
            var go=PanelObject(label,parent,new Vector2(72,72));var rt=go.GetComponent<RectTransform>();
            rt.anchorMin=rt.anchorMax=new Vector2(0,0);rt.pivot=new Vector2(0,0);rt.anchoredPosition=new Vector2(index==0?0:240,0);
            var t=MakeText("Text",go.transform,9,index==1?new Color(.47f,.48f,.48f):GoldSoft,TextAnchor.MiddleCenter);
            t.text=glyph+"\n"+label;Stretch(t.rectTransform,2);
            var o=go.AddComponent<Outline>();o.effectColor=index==1?new Color(.26f,.26f,.26f):Bronze;o.effectDistance=new Vector2(2,-2);
        }

        static void Badge(Transform parent,string value,Vector2 anchor,Vector2 size,Vector2 pos,Color bg)
        {
            var go=PanelObject("Badge "+value,parent,size);var rt=go.GetComponent<RectTransform>();rt.anchorMin=rt.anchorMax=anchor;rt.pivot=new Vector2(.5f,.5f);rt.anchoredPosition=pos;
            go.GetComponent<Image>().color=bg;var t=MakeText("Text",go.transform,8,Color.white,TextAnchor.MiddleCenter);t.text=value;Stretch(t.rectTransform,1);
        }

        static void Frame(GameObject go,bool corners,float edge)
        {
            if(go==null)return;
            var image=go.GetComponent<Image>();
            if(image!=null)
            {
                image.color=Panel;
                var shadow=image.GetComponent<Shadow>()??image.gameObject.AddComponent<Shadow>();
                shadow.effectColor=new Color(0,0,0,.70f);shadow.effectDistance=new Vector2(0,-3);shadow.useGraphicAlpha=true;
            }
            if(go.transform.Find("ReferenceArtFrame")!=null)return;
            var frame=new GameObject("ReferenceArtFrame",typeof(RectTransform));frame.transform.SetParent(go.transform,false);frame.transform.SetAsFirstSibling();
            var rt=frame.GetComponent<RectTransform>();Stretch(rt,0);var ignore=frame.AddComponent<LayoutElement>();ignore.ignoreLayout=true;
            Edge(frame.transform,"Top bronze",new Vector2(0,1),new Vector2(1,1),new Vector2(0,-edge),new Vector2(0,0),Bronze);
            Edge(frame.transform,"Bottom bronze",new Vector2(0,0),new Vector2(1,0),new Vector2(0,0),new Vector2(0,edge),BronzeDark);
            Edge(frame.transform,"Left bronze",new Vector2(0,0),new Vector2(0,1),new Vector2(0,0),new Vector2(edge,0),Bronze);
            Edge(frame.transform,"Right bronze",new Vector2(1,0),new Vector2(1,1),new Vector2(-edge,0),new Vector2(0,0),BronzeDark);
            Edge(frame.transform,"Top inner",new Vector2(0,1),new Vector2(1,1),new Vector2(5,-5),new Vector2(-5,-3.5f),new Color(.84f,.68f,.38f,.45f));
            if(corners){Corner(frame.transform,"TL",new Vector2(0,1),new Vector2(7,-7));Corner(frame.transform,"TR",new Vector2(1,1),new Vector2(-7,-7));Corner(frame.transform,"BL",new Vector2(0,0),new Vector2(7,7));Corner(frame.transform,"BR",new Vector2(1,0),new Vector2(-7,7));}
        }

        static void StyleButton(Button button)
        {
            if(button==null)return;var image=button.GetComponent<Image>();if(image==null)return;
            bool primary=button.gameObject.name=="CONTINUAR"||button.gameObject.name=="Building action";
            bool nav=button.transform.parent!=null&&button.transform.parent.name=="Bottom navigation";
            var cb=button.colors;
            if(!button.interactable)image.color=nav?new Color(.025f,.030f,.033f,.92f):Disabled;
            else if(primary){image.color=Gold;cb.normalColor=Gold;cb.highlightedColor=GoldSoft;cb.pressedColor=new Color(.53f,.39f,.19f,1);cb.selectedColor=GoldSoft;}
            else if(!nav){image.color=PanelDeep;cb.normalColor=PanelDeep;cb.highlightedColor=new Color(.13f,.12f,.09f,.98f);cb.pressedColor=new Color(.08f,.065f,.045f,.98f);}
            button.colors=cb;
            if(button.transform.Find("ReferenceButtonFrame")!=null)return;
            var f=new GameObject("ReferenceButtonFrame",typeof(RectTransform));f.transform.SetParent(button.transform,false);f.transform.SetAsFirstSibling();
            Stretch(f.GetComponent<RectTransform>(),0);var ignore=f.AddComponent<LayoutElement>();ignore.ignoreLayout=true;
            var outline=image.GetComponent<Outline>()??image.gameObject.AddComponent<Outline>();outline.effectColor=primary?BronzeDark:new Color(.42f,.31f,.17f,.92f);outline.effectDistance=new Vector2(1.5f,-1.5f);outline.useGraphicAlpha=true;
            Edge(f.transform,"Button top",new Vector2(0,1),new Vector2(1,1),new Vector2(3,-3),new Vector2(-3,-1.5f),primary?GoldSoft:new Color(.58f,.44f,.24f,.75f));
            Edge(f.transform,"Button bottom",new Vector2(0,0),new Vector2(1,0),new Vector2(3,1.5f),new Vector2(-3,3),new Color(.09f,.055f,.025f,.95f));
        }

        static void StyleText(Text text)
        {
            if(text==null||text.transform.parent==null)return;
            bool primary=text.transform.parent.name=="CONTINUAR"||text.transform.parent.name=="Building action";
            if(primary){text.color=Ink;text.fontStyle=FontStyle.Bold;}
            else if(text.name=="Quest kicker"||text.name=="Building title"){text.color=GoldSoft;text.fontStyle=FontStyle.Bold;}
        }

        static void Edge(Transform parent,string name,Vector2 amin,Vector2 amax,Vector2 omin,Vector2 omax,Color color)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);
            var rt=go.GetComponent<RectTransform>();rt.anchorMin=amin;rt.anchorMax=amax;rt.offsetMin=omin;rt.offsetMax=omax;
            var img=go.GetComponent<Image>();img.color=color;img.raycastTarget=false;
        }

        static void Corner(Transform parent,string name,Vector2 anchor,Vector2 pos)
        {
            var go=new GameObject("Corner "+name,typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);
            var rt=go.GetComponent<RectTransform>();rt.anchorMin=rt.anchorMax=anchor;rt.sizeDelta=new Vector2(9,9);rt.anchoredPosition=pos;rt.localRotation=Quaternion.Euler(0,0,45);
            var img=go.GetComponent<Image>();img.color=Gold;img.raycastTarget=false;
        }

        static void Stretch(RectTransform rt,float inset)
        {
            rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=new Vector2(inset,inset);rt.offsetMax=new Vector2(-inset,-inset);
        }
    }
}
