import bpy
# Observation only. The first modelling decision is intentionally not present yet.
bpy.app.driver_namespace['interactive_snapshot']('00-observe')
print('OBSERVATION_COMPLETE; no modelling operations executed')
