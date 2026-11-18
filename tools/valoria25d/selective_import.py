#!/usr/bin/env python3
"""Copy only actual Unity Premium V6 scene dependency assets into integration branch.

Source read-only pinned 10dbc... art branch; destination a new branch based main.
No art workflow, QA historical assets, legacy demos, or economy code are copied.
"""
from pathlib import Path
import re, json, shutil, hashlib

root=Path(__file__).resolve().parents[2]
src=root/'art-source/Unity/Assets/Experimentos/Ciudad2D'
dst=root/'Unity/Assets/Experimentos/Ciudad2D'
source_sha='10dbc197d48ea8517c6a3dd6c22f1a2abf75d508'
scene_name='Experimento_Ciudad2D_PremiumV6_44133747143.unity'
src_scene=src/'QA/ArchivedScenes'/scene_name
scene=src_scene.read_bytes()
actual_hash=hashlib.sha256(scene).hexdigest()
assert actual_hash=='aa566bf6a075868ad852ac865aa8a8cb867d1ade601ac3dd74e8e091f44f0b06', actual_hash
scene_path=dst/'Scenes/Valoria_2D_Playable.unity'
assert not scene_path.exists()
scene_path.parent.mkdir(parents=True)
shutil.copy2(src_scene,scene_path)
shutil.copy2(src_scene.with_suffix('.unity.meta'),scene_path.with_suffix('.unity.meta'))

for base in ['Unity/Assets/Experimentos.meta','Unity/Assets/Experimentos/Ciudad2D.meta']:
    file=root/'art-source'/base
    if file.is_file():target=root/base;target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(file,target)

# Stable Unity GUID per source script; keep only ten runtime components used by the scene.
runtime=['City2DAmbientGating','City2DAmbientMotion','City2DCameraFit',
         'City2DFunctionalDetailGate','City2DPilotHud','City2DPremiumShadowGate',
         'City2DProgressionPolicy','City2DStateBridge','City2DTorchPulse','CityBuilding']
scripts=dst/'Scripts'
scripts.mkdir(parents=True,exist_ok=True)
shutil.copy2(src/'Scripts.meta',dst/'Scripts.meta')
for n in runtime:
    for suffix in ['.cs','.cs.meta']:
        from_path=src/'Scripts'/(n+suffix)
        assert from_path.is_file(), from_path
        shutil.copy2(from_path,scripts/(n+suffix))

referenced={x.decode('ascii') for x in re.findall(rb'guid: ([a-f0-9]{32})',scene)}
found={}
for meta in (src/'Sprites').rglob('*.png.meta'):
    txt=meta.read_text(errors='replace')
    match=re.search(r'(?m)^guid: ([a-f0-9]{32})\s*$',txt)
    if match:
        guid=match.group(1)
        if guid in referenced:
            assert guid not in found, ('Unity GUID collision',guid)
            png=meta.with_suffix('')
            assert png.exists(),png
            relative=meta.relative_to(src)
            out=dst/relative
            out.parent.mkdir(parents=True,exist_ok=True)
            shutil.copy2(meta,out)
            shutil.copy2(png,out.with_suffix(''))
            found[guid]=str(relative.with_suffix(''))
assert len(found)>25, ('Insufficient sprites copied',len(found))
for d in sorted({p.parent for p in (dst/'Sprites').rglob('*.png')}):
    rel=d.relative_to(dst)
    foldermeta=src/rel.parent/(rel.name+'.meta')
    if foldermeta.is_file():
        target=dst/rel.parent/(rel.name+'.meta')
        target.parent.mkdir(parents=True,exist_ok=True)
        shutil.copy2(foldermeta,target)

# Unity source script MonoBehaviours are GUID-linked too; verify complete.
script_guids=set()
for n in runtime:
    c=(scripts/(n+'.cs.meta')).read_text()
    m=re.search(r'(?m)^guid: ([a-f0-9]{32})',c)
    assert m, n
    script_guids.add(m.group(1))
assert len(referenced&script_guids)>=7, ('Scene references missing scripts',referenced&script_guids)
meta_guids=referenced-set(found)-script_guids
manifest={
    'source_branch':'art/ciudad-2d-hitos-1-5-20261116',
    'source_commit_sha':source_sha,
    'main_parent_sha':'904ff183ffb9053f68b9c0f359b2e9d3d40ce805',
    'archived_scene_sha256':actual_hash,
    'new_scene':'Assets/Experimentos/Ciudad2D/Scenes/Valoria_2D_Playable.unity',
    'source_png_guids_referenced_and_copied':len(found),
    'source_runtime_components':runtime,
    'pngs':sorted(found.values()),
    'other_guids_referenced_builtin_or_unmapped':sorted(meta_guids),
    'integration_mode':'selective_scene_and_runtime_dependencies_only',
    'art_source_unchanged':True,'main_unchanged':True,
    'qa_artifacts_copied':0,
    'gameplay_rules_copied':0
}
mfile=root/'tools/valoria25d/SELECTIVE_IMPORT_MANIFEST.json'
mfile.parent.mkdir(parents=True,exist_ok=True)
mfile.write_text(json.dumps(manifest,indent=2,ensure_ascii=False)+'\n')
print('SELECTIVE_PREMIUM_V6_COPY_PASS',len(found),'sprite images,',len(runtime),'runtime components',
      'other GUID count',len(meta_guids))
print('COPIED_SPRITES',json.dumps(sorted(found.values()),ensure_ascii=False))
