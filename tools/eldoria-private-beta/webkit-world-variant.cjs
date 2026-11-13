'use strict';
// Pure browser-mode validator for the exact same WebGL bundle A/B.
// A successful deferred-city observation is NOT proof of world entry.
function verifyWorldStart({cityOnly=false,lazyWorld=false,url,events}) {
  if(cityOnly&&lazyWorld)throw Error('P0 city-only and full lazy-world QA are mutually exclusive');
  if(typeof events!=='string')throw Error('Missing genuine Unity event log');
  const query=new URL(url||'http://localhost/','http://localhost/').searchParams;
  const installed=events.includes('REGION1_CANON_WORLD_INSTALLED');
  if(cityOnly){
    if(query.get('eldoria_p0_city_only')!=='1')throw Error('City-only QA requires actual city-only URL');
    if(!events.includes('ELDORIA_P0_CITY_ONLY_WORLD_INITIALIZATION_SKIPPED'))throw Error('City-only deferral absent');
    if(installed)throw Error('City-only unexpectedly built real Region 1');
    return {cityOnlyDeferredWorldVerified:true};
  }
  if(lazyWorld){
    if(query.get('eldoria_p0_lazy_world')!=='1'||query.get('eldoria_p0_city_only')==='1')
      throw Error('Lazy-world QA requires full real-world opt-in without city-only');
    if(!events.includes('ELDORIA_P0_LAZY_WORLD_WAITING_FOR_ENTRY'))
      throw Error('Lazy-world startup did not defer real Region 1');
    if(events.includes('ELDORIA_P0_CITY_ONLY_WORLD_INITIALIZATION_SKIPPED'))
      throw Error('Lazy-world QA incorrectly installed a city-only build');
    if(installed)throw Error('Lazy-world QA allocated Region 1 before map entry');
    return {lazyWorldDeferredToFirstEntryVerified:true,realWorldEntryVerified:false};
  }
  if(!installed)throw Error('Eager control did not construct real Region 1');
  return {eagerWorldInstalledVerified:true};
}
module.exports={verifyWorldStart};
