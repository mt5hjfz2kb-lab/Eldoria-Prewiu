using System;
using System.Collections.Generic;
using Eldoria.Domain;
static class DepthChecks
{
    static EldoriaV41CombatEvolution.Force Force(int[] counts,int tier)
    {var f=new EldoriaV41CombatEvolution.Force();for(int i=0;i<3;i++)f.Add(i,tier,counts[i]);return f;}
    public static void Run()
    {
        var forms=new List<int[]>();for(int p=0;p<=100;p+=10)for(int a=0;a<=100-p;a+=10)forms.Add(new[]{p,a,100-p-a});
        int duels=0;int[] expectedBest={42,43,45,44,44},expectedLowerWins={98,86,75,63};
        for(int tier=1;tier<=5;tier++)
        {
            int best=0;
            foreach(var a in forms)
            {
                int wins=0,losses=0;
                foreach(var b in forms){int w=EldoriaV41CombatEvolution.Resolve(Force(a,tier),Force(b,tier)).Winner;if(w==1)wins++;if(w==-1)losses++;duels++;}
                if(losses==0)throw new Exception("Unexpected dominant composition tier "+tier);
                best=Math.Max(best,wins);
            }
            if(best!=expectedBest[tier-1])throw new Exception("Historical strategic matrix differs tier "+tier+" best "+best);
            if(tier==5)continue;
            int lowerWins=0,draws=0;
            foreach(var a in forms)foreach(var b in forms)
            {int w=EldoriaV41CombatEvolution.Resolve(Force(a,tier),Force(b,tier+1)).Winner;if(w==1)lowerWins++;if(w==0)draws++;duels++;}
            if(lowerWins!=expectedLowerWins[tier-1] || draws!=0)throw new Exception("Historical cross-tier matrix differs tier "+tier+" wins "+lowerWins);
        }
        Console.WriteLine("V41_HISTORICAL_DEPTH_CSHARP_PASS "+duels);
    }
}
