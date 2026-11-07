import {readFile, readdir, lstat} from 'node:fs/promises';
import {createHash, timingSafeEqual} from 'node:crypto';
import {join, relative, resolve} from 'node:path';
import {fileURLToPath} from 'node:url';
const ROOT=resolve(fileURLToPath(new URL('.',import.meta.url)));
const SAFE_RELEASE='native325-fb1292b3d9ce';
const PINNED_SOURCE='948ffa9837168a0b68dab80be76b66350422b23a';
const PINNED_ZIP='fb1292b3d9ce4d6096d69fa18b417bc0a8cc6016f100defb3deaf29fbdb95771';
const MIME={
 '.png':'image/png','.html':'text/html; charset=utf-8',
 '.css':'text/css','.js':'application/javascript','.json':'application/json',
 '.glb':'model/gltf-binary'
};
function err(s){throw new Error(s)}
export function validateUploadManifest(m) {
  if(m?.schema!==1||m.release!==SAFE_RELEASE||m.gameplay_source_sha!==PINNED_SOURCE||
     m.original_web_zip_sha256!==PINNED_ZIP||
     m.r2_prefix!=='releases/'+SAFE_RELEASE+'/'||
     m.access!=='private-r2-only'||m.deployment_status!=='NOT_DEPLOYED')
    return false;
  if(!Array.isArray(m.files)||m.files.length!==19||m.total_bytes!==490317080)return false;
  let total=0;const paths=new Set();
  for(const f of m.files){
    if(!f || !/^[A-Za-z0-9_+./-]+$/.test(f.path)||
      f.path.split('/').some(x=>!x||x==='.'||x==='..')||
      paths.has(f.path)||!Number.isSafeInteger(f.bytes)||f.bytes<1||
      !/^[0-9a-f]{64}$/.test(f.sha256))return false;
    paths.add(f.path);total+=f.bytes;
    const gzip=f.path.endsWith('.unityweb')&&f.path.includes('.framework.js.')||
               f.path.endsWith('.wasm.unityweb');
    const br=f.path.endsWith('.bin.br');
    if((br&&f.content_encoding!=='br')||(gzip&&f.content_encoding!=='gzip')||
       (!br&&!gzip&&f.content_encoding!==null))return false;
    if(br&&f.content_type!=='application/octet-stream')return false;
    if(f.path.endsWith('.wasm.unityweb')&&f.content_type!=='application/wasm')return false;
    if(f.path.endsWith('.framework.js.unityweb')&&f.content_type!=='application/javascript')return false;
  }
  return total===m.total_bytes &&
    paths.has('index.html')&&paths.has('play.js')&&paths.has('build-manifest.json');
}
const sha256=bytes=>createHash('sha256').update(bytes).digest('hex');
export async function validatePackageTree(root,m){
  if(!validateUploadManifest(m))err('Manifest was changed or incorrect');
  let size=0;const listed=new Set(m.files.map(f=>f.path));
  async function filesUnder(folder){
    const got=[];
    for(const entry of await readdir(folder,{withFileTypes:true})){
      const abs=join(folder,entry.name),info=await lstat(abs);
      if(info.isSymbolicLink())err('Symlinks cannot be uploaded');
      if(info.isDirectory())got.push(...await filesUnder(abs));
      else if(info.isFile())got.push(abs);
    }
    return got;
  }
  const all=await filesUnder(root);
  if(all.length!==m.files.length)err('Unexpected file count');
  for(const abs of all)if(!listed.has(relative(root,abs).replaceAll('\\','/')))err('Unexpected file '+abs);
  for(const file of m.files){
    const abs=join(root,file.path),b=await readFile(abs);
    if(b.length!==file.bytes||sha256(b)!==file.sha256)err('Bytes differ: '+file.path);
    size+=b.length;
  }
  if(size!==m.total_bytes)err('Bytes total differ');
  return {verified:true,count:all.length,total_bytes:size,release:m.release};
}
export function uploadCommands(m,root,bucket='eldoria-beta-privada'){
  if(!validateUploadManifest(m))err('Manifest invalid');
  return m.files.map(f=>{
    const args=['r2','object','put',bucket+'/'+m.r2_prefix+f.path,
     '--file',join(root,f.path),'--content-type',f.content_type,'--remote'];
    if(f.content_encoding)args.push('--content-encoding',f.content_encoding);
    args.push('--force');
    return {program:'wrangler',args};
  });
}
if(process.argv[1]&&resolve(process.argv[1])===fileURLToPath(import.meta.url)){
  const [root,manifest]=process.argv.slice(2);
  if(!root||!manifest)err('usage: node verify-r2-files.mjs WEBGL_DIR R2_MANIFEST_JSON');
  const m=JSON.parse(await readFile(manifest,'utf8'));
  console.log(JSON.stringify(await validatePackageTree(root,m)));
  console.log('R2_OBJECT_METADATA_VERIFIED; no upload executed');
}
