"""Manual target annotations vs projected 3D bounds. Never scores final art quality."""
import json, math
from pathlib import Path
import numpy as np
from PIL import Image, ImageDraw, ImageFont

OUT=Path('docs/evidence/valoria-art-production-reset-v1')
FONT=ImageFont.truetype('/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf',20)
TARGETS={
 'Bastion':{'bbox':[608,64,1265,270],'prefixes':['Bastion'],'anchor':[952,217]},
 'Lower gate':{'bbox':[565,526,838,737],'prefixes':['LowerGate_WestTower','LowerGate_EastTower','LowerGate_Lintel'],'anchor':[691,702]},
 'Bridge':{'bbox':[421,688,751,885],'prefixes':['BridgeDeck','BridgeParapet'],'anchor':[589,787]},
 'Stair':{'bbox':[792,272,948,381],'prefixes':['Stair'],'anchor':[860,357]},
 'Cabin':{'bbox':[383,306,527,404],'prefixes':['CabinRoof','CabinChimney','Cabin'],'anchor':[451,398]},
 'Camp':{'bbox':[1108,392,1380,499],'prefixes':['CampTent','CampFence'],'anchor':[1261,457]},
 'West wall':{'bbox':[327,446,495,579],'prefixes':['WestPartialWall','WestWallTower'],'anchor':[411,536]},
 'East wall':{'bbox':[1237,486,1458,653],'prefixes':['EastPartialWall','EastWallTower'],'anchor':[1361,590]},
}
# Prefix Cabin intentionally excludes its parcel fence; actual name starts CabinFence,
# filtered below to measure the building rather than the compound.
def box_for(metrics,prefixes):
 b=[v for k,v in metrics['projected_bounds'].items() if any(k.startswith(p) for p in prefixes) and k!='CabinFence']
 return [min(x[0] for x in b),min(x[1] for x in b),max(x[2] for x in b),max(x[3] for x in b)]

def annotate(im,boxes,color):
 im=im.convert('RGB').copy();d=ImageDraw.Draw(im)
 for label,bb in boxes.items():
  d.rectangle(bb,outline=color,width=3);x,y=bb[:2]
  d.text((x+3,y+3),label,font=FONT,fill=color,stroke_width=2,stroke_fill='black')
 return im

def evaluate(metrics):
 result={}
 for label,t in TARGETS.items():
  a=np.array(t['bbox'],float);b=np.array(box_for(metrics,t['prefixes']))
  ac=(a[:2]+a[2:])/2;bc=(b[:2]+b[2:])/2
  size=a[2:]-a[:2];bs=b[2:]-b[:2]
  result[label]={'target_bbox_manual':a.tolist(),'blockout_bbox_projected':np.round(b,3).tolist(),'center_error_px':round(float(np.linalg.norm(ac-bc)),3),'center_error_percent_frame_diagonal':round(float(np.linalg.norm(ac-bc))/math.hypot(1536,1024)*100,3),'width_ratio':round(float(bs[0]/size[0]),3),'height_ratio':round(float(bs[1]/size[1]),3),'target_annotation_uncertainty_px':12,'macro_tolerance':'center <= 2.5% frame diagonal; width and height ratios 0.75..1.25; no route-blocking occlusion'}
  result[label]['numerical_macro_pass']=result[label]['center_error_percent_frame_diagonal']<=2.5 and .75<=result[label]['width_ratio']<=1.25 and .75<=result[label]['height_ratio']<=1.25
 return result

