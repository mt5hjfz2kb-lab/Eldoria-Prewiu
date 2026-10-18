using System;
using Eldoria.Application;
using Eldoria.Domain;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
namespace Eldoria.UI.RoyalGold.Preview {
 /// <summary>Only referenced by RoyalGold_UI_TEST.unity. Memory fixture, no canonical save or global profile change.</summary>
 public sealed class RoyalPreviewDriver:MonoBehaviour {
  public RoyalHUD Hud;public RoyalButton Normal,Locked,Busy,Error,Sawmill,Barracks,Bastion,March;
  LocalGateway gateway;readonly TestClock clock=new TestClock();float next;
  sealed class TestClock:IClock{public long UtcTicks=>DateTime.UtcNow.Ticks;}
  sealed class TestMemory:IStateStore{PlayerState state;public TestMemory(PlayerState seed){state=seed;}public PlayerState Load()=>state;public void Save(PlayerState next){state=next;}}
  sealed class SimulatedFailure:IRoyalSession{readonly IRoyalSession data;public SimulatedFailure(IRoyalSession data){this.data=data;}public RoyalSnapshot Read(string id,bool en)=>data.Read(id,en);public RoyalResult Submit(RoyalAction a)=>new RoyalResult(false,"TEST · Error simulado; ningún comando enviado / Simulated error; no command sent");}
  void Awake(){
   if(!EventSystem.current){var o=new GameObject("UI TEST EventSystem");o.transform.SetParent(transform,false);o.AddComponent<EventSystem>();o.AddComponent<InputSystemUIInputModule>();}
   if(!Camera.main){var o=new GameObject("UI TEST backdrop camera");o.transform.SetParent(transform,false);var c=o.AddComponent<Camera>();c.tag="MainCamera";c.clearFlags=CameraClearFlags.SolidColor;c.backgroundColor=new Color(.025f,.038f,.065f);c.cullingMask=0;}
  }
  void OnEnable(){Normal.onClick.AddListener(SetNormal);Locked.onClick.AddListener(SetLocked);Busy.onClick.AddListener(SetBusy);Error.onClick.AddListener(SetError);Sawmill.onClick.AddListener(SelectSawmill);Barracks.onClick.AddListener(SelectBarracks);Bastion.onClick.AddListener(SelectBastion);March.onClick.AddListener(SelectMarch);}
  void OnDisable(){Normal.onClick.RemoveListener(SetNormal);Locked.onClick.RemoveListener(SetLocked);Busy.onClick.RemoveListener(SetBusy);Error.onClick.RemoveListener(SetError);Sawmill.onClick.RemoveListener(SelectSawmill);Barracks.onClick.RemoveListener(SelectBarracks);Bastion.onClick.RemoveListener(SelectBastion);March.onClick.RemoveListener(SelectMarch);}
  void Start(){SetNormal();}
  void Update(){if(gateway!=null&&Time.unscaledTime>=next){next=Time.unscaledTime+.20f;gateway.Advance();}}
  void ResetFixture(int mode){
   var s=new PlayerState{Resources=new ResourceWallet{Wood=900,Stone=600,Food=120}};
   if(mode==1)s.Resources=new ResourceWallet();
   gateway=new LocalGateway(clock,new TestMemory(s));
   if(mode==2){var before=gateway.Snapshot();gateway.Execute(new GameCommand("test:busy",before.PlayerId,"Build","sawmill",before.Revision));}
   IRoyalSession binding=new ExistingSliceBinding(gateway,clock,true);
   if(mode==3)binding=new SimulatedFailure(binding);
   Hud.Modal.Hide();Hud.Feedback.Put("TEST · "+new[]{"Normal","Bloqueado / Locked","En curso / In progress","Error simulado / Simulated error"}[mode]);Hud.Bind(binding);Hud.SelectBuilding("sawmill");
  }
  public void SetNormal(){ResetFixture(0);}public void SetLocked(){ResetFixture(1);}public void SetBusy(){ResetFixture(2);}public void SetError(){ResetFixture(3);}
  void SelectSawmill(){Hud.SelectBuilding("sawmill");}void SelectBarracks(){Hud.SelectBuilding("barracks");}void SelectBastion(){Hud.SelectBuilding("bastion");}void SelectMarch(){Hud.SelectBuilding("march-main");}
 }
}
