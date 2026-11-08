using System;

namespace Eldoria.DailyTreasures
{
    // Original procedural sound effects, not a song, recording, licensed
    // sample, or separately paid asset. Computed once per UI instance.
    // Deliberately independent from Unity for source-level wave validation.
    public enum DailyTreasureFoleyCue { Latch, Discovery, GrandDiscovery }

    public static class DailyTreasureFoley
    {
        public const int SampleRate=22050;
        public static float Duration(DailyTreasureFoleyCue cue)
        {
            switch(cue)
            {
                case DailyTreasureFoleyCue.Latch: return .43f;
                case DailyTreasureFoleyCue.Discovery: return 1.25f;
                case DailyTreasureFoleyCue.GrandDiscovery: return 1.48f;
                default: throw new ArgumentOutOfRangeException(nameof(cue));
            }
        }
        static double Sine(double t,double f)
            =>Math.Sin(2.0*Math.PI*f*t);

        static double Envelope(double t,double start,double duration)
        {
            if(t<start || t>start+duration)return 0.0;
            double x=(t-start)/duration;
            return (1.0-Math.Exp(-45.0*x))*Math.Exp(-5.0*x);
        }
        static double Ring(double t,double start,double frequency,
            double length,double damping)
        {
            if(t<start || t>start+length)return 0.0;
            double age=t-start;
            double attack=1.0-Math.Exp(-400.0*age);
            double wobble=1.0+0.0025*Math.Sin(age*15.0);
            return attack*Math.Exp(-damping*age)*
                (Sine(age,frequency*wobble)+
                0.19*Sine(age,frequency*1.39)+
                0.07*Sine(age,frequency*2.18));
        }
        // Hash noise uses fixed integer arithmetic. Stable across replays,
        // snapshots and CI; does not rely on System.Random or external data.
        static double Noise(int sample)
        {
            unchecked
            {
                uint z=(uint)sample*747796405u+2891336453u;
                z=((z>>((int)(z>>28)+4))^z)*277803737u;
                z=(z>>22)^z;
                return (z/(double)uint.MaxValue)*2.0-1.0;
            }
        }

        public static float[] Render(DailyTreasureFoleyCue cue,int rate=SampleRate)
        {
            if(rate<11025 || rate>48000)
                throw new ArgumentOutOfRangeException(nameof(rate));
            int count=(int)Math.Ceiling(Duration(cue)*rate);
            float[] dry=new float[count];
            double lowpass=0.0;
            bool latch=cue==DailyTreasureFoleyCue.Latch;
            bool grand=cue==DailyTreasureFoleyCue.GrandDiscovery;

            for(int i=0;i<count;i++)
            {
                double t=i/(double)rate;
                double noise=Noise(i);
                double sample;
                if(latch)
                {
                    // Heavy oak latch: low physical body, soft inharmonic iron,
                    // two short smaller ratchets and a decaying scrape.
                    double body=0.64*Sine(t,103)*Math.Exp(-t*17.0)+
                        0.15*Sine(t,173)*Math.Exp(-t*26.0);
                    double iron=0.28*Ring(t,.015,327,.24,21.0)+
                        0.09*Ring(t,.051,613,.17,23.0);
                    double rattle=(0.21*Envelope(t,.019,.094)+
                        0.13*Envelope(t,.081,.070))*noise;
                    lowpass=lowpass*.79+noise*.21;
                    double scrape=0.06*Envelope(t,.11,.19)*lowpass;
                    sample=body+iron+rattle+scrape;
                }
                else
                {
                    // A restrained physical shimmer: air movement + non-
                    // musical inharmonic bronze ringing. The larger event
                    // receives a fuller low resonance, not a loud arcade jingle.
                    double initial=grand?.13:.16;
                    double airStart=grand?.045:.050;
                    lowpass=lowpass*.87+noise*.13;
                    double air=(0.24*Envelope(t,airStart,.66)+
                        0.08*Envelope(t,.55,.49))*lowpass;
                    double bronze=0.36*Ring(t,initial,541,1.14,2.7)+
                        0.21*Ring(t,initial+.035,871,1.04,3.9)+
                        0.11*Ring(t,initial+.15,1339,.87,5.0);
                    double glint=.13*Ring(t,.35,1913,.72,6.2)+
                        .08*Ring(t,.68,2557,.50,7.8);
                    double low=grand?
                        .24*Sine(t,97)*Envelope(t,.085,.73)+
                        .12*Ring(t,.22,307,.86,3.1):
                        .07*Ring(t,.22,313,.73,4.4);
                    sample=air+bronze+glint+low;
                }
                dry[i]=(float)sample;
            }

            // Small room/reflection tail; separate delay taps, not sustained
            // music or loop. Preserve the original dry impact at full attack.
            int d1=(int)(rate*.047),d2=(int)(rate*.113);
            double peak=0.0,mean=0.0;
            var wet=new float[count];
            for(int i=0;i<count;i++)
            {
                double reflected=dry[i]+
                    (i>=d1?dry[i-d1]*(latch?.12:.19):0.0)+
                    (i>=d2?dry[i-d2]*(latch?.055:.10):0.0);
                double outFade=Math.Min(1.0,
                    Math.Max(0.0,(count-i)/(rate*.028)));
                double value=reflected*outFade;
                wet[i]=(float)value;
                peak=Math.Max(peak,Math.Abs(value));
                mean+=value;
            }
            // Removes DC offset and normalizes deterministically; no clipping,
            // large allocations or changing clip volume between daily sessions.
            mean/=count;
            double gain=peak>1e-8?Math.Min(1.0,.67/peak):1.0;
            for(int i=0;i<count;i++)
                wet[i]=(float)Math.Max(-.75,Math.Min(.75,(wet[i]-mean)*gain));
            return wet;
        }
    }
}
