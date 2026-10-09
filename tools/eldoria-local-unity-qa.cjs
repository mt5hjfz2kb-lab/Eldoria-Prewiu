'use strict';
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict'),crypto=require('node:crypto'),cp=require('node:child_process');
const ID='eldoria-local-agent-preflight-v1',OWNER='chat-work-local-recovery-20261009';
const sha=b=>crypto.createHash('sha256').update(b).digest('hex');
function authorize(reg,req,source){
 assert.equal(req.mode,'unity-local-save-notice-v1');assert.equal(req.enabled,true);assert.equal(req.owner,OWNER);assert.equal(req.workstream_id,ID);assert.equal(req.budget_eur,0);assert.equal(req.max_attempts,2);assert.equal(req.model,'qwen2.5-coder:3b');
 assert(Date.parse(req.expires_at)>Date.now(),'Order expired');assert.equal(req.stable_publish,false);
 const w=reg.active.find(x=>x.id===ID&&x.owner===OWNER&&x.status==='active');assert(w,'Owner absent');
 const {conflicts}=require('./eldoria-local-recovery-v1.cjs');
 assert(conflicts(reg,ID).every(x=>x.id==='r2-b-strategic-choice'),'Unapproved reservation');
 if(!(req.phase==='build'&&req.authorized_reuse_run_id))assert.equal(w.runner_loan.status,'authorized_temporary'); // Immutable built artifact QA uses Linux only; Windows has already been returned.
 const target='Unity/Assets/Eldoria/Scripts/Presentation/SaveLoadNoticePolicy.cs';assert(w.scope.includes(target));assert(w.scope.includes('Unity/Assets/Eldoria/Scripts/Presentation/SliceBoot.cs'));
 const boot=fs.readFileSync('Unity/Assets/Eldoria/Scripts/Presentation/SliceBoot.cs');
 return {schema_version:1,task_id:req.order_id,kind:'unity-save-notice-policy',department:'D06',issuer:'eldoria-coordinator-v1',owner:OWNER,workstream_id:ID,source_sha:source,model:req.model,model_digest:'f72c60cabf6237b07f6e632b2c48d533cef25eda2efbd34bed21c5e9c01e6225',max_attempts:2,budget_eur:0,target,boot_sha256:sha(boot),authorized_by:req.authorization,prompt:'You are the D06 Eldoria Unity C# programmer. Implement an actual pure save-load notice policy. The integration worker supplies this fixed C# class/method: namespace Eldoria.Presentation { public static class SaveLoadNoticePolicy { public static string Message(bool loadFailed) { YOUR_IMPLEMENTATION } } }. Return JSON with only an implementation string. Write exactly one C# return statement using a ternary expression on loadFailed. When false return string.Empty. When true return this Spanish player text: El guardado original se conserva. El progreso de esta sesión no se guardará. Use real C# syntax with a semicolon. Do not include namespace/class/method declarations, braces, markdown or extra code. No side effects, imports, loops or calls. The UI integration is provided separately; you write the real game policy.'};
}
function verifyCode(folder,source,run,attempt){
 const {verify}=require('./eldoria-local-recovery-v1.cjs'); // Performs shared independent provenance/cleanup checks.
 const shared=verify(folder,source,run,attempt);
 const read=n=>JSON.parse(fs.readFileSync(path.join(folder,n),'utf8').replace(/^\uFEFF/,''));
 const candidate=read('candidate.json');assert.deepEqual(Object.keys(candidate),['source']);assert.equal(typeof candidate.source,'string');assert(candidate.source.length<2400);assert(!/[ÃÂ\uFFFD]/.test(candidate.source),'UTF-8 player text was corrupted by the adapter');
 // Compile/execute only this fully constrained pure expression: no arbitrary generated C#.
 const grammar=/^\s*namespace\s+Eldoria\.Presentation\s*\{\s*public\s+static\s+class\s+SaveLoadNoticePolicy\s*\{\s*public\s+static\s+string\s+Message\s*\(\s*bool\s+loadFailed\s*\)\s*\{\s*return\s+!?loadFailed\s*\?\s*(?:"[^"\\\r\n]{0,350}"|string\.Empty)\s*:\s*(?:"[^"\\\r\n]{0,350}"|string\.Empty)\s*;\s*\}\s*\}\s*\}\s*$/;
 assert(grammar.test(candidate.source),'Unsupported C# syntax: use the requested pure ternary, exact namespace/class/method, ordinary string literals and string.Empty only');
 const dir=path.join(folder,'independent-csharp');fs.mkdirSync(dir,{recursive:true});
 fs.writeFileSync(path.join(dir,'Policy.cs'),candidate.source);
 fs.writeFileSync(path.join(dir,'Policy.csproj'),'<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><NuGetAudit>false</NuGetAudit><RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources></PropertyGroup></Project>');
 fs.writeFileSync(path.join(dir,'Program.cs'),`using System; using Eldoria.Presentation;
class Program {static int Main(){
 string ok=SaveLoadNoticePolicy.Message(false),bad=SaveLoadNoticePolicy.Message(true);
 if(ok!=string.Empty)throw new Exception("Healthy saves must not show a failure notice");
 if(string.IsNullOrWhiteSpace(bad)||bad.Length>350)throw new Exception("Failure message must be readable");
 string lower=bad.ToLowerInvariant();
 if(!lower.Contains("guardad")||!lower.Contains("conserv"))throw new Exception("Explain the original save is preserved");
 if(!(lower.Contains("no se guardar")||lower.Contains("no se guardará")||lower.Contains("sin guardar")))throw new Exception("Explain session progress is NOT persisted");
 for(int i=0;i<25;i++)if(SaveLoadNoticePolicy.Message(true)!=bad||SaveLoadNoticePolicy.Message(false)!=ok)throw new Exception("Policy must be deterministic");
 Console.WriteLine("INDEPENDENT_REAL_CSHARP_PASS healthy-empty, failure-readable, original-preserved, session-not-persisted, deterministic"); return 0;}}`);
 const output=cp.execFileSync('dotnet',['run','--project',path.join(dir,'Policy.csproj'),'--verbosity','quiet'],{encoding:'utf8',timeout:120000,env:{...process.env,GITHUB_TOKEN:'',GH_TOKEN:'',DOTNET_CLI_TELEMETRY_OPTOUT:'1'}});
 assert(output.includes('INDEPENDENT_REAL_CSHARP_PASS'));fs.writeFileSync(path.join(folder,'csharp-test-output.txt'),output);
 return {...shared,pass:true,independent_checks:5,code_sha256:sha(Buffer.from(candidate.source)),generated_code_execution:'Restricted pure C# on independent Linux; no worker execution',scope:'Unity save-load feedback policy; game acceptance still requires real candidate build and independent playable/visual QA'};
}
module.exports={authorize,verifyCode,sha};
if(require.main===module){
 const [mode,folder,source,run,attempt,out]=process.argv.slice(2);assert.equal(mode,'review');
 let report;try{report=verifyCode(folder,source,run,Number(attempt));}catch(e){report={pass:false,errors:[e.message],source_sha:source,run_id:run,attempt:Number(attempt)};try{report.rejected_candidate=JSON.parse(fs.readFileSync(path.join(folder,'candidate.json'),'utf8'));}catch{}}
 fs.writeFileSync(out,JSON.stringify(report,null,2));console.log(JSON.stringify(report));
}
