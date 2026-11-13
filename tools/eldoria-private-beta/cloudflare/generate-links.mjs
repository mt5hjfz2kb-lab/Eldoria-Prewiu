#!/usr/bin/env node
// Generate invitations ONLY AFTER the tester agrees to anonymized analytics.
// The secret belongs in the local host environment AND Cloudflare Secret store.
import {createHmac,randomUUID} from 'node:crypto';
const [origin,countText='3',daysText='14']=process.argv.slice(2);
const secret=process.env.ELDORIA_LINK_SECRET;
if(!secret||secret.length<32)throw Error('Missing 32+ byte ELDORIA_LINK_SECRET');
if(!/^https:\/\/[a-z0-9.-]+$/i.test(origin||''))throw Error('Expected https origin');
const count=Number(countText),days=Number(daysText);
if(!Number.isInteger(count)||count<1||count>25)throw Error('count 1..25');
if(!Number.isInteger(days)||days<1||days>30)throw Error('days 1..30');
const exp=Math.floor(Date.now()/1000)+days*86400;
console.log('TESTER_ID\tENLACE (tratar como secreto)');
for(let i=0;i<count;i++){
 const id=randomUUID(),data=Buffer.from('v1:'+id+':'+exp);
 const sig=createHmac('sha256',secret).update(data).digest('base64url');
 console.log(id+'\t'+origin+'/entrar/'+data.toString('base64url')+'.'+sig);
}
console.error('Copy IDs into CONSENTED_TESTER_IDS only after consent; never commit URLs or secret.');
