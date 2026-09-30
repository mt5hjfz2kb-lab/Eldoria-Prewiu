using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Eldoria.Presentation
{
    /// <summary>
    /// Runtime implementation of the owner-approved Eldoria HUD reference (final reference-convergence gate; certified with Presentation-aware EditMode tests).
    /// Functional controls remain owned by SlicePresenter; this component reshapes and
    /// decorates them into the reference composition and adds visual-only reference chrome.
    /// Owner reference composition lock: landscape HUD uses the approved screenshot hierarchy.
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
        static Sprite circleSprite;
        static Texture2D referenceAtlas;
        static readonly Dictionary<string,Sprite> ReferenceSprites=new Dictionary<string,Sprite>();

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
            Frame(GameObject.Find("World objective dock"), true, 2f);
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
            if (safe.Find("Reference future resources") == null) CreateFutureResources();
            if (safe.Find("Reference extra nav") == null) CreateExtraNav();
            EnsureWorldLabel("Bastion · target","Bastión","⬡");
            EnsureWorldLabel("Aserradero · target","Aserradero","⚒");
            EnsureWorldLabel("Cuartel · target","Cuartel","⚔");
            EnsureWorldLabel("Granero · target","Granero","✥");
        }

        void CreatePortrait()
        {
            var go = new GameObject("Reference portrait", typeof(RectTransform), typeof(Image), typeof(Mask));
            go.transform.SetParent(safe,false);
            var rt=go.GetComponent<RectTransform>();
            rt.anchorMin=rt.anchorMax=new Vector2(0,1);rt.pivot=new Vector2(0,1);rt.sizeDelta=new Vector2(78,78);
            var ring=go.GetComponent<Image>();ring.sprite=ReferenceSprite("portrait_face");ring.color=Color.white;ring.preserveAspect=true;ring.raycastTarget=false;
            go.GetComponent<Mask>().showMaskGraphic=true;
            var outline=go.AddComponent<Outline>();outline.effectColor=BronzeDark;outline.effectDistance=new Vector2(2,-2);
        }

        void CreateVip()
        {
            var go=PanelObject("Reference VIP",safe,new Vector2(92,26));
            var rt=go.GetComponent<RectTransform>();rt.anchorMin=rt.anchorMax=new Vector2(0,1);rt.pivot=new Vector2(0,1);
            var img=go.GetComponent<Image>();img.sprite=ReferenceSprite("vip");img.color=Color.white;img.preserveAspect=true;
            var text=MakeText("VIP text",go.transform,11,Color.clear,TextAnchor.MiddleCenter);text.text="VIP 2";Stretch(text.rectTransform,5f);
        }

        void CreateQuestSubtitle()
        {
            var quest=GameObject.Find("Quest panel"); if(quest==null)return;
            var chapter=CreateReferenceImage("Reference chapter icon",quest.transform,"chapter");var cir=chapter.GetComponent<RectTransform>();cir.anchorMin=cir.anchorMax=new Vector2(0,1);cir.pivot=new Vector2(0,1);cir.anchoredPosition=new Vector2(5,-5);cir.sizeDelta=new Vector2(34,31);
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
            var rr=root.GetComponent<RectTransform>();rr.anchorMin=rr.anchorMax=new Vector2(0,1);rr.pivot=new Vector2(0,1);rr.sizeDelta=new Vector2(92,300);
            ActionMedallion(root.transform,"Construcción","left_build","0/2",0);
            ActionMedallion(root.transform,"Investigación","left_research","0/1",1);
            ActionMedallion(root.transform,"Población","left_people","0/1",2);
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

        void CreateFutureResources()
        {
            var root=new GameObject("Reference future resources",typeof(RectTransform));root.transform.SetParent(safe,false);
            var rt=root.GetComponent<RectTransform>();rt.anchorMin=rt.anchorMax=new Vector2(1,1);rt.pivot=new Vector2(1,1);rt.sizeDelta=new Vector2(430,54);
            FutureResource(root.transform,"Wheat","wheat",0);
            FutureResource(root.transform,"Iron","iron",1);
            FutureResource(root.transform,"Gems","gem",2);
        }

        void CreateTopMenu()
        {
            var root=new GameObject("Reference top menu",typeof(RectTransform));root.transform.SetParent(safe,false);
            var rt=root.GetComponent<RectTransform>();rt.anchorMin=rt.anchorMax=new Vector2(1,1);rt.pivot=new Vector2(1,1);rt.sizeDelta=new Vector2(214,64);
            MenuChip(root.transform,"Social","social",0);
            MenuChip(root.transform,"Correo","mail",1);
            MenuChip(root.transform,"Menú","menu",2);
        }

        void CreateExtraNav()
        {
            var root=new GameObject("Reference extra nav",typeof(RectTransform));root.transform.SetParent(safe,false);
            var rt=root.GetComponent<RectTransform>();rt.anchorMin=rt.anchorMax=new Vector2(1,0);rt.pivot=new Vector2(1,0);rt.sizeDelta=new Vector2(420,104);
            NavMedallion(root.transform,"Ejército","army",0);
            NavMedallion(root.transform,"Alianza","alliance",1);
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
            float h=safe.rect.height;

            var top=GameObject.Find("Reference topbar")?.GetComponent<RectTransform>();
            if(top!=null)
            {
                top.anchorMin=new Vector2(0,1);top.anchorMax=new Vector2(1,1);top.pivot=new Vector2(.5f,1);
                top.sizeDelta=new Vector2(0,h*.075f);top.anchoredPosition=Vector2.zero;
                var g=top.GetComponent<HorizontalLayoutGroup>();if(g!=null)g.enabled=false;

                var crest=top.Find("Realm crest chip") as RectTransform;
                if(crest!=null)crest.gameObject.SetActive(false);
                var headingRt=top.Find("Heading") as RectTransform;
                if(headingRt!=null)headingRt.gameObject.SetActive(false);

                Place(top,"Power chip",new Vector2(w*.092f,-h*.008f),new Vector2(w*.15f,h*.060f),new Vector2(0,1));
                Place(top,"Wood resource chip",new Vector2(w*.405f,-h*.008f),new Vector2(w*.135f,h*.060f),new Vector2(0,1));
                Place(top,"Stone resource chip",new Vector2(w*.535f,-h*.008f),new Vector2(w*.125f,h*.060f),new Vector2(0,1));
                SetFont(top,"Power chip",16);
                SetFont(top,"Wood resource chip",15);
                SetFont(top,"Stone resource chip",15);
                ApplyReferenceChip(top,"Power chip","power");
                ApplyReferenceChip(top,"Wood resource chip","wood");
                ApplyReferenceChip(top,"Stone resource chip","stone");
            }

            float portraitSize=h*.132f;
            var portrait=safe.Find("Reference portrait") as RectTransform;
            if(portrait!=null){portrait.anchoredPosition=new Vector2(w*.006f,-h*.004f);portrait.sizeDelta=new Vector2(portraitSize,portraitSize);}
            var vip=safe.Find("Reference VIP") as RectTransform;
            if(vip!=null){vip.anchoredPosition=new Vector2(w*.068f,-h*.077f);vip.sizeDelta=new Vector2(w*.104f,h*.038f);SetFont(vip,"VIP text",14);}

            var quest=GameObject.Find("Quest panel")?.GetComponent<RectTransform>();
            if(quest!=null)
            {
                quest.anchorMin=quest.anchorMax=new Vector2(0,1);quest.pivot=new Vector2(0,1);
                quest.anchoredPosition=new Vector2(w*.012f,-h*.126f);quest.sizeDelta=new Vector2(w*.238f,h*.245f);
                var v=quest.GetComponent<VerticalLayoutGroup>();if(v!=null)v.enabled=false;
                var kicker=quest.transform.Find("Quest kicker")?.GetComponent<Text>();
                if(kicker!=null)
                {
                    kicker.text="❓   Capítulo I                         3/5   ›";
                    kicker.fontSize=16;kicker.alignment=TextAnchor.MiddleLeft;
                    var rt=kicker.rectTransform;rt.anchorMin=rt.anchorMax=new Vector2(0,1);rt.pivot=new Vector2(0,1);rt.anchoredPosition=new Vector2(10,-8);rt.sizeDelta=new Vector2(quest.sizeDelta.x-20,h*.038f);
                }
                var subtitle=quest.transform.Find("Reference quest subtitle")?.GetComponent<Text>();
                if(subtitle!=null){subtitle.fontSize=14;var rt=subtitle.rectTransform;rt.anchoredPosition=new Vector2(13,-h*.055f);rt.sizeDelta=new Vector2(quest.sizeDelta.x-26,h*.036f);}
                var checks=quest.transform.Find("Reference quest checks")?.GetComponent<Text>();
                if(checks!=null){checks.fontSize=12;var rt=checks.rectTransform;rt.anchoredPosition=new Vector2(13,-h*.093f);rt.sizeDelta=new Vector2(quest.sizeDelta.x-26,h*.09f);}
                var obj=quest.transform.Find("Objective")?.GetComponent<Text>();
                if(obj!=null)
                {
                    obj.fontSize=10;obj.color=new Color(.82f,.84f,.84f,1);obj.alignment=TextAnchor.LowerLeft;
                    var rt=obj.rectTransform;rt.anchorMin=rt.anchorMax=new Vector2(0,0);rt.pivot=new Vector2(0,0);rt.anchoredPosition=new Vector2(13,7);rt.sizeDelta=new Vector2(quest.sizeDelta.x-26,h*.038f);
                }
            }

            var left=safe.Find("Reference left actions") as RectTransform;
            if(left!=null){left.anchoredPosition=new Vector2(w*.012f,-h*.39f);left.localScale=Vector3.one;}

            var chat=safe.Find("Reference chat") as RectTransform;
            if(chat!=null)
            {
                chat.anchoredPosition=new Vector2(w*.073f,h*.010f);chat.sizeDelta=new Vector2(w*.315f,h*.135f);
                SetFont(chat,"Chat copy",13);
            }

            var menu=safe.Find("Reference top menu") as RectTransform;
            if(menu!=null){menu.anchoredPosition=new Vector2(-w*.008f,-h*.006f);menu.localScale=Vector3.one;}
            var future=safe.Find("Reference future resources") as RectTransform;
            if(future!=null){future.gameObject.SetActive(true);future.anchoredPosition=new Vector2(-w*.155f,-h*.006f);future.localScale=Vector3.one;}

            var dock=GameObject.Find("World objective dock")?.GetComponent<RectTransform>();
            if(dock!=null)
            {
                dock.GetComponent<Image>().color=new Color(0,0,0,0);
                var frame=dock.Find("ReferenceArtFrame");if(frame!=null)frame.gameObject.SetActive(false);
                dock.anchorMin=dock.anchorMax=new Vector2(0,1);dock.pivot=new Vector2(0,1);
                dock.anchoredPosition=new Vector2(w*.205f,-h*.108f);dock.sizeDelta=new Vector2(w*.032f,h*.036f);
                var vg=dock.GetComponent<VerticalLayoutGroup>();if(vg!=null)vg.enabled=false;
                foreach(Transform child in dock)child.gameObject.SetActive(false);
                var actionRow=dock.Find("Primary objective action") as RectTransform;
                if(actionRow!=null)
                {
                    actionRow.gameObject.SetActive(true);
                    actionRow.anchorMin=Vector2.zero;actionRow.anchorMax=Vector2.one;actionRow.offsetMin=actionRow.offsetMax=Vector2.zero;
                    var primary=actionRow.Find("CONTINUAR")?.gameObject;
                    if(primary!=null)
                    {
                        primary.SetActive(true);
                        var pr=primary.GetComponent<RectTransform>();pr.anchorMin=Vector2.zero;pr.anchorMax=Vector2.one;pr.offsetMin=pr.offsetMax=Vector2.zero;
                        primary.GetComponent<Image>().color=new Color(.82f,.65f,.31f,.12f);
                        var t=primary.GetComponentInChildren<Text>();if(t!=null)t.color=Color.clear;
                        var arrow=primary.transform.Find("Reference quest arrow");
                        Text arrowText=arrow!=null?arrow.GetComponent<Text>():null;
                        if(arrowText==null)
                        {
                            arrowText=MakeText("Reference quest arrow",primary.transform,20,GoldSoft,TextAnchor.MiddleCenter);
                            Stretch(arrowText.rectTransform,0f);
                        }
                        arrowText.gameObject.SetActive(true);arrowText.text="›";arrowText.fontSize=20;arrowText.color=GoldSoft;
                    }
                }
            }

            LayoutBottomNavigation(w,h);
            var extra=safe.Find("Reference extra nav") as RectTransform;
            if(extra!=null){extra.gameObject.SetActive(true);extra.anchoredPosition=new Vector2(-w*.355f,h*.010f);extra.localScale=Vector3.one;}
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
            if(quest!=null)
            {
                quest.anchorMin=quest.anchorMax=new Vector2(0,1);quest.pivot=new Vector2(0,1);
                quest.anchoredPosition=new Vector2(10,-142);quest.sizeDelta=new Vector2(Mathf.Min(300,safe.rect.width-20),150);
                var ql=quest.GetComponent<VerticalLayoutGroup>();if(ql!=null)ql.enabled=false;
                var kicker=quest.transform.Find("Quest kicker")?.GetComponent<Text>();
                if(kicker!=null){kicker.text="❓   Capítulo I                    3/5   ›";kicker.fontSize=11;var rt=kicker.rectTransform;rt.anchorMin=rt.anchorMax=new Vector2(0,1);rt.pivot=new Vector2(0,1);rt.anchoredPosition=new Vector2(10,-7);rt.sizeDelta=new Vector2(quest.sizeDelta.x-20,25);}
                var subtitle=quest.transform.Find("Reference quest subtitle")?.GetComponent<Text>();
                if(subtitle!=null){subtitle.fontSize=10;var rt=subtitle.rectTransform;rt.anchorMin=rt.anchorMax=new Vector2(0,1);rt.pivot=new Vector2(0,1);rt.anchoredPosition=new Vector2(12,-34);rt.sizeDelta=new Vector2(quest.sizeDelta.x-24,22);}
                var checks=quest.transform.Find("Reference quest checks")?.GetComponent<Text>();
                if(checks!=null){checks.fontSize=8;var rt=checks.rectTransform;rt.anchorMin=rt.anchorMax=new Vector2(0,1);rt.pivot=new Vector2(0,1);rt.anchoredPosition=new Vector2(12,-58);rt.sizeDelta=new Vector2(quest.sizeDelta.x-24,58);}
                var obj=quest.transform.Find("Objective")?.GetComponent<Text>();
                if(obj!=null){obj.fontSize=7;obj.alignment=TextAnchor.LowerLeft;var rt=obj.rectTransform;rt.anchorMin=rt.anchorMax=new Vector2(0,0);rt.pivot=new Vector2(0,0);rt.anchoredPosition=new Vector2(12,6);rt.sizeDelta=new Vector2(quest.sizeDelta.x-24,28);}
            }
            var left=safe.Find("Reference left actions") as RectTransform;if(left!=null)left.anchoredPosition=new Vector2(8,-310);
            var chat=safe.Find("Reference chat") as RectTransform;if(chat!=null){chat.anchoredPosition=new Vector2(8,82);chat.sizeDelta=new Vector2(Mathf.Min(270,safe.rect.width-16),68);}
            var menu=safe.Find("Reference top menu") as RectTransform;if(menu!=null)menu.anchoredPosition=new Vector2(-5,-72);
            var future=safe.Find("Reference future resources");if(future!=null)future.gameObject.SetActive(false);

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
            var primary=GameObject.Find("CONTINUAR");
            if(primary!=null)
            {
                var liveText=primary.GetComponentInChildren<Text>();if(liveText!=null)liveText.color=Ink;
                var arrow=primary.transform.Find("Reference quest arrow");if(arrow!=null)arrow.gameObject.SetActive(false);
            }
            var extra=safe.Find("Reference extra nav");if(extra!=null)extra.gameObject.SetActive(false);
        }

        void LayoutBottomNavigation(float width,float height)
        {
            var nav=GameObject.Find("Bottom navigation")?.GetComponent<RectTransform>();if(nav==null)return;
            nav.anchorMin=new Vector2(0,0);nav.anchorMax=new Vector2(1,0);nav.pivot=new Vector2(.5f,0);nav.sizeDelta=new Vector2(0,height*.145f);nav.anchoredPosition=Vector2.zero;
            var hg=nav.GetComponent<HorizontalLayoutGroup>();if(hg!=null)hg.enabled=false;
            nav.GetComponent<Image>().color=new Color(.01f,.015f,.018f,.72f);

            var world=nav.Find("MUNDO")?.GetComponent<RectTransform>();
            var city=nav.Find("CIUDAD")?.GetComponent<RectTransform>();
            var heroes=nav.Find("HÉROES")?.GetComponent<RectTransform>();
            var chest=nav.Find("ARCÓN")?.GetComponent<RectTransform>();
            var codex=nav.Find("CÓDICE")?.GetComponent<RectTransform>();
            if(world!=null)PlaceBottom(world,width*.018f,height*.010f,height*.135f,height*.135f,"world");
            if(heroes!=null)PlaceBottom(heroes,width*.535f,height*.010f,height*.125f,height*.125f,"heroes");
            if(codex!=null)PlaceBottom(codex,width*.655f,height*.010f,height*.125f,height*.125f,"missions");
            if(chest!=null)PlaceBottom(chest,width*.745f,height*.010f,height*.125f,height*.125f,"inventory");
            if(city!=null)
            {
                PlaceBottom(city,width-height*.16f,height*.004f,height*.15f,height*.15f,"bastion");
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
                go=PanelObject("Reference label "+title,safe,new Vector2(title=="Bastión"?190:170,50));
                go.GetComponent<RectTransform>().anchorMin=go.GetComponent<RectTransform>().anchorMax=new Vector2(.5f,.5f);
                go.GetComponent<RectTransform>().pivot=new Vector2(.5f,0);
                var text=MakeText("Label",go.transform,12,Color.white,TextAnchor.MiddleCenter);
                text.text=icon+"   "+title+"   ▲\nNv. 1";
                Stretch(text.rectTransform,3f);
                Frame(go,true,1.5f);
            }
            labels[targetName]=go;
        }

        static void PlaceBottom(RectTransform rt,float x,float y,float w,float h,string spriteName)
        {
            rt.anchorMin=rt.anchorMax=new Vector2(0,0);rt.pivot=new Vector2(0,0);rt.anchoredPosition=new Vector2(x,y);rt.sizeDelta=new Vector2(w,h);
            var t=rt.GetComponentInChildren<Text>();if(t!=null)t.color=Color.clear;
            var img=rt.GetComponent<Image>();
            if(img!=null)
            {
                img.sprite=ReferenceSprite(spriteName);img.color=Color.white;img.preserveAspect=true;
                var o=img.GetComponent<Outline>();if(o!=null)o.enabled=false;
            }
        }

        static GameObject CreateReferenceImage(string name,Transform parent,string spriteName)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);
            var img=go.GetComponent<Image>();img.sprite=ReferenceSprite(spriteName);img.color=Color.white;img.preserveAspect=true;img.raycastTarget=false;return go;
        }

        static Sprite ReferenceSprite(string name)
        {
            if(ReferenceSprites.TryGetValue(name,out var cached)&&cached!=null)return cached;
            if(referenceAtlas==null)
            {
                string encoded="";
                for(int i=0;i<4;i++)
                {
                    var chunk=Resources.Load<TextAsset>("UI/eldoria_ui_reference_atlas_"+i);
                    if(chunk!=null)encoded+=chunk.text.Trim();
                }
                if(!string.IsNullOrEmpty(encoded))
                {
                    try
                    {
                        var bytes=System.Convert.FromBase64String(encoded);
                        referenceAtlas=new Texture2D(2,2,TextureFormat.RGBA32,false){name="Approved Eldoria HUD atlas",wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Bilinear};
                        referenceAtlas.LoadImage(bytes,false);
                    }
                    catch(System.Exception e){Debug.LogWarning("Reference HUD atlas decode failed: "+e.Message);}
                }
            }
            if(referenceAtlas==null)return CircleSprite();
            Rect r=AtlasRect(name);
            var sprite=Sprite.Create(referenceAtlas,r,new Vector2(.5f,.5f),100f);
            ReferenceSprites[name]=sprite;return sprite;
        }

        static Rect AtlasRect(string name)
        {
            // Source crop atlas is 128×128. Coordinates below are Unity bottom-left.
            switch(name)
            {
                case "portrait_face": return AtlasTop(1,1,24,24);
                case "vip": return AtlasTop(26.25f,8.75f,24,7.75f);
                case "power": return AtlasTop(56.25f,5.75f,14.75f,13.75f);
                case "wood": return AtlasTop(81.25f,5.75f,15.75f,13.75f);
                case "stone": return AtlasTop(106.25f,5.75f,16.75f,13.75f);
                case "wheat": return AtlasTop(4.5f,31.25f,16.25f,13.75f);
                case "iron": return AtlasTop(29.25f,31.25f,18,13.75f);
                case "gem": return AtlasTop(55.25f,31.25f,16.75f,13.75f);
                case "chapter": return AtlasTop(80.75f,30.5f,17,15.25f);
                case "left_build": return AtlasTop(105.75f,29.25f,18,18);
                case "left_research": return AtlasTop(3.75f,54.75f,18,18);
                case "left_people": return AtlasTop(29.25f,54.75f,18,18);
                case "world": return AtlasTop(51.75f,51.75f,24,24);
                case "heroes": return AtlasTop(78.75f,53.25f,21,21);
                case "army": return AtlasTop(104.25f,53.25f,21,21);
                case "missions": return AtlasTop(2.25f,78.75f,21,21);
                case "inventory": return AtlasTop(27.75f,78.75f,21,21);
                case "alliance": return AtlasTop(53.25f,78.75f,21,21);
                case "bastion": return AtlasTop(77.25f,78.75f,24,21);
                case "social": return AtlasTop(107.25f,82.5f,15,13.5f);
                case "mail": return AtlasTop(5.25f,108,15,13.5f);
                case "menu": return AtlasTop(29.25f,108,17.75f,13.5f);
                default: return new Rect(0,0,1,1);
            }
        }

        static Rect AtlasTop(float x,float y,float w,float h)=>new Rect(x,128f-y-h,w,h);

        static void SetFont(Transform parent,string childName,int size)
        {
            var child=parent.Find(childName);if(child==null)return;
            var text=child.GetComponentInChildren<Text>();if(text!=null)text.fontSize=size;
        }

        static Sprite CircleSprite()
        {
            if(circleSprite!=null)return circleSprite;
            const int n=64;
            var tex=new Texture2D(n,n,TextureFormat.RGBA32,false);
            tex.name="Reference UI circle";
            tex.wrapMode=TextureWrapMode.Clamp;
            var pixels=new Color32[n*n];
            float c=(n-1)*.5f,r=n*.49f;
            for(int y=0;y<n;y++)for(int x=0;x<n;x++)
            {
                float dx=x-c,dy=y-c;
                byte a=(byte)((dx*dx+dy*dy)<=r*r?255:0);
                pixels[y*n+x]=new Color32(255,255,255,a);
            }
            tex.SetPixels32(pixels);tex.Apply(false,true);
            circleSprite=Sprite.Create(tex,new Rect(0,0,n,n),new Vector2(.5f,.5f),100f);
            return circleSprite;
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

        static void ApplyReferenceChip(Transform parent,string childName,string spriteName)
        {
            var child=parent.Find(childName);if(child==null)return;
            var image=child.GetComponent<Image>();if(image!=null){image.sprite=null;image.color=new Color(.015f,.020f,.024f,.78f);}
            var text=child.GetComponentInChildren<Text>();
            if(text!=null)
            {
                var parts=(text.text??"").Split('\n');
                text.text=parts.Length>0?parts[parts.Length-1]:text.text;
                text.color=new Color(.96f,.96f,.93f,1f);text.alignment=TextAnchor.MiddleRight;
                var tr=text.rectTransform;tr.anchorMin=Vector2.zero;tr.anchorMax=Vector2.one;tr.offsetMin=new Vector2(36,1);tr.offsetMax=new Vector2(-5,-1);
            }
            var icon=child.Find("Reference exact icon")?.GetComponent<Image>();
            if(icon==null)icon=CreateReferenceImage("Reference exact icon",child,spriteName).GetComponent<Image>();
            icon.sprite=ReferenceSprite(spriteName);
            var ir=icon.rectTransform;ir.anchorMin=ir.anchorMax=new Vector2(0,.5f);ir.pivot=new Vector2(0,.5f);ir.anchoredPosition=new Vector2(2,0);ir.sizeDelta=new Vector2(34,31);
        }

        static void FutureResource(Transform parent,string name,string spriteName,int index)
        {
            var go=PanelObject(name,parent,new Vector2(126,48));var bg=go.GetComponent<Image>();bg.color=new Color(.015f,.020f,.024f,.78f);
            var rt=go.GetComponent<RectTransform>();rt.anchorMin=rt.anchorMax=new Vector2(0,1);rt.pivot=new Vector2(0,1);rt.anchoredPosition=new Vector2(index*136f,0);
            var icon=CreateReferenceImage("Reference exact icon",go.transform,spriteName);var ir=icon.GetComponent<RectTransform>();ir.anchorMin=ir.anchorMax=new Vector2(0,.5f);ir.pivot=new Vector2(0,.5f);ir.anchoredPosition=new Vector2(2,0);ir.sizeDelta=new Vector2(34,31);
            var value=MakeText("Value",go.transform,14,new Color(.70f,.72f,.72f,1),TextAnchor.MiddleRight);value.text="—";Stretch(value.rectTransform,4);value.rectTransform.offsetMin=new Vector2(38,1);
        }

        static void ActionMedallion(Transform parent,string name,string spriteName,string count,int index)
        {
            var go=PanelObject(name,parent,new Vector2(58,58));var img=go.GetComponent<Image>();img.sprite=ReferenceSprite(spriteName);img.color=Color.white;img.preserveAspect=true;
            var rt=go.GetComponent<RectTransform>();rt.anchorMin=rt.anchorMax=new Vector2(0,1);rt.pivot=new Vector2(0,1);rt.anchoredPosition=new Vector2(0,-index*68f);
            var countText=MakeText("Count",go.transform,8,Color.white,TextAnchor.LowerCenter);countText.text=count;countText.rectTransform.anchorMin=new Vector2(0,0);countText.rectTransform.anchorMax=new Vector2(1,.35f);countText.rectTransform.offsetMin=countText.rectTransform.offsetMax=Vector2.zero;
        }

        static void MenuChip(Transform parent,string name,string spriteName,int index)
        {
            var go=PanelObject(name,parent,new Vector2(44,40));var img=go.GetComponent<Image>();img.sprite=ReferenceSprite(spriteName);img.color=Color.white;img.preserveAspect=true;
            var rt=go.GetComponent<RectTransform>();rt.anchorMin=rt.anchorMax=new Vector2(0,1);rt.pivot=new Vector2(0,1);rt.anchoredPosition=new Vector2(index*48,0);
        }

        static void NavMedallion(Transform parent,string label,string spriteName,int index)
        {
            var go=PanelObject(label,parent,new Vector2(72,72));var img=go.GetComponent<Image>();img.sprite=ReferenceSprite(spriteName);img.color=Color.white;img.preserveAspect=true;
            var rt=go.GetComponent<RectTransform>();rt.anchorMin=rt.anchorMax=new Vector2(0,0);rt.pivot=new Vector2(0,0);rt.anchoredPosition=new Vector2(index==0?0:240,0);
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
