using System;
using System.IO;
using Eldoria.Application;
using Eldoria.ReferenceClean;
using Eldoria.Domain;
using Eldoria.Infrastructure;
using Eldoria.UI.RoyalGold;
using UnityEngine;

namespace Eldoria.Experimentos.ValoriaModular25D
{
    /// <summary>
    /// Thin presentation adapter to the canonical Eldoria command gateway.
    /// No invented wallet, production rate, timer, unlock rule or parallel save.
    /// A separate canonical IStateStore protects the player's existing beta.
    /// </summary>
    [DefaultExecutionOrder(-300)]
    public sealed class Modular25DGameplay : MonoBehaviour
    {
        const string IsolatedSaveName = ValoriaTravelSession.IsolatedSaveName;
        public const string SawmillId = "sawmill";
        ICommandGateway gateway;
        ExistingSliceBinding buildingOffers;
        long offerRevision=-1;
        readonly System.Collections.Generic.Dictionary<string,RoyalBuilding> offers =
            new System.Collections.Generic.Dictionary<string,RoyalBuilding>();

        internal ICommandGateway Commands => gateway;
        internal IRoyalSession Session => buildingOffers;

        public RoyalBuilding BuildingOffer(string selection, long revision)
        {
            if (revision!=offerRevision) { offers.Clear(); offerRevision=revision; }
            if (!offers.TryGetValue(selection,out var offer))
            {
                offer=buildingOffers.Read(selection,false).Building;
                offers[selection]=offer;
            }
            return offer;
        }

        Modular25DWorld world;
        PlayerState lastSnapshot;
        float nextTick;
        string feedback = "";
        public event Action<PlayerState> StateChanged;
        public event Action<string> MessageChanged;
        public bool IsAuthoritative => gateway != null;
        public string LastMessage => feedback;
        public string SandboxSavePath
        {
            get
            {
#if UNITY_EDITOR
                // Unique by Windows GitHub run ID. Editor PlayMode certification
                // must NEVER read/write the user's own sandbox or beta save.
                var nativeId=System.Environment.GetEnvironmentVariable("ELDORIA_MOD25D_PLAYMODE_RUN");
                if(!string.IsNullOrEmpty(nativeId))
                {
                    if(nativeId.Length<7||nativeId.Length>20)
                        throw new InvalidOperationException("Invalid isolated PlayMode run id");
                    foreach(var ch in nativeId)if(ch<'0'||ch>'9')
                        throw new InvalidOperationException("Unsafe isolated PlayMode run id");
                    // CI may supply a directory INSIDE its own isolated
                    // SHA-pinned scratch project. Never write to the actual
                    // player's AppData or beta. Local manual testing without
                    // that env still uses a distinct QA-only save filename.
                    var isolated=System.Environment.GetEnvironmentVariable(
                        "ELDORIA_MOD25D_PLAYMODE_OUTPUT");
                    var root=string.IsNullOrWhiteSpace(isolated)
                        ? UnityEngine.Application.persistentDataPath
                        : Path.GetFullPath(isolated);
                    if(!Directory.Exists(root))
                        throw new DirectoryNotFoundException("Missing QA-only save folder: "+root);
                    return Path.Combine(root,
                        "eldoria-experiment-modular25d-playmode-"+nativeId+".json");
                }
#endif
                return Path.Combine(UnityEngine.Application.persistentDataPath,IsolatedSaveName);
            }
        }

        void Awake()
        {
            world=GetComponent<Modular25DWorld>();
            try
            {
                // Select an existing, complete content profile, never splice QA_FAST
                // initial resources with owner-facing build costs and timers.
                SliceContentProfiles.SetRuntimeProfileOverride(SliceContentProfiles.OwnerIiiId);
                // The existing FileStateStore is authoritative on native targets.
                // In WebGL the canonical project uses PlayerPrefs.Save to flush
                // durable browser storage; the virtual filesystem does not
                // guarantee immediate persistence across a browser refresh.
                var clock=new SystemClock();
                var local = ValoriaTravelSession.CombinedPlayer
                    ? new LocalGateway(clock, OpenStore(), EldoriaV41GatewayResolver.Campaign())
                    : new LocalGateway(clock, OpenStore());
                if (!ValoriaTravelSession.CombinedPlayer)
                    local.ConfigureLocalResourceRespawn(new DistrictSupplies(), RenewablePopulationPolicy.Delay);
                gateway=local;
                buildingOffers=new ExistingSliceBinding(local,clock);
                lastSnapshot=gateway.Snapshot();
                world.Apply(lastSnapshot);
                SetMessage("Partida experimental aislada · mecánicas reales de Eldoria");
            }
            catch(Exception error)
            {
                // A damaged saved game is preserved; do not silently reset, reissue
                // resources, or replace it with fictitious gameplay.
                gateway=null;
                SetMessage("Guardado experimental inaccesible: "+error.Message);
                Debug.LogError("[ValoriaModular25D] Save/gateway unavailable: "+error);
            }
        }

