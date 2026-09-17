import fs from 'node:fs';
import path from 'node:path';
import zlib from 'node:zlib';
import crypto from 'node:crypto';

const root=process.cwd();
const parts=['d0.txt','r1.txt','r2.txt','r3.txt','r4.txt','r5a.txt','r5b.txt','r6a.txt','r6b.txt'];
const encoded=parts.map(f=>fs.readFileSync(path.join(root,'v0195',f),'utf8').trim()).join('');
let html=zlib.gunzipSync(Buffer.from(encoded,'base64')).toString('utf8');

for(const file of ['r2.json','r3.json','r4.json','r5.json','r6.json']){
  const patches=JSON.parse(fs.readFileSync(path.join(root,'v0195-clean',file),'utf8'));
  for(const [from,to] of patches){
    if(!html.includes(from)) throw new Error(`Missing clean integration marker in ${file}: ${from.slice(0,80)}`);
    html=html.replace(from,to);
  }
}

const cleanHash=crypto.createHash('sha256').update(html).digest('hex');
const expectedClean='03002b840f9a6e11212cb1825f014304f36c3531c3b59f656b39a7be795f03ba';
if(cleanHash!==expectedClean) throw new Error(`Validated r6 baseline mismatch: ${cleanHash}`);

const patch=fs.readFileSync(path.join(root,'v0195-r7','r7patch.js'),'utf8');
html=new Function('base',`${patch}\nreturn base;`)(html);

const required=[
  "const BUILD = '0.19.5-r7'",
  "version:BUILD",
  'onboardingDone',
  'showGlobalWorldIntro',
  'Duelo de Reliquias',
  'CAPÍTULO III',
  'MAR DE CRISTAL',
  'data-testid="upgrade-building"',
  'data-testid="begin-auto-combat"',
  'data-testid="claim-first-card"'
];
for(const marker of required) if(!html.includes(marker)) throw new Error(`r7 consolidated output missing marker: ${marker}`);

// The published artifact must not contain the historical runtime loader chain.
for(const forbidden of ['../v0195-r6/index.html','r7patch.js</script>']){
  if(html.includes(forbidden)) throw new Error(`Runtime chain leaked into consolidated output: ${forbidden}`);
}

const out=path.join(root,'playtest');
fs.rmSync(out,{recursive:true,force:true});
fs.mkdirSync(out,{recursive:true});
fs.writeFileSync(path.join(out,'index.html'),html);
const finalHash=crypto.createHash('sha256').update(html).digest('hex');
fs.writeFileSync(path.join(out,'build.json'),JSON.stringify({version:'0.19.5',revision:'r7-consolidated',sha256:finalHash,builtAt:new Date().toISOString()},null,2));
console.log(`Eldoria r7 consolidated: ${html.length} bytes sha256=${finalHash}`);
