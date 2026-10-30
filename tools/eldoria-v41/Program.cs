using System;
using System.Collections.Generic;
using System.Linq;
using Eldoria.Domain;

static class Tests
{
    static int passed;
    static void Check(bool condition,string name)
    {
        if(!condition)throw new Exception("FAIL: "+name);
        Console.WriteLine("PASS "+name);passed++;
    }
    static void ShouldThrow(Action action,string name)
    {
        bool threw=false;
        try{action();}catch(ArgumentException){threw=true;}
        Check(threw,name);
    }
    static Dictionary<string,int> Counts(int a,int p=0,int w=0)
        => new Dictionary<string,int>{{"archer",a},{"paladin",p},{"warlock",w}};
    static void Main()
    {
        var all=new[]{"archer","paladin","warlock"};
        var stock=Counts(800,600,400);
        var count=Counts(120,80,60);
        var result=EldoriaV41Planning.EditPercent("archer",20,200,1800,count,stock,all);
        Check(result["archer"]==40 && result.Values.Sum()==200,"20 percent of 200 = 40");
        Check(EldoriaV41Planning.Percentages(result).Values.Sum()==100,"integer displayed percentages total 100");
        Check(EldoriaV41Planning.Percentages(Counts(1,1,1))["archer"]==34,"remainder tie favors first family");
        ShouldThrow(()=>EldoriaV41Planning.EditPercent("archer",60,200,200,Counts(50,75,75),Counts(70,100,100),all),"reject requesting more than available");
        var capped=EldoriaV41Planning.EditCount("archer",60,200,200,Counts(60,100,40),Counts(80,60,100),all);
        Check(capped["archer"]==60 && capped["paladin"]==60 && capped["warlock"]==80,"reallocates shortage under caps");
        var solo=EldoriaV41Planning.SetTarget(36,60,Counts(30),Counts(36),new[]{"archer"});
        Check(solo["archer"]==36 && solo["paladin"]==0 && solo["warlock"]==0,"early Bastion: archer only");
        ShouldThrow(()=>EldoriaV41Planning.SetTarget(37,60,Counts(37),Counts(36),new[]{"archer"}),"never invent units");
        ShouldThrow(()=>EldoriaV41Planning.EditCount("paladin",1,36,60,solo,Counts(36,20,0),new[]{"archer"}),"reject locked Paladin");
        var heroes=EldoriaV41Planning.SelectHero(new[]{"aldric",null,null},1,"lyra",new[]{"aldric","lyra"});
        Check(heroes[0]=="aldric" && heroes[1]=="lyra","two heroes selected");
        ShouldThrow(()=>EldoriaV41Planning.SelectHero(heroes,2,"aldric",new[]{"aldric","lyra"}),"cannot duplicate hero");
        ShouldThrow(()=>EldoriaV41Planning.SelectHero(heroes,0,null,new[]{"aldric","lyra"}),"leader cannot be removed with companions");
        ShouldThrow(()=>EldoriaV41Planning.SelectHero(heroes,2,"secret",new[]{"aldric","lyra"}),"cannot use narratively locked hero");
        var mail=new EldoriaV41Mailbox();
        Check(mail.Receive(new EldoriaV41Message{EventId="fight:01",Category="informes",Type="battle",Title="Victoria",ReportId="report:01",OccurredAtUtcTicks=11}),"receive real report reference");
        Check(mail.Unread()==1 && mail.Unread("informes")==1,"unread count");
        Check(!mail.Receive(new EldoriaV41Message{EventId="fight:01",Category="informes",Type="battle",Title="Duplicado"}),"offline replay idempotent");
        mail.MarkRead("fight:01");
        Check(mail.Unread()==0,"mark read");
        mail.Delete("fight:01");
        Check(!mail.Receive(new EldoriaV41Message{EventId="fight:01",Category="informes",Type="battle",Title="Revivido"}),"deleted event stays tombstoned");
        Check(mail.Messages.Count==0 && mail.ReceivedEventIds.Count==1,"mail deletion does not reset ledger reference");
        Check(mail.Receive(new EldoriaV41Message{EventId="reward:02",Category="recompensas",Title="Premio",RewardReceiptId="ledger:02",OccurredAtUtcTicks=13}),"receipt is only a reference");
        Check(mail.List()[0].RewardReceiptId=="ledger:02","receipt retained");
        Check(mail.SchemaVersion==1 && mail.Revision>0,"serializable mailbox version");
        Console.WriteLine("ELDORIA_V41_CONTRACT_PASS "+passed);
    }
}

    }
}
