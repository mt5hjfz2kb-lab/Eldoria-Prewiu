using UnityEngine;
using Eldoria.UI.RoyalGold;

namespace Eldoria.UI.RoyalGold.V2
{
    /// <summary>
    /// Owner-approved V3 presentation tokens for the existing Royal Gold HUD.
    /// Never changes narrative state, gameplay, currency or button actions.
    /// Default is clean royal; supernatural visuals require a real event trigger.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EldoriaV3VisualDirection : MonoBehaviour
    {
        public enum NarrativeTreatment { Royal, Fisura, Orin }
        [SerializeField] private NarrativeTreatment treatment = NarrativeTreatment.Royal;
        [SerializeField] private RoyalGoldV2FrameGraphic[] contextualFrames;
        [SerializeField] private RoyalText[] contextualText;

        public NarrativeTreatment ActiveTreatment => treatment;

        private static readonly Color RoyalInk = new Color(.95f, .92f, .86f);
        private static readonly Color FisuraText = new Color(.87f, .79f, .98f);
        private static readonly Color OrinText = new Color(.91f, .85f, .97f);

        // Must be called only by a validated canonical narrative/event adapter.
        // The HUD itself must never infer a story event from the scene's color.
        public void ApplyFromCanonicalEvent(NarrativeTreatment next)
        {
            treatment = next;
            Refresh();
        }

        public void RestoreRoyal() => ApplyFromCanonicalEvent(NarrativeTreatment.Royal);

        private void OnEnable() => Refresh();
        private void OnDisable() => RestoreRoyal();

        private void Refresh()
        {
            bool altered = treatment != NarrativeTreatment.Royal;
            if (contextualFrames != null)
                foreach (var frame in contextualFrames)
                {
                    if (!frame) continue;
                    frame.color = altered
                        ? new Color(.83f, .72f, 1f, 1f)
                        : Color.white;
                    frame.SetVerticesDirty();
                }
            if (contextualText != null)
                foreach (var label in contextualText)
                {
                    if (!label) continue;
                    label.color = treatment == NarrativeTreatment.Fisura ? FisuraText :
                        treatment == NarrativeTreatment.Orin ? OrinText : RoyalInk;
                }
        }
    }
}
