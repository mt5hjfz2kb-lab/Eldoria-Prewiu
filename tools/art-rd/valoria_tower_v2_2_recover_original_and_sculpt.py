"""Quality rejection repair: reuse the genuine V1 coherent tower masonry and apply fresh real Qwen-directed crown edit via proven BlenderMCP, NOT the warped V2 tower."""
import bpy,json
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
STUDIO=ROOT/'tools/art-rd/valoria_family_v2_1_mcp_author.py'
# Reuse existing BlenderMCP transport, art operation logic and material-preserving exporters.
src=STUDIO.read_text().split('for asset,plan in PLANS.items():')[0]
ns={'__file__':str(STUDIO),'__name__':'eldoria_tower_restoration'}
exec(compile(src,str(STUDIO),'exec'),ns)
old=ROOT/'art-source/valoria/production/first-asset-family-v1/watchtower/watchtower.blend'
assert old.exists(),'Actual original watchtower Blender art is necessary; never approximate or regenerate'
bpy.ops.wm.open_mainfile(filepath=str(old))
plan=ns['PLANS']['watchtower']
assert plan['operation']=='reshape_crown_silhouette',plan
report={}
def operation():
 report.update(ns['author']('watchtower',plan))
ns['execute'](operation)
dest=ns['OUT']/'watchtower';dest.mkdir(parents=True,exist_ok=True)
ns['render'](dest)
ns['export']('watchtower',dest)
for suffix in ('hero.png','strategic.png'):
 (dest/('v2-1-'+suffix)).rename(dest/('v2-2-'+suffix))
(dest/'watchtower-v2-1.blend').rename(dest/'watchtower-v2-2.blend')
(dest/'watchtower-v2-1.glb').rename(dest/'watchtower-v2-2.glb')
report['restored_source']='first-asset-family-v1/watchtower/watchtower.blend'
report['rejected_old_tower']='asset-family-v2/watchtower/watchtower-v2-1.blend'
report['recovery_strategy']='exact original masonry + new Qwen crown-only reshape through BlenderMCP'
report['art_pass']=False
(dest/'watchtower-v2-2-evidence.json').write_text(json.dumps(report,indent=2))
print('ELDORIA_TOWER_RECOVERY_TRUE_ORIGINAL_REAL_MCP_SUCCESS')
