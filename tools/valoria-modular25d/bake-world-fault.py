"""Pack the canonical fault corridors into a linear surface mask for Unity."""
import json
from pathlib import Path
import numpy as np
from PIL import Image

root = Path(__file__).resolve().parents[2]
art = root / 'Unity/Assets/Eldoria/World4XCanon/Resources/World4X'
layout = json.loads((art / 'Blender/production-region.json').read_text())
# The image top corresponds to high world Z; Unity samples UV (0,0) at bottom.
z, x = np.mgrid[0:256, 0:320]
x = -430 + (x + .5)
z = 288 - (z + .5)
depth = np.zeros(x.shape)
shoulder = np.zeros(x.shape)
vein = np.zeros(x.shape)
for f in layout['faults']:
    dx, dz = f['bx']-f['ax'], f['bz']-f['az']
    t = np.clip(((x-f['ax'])*dx + (z-f['az'])*dz)/(dx*dx + dz*dz), 0, 1)
    d = np.hypot(x-f['ax']-t*dx, z-f['az']-t*dz)
    width = f['radius']*(.34+.05*np.sin(.16*x+.09*z)+.045*np.sin(.32*z))
    depth = np.maximum(depth, np.clip(1-d/width, 0, 1))
    v = np.maximum(0, 1-d/(width*.32))
    vein = np.maximum(vein, v*(.37+.63*(.5+.5*np.sin(x*.31-z*.48))**2))
    shoulder = np.maximum(shoulder, np.clip(1-d/(width*2.15),0,1))
rgb = np.stack((depth, shoulder, vein),axis=-1)
Image.fromarray(np.uint8(rgb*255+.5)).save(art/'CoronaFaultPigment.png',optimize=True)
