"""Create original, tileable Valoria texture studies using Blender's bundled numpy.

Run from Blender in background mode. There are no third-party model downloads.
"""

import os
import bpy
import numpy as np

OUTPUT = os.environ.get("ELDORIA_HERO_OUTPUT", os.path.abspath("Unity/Assets/Eldoria/ArtTests/OriginalHero"))
TEXTURES = os.path.join(OUTPUT, "Textures")


def field(size, seed, octaves=((7, .55), (29, .27), (113, .12), (263, .06))):
    rng = np.random.default_rng(seed)
    result = np.zeros((size, size), dtype=np.float32)
    axis = np.arange(size)
    for count, weight in octaves:
        grid = rng.uniform(-1, 1, (count, count)).astype(np.float32)
        gx = axis * count / size
        ix = gx.astype(np.int32) % count
        fx = gx - np.floor(gx)
        fx = fx * fx * (3 - 2 * fx)
        a = grid[ix[:, None], ix[None, :]]
        b = grid[(ix + 1)[:, None] % count, ix[None, :]]
        c = grid[ix[:, None], (ix + 1)[None, :] % count]
        d = grid[(ix + 1)[:, None] % count, (ix + 1)[None, :] % count]
        result += weight * (a * (1 - fx[:, None]) * (1 - fx[None, :]) +
                            b * fx[:, None] * (1 - fx[None, :]) +
                            c * (1 - fx[:, None]) * fx[None, :] + d * fx[:, None] * fx[None, :])
    return result


def save(name, rgb):
    os.makedirs(TEXTURES, exist_ok=True)
    height, width = rgb.shape[:2]
    img = bpy.data.images.new(name, width=width, height=height, alpha=True)
    pixels = np.empty((height, width, 4), dtype=np.float32)
    pixels[:, :, :3] = np.clip(rgb, 0, 1)
    pixels[:, :, 3] = 1
    img.pixels.foreach_set(pixels.ravel())
    img.filepath_raw = os.path.join(TEXTURES, name + ".png")
    img.file_format = "PNG"
    img.save()
    return img


def make_textures(n=512):
    y, x = np.mgrid[:n, :n]
    soft = field(n, 200)
    fine = field(n, 202, ((127, .6), (263, .4)))
    brown = field(n, 208, ((5, .8), (17, .2)))
    base = np.array((.63, .60, .53))[None, None, :]
    limestone = base + soft[:, :, None] * np.array((.13, .11, .09)) + fine[:, :, None] * .035
    limestone += brown[:, :, None] * np.array((.03, .018, -.008))
    oldstone = np.array((.50, .51, .47))[None, None, :] + soft[:, :, None] * .13 + fine[:, :, None] * .047
    oldstone += np.maximum(0, field(n, 113))[:, :, None] * np.array((-.09, -.025, -.065))

    # Staggered, scale-varying slate courses, with hairline mortar, stained edges
    # and directional grain. Geometry will add the true overlapping eaves.
    rng = np.random.default_rng(904)
    course = np.floor(y / 44).astype(np.int32)
    offset = (course % 2) * 18
    tile = np.floor(((x + offset) % n) / 36).astype(np.int32)
    palette = rng.uniform(-.055, .06, (n // 44 + 2, n // 36 + 2))
    slate_var = palette[np.minimum(course, len(palette)-1), np.minimum(tile, palette.shape[1]-1)]
    seams = ((x + offset) % 36 < 2) | (y % 44 < 2)
    slate = np.array((.155, .191, .237))[None, None, :] + slate_var[:, :, None] + soft[:, :, None] * .063
    slate += np.sin(x * .29 + y * .08)[:, :, None] * .009
    slate[seams] *= .52

    # Longitudinal tool marks and subtle knots on dark structural oak.
    timber_grain = np.sin(x * .18 + np.sin(y * .013) * 2 + fine * 3) * .025
    timber = np.array((.27, .18, .105))[None, None, :] + brown[:, :, None] * np.array((.09, .058, .035))
    timber += timber_grain[:, :, None] + fine[:, :, None] * .018
    wood_planks = (x % 84 < 3)
    timber[wood_planks] *= .70

    # Coarse stratified limestone for the rock terrace, not painted stone bricks.
    strata = np.sin(y * .047 + soft * 8) * .045
    rock = np.array((.42, .39, .34))[None, None, :] + soft[:, :, None] * .14 + fine[:, :, None] * .055
    rock += strata[:, :, None]
    rock += np.maximum(0, field(n, 303))[:, :, None] * np.array((-.035, .002, -.015))

    return {"limestone": save("limestone", limestone),
            "oldstone": save("oldstone", oldstone),
            "slate": save("slate", slate),
            "oak": save("oak", timber),
            "rock": save("rock", rock)}


if __name__ == "__main__":
    make_textures()
