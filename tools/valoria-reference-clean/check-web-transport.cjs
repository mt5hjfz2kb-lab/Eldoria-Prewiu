const fs=require("fs"),vm=require("vm"),assert=require("assert");
const source=fs.readFileSync(process.argv[2]||"dist/play.js","utf8");
const a=Uint8Array.from([31,139,8,1]),b=Uint8Array.from([2,3,4,5,6]);
let count=0;const nativeFetch=async url=>{count++;if(url==="one")return new Response(a);if(url==="two")return new Response(b);return new Response("passthrough");};
const context={window:{fetch:nativeFetch},location:{href:"https://example.test/"},URL,Response,ReadableStream,Map,Error,console,document:{getElementById:()=>({addEventListener(){}})}};
vm.createContext(context);vm.runInContext(source,context);
(async()=>{
 vm.runInContext('installChunkTransport({"Build/game.data.unityweb":{parts:["one","two"],bytes:9}})',context);
 const r=await context.window.fetch("Build/game.data.unityweb");assert.equal(r.url,"https://example.test/Build/game.data.unityweb");assert.equal(r.headers.get("Content-Length"),"9");assert.deepEqual(new Uint8Array(await r.arrayBuffer()),Uint8Array.from([...a,...b]));
 const other=await context.window.fetch("other");assert.equal(await other.text(),"passthrough");assert.equal(count,3);
 console.log("PASS: ordered original bytes, original URL, length, unrelated fetch passthrough.");
})().catch(e=>{console.error(e);process.exitCode=1;});