if __name__=='__main__':
 metrics=json.loads((OUT/'BLOCKOUT-source-3x2-metrics.json').read_text())
 if (OUT/'unity-final/evidence.json').exists():
  evidence=json.loads((OUT/'unity-final/evidence.json').read_text())
  view=next(v for v in evidence['views'] if v['name']=='UNITY-source-3x2')
  metrics['projected_bounds']={v['name']:[v['bbox'][k] for k in ['x','y','z','w']] for v in view['bounds']}
 results=evaluate(metrics)
 (OUT/'target-blockout-comparison.json').write_text(json.dumps({'method':'Target manually annotated once; blockout measurements from projected mesh vertices, including hidden extents. Annotation uncertainty is explicit. No image similarity/final-art score.','source_resolution':[1536,1024],'landmarks':results},indent=2)+'\n')
 target=Image.open(OUT/'canonical-target.jpeg')
 block=Image.open(OUT/'unity-final/UNITY-source-3x2.png') if (OUT/'unity-final/UNITY-source-3x2.png').exists() else Image.open(OUT/'BLOCKOUT-source-3x2.png')
 ta=annotate(target,{k:v['bbox'] for k,v in TARGETS.items()},'#ffb452')
 ba=annotate(block,{k:box_for(metrics,v['prefixes']) for k,v in TARGETS.items()},'#66e4e7')
 ta.save(OUT/'annotated-target.png');ba.save(OUT/'annotated-blockout.png')
 panel=Image.new('RGB',(1536*2,1080),'#1c222a');panel.paste(ta,(0,56));panel.paste(ba,(1536,56));d=ImageDraw.Draw(panel)
 d.text((20,15),'OWNER TARGET — annotations only',font=FONT,fill='white');d.text((1556,15),'UNITY GREYBOX — no final art',font=FONT,fill='white');panel.save(OUT/'TARGET-vs-BLOCKOUT.png')
 Image.blend(target.convert('RGB'),block.convert('RGB'),.5).save(OUT/'target-blockout-50pct-overlay.png')
 svg=['<svg xmlns="http://www.w3.org/2000/svg" width="1536" height="1024" viewBox="0 0 1536 1024">','<title>Valoria reset target structure, manual screen-space measurements</title>']
 for k,v in TARGETS.items():
  x,y,xx,yy=v['bbox'];svg.append(f'<rect x="{x}" y="{y}" width="{xx-x}" height="{yy-y}" fill="none" stroke="#ffb452" stroke-width="3"/><text x="{x+4}" y="{y+23}" fill="#ffb452" font-size="20">{k}</text>')
 svg.append('<path d="M 444 920 L 691 702 L 860 357 L 952 217" fill="none" stroke="#66e4e7" stroke-width="5" stroke-dasharray="10 8"/>')
 svg.append('<polygon points="550,423 787,407 719,553 567,576" fill="#66e4e7" fill-opacity="0.15" stroke="#66e4e7"/><polygon points="963,408 1085,417 1230,537 1130,634 851,583" fill="#66e4e7" fill-opacity="0.15" stroke="#66e4e7"/></svg>')
 (OUT/'target-overlay.svg').write_text('\n'.join(svg)+'\n')
 # UI occupancy tests are explicitly provisional masks, not actual HUD integration.
 for name in ['BLOCKOUT-mobile-landscape','BLOCKOUT-portrait-home','BLOCKOUT-portrait-left','BLOCKOUT-portrait-right','BLOCKOUT-portrait-entry']:
  unity_name=name.replace('BLOCKOUT-','UNITY-');source=OUT/'unity-final'/(unity_name+'.png');im=Image.open(source if source.exists() else OUT/(name+'.png')).convert('RGBA');w,h=im.size;layer=Image.new('RGBA',(w,h),(0,0,0,0));d=ImageDraw.Draw(layer)
  top=.08 if w<h else .05;bottom=.16 if w<h else .12;side=.05
  d.rectangle((0,0,w,h*top),fill=(20,30,45,150));d.rectangle((0,h*(1-bottom),w,h),fill=(20,30,45,150))
  d.rectangle((0,0,w*side,h),fill=(20,30,45,80));d.rectangle((w*(1-side),0,w,h),fill=(20,30,45,80))
  d.text((8,8),'UI SAFE AREA',font=FONT,fill=(255,255,255,230));Image.alpha_composite(im,layer).convert('RGB').save(OUT/(name+'-safe-areas.png'))
 for k,v in results.items():print(k,v['numerical_macro_pass'],v['center_error_px'],v['width_ratio'],v['height_ratio'])
