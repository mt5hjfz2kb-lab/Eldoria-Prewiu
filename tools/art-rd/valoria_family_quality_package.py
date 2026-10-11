"""Package only the independently reviewed, genuinely authored asset family."""
import hashlib, json, sys, zipfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / 'art-source/valoria/production/asset-family-v2'
BASE = SOURCE / 'quality-v3'
ASSETS = ['wall_straight', 'watchtower', 'wall_corner', 'wall_connections',
          'stone_path_platform', 'cliff_rocks', 'defensive_props']
directions = json.loads((SOURCE / 'qwen-family-quality-directions.json').read_text())
traces = json.loads((SOURCE / 'qwen-family-quality-traces.json').read_text())
assert not directions.get('TEST_FIXTURE_NON_AUTHORITATIVE')
assert set(directions['plans']) == set(traces) == set(ASSETS)
assert all(any(attempt.get('valid') for attempt in traces[a]['attempts']) for a in ASSETS)
review = json.loads((BASE / 'visual-review.json').read_text())
assert review['family_source_art_pass'] and set(review['assets']) == set(ASSETS)
audit = json.loads((BASE / 'glb-preflight.json').read_text())
assert audit['structural_pass'] and len(audit['assets']) == 21
assert all(not a['errors'] and not a['warnings'] for a in audit['assets'])
roundtrip = json.loads((BASE / 'roundtrip-evidence.json').read_text())
assert len(roundtrip['exports']) == 14
for asset in ASSETS:
    evidence = json.loads((BASE / asset / 'evidence.json').read_text())
    assert evidence['authoring_evidence'] == 'GENUINE_EXTERNAL_QWEN_PLAN_BEFORE_BLENDER_MCP'
    assert review['assets'][asset]['source_art_pass']
    for key, suffix in [('blend_sha256', '.blend'), ('glb_sha256', '.glb')]:
        path = BASE / asset / (asset + suffix)
        assert hashlib.sha256(path.read_bytes()).hexdigest() == review['assets'][asset][key]

destination = Path(sys.argv[1]).resolve()
destination.mkdir(parents=True, exist_ok=True)
readme = destination / 'Valoria-Familia-V3-LEEME.md'
lines = ['# Familia modular de Valoria — calidad mínima Puerta V3', '',
         'Siete assets con fuentes editables de Blender y GLB autosuficientes en tres niveles de detalle.', '',
         '| Asset | Uso | Triángulos LOD0 | LOD1 | LOD2 |',
         '| --- | --- | ---: | ---: | ---: |']
uses = ['Muro recto de 8 m', 'Torre completa de la puerta V3', 'Esquina de 90°',
        'Unión de muro de 2 m', 'Camino continuo de 4 × 8 m',
        'Acantilado modular de 12 m', 'Estandartes, braseros y defensa de madera']
for asset, use in zip(ASSETS, uses):
    evidence = json.loads((BASE / asset / 'evidence.json').read_text())
    lines.append(f"| {asset} | {use} | {evidence['triangles']} | {evidence['lods'][0]['triangles']} | {evidence['lods'][1]['triangles']} |")
lines += ['', '## Uso', '',
          'Blender usa Z vertical y unidades en metros. GLB convierte al sistema glTF estándar con Y vertical. No deformar módulos para encajarlos: usar traslación y rotación. Los muros mantienen la cota común de adarve; el camino se apoya a nivel del terreno y el acantilado baja desde su coronación.', '',
          'Los .blend conservan las piedras y detalles editables. Los GLB agrupan geometría por material equivalente y llevan las texturas integradas. Los emblemas, paños, metal fino y llamas están protegidos frente a la simplificación de LOD. Los umbrales de cambio de LOD deben establecerse por tamaño en pantalla dentro del motor.', '',
          'La escena family-assembly.blend demuestra encuentros y repetición mediante reimportación de los GLB exactos. Su suelo de apoyo es contexto de comprobación. La puerta V3 queda incluida en la escena como referencia congelada; no es un nuevo miembro de esta familia.', '',
          '## Autoría y comprobación', '',
          'Qwen aporta diagnósticos, operaciones y proporciones antes de construir en BlenderMCP. Se incluyen sus respuestas originales y los hashes de las imágenes que vio. La revisión independiente compara las piezas con la puerta V3 y con la imagen exacta adjunta por el propietario. Las pruebas provisionales no forman parte de esta entrega.', '',
          'Se han revisado las siete piezas, catorce reimportaciones visuales y veintiún GLB sin errores ni advertencias estructurales. La aprobación es de esta familia de fuentes. La imagen objetivo 9,5/10 corresponde al mundo completo: terreno, vegetación, agua, luces y composición deberán construirse en la fase de integración. Esta entrega no certifica todavía rendimiento en Unity ni en un dispositivo físico.', '',
          '## Procedencia', '',
          'Piedra, heráldica y braseros: fuentes existentes de Eldoria / Puerta V3, preservadas. Las texturas adicionales de tierra y roca se generan para esta familia. El relieve geológico reutiliza el escaneo CC0 de https://opengameart.org/content/free-photogrammetric-rock; el archivo donante y su SHA-256 constan en la evidencia del acantilado.', '',
          'Los scripts y el workflow conservados en la rama artística documentan la reconstrucción. Su reproducción requiere el repositorio de Eldoria y los artefactos de partida indicados en pipeline/valoria-family-quality-request.json.', '']
readme.write_text('\n'.join(lines))

archive = destination / 'Valoria-Familia-V3.zip'
files = [p for p in BASE.rglob('*') if p.is_file() and not p.name.startswith('.')
         and p.suffix in {'.blend', '.glb', '.png', '.json'}]
files += list((SOURCE / 'quality-textures').glob('*.png'))
files += list(SOURCE.glob('*-artist-input-*.jpg'))
files += [SOURCE / 'qwen-family-quality-directions.json', SOURCE / 'qwen-family-quality-traces.json']
with zipfile.ZipFile(archive, 'w', zipfile.ZIP_DEFLATED, compresslevel=6) as z:
    for path in files:
        z.write(path, str(path.relative_to(SOURCE)))
    z.write(readme, readme.name)
    z.write(ROOT / 'docs/evidence/valoria-asset-family-v2/QUALITY_TAKEOVER.md', 'AUTHORING/QUALITY_TAKEOVER.md')
    for path in (ROOT / 'tools/art-rd').glob('valoria_family_quality_*.py'):
        z.write(path, 'AUTHORING/' + path.name)
print(json.dumps({'archive': str(archive), 'bytes': archive.stat().st_size,
                  'sha256': hashlib.sha256(archive.read_bytes()).hexdigest(),
                  'readme': str(readme), 'files': len(files)}))
