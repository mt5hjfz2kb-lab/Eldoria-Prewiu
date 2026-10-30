# Pending source-workshop experiment, NOT applied to production.
# Art diagnosis: projected fir crowns are squat at the real 62 degree view.
# Keep every tree anchor, species topology, UV and material. Do not add objects.
import bpy,math,json,pathlib

def apply_canopy_proportions():
 changes=[]
 for tree in bpy.context.scene.objects:
  if not tree.name.startswith(('BLD_ForestMass_','BLD_ForestFar_')):continue
  if tree.hide_get():continue
  x,y=tree.location.x,tree.location.y
  # Preserve exact retained AF/AG representation; gradual change outside its edge.
  distance=math.hypot(max(0,abs(x)-96),max(0,-78-y,y-270))
  weight=max(0,min(1,(distance-35)/180));weight=weight*weight*(3-2*weight)
  if weight==0:continue
  before=list(tree.scale)
  tree.scale.x*=1-.15*weight;tree.scale.y*=1-.15*weight;tree.scale.z*=1+.65*weight
  changes.append({'name':tree.name,'anchor':list(tree.location),'before':before,'after':list(tree.scale)})
 bpy.context.view_layer.update()
 return changes
