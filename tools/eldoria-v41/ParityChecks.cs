using System;
using System.Globalization;
using System.IO;
using Eldoria.Domain;
static class ParityChecks
{
    static void Near(double actual,string expected,int row,string field)
    {
        double value=double.Parse(expected,CultureInfo.InvariantCulture);
        if(double.IsNaN(actual) || Math.Abs(actual-value)>1e-7*Math.Max(1,Math.Abs(value)))
            throw new Exception($"PYTHON_CSHARP_DIVERGENCE row={row} field={field} actual={actual:R} expected={value:R}");
    }
    public static void Run()
    {
        int count=0;
        foreach(var line in File.ReadLines("tools/eldoria-v41/laboratory/parity.csv"))
        {
            if(line.StartsWith("#"))continue;
            var cells=line.Split(',');int at=0;
            var a=new EldoriaV41CombatEvolution.Force();var b=new EldoriaV41CombatEvolution.Force();
            foreach(var force in new[]{a,b})for(int f=0;f<3;f++)for(int t=1;t<=5;t++)force.Add(f,t,int.Parse(cells[at++]));
            foreach(var force in new[]{a,b})for(int h=0;h<3;h++){string x=cells[at++];force.HeroIds[h]=x=="-"?null:x;}
            var result=EldoriaV41CombatEvolution.Resolve(a,b);
            string outcome=result.Winner==1?"victory":result.Winner==-1?"defeat":"draw";
            if(outcome!=cells[at++] || result.Rounds!=int.Parse(cells[at++]))throw new Exception("PARITY outcome/rounds row "+count);
            Near(result.RemainingA,cells[at++],count,"remainingA");Near(result.RemainingB,cells[at++],count,"remainingB");
            foreach(var damage in new[]{result.DealtA,result.DealtB})foreach(double n in damage)Near(n,cells[at++],count,"damage");
            foreach(var hp in new[]{result.DamagedHpA,result.DamagedHpB})for(int f=0;f<3;f++)for(int t=0;t<5;t++)Near(hp[f,t],cells[at++],count,"damagedHp");
            if(at!=cells.Length)throw new Exception("PARITY malformed fixture");count++;
        }
        if(count!=434)throw new Exception("PARITY missing cases");
        Console.WriteLine("V41_PYTHON_CSHARP_PARITY_PASS "+count);
    }
}
