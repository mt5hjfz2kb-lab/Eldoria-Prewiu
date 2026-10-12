using UnityEngine;

namespace Eldoria
{
    /// <summary>
    /// Stable bridge between an authored Valoria mesh instance and future
    /// gameplay-backed selection. Data only: does not issue domain commands,
    /// invent resources or change Bastion progression.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ValoriaFirstZoneAnchor : MonoBehaviour
    {
        [SerializeField] string instanceId;
        [SerializeField] string sourceAssetId;
        [SerializeField] bool gameplayWired;

        public string InstanceId => instanceId;
        public string SourceAssetId => sourceAssetId;
        public bool GameplayWired => gameplayWired;

        public void ConfigureForIsolatedScene(string stableInstanceId,string sourceAsset)
        {
            instanceId=stableInstanceId;
            sourceAssetId=sourceAsset;
            gameplayWired=false; // Cannot claim interaction parity until domain mapping QA.
        }
    }
}
