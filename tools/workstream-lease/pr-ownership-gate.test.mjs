import {test} from 'node:test';
import assert from 'node:assert/strict';
import {claimId,evaluate,globMatch} from './pr-ownership-gate.mjs';
const ui='eldoria-ui-v3-management-completion-20261021';
const art='eldoria-tripo-art-brain-phase2-20261021';
const registry={active:[
 {id:ui,status:'active',ownership_state:'claimed',owner:'cronicareal',scope:['Unity/Assets/Eldoria/UI/**']},
 {id:art,status:'active',ownership_state:'claimed',owner:'world-artist',scope:['Unity/Assets/Eldoria/SpawnlingBastion2/**']},
 {id:'valoria-master-plan-layered-construction-20261016',status:'active',ownership_state:'available',owner:null,scope:['Unity/Assets/Eldoria/**']}
]};
const pr=(number,title,body='',created_at='2026-10-27T08:08:36Z')=>({number,title,body,created_at,head:{ref:'fix/ui-cronica-real-reference-20261027'}});
const original=pr(115,'UI · restaurar Crónica Real original');
const duplicate=pr(116,'[DRAFT] UI Crónica Real — recuperar composición original y navegación a bordes seguros','','2026-10-27T08:11:56Z');
const unrelated=pr(98,'[ISSUE #95] Strategic world overview Spawnling','','2026-10-22T00:00:00Z');
unrelated.head.ref='fix/issue95-strategic-world-spawnling-ui-20261022';
test('real 115 and 116 conflict; original retains its priority',()=>{
 const b=evaluate({current:duplicate,openPulls:[original,duplicate],registry});
 assert.equal(b.ok,false);assert.match(b.errors.join(' '),/duplicates canonical workstream/);
 assert.equal(evaluate({current:original,openPulls:[original,duplicate],registry}).ok,true);
});
test('closed 116 does not conflict; unrelated PR 98 is not mislabeled as UI',()=>{
 assert.equal(claimId(unrelated),null);
 assert.equal(evaluate({current:original,openPulls:[original,unrelated],registry}).ok,true);
});
test('new PR cannot proceed without a real active canonical owner marker',()=>{
 assert.equal(evaluate({current:pr(117,'UI alternate'),openPulls:[original],registry}).ok,false);
 const d=pr(117,'new PR','Workstream-ID: '+ui);
 assert.equal(evaluate({current:d,openPulls:[original,d],registry,changedFiles:['Unity/Assets/Eldoria/UI/Hud.cs']}).ok,false);
 assert.equal(evaluate({current:pr(118,'fake','Workstream-ID: invented'),openPulls:[],registry,changedFiles:[]}).ok,false);
});
test('valid distinct work proceeds but cannot invade claimed foreign scope',()=>{
 const a=pr(117,'world pilot','Workstream-ID: '+art);
 assert.equal(evaluate({current:a,openPulls:[original,a],registry,changedFiles:['Unity/Assets/Eldoria/SpawnlingBastion2/Test.cs']}).ok,true);
 assert.equal(evaluate({current:a,openPulls:[original,a],registry,changedFiles:['Unity/Assets/Eldoria/UI/Hud.cs']}).ok,false);
});
test('same live owner can combine two canonical scopes while foreign owner stays excluded',()=>{
 const owner='chatgpt-cronica-real-ux-polish-20261027';
 const two={active:registry.active.map(x=>({...x}))};
 two.active[0].owner=owner;
 two.active[2].owner=owner;
 two.active[2].ownership_state='claimed';
 const follow=pr(119,'Crónica Real follow-up','Workstream-ID: '+ui,'2026-10-27T22:20:52Z');
 const changed=['Unity/Assets/Eldoria/UI/Hud.cs','Unity/Assets/Eldoria/City/Camera.cs'];
 const ok=evaluate({current:follow,openPulls:[follow],registry:two,changedFiles:changed});
 assert.equal(ok.ok,true,ok.errors.join('; '));
 assert.match(ok.notes.join(' '),/same live owner/i);
 two.active[2].owner='another-chat';
 const bad=evaluate({current:follow,openPulls:[follow],registry:two,changedFiles:changed});
 assert.equal(bad.ok,false);
 assert.match(bad.errors.join(' '),/another claimed workstream/);
});

test('exact and wildcard scopes match only intended paths',()=>{
 assert.equal(globMatch('Unity/Assets/Eldoria/UI/**','Unity/Assets/Eldoria/UI/Hud.cs'),true);
 assert.equal(globMatch('Unity/Assets/Eldoria/UI/**','Unity/Assets/Eldoria/UI_old/Hud.cs'),false);
 assert.equal(globMatch('docs/**/foo-*.md','docs/a/b/foo-1.md'),true);
 assert.equal(globMatch('README.md','x/README.md'),false);
});
test('missing live registry/PR observation fails closed',()=>{
 assert.equal(evaluate({current:original,openPulls:null,registry}).ok,false);
 assert.equal(evaluate({current:null,openPulls:[],registry}).ok,false);
});
