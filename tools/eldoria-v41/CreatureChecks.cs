using System;
using Eldoria.Domain;
static class CreatureChecks
{
 static void Check(bool value,string name){if(!value)throw new Exception("V41_CREATURE_FAIL "+name);}
 public static void Run()
 {
  var weak=new EldoriaV41CombatEvolution.Force().Add(1,1,1);
  var starter=new EldoriaV41CombatEvolution.Force().Add(1,1,36);
  var developed=new EldoriaV41CombatEvolution.Force().Add(1,1,80);
  Check(EldoriaV41CombatEvolution.ResolveCreature(weak,SliceRules.CampaignEnemy("corrupt-scout")).Winner==-1,"one soldier cannot clear scout");
  Check(EldoriaV41CombatEvolution.ResolveCreature(starter,SliceRules.CampaignEnemy("corrupt-scout")).Winner==1,"starter campaign accessible");
  Check(EldoriaV41CombatEvolution.ResolveCreature(starter,EarlyRetentionRules.Stage(1).Enemy).Winner==1,"Vigilants entry accessible");
  Check(EldoriaV41CombatEvolution.ResolveCreature(starter,EarlyRetentionRules.Stage(25).Enemy).Winner==-1,"existing boss remains a growth wall");
  Check(EldoriaV41CombatEvolution.ResolveCreature(developed,EarlyRetentionRules.Stage(25).Enemy).Winner==1,"larger army can overcome existing boss");
  for(int tier=1;tier<=5;tier++)for(int family=0;family<3;family++)
  {
   var force=new EldoriaV41CombatEvolution.Force().Add(family,tier,100);
   var r=EldoriaV41CombatEvolution.ResolveCreature(force,EarlyRetentionRules.Stage(25).Enemy);
   Check(r.CounterAdvantage==0,"untyped creature has no invented family counter");
   Check(!double.IsNaN(r.RemainingA)&&r.RemainingA>=0&&r.RemainingA<=1,"finite remaining own health");
  }
  var resolver=EldoriaV41GatewayResolver.Campaign();
  var input=resolver.PrepareCreature(new ArmyRoster{ArcherT1=36},new[]{"aldric",null,null},"corrupt-scout",SliceRules.CampaignEnemy("corrupt-scout"));
  var copy=input.Copy();input.EnemyCreature.Health=100000;
  Check(resolver.Resolve(copy).Outcome=="victory","creature definition frozen before change");
  Check(copy.EnemyTroops==null&&copy.EnemyHeroes==null,"no invented enemy troops or heroes");
  var shell=new EldoriaV41BattleInput{OwnTroops=new ArmyRoster(),EnemyTroops=new ArmyRoster(),EnemyCreature=new EldoriaV41CreatureInput(),OwnHeroes=new[]{"","",""}};
  Check(shell.IsLegacyPlaceholder,"Unity empty legacy shell recognized");shell.TargetId="corrupt-scout";
  Check(!shell.IsLegacyPlaceholder,"partial battle input cannot downgrade silently");
  Console.WriteLine("V41_CREATURE_ADAPTER_PASS 39");
 }
}
