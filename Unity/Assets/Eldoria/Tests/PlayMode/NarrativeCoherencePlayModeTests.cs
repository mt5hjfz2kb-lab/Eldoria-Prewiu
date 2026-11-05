using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eldoria.Application;
using Eldoria.Domain;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Eldoria.Tests
{
    // Genuine integrated scene; no invented dialogues and no production save.
    // Run with VALORIA_GAMEPLAY_PROBE=1, which creates a unique disposable save.
    public sealed class NarrativeCoherencePlayModeTests
    {
        static object Read(object target,string name)
        {
            Assert.That(target,Is.Not.Null,name+" host missing");
            var type=target.GetType();
            var prop=type.GetProperty(name,BindingFlags.Public|BindingFlags.Instance);
            if(prop!=null)return prop.GetValue(target);
            var field=type.GetField(name,BindingFlags.Public|BindingFlags.Instance);
            Assert.That(field,Is.Not.Null,type.Name+"."+name+" missing");
            return field.GetValue(target);
        }

        [UnityTest]
        public IEnumerator OpeningIsAcknowledgedOnlyAfterLastRealDialogueAndPersistsAfterReload()
        {
            if(Environment.GetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE")!="1")
                Assert.Ignore("Requires the project's isolated native probe save");
            SceneManager.LoadScene("Valoria_City_World_UI");
            Component host=null,hud=null;
            for(int frame=0;frame<180;frame++)
            {
                yield return null;
                host=Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                    .FirstOrDefault(x=>x&&x.GetType().Name=="ValoriaIntegratedWorld");
                if(!host||(bool)Read(host,"Ready")==false)continue;
                hud=Read(host,"Hud") as Component;
                if(hud)break;
            }
            Assert.That(hud,Is.Not.Null,"Canonical playable HUD must exist");
            var city=Read(host,"City");
            Assert.That((bool)Read(city,"Probe"),Is.True,"Never access a real player's save");
            var gateway=Read(city,"Gateway") as ICommandGateway;
            Assert.That(gateway,Is.Not.Null);
            var before=gateway.Snapshot();
            Assert.That(before.BastionLevel,Is.EqualTo(1));
            Assert.That(before.ChapterProgress.SeenNarrative,Does.Not.Contain("start"),
                "A fresh player has not acknowledged the prologue before reading");
            var guidance=Read(host,"Guidance") as Behaviour;
            Assert.That(guidance,Is.Not.Null);
            guidance.enabled=false; // prevent any automatic second scene during assertions
            if(!(bool)Read(hud,"NarrativeOpen"))
                guidance.SendMessage("PresentMilestone",SendMessageOptions.RequireReceiver);
            Assert.That((bool)Read(hud,"NarrativeOpen"),Is.True,
                "Real narrator must introduce Valoria before any first-use hints");
            var modal=Read(hud,"Modal");
            var next=Read(modal,"Extra") as Button;
            Assert.That(next,Is.Not.Null);
            var turns=new List<string>();
            for(int tap=0;tap<40&&(bool)Read(hud,"NarrativeOpen");tap++)
            {
                var who=(string)Read(hud,"NarrativeSpeaker");
                if(turns.Count==0||turns[turns.Count-1]!=who)turns.Add(who);
                Assert.That(gateway.Snapshot().ChapterProgress.SeenNarrative,
                    Does.Not.Contain("start"),"Partial reading cannot acknowledge the prologue");
                next.onClick.Invoke();
                yield return null;
            }
            Assert.That((bool)Read(hud,"NarrativeOpen"),Is.False,
                "Narrative sequence must finish, without trapping interaction");
            CollectionAssert.AreEqual(new[]{"NARRADOR","Sir Aldric","NARRADOR"},turns,
                "Only canonical narrator and Aldric turns, in their authored order");
            Assert.That(gateway.Snapshot().ChapterProgress.SeenNarrative,Does.Contain("start"),
                "Finish the last actual page before the save acknowledges the scene");
            city.GetType().GetMethod("ReloadForProbe",BindingFlags.Public|BindingFlags.Instance)
                .Invoke(city,null);
            var reloaded=Read(city,"Gateway") as ICommandGateway;
            Assert.That(reloaded.Snapshot().ChapterProgress.SeenNarrative,Does.Contain("start"),
                "Narrative acknowledgement survives the real persisted reload");
            Assert.That(reloaded.Snapshot().ChapterProgress.SeenNarrative.Count(x=>x=="start"),
                Is.EqualTo(1),"Prologue is never stored twice");
        }
    }
}
