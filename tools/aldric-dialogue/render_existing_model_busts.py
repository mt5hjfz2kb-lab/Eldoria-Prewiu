#!/usr/bin/env python3
"""Aldric dialogue bust assets, rendered from the authentic approved Tripo/Blender cape-blue art-pass GLB.
No painted character invention; images are static 3D captures, no facial morph claim.
Input checksum is enforced by caller. Assets are presentation-only, never gameplay.
"""
import os, sys, json, hashlib
from pathlib import Path
import numpy as np
import vtk
from vtk.util.numpy_support import vtk_to_numpy
from PIL import Image

SOURCE=Path(sys.argv[1])
OUT=Path(sys.argv[2])
OUT.mkdir(parents=True,exist_ok=True)
EXPECTED="b7b91923f1e8459a8327c98a83afc744cad44813267f92c59640cf755aecb555"
actual=hashlib.sha256(SOURCE.read_bytes()).hexdigest()
if actual != EXPECTED:
    raise SystemExit("WRONG_SOURCE_GLB expected="+EXPECTED+" actual="+actual)

# Eldoria-approved face/armor, shoulder and chest crop. These are lighting and
# viewpoint treatments only: DO NOT report morph expressions or animated faces.
poses={
 "aldric_neutral": ((2.9,.62,.72),[( (3,3.4,2.0),1.54,(1,.94,.85)),((-2,1.4,2.8),.92,(.73,.83,1)),((.4,2.0,-2.5),1.0,(.71,.86,1))]),
 "aldric_firme": ((3.0,.57,.32),[((2.9,3.5,1.9),1.72,(1,.90,.76)),((-2,1.2,1.8),.68,(.68,.81,1)),((-.3,2.3,-2.8),1.09,(.66,.77,1))]),
 "aldric_preocupado": ((2.9,.69,1.25),[((2.4,2.9,1.5),1.22,(.91,.88,.82)),((-2,1.3,2.8),.58,(.66,.74,.92)),((-.5,2.5,-2.2),1.24,(.56,.69,.99))]),
 "aldric_sereno": ((3.0,.56,.70),[((3.2,3.6,2.6),1.77,(1,.96,.87)),((-2,1.8,2.5),1.07,(.81,.86,1)),((-.5,2.7,-2.3),.98,(.75,.82,1))])
}
results=[]
for name,(position,lights) in poses.items():
    rw=vtk.vtkRenderWindow()
    rw.SetOffScreenRendering(1)
    rw.SetAlphaBitPlanes(1)
    rw.SetSize(512,512)
    ren=vtk.vtkRenderer()
    rw.AddRenderer(ren)
    ren.SetBackground(0,0,0)
    ren.SetBackgroundAlpha(0)
    imp=vtk.vtkGLTFImporter()
    imp.SetFileName(str(SOURCE))
    imp.SetRenderWindow(rw)
    imp.Update()
    actors=ren.GetActors().GetNumberOfItems()
    if actors<1: raise SystemExit("NO_MODEL_GEOMETRY_RENDERED")
    ren.RemoveAllLights()
    for pos,intensity,color in lights:
        l=vtk.vtkLight()
        l.SetLightTypeToSceneLight()
        l.SetPosition(*pos)
        l.SetFocalPoint(0,.296,0)
        l.SetIntensity(intensity)
        l.SetColor(*color)
        ren.AddLight(l)
    cam=ren.GetActiveCamera()
    cam.ParallelProjectionOn()
    cam.SetParallelScale(.235)
    cam.SetFocalPoint(0,.296,0)
    cam.SetPosition(*position)
    cam.SetViewUp(0,1,0)
    ren.ResetCameraClippingRange()
    rw.Render()
    w=vtk.vtkWindowToImageFilter()
    w.SetInput(rw)
    w.SetInputBufferTypeToRGBA()
    w.ReadFrontBufferOff()
    w.Update()
    rgba=vtk_to_numpy(w.GetOutput().GetPointData().GetScalars()).reshape(512,512,4)[::-1].copy()
    occupied=int((rgba[:,:,3]>220).sum())
    if occupied<118000 or (rgba[:,:,:3].max()<30):
        raise SystemExit("EMPTY_OR_MISFRAMED_RENDER "+name+" alpha="+str(occupied))
    img=Image.fromarray(rgba,'RGBA')
    # Owner correction 27/10: the previous bust still displayed too much torso.
    # Tight square crop from true rendered pixels: forehead, eyes, beard,
    # collar and shoulder tops ONLY. No armoured chest/waist in dialogue.
    # Normalised source area x=[138,374], y=[16,252] on the 512x512 render.
    # The RGBA alpha is retained; no generative repainting or Tripo rerun.
    tight_box=(138,16,374,252)
    img=img.crop(tight_box).resize((512,512),Image.Resampling.LANCZOS)
    occupied=int((np.asarray(img)[:,:,3]>220).sum())
    if occupied<100000:
        raise SystemExit("TIGHT_SHOULDER_CROP_ALPHA_TOO_SMALL "+name+" alpha="+str(occupied))
    out=OUT/(name+".png")
    img.save(out,optimize=True)
    meta=out.with_name(out.name+'.meta')
    guid=hashlib.sha256(('eldoria20261027/'+name).encode()).hexdigest()[:32]
    meta.write_text(
        'fileFormatVersion: 2\n'
        'guid: '+guid+'\n'
        'TextureImporter:\n'
        '  internalIDToNameTable: []\n'
        '  externalObjects: {}\n'
        '  serializedVersion: 13\n'
        '  mipmaps:\n'
        '    mipMapMode: 0\n'
        '    enableMipMap: 0\n'
        '  isReadable: 0\n'
        '  streamingMipmaps: 0\n'
        '  textureType: 0\n'
        '  textureShape: 1\n'
        '  sRGBTexture: 1\n'
        '  alphaSource: 1\n'
        '  alphaIsTransparency: 1\n'
        '  spriteMode: 1\n'
        '  spritePixelsToUnits: 100\n'
        '  alphaUsage: 1\n'
        '  textureCompression: 1\n'
        '  compressionQuality: 50\n'
        '  maxTextureSize: 512\n'
        '  platformSettings: []\n',
        encoding='utf-8')
    results.append(dict(key=name,size=out.stat().st_size,sha256=hashlib.sha256(out.read_bytes()).hexdigest(),opaque_pixels=occupied,source=actual))
    print("ALDRIC_BUST_RENDER",name,"bytes",out.stat().st_size,"opaque",occupied,flush=True)
    rw.Finalize()
manifest=OUT/'ALDRIC_BUST_PROVENANCE.json'
manifest.write_text(json.dumps({"source":actual,"tripo_credits_new":0,"model":"SirAldric_ArtPass_CapaAzul_Approved", "face_morph":"none; four camera/light treatments", "body_framing":"close head and shoulders, NO torso, owner correction 27/10","crop_box_512":[138,16,374,252],"output_size":[512,512],"unity_checked":False,"files":results},indent=2),encoding="utf-8")
