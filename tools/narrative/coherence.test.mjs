import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import vm from 'node:vm';

const html=fs.readFileSync(new URL('../../v0220/index.html',import.meta.url),'utf8');
const chapters=fs.readFileSync(new URL('../../v0220/js/chapters.js',import.meta.url),'utf8');
const domain=fs.readFileSync(new URL('../../Unity/Assets/Eldoria/Scripts/Domain/GameState.cs',import.meta.url),'utf8');
const journey=fs.readFileSync(new URL('../../Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/ValoriaJourneyGuidance.cs',import.meta.url),'utf8');
const binding=fs.readFileSync(new URL('../../Unity/Assets/Eldoria/UI/ValoriaRoyalHUDV1/Bindings/ExistingSliceBinding.cs',import.meta.url),'utf8');

function part(start,end){
 const a=html.indexOf(start),b=html.indexOf(end,a+start.length);
 assert.ok(a>=0&&b>a,'runtime source segment missing: '+start);
 return html.slice(a,b);
}
function context(props={}){
 return vm.createContext({window:{ELDORIA:{tutorial:{isQA:()=>false}}},document:{querySelector:()=>null},requestAnimationFrame:fn=>fn(),setTimeout:fn=>{fn();return 1},save(){},render(){},record(){},...props});
}

test('every authored inline script parses as valid JavaScript',()=>{
 const scripts=[...html.matchAll(/<script\b([^>]*)>([\s\S]*?)<\/script>/gi)];
 let checked=0;
 for(const m of scripts){
  if(!m[2].trim())continue;
  assert.doesNotThrow(()=>new vm.Script(m[2]));checked++;
 }
 assert.ok(checked>=2);
 assert.doesNotThrow(()=>new vm.Script(chapters));
});

test('narrative queue serializes speakers, blocks modal overlap and calls completion last',()=>{
 const events=[],callbacks=[],frames=[],timers=[];
 let blocked=true;
 const ctx=context({
  document:{querySelector:()=>blocked?{}:null},
  dialogue:(speaker,copy,done)=>{events.push(speaker+':'+copy);callbacks.push(done)},
  setTimeout:fn=>{timers.push(fn);return timers.length},
  requestAnimationFrame:fn=>frames.push(fn)
 });
 vm.runInContext(part('let narrativeQueue=[]','function exploreFissure('),ctx);
 let completed=0;
 ctx.storySequence([['NARRADOR','Primero'],['Sir Aldric','Segundo']],()=>completed++);
 ctx.storySequence([['Lyra','Tercero']],()=>completed++);
 assert.equal(events.length,0,'must not interrupt existing modal');
 blocked=false;timers.shift()();
 assert.deepEqual(events,['NARRADOR:Primero']);
 callbacks.shift()();
 assert.deepEqual(events,['NARRADOR:Primero','Sir Aldric:Segundo']);
 assert.equal(completed,0);
 callbacks.shift()();
 assert.equal(completed,1);
 frames.shift()();
 assert.deepEqual(events,['NARRADOR:Primero','Sir Aldric:Segundo','Lyra:Tercero']);
 callbacks.shift()();
 assert.equal(completed,2);
});

test('first-use beat only acknowledges after the player closes it, never in QA',()=>{
 const s={bastionLevel:1,sawmill:false,tutorialSeen:{}};
 let acknowledgement=null,saves=0;
 const ctx=context({s,save:()=>saves++,guideDialogue:(_speaker,_copy,done)=>{acknowledgement=done;return true}});
 vm.runInContext(part('function tutorialBeat(){','function toastFeedback026('),ctx);
 ctx.tutorialBeat();
 assert.equal(s.tutorialSeen.start,undefined);
 assert.equal(saves,0);
 assert.equal(typeof acknowledgement,'function');
 acknowledgement();
 assert.equal(s.tutorialSeen.start,true);
 assert.equal(saves,1);
 s.tutorialSeen={};
 ctx.window.ELDORIA.tutorial.isQA=()=>true;
 acknowledgement=null;ctx.tutorialBeat();
 assert.equal(acknowledgement,null);
 assert.equal(s.tutorialSeen.start,undefined);
});

test('first-world introduction has no Lyra/Fissure spoilers and acknowledges after reading',()=>{
 const s={sawmill:true,view:'world',tutorialSeen:{}};
 let entry=null;
 const ctx=context({s,guideDialogue:(who,text,done)=>{entry={who,text,done};return true}});
 vm.runInContext(part('function worldIntro(){','function playWorldMarch0304('),ctx);
 ctx.worldIntro();
 assert.equal(s.tutorialSeen.worldMap,undefined);
 assert.ok(entry);
 assert.doesNotMatch(entry.text,/Lyra|Fisura|reclutamiento|aliad[oa]/i);
 entry.done();
 assert.equal(s.tutorialSeen.worldMap,true);
});

