using System;
using Eldoria.Domain;

namespace Eldoria.Presentation
{
    // Read-only projection of authoritative LocalGateway tasks.
    public readonly struct ConstructionDelightTask
    {
        public readonly string BuildingId;
        public readonly string TaskKey;
        public readonly long DeadlineUtcTicks;
        public readonly int LevelAtStart;

        public ConstructionDelightTask(string id, string key, long deadline, int level)
        {
            BuildingId=id; TaskKey=key; DeadlineUtcTicks=deadline; LevelAtStart=level;
        }
        public bool IsValid => !string.IsNullOrEmpty(BuildingId) && !string.IsNullOrEmpty(TaskKey);
    }

    public static class ConstructionDelightState
    {
        public static bool TryRead(PlayerState state, out ConstructionDelightTask task)
        {
            task=default;
            if(state==null)return false;
            if(state.BastionUpgradeCompletesUtcTicks>0 &&
                !string.IsNullOrEmpty(state.BastionUpgradeTaskId))
            {
                task=new ConstructionDelightTask("bastion",state.BastionUpgradeTaskId,
                    state.BastionUpgradeCompletesUtcTicks,state.BastionLevel);
                return true;
            }
            if(state.BuildingCompletesUtcTicks<=0 || string.IsNullOrEmpty(state.BuildingTaskId))
                return false;
            string key=state.BuildingTaskId;
            string id=null;
            if(key.StartsWith("sawmill:",StringComparison.Ordinal))id="sawmill";
            else if(key.StartsWith("barracks:",StringComparison.Ordinal))id="barracks";
            else if(key.StartsWith("c3:",StringComparison.Ordinal))
            {
                int end=key.IndexOf(':',3);
                if(end>3)
                {
                    string candidate=key.Substring(3,end-3);
                    if(candidate=="sawmill"||candidate=="barracks"||
                       candidate=="granary"||candidate=="stoneworks")id=candidate;
                }
            }
            if(id==null)return false;
            task=new ConstructionDelightTask(id,key,state.BuildingCompletesUtcTicks,Level(state,id));
            return true;
        }

        public static int Level(PlayerState state,string id)
        {
            if(state==null)return 0;
            switch(id)
            {
                case "sawmill":return state.SawmillLevel;
                case "barracks":return state.BarracksLevel;
                case "bastion":return state.BastionLevel;
                case "granary":return state.ChapterThree?.GranaryLevel??0;
                case "stoneworks":return state.ChapterThree?.StoneworksLevel??0;
                default:return 0;
            }
        }

        public static bool SameTask(ConstructionDelightTask a,ConstructionDelightTask b)
            => a.IsValid && b.IsValid &&
               string.Equals(a.TaskKey,b.TaskKey,StringComparison.Ordinal) &&
               string.Equals(a.BuildingId,b.BuildingId,StringComparison.Ordinal);

        // Never celebrate a timer expiry, reload, cancellation or training.
        // Only the real gateway's completed level progression can trigger a reveal.
        public static bool JustCompleted(ConstructionDelightTask previous,PlayerState current)
        {
            if(!previous.IsValid||current==null)return false;
            if(TryRead(current,out var active)&&SameTask(previous,active))return false;
            // LocalGateway records every genuinely completed building task.
            // A changed level on its own could belong to another task, reload,
            // or a cancelled/replaced operation; never celebrate that.
            if(current.CompletedTaskIds==null ||
               !current.CompletedTaskIds.Contains(previous.TaskKey))return false;
            return Level(current,previous.BuildingId)>previous.LevelAtStart;
        }
    }
}
