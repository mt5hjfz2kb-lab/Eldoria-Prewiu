using System;
using Eldoria.Application;
using Eldoria.Domain;
using NUnit.Framework;
using UnityEngine;

namespace Eldoria.Tests
{
    public sealed class PlayerClarityGatewayIntegrationTests
    {
        private sealed class FrozenClock : IClock { public long UtcTicks {get;set;} }
        private sealed class JsonStore : IStateStore
        {
            public string Payload;
            public JsonStore(PlayerState s) { Payload=JsonUtility.ToJson(s); }
            public PlayerState Load() => JsonUtility.FromJson<PlayerState>(Payload);
            public void Save(PlayerState state) { Payload=JsonUtility.ToJson(state); }
        }
        private sealed class TerrainFixture : IWorldResourcePlacement,IWorldCreaturePlacement
        {
            public bool TryChoose(PlayerState snapshot,string target,string prior,int generation,
                out WorldResourceLocation site)
            {
                site=new WorldResourceLocation{
                    TargetId=target,ResourceKind="wood",SiteId="verified-new-wood",
                    X=120,Y=3,Z=230,Generation=generation
                };
                return true;
            }
            public bool TryChoose(PlayerState snapshot,string target,string prior,int generation,
                out WorldCreatureLocation site)
            {
                site=new WorldCreatureLocation{
                    TargetId=target,CreatureKind="boar",Level=3,
                    SiteId="verified-new-boar",Generation=generation,
                    X=250,Y=3,Z=350
                };
                return true;
            }
        }

        [Test]
        public void UnityJsonRoundTripPersistsNicknameAndBothPopulationTiers()
        {
            var state=new PlayerState{PlayerName="Áurea_7"};
            state.WorldResourceLocations.Add(new WorldResourceLocation{
                TargetId="region1:wood:001",ResourceKind="wood",SiteId="wood-123",
                Level=3,Capacity=ResourceNodeTierPolicy.Capacity("wood",3),
                Remaining=300,Generation=3, X=110,Y=5,Z=220
            });
            state.WorldCreatureLocations.Add(new WorldCreatureLocation{
                TargetId="region1:boar:001",CreatureKind="boar",Level=4,
                Generation=2,SiteId="boar-456", X=270,Y=5,Z=310
            });
            var copy=JsonUtility.FromJson<PlayerState>(JsonUtility.ToJson(state));
            Assert.AreEqual(state.PlayerName,copy.PlayerName);
            Assert.AreEqual(3,copy.WorldResourceLocations[0].Level);
            Assert.AreEqual(300,copy.WorldResourceLocations[0].Remaining);
            Assert.AreEqual(4,copy.WorldCreatureLocations[0].Level);
            Assert.AreEqual(state.PlayerId,copy.PlayerId);
        }

        [Test]
        public void LegacyJsonSaveKeepsProgressAndSpawnsAfterActualFiveMinuteDeadline()
        {
            long eventAt=new DateTime(2026,11,5,13,0,0,DateTimeKind.Utc).Ticks;
            var state=new PlayerState{PlayerName=""};
            state.WorldResourceLocations.Add(new WorldResourceLocation{
                TargetId="region1:wood:001",SiteId="old",
                ResourceKind="wood",Generation=1,
                Remaining=0,Capacity=1250,Level=0,
                DepletedUtcTicks=eventAt,
                RespawnUtcTicks=eventAt+TimeSpan.FromMinutes(30).Ticks
            });
            state.WorldCreatureLocations.Add(new WorldCreatureLocation{
                TargetId="region1:boar:001",SiteId="old-boar",
                CreatureKind="boar",Generation=1,Level=2,
                DefeatedUtcTicks=eventAt,
                RespawnUtcTicks=eventAt+TimeSpan.FromMinutes(15).Ticks
            });
            var store=new JsonStore(state);
            // Mimics missing fields in a shipped schema-1 JSON save.
            store.Payload=store.Payload.Replace(",\"PlayerName\":\"\"","")
                .Replace(",\"Level\":0","");
            var clock=new FrozenClock{UtcTicks=eventAt+TimeSpan.FromMinutes(6).Ticks};
            var gateway=new LocalGateway(clock,store);
            Assert.AreEqual("",gateway.Snapshot().PlayerName);
            Assert.AreEqual(1,gateway.Snapshot().WorldResourceLocations[0].Level);
            var sites=new TerrainFixture();
            gateway.ConfigureLocalResourceRespawn(sites,RenewablePopulationPolicy.Delay);
            gateway.ConfigureLocalCreatureRespawn(sites,RenewablePopulationPolicy.Delay);
            var snapshot=gateway.Snapshot();
            var wood=snapshot.WorldResourceLocations[0];
            var boar=snapshot.WorldCreatureLocations[0];
            Assert.AreEqual(2,wood.Generation);
            Assert.AreEqual(2,boar.Generation);
            Assert.AreEqual(wood.Capacity,wood.Remaining);
            Assert.AreEqual(ResourceNodeTierPolicy.Capacity("wood",wood.Level),wood.Capacity);
            Assert.AreEqual(0,wood.RespawnUtcTicks);
            Assert.AreEqual(0,boar.DefeatedUtcTicks);
            var restored=new LocalGateway(clock,store).Snapshot();
            Assert.AreEqual(wood.Level,restored.WorldResourceLocations[0].Level);
            Assert.AreEqual(boar.Level,restored.WorldCreatureLocations[0].Level);
        }

        [Test]
        public void NameIsSavedOnceWithIdempotentReceiptsAndDoesNotChangeSaveOwnerId()
        {
            var store=new JsonStore(new PlayerState());
            var clock=new FrozenClock{UtcTicks=new DateTime(2026,11,5,13,0,0,DateTimeKind.Utc).Ticks};
            var gateway=new LocalGateway(clock,store);
            var old=gateway.Snapshot();
            var first=gateway.Execute(new GameCommand("create-name-1",old.PlayerId,
                "ChoosePlayerName","  Sir   Álvaro  ",old.Revision));
            Assert.IsTrue(first.Ok,first.Message);
            Assert.AreEqual("Sir Álvaro",gateway.Snapshot().PlayerName);
            Assert.AreEqual(old.PlayerId,gateway.Snapshot().PlayerId);
            Assert.IsTrue(gateway.Execute(new GameCommand("create-name-1",old.PlayerId,
                "ChoosePlayerName","Sir Álvaro",old.Revision)).Ok);
            var beforeRename=gateway.Snapshot();
            Assert.IsFalse(gateway.Execute(new GameCommand("rename",old.PlayerId,
                "ChoosePlayerName","Other Knight",beforeRename.Revision)).Ok);
            Assert.AreEqual("Sir Álvaro",new LocalGateway(clock,store).Snapshot().PlayerName);
        }
    }
}
