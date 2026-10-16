#!/usr/bin/env node
// Source-origin gate for FIRST isolated Valoria layers; never claims SHARP parity or art success.
import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
const root=process.cwd();
const layoutPath='Unity/Assets/Eldoria/ArtTests/ValoriaLayeredConstruction/Layout/certified-legacy-skeleton.json';
const masterPath='Unity/Assets/Eldoria/Scripts/Presentation/VisualWorld.cs';
const refPath='references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg';
const dest='docs/evidence/valoria-layered-construction-20261016';
const data=JSON.parse(fs.readFileSync(path.join(root,layoutPath),'utf8'));
const src=fs.readFileSync(path.join(root,masterPath),'utf8');
const raw=fs.readFileSync(path.join(root,masterPath));
const sha=crypto.createHash('sha1').update(Buffer.concat([Buffer.from('blob '+raw.length+'\0'),raw])).digest('hex');
const refSha=crypto.createHash('sha256').update(fs.readFileSync(refPath)).digest('hex');
function assert(x,message){if(!x)throw Error('VALORIA_SOURCE_GATE_FAIL '+message);}
assert(sha===data.source.blob_sha,'VisualWorld exact Git blob was changed: '+sha);
assert(refSha===data.visual_authority.sha256,'Approved reference SHA-256 changed');
assert(data.visual_authority.legacy_qa_only===true,'Historical plane must never be canonical');
assert(data.footprints.length===data.expected_footprint_count,'Footprint count changed');
assert(data.footprints.length===22,'Expected 22 actual source footprints');
assert(data.stair.count===12,'Expected 12 steps');
const names=new Set();
const diffs=[];
for(const f of data.footprints){
 assert(!names.has(f.id),'Repeated key '+f.id);names.add(f.id);
 const pos=src.indexOf('"'+f.id+'"');
 assert(pos>=0,'Missing source object '+f.id);
 const fragment=src.slice(pos,pos+345);
 const vecs=[...fragment.matchAll(/new Vector3\(([^)]+)\)/g)].slice(0,2);
 assert(vecs.length>=2,'Missing actual center/size pair for '+f.id);
 const got=vecs.map(x=>x[1].split(',').map(Number.parseFloat));
 assert(got.every(v=>v.length===3&&v.every(Number.isFinite)),'Unparseable source Vector3 '+f.id);
 const expected=[f.center,f.size];
 for(let j=0;j<2;j++)for(let k=0;k<3;k++){
   if(Math.abs(expected[j][k]-got[j][k])>1e-4)
     diffs.push({id:f.id,kind:j===0?'center':'size',axis:k,expected:expected[j][k],found:got[j][k]});
 }
}
assert(diffs.length===0,'Source drift '+JSON.stringify(diffs));
const stairs=src.slice(src.indexOf('const int steps=12'),src.indexOf('float l1=2.55f;'));
assert(stairs.includes('const float rise=.18f;')&&stairs.includes('const float depth=.52f;'),
       'Stair distances drifted');
