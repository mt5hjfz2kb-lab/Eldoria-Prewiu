#!/usr/bin/env node
// Non-destructive triage of the EXISTING canonical documentation catalog.
// No status promotions, deletion, interpretation of old workstreams or
// automatic authority grants. Separate from the document governance checker.
import fs from 'node:fs';
import path from 'node:path';
import {fileURLToPath} from 'node:url';

const statuses=new Set(['current','reference','review_pending','historical']);
const critical=new Set(['operations','automation','infrastructure','governance','engineering','quality']);
const safe=x=>typeof x==='string'&&x.length>0&&!x.startsWith('/')&&!x.includes('\\')&&!x.split('/').includes('..');
export function documentTriage(catalog) {
  if(!catalog||catalog.schema_version!==1||!Array.isArray(catalog.entries))
    throw Error('Expected versioned canonical catalog');
  const byStatus=Object.fromEntries([...statuses].map(x=>[x,0]));
  const byDomain={},pending=[],seen=new Set();
  for(const e of catalog.entries){
    if(!e||!safe(e.path)||typeof e.domain!=='string'||!e.domain||
      !statuses.has(e.status)||seen.has(e.path)) throw Error('Invalid/duplicate catalog entry');
    seen.add(e.path);
    byStatus[e.status]+=1;
    byDomain[e.domain]??={total:0,review_pending:0};
    byDomain[e.domain].total+=1;
    if(e.status==='review_pending'){
      byDomain[e.domain].review_pending+=1;
      pending.push({path:e.path,domain:e.domain,
        review_priority:critical.has(e.domain)?'operational_first':'ordinary'});
    }
  }
  pending.sort((a,b)=>(a.review_priority==='operational_first'?0:1)-
      (b.review_priority==='operational_first'?0:1)||
    a.domain.localeCompare(b.domain)||a.path.localeCompare(b.path));
  return {schema_version:1,read_only:true,total:catalog.entries.length,by_status:byStatus,
    by_domain:Object.fromEntries(Object.entries(byDomain).sort(([a],[b])=>a.localeCompare(b))),
    pending_count:pending.length,priority_review_queue:pending,
    assurance:'Review priority is a heuristic, NOT a declaration that a document is invalid, obsolete or current.',
    boundaries:['Never change docs/documentation-catalog.json without owner/workstream scope.',
      'Historical records remain preserved; no workstream is revived.',
      'A green structural check does not determine whether advice is semantically current.']};
}
const sanitize=v=>String(v).replace(/[|\r\n<>]/g,' ').slice(0,200);
export function markdown(report,limit=15){
  let out='# Eldoria — triaje documental (solo lectura)\n\n';
  out+='Total '+report.total+' · vigentes '+report.by_status.current+' · referencias '+report.by_status.reference+
    ' · pendientes '+report.by_status.review_pending+' · históricos '+report.by_status.historical+'.\n\n';
  out+='**Próximos documentos a verificar**, sin re-clasificarlos automáticamente:\n\n';
  for(const e of report.priority_review_queue.slice(0,limit))
    out+='- '+sanitize(e.path)+' ('+sanitize(e.domain)+', '+sanitize(e.review_priority)+')\n';
  out+='\nEl orden prioriza dependencias operativas, no garantiza obsolescencia. Solo la autoridad del propietario y la evidencia vigente permiten cambiar estados.\n';
  return out;
}
export function cli(args){
  const root=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'../..');
  const i=args.indexOf('--catalog'), f=args.indexOf('--format');
  const catalogPath=i<0?path.join(root,'docs/documentation-catalog.json'):path.resolve(args[i+1]);
  const format=f<0?'json':args[f+1];
  if(!['json','markdown'].includes(format))throw Error('Expected --format json|markdown');
  const result=documentTriage(JSON.parse(fs.readFileSync(catalogPath,'utf8')));
  return format==='markdown'?markdown(result):JSON.stringify(result,null,2)+'\n';
}
if(process.argv[1]&&path.resolve(process.argv[1])===fileURLToPath(import.meta.url)){
  try{process.stdout.write(cli(process.argv.slice(2)));}
  catch(e){process.stderr.write('DOCUMENTATION_TRIAGE_ERROR: '+e.message+'\n');process.exitCode=1;}
}
