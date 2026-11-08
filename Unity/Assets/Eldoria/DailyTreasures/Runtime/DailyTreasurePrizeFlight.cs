using System;

namespace Eldoria.DailyTreasures
{
    /// <summary>
    /// Source-art independent flight choreography for the real credited
    /// reward cards. No Unity engine, player state, loot or duplicate grant.
    /// The same function can be audited on Ubuntu and rendered in Unity.
    /// </summary>
    public readonly struct DailyTreasurePrizeFlightFrame
    {
        public readonly float X,Y,Scale,Opacity,Angle;
        public readonly bool Finished;
        public DailyTreasurePrizeFlightFrame(float x,float y,float scale,
            float opacity,float angle,bool finished)
        {
            X=x;Y=y;Scale=scale;Opacity=opacity;Angle=angle;
            Finished=finished;
        }
    }

    public static class DailyTreasurePrizeFlight
    {
        public const float QuietDuration=.34f;
        public const float ChestDuration=.64f;
        public const float DelayBetweenCards=.13f;

        static float Clamp01(float x)=>x<0f?0f:x>1f?1f:x;

        public static DailyTreasurePrizeFlightFrame Sample(float elapsed,
            int index,bool chest,float x0,float y0,float x1,float y1)
        {
            if(index<0||index>=DailyTreasureRewardPresentation.MaxSimultaneousItems)
                throw new ArgumentOutOfRangeException(nameof(index));
            if(float.IsNaN(elapsed)||float.IsInfinity(elapsed))
                throw new ArgumentOutOfRangeException(nameof(elapsed));

            float duration=chest?ChestDuration:QuietDuration;
            float t=Clamp01((elapsed-index*DelayBetweenCards)/duration);
            float ease=t*t*(3f-2f*t);
            float fromX=chest?x0:x1+(index%2==0?-20f:20f);
            float fromY=chest?y0:y1+18f;
            float toX=fromX+(x1-fromX)*ease;
            float toY=fromY+(y1-fromY)*ease+
                (float)Math.Sin(Math.PI*ease)*(chest?18f:4f);
            float opacity=Clamp01(t*2.3f);
            float scale=(chest?.52f:.91f)+
                (chest?.48f:.09f)*ease;
            float angle=chest?(index%2==0?-9f:9f)*(1f-ease):0f;
            if(t>=1f)return new DailyTreasurePrizeFlightFrame(
                x1,y1,1f,1f,0f,true);
            return new DailyTreasurePrizeFlightFrame(
                toX,toY,scale,opacity,angle,false);
        }
    }
}
