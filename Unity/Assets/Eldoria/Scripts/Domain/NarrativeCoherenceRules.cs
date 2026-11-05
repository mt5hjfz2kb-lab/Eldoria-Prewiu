using System;

namespace Eldoria.Domain
{
    /// <summary>
    /// Narrative reveal order derived from player save; no mutation or rewards.
    /// Lives separately from GameState to prevent parallel combat work conflicts.
    /// </summary>
    public static class NarrativeCoherenceRules
    {
        static bool Seen(PlayerState s, string beat) =>
            s.ChapterProgress?.SeenNarrative != null &&
            s.ChapterProgress.SeenNarrative.Contains(beat);

        public static int FissureKnowledgeStage(PlayerState s)
        {
            var c=s?.ChapterThree;
            if(c==null) return 0; // Uninvestigated: no character knowledge
            if(c.LyraContacted || c.FissureClosed) return 2; // Lyra met
            return c.FissureExplored ? 1 : 0; // Human tracks found
        }

        // Automatic story must not interrupt the player's current real screen.
        // Only presenter state is passed; no gameplay or save data is mutated.
        public static bool CanPresent(bool narrativeOpen,bool modalOpen,
            bool managementOpen,bool retentionOpen,bool mailboxOpen)
            => !narrativeOpen&&!modalOpen&&!managementOpen&&!retentionOpen&&!mailboxOpen;

        public static string NextUnseenBeat(PlayerState s)
        {
            if(s==null)return "";
            int level=s.BastionLevel;
            var p=s.ChapterProgress;
            var c=s.ChapterThree;
            // Never replay the entire opening in a mature legacy save.
            if(level==1 && !Seen(s,"start"))return "start";
            if(level<=2)
            {
                if(s.SawmillLevel>0&&!Seen(s,"world"))return "world";
                if((p?.GatheredWood??0)>0&&!Seen(s,"mark"))return "mark";
                if(level==2&&!Seen(s,"barracks"))return "barracks";
                if(level==2&&(s.EngendroDefeated||p?.EngendroDefeated==true)&&!Seen(s,"secured"))return "secured";
            }
            if(level==3&&c?.FissureClosed!=true&&!Seen(s,"frontier-open"))return "frontier-open";
            // The conclusion of Lyra's story must precede the next Bastion's
            // council even if a saved game advances several states offline.
            if(level>=3&&c?.FissureClosed==true&&!Seen(s,"fissure-closed"))return "fissure-closed";
            if(level==4&&!Seen(s,"consolidate-valoria"))return "consolidate-valoria";
            if(level>=5)
            {
                if(!Seen(s,"productive-valoria"))return "productive-valoria";
                if(c?.ExtractionOddity==true&&!Seen(s,"extraction-oddity"))return "extraction-oddity";
            }
            return "";
        }

        public static string RecapBeat(PlayerState s)
        {
            if(s==null)return "start";
            if(s.BastionLevel>=5)return "productive-valoria";
            if(s.BastionLevel==4)return "consolidate-valoria";
            if(s.BastionLevel==3)return s.ChapterThree?.FissureClosed==true?"fissure-closed":"frontier-open";
            if(s.BastionLevel==2)return s.EngendroDefeated?"secured":"barracks";
            return (s.ChapterProgress?.GatheredWood??0)>0?"mark":s.SawmillLevel>0?"world":"start";
        }
    }
}
