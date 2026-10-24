#!/usr/bin/env node
// Synthetic but real Git history: narrow bridge iterations, stale strategy, valid pivot, broad progress.
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import {spawnSync,execFileSync} from 'node:child_process';
const gate=path.resolve('tools/world4x-artistic-process/strategy.mjs');
const tmp=fs.mkdtempSync(path.join(os.tmpdir(),'eldoria-direction-'));
const repo=path.join(tmp,'game'),folder='docs/evidence/world4x-canon-20261022/authoring',decision='docs/evidence/world4x-canon-20261022/PRODUCTION_DIRECTION_CURRENT.json';
function git(...args){return execFileSync('git',['-C',repo,...args],{encoding:'utf8'}).trim();}
function file(p,content){let f=path.join(repo,p);fs.mkdirSync(path.dirname(f),{recursive:true});fs.writeFileSync(f,content);}
function change(message){file(folder+'/bridge-geom.py',message+'\n');git('add','.');git('commit','-m',message);return git('rev-parse','HEAD');}
function check(expect,needle){const r=spawnSync(process.execPath,[gate,repo],{encoding:'utf8'});const s=(r.stdout||'')+(r.stderr||'');if((r.status===0)!==expect||!s.includes(needle))throw Error('Wrong result ['+expect+', '+needle+'] '+r.status+' '+s.slice(0,2500));console.log('STRATEGY_SMOKE_PASS '+needle);}
try{
  fs.mkdirSync(repo,{recursive:true});execFileSync('git',['init',repo],{stdio:'ignore'});
  git('config','user.email','fixture@example.invalid');git('config','user.name','Fixture');
  change('region-production(world4x): first bridge study');
  check(true,'WORLD4X_DIRECTION_OK');
  change('region-production(world4x): improve central bridge banks');
  change('region-production(world4x): remeasure bridge deck');
  const latest=change('region-production(world4x): change bridge castle gate overlap');
  check(false,'Missing strategic direction record');
  const plan={
    schema_version:1,workstream_id:'eldoria-world-region-1-canon-20261022',repeated_topic:'bridge',responds_to_sha:latest,
    decision:'PIVOT_TO_HIGH_IMPACT',visual_evidence:'La captura real regional mantiene bosques planos y un puente simplificado; el puente no corrige la Brecha de la panorámica.',
    root_cause:'Se está empleando el mismo tipo de pequeño arreglo del puente sin replantear las prioridades de la composición ni los problemas de raíz.',
    reason_for_priority:'Una revisión del mundo debe atender a la composición panorámica, cuyo defecto de masas afecta una superficie mucho mayor que el puente cercano.',
    competing_problems:[
      {view:'cercana',defect:'Puente no conectado visualmente al camino del castillo',impact:3},
      {view:'panoramica',defect:'La Brecha carece de protagonismo y las masas son uniformes',impact:5}
    ],
    preserve_AF_AG:true,heavy_run_rule:'QUICK_VISUAL_PREVIEW_FIRST',
    shared_artistic_goal:'Devolver jerarquía geográfica al continente, con la Brecha central y las masas forestales integradas en las escalas visuales.',
    next_batch:[
      {view:'panoramica',visual_change:'Diseñar una Brecha legible desde el zoom panorámico con geología real y no una pegatina radial.'},
      {view:'regional',visual_change:'Recomponer los bordes de las masas forestales en agrupaciones continuas de árboles auténticos.'}
    ]
  };
  file(decision,JSON.stringify({...plan,responds_to_sha:'a'.repeat(40)}));check(false,'decision is stale');
  file(decision,JSON.stringify(plan));check(true,'WORLD4X_STRATEGIC_DIRECTION_REVIEW_PASS');
  file(decision,JSON.stringify({...plan,next_batch:[]}));check(false,'pivot must group >=2');
  file(decision,JSON.stringify(plan));
  change('region-production(world4x): one more bridge micro-fix');check(false,'decision is stale');
  change('region-production(world4x): reshape continental panorama and forests');
  check(true,'WORLD4X_DIRECTION_OK');
  console.log('WORLD4X_DIRECTION_TESTS_ALL_PASS');
}finally{fs.rmSync(tmp,{recursive:true,force:true});}
