"""Stage exact committed inputs without checking out or changing another branch."""
import hashlib, pathlib, subprocess, sys
target=pathlib.Path(sys.argv[1]);target.mkdir(parents=True,exist_ok=True)
source='0abf13870d21725c30acf3f3e3a8c86385470073'
paths={n:f'art-source/valoria/production/asset-family-v2/quality-v3/{n}/{n}.glb' for n in ['wall_straight','watchtower','wall_corner','wall_connections','stone_path_platform','cliff_rocks','defensive_props']}
paths['hero_gate_v3']='art-source/valoria/production/first-hero-gate-v1/quality-v3/ValoriaHeroGate.glb'
expected={'cliff_rocks':'f681c978680f24b68e019cb79912214e64085bb14daceac456b29557910410ed','wall_connections':'88a1d0902cebeed82645b72f50cac6ba7f97e2feb77ea27172c9ca96015b1858','wall_straight':'99ec144ae332c1deb58b57b2d22516dd49a2a87a17723c400f1970c29a52337c','watchtower':'534af063ab7ca81016e01b9feb80d66a398ca2450658216c7d5ca92927b8202d','stone_path_platform':'26f159c66ddb54d890a1eaba1a3626b14aa3d6beb38b759d00ce3cd02de0ddd1','defensive_props':'5da2ae8e1a45ca6df3a0b8a12e15c34ba58201f6aafa8db2f986f89fd726930c','wall_corner':'f104f071fe1cd03da8189a5a6fed909e63186593bcbf77c26886835288e4d73f','hero_gate_v3':'81e63b7269bef3180648b636ad7341a4373e800afd05527e8e8aa95d88d35e2f'}
subprocess.run(['git','fetch','--depth','1','origin',source],check=True)
for name,path in paths.items():
 data=subprocess.check_output(['git','show',source+':'+path]);sha=hashlib.sha256(data).hexdigest()
 if sha!=expected[name]:raise RuntimeError('Frozen source hash mismatch: '+name)
 (target/(name+'.glb')).write_bytes(data);print('FROZEN_SOURCE_OK',name,len(data),flush=True)
extras={'stairs.glb':'art-source/valoria/production/stair-family-v1/StairFamilyV1.glb','bridge.glb':'art-source/valoria/production/bridge-family-v1/BridgeFamilyV1.glb','pine.fbx':'Unity/Assets/Holotna/Mountain/Models/Tree01.fbx','pine-leaf.png':'Unity/Assets/Holotna/Mountain/Textures/Leaf01_ALB.png','pine-trunk.png':'Unity/Assets/Holotna/Mountain/Textures/Trunk01_ALB.png','bush.fbx':'Unity/Assets/EmaceArt/Slavic World Free/Meshes/EA03_Nature_Bush_01a.fbx'}
for name,path in extras.items():
 data=subprocess.check_output(['git','show','39cc8b9ce60ae7b5a765e8ae1de670809a8d6e2f:'+path]);(target/name).write_bytes(data)
 print('EXISTING_SUPPLEMENT',name,hashlib.sha256(data).hexdigest(),flush=True)
