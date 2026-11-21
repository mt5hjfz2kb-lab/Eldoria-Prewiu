using System;
using System.IO;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.Infrastructure;
using UnityEngine;

namespace Eldoria.ReferenceClean
{
    // Both scene hosts reopen the same sandbox on travel. No live presenter,
    // rendering object or production save crosses the scene boundary.
    public static class ValoriaTravelSession
    {
        public const string IsolatedSaveName = "eldoria-experiment-modular-25d-bastion1-v1.json";
        public const string CityScene = "BastionI_Modular25D";
        public const string RegionScene = "Valoria_City_World_UI";

        public static bool CombinedPlayer => Eldoria.UI.RoyalGold.V2.RoyalGoldV2Hud.UseCoronaArt;

        public static IStateStore OpenStore()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return new IsolatedWebGlPrefsStore();
#else
            return new FileStateStore(Path.Combine(UnityEngine.Application.persistentDataPath, IsolatedSaveName));
#endif
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

    }
}
