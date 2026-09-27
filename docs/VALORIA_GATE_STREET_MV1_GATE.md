# GateStreetRiseRock MV1 multiview — final canonical gate

Date: 2026-09-27.

## Source

The Windows self-hosted runner selected the exact owner export from Downloads:

- `Eldoria_Module_GateStreetRiseRock_MV1.glb`
- 8,186,028 bytes
- SHA-256 `4d1c19978302643003600b0da5ed2f0ef14f7256c85da03077d36fa9f13e294e`
- last write 2026-09-27T21:48:11.6257811+02:00

Known V1/V2/V3 hashes were explicitly excluded.

## Blender

Raw MV1:
- 1 mesh object
- 170,408 vertices
- 341,254 triangles
- 1 material
- UV0 absent
- normals present

Canonical Blender output:
- 24,678 source-side vertices
- **49,799 triangles**
- 1 material
- UV0 present
- normals present
- 2,101,016 bytes
- optimized SHA-256 `9d1a97ea385d557b77779eef7a7b65f8027bfc79a56b26081abc6b62f25a8302`

Accepted triangle range: 49,500–50,000.

## Important orientation correction

The earlier MV1 review run **36345950898** produced a functional FAIL because the review scene assumed the legacy module front. Cardinal diagnostics later proved that the multiview export's functional gate face was on the opposite side.

That earlier FAIL is **superseded** as an orientation false negative, not a geometry failure.

The canonical pipeline was extended with per-module `unity_yaw_degrees`. MV1 was re-run with **180° yaw**; the GLB geometry itself was not changed.

## Final Unity gate

Canonical run: **36348029333**  
Artifact: **10941805252**

Unity 6000.3.23f1 measured:
- 1 mesh / 1 renderer / 1 material
- 0 textures
- 56,282 imported vertices
- **49,799 triangles**
- UV0 present
- normals present
- 1 MeshCollider
- mesh runtime memory 4,798,784 bytes
- positive raycast PASS
- empty-space miss PASS
- all 8 captures non-empty: 19 / 12 / 9 / oblique / front / rear / left / right

**Technical gate: PASS.**

## Functional visual review

Required chain:

`lower entry → open arch → visible ascent → upper landing`

### 19 strategic
The lower approach now aligns with the gate face. The module reads as an elevated fortified access rather than a closed rock pedestal.

### 12 city
The road visibly feeds into the open arch and the rising interior route remains readable as part of the same circulation chain.

### 9 detail
The lower path, arch opening and ascending surface are simultaneously visible. The route survives Blender reduction and Unity import.

### Oblique
Depth is genuine rather than façade-only. The gate opens into a climbing route that continues toward the higher fortified platform.

### Front diagnostic
This is the decisive evidence. The lower approach enters the open arch directly and the stair/rise remains exposed behind it. Unlike V1/V2/V3, the route does not terminate against a front rock mass.

## Final verdict

**GateStreetRiseRock MV1 multiview: PASS technical / PASS functional visual.**

Certified as the third distinct functional Valoria module family:

1. TowerWallRock — defensive tower/wall/rock mass.
2. TerraceStairRock — terrace/elevation/stair/rock.
3. GateStreetRiseRock MV1 — lower entry/open arch/visible ascent/upper landing.

This does not approve final textures/material finish or mobile performance; the source has no textures and the gate uses diagnostic clay.

The next composition milestone may build a new isolated three-family Micro-Valoria. `Valoria.unity`, `VisualWorld` and gameplay remain untouched.
