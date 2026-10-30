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

## Consolidación ejecutable — 30/10/2026

La PR128 conserva su alcance de archivo: no sustituye modelos ni aprueba arte.
`scan_visual_assets.py` enumera todos los archivos visuales seguidos por Git,
registra SHA256/tamaño cuando los bytes están presentes y conserva filas explícitas
para checkout parcial, enlaces no seguidos y punteros LFS sin objeto. Un índice
parcial nunca acredita respaldo completo. Añade GUID y referencias serializadas
Unity; las cargas dinámicas Resources/GLB requieren revisar el host por separado.
El Bastión antiguo vetado conserva `OWNER_REJECTED`; el resto no gana aprobación
por existir en Git o por estar referenciado en una escena.

`ingest_visual_asset.py` copia sin mover fuentes, verifica hashes, conserva objetos
existentes y rechaza conflictos de bytes, metadatos y escapes por enlaces. Cada
objeto se nombra por hash y cada registro conserva la identidad original. No se
confunde este depósito local con una copia remota permanente. `verify_visual_backup.py`
comprueba un manifiesto contra los bytes de un depósito ya disponible.

Validación local: 8 pruebas pasan (clasificación, lectura sin mutación, ausentes,
LFS, GUID, enlaces, deduplicación y conflictos). El checkout parcial examinado
registró 5.687 rutas, de las que solo 18 estaban materializadas: esa ejecución
NO certifica las 5.669 fuentes ausentes. CI ejecuta el inventario en checkout
completo y conserva su resultado como evidencia temporal, no como respaldo.

Pendiente verificable: relacionar fuentes Tripo originales/optimizadas/BLEND de
PR127 con un depósito persistente comprobado y producir la galería curada. No
cerrar el encargo de archivo completo por estas pruebas de herramientas.
