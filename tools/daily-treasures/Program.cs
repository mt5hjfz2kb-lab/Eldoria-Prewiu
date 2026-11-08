using System;
using Eldoria.Domain;
using Eldoria.DailyTreasures;
internal static class Program
{
    static int checks;
    static void Eq<T>(T actual,T expected,string name)
    {
        checks++;
        if(!object.Equals(actual,expected))
            throw new InvalidOperationException(name+": expected "+expected+", got "+actual);
    }
    static void True(bool value,string name) => Eq(value,true,name);
    static long Date(int day,int hour=12,int minute=0)
        => new DateTime(2026,11,day,hour,minute,0,DateTimeKind.Utc).Ticks;
    static void Main()
    {
        var s=new DailyTreasureState();
        Eq(s.CycleDay,0,"new calendar");
        True(DailyTreasureRules.CanClaim(s,Date(8)),"first day available");
        int[] wood={60,0,0,90,0,0,180};
        int[] stone={0,40,0,60,0,80,120};
        int[] food={0,0,25,0,0,125,80};
        for(int day=1;day<=7;day++)
        {
            long clock=Date(7+day);
            var preview=DailyTreasureRules.Preview(s,clock);
            Eq(preview.CycleDay,day,"preview cycle day "+day);
            Eq(preview.Wood,wood[day-1],"wood "+day);
            Eq(preview.Stone,stone[day-1],"stone "+day);
            Eq(preview.Food,food[day-1],"food "+day);
            Eq(preview.ShowChest,day==4 || day==7,"ceremony tier "+day);
            True(DailyTreasureRules.TryClaim(s,clock,out var award),"claim "+day);
            Eq(award.CycleDay,day,"awarded day "+day);
            Eq(s.ConsecutiveDays,day,"streak "+day);
            Eq(s.TotalClaims,day,"total "+day);
            True(!DailyTreasureRules.TryClaim(s,clock,out _),"no second claim "+day);
            Eq(DailyTreasureRules.Preview(s,clock).IsValid,false,"no second preview "+day);
        }
        True(DailyTreasureRules.TryClaim(s,Date(15),out var eighth),"cycle rollover");
        Eq(eighth.CycleDay,1,"cycle restarts after seven");
        Eq(s.ConsecutiveDays,8,"streak continues across cycle");
        Eq(eighth.Wood,60,"day one reward unchanged");
        var copy=s.Copy();
        copy.CycleDay=6;
        Eq(s.CycleDay,1,"snapshot is independent");
        True(!DailyTreasureRules.TryClaim(s,Date(14),out _),"clock rollback safe");
        var skipped=new DailyTreasureState();
        True(DailyTreasureRules.TryClaim(skipped,Date(8),out _),"skip test day1");
        True(DailyTreasureRules.TryClaim(skipped,Date(11),out var second),"skip test day2");
        Eq(second.CycleDay,2,"missing days do not lose calendar progress");
        Eq(second.Streak,1,"missing days reset streak only");
        True(DailyTreasureRules.TryClaim(skipped,Date(12),out var third),"streak increment");
        Eq(third.CycleDay,3,"third claim");
        Eq(third.BonusFood,0,"no false bonus after missed days");
        var boundary=new DailyTreasureState();
        True(DailyTreasureRules.TryClaim(boundary,Date(8,23,59),out _),"near UTC midnight");
        True(!DailyTreasureRules.TryClaim(boundary,Date(8,23,59),out _),"same UTC date");
        True(DailyTreasureRules.TryClaim(boundary,Date(9,0,0),out _),"new UTC date");
        Eq(DailyTreasureRules.ForDay(3,3).BonusFood,25,"three-day bonus");
        Eq(DailyTreasureRules.ForDay(7,7).BonusFood,80,"seven-day bonus");
        Eq(DailyTreasureRules.ForDay(7,6).BonusFood,25,"six-day mini-bonus");
        True(DailyTreasureRules.Receipt(DailyTreasureRules.ForDay(7,7)).Contains("5 min"),
            "accurate acceleration receipt");
        True(!DailyTreasureRules.TryClaim(null,Date(8),out _),"null save safe");
        var extreme=new DailyTreasureState{
            CycleDay=3,LastClaimUtcDay=DailyTreasureRules.UtcDay(Date(8)),
            ConsecutiveDays=int.MaxValue,TotalClaims=1000
        };
        var extremePreview=DailyTreasureRules.Preview(extreme,Date(9));
        Eq(extremePreview.Streak,1000000,"extreme streak saturates safely");
        True(DailyTreasureRules.TryClaim(extreme,Date(9),out _),
            "old invalid high streak must not wrap to negative");
        Eq(extreme.ConsecutiveDays,1000000,"saturated consecutive days are saved");
        var exhausted=new DailyTreasureState{
            CycleDay=6,ConsecutiveDays=6,TotalClaims=int.MaxValue,
            LastClaimUtcDay=DailyTreasureRules.UtcDay(Date(8))
        };
        True(!DailyTreasureRules.CanClaim(exhausted,Date(9)),
            "fully exhausted total-claims counter never offers a false button");
        Eq(DailyTreasureRules.Preview(exhausted,Date(9)).IsValid,false,
            "exhausted streak preview is invalid too");
        var initial=DailyTreasureRevealTimeline.Sample(0f);
        Eq(initial.Closed,1f,"chest starts fully closed");
        Eq(initial.Opened,0f,"open image initially invisible");
        for(int i=0;i<=500;i++)
        {
            float time=i*DailyTreasureRevealTimeline.DurationSeconds/500f;
            var frame=DailyTreasureRevealTimeline.Sample(time);
            True(frame.Closed>=0f && frame.Closed<=1f,"closed alpha bounded "+i);
            True(frame.Opened>=0f && frame.Opened<=1f,"opened alpha bounded "+i);
            True(Math.Abs(frame.Closed+frame.Opened-1f)<.0001f,
                "two original images form a single opaque silhouette "+i);
            True(frame.Glow>=0f && frame.Glow<=1f,"warm flash bounded "+i);
            True(frame.Embers>=0f && frame.Embers<=1f,"ember alpha bounded "+i);
            True(frame.Scale>=.95f && frame.Scale<=1.04f,"subtle premium scale "+i);
            True(Math.Abs(frame.Shake)<=.007f,"controlled hinge shake "+i);
            True(frame.Text>=0f && frame.Text<=1f,"caption visibility bounded "+i);
        }
        var peak=DailyTreasureRevealTimeline.Sample(
            DailyTreasureRevealTimeline.RevealPeakSeconds);
        True(peak.Glow>.35f,"flash genuinely disguises image pose transition");
        True(peak.Closed>.1f && peak.Opened>.1f,
            "pose transition takes place under peak flash");
        var final=DailyTreasureRevealTimeline.Sample(
            DailyTreasureRevealTimeline.DurationSeconds+.1f);
        True(final.Finished && final.Opened==1f && final.Closed==0f,
            "duration completed with one open asset");
        Eq(DailyTreasureRules.NextConsecutiveBonusDay(0),3,
            "new account sees first three-day loyalty target");
        Eq(DailyTreasureRules.NextConsecutiveBonusDay(2),3,
            "two-day streak previews bonus tomorrow");
        Eq(DailyTreasureRules.NextConsecutiveBonusDay(3),6,
            "after first bonus the next is day six");
        Eq(DailyTreasureRules.NextConsecutiveBonusDay(6),7,
            "seventh consecutive day gets the special loyalty reward");
        Eq(DailyTreasureRules.NextConsecutiveBonusDay(7),9,
            "loyalty milestones continue across reward cycles");
        Eq(DailyTreasureRules.NextConsecutiveBonusDay(13),14,
            "fourteen-day consecutive milestone remains special");
        Eq(DailyTreasureRules.BonusFoodForStreak(3),25,
            "three-day extra food");
        Eq(DailyTreasureRules.BonusFoodForStreak(7),80,
            "seventh-day extra food");
        Eq(DailyTreasureRules.BonusFoodForStreak(14),80,
            "next week loyalty stays limited");
        for(int i=0;i<=500;i++)
        {
            float t=i*DailyTreasureRevealTimeline.DurationSeconds/500f;
            var quiet=DailyTreasureRevealTimeline.Sample(
                t,DailyTreasureCeremonyTier.Quiet);
            var special=DailyTreasureRevealTimeline.Sample(
                t,DailyTreasureCeremonyTier.Special);
            var legendary=DailyTreasureRevealTimeline.Sample(
                t,DailyTreasureCeremonyTier.Legendary);
            True(quiet.Glow<=special.Glow+0.0001f &&
                 special.Glow<=legendary.Glow+0.0001f,
                 "legendary lighting must never be weaker "+i);
            True(quiet.Embers<=special.Embers+0.0001f &&
                 special.Embers<=legendary.Embers+0.0001f,
                 "legendary embers must never be weaker "+i);
            True(legendary.Scale>=special.Scale &&
                 legendary.Scale<=1.05f,
                 "rare ceremony stays within premium mobile scale "+i);
            True(legendary.Closed==special.Closed &&
                 legendary.Opened==special.Opened,
                 "different tiers must not make inconsistent lid poses "+i);
            True(legendary.Finished==special.Finished &&
                 quiet.Finished==special.Finished,
                 "all reward ceremonies must finish at the same duration "+i);
        }
        var normalPeak=DailyTreasureRevealTimeline.Sample(
            DailyTreasureRevealTimeline.RevealPeakSeconds,
            DailyTreasureCeremonyTier.Special);
        var grandPeak=DailyTreasureRevealTimeline.Sample(
            DailyTreasureRevealTimeline.RevealPeakSeconds,
            DailyTreasureCeremonyTier.Legendary);
        True(grandPeak.Glow>normalPeak.Glow,
            "real seventh-day grand coffer gets a distinct cinematic flash");
        // The luxurious reward cards must show what the gateway actually
        // credited, not a different visual-only loot table.
        int[] expectedCards={1,1,2,2,1,2,4};
        for(int day=1;day<=7;day++)
        {
            var received=DailyTreasureRules.ForDay(day,day);
            var art=DailyTreasureRewardPresentation.FromCommittedReward(received);
            Eq(art.Length,expectedCards[day-1],
                "authored separate reveal cards for real paid day "+day);
            True(art.Length<=DailyTreasureRewardPresentation.MaxSimultaneousItems,
                "every paid reward fits mobile reveal layout "+day);
            for(int i=0;i<art.Length;i++)
            {
                True(!string.IsNullOrWhiteSpace(art[i].Icon),
                    "earned item has an original PNG identity "+day+"/"+i);
                True(!string.IsNullOrWhiteSpace(art[i].Name) &&
                     !string.IsNullOrWhiteSpace(art[i].Quantity),
                    "earned item caption is understandable "+day+"/"+i);
                True(art[i].Quantity.StartsWith("x "),
                    "quantities are accurate numbers, not fake random loot "+day+"/"+i);
            }
        }
        var grand=DailyTreasureRewardPresentation.FromCommittedReward(
            DailyTreasureRules.ForDay(7,7));
        Eq(grand[0].Name,"MADERA","grand chest wood card");
        Eq(grand[0].Quantity,"x 180","grand chest exactly 180 actual wood");
        Eq(grand[1].Quantity,"x 120","grand chest exactly 120 actual stone");
        Eq(grand[2].Name,"COMIDA · RACHA","seventh-day streak is an explicit bonus");
        Eq(grand[2].Quantity,"x 80","grand chest never invents more than eighty food");
        True(grand[2].LoyaltyBonus,"real paid loyalty bonus has its own label");
        Eq(grand[3].Name,"ACELERADOR 5 MIN","grand chest earned timer is 5 min");
        Eq(grand[3].Quantity,"x 1","grand chest exactly one 5-min timer");
        var dayFourItems=DailyTreasureRewardPresentation.FromCommittedReward(
            DailyTreasureRules.ForDay(4,4));
        Eq(dayFourItems.Length,2,"fourth-day special chest has only two items");
        True(dayFourItems[0].Quantity=="x 90" && dayFourItems[1].Quantity=="x 60",
            "fourth-day chest matches exact paid wood and stone");
        Eq(DailyTreasureRewardPresentation.FromCommittedReward(
            default(DailyTreasureReward)).Length,0,
            "invalid reward cannot display fabricated cards");
        foreach(var label in new[]{
            "milestone:hero:lyra","milestone:bastion:2",
            "scout:first:order42","vigilant:chapter:25",
            null})
        {
            var profile=DailyTreasureCeremonyTheme.ForEvent(label,true);
            True(profile.R>=0f && profile.R<=1f &&
                 profile.G>=0f && profile.G<=1f &&
                 profile.B>=0f && profile.B<=1f &&
                 profile.Strength>0f && profile.Strength<.6f,
                 "all original premium ceremony color accents remain bounded "+label);
            var weaker=DailyTreasureCeremonyTheme.ForEvent(label,false);
            True(profile.Strength>weaker.Strength,
                 "legendary ceremonies are subtly stronger in light "+label);
            True(profile.R==weaker.R && profile.G==weaker.G &&
                 profile.B==weaker.B,
                 "visual tier never replaces the event's true art identity "+label);
        }
        var heroMood=DailyTreasureCeremonyTheme.ForEvent(
            "milestone:hero:lyra",true);
        var guardianMood=DailyTreasureCeremonyTheme.ForEvent(
            "vigilant:chapter:25",true);
        var explorationMood=DailyTreasureCeremonyTheme.ForEvent(
            "scout:first:order42",false);
        var cityMood=DailyTreasureCeremonyTheme.ForEvent(
            "milestone:bastion:5",true);
        True(heroMood.B<guardianMood.B &&
             explorationMood.G>guardianMood.G &&
             cityMood.R>guardianMood.R,
             "hero, guardian, discovery and rebuilding remain visually distinct");
        // Original painted loot emerging from the genuinely opened coffer.
        // Use the exact Unity flight source for deterministic sequencing,
        // bounded phone-scale motion and guaranteed settled end states.
        for(int idx=0;idx<DailyTreasureRewardPresentation.MaxSimultaneousItems;idx++)
        {
            for(int mode=0;mode<2;mode++)
            {
                bool chestMode=mode==1;
                float homeX=(idx-1.5f)*75f;
                float homeY=idx<2?30f:-30f;
                var beginning=DailyTreasurePrizeFlight.Sample(
                    0f,idx,chestMode,0f,195f,homeX,homeY);
                Eq(beginning.Opacity,0f,"loot begins hidden at count "+idx);
                True(!beginning.Finished,"real reward animation does not finish before reveal");
                float previousAlpha=0f;
                for(int step=0;step<=140;step++)
                {
                    float elapsed=step*.011f;
                    var frame=DailyTreasurePrizeFlight.Sample(
                        elapsed,idx,chestMode,0f,195f,homeX,homeY);
                    True(frame.Opacity>=previousAlpha-.00001f &&
                         frame.Opacity>=0f && frame.Opacity<=1f,
                         "earned-item opacity changes smoothly "+idx+"/"+mode+"/"+step);
                    True(frame.Scale>=.51f && frame.Scale<=1.0001f,
                         "reward scale never clips the premium panel "+idx+"/"+mode+"/"+step);
                    True(Math.Abs(frame.Angle)<=9.0001f,
                         "no frantic card rotation "+idx+"/"+mode+"/"+step);
                    previousAlpha=frame.Opacity;
                }
                var finalCard=DailyTreasurePrizeFlight.Sample(
                    2f,idx,chestMode,0f,195f,homeX,homeY);
                True(finalCard.Finished && finalCard.X==homeX &&
                     finalCard.Y==homeY && finalCard.Scale==1f &&
                     finalCard.Opacity==1f && finalCard.Angle==0f,
                     "all credit-backed art cards settle exactly at target "+idx+"/"+mode);
            }
        }
        bool rejectedIndex=false;
        try{DailyTreasurePrizeFlight.Sample(0f,5,true,0f,0f,10f,10f);}
        catch(ArgumentOutOfRangeException){rejectedIndex=true;}
        True(rejectedIndex,"oversized prize count cannot create unintended UI cards");
        bool rejectedNan=false;
        try{DailyTreasurePrizeFlight.Sample(float.NaN,0,true,0f,0f,10f,10f);}
        catch(ArgumentOutOfRangeException){rejectedNan=true;}
        True(rejectedNan,"invalid animation time cannot corrupt Unity transforms");
        // The original physical audio cues must sound different from a
        // single click or an indefinitely sustained sine tone; the source
        // renderer is compiled and run here without any Unity mocks.
        float[] lastAudio=null;
        foreach(var cue in new[]{
            DailyTreasureFoleyCue.Latch,
            DailyTreasureFoleyCue.Discovery,
            DailyTreasureFoleyCue.GrandDiscovery})
        {
            var samples=DailyTreasureFoley.Render(cue);
            var repeat=DailyTreasureFoley.Render(cue);
            Eq(samples.Length,repeat.Length,"original Foley is reproducible");
            True(samples.Length>=9000 && samples.Length<=36000,
                "short authored Foley is mobile-memory bounded");
            double sum=0,sumSq=0,audioPeak=0;
            for(int j=0;j<samples.Length;j++)
            {
                True(!float.IsNaN(samples[j]) && !float.IsInfinity(samples[j]),
                    "procedural audio produces finite samples");
                True(Math.Abs(samples[j])<=.75001,
                    "wood/bronze audio will not digitally clip");
                sum+=samples[j];
                sumSq+=samples[j]*samples[j];
                audioPeak=Math.Max(audioPeak,Math.Abs(samples[j]));
                if(j%197==0)
                    Eq(samples[j],repeat[j],
                        "same sound does not change across sessions");
            }
            double rms=Math.Sqrt(sumSq/samples.Length);
            True(audioPeak>.04 && rms>.012,
                "original audible cue has useful but controlled body");
            True(Math.Abs(sum/samples.Length)<.004,
                "Foley has near-zero DC component");
            True(Math.Abs(samples[samples.Length-1])<.04,
                "sound fades instead of stopping with a harsh pop");
            if(lastAudio!=null)
                True(lastAudio.Length!=samples.Length,
                    "different ceremonies have distinct sound durations");
            lastAudio=samples;
        }
        Eq(DailyTreasureFoley.SampleRate,22050,
            "sound buffers use a low-memory mobile sample rate");
        Eq(DailyTreasureFoley.Duration(DailyTreasureFoleyCue.GrandDiscovery),
            1.48f,"grand chest chime duration");
        bool badSoundRate=false;
        try{DailyTreasureFoley.Render(DailyTreasureFoleyCue.Latch,1000);}
        catch(ArgumentOutOfRangeException){badSoundRate=true;}
        True(badSoundRate,"invalid audio sampling is rejected");
        // Premium live-game milestone stages are DIFFERENT SCENES, not the
        // same daily coffer with a tint. Verify stage boundaries and title/
        // Skip/Continue touch space at every supported portrait/landscape.
        var stageTypes=new[]{
            ("milestone:hero:lyra",DailyTreasureStageKind.Lyra),
            ("scout:first:family",DailyTreasureStageKind.Explorers),
            ("vigilant:chapter:25",DailyTreasureStageKind.Vigilants),
            ("milestone:bastion:5",DailyTreasureStageKind.Bastion)};
        for(int n=0;n<stageTypes.Length;n++)
            Eq(DailyTreasureStageDirection.FromEvent(stageTypes[n].Item1),
                stageTypes[n].Item2,"true milestone stage type "+n);
        Eq(DailyTreasureStageDirection.FromEvent(null),
            DailyTreasureStageKind.Daily,"daily loot never fabricates a milestone");
        Eq(DailyTreasureStageDirection.FromEvent("future:unknown"),
            DailyTreasureStageKind.Daily,"future event only uses safe baseline art");
        foreach(var device in new[]{
            (390f,844f),(844f,390f),(900f,1600f),(1600f,900f),
            (375f,667f),(667f,375f),(1024f,768f),(768f,1024f)})
        {
            bool portrait=device.Item2>device.Item1;
            float width=Math.Min(device.Item1-24f,portrait?440f:740f);
            float height=Math.Min(device.Item2-20f,portrait?700f:370f);
            foreach(var target in stageTypes)
            {
                var stage=DailyTreasureStageDirection.Compose(
                    target.Item1,device.Item1,device.Item2);
                True(stage.Kind==target.Item2,"unique painted event stage");
                True(stage.ChestW>180f && stage.ChestH>150f &&
                     stage.ChestX-stage.ChestW/2>=-width/2 &&
                     stage.ChestX+stage.ChestW/2<=width/2 &&
                     stage.ChestY-stage.ChestH/2>=-height/2 &&
                     stage.ChestY+stage.ChestH/2<=height/2,
                     "premium chest remains fully within real safe area");
                True(stage.ReceiptW>180f && stage.ReceiptH>50f &&
                     stage.ReceiptX-stage.ReceiptW/2>=-width/2 &&
                     stage.ReceiptX+stage.ReceiptW/2<=width/2,
                     "readable real-earned explanation is visible");
                // Continue is the same canonical 54px action at the base.
                float continueTop=-height/2+67f;
                True(stage.ReceiptY-stage.ReceiptH/2>=continueTop+8f,
                    "earned event description cannot cover Continue");
                True(stage.ReceiptY+stage.ReceiptH/2<=
                    stage.ChestY-stage.ChestH/2-7f,
                    "explanation stays below dimensional illustrated chest");
                if(target.Item2==DailyTreasureStageKind.Bastion)
                {
                    True(stage.SceneryOpacity==0f &&
                         stage.BackdropOpacity<.70f,
                         "completed Bastion preserves actual city behind translucent bronze");
                    True(!stage.SideBySide,
                         "actual-city Bastion must not imitate unrelated landmark artwork");
                }
                else
                {
                    True(stage.SceneryOpacity>=.70f &&
                         stage.SceneW>=300f && stage.SceneH>190f,
                         "real painterly event scene is large and legible");
                    True(stage.SceneX-stage.SceneW/2>=-width/2-1 &&
                         stage.SceneX+stage.SceneW/2<=width/2+1 &&
                         stage.SceneY-stage.SceneH/2>=-height/2 &&
                         stage.SceneY+stage.SceneH/2<=height/2,
                         "real original painting stays within decorated event frame");
                    Eq(stage.SideBySide,!portrait,
                         "true painterly scene and coffer use horizontal storytelling on landscape");
                    if(stage.SideBySide)
                        True(stage.SceneX+stage.SceneW/2+8f<=
                             stage.ChestX-stage.ChestW/2,
                            "large original painterly scenery and coffer do not obscure each other");
                    else
                        True(stage.SceneY-stage.SceneH/2<
                             stage.ChestY+stage.ChestH/2 &&
                             stage.SceneY+stage.SceneH/2>
                             stage.ChestY-stage.ChestH/2,
                            "portrait imagery layers behind the actual chest, not as an unrelated thumbnail");
                }
            }
        }
        foreach(float unusedUv in new[]{0f,.08f,.22f,.65f,1f})
        {
            for(int frame=0;frame<=100;frame++)
            {
                float seconds=frame*.047f;
                foreach(bool vertical in new[]{false,true})
                {
                    float delta=DailyTreasureStageDirection.SafePainterlyDrift(
                        seconds,unusedUv,vertical);
                    True(!float.IsNaN(delta)&&!float.IsInfinity(delta),
                        "actual painted scenery motion must be finite");
                    True(Math.Abs(delta)<=unusedUv*.13001f+.000001f,
                        "scenery remains within authored image UV margin");
                    float originalCenter=unusedUv*.5f;
                    True(originalCenter+delta>=-.00001f &&
                         originalCenter+delta<=unusedUv+.00001f,
                         "never sample repeated/empty outside of actual illustrated landmark");
                }
            }
        }
        bool invalidPainterlySize=false;
        try{DailyTreasureStageDirection.Compose("milestone:hero:lyra",
            float.PositiveInfinity,840f);}
        catch(ArgumentOutOfRangeException){invalidPainterlySize=true;}
        True(invalidPainterlySize,
            "invalid or infinite native viewport cannot corrupt hero stage");
        // Protect the owner's actual charcoal/copper/burgundy language.
        // Hero and kingdom milestones may have a mineral accent but NEVER
        // become saturated purple, floral or a fake gem/coin loot scene.
        foreach(var eventKey in new[]{
            (string)null,"milestone:hero:lyra","milestone:bastion:5",
            "scout:first:family","vigilant:chapter:25"
        })
        {
            foreach(bool grandAccent in new[]{false,true})
            {
                var style=DailyTreasureCeremonyTheme.ForEvent(eventKey,grandAccent);
                True(style.R>=0f && style.R<=1f &&
                     style.G>=0f && style.G<=1f &&
                     style.B>=0f && style.B<=1f,
                    "event palette component stays in an actual display gamut");
                float low=Math.Min(style.R,Math.Min(style.G,style.B));
                float high=Math.Max(style.R,Math.Max(style.G,style.B));
                True(high-low<=.55f,
                    "premium mineral event tint must not become high-chroma neon");
                True(style.Strength>=.15f && style.Strength<=.34f,
                    "events stay atmospheric, not an overwhelming colored screen wash");
                if(eventKey!=null && eventKey.StartsWith("vigilant:"))
                    True(style.B<=.54f &&
                         style.G>=.37f,
                        "violet of the Breach remains stone-like, never electric purple");
            }
        }
        Console.WriteLine("PASS daily treasures domain: "+checks+" assertions");
    }
}
