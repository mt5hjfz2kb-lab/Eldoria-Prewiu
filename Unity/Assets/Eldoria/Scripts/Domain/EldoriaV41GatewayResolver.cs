using System;
using System.Collections.Generic;

namespace Eldoria.Domain
{
    // Explicit catalog injection only. No invented campaign opponents or unlocks.
    // The immutable departure snapshot, not a later catalog revision, resolves returns.
    public sealed class EldoriaV41GatewayResolver : IEldoriaVersionedBattleResolver
    {
        readonly Dictionary<string,EldoriaV41BattleInput> targets = new Dictionary<string,EldoriaV41BattleInput>(StringComparer.Ordinal);
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
        public EldoriaV41CombatRecord Resolve(EldoriaV41BattleInput input)
        {
            if(input==null || input.RulesVersion!=RulesVersion || string.IsNullOrEmpty(input.TargetId))
                throw new ArgumentException("Unsupported battle rules");
            var player=Force(input.OwnTroops,input.OwnHeroes);var enemy=Force(input.EnemyTroops,input.EnemyHeroes);
            var result=EldoriaV41CombatEvolution.Resolve(player,enemy);
            // Only the public facts leave the resolver; gateway owns event/time/ledger.
            return EldoriaV41CombatEvolution.ToMailbox(result,player,enemy,"uncommitted",input.TargetId,1).Battle;
        }
    }
}