        IStateStore OpenStore()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return ValoriaTravelSession.OpenStore();
#else
            return new FileStateStore(SandboxSavePath);
#endif
        }

        // Two gathering approaches per resource on the experimental map.
        // Population, output and depletion deadlines belong to the existing gateway.
        sealed class DistrictSupplies : IWorldResourcePlacement
        {
            public bool TryChoose(PlayerState state, string target, string previous, int generation,
                out WorldResourceLocation site)
            {
                bool forest = target == "forest-valoria";
                site = null;
                if (!forest && target != "quarry-valoria") return false;
                string first = target + "-west";
                bool west = previous != first;
                site = new WorldResourceLocation {
                    TargetId = target, SiteId = west ? first : target + "-east",
                    X = forest ? (west ? -3.2f : 2.4f) : (west ? -4.6f : -1.2f),
                    Z = forest ? -2.7f : Modular25DLandscape.StoneworksFoot.y
                };
                return true;
            }
        }

        void Start(){if(gateway!=null)StateChanged?.Invoke(lastSnapshot);}

        public PlayerState Snapshot()
        {
            return gateway!=null?gateway.Snapshot():null;
        }

        void Update()
        {
            if(gateway==null||Time.unscaledTime<nextTick)return;
            nextTick=Time.unscaledTime+.25f;
            try
            {
                bool changed=gateway.Advance();
                PlayerState state=gateway.Snapshot();
                if(changed||lastSnapshot==null||lastSnapshot.Revision!=state.Revision ||
                   lastSnapshot.SawmillLevel!=state.SawmillLevel ||
                   lastSnapshot.BuildingCompletesUtcTicks!=state.BuildingCompletesUtcTicks)
                {
                    lastSnapshot=state;
                    world.Apply(state);
                    StateChanged?.Invoke(state);
                }
            }
            catch(Exception error)
            {
                SetMessage("Error al actualizar estado: "+error.Message);
                Debug.LogError(error);
            }
        }

        public CommandResult BuildSawmill() => Issue("Build", SawmillId);

        public CommandResult Issue(string command, string target)
        {
            if(gateway==null)
            {
                SetMessage("El sistema de guardado no está disponible.");
                return new CommandResult(false,feedback,-1);
            }
            try
            {
                PlayerState before=gateway.Snapshot();
                // A fresh command ID is generated only by a physical user action,
                // never on a timer or UI refresh; the gateway handles idempotence.
                var action=new GameCommand(Guid.NewGuid().ToString("N"),before.PlayerId,
                    command,target,before.Revision);
                var result=gateway.Execute(action);
                SetMessage(result.Message);
                lastSnapshot=gateway.Snapshot();
                world.Apply(lastSnapshot);
                StateChanged?.Invoke(lastSnapshot);
                return result;
            }
            catch(Exception error)
            {
                SetMessage("Error en la construcción: "+error.Message);
                Debug.LogError(error);
                return new CommandResult(false,feedback,-1);
            }
        }

        public ParcelBuildingState SawmillStage()
        {
            var state=Snapshot();
            return state==null?ParcelBuildingState.NOT_BUILT:ParcelBuildingStates.For(state,SawmillId);
        }

        public long SawmillSecondsRemaining()
        {
            PlayerState s=Snapshot();
            if(s==null||!((s.BuildingTaskId??"").StartsWith("sawmill:",StringComparison.Ordinal)) ||
               s.BuildingCompletesUtcTicks<=0)return 0;
            return Math.Max(0,(long)Math.Ceiling(
                (s.BuildingCompletesUtcTicks-DateTime.UtcNow.Ticks)/(double)TimeSpan.TicksPerSecond));
        }

        void SetMessage(string text)
        {
            feedback=text??"";
            MessageChanged?.Invoke(feedback);
        }

        void OnApplicationPause(bool paused)
        {
            if(!paused&&gateway!=null)
            {
                try{gateway.Advance();lastSnapshot=gateway.Snapshot();world.Apply(lastSnapshot);StateChanged?.Invoke(lastSnapshot);}
                catch(Exception error){SetMessage("Estado inaccesible: "+error.Message);}
            }
        }

        void OnDestroy()
        {
            // Avoid leaking the owner's temporary profile into unrelated Editor
            // PlayMode scenarios when domain reload has been disabled.
            SliceContentProfiles.SetRuntimeProfileOverride(null);
        }
    }
}


