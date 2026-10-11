"""Deterministic, original portable PBR micro-surfaces for the existing hero gate."""
from pathlib import Path
import numpy as np
from PIL import Image, ImageFilter

OUT = Path(__file__).resolve().parents[3] / 'art-source/valoria/production/first-hero-gate-v1/quality-v2/textures'
OUT.mkdir(parents=True, exist_ok=True)
N = 1024
rng = np.random.default_rng(11011)

def noise(radius):
    a = rng.integers(0, 256, (N, N), dtype=np.uint8)
    b = np.asarray(Image.fromarray(a).filter(ImageFilter.GaussianBlur(radius)), dtype=np.float32)
    return np.clip((b - b.mean()) / (b.std() + 1e-5), -2.5, 2.5)

def save(name, array):
    Image.fromarray(np.uint8(np.clip(array, 0, 255))).save(OUT / name)

micro, grain, strata, blotch = noise(.7), noise(4), noise(19), noise(65)
y, x = np.mgrid[:N, :N]
lamina = np.sin(y * .057 + 2.5 * strata + 1.4 * blotch) * .5
pits = np.maximum(0, -grain - .60) ** 1.4
ironstain = np.maximum(0, blotch + .25) ** 1.5
mineral = 4 * micro + 4 * grain + 6 * strata + 7 * blotch + 1.2 * lamina - 5 * pits
stone = np.stack((150 + mineral + .9 * ironstain,
                  146 + mineral,
                  136 + mineral - .6 * ironstain), axis=-1)
save('stone_basecolor.png', stone)
save('stone_roughness.png', 207 + 6 * micro + 7 * grain + 7 * blotch + 7 * pits)
height = .8 * micro + 1.7 * grain + 2.4 * strata + 1.0 * lamina - 3 * pits
dy, dx = np.gradient(height)
save('stone_normal.png', np.stack((128 - 5 * dx, 128 - 5 * dy, np.full((N, N), 250)), axis=-1))

fiber, weather, fine = noise(2), noise(50), noise(.6)
grainline = np.sin(x * .20 + 4 * weather + 1.5 * fiber) + .35 * np.sin(x * .53 + fiber)
checks = np.maximum(0, -fiber - 1.1)
oak = np.stack((87 + 12 * grainline + 9 * weather - 12 * checks,
                62 + 8 * grainline + 7 * weather - 10 * checks,
                38 + 5 * grainline + 5 * weather - 7 * checks), axis=-1)
save('oak_basecolor.png', oak)
save('oak_roughness.png', 193 + 14 * weather + 14 * checks + 5 * fine)
dy, dx = np.gradient(grainline * 4 + fiber)
save('oak_normal.png', np.stack((128 - 13 * dx, 128 - 6 * dy, np.full((N, N), 252)), axis=-1))

for name in ('stone_basecolor.png', 'stone_normal.png', 'stone_roughness.png',
             'oak_basecolor.png', 'oak_normal.png', 'oak_roughness.png'):
    print(name, (OUT / name).stat().st_size)
