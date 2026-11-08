# Tesoros de Valoria — dirección de arte, evidencia y cierre nativo
Corte: 2026-11-08. PR canónica: #181. Presupuesto adicional: 0 €.

## Decisión de calidad: descartar el acabado geométrico como arte definitivo
Se hizo una reproducción visual LOCAL del cofre de polígonos bajo luces planas. No es una captura Unity, y el resultado era demasiado plano y geométrico para el objetivo Vigilia de la Brecha. No elevarlo a ART_APPROVED por aumentar las capas vectoriales. En la PR se conserva como **fallback técnico** editable y barato, no como aspecto premium aprobado.

## Art nuevo producido, todavía NO importado al repositorio
Se produjo una ilustración específica de Tesoros de Valoria con dos estados consistentes de un mismo cofre: roble ennegrecido, hierro forjado, bronce desgastado, sigilo de la Brecha y luz ámbar interior. No procede de un pack externo comprado. En el chat propietario existe un paquete fuente/exportaciones originales, con PNG transparentes alineados (887×887 por estado) y optimizados (512×512 por estado), además de manifiesto SHA256.

Los dos PNG candidatos para integración tienen identidades:
- `treasure_closed_512.png`: SHA256 `313c17d35dcb6f44129a143fabc0448e5a20bc3fb7d00f700394bb73b2d0a0dd`, 68.382 bytes.
- `treasure_opened_512.png`: SHA256 `a75afb45b6183d0a8d7e2841c82ef13b812d08fb7620f90c849e679f24429ccb`, 76.263 bytes.

**No afirmar que estos PNG están versionados en GitHub:** el conector disponible ha permitido escribir C#/texto, pero no importar directamente los bytes binarios de las imágenes generadas. El paquete se conserva como artefacto del chat, no en una rama pública. Dejar el paso de importación pendiente; NO sustituirlo por datos inventados ni un screenshot.

## Puente de importación binaria ya producido y probado
- Se preparó el paquete de conversación `ELDORIA_181_ARTE_ORIGINAL_LISTO_PARA_UNITY.zip`, SHA-256 `031eb431fd5564d25e9bc368d08f2c9c169d5406fb0a48fcc874721d6e60dd8c`. Incluye los dos PNG de 512 originales y sus importadores .meta exclusivos, un manifiesto con SHA de cada arte, verificador y parche binario `eldoria_daily_treasures_art_binary.patch` (SHA-256 `3baaa71ef1d3e8d55bfd4f0142db15de75a468a074a88bcf1d67928a5c09c5d1`).
- **Prueba local efectuada:** en una copia de directorio sin los PNG, `git apply --check`, `git apply --binary` y el verificador de SHA/transparencia/GUID han pasado. Esto valida que el paquete puede aplicarse técnicamente; **NO acredita** la importación en el repositorio ni renderiza Unity.
- No se ha hecho push de los PNG porque el conector GitHub de esta sesión solo acepta archivos UTF-8 y no proporciona transferencia binaria desde el contenedor. En un entorno Git con acceso a este artefacto, aplicar desde la raíz de la rama PR181 y ejecutar las pruebas de CI y Unity. No convertir el ZIP en un nuevo repositorio ni tocar PR173 o la beta.
- Se ha probado también una animación de laboratorio en navegador y cuatro resoluciones usando los PNG fuente, sin Unity. El cofre ilustrado es una mejora importante frente al fallback geométrico; la calidad integrada definitiva permanece pendiente.

## Preparación ya implementada en Unity
`DailyTreasurePanel` está preparado para cargar dos sprites independientes:
- `Unity/Assets/Eldoria/DailyTreasures/Resources/DailyTreasureArt/treasure_closed_512.png`
- `Unity/Assets/Eldoria/DailyTreasures/Resources/DailyTreasureArt/treasure_opened_512.png`

Ambos deben importarse como sprites individuales con transparencia real y sus propios GUID estables. Si falta cualquiera, `HasAuthoredChestArt=false` y se muestra el cofre procedural de código. Si existen ambos, el panel usa imágenes ilustradas equivalentes, con `preserveAspect`, sin raycast y crossfade breve al abrir, luz y brasas independientes. El premio se registra ANTES de iniciar cualquier animación.

No confundir tener la carga condicional implementada con tener un sprite dentro del paquete Unity. El test final debe exigir `HasAuthoredChestArt=true`, dos sprites de tamaños/versiones acordados y una comprobación visual en pantalla, de lo contrario es un **NO-GO artístico**.

## Verificaciones ya posibles sin Unity
1. Reglas de calendario, racha y repetición, ciclos, UTC, rollback y desbordamientos.
2. Gateway real: siete días consecutivos, recursos exactos, buzón, lectura, borrado, recarga, orden de command IDs, ausencia y vuelta a día uno.
3. Cinco elementos de UI nativos de código independientes: fondo/grabado, iconos, botón, cofre funcional y tipografía de referencia disponible.
4. Disposición aritmética en ocho geometrías: 1600×900, 900×1600, 844×390, 390×844, 667×375, 375×667, 1024×768 y 768×1024. El séptimo día ocupa dos columnas para evitar letra diminuta.
5. Roslyn de nueve fuentes C# y guardas de no duplicidad; no equivalen a Unity API compilada.

## Revisión nativa y condición de verdadero cierre
- El recurso `windows-runner-heavy` debe estar formalmente liberado por PR173 y reclamado mediante ownership/lease CAS antes de ejecutar `.github/workflows/daily-treasures-native.yml`. No invadir ventanas de trabajo de la beta.
- En la misma SHA: importar originales con metadatos, compilar Unity 6000.3.23f1, EditMode/PlayMode, obtener 12 PNG originales del componente (calendario, cofre cerrado y abierto, cuatro resoluciones) y comprobar fuente, componentes y clics. La prueba dispone de una variante de gateway descartable y calcula IDs de recibos.
- La prueba de componente NO certifica la ciudad real. Repetir entrada y progreso I–V desde partida nueva en el juego real, presencia del botón, apertura en días 4/7, cierre de capítulo de Vigilantes, buzón, persistencia y orientación. Confirmar no se duplica el premio al saltar/reabrir.
- Evaluación artística independiente contra imagen aprobada de Vigilia: contraste, materiales, lectura, escala, ocupación, coherencia con ciudad, efectos, sonido y rendimiento. Si la animación es torpe, reautorizar arte/rig y repetir; no bajar los criterios.
- Android e iOS/WebGL reales, latencia, sonido y uso de memoria todavía NO medidos. No usar Android/iPhone simulado como prueba física.
- No fusionar o publicar antes de las pruebas y aprobación correspondientes. El resto de la beta native325 se conserva inalterado.

## Estado
**LÓGICA VERIFICADA / REFACTORIZACIÓN VISUAL CODIFICADA / ARTE ILUSTRADO EXPORTADO FUERA DEL REPO / TESTS NATIVOS PENDIENTES / ART_APPROVED = FALSE / NO MERGE / NO RELEASE.**
