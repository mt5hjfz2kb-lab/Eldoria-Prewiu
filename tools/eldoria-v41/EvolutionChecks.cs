using System;
using Eldoria.Domain;

static class EvolutionChecks
{
    static int checks;
    static void Check(bool value, string name)
    {
        if (!value) throw new Exception("V41_EVOLUTION_FAIL: " + name);
        checks++;
    }
    static EldoriaV41CombatEvolution.Force Force(int f, int t, int n) =>
        new EldoriaV41CombatEvolution.Force().Add(f,t,n);
    public static void Run()
    {
        for(int f=0; f<3; f++) for(int t=1; t<=5; t++)
        {
            var a=Force(f,t,100);
            var first=EldoriaV41CombatEvolution.Resolve(a,Force(f,t,100));
            var replay=EldoriaV41CombatEvolution.Resolve(a,Force(f,t,100));
            Check(first.Winner==0,"symmetric draw");
            Check(first.Rounds==replay.Rounds && first.RemainingA==replay.RemainingA,"deterministic replay");
            Check(a.Count==100,"input roster unchanged");
            if(t<5) Check(EldoriaV41CombatEvolution.Resolve(a,Force(f,t+1,100)).Winner==-1,"same family tier advantage");
            Check(EldoriaV41CombatEvolution.Resolve(a,Force((f+1)%3,t,100)).Winner==1,"original counter cycle");
        }
        for(int f=0;f<3;f++)for(int e=0;e<3;e++)
            Check(EldoriaV41CombatEvolution.Resolve(Force(f,1,1000),Force(e,5,1)).Winner==1,"no automatic tier victory");
        var player=Force(1,3,50).WithHeroes("aldric","lyra");
        var enemy=Force(0,3,50);
        var result=EldoriaV41CombatEvolution.Resolve(player,enemy);
        var message=EldoriaV41CombatEvolution.ToMailbox(result,player,enemy,"battle:test:1","vigilant:25",637000000000000000L);
        var mailbox=new EldoriaV41Mailbox();
        Check(mailbox.Receive(message),"new real report");
        Check(!mailbox.Receive(message),"replayed report rejected");
        var restored=mailbox.Copy();
        Check(restored.Messages[0].Battle.OwnDamageByFamily==message.Battle.OwnDamageByFamily,"own analysis survives copy");
        restored.Delete(message.EventId);
        Check(!restored.Receive(message),"deleted report tombstone survives");
        Check(message.Battle.RewardFood==0 && message.Battle.RewardStone==0 && message.Battle.RewardWood==0,"engine cannot grant currency");
        Check(!EldoriaV41Combat.ProductionApproved && !EldoriaV41CombatEvolution.ProductionApproved,"laboratory does not silently activate");
        Console.WriteLine("V41_EVOLUTION_CHECKS_PASS "+checks);
    }
}
