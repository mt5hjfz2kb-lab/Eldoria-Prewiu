"""Generic matched-camera evidence comparator; numerical checks never issue visual PASS."""
import json,sys,pathlib,hashlib
from PIL import Image,ImageDraw
request=json.load(open(sys.argv[1]));folder=pathlib.Path(sys.argv[2]);out=pathlib.Path(sys.argv[3]);out.mkdir(parents=True,exist_ok=True)
e=json.load(open(folder/'evidence.json'));views={v['name']:v for v in e['views']}
oldnames=set(request['replacement']['names']);results=[]
def bvec(b):return [b[k] for k in ['x','y','z','w']]
def union(bb):return [min(b[0] for b in bb),min(b[1] for b in bb),max(b[2] for b in bb),max(b[3] for b in bb)]
for v in request['views']:
 a=views['BEFORE-'+v['name']];b=views['AFTER-'+v['name']];before={x['name']:bvec(x['bbox']) for x in a['bounds']};after={x['name']:bvec(x['bbox']) for x in b['bounds']}
 protected={k:bb for k,bb in before.items() if k not in oldnames};missing=[k for k in protected if k not in after];delta=max([abs(x-y) for k,bb in protected.items() if k in after for x,y in zip(bb,after[k])]+[0])
 old=union([bb for k,bb in before.items() if k in oldnames and 'Wing' not in k]);new=union([bb for k,bb in after.items() if any(s in k for s in ['LowerGate_WestTower','LowerGate_EastTower','LowerGate_Arch','LowerGate_Base'])])
 ratios=[(new[2]-new[0])/(old[2]-old[0]),(new[3]-new[1])/(old[3]-old[1])];center=((new[0]+new[2]-old[0]-old[2])**2+(new[1]+new[3]-old[1]-old[3])**2)**.5/2
 results.append({'view':v['name'],'protected_mesh_count':len(protected),'protected_missing':missing,'protected_projection_max_delta_px':delta,'old_primary_bbox':old,'new_primary_bbox':new,'extent_ratio':ratios,'center_delta_px':center,'technical_alignment_pass':not missing and delta<.02 and all(.95<=q<=1.05 for q in ratios) and center<=12})
 # Exact camera full frame; optional target source preserved same aspect.
 images=[Image.open(folder/('BEFORE-'+v['name']+'.png')).convert('RGB'),Image.open(folder/('AFTER-'+v['name']+'.png')).convert('RGB')];labels=['APPROVED BLOCKOUT','LOWER GATE INTEGRATED']
 if v['name']=='UNITY-source-3x2':images.insert(0,Image.open(request['target_image']).convert('RGB'));labels.insert(0,'EXACT TARGET')
 w=v['width'];h=v['height'];canvas=Image.new('RGB',(w*len(images),h+40),'#17212b');d=ImageDraw.Draw(canvas)
 for i,(im,label) in enumerate(zip(images,labels)):canvas.paste(im,(i*w,40));d.text((i*w+16,12),label,fill='white')
 canvas.save(out/(v['name']+'-comparison.jpg'),quality=94)
 if v['name'] in ['UNITY-source-3x2','UNITY-mobile-landscape','UNITY-portrait-entry']:
  # Crops retain one shared absolute region; no independent fitting or different camera.
  bb=union([old,new]);pad=35;crop=(max(0,int(bb[0])-pad),max(0,int(bb[1])-pad),min(w,int(bb[2])+pad),min(h,int(bb[3])+pad));cw=crop[2]-crop[0];ch=crop[3]-crop[1]
  c=Image.new('RGB',(cw*len(images),ch+40),'#17212b');d=ImageDraw.Draw(c)
  for i,(im,label) in enumerate(zip(images,labels)):c.paste(im.crop(crop),(i*cw,40));d.text((i*cw+8,12),label,fill='white')
  c.save(out/(v['name']+'-gate-crop.jpg'),quality=96)
source_integrity_pass=bool(e.get('source_triangles',0)>0 and e.get('source_uv') and e.get('source_normals') and e.get('source_tangents') and e.get('colliders',0)==0 and not e.get('production_scene_opened') and not e.get('production_scene_saved'))
report={'classification':'TECHNICAL_MATCHED_CAMERA_CHECK_NOT_VISUAL_VERDICT','source_identity':request['source_glb'],'camera_request':sys.argv[1],'source_statistics':{k:v for k,v in e.items() if k.startswith('source_')},'results':results,'source_integrity_pass':source_integrity_pass,'all_technical_alignment_pass':source_integrity_pass and all(v['technical_alignment_pass'] for v in results),'tripo_credits':0,'gameplay_scene_opened':e['production_scene_opened'],'gameplay_scene_saved':e['production_scene_saved'],'colliders':e['colliders']}
(out/'matched-camera-metrics.json').write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report,indent=2))
