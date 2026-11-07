# Eldoria · Forja Oscura v0.4 — staging técnico aislado
Este material todavía NO se ha compilado ni capturado en Unity. La rama no altera main, PR163, el ejecutor Windows ni las partidas.
El ZIP original (3.489.120 bytes, SHA-256 7fc8ad748199375c2ab8e3b4a64e5b1d3e0bf027a33b31e83452f5b134f4d74f) está dividido en 15 fragmentos binarios y se reconstruye con **python tools/forja-oscura/reconstruct.py --extract .** desde la raíz de un checkout limpio.
Incluye 41 sprites de producción, arte ilustrado para los 20 iconos y 7 fuentes C#/asmdef en Unity/Assets/EldoriaVisualKit/.
**No ejecutar el capturador nativo hasta recuperar exclusivamente la reserva de Windows/Unity del propietario, certificar la compilación y proteger los cambios de PR163.**
Código de captura: Eldoria.VisualKit.Editor.ForjaOscuraNativeEvidence.CaptureBatch (8 fotografías genuinas de Unity, misma sesión y cuatro resoluciones). No publicar, no integrar main sin validación.

## Material premium: seguimiento de la referencia seleccionada (07/11/2026)

La referencia aprobada tiene un botón principal ámbar iluminado, navegación elegida en bronce cálido, controles bloqueados en acero apagado y marcos de metal negro. El código de `forja-detail-art.js` reproduce esos **materiales en sprites independientes**, sin pintar textos ni inventar indicadores. Es una aproximación técnica — **no una reproducción exacta demostrada en el juego**.

La variante anterior producía un botón rectangular marrón apagado, selección inferior gris-azulada y un botón deshabilitado también dorado. Se revisaron las paletas por estado: `button_primary_normal` (ámbar), `hover` (ámbar más luminoso), `pressed` (cobre sombreado), `disabled` (acero), `nav_tile_selected` (bronce con ribete ámbar). Los cuatro bordes y remaches siguen protegidos por `check-premium-corner-safety.js`; los nueve PNG reales se comparan byte a byte con su generador mediante `verify-current-premium-art.js`.

Estos cambios sólo afectan a las texturas `.png` de propiedad de PR #173. Conservan los 55 GUID, no sustituyen ciudad, misiones, costes, partidas ni eventos de controles. Para obtener la evidencia imprescindible falta Unity 6000.3.23f1 y capturas `native-before`/`native-after` sobre el HUD jugable. Ni CI de arte ni render de una textura aislada autorizan `ART_APPROVED`.

## Recuperación de Unity Windows — hechos contrastados (07/11/2026)

La PR #173 no puede aceptar un PASS basado únicamente en Roslyn o imágenes compuestas. La prueba Unity requiere las 8 capturas reales y el reporte de acciones en una sesión desechable. Las siguientes incidencias quedaron verificadas en GitHub Actions:

- [Run 42376865738](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42376865738): `actions/checkout@v4` comenzó una descarga integral enorme al no encontrar Git en PATH y no llegó a arrancar Unity antes de su cancelación.
- [Run 42380673233](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42380673233): la acción intentó borrar y descargar de nuevo el antiguo `valoria-layered-source`; no podía tratarse de una caché conservada. La ruta de recuperación sin clon falló correctamente al detectar la ausencia de `.git`.
- [Run 42382749739](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42382749739): GitHub Desktop proporciona realmente `git.exe` 2.53.0; la recuperación inicial del PATH dio PASS. Un paso redundante posterior falló porque `Get-Command` devuelve `ApplicationInfo` y no la propiedad `FullName` asumida. Se quitó ese paso.
- [Run 42382877546](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42382877546): superó descarga, reclamación de Windows y verificación de los 41 recursos, y alcanzó la ejecución nativa de Unity. **Resultado de capturas aún sin certificar al escribir esta nota.**

Diagnóstico cuantitativo del repositorio: el subárbol `Unity/Assets/Eldoria/ArtTests` incluye aproximadamente 9,5 GiB, muchos de ellos duplicados de `ValoriaLayeredConstruction/GeneratedEntryV1–17` e `Integration/Generated-3913*`. No confundirlos con activos usados: sus dependencias deben seguir verificándose contra la escena concreta, no borrarse del repositorio. Se preparó un clon parcial Git con exclusiones **solo en el directorio temporal del ejecutor**, nunca una eliminación en `main` ni en la PR. El objetivo es recuperar una compilación limpia sin otra transferencia integral. Cualquier ausencia visual o dependencia faltante invalida esa prueba. `UNITY_NATIVE_PASS` y `ART_APPROVED` siguen independientes.
