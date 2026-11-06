# PR162 — integración y publicación (6 noviembre 2026)

Estado: PR162 integrada; Android1.21 publicado y verificado; WebGL299 compilado, prueba completa de navegador en curso. No declarar cerrada la entrega hasta confirmar la publicación WebGL.

## Integración y evidencia

- PR162 fusionada en main: e7e37d236e982553e06168f0ec216e4de80b6b03.
- Nombre Unicode persistente, pictogramas y niveles de edificios/nodos/caza, regeneración UTC300segundos. Conservado límite48h de producción ausente.
- Certificación ampliada42216689519/sourcecb8a20aaae20bc5c41e042ba34d781359019b737: SUCCESS.216EditMode y75PlayMode, cero fallos u omitidos. XML descargados y leídos.
-225 comprobaciones de integración;20encuadres y16destinos de gestión, todosPASS; progresión continuaI–V y partida recargada. V41, diálogos,3marchas, Vigilantes/Exploradores y AldricPASS. Capturas de bienvenida, niveles, cartelas y pantallas revisadas.
- Evidencia principal13504299840 SHA25615aaae3f68bb146c34c994859cbc70d38073cc05413222696f72c083fc6cc185. V4113504257021,retención13505100179,diálogos13504746285,3marchas13503983916,Aldric13506270174.
- Certificación general anterior42210784774:216/75PASS y compilaciónWindowsPASS.

## Android publicado

- Versión1.21/code21. Compilación42217404501/sourcee3d8347c5ea0aecf8c48f216d1599d2ca7704db9.
- Publicación42225704456/job127918525196: SUCCESS. Descarga pública comparada byte a byte, canal main actualizado y3pruebas de canalPASS.
- APK470229532bytes, SHA256b90d72811e89bc3e00ff3b88806d8d550794a6e94f96261213228a30cedd7913.
- Paquetecom.eldoria.valoria.v5test; certificado originalf479ca92ccfa256a991936b23d0153ffad247b352a8a5ed569457db8274443d8. Sin reset de datos.
- URL:https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/releases/download/eldoria-android-code21/Eldoria-Valoria-code21.apk
- updates/android/family.json verificado code21. Solicitudes build y promoción desactivadas para evitar repetición.
- ArtifactAPK13503044417; prueba de empaquetado13504605007.
- Optimización exclusivaAndroid:32ilustraciones grandes ASTC4x4 calidad100. Dimensiones importadas conservadas, fuentes y metadatos originales restaurados/verificados. Verificador independiente tools/android-updates/verify-apk-textures.py (UnityPy1.25.4,Pillow,NumPy).
-32texturas reales decodificadas de APK y comparadas, SHAfuentes verificados; PSNRmínimo38.51dB, error medio máximo1.90/255. Seis recortes de mayor diferencia inspeccionados, sin corrupción/deterioro visible apreciable. Es compresión con pérdida, no identidad de píxeles.
- scout-action-skin-v4 ya estaba importada a2048x725 desde2107x746; referenciaLanczos aproximada documentada, PSNR45.16dB. No se atribuye a este filtro identidad con el importadorUnity.

## WebGL candidato y prueba

- Native299/build42213699104/source1db8585329fad788fa702dab3e8a7b76a916b4bc SUCCESS.
- Artifact13501518766 SHA2564adb472e14e384fafd5c55ba007457d82c8fddd81414ebc2b979ffd3774025b6.
- Comparación1db→e3d: solo dosfixturesEditor, builderAndroid, workflow, registro y solicitud; mismo código runtime y fuentesartísticas.
- QA actual42226584557/sourcef57a21257db03dcda11bd4682c7d7f2d72208498. Mismos bytesUnity, controles reales, reloj normal,844x390/390x844, nombre por teclado/puntero, progresiónI–II, marchas, vídeo y guardado/recarga exactos. Pendiente de resultado.
- Destino autorizado existente:https://chrismad.itch.io/eldoria-pruebas, canalwebRestricted,0EUR. Publicar una vez trasPASS y verificar versión remota activa.

## Causas corregidas y aprendizaje

1. Fixtures avanzados bloqueados por nueva bienvenida: completar formulario real solo en partidas de prueba; conservar prueba específica sin nombre. c788c942 conserva también firma pública de captura usada por reflexión y refresca etiquetas después del encuadre.
2. Ruta de lease en certificación reconocía únicamente antiguos propietarios. Admitir responsable canónico de main para PR162 sin relajar exclusividad; eliminar disparador pesado por editar el propio workflow.
3. Capturas verticales alteraban escala aheight/900 después de tomarPNG. Usar escala realwidth/800vertical yheight/900horizontal; conservar umbrales de geometría y legibilidad. cb8a20aa.
4. PrimeraAPK497493588bytes excedía450MiB. No elevar límite: ZIP9 daba ahorro insuficiente y librerías ya comprimidas. Aplicar ASTC4x4 solo a ilustraciones y comprobar resultado real.
5. Browser42218825091 esperaba conservar pan, pero TrySetWorld y contrato nativo vuelven aWorldHome+NearZoom en cada visita. QA exige dos retornos reales idénticos, zoom64vertical/36horizontal.
6. Browser42222753516 abortaba Bastión a90s por expectativa antigua10s. EconomyBastionTimingV2 y fechas guardadas demuestran120s. QA exige120s completos y admite máximo180s. También exige cartela de edificio sobre ciudad; entrenamiento/marchas mantienen pantalla independiente. No manipular reloj/estado para aprobar.

## Límites

No se ha probado instalación/rendimiento en teléfono físico ni jugabilidad del alojamientoRestricted. Los certificados funcionales y revisión de regresión no equivalen a una nueva aprobación artística global. Las capturas conservan las pantallas aprobadas provisionalmente; no se declara resuelto todo el trabajo artístico del proyecto.
