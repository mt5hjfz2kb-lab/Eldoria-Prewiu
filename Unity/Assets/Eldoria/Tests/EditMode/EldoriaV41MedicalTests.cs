using System;
using NUnit.Framework;
using UnityEngine;
using Eldoria.Domain;
namespace Eldoria.Tests
{
    public sealed class EldoriaV41MedicalTests
    {
        static EldoriaV41Medical.Arrival Arrival(string id,long ticks)=>new EldoriaV41Medical.Arrival{
            EventId=id,Mode="pve",UtcTicks=ticks,TreatmentTicks=10,
            Deployed=new ArmyRoster{PaladinT5=4,ArcherT1=6},
            Graves=new ArmyRoster{PaladinT5=2,ArcherT1=3},DirectDeaths=new ArmyRoster()};
        [Test] public void HospitalReceiptsAndTreatmentsSurviveUnitySerializationWithoutDuplicatingSoldiers()
        {
            var state=new EldoriaV41Medical.Hospital{FeatureOperational=true,Capacity=3};
            var first=EldoriaV41Medical.PlanReturns(state,new[]{Arrival("return:a",100)},100);
            Assert.That(first.Returned.Total,Is.EqualTo(5));
            Assert.That(first.Applied[0].Admitted.Total,Is.EqualTo(3));
            Assert.That(first.Applied[0].OverflowDeaths.Total,Is.EqualTo(2));
            var saved=JsonUtility.FromJson<EldoriaV41Medical.Hospital>(JsonUtility.ToJson(first.State));
            var replay=EldoriaV41Medical.PlanReturns(saved,new[]{Arrival("return:a",100)},100);
            Assert.That(replay.Returned.Total,Is.Zero);
            var healed=EldoriaV41Medical.PlanReturns(replay.State,Array.Empty<EldoriaV41Medical.Arrival>(),110);
            Assert.That(healed.Returned.Total,Is.EqualTo(3));
            Assert.That(healed.State.Treatments,Is.Empty);
            Assert.That(healed.State.Receipts[0].OverflowDeaths.Total,Is.EqualTo(2));
        }
        [Test] public void MissingOperationalHospitalProtectsEveryDeployedTroop()
        {
            var result=EldoriaV41Medical.PlanReturns(new EldoriaV41Medical.Hospital(),new[]{Arrival("return:a",100)},100);
            Assert.That(result.Returned.Total,Is.EqualTo(10));
            Assert.That(result.Applied[0].OverflowDeaths.Total,Is.Zero);
        }
        [Test] public void ReusedMedicalEventWithDifferentLossesIsRejectedBeforeMutation()
        {
            var first=EldoriaV41Medical.PlanReturns(new EldoriaV41Medical.Hospital{FeatureOperational=true,Capacity=3},new[]{Arrival("return:a",100)},100);
            var changed=Arrival("return:a",100);changed.Graves.PaladinT5=3;
            Assert.Throws<ArgumentException>(()=>EldoriaV41Medical.PlanReturns(first.State,new[]{changed},100));
            Assert.That(first.State.Receipts.Count,Is.EqualTo(1));
            Assert.That(first.State.Receipts[0].OverflowDeaths.Total,Is.EqualTo(2));
        }
    }
}