assert(data.bridge.certified===false,'Bridge dimensions must not be elevated to certified');
assert(data.canonical_sharp_camera.projection==='perspective','SHARP projection was replaced');
const kinds=Object.groupBy?Object.groupBy(data.footprints,x=>x.kind):{};
fs.mkdirSync(path.join(root,dest),{recursive:true});
const minX=-31,maxX=31,minZ=-13,maxZ=30, scale=15, W=1200,H=850;
const px=x=>Math.round(125+(x-minX)*scale);
const py=z=>Math.round(735-(z-minZ)*scale);
const esc=s=>String(s).replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;');
function tint(kind){
 if(kind.startsWith('route'))return '#b3b8bb';
 if(kind.startsWith('plot'))return '#d0b98b';
 if(kind==='floor_upper')return '#b8c49b';
 if(kind.startsWith('reserve'))return '#b7cbb4';
 if(kind.startsWith('terrain'))return '#87977b';
 if(kind==='gate_hotspot_volume')return '#9b6657';
 if(kind==='support')return '#7d827b';
 return '#d8c5a2';
}
const p=[];
p.push('<svg xmlns="http://www.w3.org/2000/svg" width="'+W+'" height="'+H+'" viewBox="0 0 '+W+' '+H+'">');
p.push('<rect width="100%" height="100%" fill="#f8f7f2"/>');
p.push('<text x="45" y="44" font-size="26" fill="#273538" font-weight="bold">VALORIA · plano maestro recuperado · geometría histórica exacta</text>');
p.push('<text x="45" y="72" font-size="15" fill="#754735">VERIFICADO contra VisualWorld.cs por SHA de blob Git y 22 centros/dimensiones; NO es el runtime SHARP</text>');
for(let x=-30;x<=30;x+=5){p.push('<path d="M'+px(x)+' 90V760" stroke="#b6bbb5" opacity=".5"/>');p.push('<text x="'+(px(x)-8)+'" y="784" font-size="12">'+x+'</text>');}
for(let z=-10;z<=30;z+=5){p.push('<path d="M120 '+py(z)+' H1070" stroke="#b6bbb5" opacity=".5"/>');p.push('<text x="81" y="'+(py(z)+4)+'" font-size="12">'+z+'</text>');}
const sorted=[...data.footprints].sort((a,b)=>(
  ({terrain_growth:0,terrain:1,reserve_west:2,reserve_east:2,reserve_future:2,
     support:3,reserve_upper:4,route_growth:5,route_upper:5,floor_lower:6,
     floor_upper:6,plot_west:7,plot_east:7,plot_upper:7,route_lower:8,gate_hotspot_volume:10}[a.kind]??4)-
  ({terrain_growth:0,terrain:1,reserve_west:2,reserve_east:2,reserve_future:2,
     support:3,reserve_upper:4,route_growth:5,route_upper:5,floor_lower:6,
     floor_upper:6,plot_west:7,plot_east:7,plot_upper:7,route_lower:8,gate_hotspot_volume:10}[b.kind]??4)
));
for(const f of sorted){
 const [x,,z]=f.center,[w,,h]=f.size;
 p.push('<rect x="'+px(x-w/2)+'" y="'+py(z+h/2)+'" width="'+Math.round(w*scale)+'" height="'+Math.round(h*scale)+'" fill="'+tint(f.kind)+'" fill-opacity=".73" stroke="#425052" stroke-width="1.5"><title>'+esc(f.id)+'  XYZ='+f.center.join(',')+'  SIZE='+f.size.join(',')+'</title></rect>');
}
for(let i=0;i<12;i++){const z=data.stair.first_center[2]+i*data.stair.run;const w=data.stair.approx_width;
 p.push('<rect x="'+px(-w/2)+'" y="'+py(z+data.stair.step_depth/2)+'" width="'+Math.round(w*scale)+'" height="'+Math.round(data.stair.step_depth*scale)+'" fill="#986d50" stroke="#51352a"/>');
}
for(const [x,z,label] of [[0,-9.3,'PUERTA'],[-7,-2.8,'ASERRADERO'],[7,-4,'CUARTEL'],[0,2.8,'ESCALERA'],[0,7.0,'L1 / BASTIÓN'],[-16,1.4,'OESTE'],[16,1.8,'ESTE'],[0,15.4,'CÍVICO'],[0,24,'FUTURO']]){
 p.push('<text x="'+px(x)+'" y="'+(py(z)+3)+'" text-anchor="middle" font-size="12" font-weight="bold" fill="#253034" stroke="#f8f7f2" stroke-width="2.4" paint-order="stroke">'+esc(label)+'</text>');
}
p.push('<text x="125" y="812" font-size="13" fill="#6c4240">Puente no certificado por este esqueleto · capas artísticas 3D deben permanecer aisladas hasta validación de cámara y gameplay.</text>');
p.push('</svg>');
fs.writeFileSync(path.join(root,dest,'master-plan-certified-legacy.svg'),p.join('\n')+'\n');
const result={status:"SOURCE_TOPOLOGY_PASS",classification:data.classification,source_blob_sha:sha,
reference_sha256:refSha,verified_footprints:22,stair_count:12,proof:"22 source constructor XYZ/scale pairs compared to manifest; 12-step rise/run recovered",
bridge_geometry_certified:false,canonical_sharp_gameplay_certified:false,physical_mobile_tested:false,
generated_svg:dest+'/master-plan-certified-legacy.svg'};
fs.writeFileSync(path.join(root,dest,'source-topology-gate.json'),JSON.stringify(result,null,2)+'\n');
process.stdout.write('VALORIA_MASTER_SOURCE_PASS zones=22 steps=12 source='+sha.slice(0,12)+' reference='+refSha.slice(0,12)+'\n');
