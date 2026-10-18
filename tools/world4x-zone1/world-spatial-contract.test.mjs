import assert from 'node:assert/strict';
import {WORLD_SIZE,CHUNK_SIZE,ZOOM_LEVELS,assertWorldPoint,chunkOf,worldCatalog,validateWorldLayout} from './world-spatial-contract.mjs';

const points = [
  {id:'valoria-city', x:31,z:83},
  {id:'forest-valoria',x:33,z:87},
  {id:'quarry-valoria',x:28,z:82},
  {id:'corrupt-scout',x:35,z:90},
  {id:'breach-center',x:64,z:64},
];
const catalog=worldCatalog(points);
assert.equal(WORLD_SIZE,128);assert.equal(CHUNK_SIZE,16);
assert.equal(Object.keys(ZOOM_LEVELS).length,3);
for(const point of points){
  for(const zoom of ['global','regional','local']){
    assert.strictEqual(catalog.atZoom(zoom,point.id),catalog.get(point.id));
    assert.deepEqual([catalog.atZoom(zoom,point.id).x,catalog.atZoom(zoom,point.id).z],[point.x,point.z]);
  }
}
assert.deepEqual(chunkOf({id:'valoria-city',x:31,z:83}),{x:1,z:5});
assert.equal(validateWorldLayout(points).entities,5);
assert.equal(catalog.atZoom('local','missing'),null);
assert.throws(()=>catalog.atZoom('ultra','valoria-city'));
assert.throws(()=>assertWorldPoint({id:'test',x:-1,z:5}));
assert.throws(()=>assertWorldPoint({id:'test',x:128,z:5}));
assert.throws(()=>worldCatalog([points[0],{id:'valoria-city',x:1,z:1}]));
assert.throws(()=>worldCatalog([points[0],{id:'another',x:31,z:83}]));
assert.equal(typeof catalog.save,'undefined');
assert.equal(typeof catalog.execute,'undefined');
console.log('WORLD4X_SPATIAL_CONTRACT_PASS: 3 zooms, stable IDs/cells, chunk boundaries, invalid/duplicate rejection; read-only');
