#!/usr/bin/env python3
"""Canonical Lyra PBR studio source-art experiment. Never a generated heroine.

Renders the approved PR127 original GLB with physically lit 3D (neutral studio
IBL, 3-point local lighting and source-space anti-aliasing), rather than
brightening/upscaling the rejected dark software snapshot. This is pre-Unity
evidence only. The original character, mesh and textures remain unchanged.
"""
import argparse
import hashlib
import io
import json
import tempfile
from pathlib import Path

import numpy as np
from PIL import Image
import vtk
from vtk.util.numpy_support import vtk_to_numpy

EXPECTED_SHA = "a9fa7899e7ead2ad26e7b26d368c9006085ae8b8ded2df6515ca466c863f6725"
EXPECTED_SIZE = 4770000

def environment_texture():
    # Neutral silk-softbox environment. Tiny physically motivated light map,
    # NOT a synthetic character, model, reward, building, or world landscape.
    w,h = 1024,512
    x,y = np.meshgrid((np.arange(w)+.5)/w, (np.arange(h)+.5)/h)
    key = np.exp(-.5*(((x-.44)/.17)**2+((y-.40)/.26)**2))
    fill = np.exp(-.5*(((x-.82)/.22)**2+((y-.46)/.30)**2))
    env = (np.array([.44,.45,.46])[None,None,:] +
           key[:,:,None]*np.array([2.05,1.83,1.57])[None,None,:] +
           fill[:,:,None]*np.array([.86,.94,1.08])[None,None,:]).astype("float32")
    texture_data = vtk.vtkImageData()
    texture_data.SetDimensions(w,h,1)
    texture_data.AllocateScalars(vtk.VTK_FLOAT,3)
    vtk_to_numpy(texture_data.GetPointData().GetScalars())[:] = env.reshape(-1,3)
    texture = vtk.vtkTexture()
    texture.SetInputData(texture_data)
    texture.InterpolateOn()
    texture.MipmapOn()
    texture.RepeatOn()
    return texture

def render(source:Path, output:Path, camera_x=.065):
    data=source.read_bytes()
    digest=hashlib.sha256(data).hexdigest()
    if digest!=EXPECTED_SHA or len(data)!=EXPECTED_SIZE or data[:4]!=b"glTF":
        raise ValueError("Canonical PR127 Lyra GLB proof failed: no invented substitute")
    width,height=1100,1500
    rw,rh=round(width*1.5),round(height*1.5)
    window=vtk.vtkRenderWindow()
    window.SetOffScreenRendering(True)
    window.SetAlphaBitPlanes(1)
    window.SetMultiSamples(8)
    window.SetSize(rw,rh)
    renderer=vtk.vtkRenderer()
    window.AddRenderer(renderer)
    with tempfile.TemporaryDirectory(prefix="eldoria-original-lyra-") as folder:
        model=Path(folder)/"immutable-approved.glb"
        model.write_bytes(data)
        importer=vtk.vtkGLTFImporter()
        importer.SetFileName(str(model))
        importer.SetRenderWindow(window)
        importer.Update()
        scene=window.GetRenderers().GetFirstRenderer()
        if scene.GetActors().GetNumberOfItems()!=1:
            raise RuntimeError("Expected exactly one authorized original Lyra mesh")
        scene.AutomaticLightCreationOff()
        scene.SetBackground(.025,.027,.031)
        scene.SetBackgroundAlpha(0.)
        scene.SetEnvironmentTexture(environment_texture(),False)
        scene.UseImageBasedLightingOn()
        camera=scene.GetActiveCamera()
        camera.SetPosition(camera_x,.27,-1.08)
        camera.SetFocalPoint(0,.255,0)
        camera.SetViewUp(0,1,0)
        camera.SetViewAngle(29)
        scene.ResetCameraClippingRange()
        for pos,intensity,color in [
            ((-.52,.88,-.75),1.60,(1.,.89,.75)),
            ((.45,.62,-.61),.69,(.76,.85,1.)),
            ((.72,.95,.8),.83,(1.,.73,.49))]:
            light=vtk.vtkLight()
            light.SetLightTypeToSceneLight()
            light.SetPosition(*pos)
            light.SetFocalPoint(0,.25,0)
            light.SetIntensity(intensity)
            light.SetColor(*color)
            scene.AddLight(light)
        window.Render()
        capture=vtk.vtkWindowToImageFilter()
        capture.SetInput(window)
        capture.SetInputBufferTypeToRGBA()
        capture.ReadFrontBufferOff()
        capture.Update()
        writer=vtk.vtkPNGWriter()
        writer.WriteToMemoryOn()
        writer.SetInputConnection(capture.GetOutputPort())
        writer.Write()
        rgba=Image.open(io.BytesIO(bytes(memoryview(writer.GetResult())))).convert("RGBA")
        rgba=rgba.resize((width,height),Image.Resampling.LANCZOS)
    output.parent.mkdir(parents=True,exist_ok=True)
    rgba.save(output,optimize=True)
    bbox=rgba.getchannel("A").getbbox()
    pixels=np.asarray(rgba)
    solid=pixels[:,:,3]>200
    visible=float(solid.mean())
    if bbox is None or visible<.32 or bbox[1]<10 or bbox[0]>width*.40:
        raise RuntimeError(f"Art is still empty/cropped: bounds={bbox}, coverage={visible:.4f}")
    # Source-only pixel evidence; no claim of 7.5/10 or Unity certification.
    record={
       "classification":"ORIGINAL_CANONICAL_LYRA_PBR_STUDIO_CANDIDATE_NOT_GAMEPLAY",
       "input":"Unity/Assets/Eldoria/Resources/EldoriaV41/lyra-approved-model-v1.bytes",
       "input_sha256":digest,"source_edited":False,"generated_person":False,
       "renderer":"VTK real original glTF, IBL + 3 scene lights + 1.5x supersampling",
       "camera_x":camera_x,"output_sha256":hashlib.sha256(output.read_bytes()).hexdigest(),
       "size":[width,height],"alpha_bounds":list(bbox),
       "visible_ratio":round(visible,4),"art_approved":False,"unity_native_verified":False,
       "note":"Independent render method test, not approved game art or replacement hero"}
    output.with_suffix(".provenance.json").write_text(
        json.dumps(record,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
    print(json.dumps(record,ensure_ascii=False))

if __name__=="__main__":
    arg=argparse.ArgumentParser()
    arg.add_argument("--glb",required=True,type=Path)
    arg.add_argument("--out",required=True,type=Path)
    arg.add_argument("--camera-x",type=float,default=.065)
    a=arg.parse_args()
    render(a.glb,a.out,a.camera_x)
