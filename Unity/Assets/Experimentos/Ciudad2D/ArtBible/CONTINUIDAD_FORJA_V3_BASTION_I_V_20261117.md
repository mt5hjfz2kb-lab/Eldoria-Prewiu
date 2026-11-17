# Valoria 2.5D — auditoría de continuidad artística (Forja V3 / Bastión I–V)
**Fecha:** 17/11/2026 · Rama aislada `art/ciudad-2d-hitos-1-5-20261116`.

## Diagnóstico de trabajo, no aprobación automática

El método 2.5D ilustrado ya está implementado y probado con capturas auténticas de Unity Editor, importación RGBA, transformaciones de escala y pivotes. **La posibilidad de importar un edificio visualmente atractivo no equivale aún a reproducir una familia coherente de edificios**.

### Forja V3: un avance artístico real

- Fuente intacta `ArtBible/Pilots/forja_guardia/v3/source_original_rgba.png`, 1536×1024, SHA-256 `1fb781c22a60ae61ce4a3cb28d6f394be3b15d8d344ca5e48a3afbd37877bd63`.
- Candidato aislado `Sprites/AIPilot/Versions/Forja_Guardia_RGBA_v3.png`, 1433×1433, SHA-256 `ed42684004cd891c83d933d38b195fb794f5db8449bb4436b2c043a7ab8c99ad`.
- Sombra independiente `Sprites/AIPilot/Versions/Forja_Guardia_ContactShadow_v3.png`, SHA-256 `24bcd1e5ec6abbbf5ce3455632b3728a5a732a2057a46d17625483a5167c8e80`. Derivada de la huella alfa baja; **aproximación 2D**, no render de sombras físicamente correcto ni aprobación de suelo.
- [Importación y hash #44044752465](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44044752465), [capa de sombra #44046028729](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44046028729).
- [Unity nativo #44045010681](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44045010681), ocho imágenes A/B, y [#44046565867](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44046565867), diez imágenes incluidas tomas enfocadas a la Forja y sombra; fuentes y SHA verificables.
- **Cambio importante del experimento:** la primera galería con tres recursos IA situados simultáneamente inducía una conclusión visual injusta: el Observatorio V1, desproporcionadamente alto, invadía la toma cercana de la Forja. Se corrigió `City2DAIPilotGallery.CaptureForjaV3` para mantener los otros dos edificios originales y comparar **una sola variable**. La nueva ejecución [#44047998748](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44047998748) está destinada a cerrar esa comparación.

### Evaluación artística independiente de la compilación

**Mejoras visibles de V3 respecto a la Forja inicial:** arquitectura funcional más característica, piedra y pizarra más detalladas, ventanas emisivas, madera menos uniforme, humo original conservado y ausencia de plataforma rectangular. Son pruebas visuales sobre sus imágenes originales y en Unity, no métricas automáticas de excelencia.

**Aún por corregir:** el símbolo astral dorado de la Forja no replica exactamente el árbol/heráldica de los maestros, su calidez/saturación difiere de la mampostería del Bastión V, y su escala en 390×844 necesita revisión final en galería aislada. El Observatorio V1 no cumple volumen/proporción/perspectiva; el Archivo V1 necesita encuadre, heráldica y control de base. Por tanto, **tres edificios homogéneos de nivel premium NO demostrados**.

### Obstáculos verificados del método generativo

- La conexión Runway estaba a **0 créditos** y rechazó una petición de Bastión II con el maestro físico `Bastion_I_MASTER.png` adjunto como referencia. El intento se hizo una vez; **no reintentar cambiando modelo sin resolver la causa**.
- El generador de imágenes alternativo produjo una Forja V3 utilizable, pero solicitudes posteriores de arte aislado Bastión II devolvieron infografías con afirmaciones falsas de calidad/validación. Se descartaron. **No son sprites de Unity ni prueban la progresión I–V**.
- La generación exacta de la Forja V3 no tiene evidencia de que los maestros binarios fueran consumidos directamente como referencias, aunque el resultado presente detalles similares. La reproducción de *especificación* es una hipótesis, no una técnica certificada para catálogo.

### Estado real de Bastión I–V

- [Unity #44045010681](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44045010681) incluye capturas 390×844 de los cinco estados; hashes individualizados en `BASTION_I_V_RECONSTRUCTION_CONTRACT_v1.json`.
- La ciudad y las parcelas sí evolucionan por hitos según `City2DProgressionPolicy`, sin alterar guardados ni reglas.
- **Arte de Bastión: únicamente I y V son sprites originales únicos**. Para II, III y IV sigue apareciendo el maestro I. Los sprites individuales `Bastion_Nivel02.png`, `03.png`, `04.png` NO existen y **no se han inventado**. Necesitan fuentes arquitectónicas propias y coherencia de silueta progresiva antes de declararlos terminados.
- La validación de la Biblia Visual y el nuevo contrato de fases impide declarar restauración 5/5 o avanzar a interfaz/mundo con el resultado incompleto. [CI valida este estado de forma explícita](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/workflows/ciudad2d-visual-bible.yml).

## Decisión aplicada

Mantener los seis maestros originales, la escena 2.5D, el terreno A/B, las versiones Blender y las tres fuentes IA. Consolidar V3 como **candidato de estudio, no nuevo maestro aprobado**. No tocar `main`, mecánicas, economía, guardados o escenas jugables.

**Próximo gate genuino de producción:** conseguir Bastión II con referencia original REAL y alfa correcto; juzgarlo en Unity bajo la misma cámara y solo si mantiene identidad generar III/IV. No construir montajes simulados como si fueran cinco niveles reales. No pasar a interfaz/pantallas/mundo mientras haya tres niveles sin arte satisfactorio.
