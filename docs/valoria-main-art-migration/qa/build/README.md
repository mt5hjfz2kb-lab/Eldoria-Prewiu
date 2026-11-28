# Compilaciones privadas de Android y WebGL

Ejecutar en el PC Windows reservado para PR #201 cuando su propietario lo haya liberado para esta fase. Usar un checkout **desechable, exclusivo y limpio por SHA y plataforma**, con todos los blobs reales. Los empaquetadores de Main cambian ajustes del proyecto y generan arte. Conservar las salidas privadas fuera del checkout. Devolver el PC en cuanto terminen las operaciones asignadas. Este directorio no define reservas, jobs ni transportes de publicación.

```powershell
python $kit/private_build.py --checkout $checkout --source $sha --target Android --unity $unity --blender $blender --output $android
python $kit/private_build.py --checkout $otherCheckout --source $sha --target WebGL --unity $unity --blender $blender --output $web
python $kit/compare.py --main $webMain --candidate $webCandidate --output $webComparison
```

| Condición | Evidencia y resolución |
|---|---|
| Entorno | Unity 6000.3.23f1 con licencia activa, soporte del reproductor Win64, D3D11 y el módulo local de destino. En Android se usan el SDK, JDK, NDK, IL2CPP y herramientas de firma del editor. Obtener la falta concreta del preflight o del log Unity. Los scripts no instalan módulos ni compran servicios. |
| Bastión III | El SHA base contiene las recetas y los recursos fuente. La fisura GLB y los prefabs Granary, StoneWorkshop y Stock no se versionan. El proceso ejecuta el exportador Blender y BastionIIIArtBake del repositorio, verifica los productos y registra hashes. Repetir exactamente los mismos pasos sobre el candidato. |
| Android | Se usa ValoriaReferenceCleanAndroidBuild, el certificado de la aplicación canónica y los recortes de empaquetado de eldoria-family-android-build.yml. Requiere el almacén local documentado por Main. Se verifica la firma del APK final y la presencia de IL2CPP ARM64. Si falta la identidad, usar su runner autorizado. El APK queda en player.apk, sin instalación ni distribución. |
| WebGL | Se usa ValoriaIntegratedWebBuild para la escena ciudad/mundo de Main. Cada compilación genera nuevamente la prueba nativa exigida por el empaquetador y contrasta su firma de entradas en WebGL. Un desacuerdo se investiga en las diferencias de manifiesto según las reglas del empaquetador. El jugador local queda en player/. El historial de una ejecución anterior no satisface la prueba de esta ejecución. |
| Inventario de paquete | El observador Editor exporta las filas reales de BuildReport con contenedor, GUID, ruta y bytes. El manifiesto exige el SHA de juego y versión de Unity correctos, recibo de éxito del empaquetador y archivos actuales con SHA-256. compare.py vuelve a calcular hashes y entrega tamaños y diferencias por recurso. El tamaño del activo empaquetado y el tamaño comprimido del archivo final se registran por separado. |

Una compilación privada acredita el contenido empaquetado. FPS, memoria residente, draw calls, estabilidad, tiempos de carga del jugador, recuperación de partidas, WebKit e iPhone físico requieren ejecución independiente sobre ese mismo artefacto. Los ejecutables, APK y WebGL deben permanecer privados. Ninguno de los dos programas publica, registra una beta o modifica el canal Android.
