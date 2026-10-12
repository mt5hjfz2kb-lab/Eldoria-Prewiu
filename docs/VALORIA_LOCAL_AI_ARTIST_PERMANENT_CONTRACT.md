# Eldoria — contrato permanente del artista IA local

**Decisión del propietario (2026-10-12):** la razón de disponer de Qwen/Ollama + BlenderMCP es que actúe SIEMPRE como artista profesional y autor real de los assets de Eldoria. Este trabajo NO se considera terminado por haber escrito un prompt.

## Condición obligatoria de finalización
1. Conectar el perfil `tools/art-direction/VALORIA_LOCAL_ARTIST_SYSTEM_PROMPT.md` como mensaje de **sistema en cada invocación** del artista local, tanto en PC Windows como en workflows remotos. No basta con mencionarlo en documentación.
2. Comprobar con un registro de ejecución real que Ollama/Qwen recibe el perfil y responde con diagnóstico, intención artística, cambios concretos en Blender y criterios de validación. El registro debe identificar modelo, hash del perfil, imágenes de referencia y captura evaluadas; no registrar secretos.
3. Probar una **edición real en Blender/BlenderMCP decidida por el modelo**, partiendo de una variante del asset, sin reescribir el original aprobado, y exportar GLB con evidencia antes/después.
4. Importar el GLB de prueba a Unity, renderizar desde cámara comparable y evaluar diferencias visibles. Separar éxito técnico de éxito artístico. No afirmar aprendizaje automático de pesos: el aprendizaje persistente inicial se materializa en memoria documental de defectos, decisiones, resultados y correcciones.
5. Guardar en GitHub prompt versionado, interfaz de ejecución, documentación de puesta en marcha, pruebas, logs y artefactos. Comprobar persistencia después de reiniciar la sesión/runner.
6. Solo tras verificar estos puntos, actualizar la entrada canónica de AGENTS.md mediante la coordinación de propietarios; NO modificar el archivo ni otras rutas reservadas por Work en paralelo.

## Integración prevista y límites de concurrencia
- Integración activa existente: `tools/art-rd/valoria_cliff_unity_variant.py` invoca `/api/chat` con imágenes y un mensaje de usuario, pero **no carga el nuevo perfil como SYSTEM**. Esta es la deuda técnica prioritaria.
- Work es propietario temporal de `integration/valoria-v3-unity-lookdev-20261012` y de los scripts de ese flujo. No escribir en su rama mientras esté activo: integrar después mediante cherry-pick/revisión coordinada.
- No iniciar workflows con Ollama/Blender/Unity concurrentes que interfieran con trabajos activos.
- No consumir créditos de pago. El éxito se demuestra con resultados, no solo con un pipeline verde.

## Estado a 2026-10-12
- [x] Prompt profesional persistido y versionado en rama independiente.
- [x] Contrato de integración y criterios de aceptación registrados.
- [ ] Perfil cargado automáticamente en invocaciones de Ollama.
- [ ] Evidencia de diálogo del modelo con el perfil cargado.
- [ ] Evidencia de modificación BlenderMCP dirigida por Qwen con el perfil.
- [ ] Comparativa visual real de Unity, validada.
- [ ] Memoria de decisiones artísticas actualizada tras cada ejecución.
- [ ] Regla integrada al flujo canónico tras cesión de propiedad.

**Estado: PENDIENTE DE IMPLEMENTACIÓN Y VALIDACIÓN. No declarar completado antes.**
