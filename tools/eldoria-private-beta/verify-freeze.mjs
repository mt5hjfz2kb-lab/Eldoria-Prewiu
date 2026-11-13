// Immutable web artifact preflight. No code/games are mutated by this script.
import {readFile, readdir, lstat, mkdir, writeFile} from 'node:fs/promises';
import {createReadStream} from 'node:fs';
import {createHash} from 'node:crypto';
import {join, resolve, basename} from 'node:path';
import {fileURLToPath} from 'node:url';
const SHA40=/^[a-f0-9]{40}$/i, SHA64=/^[a-f0-9]{64}$/i;
const EXPECTED={
  source_sha:'948ffa9837168a0b68dab80be76b66350422b23a',
  web_version:'native325-fb1292b3d9ce',
  artifact_id:13583674142,
  artifact_run_id:42397405907,
  artifact_workflow_head_sha:'e4dbedaff72963203b72448cb29cdcbfcf82e9e1',
  inner_package_sha256:'fb1292b3d9ce4d6096d69fa18b417bc0a8cc6016f100defb3deaf29fbdb95771',
  artifact_zip_digest:'sha256:2e1e34480eabd2f5134f09f2d17e57c98ca8d72a603cfe040549f218b789c80f'
};
export function validateManifest(m){
  const errors=[];
  if(m?.schema!==1) errors.push('schema');
  for(const [k,v] of Object.entries(EXPECTED)) if(m?.[k]!==v) errors.push('identity:'+k);
  if(!SHA40.test(m?.source_sha||'')||!SHA40.test(m?.artifact_workflow_head_sha||''))errors.push('sha');
  if(!SHA64.test(m?.inner_package_sha256||''))errors.push('package_digest');
  if(m?.distribution_enabled!==false) errors.push('distribution must remain disabled');
  if(m?.archive_publication!=='draft_only') errors.push('release must remain draft');
  if(m?.hosted_gameplay_verified!==false||m?.physical_mobile_tested!==false)
    errors.push('untested product falsely claimed pass');
  if(m?.no_code_changes!==true)errors.push('gameplay must remain unmodified');
  return errors;
}
async function scan(root,depth=0){
  if(depth>9)throw Error('Artifact nesting limit');
  const result=[];
  for(const e of await readdir(root,{withFileTypes:true})){
    const path=join(root,e.name);
    const meta=await lstat(path);
    if(meta.isSymbolicLink())throw Error('Symlink inside artifact forbidden');
    if(meta.isDirectory())result.push(...await scan(path,depth+1));
    else if(meta.isFile())result.push({path,size:meta.size});
  }
  return result;
}
async function sha256(file){
  const h=createHash('sha256');
  for await(const chunk of createReadStream(file))h.update(chunk);
  return h.digest('hex');
}
export async function locatePackage(dir,manifest){
  const errors=validateManifest(manifest);if(errors.length)throw Error('Manifest invalid: '+errors.join(','));
  const files=await scan(resolve(dir));
  if(files.length>100)throw Error('Unexpected artifact file count');
  const candidates=files.filter(f=>f.path.toLowerCase().endsWith('.zip')&&f.size>100_000_000&&f.size<700_000_000);
  const matches=[];
  for(const f of candidates)if(await sha256(f.path)===manifest.inner_package_sha256)matches.push(f);
  if(matches.length!==1)throw Error('Expected exactly one byte-identical native325 ZIP; found '+matches.length);
  return matches[0];
}
async function main(){
  const [manifestPath,artifactRoot]=process.argv.slice(2);
  if(!manifestPath||!artifactRoot)throw Error('Usage: node verify-freeze.mjs MANIFEST FILES_DIR');
  const m=JSON.parse(await readFile(manifestPath,'utf8'));
  const found=await locatePackage(artifactRoot,m);
  const output={
    verified:true, version:m.web_version, source_sha:m.source_sha,
    asset_path:found.path, inner_zip_sha256:m.inner_package_sha256,
    archive_bytes:found.size, github_artifact_id:m.artifact_id,
    verified_at:new Date().toISOString(),
    published_to_testers:false, proof_type:'byte-digest-only'
  };
  if(process.env.GITHUB_OUTPUT){
    await writeFile(process.env.GITHUB_OUTPUT,'zip_path='+found.path+'\n',{flag:'a'});
  }
  if(process.env.BETA_FREEZE_RECEIPT){
    await mkdir(resolve(process.env.BETA_FREEZE_RECEIPT,'..'),{recursive:true});
    await writeFile(process.env.BETA_FREEZE_RECEIPT,JSON.stringify(output,null,2)+'\n');
  }
  console.log('BETA_NATIVE325_ZIP_EXACT_SHA256_PASS '+JSON.stringify(output));
}
if(process.argv[1]&&resolve(process.argv[1])===fileURLToPath(import.meta.url))
  main().catch(err=>{console.error('BETA_FREEZE_FAIL',err.message);process.exitCode=1;});