test('Relicario teaching is progressive, requires a first choice, and Orin appears later',()=>{
 const s={view:'relicario',relicTutorial:{},relicTutorialComplete:false,relicSidesRevealed:false,relicarioTab:'collection'};
 let progress=1;
 const queued=[];
 const ctx=context({s,relicDiscoveryCount027:()=>progress,storySequence:(lines,done)=>queued.push({lines,done})});
 vm.runInContext(part('let codexIntroPending=false','function completeRelicIntro031('),ctx);
 vm.runInContext(part('function completeRelicIntro031(){','function useRelic('),ctx);
 ctx.maybeCodexTutorial();
 assert.equal(queued.length,1);
 assert.doesNotMatch(queued[0].lines.map(x=>x[1]).join(' '),/legendaria|Indestructible|Norte|Sur/i);
 assert.equal(s.relicTutorial.started,undefined);
 queued.shift().done();
 assert.equal(s.relicTutorial.started,true);
 assert.equal(s.relicTutorialComplete,false);
 progress=2;ctx.maybeCodexTutorial();
 assert.equal(queued.length,0,'rarity teaching must wait until real first decision');
 ctx.completeRelicIntro031();
 assert.equal(s.relicTutorialComplete,true);
 ctx.maybeCodexTutorial();assert.equal(queued.length,1);
 assert.match(queued[0].lines.map(x=>x[1]).join(' '),/Indestructible/);
 queued.shift().done();
 assert.equal(s.relicTutorial.rarities,true);
 progress=3;ctx.maybeCodexTutorial();assert.equal(queued.length,1);
 assert.equal(queued[0].lines[0][0],'Maestre Orin');
 assert.equal(s.relicSidesRevealed,false);
 queued.shift().done();
 assert.equal(s.relicSidesRevealed,true);
});

test('Bastion story resumes a pending milestone and only marks it read after its last speaker',()=>{
 let staged,completions=0;
 const s={storySeen:{},bastionLevel:3,chapterProgress:{}};
 const ctx=context({s,storySequence:(lines,done)=>{staged={lines,done}},save(){},render(){}});
 vm.runInContext(part('let narrativeQueue=[]','function exploreFissure('),ctx);
 // Replace the renderer with a controlled acknowledgement harness before
 // invoking Bastion prose; no DOM or timer can accidentally consume the beat.
 ctx.storySequence=(lines,done)=>{staged={lines,done}};
 vm.runInContext(part('function bastionNarrative(n){','function upgradeMaterial('),ctx);
 ctx.bastionNarrative(3);
 assert.equal(s.storySeen['bastion-3'],undefined);
 assert.equal(s.pendingBastionNarrative,3);
 assert.doesNotMatch(staged.lines.map(l=>l[1]).join(' '),/Allí espera Lyra|buscando a su hermano/i);
 ctx.bastionNarrative(3);
 assert.equal(completions,0);
 staged.done();completions++;
 assert.equal(s.storySeen['bastion-3'],true);
 assert.equal(s.pendingBastionNarrative,0);
});

test('chapter introduction is committed after the last page, not when scheduled',()=>{
 const win={ELDORIA:{gameplay:{CHAPTERS:[]}}};
 const s={introSeen:true,missionIntroSeen:false,bastionLevel:1};
 const ctx=context({window:win,document:{querySelector:()=>null,getElementById:()=>true}});
 vm.runInContext(chapters,ctx);
 let ack=null,saves=0;
 win.ELDORIA.chapters.intro(s,{isQA:()=>false,save:()=>saves++,story:(_lines,done)=>ack=done});
 assert.equal(s.missionIntroSeen,false);
 assert.equal(typeof ack,'function');
 ack();
 assert.equal(s.missionIntroSeen,true);
 assert.equal(saves,1);
});

test('Unity prevents pre-discovery spoilers and orders saved narrative milestones',()=>{
 assert.match(domain,/public static class NarrativeCoherenceRules/);
 assert.match(domain,/FissureKnowledgeStage\(PlayerState s\)/);
 assert.match(domain,/NextUnseenBeat\(PlayerState s\)/);
 assert.match(domain,/level>=3&&level<=4&&c\?\.FissureClosed==true/);
 assert.match(journey,/NarrativeCoherenceRules.NextUnseenBeat\(s\)/);
 assert.match(journey,/if\(string.IsNullOrEmpty\(beat\)\)return;/);
 assert.doesNotMatch(binding,/Una fisura local\. Lyra busca a su hermano/);
 assert.doesNotMatch(binding,/case "b3.lyra":title=L\(en,"Conoce a Lyra"/);
 assert.match(binding,/case "b3.lyra":title=L\(en,"Investiga las huellas humanas"/);
 assert.match(html,/s.fissureExplored\?'ACERCARSE A LA ARQUERA'/);
 assert.match(binding,/knowledge==0\?/);
 assert.match(html,/maybeCodexTutorial\(\);tutorialBeat\(\)/);
 assert.match(html,/s.bastionLevel===9&&!s.trialWon/);
 assert.doesNotMatch(html,/Bastión IX es la prueba de autonomía/);
 assert.doesNotMatch(html,/En las vetas más profundas, sin embargo, aparecen fragmentos/);
 assert.match(html,/popup\('CAPÍTULO I COMPLETADO'/);
 assert.match(chapters,/ETAPA '\+ch.id/);
});
