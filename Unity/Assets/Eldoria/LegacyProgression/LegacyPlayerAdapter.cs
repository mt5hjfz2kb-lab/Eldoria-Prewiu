using System;
using Eldoria.Domain;
using Eldoria.Application;

namespace Eldoria.LegacyProgression
{
    // Read the existing authoritative snapshot; never mutate or maintain a second save.
    // A future approved HUD can call Read on the same ICommandGateway it already owns.
    public static class LegacyPlayerAdapter
    {
        public static LocalProfile Read(ICommandGateway gateway)
        {
            if(gateway==null)throw new ArgumentNullException(nameof(gateway));
            return Project(gateway.Snapshot());
        }

        public static LocalProfile Project(PlayerState s)
        {
            if(s==null)throw new ArgumentNullException(nameof(s));
            var power=SliceRules.TotalPower(s);
            var c=s.ChapterThree;
            var r=s.Retention;
            return new LocalProfile(s.PlayerName,new ProgressFacts{
                Bastion=s.BastionLevel,TotalPower=power.Total,KingdomPower=power.Kingdom,
                ArmyPower=power.Army,VigilantStage=r?.LastClearedStage??0,
                ScoutReports=r?.TotalScoutReports??0,DiscoveryMask=r?.ScoutDiscoveryMask??0,
                RecordedHunts=c?.Hunts??0,JourneyComplete=s.JourneyComplete,
                RouteCleared=s.ChapterProgress?.RouteCleared??false,
                EngendroDefeated=s.EngendroDefeated||s.ChapterProgress?.EngendroDefeated==true,
                LyraContacted=c?.LyraContacted??false,FissureExplored=c?.FissureExplored??false,
                FissureClosed=c?.FissureClosed??false,LyraRecruited=c?.LyraRecruited??false,
                AshStalkerDefeated=c?.AshStalkerDefeated??false
            });
        }
    }
}
