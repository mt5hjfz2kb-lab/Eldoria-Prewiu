// Evaluate only the pinned existing game catalog in a minimal read-only DOM.
import {readFileSync,writeFileSync} from "node:fs";
import vm from "node:vm";
import assert from "node:assert/strict";
const [source,dest]=process.argv.slice(2),context={window:{},document:{getElementById:()=>({})}};
vm.createContext(context,{codeGeneration:{strings:false,wasm:false}});
vm.runInContext(readFileSync(source,"utf8"),context,{timeout:1000});
const R=context.window.ELDORIA.relicario,c=JSON.parse(JSON.stringify(R.spec("corona-valoria")));
assert.equal(R.CATALOG.length,15);
assert.equal(c.rarity,"legendary");assert.equal(c.name,"Corona de Valoria");assert.deepEqual(c.values,{N:8,S:7,E:6,O:7});assert.equal(c.effectKey,"construction50");assert.equal(c.effect,"Aumenta un 50 % la velocidad de construcción durante exactamente 4 horas.");assert.equal(c.duel,null);
assert.equal(R.known({bastionLevel:9},R.spec(c.id)),false);assert.equal(R.known({bastionLevel:10},R.spec(c.id)),true);
const state={codex:[],relicarioUnlocked:true,bastionLevel:10};R.grant(state,c.id,{indestructible:false,source:"isolated-fixture"});
const result=R.use(state,c.id,{now:Date.now()});assert.equal(result.ok,true);assert.equal(result.consumed,true);assert.equal(state.relicEffects.constructionSpeedPct.pct,50);assert.equal(state.relicEffects.constructionSpeedPct.end-state.relicEffects.constructionSpeedPct.start,4*3600000);
const ind={codex:[],relicarioUnlocked:true,bastionLevel:10};R.grant(ind,c.id,{indestructible:true,source:"isolated-fixture"});assert.equal(R.use(ind,c.id,{now:Date.now()}).indestructible,true);assert.equal(ind.codex.length,1);
const record={schema:"eldoria.relicario.card.v1",id:c.id,name:c.name,rarity:c.rarity,values:{N:c.values.N,E:c.values.E,S:c.values.S,O:c.values.O},effectKey:c.effectKey,effect:c.effect,effectShort:"+50 % velocidad de construcción · 4 h.",effectIcon:"assets/icon_construction.svg",illustration:"assets/illustration.png",duel:null};
writeFileSync(dest,JSON.stringify(record,null,2)+"\n");console.log("RELICARIO_CATALOG_PROOF "+JSON.stringify({count:15,record,unlock_level:10,normal_consumed:true,indestructible_preserved:true,duration_hours:4,percent:50,real_catalog:true,production_state_changed:false}));
