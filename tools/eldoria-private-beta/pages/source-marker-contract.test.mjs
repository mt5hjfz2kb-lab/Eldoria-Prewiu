import test from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
import {parseUnitySignal} from './signal-parser.mjs';

// Source-only safety net: this is NOT evidence that Unity compiled or emitted
// any of the markers in the currently public mobile11 WebGL build.
const source=readFileSync(new URL('../native/ux/ValoriaIntegratedWorld.cs.txt',import.meta.url),'utf8');

test('Only the opt-in WebGL URL can activate the minimal I-V signal scan',()=>{
 assert.match(source,/betaMetricsEnabled=UnityEngine\.Application\.platform==RuntimePlatform\.WebGLPlayer/);
 assert.match(source,/betaUrl\.Contains\("\?eldoriaBetaTelemetry=1"\)/);
 assert.match(source,/betaUrl\.Contains\("&eldoriaBetaTelemetry=1"\)/);
 assert.match(source,/if\(betaMetricsEnabled&&Time\.unscaledTime>=nextBetaSignal\)/);
 assert.match(source,/void ScanBetaMilestones\(\)/);
 assert.match(source,/city\.Gateway\.Snapshot\(\)/);
 assert.match(source,/Mathf\.Clamp\(state\.BastionLevel,1,5\)/);
 assert.doesNotMatch(source,/ELDORIA_BETA_SIGNAL[^\n]*PlayerState|ELDORIA_BETA_SIGNAL[^\n]*[Pp]assword/);
});
test('All signal names emitted by the real I-V model are parsed with a whitelist',()=>{
 const markers=[
  ['bastion_reached','bastion=1'],['bastion_reached','bastion=2'],
  ['bastion_reached','bastion=3'],['bastion_reached','bastion=4'],
  ['bastion_reached','bastion=5'],['end_of_slice','bastion=5'],
  ['world_opened','bastion=3'],['build_completed','building_code=sawmill|level=2'],
  ['march_sent','march_slots_used=1'],['battle_resolved','outcome_code=win'],
  ['lyra_state','state_code=recruited'],['fissure_state','state_code=closed']
 ];
 for(const [name,payload] of markers){
  assert.ok(source.includes('EmitBetaSignal("'+name+'",'),'Missing authored Unity marker: '+name);
  assert.deepEqual(parseUnitySignal('ELDORIA_BETA_SIGNAL|'+name+'|'+payload),{
   name,payload:Object.fromEntries(payload.split('|').map(t=>{
    const [k,v]=t.split('=');return [k,/^(bastion|level|march_slots_used)$/.test(k)?Number(v):v];
   }))
  });
 }
});
test('End of slice requires canonical completed objective, not just reaching Bastion V',()=>{
 assert.match(source,/!betaEndSeen&&SliceRules\.CurrentObjectiveKey\(state\)=="b5\.complete"/);
 assert.equal(parseUnitySignal('ELDORIA_BETA_SIGNAL|end_of_slice|bastion=4'),null);
 assert.equal(parseUnitySignal('ELDORIA_BETA_SIGNAL|bastion_reached|bastion=6'),null);
 assert.equal(parseUnitySignal('PlayerState|save='+'X'.repeat(10)),null);
 assert.equal(parseUnitySignal('ELDORIA_BETA_SIGNAL|world_opened|bastion=3|player_name=PERSON'),null);
});
