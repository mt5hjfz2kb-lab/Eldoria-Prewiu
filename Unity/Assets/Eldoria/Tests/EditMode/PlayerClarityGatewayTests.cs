using System;
using System.Collections.Generic;
using Eldoria.Application;
using Eldoria.Domain;
using NUnit.Framework;
using UnityEngine;

namespace Eldoria.Tests.EditMode
{
    public sealed class PlayerClarityGatewayTests
    {
        sealed class ManualClock : IClock
        {
            public long UtcTicks { get; set; } = new DateTime(2026,11,5,12,0,0,DateTimeKind.Utc).Ticks;
            public void Advance(int seconds) { UtcTicks += TimeSpan.FromSeconds(seconds).Ticks; }
        }
        sealed class JsonStore : IStateStore
        {
            public string Json;
            public PlayerState Initial = new PlayerState();
            public int Saves;
            public PlayerState Load() => Json != null ? JsonUtility.FromJson<PlayerState>(Json) : Initial;
            public void Save(PlayerState state) { Json = JsonUtility.ToJson(state); Saves++; }
        }
        sealed class ValidResourceSite : IWorldResourcePlacement
        {
            public int Calls;
            public bool TryChoose(PlayerState snapshot,string id,string previous,int generation,
                out WorldResourceLocation location)
            {
                Calls++;
                location = new WorldResourceLocation { TargetId=id,SiteId="region1:verified:new-site",
                    X=144f,Y=1f,Z=222f,Generation=generation,ResourceKind="wood" };
                return true;
            }
        }
        sealed class ValidCreatureSite : IWorldCreaturePlacement
        {
            public int Calls;
            public bool TryChoose(PlayerState snapshot,string id,string previous,int generation,
                out WorldCreatureLocation location)
            {
                Calls++;
                location = new WorldCreatureLocation { TargetId=id,SiteId="region1:verified:creature",
                    X=300,Y=2,Z=210,Generation=generation,CreatureKind="boar",Level=3 };
                return true;
            }
        }

        [Test]
        public void NamingIsPersistedExactlyOnceWithSamePlayerIdAndExistingWallet()
        {
            var clock = new ManualClock();
            var store = new JsonStore();
            var gw = new LocalGateway(clock,store);
            var before = gw.Snapshot();
            Assert.That(before.PlayerName, Is.Empty);
            var command = new GameCommand("test:first-name",before.PlayerId,
                "ChoosePlayerName","  Álvaro   XII ",before.Revision);
            Assert.That(gw.Execute(command).Ok,Is.True);
            var chosen = gw.Snapshot();
            Assert.That(chosen.PlayerName,Is.EqualTo("Álvaro XII"));
            Assert.That(chosen.PlayerId,Is.EqualTo(before.PlayerId));
            Assert.That(chosen.Resources.Wood,Is.EqualTo(before.Resources.Wood));
            Assert.That(chosen.Resources.Stone,Is.EqualTo(before.Resources.Stone));
            Assert.That(gw.Execute(command).Ok,Is.True,"Original command ID must be replay-safe");
            Assert.That(gw.Snapshot().Revision,Is.EqualTo(chosen.Revision));
            gw = new LocalGateway(clock,store);
            Assert.That(gw.Snapshot().PlayerName,Is.EqualTo("Álvaro XII"));
            var loaded = gw.Snapshot();
            Assert.That(gw.Execute(new GameCommand("test:rename",loaded.PlayerId,
                "ChoosePlayerName","Otro nombre",loaded.Revision)).Ok,Is.False);
            Assert.That(gw.Snapshot().PlayerName,Is.EqualTo("Álvaro XII"));
        }

        [Test]
        public void InvalidNamesCannotChangeSaveOrRevision()
        {
            var clock = new ManualClock();
            var store = new JsonStore();
            var gw = new LocalGateway(clock,store);
            var before=gw.Snapshot();
            Assert.That(gw.Execute(new GameCommand("bad1",before.PlayerId,
                "ChoosePlayerName","A",before.Revision)).Ok,Is.False);
            Assert.That(gw.Execute(new GameCommand("bad2",before.PlayerId,
                "ChoosePlayerName","<script>",before.Revision)).Ok,Is.False);
            Assert.That(gw.Snapshot().PlayerName,Is.Empty);
            Assert.That(gw.Snapshot().Revision,Is.EqualTo(before.Revision));
        }

