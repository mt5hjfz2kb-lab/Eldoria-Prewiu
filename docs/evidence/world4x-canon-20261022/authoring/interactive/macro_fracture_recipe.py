"""Authored geological hierarchy after inspecting real Unity and MCP baseline.
Coordinates are deliberate branching strokes, not a noise-generated terrain.
The original AF/AG rectangle and its topology/materials/UV remain protected.
"""
import numpy as np
ORIGINAL_FAULTS=[[(-385,260),(-345,218),(-308,206),(-280,165),(-260,150),(-222,137),(-195,86),(-135,40)],[(-308,206),(-350,140),(-365,110)],[(-222,137),(-175,187),(-150,230)]]
AUTHOR_FAULTS=ORIGINAL_FAULTS+[
 [(-385,260),(-430,292),(-470,280),(-510,322),(-560,335),(-590,382)],
 [(-308,206),(-255,270),(-240,325),(-260,385),(-240,445),(-275,540)],
 [(-150,230),(-125,280),(-155,340),(-125,385)],
 [(-260,150),(-335,102),(-400,125),(-460,85),(-530,120)],
]
def stroke_distance(x,y,paths):
 d=np.full(np.shape(x),1e6,dtype=np.float64)
 for path in paths:
  for (ax,ay),(bx,by) in zip(path,path[1:]):
   t=np.clip(((x-ax)*(bx-ax)+(y-ay)*(by-ay))/((bx-ax)**2+(by-ay)**2),0,1)
   d=np.minimum(d,np.hypot(x-ax-t*(bx-ax),y-ay-t*(by-ay)))
 return d

def geological_paint(rgb,x,y):
 """Bounded material relationship: pale broken shoulders, dark mineral floor."""
 d=stroke_distance(x,y,AUTHOR_FAULTS)
 core_clear=np.clip((-102-x)/23,0,1)
 apron=np.clip((35-d)/12,0,1);apron=apron*apron*(3-2*apron)*.80*core_clear
 # Broken mineral plates derived from real incised ground; no floating rocks.
 first=np.full(x.shape,np.inf);second=first.copy();cell=np.zeros(x.shape,np.int32)
 rng=np.random.default_rng(241017)
 seeds=[(sx+rng.uniform(-8,8),sy+rng.uniform(-8,8)) for sy in range(-20,651,27) for sx in range(-700,-70,27)]
 for i,(sx,sy) in enumerate(seeds):
  q=np.hypot(x-sx,y-sy);near=q<first
  second=np.where(near,first,np.minimum(second,q));cell=np.where(near,i,cell);first=np.minimum(first,q)
 joint=np.clip((1.3-(second-first))/1.3,0,1)
 shoulder=np.array([(.49,.45,.43),(.44,.40,.41),(.55,.49,.44),(.40,.36,.40)])[cell%4]
 shoulder*=1-joint[...,None]*.38
 rgb[:]=rgb*(1-apron[...,None])+shoulder*apron[...,None]
 floor=np.clip((11-d)/7,0,1)*.55*core_clear
 rgb[:]=rgb*(1-floor[...,None])+np.array((.25,.19,.29))*floor[...,None]
 heart=np.clip(1-((x+260)/65)**2-((y-150)/75)**2,0,1)*.50*core_clear
 rgb[:]=rgb*(1-heart[...,None])+np.array((.29,.12,.36))*heart[...,None]
 return apron
