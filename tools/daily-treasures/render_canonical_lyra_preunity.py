#!/usr/bin/env python3
"""Offline portrait of the EXACT SHA-verified Eldoria Lyra 3D GLB.

This draws ORIGINAL geometry and its embedded texture, not an AI-invented
portrait. Cinematic key/fill lights alter illumination only. The output is
source reference, NOT a Unity screenshot, new playable hero or art approval.
Requires: pip install vtk (free/open-source); runs outside Windows lease.
"""
import argparse
import hashlib
import json
import math
import tempfile
from pathlib import Path

APPROVED_SHA = "a9fa7899e7ead2ad26e7b26d368c9006085ae8b8ded2df6515ca466c863f6725"
APPROVED_BYTES = 4770000

def render(model: Path, output: Path, yaw=147, width=1100, height=1500):
    import vtk
    from PIL import Image
    payload = model.read_bytes()
    digest = hashlib.sha256(payload).hexdigest()
    if len(payload) != APPROVED_BYTES or digest != APPROVED_SHA or payload[:4] != b"glTF":
        raise ValueError("Lyra model identity not verified; refuse fictional portrait")
    scene = vtk.vtkRenderer()
    scene.SetBackground(0.0, 0.0, 0.0)
    scene.SetBackgroundAlpha(0.0)
    window = vtk.vtkRenderWindow()
    window.OffScreenRenderingOn()
    window.SetAlphaBitPlanes(1)
    window.SetSize(width, height)
    window.AddRenderer(scene)
    # VTK dispatches glTF by extension: genuine Unity .bytes fails even
    # though it is the exact same checked GLB. Render a temporary .glb copy.
    temporary = tempfile.TemporaryDirectory(prefix="eldoria-canonical-lyra-")
    verified_glb = Path(temporary.name) / "exact-approved-model.glb"
    verified_glb.write_bytes(payload)
    importer = vtk.vtkGLTFImporter()
    importer.SetFileName(str(verified_glb.resolve()))
    importer.SetRenderWindow(window)
    importer.Update()
    actors = scene.GetActors()
    actors.InitTraversal()
    count = 0
    while actor := actors.GetNextActor():
        actor.GetProperty().SetAmbient(0.28)
        count += 1
    if count <= 0:
        raise RuntimeError("Original Lyra source has no renderable mesh")
    angle = math.radians(yaw)
    camera = scene.GetActiveCamera()
    camera.ParallelProjectionOff()
    camera.SetPosition(1.45 * math.sin(angle), 0.04, 1.45 * math.cos(angle))
    camera.SetFocalPoint(0.0, 0.0, 0.0)
    camera.SetViewUp(0.0, 1.0, 0.0)
    camera.SetViewAngle(39.0)
    camera.SetClippingRange(0.05, 10.0)
    scene.RemoveAllLights()
    # Original textured pixels remain the source of truth. These only light it.
    for offset, strength, rgb in [
        (-0.65, 2.40, (1.0, 0.78, 0.59)),
        (0.85, 1.55, (0.68, 0.77, 1.0)),
        (0.0, 0.55, (1.0, 1.0, 1.0))
    ]:
        lamp = vtk.vtkLight()
        lamp.SetLightTypeToSceneLight()
        lamp.SetPositional(False)
        lamp.SetPosition(1.45 * math.sin(angle) + offset,
                         1.25, 1.45 * math.cos(angle) + 0.25)
        lamp.SetFocalPoint(0.0, 0.0, 0.0)
        lamp.SetIntensity(strength)
        lamp.SetColor(*rgb)
        scene.AddLight(lamp)
    window.Render()
    capture = vtk.vtkWindowToImageFilter()
    capture.SetInput(window)
    capture.SetInputBufferTypeToRGBA()
    capture.ReadFrontBufferOff()
    capture.Update()
    writer = vtk.vtkPNGWriter()
    writer.SetFileName(str(output))
    writer.SetInputConnection(capture.GetOutputPort())
    writer.Write()
    with Image.open(output) as pic:
        assert pic.mode == "RGBA", "No transparent native source image"
        min_a, max_a = pic.getchannel("A").getextrema()
        bounds = pic.getchannel("A").getbbox()
        if min_a != 0 or max_a < 250 or bounds is None:
            raise RuntimeError("Transparent authentic portrait unavailable")
        size = list(pic.size)
    receipt = {
        "classification": "CANONICAL_3D_LYRA_SOURCE_ONLY_NOT_UNITY",
        "model_source": str(model),
        "source_sha256": digest,
        "source_length": len(payload),
        "output": str(output),
        "output_sha256": hashlib.sha256(output.read_bytes()).hexdigest(),
        "image_dimensions": size,
        "alpha_bounds": list(bounds),
        "actor_count": count,
        "view_yaw": yaw,
        "generated_person": False,
        "art_approved": False,
        "unity_verified": False,
        "limitation": "True canonical model but offline VTK lighting, not actual Unity scene"
    }
    output.with_suffix(".provenance.json").write_text(
        json.dumps(receipt, indent=2) + "\n", encoding="utf-8")
    return receipt

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--glb", type=Path, required=True)
    parser.add_argument("--out", type=Path, required=True)
    parser.add_argument("--yaw", type=int, default=147)
    args = parser.parse_args()
    args.out.parent.mkdir(parents=True, exist_ok=True)
    print(json.dumps(render(args.glb, args.out, args.yaw), indent=2))
