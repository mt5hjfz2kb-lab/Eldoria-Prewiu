import {spawn} from 'node:child_process';
const run=(cmd,args=[],env={})=>new Promise((resolve,reject)=>{const p=spawn(cmd,args,{stdio:'inherit',env:{...process.env,...env}});p.on('exit',c=>c===0?resolve():reject(new Error(cmd+' '+args.join(' ')+' exited '+c)));p.on('error',reject)});
const wait=async(url,tries=30)=>{for(let i=0;i<tries;i++){try{const r=await fetch(url);if(r.ok)return}catch{}await new Promise(r=>setTimeout(r,300))}throw new Error('Local QA server did not become ready')};
let server;
try{
 console.log('\n[1/6] BUILD');await run('npm',['run','build']);
 console.log('\n[2/6] SYNTAX + CONTRACTS');await run('npm',['run','check']);
 console.log('\n[3/6] LOCAL SERVER');server=spawn('python3',['-m','http.server','4173'],{stdio:['ignore','ignore','inherit']});await wait('http://127.0.0.1:4173/playtest/?qa=1');
 console.log('\n[4/6] TARGETED PLAYER BLOCKERS');await run('npm',['run','qa:targeted']);
 console.log('\n[5/6] REGRESSION SUITE');await run('npm',['run','qa:regression']);
 console.log('\n[6/6] UNINTERRUPTED FRESH-SAVE ARC I');await run('npm',['run','qa:arc1']);
 console.log('\nLOCAL VALIDATION PASS — candidate is eligible for final GitHub certification.');
}finally{if(server)server.kill('SIGTERM')}
