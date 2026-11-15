const {readFileSync}=require('node:fs'),vm=require('node:vm'),assert=require('node:assert/strict');
const script=readFileSync(__dirname+'/world-phase-diagnostic.js','utf8');
function boot(data=new Map(),enabled=true,blocked=false){
 const events={},devents={},logs=[],frames=[];
 const node=()=>({style:{},appendChild(){},addEventListener(){}});
 const win={EldoriaLocalDiagnostic:enabled?{enabled:true,read:()=>[]}:undefined,
  localStorage:{getItem:k=>data.get(k),setItem:(k,v)=>{if(blocked)throw Error();data.set(k,v);}},
  console:{log(...args){logs.push({self:this,args});return 17;},warn(){},error(){}},
  addEventListener:(k,f)=>events[k]=f,requestAnimationFrame:f=>frames.push(f)};
 const doc={readyState:'complete',body:node(),createElement:node,addEventListener:(k,f)=>devents[k]=f};
 vm.runInNewContext(script,{window:win,document:doc,performance:{now:()=>50},Date,JSON,Set,Object,Number,Math,Error});
 return {win,data,events,devents,logs,frames,report:()=>win.EldoriaLocalDiagnostic.readWorldReport()};
}
const a=boot();
assert.equal(a.win.console.log('ELDORIA_P0_LAZY_WORLD_REQUESTED private-token'),17);
assert.equal(a.logs[0].self,a.win.console);
a.win.console.warn('REGION1_BLENDER_IMPORTED private-player');
a.win.console.log({toString(){throw Error('must not stringify')}});
assert.equal(a.report().current.length,2);
assert(!JSON.stringify(a.report()).includes('private'));
a.win.console.log('ELDORIA_P0_LAZY_WORLD_ENTERED');
a.frames.shift()();a.frames.shift()();
assert.equal(a.report().current.at(-1).phase,'world-frame-2');
const b=boot(a.data);assert.equal(b.report().previous[0].phase,'world-requested');
b.events.pageshow({});
const c=boot(a.data);assert.equal(c.report().previous[0].phase,'world-requested');
for(let i=0;i<100;i++)c.win.console.log('ELDORIA_P0_LAZY_WORLD_ENTERED');
assert.equal(c.report().current.length,32);
const off=boot(new Map(),false);assert.equal(off.data.size,0);
const bad=boot(new Map(),true,true);bad.win.console.log('ELDORIA_P0_LAZY_WORLD_REQUESTED');assert.equal(bad.report().persistent,false);
const poison=new Map([['eldoria.world-phase.diagnostic.v4',JSON.stringify({current:[{phase:'private',at:1,ms:2}],previous:[{phase:'world-requested',at:1,ms:2,secret:'private'}]})]]);
assert(!JSON.stringify(boot(poison).report()).includes('private'));
console.log('PASS world journal: actual marker mapping, original console semantics, restart preservation, landing reload preservation, two frames, bounds, privacy, opt-out, blocked storage');
