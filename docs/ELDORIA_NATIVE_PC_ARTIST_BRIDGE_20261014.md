# Eldoria — conexión comprobada del artista IA al Blender y Ollama del PC Windows

**Capacidad técnica del transporte: PROBADA. Calidad artística profesional: NO APROBADA.** Versión de pruebas del 14-10-2026. **Producción general sigue PAUSADA**. El permiso del propietario para pruebas de conectividad/artista no habilita otras misiones, Unity ni publicar.

## Evidencia original del ordenador

- [#38606958074](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38606958074): ejecución en self-hosted Windows con Blender **5.2.2 LTS** real y `bpy`.
- [#38607499677](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38607499677): 26 operaciones de taller realizadas por Blender instalado en PC con .blend, .glb y capturas. El plan técnico de smoke fue estático, NO Qwen.
- [#38609051105](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38609051105): Ollama **0.40.2** encontrado en un perfil Windows y modelos existentes. El motivo de la respuesta inicial negativa fue que el servicio Ollama no estaba en ejecución en loopback, no que faltase la instalación.
- [#38609357863](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38609357863): Ollama ejecutado localmente bajo demanda y modelo **qwen3-vl:4b-instruct** ya descargado (también `eldoria-artist:latest` y `Qwen3-VL-2B-Instruct-GGUF:Q4_K_M`). Cero pesos o créditos nuevos.
- [#38610238442](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38610238442): primera generación del modelo LOCAL sobre la torre original propuso escultura con `distance=0.1`, `radius=0.05`, rechazados por contrato de Blender. Se conservó el original; no se fingió un éxito.
- [#38610802289](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38610802289): **SUCCESS de cadena en el MISMO PC**: el Qwen 4B LOCAL escogió originalmente `sculpt_anchor(x_max,+Y,1.4,3)` y material PBR RGBA `[0.75,0.65,0.55,1.0]`, roughness `0.85`, metallic `0.15`; Blender 5.2 ejecutó 2 acciones originales, modificó la malla heredada, exportó `.blend`/`.glb` y captura A/B real. Fuente congelada intacta. SHA256 del JSON bruto Qwen == SHA256 del plan realmente ejecutado: `3962044c882b23712fbab195e5a2570af59e8e9395cde8293a1631813b7b3d09`. La autoría es de Qwen, sin sustituir sus números creativos.
- [#38611090822](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38611090822): cuatro operaciones de herramienta elegidas y ejecutadas por Qwen en Blender Windows (`modifier,material,uv,modifier`). **Solo prueba API y ejecución, NO cambio de datos de malla heredada** (`inherited_mesh_data_changed=false`) ni resultado de calidad.

## Integración segura en el estudio

- `tools/art-direction/full-studio/pc_local_qwen_blender_cycle.ps1`: módulo reutilizable de ejecución controlada, Windows; busca la instalación de Blender y Ollama ya presentes, levanta `ollama serve` en loopback si es necesario, no descarga pesos, inspecciona un GLB real mediante Blender, envía referencia + render original al modelo Qwen, le exige un plan tipado, permite hasta tres propuestas originales si alguna viola el contrato, y **nunca altera los valores creativos** para hacer pasar una ejecución. Ejecuta plan exactamente idéntico al JSON aceptado por Blender y verifica SHA, malla, capturas y exportación.
- `.github/workflows/eldoria-artist-native-windows-ollama-qwen-blender-manual.yml`: flujo **solo manual** desde la rama artística autorizada. No activa automáticamente Unity, publicaciones ni misiones antiguas; utiliza las etiquetas `self-hosted, windows, unity-6000-3-23f1`. Mientras permanezca la pausa general, usarlo exclusivamente para validación artística autorizada.

**Limitación importante:** este ensayo controla dos acciones de escultura/PBR, no las cientos de herramientas de Blender ni una sesión GUI/MCP completa. El taller Python principal posee 26 familias de operaciones, pero no está demostrado que Qwen pueda seleccionarlas, combinarlas y corregirse con calidad profesional de forma sostenida. La modificación de la torre es visible, pero no presenta suficiente mejora en definición de materiales, coherencia arquitectónica y acabado respecto a la referencia: `ART_APPROVED=false`.

## Próximos criterios para reconocer un verdadero artista profesional

Un ciclo nuevo con selección genuina de varias familias de herramientas útiles (malla, modifier, UV, nodos/material, reparación de geometría), **edición realmente acumulativa y localizada por parte semántica**, comparación visual independiente con la referencia, iteración sobre defectos reales, `.blend` + `.glb` reutilizables y revisión Unity final. Actualmente: **conexión resuelta, herramienta validada, calidad objetivo no demostrada**.
