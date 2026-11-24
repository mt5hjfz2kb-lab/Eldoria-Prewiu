"""Exercise the integration on an isolated temporary checkout copy only.
No in-place option. Fail if any upstream source anchor changed.
"""
from pathlib import Path
import shutil, tempfile, subprocess, json

REPO = Path(__file__).resolve().parents[3]
with tempfile.TemporaryDirectory(prefix="eldoria-legacy-integration-") as tmp:
    root=Path(tmp)
    src=Path("Unity/Assets/Eldoria")
    for rel in [src/"LegacyProgression",src/"Scripts/Domain",src/"Scripts/Application",Path("tools/legacy-progression")]:
        shutil.copytree(REPO/rel,root/rel)
    domain=root/src/"Scripts/Domain"
    app=root/src/"Scripts/Application"
    core=root/src/"LegacyProgression"
    tool=root/"tools/legacy-progression"
    def replace(path, old, new):
        text=path.read_text(encoding="utf-8-sig")
        if text.count(old)!=1:
            raise RuntimeError(f"Upstream anchor mismatch {path.name}: {old[:90]}")
        path.write_text(text.replace(old,new,1),encoding="utf-8")
    # Additive field, initialized at load and deep-copied in Snapshot.
    replace(domain/"GameState.cs","    [Serializable] public sealed class PlayerState\n    {",
        "    [Serializable] public sealed class PlayerState\n    {\n        public Eldoria.LegacyProgression.LegacySavedProgress LegacyProgression;")
    g=app/"LocalGateway.cs"
    replace(g,"using Eldoria.Domain;","using Eldoria.Domain;\nusing Eldoria.LegacyProgression;")
    replace(g,"            state = store.Load() ?? new PlayerState();",
        """            state = store.Load() ?? new PlayerState();
            bool legacyMigrated=state.LegacyProgression==null;
            if(legacyMigrated)state.LegacyProgression=new LegacySavedProgress();
            state.LegacyProgression.BindIdentity(state.PlayerId,state.WorldId);""")
    replace(g,"            if(!Advance() && migratedPrepared)Commit();",
        "            if(!Advance() && (migratedPrepared||legacyMigrated||CanonicalLegacyFacts.Reconcile(state)))Commit();")
    replace(g,"                Retention=s.Retention?.Copy()??new EarlyRetentionState(),",
        "                Retention=s.Retention?.Copy()??new EarlyRetentionState(),\n                LegacyProgression=s.LegacyProgression.Copy(),")
    replace(g,'            if (command.Kind == "ChoosePlayerName")',
        """            if(command.Kind=="SelectLegacyTitle"||command.Kind=="SelectLegacyBanner"||command.Kind=="SelectLegacyMonument")
            {
                string kind=command.Kind=="SelectLegacyTitle"?"title":command.Kind=="SelectLegacyBanner"?"banner":"monument";
                if(!state.LegacyProgression.TrySelect(CanonicalLegacyFacts.Read(state),kind,command.TargetId))
                    return Fail("Distinción no desbloqueada");
                state.CompletedCommandIds.Add(command.Id);
                Commit();
                return new CommandResult(true,"Distinción guardada",state.Revision);
            }
            if (command.Kind == "ChoosePlayerName")""")
    replace(g,'                if(command.Kind=="VigilantBattle" && state.LastBattleReport!=null &&',
        """                if(command.Kind=="VigilantBattle"&&state.LastBattleReport?.Won==true)
                    CanonicalLegacyFacts.Record(state,"vigilant:"+command.Id,ReceiptKind.VigilantClear,
                        "stage:"+state.Retention.LastClearedStage,clock.UtcTicks,1,state.Retention.LastClearedStage);
                if(command.Kind=="VigilantBattle" && state.LastBattleReport!=null &&""")
    replace(g,'                    state.Mailbox.Receive(new EldoriaV41Message {\n                        EventId="exploration:"+command.Id',
        """                    if(claimed!=null)
                        CanonicalLegacyFacts.Record(state,"scout:"+claimed.Id,ReceiptKind.ScoutDiscovery,
                            "family:"+claimed.Kind,clock.UtcTicks,1,0);
                    state.Mailbox.Receive(new EldoriaV41Message {
                        EventId="exploration:"+command.Id""")
    replace(g,"                                creature.DefeatedUtcTicks=due;",
        """                                CanonicalLegacyFacts.Record(state,"creature:"+m.MarchId+":"+m.TargetId+":"+due,
                                    ReceiptKind.HuntVictory,"regional:"+creature.CreatureKind,due,1,creature.Level);
                                creature.DefeatedUtcTicks=due;""")
    replace(g,"                        state.CompletedTaskIds.Add(id);\n                        if(m.PendingFood>0 || m.PendingWood>0 || m.PendingStone>0)",
        """                        if(IsWoodTarget(m.TargetId)&&m.PendingWood>0)
                            CanonicalLegacyFacts.Record(state,id+":wood",ReceiptKind.GatherReturn,"wood",due,m.PendingWood,0);
                        if(IsStoneTarget(m.TargetId)&&m.PendingStone>0)
                            CanonicalLegacyFacts.Record(state,id+":stone",ReceiptKind.GatherReturn,"stone",due,m.PendingStone,0);
                        if(m.TargetId=="meat-valoria"&&m.PendingFood>0)
                            CanonicalLegacyFacts.Record(state,id+":food",ReceiptKind.GatherReturn,"food",due,m.PendingFood,0);
                        state.CompletedTaskIds.Add(id);
                        if(m.PendingFood>0 || m.PendingWood>0 || m.PendingStone>0)""")
    # Story hunts have no real numeric tier: base 1 is a prospective scoring convention,
    # not retroactive reconstruction. Do not classify the fissure or campaign bosses as hunting.
    replace(g,'                c.Hunts++;bool wolf=m.TargetId=="hunt-wolf";m.PendingFood=wolf?140:190;',
        """                c.Hunts++;bool wolf=m.TargetId=="hunt-wolf";m.PendingFood=wolf?140:190;
                CanonicalLegacyFacts.Record(state,"hunt:"+m.MarchId+":"+m.TargetId+":"+due,
                    ReceiptKind.HuntVictory,wolf?"legacy:wolf":"legacy:boar",due,1,1);""")
    replace(g,"        private void Commit() { state.Revision++; store.Save(state); }",
        "        private void Commit() { CanonicalLegacyFacts.Reconcile(state); state.Revision++; store.Save(state); }")
    # Pure core can be referenced by Domain. Adapter depends on Domain+Application,
    # avoiding a circular dependency in a future Unity integration.
    def edit_json(path, update):
        data=json.loads(path.read_text());update(data);path.write_text(json.dumps(data,indent=2)+"\n")
    edit_json(core/"Eldoria.LegacyProgression.asmdef",lambda d:d.update(references=[]))
    edit_json(domain/"Eldoria.Domain.asmdef",lambda d:d["references"].append("Eldoria.LegacyProgression"))
    edit_json(app/"Eldoria.Application.asmdef",lambda d:d["references"].append("Eldoria.LegacyProgression"))
    adapter=core/"Adapter";adapter.mkdir()
    for name in ["LegacyPlayerAdapter.cs","LegacyPlayerAdapter.cs.meta"]:
        shutil.move(core/name,adapter/name)
    (adapter/"Eldoria.LegacyProgression.Adapter.asmdef").write_text(json.dumps({
        "name":"Eldoria.LegacyProgression.Adapter","rootNamespace":"Eldoria.LegacyProgression",
        "references":["Eldoria.LegacyProgression","Eldoria.Domain","Eldoria.Application"],"autoReferenced":True},indent=2))
    edit_json(core/"UI/Eldoria.LegacyProgression.UI.asmdef",lambda d:d["references"].append("Eldoria.LegacyProgression.Adapter"))
    (adapter/"LegacyPlayerAdapter.cs").write_text("""using System;
using Eldoria.Domain;
using Eldoria.Application;
namespace Eldoria.LegacyProgression {
 public static class LegacyPlayerAdapter {
  public static LocalProfile Read(ICommandGateway gateway) => Project(gateway.Snapshot());
  public static LocalProfile Project(PlayerState s) => new LocalProfile(s.PlayerName,CanonicalLegacyFacts.Read(s),s.LegacyProgression);
 }
}""")
    shutil.copy(tool/"integration/CanonicalLegacyFacts.cs",domain/"CanonicalLegacyFacts.cs")
    proj=tool/"Smoke.csproj"
    replace(proj,'<Compile Include="Program.cs"/>','<Compile Include="integration/IntegrationChecks.cs"/>')
    replace(proj,'LegacyProgression/LegacyPlayerAdapter.cs','LegacyProgression/Adapter/LegacyPlayerAdapter.cs')
    replace(proj,'  </ItemGroup>','    <Compile Include="../../Unity/Assets/Eldoria/Scripts/Domain/CanonicalLegacyFacts.cs"/>\n  </ItemGroup>')
    subprocess.run(["dotnet","run","--project",str(proj),"--configuration","Release"],check=True,cwd=root)
