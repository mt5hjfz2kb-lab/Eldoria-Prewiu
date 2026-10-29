# Archivo artístico unificado — protocolo de conservación

Este documento amplía la biblioteca existente en `pipeline/asset-vault/`, sin reemplazarla. Fase documental no invasiva: ninguna escena, modelo, build o generación Tripo se modifica.

## Objetivo
Catalogar imágenes 2D, referencias aprobadas, retratos, interfaces, texturas, modelos GLB/FBX, fuentes Blender y generaciones Tripo. Conservar fuente original y variantes editables/optimizadas, identificador de generación, hashes, referencia, estado de aprobación y ubicación permanente.

## Reglas obligatorias
1. Buscar en el catálogo existente antes de crear o regenerar un recurso.
2. No confundir GitHub Actions artifacts ni carpetas temporales del runner con copias permanentes.
3. No borrar ni mover archivos actuales; usar registro de rutas y hashes. No tocar PR #127, Unity, Blender, escenas, builds ni publicar.
4. No repetir generaciones pagadas para recuperar una fuente: buscar task ID, descargar el resultado existente y verificar integridad.
5. Mantener separados estados de candidato, aprobado, rechazado y bloqueado por el propietario.
6. No almacenar claves API, credenciales, URLs temporales firmadas ni secretos.
7. Antes de automatizar, validar con pruebas en rama independiente, sin llamadas pagadas.

## Aldric
- Retrato aprobado reconstruible: `tools/aldric-dialogue/approved-artpass-source.zlib.b64` (en main).
- Cuatro expresiones 2D: `Unity/Assets/Eldoria/Resources/UI/AldricDialogue/`.
- Proveniencia: `Unity/Assets/Eldoria/Resources/UI/AldricDialogue/ALDRIC_BUST_PROVENANCE.json`.
- PR #118 fusionada. La existencia de GLB original completo requiere comprobación separada; no confundir la fuente de retrato con el modelo 3D íntegro.

## Tripo Tier V
- PR #127 en producción: seis referencias en `ArtSource/Tier5ReferenceProduction/references/`, manifest con task IDs.
- Archivos de runner: `$env:USERPROFILE/EldoriaArtSource/Tier5ReferenceProduction/`.
- Los artifacts de Actions no son almacenamiento permanente. Registrar cada GLB original, GLB optimizado y BLEND en almacenamiento versionado duradero, sin interferir en la ejecución.

## Entregables de la siguiente fase
- Inventario cruzado 2D+3D con estado real de persistencia.
- Detección de altas sin alterar contenido; clasificación de archivos sin copia automática de secretos.
- Copia persistente verificada por hash de cada generación; recuperar fuentes faltantes cuando sea posible.
- Galería de miniaturas y búsqueda por nombre, tipo, estado y origen.
- Pruebas de recuperación y de no modificación del juego antes de fusionar.

Esta fase NO certifica que todos los assets estén respaldados ni activa una automatización de guardado.
