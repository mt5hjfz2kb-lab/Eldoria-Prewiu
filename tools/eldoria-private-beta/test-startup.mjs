import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
import vm from 'node:vm';
const play=readFileSync(process.argv[2]+'/play.js','utf8');
const probe=play.slice(0,play.indexOf('// Fetch already exposes'));
const store=new Map([['game-save','untouched']]);
function boot(query){
 const node=()=>({style:{},appendChild(){},addEventListener(){}});
 const window={location:{search:query,hash:''},innerWidth:390,innerHeight:844,addEventListener(){},localStorage:{getItem:k=>store.get(k)||null,setItem:(k,v)=>store.set(k,v),removeItem:k=>store.delete(k)}};
 const document={body:node(),readyState:'complete',createElement:node,addEventListener(){}};
 vm.runInNewContext(probe,{window,document,performance:{now:()=>1,getEntriesByType:()=>[{type:'back_forward'}]},console});
 return window.EldoriaLocalDiagnostic;
}
assert.equal(boot(''),undefined);assert.equal(store.size,1);
let api=boot('?eldoria_diag=1');
api.recordStartup('start');api.recordStartup('progress',70);api.recordStartup('pre-run');api.recordStartup('heap-mib',256);
api=boot('');
assert(api.read().some(e=>e.kind==='startup-progress'&&e.detail==='70'));
assert(api.read().some(e=>e.kind==='startup-heap-mib'&&e.detail==='256'));
api.recordStartup('https://private.example/token',123);api.recordStartup('failure','private token');
assert(!JSON.stringify(api.read()).includes('private'));
for(let n=0;n<100;n++)api.recordStartup('progress',n);
assert.equal(api.read().length,48);assert.equal(store.get('game-save'),'untouched');
assert.equal(boot('?eldoria_diag=0'),undefined);
for(const hook of ['startupCheckpoint("start")','startupCheckpoint("pre-run")','startupCheckpoint("post-run")','startupCheckpoint("ready")'])assert(play.includes(hook));
assert(!play.includes('location.reload();\n startupCheckpoint'));
console.log('PASS: opt-in only, numeric checkpoints survive restart, bounded journal, no game-save writes or raw data');
