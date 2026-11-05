using System;
using System.Collections.Generic;

namespace Eldoria.Domain
{
    // Explicit catalog injection only. No invented campaign opponents or unlocks.
    // The immutable departure snapshot, not a later catalog revision, resolves returns.
    public sealed class EldoriaV41GatewayResolver : IEldoriaVersionedBattleResolver
    {
        readonly Dictionary<string,EldoriaV41BattleInput> targets = new Dictionary<string,EldoriaV41BattleInput>(StringComparer.Ordinal);
        public bool UsesCampaignCreatures { get; private set; }
        public static EldoriaV41GatewayResolver Campaign() => new EldoriaV41GatewayResolver(Array.Empty<EldoriaV41BattleInput>()){UsesCampaignCreatures=true};
        public string RulesVersion => EldoriaV41CombatEvolution.RulesVersion;
        public EldoriaV41GatewayResolver(IEnumerable<EldoriaV41BattleInput> definitions)
        {
            if(definitions==null)throw new ArgumentNullException(nameof(definitions));
            foreach(var definition in definitions)
            {
                if(definition==null || string.IsNullOrEmpty(definition.TargetId) || definition.RulesVersion!=RulesVersion)
                    throw new ArgumentException("Invalid versioned target definition");
                Force(definition.EnemyTroops,definition.EnemyHeroes);
                targets.Add(definition.TargetId,definition.Copy());
            }
        }
        static EldoriaV41CombatEvolution.Force Force(ArmyRoster troops,IList<string> heroes)
        {
            if(troops==null || !troops.IsNonNegative || heroes==null || heroes.Count!=3)
                throw new ArgumentException("Incomplete battle snapshot");
            var force=new EldoriaV41CombatEvolution.Force().WithHeroes(heroes[0],heroes[1],heroes[2]);
            for(int f=0;f<3;f++)for(int t=1;t<=5;t++)force.Add(f,t,troops.Get(f,t));
            // Validation without computing a fictitious battle.
            EldoriaV41CombatEvolution.ValidateForce(force);
            return force;
        }
        public EldoriaV41BattleInput Prepare(ArmyRoster troops,IList<string> heroes,string targetId)
        {
            if(!targets.TryGetValue(targetId,out var definition))return null;
            Force(troops,heroes);
            var snapshot=definition.Copy();snapshot.OwnTroops=troops.Copy();
            snapshot.OwnHeroes=new string[3];for(int i=0;i<3;i++)snapshot.OwnHeroes[i]=heroes[i];
            return snapshot;
        }
        public EldoriaV41BattleInput PrepareCreature(ArmyRoster troops,IList<string> heroes,string targetId,WorldCreatureStats enemy)
        {
            Force(troops,heroes);
            if(string.IsNullOrEmpty(targetId) || enemy.Health<=0 || enemy.Attack<0 || enemy.Defense<0 || enemy.Break<0)
                throw new ArgumentException("Invalid creature definition");
            var copy=new string[3];for(int h=0;h<3;h++)copy[h]=heroes[h];
            return new EldoriaV41BattleInput{RulesVersion=RulesVersion,TargetId=targetId,OwnTroops=troops.Copy(),OwnHeroes=copy,
                EnemyCreature=new EldoriaV41CreatureInput{Health=enemy.Health,Attack=enemy.Attack,Defense=enemy.Defense,Break=enemy.Break}};
        }
        public EldoriaV41CombatRecord Resolve(EldoriaV41BattleInput input)
        {
            if(input==null || input.RulesVersion!=RulesVersion || string.IsNullOrEmpty(input.TargetId))
                throw new ArgumentException("Unsupported battle rules");
            var player=Force(input.OwnTroops,input.OwnHeroes);
            if(input.EnemyCreature!=null && (input.EnemyCreature.Health!=0 || input.EnemyCreature.Attack!=0 || input.EnemyCreature.Defense!=0 || input.EnemyCreature.Break!=0))
            {
                if((input.EnemyTroops!=null && input.EnemyTroops.Total!=0) || (input.EnemyHeroes!=null && !Array.TrueForAll(input.EnemyHeroes,string.IsNullOrEmpty)))throw new ArgumentException("Ambiguous enemy profile");
                var actual=EldoriaV41CombatEvolution.ResolveCreature(player,input.EnemyCreature.Stats);
                return EldoriaV41CombatEvolution.ToCreatureMailbox(actual,player,"uncommitted",input.TargetId,1).Battle;
            }
            var enemy=Force(input.EnemyTroops,input.EnemyHeroes);
            var result=EldoriaV41CombatEvolution.Resolve(player,enemy);
            // Only the public facts leave the resolver; gateway owns event/time/ledger.
            return EldoriaV41CombatEvolution.ToMailbox(result,player,enemy,"uncommitted",input.TargetId,1).Battle;
        }
    }
}
