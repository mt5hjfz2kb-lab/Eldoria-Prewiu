import test from 'node:test';
import assert from 'node:assert/strict';
import {createRequire} from 'node:module';
const require=createRequire(import.meta.url);
const {verifyWorldStart}=require('./webkit-world-variant.cjs');
const eager='2026-11-11 ELDORIA_BOOT\nREGION1_CANON_WORLD_INSTALLED geography=1 population_authority=existing_LocalGateway persistence=existing_PlayerState';
const lazy='ELDORIA_P0_LAZY_WORLD_WAITING_FOR_ENTRY';
const city='ELDORIA_P0_CITY_ONLY_WORLD_INITIALIZATION_SKIPPED';
test('same-build eager Mac control needs actual installed Region 1',()=>{
  assert.equal(verifyWorldStart({url:'http://localhost/control/?eldoria_diag=1',events:eager}).eagerWorldInstalledVerified,true);
  assert.throws(()=>verifyWorldStart({url:'http://localhost/control/',events:lazy}),/Eager control/);
});
test('deferred full-world Mac startup accepts only explicit lazy flag and original marker',()=>{
  const v=verifyWorldStart({lazyWorld:true,url:'http://localhost/candidate/?eldoria_p0_lazy_world=1&utm_source=chatgpt.com',events:lazy});
  assert.deepEqual(v,{lazyWorldDeferredToFirstEntryVerified:true,realWorldEntryVerified:false});
  assert.throws(()=>verifyWorldStart({lazyWorld:true,url:'http://localhost/candidate/',events:lazy}),/opt-in/);
  assert.throws(()=>verifyWorldStart({lazyWorld:true,url:'http://localhost/candidate/?eldoria_p0_lazy_world=1',events:''}),/did not defer/);
});
test('deferred full-world must not secretly prebuild Region 1 or disable it',()=>{
  const url='http://localhost/candidate/?eldoria_p0_lazy_world=1';
  assert.throws(()=>verifyWorldStart({lazyWorld:true,url,events:lazy+'\n'+eager}),/before map entry/);
  assert.throws(()=>verifyWorldStart({lazyWorld:true,url,events:lazy+'\n'+city}),/city-only/);
  assert.throws(()=>verifyWorldStart({lazyWorld:true,url:url+'&eldoria_p0_city_only=1',events:lazy}),/without city-only/);
});
test('frozen city-only owner test cannot count as a working world',()=>{
  const url='http://localhost/beta-iphone-p0-city/?eldoria_p0_city_only=1&utm_source=chatgpt.com';
  assert.equal(verifyWorldStart({cityOnly:true,url,events:city}).cityOnlyDeferredWorldVerified,true);
  assert.throws(()=>verifyWorldStart({cityOnly:true,url,events:city+'\n'+eager}),/unexpectedly built/);
  assert.throws(()=>verifyWorldStart({cityOnly:true,lazyWorld:true,url,events:city}),/mutually exclusive/);
});
