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

def render(model: Path, output: Path, yaw=147, width=1100, height=1500,
           shot="full"):
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
    loaded = importer.Update()
    if loaded is False:
        raise RuntimeError("VTK rejected the SHA-verified GLB")
    actors = scene.GetActors()
    actors.InitTraversal()
    count = 0
    while actor := actors.GetNextActor():
        actor.GetProperty().SetAmbient(0.28)
        count += 1
    if count <= 0:
        raise RuntimeError("Original Lyra source has no renderable mesh")
    # Camera distance MUST follow the actual imported GLB geometry. The old
    # hard-coded 1.45-unit position could produce an empty/cropped portrait
    # while its source-GUID check still passed.
    bounds = tuple(float(x) for x in scene.ComputeVisiblePropBounds())
    if (len(bounds) != 6 or not all(math.isfinite(v) for v in bounds)
            or any(bounds[k + 1] <= bounds[k] for k in (0, 2, 4))):
        raise RuntimeError("Canonical model has invalid visible bounds")
    center = tuple((bounds[k] + bounds[k + 1]) / 2 for k in (0, 2, 4))
    diagonal = math.sqrt(sum((bounds[k+1] - bounds[k]) ** 2
                             for k in (0, 2, 4)))
    if not 0.00001 < diagonal < 100000:
        raise RuntimeError("Canonical Lyra geometry has unreasonable scale")
    if shot not in ("full","bust","portrait"):
        raise ValueError("Camera shot must be full, bust or portrait")
    # A genuine waist-up hero reveals original facial/armor detail at phone
    # size. Only the CAMERA crops the EXACT GLB; no hallucinated portrait.
    frame_bounds = bounds if shot=="full" else (
        bounds[0],bounds[1],
        bounds[2]+(bounds[3]-bounds[2])*.53,bounds[3],
        bounds[4],bounds[5])
    focus = tuple((frame_bounds[k]+frame_bounds[k+1])/2 for k in (0,2,4))
    angle = math.radians(yaw)
    camera = scene.GetActiveCamera()
    camera.ParallelProjectionOff()
    camera.SetPosition(focus[0] + 2.0 * diagonal * math.sin(angle),
                       focus[1] + 0.18 * diagonal,
                       focus[2] + 2.0 * diagonal * math.cos(angle))
    camera.SetFocalPoint(*focus)
    camera.SetViewUp(0.0, 1.0, 0.0)
    camera.SetViewAngle(39.0)
    scene.ResetCamera(frame_bounds)
    if shot=="portrait":
        # AUTHENTIC GLB, new camera only: a true shoulder-and-face close-up.
        # The older 'bust' mode still framed much of the costume and hid
        # original facial detail at 390px. Source geometry is unchanged.
        body_height=bounds[3]-bounds[2]
        close_focus=(focus[0],bounds[3]-.22*body_height,focus[2])
        camera.ParallelProjectionOn()
        camera.SetFocalPoint(*close_focus)
        camera.SetPosition(close_focus[0]+2.0*diagonal*math.sin(angle),
                           close_focus[1]+.20*diagonal,
                           close_focus[2]+2.0*diagonal*math.cos(angle))
        camera.SetParallelScale(.24*body_height)
        focus=close_focus
    scene.ResetCameraClippingRange()
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
        lamp.SetPosition(focus[0] + diagonal * (1.45 * math.sin(angle) + offset),
                         focus[1] + 1.25 * diagonal,
                         focus[2] + diagonal * (1.45 * math.cos(angle) + 0.25))
        lamp.SetFocalPoint(*focus)
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
        alpha_bounds = pic.getchannel("A").getbbox()
        if min_a != 0 or max_a < 250 or alpha_bounds is None:
            raise RuntimeError("Transparent authentic portrait unavailable")
        # Fail on a technically successful but visually empty / badly framed
        # raster, a real failure mode not caught by a mesh-count assertion.
        alpha = pic.getchannel("A")
        visible_ratio = sum(n for i, n in enumerate(alpha.histogram()) if i >= 160) / (width * height)
        clear_ratio = sum(n for i, n in enumerate(alpha.histogram()) if i <= 16) / (width * height)
        if not 0.07 < visible_ratio < 0.93 or clear_ratio < 0.04:
            raise RuntimeError(f"Canonical portrait is empty/cropped: visible={visible_ratio:.3f} clear={clear_ratio:.3f}")
        colors = {pic.getpixel((x, y))[:3]
                  for y in range(0, height, max(1, height // 70))
                  for x in range(0, width, max(1, width // 60))
                  if alpha.getpixel((x, y)) > 160}
        if len(colors) < 30:
            raise RuntimeError("Canonical portrait lacks textured detail")
        size = list(pic.size)
    receipt = {
        "classification": "CANONICAL_3D_LYRA_SOURCE_ONLY_NOT_UNITY",
        "model_source": str(model),
        "source_sha256": digest,
        "source_length": len(payload),
        "output": str(output),
        "output_sha256": hashlib.sha256(output.read_bytes()).hexdigest(),
        "image_dimensions": size,
        "alpha_bounds": list(alpha_bounds),
        "actor_count": count,
        "geometry_bounds": list(bounds),
        "geometry_diagonal": round(diagonal, 6),
        "camera_framed_bounds": list(frame_bounds),
        "shot": shot,
        "portrait_camera_half_height_world":
            round(.24*(bounds[3]-bounds[2]),6) if shot=="portrait" else None,
        "identity_geometry_modified": False,
        "visible_pixel_ratio": round(visible_ratio, 4),
        "clear_pixel_ratio": round(clear_ratio, 4),
        "sampled_opaque_colors": len(colors),
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
    parser.add_argument("--shot", choices=("full","bust","portrait"), default="full")
    args = parser.parse_args()
    args.out.parent.mkdir(parents=True, exist_ok=True)
    print(json.dumps(render(args.glb,args.out,args.yaw,shot=args.shot), indent=2))
