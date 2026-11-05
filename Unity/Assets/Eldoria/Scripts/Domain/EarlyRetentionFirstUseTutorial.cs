using Eldoria.Domain;

namespace Eldoria.Domain
{
    // Zero additional save flags: lessons end when the player genuinely wins
    // their first Vigilant stage or claims a real Scout report.
    // No modal interruption, no fake tasks, no reward side effects.
    public enum FirstUseTutorialPhase { Locked, FirstAction, AwaitingResult, Learned }
    public static class EarlyRetentionFirstUseTutorial
    {
        public static FirstUseTutorialPhase Vigilants(PlayerState state)
        {
            if(!EarlyRetentionRules.VigilantsUnlocked(state))return FirstUseTutorialPhase.Locked;
            return (state.Retention?.LastClearedStage??0)>0?
                FirstUseTutorialPhase.Learned:FirstUseTutorialPhase.FirstAction;
        }

        public static FirstUseTutorialPhase Explorers(PlayerState state)
        {
            if(!EarlyRetentionRules.ScoutsUnlocked(state))return FirstUseTutorialPhase.Locked;
            var retention=state.Retention;
            if(EarlyRetentionRules.RevealedScoutReports(retention)>0)
                return FirstUseTutorialPhase.Learned;
            if(retention?.Orders!=null)
            {
                foreach(var order in retention.Orders)
                    if(order!=null&&!order.Claimed&&order.StartedUtcTicks>0)
                        return FirstUseTutorialPhase.AwaitingResult;
            }
            return FirstUseTutorialPhase.FirstAction;
        }
    }
}
