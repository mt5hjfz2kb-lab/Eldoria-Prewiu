// Source/geometry regression for the V3 construction countdown.
// This is deliberately not a Unity-render or a native PlayMode certificate.
import assert from 'node:assert/strict';
import fs from 'node:fs';

const source = fs.readFileSync(
  'Unity/Assets/Eldoria/UI/ValoriaRoyalHUDV2/Runtime/RoyalGoldV3ContextHud.cs',
  'utf8');
const section = (start, end) => {
  const from = source.indexOf(start);
  const to = source.indexOf(end, from + start.length);
  assert.ok(from >= 0 && to > from, 'method markers must be present: ' + start);
  return source.slice(from, to);
};

const construction = section('private static bool IsConstructionTimer(', 'private bool TryPlaceConstructionTimer(');
for (const id of ['sawmill', 'barracks', 'upgrade-barracks', 'granary', 'stoneworks', 'bastion'])
  assert.ok(construction.includes('id == "' + id + '"'), 'construction building identified: ' + id);

const position = section('private bool TryPlaceConstructionTimer(', 'private void RenderTimer(');
assert.match(position, /FindAnchor\(id == "upgrade-barracks" \? "barracks" : id\)/);
assert.match(position, /CanProject\(target, camera, safe, true, out projectedRoof\)/);
assert.match(position, /CanProject\(target, camera, safe, false, out var bottomEdge\)/);
assert.match(position, /Mathf\.Abs\(x - projectedAnchor\.x\) > Mathf\.Min\(32f, width \* \.18f\)/);
assert.match(position, /if \(!roofVisible && !footVisible\) return false/);
assert.match(position, /return false;/);
assert.doesNotMatch(position, /anchor(?:ed)?Position\s*=\s*new Vector2\(safe\.rect\.xMax/);

const render = section('private void RenderTimer(', '\n    }\n}');
assert.match(render, /if \(construction\)[\s\S]*?TryPlaceConstructionTimer/);
assert.match(render, /if \(!IsConstructionTimer\(candidate\)/);
assert.match(render, /timer\.Root\.anchoredPosition = center;/);
assert.match(render, /Visible\(timer\.Visibility, false, false\)/);

// Model the same edge policy with representative safe-local coordinates.
// This checks the behavioural intent, not Unity screen projection or Canvas APIs.
const place = ({roof, bottom, limits, width = 220, height = 46}) => {
  if (roof === null && bottom === null) return null; // off camera
  const anchor = roof ?? bottom;
  const minX = limits.left + width / 2 + 8;
  const maxX = limits.right - width / 2 - 8;
  if (maxX < minX) return null;
  const x = Math.min(maxX, Math.max(minX, anchor[0]));
  if (Math.abs(x - anchor[0]) > Math.min(32, width * .18)) return null;
  const roofY = roof === null ? null : roof[1] + height / 2 + 12;
  const fits = y => y - height / 2 >= limits.bottom &&
    y + height / 2 <= limits.top;
  if (roof !== null && fits(roofY)) return [x, roofY];
  if (bottom !== null) {
    const belowY = bottom[1] - height / 2 - 12;
    if (fits(belowY)) return [x, belowY];
  }
  return null;
};
const limits = {left:-195, right:195, bottom:-280, top:320};
assert.deepEqual(place({roof:[0,0],bottom:[0,-50],limits}),[0,35],
  'centre building: timer directly above its roof');
assert.deepEqual(place({roof:[35,20],bottom:[35,-40],limits}),[35,55],
  'camera pan: the timer follows its building, not a fixed HUD dock');
assert.deepEqual(place({roof:[0,305],bottom:[0,100],limits}),[0,65],
  'top obstacle: timer moves under building rather than onto screen edge');
assert.equal(place({roof:[240,0],bottom:[240,-50],limits}),null,
  'building beyond edge: timer hides instead of sticking to the side');
assert.equal(place({roof:null,bottom:null,limits}),null,
  'off-camera building: no free-floating countdown');
assert.deepEqual(place({roof:null,bottom:[10,120],limits}),[10,85],
  'roof outside screen but visible building foot: anchor timer beneath its silhouette');
assert.equal(place({roof:[0,400],bottom:[0,390],limits}),null,
  'no position near building: hide rather than detach');

console.log('CONSTRUCTION_ANCHOR_SOURCE_AND_GEOMETRY_MODEL_PASS (not Unity certified)');
