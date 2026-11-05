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
            bool managementOpen,bool retentionOpen,bool mailboxOpen,bool contextSelected=false)
            => !narrativeOpen&&!modalOpen&&!managementOpen&&!retentionOpen&&!mailboxOpen&&!contextSelected;

        public static string NextUnseenBeat(PlayerState s, bool loadedExistingSave=false, Func<string,bool> legacyRead=null)
        {
            if(s==null)return "";
            // Skip legacy acknowledgements while selecting, not after selecting:
            // otherwise the first old beat blocks every later unread scene forever.
            Func<string,bool> seen=beat=>Seen(s,beat)||(loadedExistingSave&&legacyRead!=null&&legacyRead(beat));
            int level=s.BastionLevel;
            var p=s.ChapterProgress;
            var c=s.ChapterThree;
            // Never replay the entire opening in a mature legacy save.
            if(level==1 && !seen("start"))return "start";
            if(level<=2)
            {
                if(s.SawmillLevel>0&&!seen("world"))return "world";
                if((p?.GatheredWood??0)>0&&!seen("mark"))return "mark";
                if(level==2&&!seen("barracks"))return "barracks";
                if(level==2&&(s.EngendroDefeated||p?.EngendroDefeated==true)&&!seen("secured"))return "secured";
            }
            if(level==3&&c?.FissureClosed!=true&&!seen("frontier-open"))return "frontier-open";
            // The conclusion of Lyra's story must precede the next Bastion's
            // council even if a saved game advances several states offline.
            if(level>=3&&c?.FissureClosed==true&&!seen("fissure-closed"))return "fissure-closed";
            if(level==4&&!seen("consolidate-valoria"))return "consolidate-valoria";
            if(level>=5)
            {
                if(!seen("productive-valoria"))return "productive-valoria";
                if(c?.ExtractionOddity==true&&!seen("extraction-oddity"))return "extraction-oddity";
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

