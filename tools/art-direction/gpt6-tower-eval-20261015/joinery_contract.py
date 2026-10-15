"""Declarative timber workshop skill validation: artist owns every dimension.

No geometry generation here; Blender consumes this verified, bounded plan.
A tenon and mortise must have explicit, physically compatible WORLD-metre sizes.
"""
from __future__ import annotations
import math

class JoineryRejected(ValueError): pass

def finite3(value,label):
    if not isinstance(value,list) or len(value)!=3 or any(type(x) not in (int,float) or not math.isfinite(x) for x in value):
        raise JoineryRejected("INVALID_VECTOR_"+label)
    return [float(x) for x in value]

def validate(spec):
    if spec.get("schema_version") != 1 or spec.get("art_approved") is not False:
        raise JoineryRejected("BAD_SCHEMA_OR_APPROVAL")
    members=spec.get("members")
    if not isinstance(members,list) or not (2<=len(members)<=12):
        raise JoineryRejected("MEMBER_COUNT")
    seen=set()
    for m in members:
        name=m.get("id")
        if not isinstance(name,str) or not name.startswith("Eldoria_") or name in seen:
            raise JoineryRejected("MEMBER_ID")
        seen.add(name)
        finite3(m.get("center_m"),"CENTER")
        dim=finite3(m.get("size_m"),"SIZE")
        if not all(.025<=x<=6 for x in dim):
            raise JoineryRejected("MEMBER_SIZE_OUT_OF_BOUNDS")
        bevel=m.get("edge_bevel_m")
        if type(bevel) not in (float,int) or not .001<=bevel<=min(dim)/4:
            raise JoineryRejected("BEVEL_OUT_OF_BOUNDS")
    joints=spec.get("joints")
    if not isinstance(joints,list) or not joints or len(joints)>16:
        raise JoineryRejected("JOINT_COUNT")
    for j in joints:
        if j.get("kind")!="mortise_tenon" or j.get("tenon_member") not in seen or j.get("socket_member") not in seen:
            raise JoineryRejected("UNSUPPORTED_OR_UNKNOWN_JOINT")
        if j["tenon_member"]==j["socket_member"]:
            raise JoineryRejected("SELF_JOINT")
        t=finite3(j.get("tenon_size_m"),"TENON")
        s=finite3(j.get("socket_size_m"),"SOCKET")
        center=finite3(j.get("center_m"),"JOINT_CENTER")
        if any(not (.015<=t[k] < s[k] <=.30) for k in range(3)):
            raise JoineryRejected("TENON_SOCKET_SIZE_MISMATCH")
        if any(not (.002<=s[k]-t[k]<=.015) for k in range(3)):
            raise JoineryRejected("IMPOSSIBLE_FIT_TOLERANCE")
        if j.get("axis")!="Z":
            raise JoineryRejected("JOINT_AXIS_NOT_IMPLEMENTED")
        a=next(m for m in members if m["id"]==j["tenon_member"])
        ztop=a["center_m"][2]+a["size_m"][2]/2
        zbottom=center[2]-t[2]/2
        if abs(ztop-zbottom)>.008:
            raise JoineryRejected("TENON_NOT_ANCHORED_TO_POST_TOP")
        target=next(m for m in members if m["id"]==j["socket_member"])
        lo=target["center_m"][2]-target["size_m"][2]/2
        hi=target["center_m"][2]+target["size_m"][2]/2
        if center[2]+s[2]/2>=hi-.005 or center[2]-s[2]/2 > lo+.015:
            raise JoineryRejected("SOCKET_BREAKS_LINTEL")
    color=spec.get("wood_color_rgba")
    if not isinstance(color,list) or len(color)!=4 or any(type(v) not in (int,float) or not .0<=v<=1. for v in color):
        raise JoineryRejected("BAD_WOOD_COLOR")
    return {"status":"PLAN_VALID","member_count":len(members),"joint_count":len(joints),
            "artist_controls_dimensions":True,"art_approved":False}
