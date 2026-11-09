// Only a deliberately authored Unity marker may become a telemetry event.
// Ignore VALORIA_PLAYTEST_CHUNK, PlayerState, names, arbitrary console output.
const NAMES=new Set(['bastion_reached','end_of_slice','world_opened',
 'build_completed','march_sent','battle_resolved','lyra_state','fissure_state']);
const FIELDS=new Set(['bastion','level','building_code','march_slots_used','outcome_code','state_code']);
export function parseUnitySignal(value){
 if(typeof value!=='string'||value.length>600)return null;
 const marker='ELDORIA_BETA_SIGNAL|',start=value.indexOf(marker);
 if(start<0)return null;
 const firstLine=value.slice(start).split(/[\r\n]/,1)[0].trim();
 const match=/^ELDORIA_BETA_SIGNAL\|([a-z_]+)\|([A-Za-z0-9_|=]+)$/.exec(firstLine);
 if(!match||!NAMES.has(match[1]))return null;
 const payload={};
 for(const field of match[2].split('|')){
  const pivot=field.indexOf('=');
  if(pivot<=0||pivot===field.length-1)return null;
  const key=field.slice(0,pivot),val=field.slice(pivot+1);
  if(!FIELDS.has(key)||Object.hasOwn(payload,key))return null;
  if(key==='bastion'||key==='level'||key==='march_slots_used'){
   if(!/^[0-9]$/.test(val))return null;
   const n=Number(val);
   if(key==='march_slots_used'?n<1||n>3:n<1||n>5)return null;
   payload[key]=n;
  }else if(key==='building_code'){
   if(!['sawmill','barracks','granary','stoneworks'].includes(val))return null;
   payload[key]=val;
  }else if(key==='outcome_code'){
   if(!['win','loss','draw'].includes(val))return null;
   payload[key]=val;
  }else if(key==='state_code'){
   if(!['recruited','closed'].includes(val))return null;
   payload[key]=val;
  }
 }
 const name=match[1];
 if(name==='bastion_reached'&&!payload.bastion)return null;
 if(name==='end_of_slice'&&payload.bastion!==5)return null;
 if(name==='world_opened'&&!payload.bastion)return null;
 if(name==='build_completed'&&(!payload.building_code||!payload.level))return null;
 if(name==='march_sent'&&!payload.march_slots_used)return null;
 if(name==='battle_resolved'&&!payload.outcome_code)return null;
 if(name==='lyra_state'&&payload.state_code!=='recruited')return null;
 if(name==='fissure_state'&&payload.state_code!=='closed')return null;
 return {name,payload};
}
