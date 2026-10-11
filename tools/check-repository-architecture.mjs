import fs from 'node:fs';
import path from 'node:path';

const manifestPath = 'pipeline/repository-architecture-invariants.json';
const failures = [];

function fail(message){ failures.push(message); }
function countFiles(root){
  let count=0;
  const stack=[root];
  while(stack.length){
    const current=stack.pop();
    if(!fs.existsSync(current)) continue;
    for(const entry of fs.readdirSync(current,{withFileTypes:true})){
      const full=path.join(current,entry.name);
      if(entry.isDirectory()) stack.push(full);
      else if(entry.isFile()) count++;
    }
  }
  return count;
}

if(!fs.existsSync(manifestPath)){
  console.error('FAIL: architecture invariant manifest is missing.');
  process.exit(1);
}

const manifest=JSON.parse(fs.readFileSync(manifestPath,'utf8'));

for(const file of manifest.requiredFiles||[]){
  if(!fs.existsSync(file) || !fs.statSync(file).isFile()) fail(`required file missing: ${file}`);
}
for(const dir of manifest.requiredDirectories||[]){
  if(!fs.existsSync(dir) || !fs.statSync(dir).isDirectory()) fail(`required directory missing: ${dir}`);
}
for(const [dir,min] of Object.entries(manifest.minimumFileCounts||{})){
  const actual=countFiles(dir);
  if(actual<min) fail(`structural file-count floor breached: ${dir} has ${actual}, requires >= ${min}`);
}

const registryPath=manifest.registry?.path;
if(registryPath && fs.existsSync(registryPath)){
  try{
    const registry=JSON.parse(fs.readFileSync(registryPath,'utf8'));
    for(const key of manifest.registry.requiredArrays||[]){
      if(!Array.isArray(registry[key])) fail(`registry invariant failed: ${registryPath}.${key} must be an array`);
    }
    if(Array.isArray(registry.active)){
      const ids=new Set();
      for(const item of registry.active){
        if(!item || typeof item.id!=='string' || !item.id) fail('active workstream without stable id');
        else if(ids.has(item.id)) fail(`duplicate active workstream id: ${item.id}`);
        else ids.add(item.id);
        if(item && item.status!=='active' && item.status!=='blocked') fail(`active registry entry ${item?.id||'<unknown>'} has invalid status ${item?.status}`);
      }
    }
  }catch(error){
    fail(`registry JSON invalid: ${error.message}`);
  }
}

console.log('Repository architecture guard');
console.log(`Required files: ${(manifest.requiredFiles||[]).length}`);
console.log(`Required directories: ${(manifest.requiredDirectories||[]).length}`);

if(failures.length){
  for(const failure of failures) console.error('FAIL:',failure);
  console.error('Repository architecture integrity FAIL — stop production and restore canonical main before continuing.');
  process.exit(1);
}
console.log('Repository architecture integrity PASS.');
