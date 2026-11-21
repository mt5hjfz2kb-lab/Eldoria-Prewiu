"""Technical sprite import preparation; no artistic repaint or background removal."""
from pathlib import Path
from PIL import Image
import hashlib, json, uuid, sys

source=Path(sys.argv[1]); root=Path(__file__).resolve().parents[2]
out=root/'Unity/Assets/Experimentos/ValoriaModular25D/Resources/ValoriaModular25D/Progression'
out.mkdir(parents=True,exist_ok=True)
template=(out.parent/'bastion_i_hd.png.meta').read_text()
spec=json.loads((source/'progression-inputs.json').read_text())
records=[]
for entry in spec:
    name=entry['name']; path=source/entry['source']
    im=Image.open(path).convert('RGBA')
    alpha=im.getchannel('A')
    assert alpha.getextrema()[0]==0, 'Missing genuine transparency'
    box=alpha.point(lambda a:255 if a>=24 else 0).getbbox()
    crop=im.crop(box)
    keep=name.startswith('bastion_')
    width=1284 if keep else 880
    height=1340 if keep else 1000
    ratio=min(width/crop.width,height/crop.height)
    crop=crop.resize((round(crop.width*ratio),round(crop.height*ratio)),Image.Resampling.LANCZOS)
    canvas=Image.new('RGBA',(1440 if keep else 1024,crop.height+40))
    baseline=canvas.height-30
    canvas.alpha_composite(crop,((canvas.width-crop.width)//2,baseline-crop.height))
    target=out/(name+'.png');canvas.save(target,optimize=True)
    meta=template.replace('guid: c2aecccf5ac14725980519996db9f024','guid: '+uuid.uuid5(uuid.NAMESPACE_URL,'eldoria-corona-progression/'+name).hex)
    # Keep lower visible contact at world y=-39/288 for every tier.
    pivot=(canvas.height-baseline+39)/canvas.height
    meta=meta.replace('y: 0.048000','y: %.6f'%pivot)
    Path(str(target)+'.meta').write_text(meta)
    records.append(dict(name=name,source=entry['source'],source_sha256=hashlib.sha256(path.read_bytes()).hexdigest(),sha256=hashlib.sha256(target.read_bytes()).hexdigest(),dimensions=canvas.size,pixels_per_unit=288,pivot_y=pivot,brief=entry['brief']))
(out.parent/'Progression.meta').write_text('fileFormatVersion: 2\nguid: '+uuid.uuid5(uuid.NAMESPACE_URL,'eldoria-corona-progression-folder').hex+'\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n')
doc=root/'docs/valoria-modular-25d';doc.mkdir(parents=True,exist_ok=True)
(doc/'CORONA_PROGRESSION_ASSET_PROVENANCE_20261121.json').write_text(json.dumps({'method':'Built-in image generation with approved original sprite references; only uniform resize and foot-anchor padding afterward','assets':records},indent=2))
print(json.dumps(records,indent=2))
