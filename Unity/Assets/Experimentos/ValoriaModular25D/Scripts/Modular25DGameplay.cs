using System;
using System.IO;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.Infrastructure;
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
        const string IsolatedSaveName = "eldoria-experiment-modular-25d-bastion1-v1.json";
        public const string SawmillId = "sawmill";
        ICommandGateway gateway;
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
                    return Path.Combine(UnityEngine.Application.persistentDataPath,
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
                IStateStore save;
#if UNITY_WEBGL && !UNITY_EDITOR
                save=new IsolatedWebGlPrefsStore();
#else
                save=new FileStateStore(SandboxSavePath);
#endif
                gateway=new LocalGateway(new SystemClock(),save);
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

        public CommandResult BuildSawmill()
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
                    "Build",SawmillId,before.Revision);
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

#if UNITY_WEBGL && !UNITY_EDITOR
        /// <summary>
        /// The exact WebGL storage principle already used by SliceBoot; this
        /// adapter only changes the SANDBOX key, never the economic rules.
        /// </summary>
        sealed class IsolatedWebGlPrefsStore : IStateStore
        {
            const string Key = "eldoria.experiment.modular25d.bastion1.v1";
            public PlayerState Load()
            {
                if(!PlayerPrefs.HasKey(Key))return null;
                string json=PlayerPrefs.GetString(Key,"");
                if(string.IsNullOrEmpty(json))return null;
                try
                {
                    var state=JsonUtility.FromJson<PlayerState>(json);
                    if(state==null||state.SchemaVersion!=1)
                        throw new InvalidDataException("Unknown sandbox save schema");
                    return state;
                }
                catch(Exception e)
                {
                    throw new InvalidDataException(
                        "Experimental WebGL save unreadable; original preserved",e);
                }
            }
            public void Save(PlayerState state)
            {
                PlayerPrefs.SetString(Key,JsonUtility.ToJson(state,true));
                PlayerPrefs.Save();
            }
        }
#endif

        void OnDestroy()
        {
            // Avoid leaking the owner's temporary profile into unrelated Editor
            // PlayMode scenarios when domain reload has been disabled.
            SliceContentProfiles.SetRuntimeProfileOverride(null);
        }
    }
}
