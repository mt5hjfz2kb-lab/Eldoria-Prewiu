using System;
using System.Linq;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.UI.RoyalGold;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
namespace Eldoria.UI.RoyalGold.V2 {
 // The web slice explicitly uses simulated profiles. Never imply online PvP.
 [DefaultExecutionOrder(1600)] public sealed class RoyalGoldV3RankingScreen:MonoBehaviour {
  RoyalHUD hud;ICommandGateway gateway;RectTransform root;RoyalText title,subtitle,rival,note;RoyalButton back;RoyalButton[] tabs=new RoyalButton[3];RoyalText[] rows=new RoyalText[9];int category;float width,height,next;
  static readonly Color Gold=new Color(.89f,.73f,.46f),Ivory=new Color(.95f,.925f,.865f);
  public RectTransform VisibleRoot=>IsOpen?root:null;
  public bool IsOpen=>root&&root.gameObject.activeSelf;
  public void Initialize(RoyalHUD h,ICommandGateway g){if(hud!=h){if(hud){hud.NarrativeOpening-=Close;hud.SelectionChanged-=SelectionChanged;}hud=h;if(hud){hud.NarrativeOpening+=Close;hud.SelectionChanged+=SelectionChanged;}}gateway=g;}
  void OnDestroy(){if(hud){hud.NarrativeOpening-=Close;hud.SelectionChanged-=SelectionChanged;}}
  void SelectionChanged(string id){Close();}
  public void Open(){if(gateway==null||!gateway.Snapshot().ChapterThree.RankUnlocked)return;Ensure();root.gameObject.SetActive(true);root.SetAsLastSibling();SynchronizeLayout();Refresh();}
  void LateUpdate(){if(IsOpen)root.SetAsLastSibling();}
  public void Close(){if(root)root.gameObject.SetActive(false);}
  void Update(){if(!IsOpen)return;if(Keyboard.current!=null&&Keyboard.current.escapeKey.wasPressedThisFrame){Close();return;}if(Mathf.Abs(root.rect.width-width)>.5f||Mathf.Abs(root.rect.height-height)>.5f)SynchronizeLayout();if(Time.unscaledTime>=next){next=Time.unscaledTime+.3f;Refresh();}}
  void Ensure(){if(root)return;
   root=RoyalGoldV3BuildingScreens.Panel("V3 / Realm rankings",hud.transform);RoyalGoldV3BuildingScreens.Stretch(root);root.GetComponent<Image>().color=new Color(.025f,.045f,.07f,1);
   var bg=RoyalGoldV3BuildingScreens.Rect("Royal parchment hall",root);RoyalGoldV3BuildingScreens.Stretch(bg);bg.gameObject.AddComponent<CanvasRenderer>();var ink=bg.gameObject.AddComponent<RoyalGoldV3ManagementBackdrop>();ink.color=Color.white;ink.raycastTarget=false;
   title=RoyalGoldV3BuildingScreens.Text("Title",root,30,Ivory);title.Put("CLASIFICACIONES DE VALORIA");subtitle=RoyalGoldV3BuildingScreens.Text("Simulated scope",root,15,Gold);subtitle.Put("PERFILES SIMULADOS · PROTOTIPO 4X");
   var names=new[]{"PODER TOTAL","CORRUPTOS","RELIQUIAS"};for(int i=0;i<3;i++){int n=i;tabs[i]=RoyalGoldV3BuildingScreens.Button(names[i],root,names[i],()=>{category=n;Refresh();},out _);}
   rival=RoyalGoldV3BuildingScreens.Text("Immediate rival",root,19,Gold);
   for(int i=0;i<rows.Length;i++){var card=RoyalGoldV3BuildingScreens.Panel("Rank "+(i+1),root);rows[i]=RoyalGoldV3BuildingScreens.Text("Row",card,20,Ivory);rows[i].alignment=TextAnchor.MiddleLeft;RoyalGoldV3BuildingScreens.Stretch((RectTransform)rows[i].transform);((RectTransform)rows[i].transform).offsetMin=new Vector2(20,0);((RectTransform)rows[i].transform).offsetMax=new Vector2(-20,0);}
   note=RoyalGoldV3BuildingScreens.Text("Scope explanation",root,15,Ivory);note.Put("Estos perfiles comparan progreso. No representan jugadores reales ni predicen resultados de combate.");back=RoyalGoldV3BuildingScreens.Button("Back",root,"VOLVER A LA GESTIÓN",Close,out _);
  }
  void Refresh(){if(!IsOpen)return;var s=gateway.Snapshot();int value=category==0?SliceRules.TotalPower(s).Total:category==1?(s.ScoutDefeated?1:0)+(s.EngendroDefeated?1:0)+(s.ChapterThree.FissureClosed?1:0)+s.WorldCreatureLocations.Where(x=>x.CreatureKind=="spawnling").Sum(x=>Math.Max(0,x.Generation-1)+(x.DefeatedUtcTicks>0?1:0)):0;
   var names=new[]{"Casa Varyn","Orden del Alba","Bastión Nareth","Lobos de Khar","Guardia Umbría","Dominio Aster","Vigías de Orun","Casa Serev","VALORIA · TÚ"};
   var offsets=category==0?new[]{2300,1100,420,-280,-760,-1500,-2600,-3900}:category==1?new[]{6,3,1,-1,-2,-3,-5,-7}:new[]{5,3,1,-1,-2,-3,-4,-6};
   var ladder=Enumerable.Range(0,9).Select(i=>new {Name=names[i],Value=i==8?value:Math.Max(category==0?100:0,value+offsets[i]),Me=i==8}).OrderByDescending(x=>x.Value).ToArray();int me=Array.FindIndex(ladder,x=>x.Me);var neighbor=ladder[me>0?me-1:1];rival.Put("RIVAL INMEDIATO  ·  "+neighbor.Name+"  ·  "+neighbor.Value.ToString("N0"));
   for(int i=0;i<rows.Length;i++){rows[i].Put("#"+(i+1)+"     "+ladder[i].Name+"     "+ladder[i].Value.ToString("N0"));rows[i].color=ladder[i].Me?Gold:Ivory;rows[i].transform.parent.GetComponent<Image>().color=ladder[i].Me?new Color(.19f,.16f,.10f,1):new Color(.065f,.105f,.14f,.96f);}
   for(int i=0;i<3;i++)tabs[i].interactable=i!=category;
  }
  public void SynchronizeLayout(){if(!IsOpen)return;root.SetAsLastSibling();Canvas.ForceUpdateCanvases();width=root.rect.width;height=root.rect.height;title.fontSize=width<650?22:30;float scale=Mathf.Max(.01f,hud.GetComponent<Canvas>().scaleFactor);var safe=Screen.safeArea;float inset=Mathf.Max(20,Mathf.Max(safe.xMin,Screen.width-safe.xMax)/scale+12);float top=Mathf.Max(25,(Screen.height-safe.yMax)/scale+12),bottom=Mathf.Max(18,safe.yMin/scale+8);float w=Mathf.Min(width-inset*2,1000),x=(width-w)*.5f;RoyalGoldV3BuildingScreens.Place((RectTransform)title.transform,x,top,w,44);RoyalGoldV3BuildingScreens.Place((RectTransform)subtitle.transform,x,top+47,w,26);
   for(int i=0;i<3;i++)RoyalGoldV3BuildingScreens.Place((RectTransform)tabs[i].transform,x+i*w/3,top+85,w/3-7,45);
   RoyalGoldV3BuildingScreens.Place((RectTransform)rival.transform,x,top+149,w,40);float row=Mathf.Min(65,(height-top-bottom-300)/9);
   for(int i=0;i<9;i++)RoyalGoldV3BuildingScreens.Place((RectTransform)rows[i].transform.parent,x,top+195+i*row,w,row-6);
   RoyalGoldV3BuildingScreens.Place((RectTransform)note.transform,x,height-bottom-94,w,45);RoyalGoldV3BuildingScreens.Place((RectTransform)back.transform,x,height-bottom-44,w,43);
  }
 }
}
