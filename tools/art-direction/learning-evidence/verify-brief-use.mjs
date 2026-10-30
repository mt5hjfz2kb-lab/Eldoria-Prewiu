#!/usr/bin/env node
// Fail-closed, read-only evidence that a previously prepared lesson brief is
// present in the actual prompt FILE. This cannot certify model inference,
// aesthetic improvement, a Unity run or ART_APPROVED.
import fs from 'node:fs';
import path from 'node:path';
import {createHash} from 'node:crypto';
import {fileURLToPath} from 'node:url';

const hash = b => createHash('sha256').update(b).digest('hex');
const isHash = x => typeof x==='string' && /^[a-f0-9]{64}$/.test(x);
const insist = (ok,msg) => {if(!ok) throw Error(msg);};

export function verifyBriefUse({brief,provenance,prompt,catalog}) {
  const data=JSON.parse(provenance.toString('utf8'));
  const canon=JSON.parse(catalog.toString('utf8'));
  const text=brief.toString('utf8'),input=prompt.toString('utf8');
  insist(provenance.length<32768 && catalog.length<4*1024*1024 &&
    prompt.length<2*1024*1024 && brief.length<16384,'Unbounded or unexpected input size');
  insist(data.schema_version===1 && /^[a-zA-Z0-9_-]{5,110}$/.test(data.target||''),
    'Invalid provenance version/target');
  insist(data.status==='HISTORIC_ADVICE_NOT_MODEL_TRAINING_OR_ART_PASS',
    'Prepared brief status does not authorize art approval');
  insist(isHash(data.brief_sha256)&&isHash(data.catalog_sha256)&&
    isHash(data.reference_sha256),'Missing trusted hashes');
  insist(hash(brief)===data.brief_sha256,'Prepared brief SHA mismatch');
  insist(hash(catalog)===data.catalog_sha256,'Source catalog has changed since prepare');
  const approved=canon.current_reference_sha256||canon.reference_sha256;
  insist(approved===data.reference_sha256,'Source reference changed since prepare');
  insist(Array.isArray(canon.cases),'Invalid observation catalog');
  insist(canon.cases.filter(x=>x.target===data.target).length===data.case_count,
    'Prepared lesson-count no longer matches catalog');
  insist(text.length>=100 && input.includes(text),
    'Verbatim memory brief missing from prompt file; do not claim used');
  return {status:'BRIEF_IN_PROMPT_FILE_VERIFIED',target:data.target,
    catalog_sha256:data.catalog_sha256,brief_sha256:data.brief_sha256,
    limitations:['Prompt-file inclusion does NOT prove that a model ran or used the advice.',
      'This check does not approve geometry, aesthetics, performance, gameplay or release.']};
}

export function cli(args) {
  const flags=new Map();
  for(let i=0;i<args.length;i+=2){
    insist(/^--(brief|provenance|prompt|catalog)$/.test(args[i]||'') &&
      typeof args[i+1]==='string','Expected --brief --provenance --prompt --catalog file paths');
    insist(!flags.has(args[i]),'Repeated argument');
    flags.set(args[i],args[i+1]);
  }
  insist(flags.size===4,'Required four input files');
  const load=k=>fs.readFileSync(path.resolve(flags.get(k)));
  return verifyBriefUse({brief:load('--brief'),provenance:load('--provenance'),
    prompt:load('--prompt'),catalog:load('--catalog')});
}

if(process.argv[1] && path.resolve(process.argv[1])===fileURLToPath(import.meta.url)){
  try{process.stdout.write(JSON.stringify(cli(process.argv.slice(2)),null,2)+'\n');}
  catch(e){process.stderr.write('MEMORY_PREFLIGHT_BLOCKED: '+e.message+'\n');process.exitCode=1;}
}
