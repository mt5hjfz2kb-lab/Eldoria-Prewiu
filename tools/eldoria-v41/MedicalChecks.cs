using System;
using Eldoria.Domain;

static class MedicalChecks
{
    static int checks;
    static void Check(bool v,string n){if(!v)throw new Exception("V41_MEDICAL_FAIL "+n);checks++;}
    static ArmyRoster Army(int n)=>new ArmyRoster{ArcherT1=n};
    static EldoriaV41Medical.Arrival Arrival(string id,long tick,int graves=3)=>new EldoriaV41Medical.Arrival{
        EventId=id,Mode="pve",UtcTicks=tick,TreatmentTicks=10,Deployed=Army(10),Graves=Army(graves),DirectDeaths=Army(0)};
    public static void Run()
    {
        var disabled=new EldoriaV41Medical.Hospital{Capacity=0};
        var safe=EldoriaV41Medical.PlanReturns(disabled,new[]{Arrival("safe",100)},100);
        Check(safe.Returned.Total==10 && safe.Applied[0].OverflowDeaths.Total==0,"no real Hospital no permanent casualties");
        var h=new EldoriaV41Medical.Hospital{Capacity=2,FeatureOperational=true};
        var p=EldoriaV41Medical.PlanReturns(h,new[]{Arrival("first",100)},100);
        Check(h.Receipts.Count==0 && h.Treatments.Count==0,"plan does not mutate original save");
        Check(p.Returned.Total==7 && p.Applied[0].Admitted.Total==2 && p.Applied[0].OverflowDeaths.Total==1,"PvE excess graves die");
        Check(p.Applied[0].DirectDeaths.Total==0,"PvE has no direct combat deaths");
        var replay=EldoriaV41Medical.PlanReturns(p.State,new[]{Arrival("first",100)},100);
        Check(replay.Returned.Total==0 && replay.Applied.Count==0,"replay does not duplicate troops or losses");
        bool rejected=false;
        try{EldoriaV41Medical.PlanReturns(p.State,new[]{Arrival("first",100,4)},100);}catch(ArgumentException){rejected=true;}
        Check(rejected,"same event cannot change payload");
        var healed=EldoriaV41Medical.PlanReturns(p.State,Array.Empty<EldoriaV41Medical.Arrival>(),110);
        Check(healed.Returned.Total==2 && healed.State.Treatments.Count==0,"UTC treatment releases troops offline");
        var again=EldoriaV41Medical.PlanReturns(healed.State,Array.Empty<EldoriaV41Medical.Arrival>(),110);
        Check(again.Returned.Total==0,"healing paid once");
        var ordered=EldoriaV41Medical.PlanReturns(h,new[]{Arrival("z",100),Arrival("a",100)},100);
        Check(ordered.Applied[0].EventId=="a" && ordered.Applied[1].Admitted.Total==0,"simultaneous returns ordered deterministically");
        var freed=EldoriaV41Medical.PlanReturns(p.State,new[]{Arrival("second",110)},110);
        Check(freed.Applied[0].Admitted.Total==2 && freed.Returned.Total==9,"discharges precede new admissions at same tick");
        var untouched=EldoriaV41Medical.PlanReturns(new EldoriaV41Medical.Hospital{FeatureOperational=true,Capacity=0},new[]{Arrival("light",100,0)},100);
        Check(untouched.Returned.Total==10 && untouched.Applied[0].OverflowDeaths.Total==0,"full Hospital cannot kill light wounds");
        for(int n=0;n<=40;n++)for(int beds=0;beds<=40;beds++)
        {
            var evt=Arrival("sweep",100,0);evt.Deployed=new ArmyRoster();evt.Graves=new ArmyRoster();
            for(int f=0;f<3;f++)for(int t=1;t<=5;t++){evt.Deployed.Set(f,t,n);evt.Graves.Set(f,t,n/2);}
            var plan=EldoriaV41Medical.PlanReturns(new EldoriaV41Medical.Hospital{FeatureOperational=true,Capacity=beds},new[]{evt},100);
            var r=plan.Applied[0];
            for(int f=0;f<3;f++)for(int t=1;t<=5;t++)
                Check(r.Ready.Get(f,t)+r.Admitted.Get(f,t)+r.DirectDeaths.Get(f,t)+r.OverflowDeaths.Get(f,t)==n,"soldier conserved per family/tier");
            Check(r.Admitted.Total<=beds,"capacity never exceeded");
        }
        Console.WriteLine("V41_MEDICAL_CHECKS_PASS "+checks);
    }
}
