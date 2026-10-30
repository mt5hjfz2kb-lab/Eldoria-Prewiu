# Entrega conjunta PR124 / PR125 / PR126

Estado: COMPLETADO Y VERIFICADO EN MAIN. PR124 fusionada; PR125 y PR126 incorporadas y cerradas, conservando sus ramas e historiales.

La PR124 reúne las tres marchas y la evolución de retención propuesta en PR125 y PR126. Se mantienen los historiales originales de ambas y un solo dominio, contador de informes, selección de relatos y sistema de guardado. Las recompensas y los valores de capacidad siguen siendo provisionales. No se han creado modelos ni gastado créditos.

## Composición visual revisada

Autoridad: `docs/early-retention/ART_DIRECTION_REFERENCE_20261028.md` y los recursos originales ya aceptados. Se conserva el camino pintado con 25 etapas, estandartes en cada quinto hito, Engendro violeta en etapa15, pergaminos de Exploradores y acciones rojas/azules originales. La adaptación vertical apila las tarjetas sin cambiar su identidad.

La revisión de las 47 capturas de la iteración40847718731 cubre las tres marchas en horizontal, vertical y teléfono; capacidad y composición; investigación más dos marchas; las 19 vistas de referencia y las 20 vistas ampliadas de guardianes, victoria, derrota y descubrimientos. Las banderas y cartelas quedan separadas de botones, temporizadores, accesos y navegación. El consejo de la derrota permanece visible tras cargar; el recuento y los sellos proceden de reclamaciones reales. Esta iteración no es el certificado final: un caso PlayMode detectó una selección antigua de100 soldados superior a la capacidad60.

La corrección adapta exclusivamente esa selección sin reservar ni perder soldados; no altera tropas viajando ni recursos, conserva el límite actual y permite continuar el encuentro original. El comando antiguo de preparación automática también selecciona dentro de la capacidad actual; una composición explícita excesiva sigue siendo rechazada. Las comprobaciones de animación, victoria y persistencia del encuentro original permanecen intactas.

## Alcance funcional

- Tres ranuras globales compartidas por expediciones de mundo, contratos e investigación; cuarto envío rechazado con tropas sobrantes.
- Capacidad por Cuartel y experiencia del héroe; tres reservas independientes, rutas visibles y retornos UTC idempotentes después de cargar.
- Aldric ausente de Valoria cuando viaja en cualquiera de las tres ranuras. Fuente artística aprobada conservada.
- Cinco perfiles de guardianes, consejo previo, informe automático con héroe, tropas, rondas, poder, índice rival y vida restante. Derrota sin avance, recursos ni XP.
- Doce relatos, hitos1/3/6 y cuatro familias descubiertas solo al reclamar; migración de reclamaciones antiguas antes de retirar encargos vencidos.
- Reclamaciones consecutivas y retornos offline atómicos: la actualización UTC propia no invalida el comando; una revisión realmente sustituida por otro comando se rechaza.

## Límites de la comprobación

Capturas auténticas y pruebas dentro de Unity Editor. Resoluciones de teléfono renderizadas por Unity; no equivalen a prueba física en móvil ni a medición de FPS. Esta entrega integra las fuentes; no publica una APK nueva. Android18 conserva su publicación previa.

## Certificación final

[Ejecución nativa40850803144](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40850803144): SUCCESS. Fuente solicitada `fd21934bc1889087d9713d5367389b094991207f`; compilada `c27b52c93c8d51e249fec86afd7228aa84596edf`; árbol raíz `e05acd3803c98ed43075115f98cab4677ce1670b`. EditMode54/54, PlayMode73/73, cero fallos u omitidos. El caso de animación/combate original que detectó la partida antigua ahora pasa con sus mismas aserciones.

Se han abierto y revisado las47 capturas de marchas y retención de esta ejecución y las12 vistas de Aldric. Las seis hojas de contacto conservan todas las vistas revisadas; ocho PNG originales quedan en el repositorio, junto al manifiesto SHA256 de los47 originales, resultados de interacción y las tres identidades de fuente. Los artefactos íntegros y sus digestos se registran en [el certificado](NATIVE_CERTIFICATE_20261029.json). Se mantiene la composición ratificada, textos completos y acciones alcanzables por raycast real. Esta aceptación corresponde a estas pantallas y marcadores; no es una aprobación del arte general del mundo ni una prueba física en teléfono.

La [ejecución final de main40859922725](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40859922725) terminó SUCCESS sobre `401a6e9d4fef2b62e53c6769aaff9e9ba6c7b138`:54/54 EditMode,73/73 PlayMode, cero omitidos y ejecutable de Windows generado. El paquete completo y sus digestos quedan registrados en [MAIN_INTEGRATION_20261029.json](MAIN_INTEGRATION_20261029.json); los XML auténticos y la comprobación de identidad se conservan en [main-native-40859922725](main-native-40859922725/test-results.json).

Se corrigió el arranque sin gráficos de BuildPlayer y se conservó la compilación después de las pruebas para que el preparador legacy no sustituyera su lista de escenas. Se mantienen todos los tests, límites, aserciones y guardas. Estas reparaciones solo cambian el proceso CI: el árbol Unity sigue siendo `6e02527491d789b2d8151d3548001c2ed16ea2aa`, igual al candidato de las47 capturas revisadas. Las ejecuciones anteriores fallidas se identifican como sustituidas; no se presentan como certificados completos.

Los tres workstreams se archivan como completados y la reserva canónica de main se libera tras terminar el job. Se conserva el trabajo ajeno de producción individual de referencias. No se publica una APK nueva en esta entrega; Android18 mantiene su publicación previa.