        [Test]
        public void SchemaOneOldSaveWithNoPlayerNameCanStillOpenAndChoose()
        {
            var clock = new ManualClock();
            var store=new JsonStore();
            string quote=((char)34).ToString();
            string field=quote+"PlayerName"+quote+":"+quote+quote;
            store.Json=JsonUtility.ToJson(new PlayerState())
                .Replace(field+",","").Replace(","+field,"");
            var gateway = new LocalGateway(clock,store);
            var initial = gateway.Snapshot();
            Assert.That(initial.SchemaVersion,Is.EqualTo(1));
            Assert.That(string.IsNullOrEmpty(initial.PlayerName),Is.True);
            Assert.That(gateway.Execute(new GameCommand("legacy-name",initial.PlayerId,
                "ChoosePlayerName","Lyra 7",initial.Revision)).Ok,Is.True);
            Assert.That(new LocalGateway(clock,store).Snapshot().PlayerName,Is.EqualTo("Lyra 7"));
        }

        [Test]
        public void LegacyExhaustedWoodIsRebasedToOriginalEventPlusFiveMinutesAndRerollsRealTier()
        {
            var clock=new ManualClock();
            var depleted=clock.UtcTicks-TimeSpan.FromSeconds(301).Ticks;
            var record=new WorldResourceLocation {
                TargetId="region1:wood:009", ResourceKind="wood", SiteId="old-site",
                Generation=1,Level=0,Capacity=1250,Remaining=0,
                DepletedUtcTicks=depleted,RespawnUtcTicks=depleted+TimeSpan.FromMinutes(30).Ticks
            };
            var store=new JsonStore {Initial=new PlayerState {WorldResourceLocations=
                new List<WorldResourceLocation>{record}}};
            var gateway=new LocalGateway(clock,store);
            var placement=new ValidResourceSite();
            gateway.ConfigureLocalResourceRespawn(placement,TimeSpan.FromSeconds(300));
            var found=gateway.Snapshot().WorldResourceLocations[0];
            int expected=ResourceNodeTierPolicy.ForNewSite(
                record.TargetId,found.SiteId,found.Generation);
            Assert.That(found.Generation,Is.EqualTo(2));
            Assert.That(found.SiteId,Is.EqualTo("region1:verified:new-site"));
            Assert.That(found.Level,Is.EqualTo(expected));
            Assert.That(found.Capacity,Is.EqualTo(ResourceNodeTierPolicy.Capacity("wood",expected)));
            Assert.That(found.Remaining,Is.EqualTo(found.Capacity));
            Assert.That(found.DepletedUtcTicks,Is.Zero);
            gateway=new LocalGateway(clock,store);
            var recovered=gateway.Snapshot().WorldResourceLocations[0];
            Assert.That(recovered.Level,Is.EqualTo(expected));
            Assert.That(recovered.Remaining,Is.EqualTo(found.Remaining));
            Assert.That(recovered.Generation,Is.EqualTo(2));
            Assert.That(placement.Calls,Is.EqualTo(1));
        }

        [Test]
        public void DefeatedBoarCannotReappearBeforeFiveMinutesButReturnsAcrossSaveReload()
        {
            var clock=new ManualClock();
            long defeated=clock.UtcTicks-TimeSpan.FromSeconds(299).Ticks;
            var store=new JsonStore { Initial = new PlayerState {
                WorldCreatureLocations=new List<WorldCreatureLocation>{new WorldCreatureLocation{
                    TargetId="region1:boar:003",SiteId="old-creature",CreatureKind="boar",
                    Generation=1,Level=1,DefeatedUtcTicks=defeated,
                    RespawnUtcTicks=defeated+TimeSpan.FromMinutes(15).Ticks }} } };
            var gateway=new LocalGateway(clock,store);
            var placements=new ValidCreatureSite();
            gateway.ConfigureLocalCreatureRespawn(placements,TimeSpan.FromSeconds(300));
            Assert.That(gateway.Snapshot().WorldCreatureLocations[0].Alive,Is.False);
            Assert.That(placements.Calls,Is.Zero);
            clock.Advance(1);
            gateway.Advance();
            var newCreature=gateway.Snapshot().WorldCreatureLocations[0];
            Assert.That(newCreature.Alive,Is.True);
            Assert.That(newCreature.Generation,Is.EqualTo(2));
            Assert.That(newCreature.Level,Is.EqualTo(3));
            Assert.That(newCreature.SiteId,Is.EqualTo("region1:verified:creature"));
            gateway=new LocalGateway(clock,store);
            Assert.That(gateway.Snapshot().WorldCreatureLocations[0].Alive,Is.True);
            Assert.That(gateway.Snapshot().WorldCreatureLocations[0].Generation,Is.EqualTo(2));
        }
    }
}
