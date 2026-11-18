using System;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.Presentation;
using Eldoria.Experimentos.Ciudad2D;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace Eldoria.Valoria25D
{
    /// <summary>
    /// Only the visual composition root. It owns no economy or construction rule.
    /// Real state and persistence belong exclusively to SliceBoot's LocalGateway.
    /// </summary>
    [DefaultExecutionOrder(-500)]
    [DisallowMultipleComponent]
    public sealed class City25DPlayableController : MonoBehaviour
    {
        private ICommandGateway gateway;
        private City2DStateBridge bridge;
        private City25DMinimalHud hud;
        private float nextPoll;
        private int lastSendFrame=-1;
        private bool executing;
        private string selectedId="bastion";
        private string lastFeedback="";
        private string initializationError="";
        public ICommandGateway Gateway => gateway;
        public City2DStateBridge Bridge => bridge;
        public string SelectedId => selectedId;
        public string LastFeedback => lastFeedback;
        public bool Ready => gateway!=null && bridge!=null && hud!=null;
        public bool IsPureComposition => true;

        private void Awake()
        {
            // Never permit two composition roots to issue duplicate commands.
            if(FindObjectsByType<City25DPlayableController>(FindObjectsSortMode.None).Length!=1)
                throw new InvalidOperationException("Multiple 2.5D gameplay compositions");
            bridge=FindFirstObjectByType<City2DStateBridge>();
            if(bridge==null)throw new InvalidOperationException("Imported premium V6 bridge missing");
            if(bridge.IsLiveBound)throw new InvalidOperationException("Bridge already bound by another gateway");
            var pilots=FindObjectsByType<City2DPilotHud>(FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            foreach(var item in pilots)
            {
                item.enabled=false;
                Destroy(item.gameObject); // removes art-demo HUD before its Start()
            }
            if(FindFirstObjectByType<SlicePresenter>()!=null)
                throw new InvalidOperationException("Legacy SlicePresenter would duplicate HUD/commands");
            if(FindObjectsByType<Camera>(FindObjectsSortMode.None).Length!=1 ||
               FindObjectsByType<City2DCameraFit>(FindObjectsSortMode.None).Length!=1)
                throw new InvalidOperationException("Must reuse the one V6 camera");
            if(FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length!=1 ||
               FindObjectsByType<InputSystemUIInputModule>(FindObjectsSortMode.None).Length!=1)
                throw new InvalidOperationException("Must reuse the one V6 EventSystem/InputSystem");
            try
            {
                gateway=SliceBoot.OpenCity25DGateway();
                gateway.Advance(); // Complete due tasks/production using canonical offline rules.
                bridge.BindGateway(gateway);
                if(!bridge.IsLiveBound||bridge.RegisteredBuildingCount!=6)
                    throw new InvalidOperationException("City bridge did not attach all six real plots");
                bridge.BuildingSelected+=OnSelected;
                Debug.Log("VALORIA25D_GATEWAY_BOUND_CANONICAL no_demo_progress=true");
            }
            catch(Exception e)
            {
                initializationError=e.Message;
                Debug.LogError("VALORIA25D_PERSISTED_STATE_LOAD_FAILED_UNCHANGED "+e);
                // Never silently reset/corrupt a persisted save or grant test resources.
            }
        }

        private void Start()
        {
            var go=new GameObject("VALORIA25D_UniqueMinimalHud");
            hud=go.AddComponent<City25DMinimalHud>();
            hud.Configure(this);
            if(!string.IsNullOrEmpty(initializationError))
                hud.SetFeedback("No se pudo cargar la partida: "+initializationError,true);
            else
            {
                Refresh();
                Debug.Log("VALORIA25D_PLAYABLE_READY source=PremiumV6 "+
                          "gateway=LocalGateway owner_save=canonical");
            }
        }

        private void Update()
        {
            if(gateway==null || Time.unscaledTime<nextPoll)return;
            nextPoll=Time.unscaledTime+.50f;
            try
            {
                gateway.Advance();  // Includes actual offline production, recruitment and timers.
                Refresh();
            }
            catch(Exception ex)
            {
                Debug.LogError("VALORIA25D_REAL_ADVANCE_ERROR "+ex);
                hud?.SetFeedback("Error real de actualización: "+ex.Message,true);
                nextPoll=Time.unscaledTime+2f;
            }
        }

        private void OnSelected(CityBuilding building)
        {
            if(building==null || gateway==null)return;
            selectedId=building.BuildingId;
            Refresh();
            hud?.OpenPanel();
            Debug.Log("VALORIA25D_REAL_BUILDING_SELECTED visual_id="+selectedId);
        }
        public void Select(string id)
        {
            if(string.IsNullOrEmpty(id)||gateway==null)return;
            selectedId=id;
            Refresh();
            hud?.OpenPanel();
        }
        public PlayerState CurrentSnapshot()
        {
            return gateway?.Snapshot();
        }
        public void Refresh()
        {
            if(gateway==null||bridge==null)return;
            var state=gateway.Snapshot();
            bridge.ApplySnapshot(state); // NEVER invoke SetDemoBastionLevel for live players.
            hud?.ApplySnapshot(state,selectedId,DateTime.UtcNow.Ticks);
        }

        /// <summary>Exactly one canonical command per user press. Public for integration proof.</summary>
        public CommandResult ExecuteOffer(City25DActionCatalog.Offer offer)
        {
            if(gateway==null||!offer.CanAttempt)
                return new CommandResult(false,"No hay una acción real disponible",0);
            if(executing||lastSendFrame==Time.frameCount)
                return new CommandResult(false,"La acción ya se está procesando",gateway.Snapshot().Revision);
            lastSendFrame=Time.frameCount;
            executing=true;
            try
            {
                // Use a fresh snapshot after advancing timers. No hand-authored resource mutation.
                gateway.Advance();
                var state=gateway.Snapshot();
                var command=new GameCommand(Guid.NewGuid().ToString("N"),state.PlayerId,
                    offer.Kind,offer.Target,state.Revision);
                CommandResult result=gateway.Execute(command);
                lastFeedback=result.Message;
                Debug.Log("VALORIA25D_REAL_COMMAND kind="+offer.Kind+
                    " target="+offer.Target+" accepted="+result.Ok+
                    " revision="+result.Revision+" message="+result.Message);
                Refresh();
                hud?.SetFeedback(result.Message,!result.Ok);
                return result;
            }
            catch(Exception ex)
            {
                lastFeedback="Error real: "+ex.Message;
                Debug.LogError("VALORIA25D_COMMAND_EXCEPTION "+ex);
                hud?.SetFeedback(lastFeedback,true);
                return new CommandResult(false,lastFeedback,gateway.Snapshot().Revision);
            }
            finally{ executing=false; }
        }

        public void RunPrimary()
        {
            if(gateway==null)return;
            var offer=City25DActionCatalog.Building(gateway.Snapshot(),selectedId,
                DateTime.UtcNow.Ticks);
            if(!offer.CanAttempt)
            {
                hud?.SetFeedback(string.IsNullOrEmpty(offer.Requirements)
                    ?offer.Description:offer.Requirements,true);
                return;
            }
            ExecuteOffer(offer);
        }
        public void RunRecruit()
        {
            if(gateway==null)return;
            var offer=City25DActionCatalog.Recruit(gateway.Snapshot(),
                DateTime.UtcNow.Ticks);
            if(!offer.CanAttempt)
            {
                hud?.SetFeedback(offer.Description,true);
                return;
            }
            ExecuteOffer(offer);
        }
        public void ShowWorldEntry()
        {
            if (gateway == null)
            {
                hud?.SetFeedback("La partida real no está disponible",true);
                return;
            }
            // Reuse the CURRENT canonical gateway in the existing Frontier scene.
            // No simulation, world reconstruction, new rewards or altered rules.
            if (!City25DWorldTransition.TryEnter(gateway,out var reason))
                hud?.SetFeedback(reason,true);
        }
        private void OnDestroy()
        {
            if(bridge!=null)bridge.BuildingSelected-=OnSelected;
        }
    }
}
