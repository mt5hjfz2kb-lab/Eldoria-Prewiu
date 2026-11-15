const test=require('node:test'),assert=require('node:assert/strict'),vm=require('node:vm'),fs=require('node:fs');
const code=fs.readFileSync(__dirname+'/world-phase-memory-diagnostic.js','utf8');
function fixture(saved){
 const tasks=[],frames=[],events={};let persisted,queries=0,logs=0;
 const api={enabled:true,read:()=>[]};
 const window={EldoriaLocalDiagnostic:api,
  localStorage:{getItem:()=>JSON.stringify(saved||null),setItem:(_k,v)=>persisted=JSON.parse(v)},
  console:{log:()=>logs++,warn(){},error(){}},
  requestAnimationFrame:f=>frames.push(f),setTimeout:(f,ms)=>tasks.push({f,ms}),
  addEventListener:(name,f)=>events[name]=f,
  valoriaGame:{GetMetricsInfo(){queries++;return {usedWASMHeapSize:400000000,totalWASMHeapSize:500000000,playerName:'must not export'};}}};
 vm.runInNewContext(code,{window,document:{readyState:'loading',addEventListener(){}},performance:{now:()=>1},Set,Date,Number,Error,JSON});
 return {window,api,tasks,frames,events,queries:()=>queries,logs:()=>logs,persisted:()=>persisted};
}
test('bounded samples happen after console returns and export only numeric heap fields',()=>{
 const f=fixture();f.window.console.log('ELDORIA_P0_LAZY_WORLD_ENTERED');
 assert.equal(f.queries(),0);assert.equal(f.logs(),1);assert.equal(f.tasks.length,5);
 for(const t of f.tasks)t.f();
 assert.equal(f.queries(),5);const r=f.api.readWorldReport();assert.equal(r.diagnostic,'beta21-world-v5');
 const rows=r.current.filter(x=>x.phase.startsWith('world-live-'));assert.equal(rows.length,5);
 assert(rows.every(x=>x.usedWASMHeapBytes===400000000&&x.wasmCapacityBytes===500000000));
 assert(!JSON.stringify(r).includes('must not export'));
});
test('new entry invalidates previous timers; metric failures cannot interrupt gameplay',()=>{
 const f=fixture();f.window.console.log('ELDORIA_P0_LAZY_WORLD_ENTERED');
 f.window.console.log('ELDORIA_P0_LAZY_WORLD_ENTERED');
 for(const t of f.tasks.slice(0,5))t.f();assert.equal(f.queries(),0);
 f.window.valoriaGame.GetMetricsInfo=()=>{throw Error('runtime unavailable');};
 for(const t of f.tasks.slice(5))assert.doesNotThrow(t.f);
 assert.equal(f.api.readWorldReport().current.filter(x=>x.phase.startsWith('world-live-')).length,5);
});
test('reload retains last attempt and numeric heap values without untrusted extra fields',()=>{
 const f=fixture({current:[{phase:'world-requested',at:1,ms:1},{phase:'world-live-2s',at:2,ms:2,usedWASMHeapBytes:350000000,wasmCapacityBytes:-1,rawSave:'private'}]});
 const rows=f.api.readWorldReport().previous;assert.equal(rows.length,2);assert.equal(rows[1].usedWASMHeapBytes,350000000);
 assert(!('wasmCapacityBytes' in rows[1]));assert(!('rawSave' in rows[1]));
});

test('city return cancels pending world samples and mesh marker keeps numeric measurements',()=>{
 const f=fixture();f.window.console.log('ELDORIA_P0_STATIC_MESH_CPU_RETIRED count=4 before=87070848 after=11000000 geometry_preserved=1');
 let row=f.api.readWorldReport().current[0];assert.equal(row.staticMeshBytesBefore,87070848);assert.equal(row.staticMeshBytesAfter,11000000);
 f.window.console.log('ELDORIA_P0_LAZY_WORLD_ENTERED');f.window.console.log('ELDORIA_P0_ENTRY_RENDER_BUDGET_RESTORED');
 for(const t of f.tasks)t.f();assert.equal(f.queries(),0);
});
