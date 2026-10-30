# Eldoria 1.15 — Interfaz integrada y Región 1

Fecha de entrega: 27-10-2026. Canal familiar Android, versionCode 15.
Esta entrega reúne el estado integrado de main; no es una versión final comercial.

## Qué incluye

- Interfaz Royal Gold integrada, diálogos y paneles de gestión existentes, compartiendo una sola partida.
- Correcciones posteriores a la APK 1.14: entrada y regreso al mundo en vista cercana, estabilidad de los nodos al cambiar el zoom y mejoras de etiquetas/visibilidad.
- Bosques decorativos depurados, conservando geografía, nodos funcionales, identificadores, economía y progresión.
- Ciudad, mundo, marchas, combate y guardados existentes. El diseño multijugador y su simulación siguen siendo herramientas offline; esta APK no activa servidores ni alianzas online.

La interfaz integrada no se presenta como enteramente nueva respecto a 1.14:
esta compilación permite probar el conjunto actual y sus correcciones recientes.

## Qué probar en el móvil

1. Actualizar sobre la instalación existente y comprobar que conserva la partida.
2. Abrir ciudad y mundo, volver a entrar al mundo y recorrer los tres niveles de zoom.
3. Revisar botones, textos y paneles en vertical y horizontal.
4. Seleccionar madera/piedra, enviar una marcha, regresar a ciudad y comprobar el resultado.
5. Cerrar y volver a abrir el juego para comprobar continuidad.

La actualización mantiene el paquete y la firma anteriores. Android pide confirmar
la instalación; no es una instalación silenciosa. No desinstalar ni borrar datos
para actualizar. La versión realmente instalada en el teléfono no se ha inspeccionado
desde esta sesión.

## Verificación

Build firmado: [40418364444](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40418364444), fuente `7413d029abe17e8b4a6fe366962bd522e022085f`, artefacto12696226924.
APK:395.819.992 bytes. SHA256:
`b6205cf93ddc42a8e50c9573de83dcad97c8bc1c8268583a8a5a891d1414d55c`.
Firma original, paquete, versionCode15 y permisos verificados por Android SDK.
Descarga independiente: SHA/CRC del artefacto y de la APK,11 bibliotecas ELF ARM64 correctas.
Publicación: [40419617788](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40419617788), SUCCESS.
[Descargar Eldoria 1.15](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/releases/download/eldoria-android-code15/Eldoria-Valoria-code15.apk) (aprox.396 MB).
Asset público679808928: digest SHA256 y tamaño coinciden con el archivo verificado.
El publicador volvió a descargar la APK pública y comparó sus bytes íntegros.
El manifiesto de main y la URL pública que consulta el actualizador anuncian code15.
La entrega está completada; las solicitudes se desactivan para evitar repeticiones.
No se declara prueba física del teléfono. El árbol Unity coincide con la fuente
ya certificada con65 PlayMode/26 EditMode; esta compilación Android no repite esas pruebas.
