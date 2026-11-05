using System;
using System.Collections.Generic;
using Eldoria.Domain;
using Eldoria.PlayerClarity;

internal static class PlayerClarityAdvancedProbe
{
    static int checks;
    static void Check(bool pass, string reason)
    {
        if (!pass) throw new Exception("PLAYER_CLARITY_ADVANCED_FAIL: "+reason);
        checks++;
    }

    private static void Name()
    {
        foreach (var test in new [] {
            ("  Álvaro_27  ","Álvaro_27"),("Sir   Aldric","Sir Aldric"),
            ("Jugador 123","Jugador 123"),("Valoria-1","Valoria-1") })
        {
            bool domain=PlayerIdentityPolicy.TryNormalize(test.Item1,out var valid,out _);
            bool ui=PlayerNameRules.TryNormalize(test.Item1,out var bound,out _);
            Check(domain&&ui&&valid==bound&&valid==test.Item2,
                "identity must match UI "+test.Item2);
        }
        foreach(string raw in new[] { "", "  ", "Ab", "<x>", "A\nB", "A\tB", "Luz🙂",
            "012345678901234567890" })
            Check(!PlayerIdentityPolicy.TryNormalize(raw,out _,out _),"bad name");
        Check(PlayerIdentityPolicy.TryNormalize("012345678901234567",out var full,out _)&&
            full.Length==18,"maximum length");
    }

    private static void Tier()
    {
        Check(ResourceNodeTierPolicy.Capacity("wood",1)==1250,"legacy wood capacity");
        Check(ResourceNodeTierPolicy.Capacity("stone",1)==5200,"legacy stone capacity");
        for(int level=2;level<=5;level++)
            Check(ResourceNodeTierPolicy.Capacity("wood",level)>
                  ResourceNodeTierPolicy.Capacity("wood",level-1)
                &&ResourceNodeTierPolicy.Capacity("stone",level)>
                  ResourceNodeTierPolicy.Capacity("stone",level-1),
                "capacity increases with the displayed node level");
        Check(ResourceNodeTierPolicy.ExistingSaveLevel("wood",1250)==1&&
            ResourceNodeTierPolicy.ExistingSaveLevel("stone",5200)==1,
            "older saves retain original resource capacity");
        var count = new int[6];
        for (int i=1;i<=1000;i++)
        {
            int level=ResourceNodeTierPolicy.ForNewSite("region1:wood:"+i,
                "region1:"+((i*23)%431)+":"+((i*73)%269),i%13+1);
            Check(level>=1&&level<=5,"legal level");
            count[level]++;
        }
        for(int i=1;i<=5;i++)Check(count[i]>0,"all levels represented");
    }

    private static void Sites()
    {
        foreach(int n in new[] {1,2,7,32,97,512,1003})
        {
            var seen=new HashSet<int>();
            for(int i=0;i<n;i++)
            {
                int index=DistributedSiteOrder.Candidate(n,"region1:boar:011",6,i);
                Check(index>=0&&index<n&&seen.Add(index),"unique verified site");
            }
            Check(seen.Count==n,"exhaustive cycle");
        }
        int different=0;
        for(int i=0;i<10;i++)
            if(DistributedSiteOrder.Candidate(517,"region1:stone:019",i,0)!=
                DistributedSiteOrder.Candidate(517,"region1:stone:019",i+1,0))
                different++;
        Check(different>=6,"successive generations should not lock to the same site");
    }

    private static void Renew()
    {
        long t=new DateTime(2026,11,5,12,0,0,DateTimeKind.Utc).Ticks;
        Check(!RenewableRespawnRules.CanRespawn(t,t+TimeSpan.FromSeconds(299).Ticks),"before 300");
        Check(RenewableRespawnRules.CanRespawn(t,t+TimeSpan.FromSeconds(300).Ticks),"at 300");
        Check(RenewableRespawnRules.CanRespawn(t,t+TimeSpan.FromMinutes(20).Ticks),"offline over timer");
        for(int min=15;min<=30;min+=15)
            Check(RenewableRespawnRules.MigrateExistingDue(t,t+TimeSpan.FromMinutes(min).Ticks)==
                t+TimeSpan.FromMinutes(5).Ticks,"legacy due from historical event "+min);
        Check(RenewableRespawnRules.MigrateExistingDue(t,0)==0,"no invented due");
    }

    public static void Main()
    {
        Name();Tier();Sites();Renew();
        Console.WriteLine("PLAYER_CLARITY_ADVANCED_PASS checks="+checks);
    }
}
