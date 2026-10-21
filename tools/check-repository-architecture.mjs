import fs from 'node:fs';
import path from 'node:path';
import { execFileSync } from 'node:child_process';

const manifestPath = 'pipeline/repository-architecture-invariants.json';
const failures = [];
// CI can inspect the exact committed tree without downloading binary asset bodies.
// Default local checks still inspect the working tree, including uncommitted deletions.
const trackedMode = process.argv.includes('--tracked-tree');
const tracked = trackedMode ? new Set(execFileSync('git',
  ['ls-tree','-r','--name-only','-z','HEAD'], {encoding:'utf8',maxBuffer:64*1024*1024})
  .split('\0').filter(Boolean)) : null;
const hasFile = p => tracked ? tracked.has(p) : fs.existsSync(p) && fs.statSync(p).isFile();
const hasDir = p => tracked ? [...tracked].some(f=>f.startsWith(p.replace(/\/$/,'')+'/')) :
  fs.existsSync(p) && fs.statSync(p).isDirectory();

function fail(message){ failures.push(message); }
function countFiles(root){
  if(tracked) return [...tracked].filter(f=>f.startsWith(root.replace(/\/$/,'')+'/')).length;
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
  if(!hasFile(file)) fail(`required file missing: ${file}`);
}
for(const dir of manifest.requiredDirectories||[]){
  if(!hasDir(dir)) fail(`required directory missing: ${dir}`);
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

console.log('Repository architecture guard: '+(trackedMode?'committed HEAD tree':'local working tree'));
console.log(`Required files: ${(manifest.requiredFiles||[]).length}`);
console.log(`Required directories: ${(manifest.requiredDirectories||[]).length}`);

if(failures.length){
  for(const failure of failures) console.error('FAIL:',failure);
  console.error('Repository architecture integrity FAIL — stop production and restore canonical main before continuing.');
  process.exit(1);
}
console.log('Repository architecture integrity PASS.');
