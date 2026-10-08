'use strict';
// Trusted deterministic regression parser, never evaluates generated text as code.
// Usage: node tools/tests/eldoria-json-object-validator.cjs <path-to-case-utf8>
// Exit 0 valid JSON object, exit 1 invalid, exit 2 environment/invocation error.
const fs=require('node:fs');
if(process.argv.length!==3) process.exit(2);
try {
  let s=fs.readFileSync(process.argv[2],'utf8');
  if(s.charCodeAt(0)===0xfeff) s=s.slice(1);
  if(s.length>120) process.exit(1);
  const value=JSON.parse(s);
  process.exit(value!==null && typeof value==='object' && !Array.isArray(value)?0:1);
}catch(e){
  if(e instanceof SyntaxError) process.exit(1);
  console.error('Parser infrastructure error:',e.message);
  process.exit(2);
}
