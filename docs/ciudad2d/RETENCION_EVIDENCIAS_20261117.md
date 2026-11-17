# Eldoria / Valoria Ciudad2D — retención segura de pruebas
**Fecha:** 17 de noviembre de 2026. **Alcance:** rama aislada `art/ciudad-2d-hitos-1-5-20261116`.
**No se modifica `main`, los sprites maestros, la lógica ni los guardados.**

## Evidencia conservada de forma verificable
- `QA/ArtQualityV5_NATIVE_EVIDENCE.json`: manifiesto de los 60 fotogramas auténticos de Unity V5, 10 PNG nativos persistidos en `QA/NativeV5/`, verificados por SHA-256.
- `QA/ArtQualityV6_NATIVE_EVIDENCE.json`: manifiesto de 60 fotogramas auténticos de Unity V6, 10 PNG nativos persistidos en `QA/NativeV6/`, verificados por SHA-256.
- Los siete montajes originales contra candidatos por versión V5/V6 están versionados y se distinguen expresamente de capturas directas.
- Arte fuente, máscaras, sombras, `ArtBible/Masters`, otras pruebas históricas y sus commits se conservan.
- El respaldo portátil V6 `ciudad2d-quality-v6-review-44109813247`, artefacto **14339683231**, sigue disponible en GitHub Actions: **42.123.965 bytes**. Contiene las 10 imágenes V6 originales y el manifiesto completo; hashes comprobados también de forma independiente.

## Limpieza real ejecutada y verificada
- La comprobación en seco #44112818660 validó 20 PNG persistidos y 120 registros SHA, y aprobó únicamente dos IDs explícitos.
- La ejecución **#44112960921** comprobó de nuevo los respaldos y eliminó exclusivamente:
  - Artefacto 14335228213, ejecución 44096910663: **772.038.799 bytes**.
  - Artefacto 14337998938, ejecución 44105205248: **1.003.733.541 bytes**.
- **Espacio retirado de los artefactos GitHub Actions: 1.775.772.340 bytes (~1,65 GiB)**.
- La consulta posterior de los artefactos de ambas ejecuciones devolvió lista vacía, y el respaldo pequeño V6 seguía presente.
- Los logs de GitHub Actions y los commits anteriores permanecen en el historial, sujetos a las políticas normales de retención del proveedor.

## Evitar la acumulación a futuro
`.github/workflows/ciudad2d-native-proof.yml` mantiene la batería de comprobaciones pero **cambia el empaquetado**: solo sube las evidencias JSON con SHA, capturas representativas V6, pruebas PlayMode y logs esenciales; deja de adjuntar íntegramente V2, V3, V4, V5 y V6 en cada ejecución.
- El nuevo artefacto `ciudad2d-curated-unity-<run_id>` tiene **7 días** de retención, no 14.
- Después de subirlo correctamente, una limpieza con comprobación estricta de ruta elimina únicamente el clon temporal `city2d-native-<run_id>` creado por esa ejecución. Si la subida falla o sigue activo un proceso Unity/Blender, no lo borra.
- **Pendiente verificar en la siguiente ejecución nativa:** tamaño real del nuevo artefacto y resultado de borrado del clon temporal en el ejecutor Windows. No declarar ahorro local de disco hasta medirlo.
- La conservación permanente de las imágenes maestras, las versiones aceptadas, los manifiestos de hashes y una muestra nativa por hitos sigue siendo obligatoria.

## Distinción de tamaños
- **Artefactos borrados:** objetos remotos de GitHub Actions, no recursos del juego.
- **Directorio actual del experimento en GitHub:** aproximadamente 200 MB de archivos rastreados en el HEAD observado antes de la limpieza, incluyendo QA y variantes. La operación no eliminó sus archivos.
- **Git antiguo/caché de Unity/PC:** el tamaño del repositorio completo, la carpeta local `Library` y los discos físicos no se han medido ni modificado mediante esta limpieza.
- **Tamaño de la build y memoria de iPhone:** no están probados por esta operación.
