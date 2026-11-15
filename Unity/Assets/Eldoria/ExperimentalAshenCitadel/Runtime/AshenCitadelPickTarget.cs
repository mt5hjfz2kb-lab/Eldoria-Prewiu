using UnityEngine;

namespace Eldoria.ExperimentalAshenCitadel
{
    // Scene-authored click target. Does not own gameplay or simulated rewards.
    // A target opens the respective gateway-backed UI or invokes an existing action.
    public sealed class AshenCitadelPickTarget : MonoBehaviour
    {
        public string DisplayName = "";
        public string TargetId = "";
        public string Kind = "";
        public void Configure(string displayName,string targetId,string kind="")
        {
            DisplayName=displayName; TargetId=targetId; Kind=kind;
        }
    }
}
