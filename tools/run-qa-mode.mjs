import {spawn} from 'node:child_process';
const mode=process.argv[2]||'focus';
const run=(cmd,args=[],env={})=>new Promise((resolve,reject)=>{const p=spawn(cmd,args,{stdio:'inherit',env:{...process.env,...env}});p.on('exit',c=>c===0?resolve():reject(new Error(cmd+' '+args.join(' ')+' exited '+c)));p.on('error',reject)});
const wait=async(url)=>{for(let i=0;i<30;i++){try{const r=await fetch(url);if(r.ok)return}catch{}await new Promise(r=>setTimeout(r,250))}throw Error('QA server not ready')};
let server;try{await run('npm',['run','build']);await run('npm',['run','check']);server=spawn('python3',['-m','http.server','4173'],{stdio:['ignore','ignore','inherit']});await wait('http://127.0.0.1:4173/playtest/?qa=1');await run('node',['qa/e2e-qa-launcher.js'],{ELDORIA_QA_MODE:mode});console.log('QA '+mode.toUpperCase()+' PASS');}finally{if(server)server.kill('SIGTERM')}
