"""Typed anchored timber assembly: artist controls composition, tool fits joints.

This is a carpentry skill, not a tower template. Post count/spacing, dimensions,
height, surface, offset and finish are authored. No artist coordinates are fixed
or repaired. Mechanical joint tolerances are derived and logged explicitly.
"""
import math
from contract import RejectedPlan

def schema(face_ids):
    num=lambda lo,hi:{'type':'number','minimum':lo,'maximum':hi}
    props={'frame_skill_version':{'type':'integer','const':1},
      'asset_id':{'type':'string'},'source_object':{'type':'string'},
      'anchor_face':{'type':'integer','enum':face_ids},
      'design_intent':{'type':'string','minLength':35},
      'width_m':num(.6,2.5),'height_m':num(.6,2.3),'bottom_z_m':num(0,1.2),
      'post_offsets_m':{'type':'array','minItems':2,'maxItems':5,'items':num(-1.2,1.2)},
      'post_width_m':num(.06,.22),'depth_m':num(.08,.22),
      'lintel_height_m':num(.09,.24),'surface_offset_m':num(-.02,.05),
      'bevel_m':num(.002,.012),
      'wood_color_rgba':{'type':'array','minItems':4,'maxItems':4,'items':num(0,1)},
      'art_approved':{'type':'boolean','const':False}}
    return {'type':'object','properties':props,'required':list(props),'additionalProperties':False}

def validate(s):
    def need(ok,why):
        if not ok:raise RejectedPlan(why)
    need(s.get('frame_skill_version')==1 and s.get('art_approved') is False,'FRAME_SCHEMA_OR_APPROVAL')
    need(isinstance(s.get('source_object'),str) and len(s['source_object'])>2,'FRAME_SOURCE_REQUIRED')
    need(type(s.get('anchor_face')) is int and s['anchor_face']>=0,'REAL_FACE_REQUIRED')
    need(isinstance(s.get('design_intent'),str) and len(s['design_intent'])>=35,'FRAME_ART_INTENT_REQUIRED')
    for k,lo,hi in [('width_m',.6,2.5),('height_m',.6,2.3),('bottom_z_m',0,1.2),
       ('post_width_m',.06,.22),('depth_m',.08,.22),('lintel_height_m',.09,.24),
       ('surface_offset_m',-.02,.05),('bevel_m',.002,.012)]:
        v=s.get(k);need(type(v) in (int,float) and math.isfinite(v) and lo<=v<=hi,'FRAME_RANGE_'+k)
    offsets=s.get('post_offsets_m')
    need(isinstance(offsets,list) and 2<=len(offsets)<=5,'FRAME_POST_COUNT')
    need(all(type(v) in (int,float) and math.isfinite(v) and abs(v)+s['post_width_m']/2<=s['width_m']/2-.005 for v in offsets),'POST_OUTSIDE_ARTIST_LINTEL')
    ordered=sorted(offsets)
    need(all(b-a>s['post_width_m']*1.3 for a,b in zip(ordered,ordered[1:])),'POSTS_OVERLAP')
    need(s['height_m']-s['lintel_height_m']>=.45,'FRAME_POST_HEIGHT')
    need(s['bevel_m']<=min(s['depth_m'],s['post_width_m'],s['lintel_height_m'])/4,'FRAME_BEVEL')
    color=s.get('wood_color_rgba')
    need(isinstance(color,list) and len(color)==4 and all(type(v) in (int,float) and math.isfinite(v) and 0<=v<=1 for v in color),'FRAME_COLOR')
    return True

def compile_joinery(s,point,normal):
    validate(s)
    axis=max((0,1),key=lambda k:abs(normal[k]))
    if abs(normal[2])>.2 or abs(normal[axis])<.94:
        raise RejectedPlan('FRAME_REQUIRES_VERIFIED_VERTICAL_WALL_FACE_NOT_ROOF')
    tangent=1-axis;sign=1 if normal[axis]>0 else -1
    plane=point[axis]+sign*(s['depth_m']/2-.01+s['surface_offset_m'])
    top=s['bottom_z_m']+s['height_m'];post_top=top-s['lintel_height_m']
    center=list(point);center[axis]=plane;center[2]=top-s['lintel_height_m']/2
    lintel_size=[s['depth_m']]*2+[s['lintel_height_m']];lintel_size[tangent]=s['width_m']
    lid='Eldoria_Frame_Lintel'
    members=[{'id':lid,'center_m':center,'size_m':lintel_size,'edge_bevel_m':s['bevel_m']}]
    joints=[]
    for i,offset in enumerate(s['post_offsets_m']):
        name='Eldoria_Frame_Post_'+str(i+1)
        c=list(center);c[tangent]=point[tangent]+offset;c[2]=(s['bottom_z_m']+post_top)/2
        dims=[s['depth_m']]*2+[post_top-s['bottom_z_m']];dims[tangent]=s['post_width_m']
        members.append({'id':name,'center_m':c,'size_m':dims,'edge_bevel_m':s['bevel_m']})
        # Deterministic manufacturing fit; never shifts artist member geometry.
        h=min(.08,s['lintel_height_m']*.55)
        jc=list(c);jc[2]=post_top+h/2
        tenon=[min(.12,dims[0]*.52),min(.12,dims[1]*.52),h]
        socket=[v+.006 for v in tenon]
        joints.append({'kind':'mortise_tenon','axis':'Z','tenon_member':name,
          'socket_member':lid,'center_m':jc,'tenon_size_m':tenon,'socket_size_m':socket})
    return {'schema_version':1,'asset_id':s.get('asset_id','anchored_timber_frame'),
      'art_approved':False,'source':s['source_object'],'camera':'shared frozen review',
      'design_intent':s['design_intent'],'wood_color_rgba':s['wood_color_rgba'],
      'members':members,'joints':joints}
