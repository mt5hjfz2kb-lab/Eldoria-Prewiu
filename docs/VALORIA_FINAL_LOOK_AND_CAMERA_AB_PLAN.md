# Valoria Final Look + Camera A/B Plan

Status: execution-ready research/specification  
Program: **VALORIA PRODUCTION ART SYSTEM RESET v1**  
Updated: 2026-10-04

## Goal

Create a reversible, evidence-driven URP final-look profile and compare the current orthographic camera with a long-focal-length strategic perspective camera without changing gameplay authority.

This document is not a checklist to enable everything. Features survive only when a matched A/B improves the real Valoria frame at acceptable cost.

## Confirmed engine baseline

Repository baseline:
- Unity 6000.3.23f1
- URP 17.3.0

No evidence currently justifies changing engine or render pipeline.

## Official Unity evidence used

Unity 6 URP documents confirm:
- SSAO is a Renderer Feature, independent of Volume post-processing, and supports URP opaque shaders / compatible opaque Shader Graphs.
- SSAO can use camera normals or reconstruct normals from depth; depth reconstruction has Low/Medium/High quality options.
- Decals are provided through the Decal Renderer Feature and can use Automatic / DBuffer techniques depending on platform/configuration.
- URP supports FXAA, SMAA and TAA. TAA is incompatible with MSAA, Camera Stacking and Dynamic Resolution.
- Reflection Probes support box projection; blending modes can combine neighboring probes and, for outdoor use, the skybox.
- Adaptive Probe Volumes exist in Unity 6 URP and can be inspected through Rendering Debugger.

Official references:
- https://docs.unity3d.com/6000.0/Documentation/Manual/urp/post-processing-ssao.html
- https://docs.unity3d.com/6000.0/Documentation/Manual/urp/ssao-renderer-feature-reference.html
- https://docs.unity3d.com/6000.0/Documentation/Manual/urp/renderer-feature-decal-reference.html
- https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@15.0/manual/anti-aliasing.html
- https://docs.unity3d.com/6000.0/Documentation/ScriptReference/ReflectionProbe-boxProjection.html
- https://docs.unity3d.com/6000.0/Documentation/Manual/urp/probevolumes-showandadjust.html

## Final-look profile strategy

Create two quality profiles:

### MOBILE_BASELINE

Purpose:
- preserve current playable mobile intent;
- conservative renderer-feature cost;
- no visual degradation merely to hit speculative performance targets.

Initially preserve current mobile settings, then compare only proven improvements.

### FINAL_LOOK

Purpose:
- development/owner-facing target image;
- establish the intended production ceiling before optimization.

The FINAL_LOOK profile is the source for artistic convergence. MOBILE_BASELINE is optimized after visual target and device profiling.

## A/B sequence

### F0 — locked baseline

Capture:
- zoom 9 desktop;
- mobile portrait;
- later 12/19 only if core gate passes.

Record:
- renderer asset;
- HDR;
- AA;
- shadows/cascades;
- render scale;
- fog/post-processing;
- reflection/light probe state;
- texture filtering;
- frame/render metrics available in gate.

### F1 — SSAO

Hypothesis:
improve contact, masonry recesses, roof/wall intersections, buttresses and building-ground connection.

First candidate:
- low/moderate intensity;
- radius chosen for architectural macro contact, not dirty halos;
- compare DepthNormals vs Depth only if shader compatibility/performance requires it.

Keep only if:
- geometry reads deeper at zoom 9/mobile;
- no black creases/halo artifact;
- cost is reasonable.

### F2 — Decals

Prerequisite:
architecture and shared materials already read well.

Use decals for:
- base dirt;
- moisture;
- moss;
- soot;
- cracks;
- traffic wear;
- masonry/ground transitions.

Do not use decals to fake missing architectural depth.

Test Automatic first. DBuffer is optional only when its normal/material blending advantage is visible and platform cost is acceptable.

### F3 — anti-aliasing

Run identical captures:
- current;
- SMAA;
- TAA where compatible;
- MSAA candidate where relevant.

Decision criteria:
- roof-edge stability;
- thin timber/railing stability;
- foliage shimmer;
- text/HUD integrity;
- ghosting;
- mobile cost.

TAA must not be adopted if it conflicts with required Dynamic Resolution, Camera Stacking or MSAA setup.

### F4 — shadows

A/B:
- current shadow distance/cascade settings;
- soft-shadow quality candidate;
- cascade distribution tuned to city screen-space.

Goal:
strong form/contact without making the frame muddy.

### F5 — reflection probes

Use baked probes first for static city architecture.

Candidate zones:
- central civic/Bastion core;
- lower functional district;
- water/metal-accent area if visually justified.

Use blending and box projection only where they visibly improve local reflection/parallax behavior.

### F6 — indirect lighting

Compare:
- current baseline;
- baked/mixed lighting where static architecture supports it;
- conventional Light Probes for dynamic characters/props.

Adaptive Probe Volumes are a separate candidate, not default. Test only after the ordinary lighting baseline is stable and profile the cost/benefit.

### F7 — HDR / tonemapping / grading / fog

Goal:
luminous epic presentation with preserved stone/timber/roof separation.

Avoid:
- washed highlights;
- crushed blacks;
- heavy cinematic fog;
- saturation that makes vegetation/cartoon accents dominate.

### F8 — texture sharpness

Audit:
- mipmaps;
- anisotropic filtering;
- max texture size;
- compression;
- render scale;
- runtime LOD bias.

Do not compensate for low texel density by oversharpening the entire frame.

### F9 — render scale / upscaling

Only after target image exists:
- evaluate render-scale cost/clarity;
- evaluate available upscaling path if already supported by the project.

Never trade away architecture readability at zoom 9/mobile to optimize an unmeasured bottleneck.

## Camera A/B

### Camera A — canonical orthographic

Use current:
- pose;
- target;
- city framing;
- HUD;
- lighting;
- progression state.

### Camera B — strategic perspective

Requirements:
- same perceived city occupancy;
- same view direction;
- long focal length / narrow field of view;
- minimal perspective distortion;
- no free orbit;
- no gameplay contract change.

Tune distance/FOV together until the Bastion and city occupy approximately the same screen region as A.

Compare:
- Bastion monumentality;
- depth separation;
- maquette feeling;
- roof/road readability;
- occlusion;
- parcel visibility;
- click/hotspot screen-space;
- bounded pan behavior;
- mobile portrait;
- visual stability at zoom equivalents.

Promotion rule:
orthographic remains canonical unless perspective is a clear full-frame win with no interaction/readability regression.

## Evidence naming

Recommended:
- before-9.png
- after-9.png
- before-mobile.png
- after-mobile.png
- camera-ortho-9.png
- camera-perspective-9.png
- camera-ortho-mobile.png
- camera-perspective-mobile.png
- final-look-evidence.json

## Stop gates

Do not progress to density/life if:
- production architecture is still primitive-looking;
- surfaces remain flat/washed;
- contact with ground is weak;
- Final Look only makes the prototype cleaner.

Do not enable a renderer feature merely because Unity supports it.

## Production decision

Current expected order once production resources are free:

1. high-fidelity starter architecture family;
2. shared material stack;
3. F0/F1/F3/F4/F7 core Final Look;
4. reflection/indirect-light candidates only where justified;
5. camera A/B;
6. player-facing integration;
7. density/life/world exterior;
8. optimization/mobile profiling.
