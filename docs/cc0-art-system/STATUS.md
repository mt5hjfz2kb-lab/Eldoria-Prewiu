# Estado comprobable — Sistema artístico gratuito (Eldoria)

**Rama aislada:** `experiment/eldoria-cc0-art-system-20261116`  
**Workstream:** `eldoria-cc0-art-system-20261116`  
**Fecha de revisión:** 16/11/2026. Este documento NO es una aprobación de arte ni publica una beta.

## Hecho y confirmado

- **Creación, aislamiento y contrato:** `docs/cc0-art-system/README.md` y `asset-lock.json` presentes en esta rama, sin merges ni cambios en `main`.
- **Fuente original reproducible de Bastión I–V:** `tools/cc0-art-system/author_bastion.py` ejecutado en Blender 4.0.2 sobre Ubuntu, con archivos `.blend`, `.glb`, materiales y tres capturas auténticas de Blender (NO Unity). Ejecución de arte `43837586794` éxito; [evidencias en carpeta](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/tree/experiment/eldoria-cc0-art-system-20261116/docs/cc0-art-system/previews).
- **Quaternius real, no inventado:** 6 ficheros originales del pack Ultimate Fantasy RTS descargados desde Google Drive del autor, verificados por hash SHA-256 e importados físicamente en Blender, con captura real de comparación (incluye cuartel, mina, almacén, puerta y árboles). [Run 43844725460](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43844725460) SUCCESS; artifact `quaternius-actual-source-style-audit`.
- **Conclusión visual de la muestra:** el recurso `TownCenter_SecondAge_Level3.gltf` representa una fuente monumental y NO el Bastión. Quaternius es valioso para piezas secundarias y árboles; calidad/coherencia con nuestro Bastión sigue a examen. La captura de seis modelos **no equivale a una ciudad integrada**.
- **Licencias:** ficha de Quaternius marcada CC0, carpeta de descarga original confirmada, `License.txt` suministrado en la carpeta nombra por error Ultimate Platformer Pack; QAL de 2026 contiene restricciones de redistribución de fuentes. Por tanto **NO** publicar originales FBX/glTF en GitHub público sin resolver derechos por archivo. Las pruebas descargan a almacenamiento efímero de runner y publican solo PNG + auditoría.
- **Generador de escena Unity:** `Unity/Assets/Art_Reboot/CC0System/Editor/CC0SystemSceneBuilder.cs` implementado como candidato. **No ejecutado ni validado en Unity**: escenas, imágenes nativas, WebGL, Android y iPhone siguen pendientes.

## Prueba comparativa en curso

- [x] Guion híbrido `render_hybrid_valoria.py` implementado: fortaleza propia + modelos reales del editor (cuartel, granero, mina, troncos, ruinas, coníferas y rocas) con paleta común y sin plataformas nuevas.
- [x] Workflow alojado Ubuntu `.github/workflows/cc0-art-system-hybrid.yml`, completamente independiente del PC compartido.
- [ ] Verificar el run híbrido actual y sus tres imágenes 1600x900 / 720x1200.
- [ ] Crítica real de composición, proporciones, iluminación, conservación de silueta I/V y uniformidad entre packs. Si la combinación resulta pobre, pivotar antes de llevarla a Unity.

## Reglas de relevo

1. Al abrir otro chat, leer `main/AGENTS.md` y `pipeline/active-workstreams.json` (muy extenso, buscar el ID específico), luego estos archivos de la rama. No inferir permisos por silencio.
2. Leer HEAD/runs exactos de la rama y comprobar estado del workflow híbrido. No reutilizar citas de otras ramas.
3. No ocupar Windows sin reserva: el registro principal asigna recurso Windows a otros workstreams. No alterar `main`, `art/visual-reboot`, `experiment/nocturne-rebuild-bastion-i-v-20261115` ni PR #196.
4. No generar, instalar ni publicar licencias dudosas; no gastar créditos; no fusionar ni promover arte hasta pruebas.
5. Siguiente hito verificable: imágenes híbridas de calidad suficiente, después escena Unity y prueba de viewport real, después móvil.

## Estados separados

- **Investigado:** fuentes, packs, ediciones y licencias públicas.
- **Implementado:** scripts Blender, manifiesto, escena Unity solo como código y pipelines de captura.
- **Probado:** geometría propia/render Blender, 6 assets reales de Quaternius importados en Blender.
- **NO probado:** combinación híbrida hasta que su run complete, integración Unity y móviles.
- **NO aprobado:** calidad visual de juego, licencia de redistribución global y beta tester.
