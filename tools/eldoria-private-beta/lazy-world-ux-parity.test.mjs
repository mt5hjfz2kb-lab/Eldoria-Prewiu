import test from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';

// This is a byte-level parity check. Lazy World must be an additive experiment
// over the 19-overlay pretester UX: never lose Mailbox, beta milestones, HUD,
// barracks navigation or player persistence while fixing the iPhone P0.
const frozen=readFileSync('tools/eldoria-private-beta/native/ux/ValoriaIntegratedWorld.cs.txt','utf8');
const host=readFileSync('Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/ValoriaIntegratedWorld.cs','utf8');
function exactlyOne(haystack,needle){
  assert.ok(needle.length>0,'Empty source anchor');
  assert.equal(haystack.split(needle).length-1,1,'Expected exactly one source anchor: '+needle.slice(0,68));
  return needle;
}
function between(body,start,end){
  exactlyOne(body,start);
  const a=body.indexOf(start),b=body.indexOf(end,a+start.length);
  assert.ok(b>a,'Missing end anchor after '+start);
  return body.slice(a,b);
}

test('lazy-world game host restores exactly to unchanged 19-overlay UX baseline',()=>{
  const oldReady='  public bool Ready=>presentationBindingsReady&&(!worldPresentation||worldPresentation.IsReady);\n';
  const front=between(host,
    '  // Diagnostic-only experiment: isolate city startup',
    '  public bool English=>');
  assert.match(front,/public static bool P0CityOnlyProbe/);
  assert.match(front,/public static bool P0LazyWorldProbe/);
  assert.match(front,/IEnumerator EnterWorldAfterDeferredBuild\(int epoch\)/);
  const city=between(host,'   if(open&&P0CityOnlyProbe){','   if(open&&!CanEnterWorld');
  const defer=between(host,'   if(!open){\n    deferredWorldObjective=null;','   if(InWorld==open)return true;');
  const navigation=between(host,'  // A quest may request a target while the first real Region 1 is still loading.',
    '  public bool Select(string id){');
  assert.match(navigation,/public bool NavigateToObjective\(string id\)/);
  assert.match(navigation,/if\(deferredWorldEntryQueued\)deferredWorldObjective=id;/);
  assert.doesNotMatch(navigation,/PlayerPrefs|Commit\(|Save\(|new LocalGateway/);
  const selectionCancel='   if(deferredWorldEntryQueued&&id!="gate"&&!IsWorldTarget(id)){\n'+
    '    deferredWorldEntryQueued=false;deferredWorldEpoch++;deferredWorldObjective=null;\n   }\n';
  exactlyOne(host,selectionCancel);
  const marker='   if(open&&P0LazyWorldProbe)Debug.Log("ELDORIA_P0_LAZY_WORLD_ENTERED");\n';
  exactlyOne(host,marker);
  let back=host.replace(front,oldReady).replace(city,'').replace(defer,'').replace(marker,'')
    .replace(navigation,'').replace(selectionCancel,'');
  assert.equal(back,frozen,'Unrelated UX, telemetry, progress, mailbox or save behavior changed');
});

test('lazy option stays isolated from city-only diagnostic and default production',()=>{
  assert.match(host,/#if UNITY_EDITOR\s+return P0LazyWorldEditorTestOverride;\s+#elif UNITY_WEBGL\s+return !P0CityOnlyProbe && UnityEngine\.Application\.absoluteURL\.Contains\("eldoria_p0_lazy_world=1"\);\s+#else\s+return false;/);
  assert.match(host,/if\(open&&P0CityOnlyProbe\)[\s\S]*?return false;/);
  assert.match(host,/if\(open&&P0LazyWorldProbe\)[\s\S]*?worldPresentation\.BeginDeferredLoad\(\)/);
  assert.doesNotMatch(host,/PlayerPrefs\.DeleteAll\(|File\.Delete\(/);
});

test('live UX capabilities survive region streaming experiment',()=>{
  for(const item of ['ScanBetaMilestones();','RoyalGoldV41MailboxScreen',
    'if(section==RoyalSection.Mailbox)','if(state.BarracksLevel>0)id="upgrade-barracks";',
    'betaMetricsEnabled','ELDORIA_P0_LAZY_WORLD_READY_FOR_ENTRY']){
    assert.ok(host.includes(item),'Required retained UX capability: '+item);
  }
});
