# Eldoria · QA visual en GitHub (candidata aislada)

**Qué gana el jugador:** cada certificación de UI puede adjuntar un informe de las capturas antes/después, con los SHA reales y una advertencia explícita si faltan pruebas. Evita confundir un CI verde con fidelidad al arte aprobado.

**Qué NO hace:** no modifica Unity, no crea arte, no controla Windows, no decide si una pantalla es bonita, no publica ni fusiona código. Unity no es una IA. La futura IA operadora sería Codex, utilizando Unity CLI + Unity Pipeline.

## Entrada y evidencia
- GitHub-hosted Ubuntu, permiso de solo lectura `contents: read` + `actions: read`.
- Se dispara después de `UI Progression Certification (fast)` (solo si el run finaliza correctamente); acepta `workflow_dispatch` manual con un run conocido.
- Selecciona la ejecución anterior **del mismo workflow, rama y repositorio**, cuyo commit SHA completo sea diferente y cuyo ZIP nativo `eldoria-v41-native-<run_id>` siga vigente.
- Descarga únicamente los ZIP de capturas; controla tamaños, nombres de fichero peligrosos, PNG duplicados, dimensiones y hashes.
- Produce `visual-report/index.html` y `report.json` con métricas descriptivas, sin inventar una valoración artística.
- `art_approved=false` y `artistic_progress_machine_decided=false` siempre.

## Prueba aislada realizada
Se compararon 53 pares de PNG reales de Unity de las ejecuciones `41219845900` y `41434883428`: 45 archivos modificados, 8 idénticos, ninguna incompatibilidad de resolución. El prototipo superó 8 pruebas automáticas de manipulación de entradas y seguridad, sin Windows ni Unity abierto.

## Prueba local
```bash
python -m pip install Pillow==11.3.0
python -m unittest discover -s tools/visual-qa -p "test_*.py" -v
python tools/visual-qa/compare.py --before-zip old.zip --after-zip new.zip --before-sha <SHA40_ANTERIOR> --after-sha <SHA40_NUEVO> --output visual-report
```

## Prueba futura Codex → Unity CLI
1. Esperar a que el propietario vigente de PR #136 libere Windows. No cancelar ni reiniciar procesos.
2. Crear una copia de Unity totalmente aislada, sin reutilizar el mismo `Library` o `ProjectSettings` activo.
3. Comprobar `unity --version`, paquete `com.unity.pipeline` ausente en la copia y configuración de seguridad.
4. Instalar el complemento oficial de Codex y, solo en la copia, `unity pipeline install` cuando esté autorizado por la seguridad y el entorno.
5. Comenzar por inspección sin escrituras; después una única corrección reversible, captura original A/B y pruebas funcionales.
6. No confundir la capacidad del agente con una aprobación de la dirección artística.

Fuentes: https://docs.unity.com/en-us/unity-cli/unity-pipeline/unity-pipeline-package y https://github.com/Unity-Technologies/unity-agent-plugin

**Frontera de integración:** mientras esta PR sea borrador, no se ejecuta el workflow automático en `main`. La automatización entra en vigor solamente si se revisa y fusiona expresamente. No hay nuevas credenciales, servicios ni gastos.
