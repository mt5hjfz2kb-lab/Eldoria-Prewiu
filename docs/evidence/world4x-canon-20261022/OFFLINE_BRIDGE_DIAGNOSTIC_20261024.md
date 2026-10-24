# Región 1 — análisis geométrico fuera del PC, puente central

**Fecha:** 24-10-2026. **Estado:** evidencia de análisis estático, NO certificación de Unity, NO aprobación artística y NO autorización para fusionar.

## Por qué se hace fuera de Windows

La ejecución de PR #100 [40089182936](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40089182936) fue asignada a `DESKTOP-R10PE55`, pero terminó `failure` sin pasos publicados, artefactos ni logs accesibles. El diagnóstico PowerShell mínimo [40090176843](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40090176843) también mostró cero pasos y terminó `cancelled`. Es un bloqueo del canal Windows; **no demuestra que el código haya fallado**, ni revela el estado local del servicio.

Los scripts locales existentes `scripts/repair-unity-runner-windows.ps1` y `scripts/switch-unity-runner-to-user-session.ps1` requieren ejecución administrativa local; no pueden controlar Windows si el propio runner no inicia comandos. No cancelar/reintentar en bucle ni interferir con procesos desconocidos.

## Fuentes auténticas recuperadas

- Artefacto de producción nativa de [40087460002](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40087460002), con `RegionOneBlenderWorld.glb` y `Eldoria_Region1_Production.blend` editables (fuente previa al cambio sin verificar).
- GLB extraído, SHA-256: `31573d467217a312938967e9d83f0a05191216757a3721d244495047bf26a664`.
- Metadata original de cámara Unity `cercana-horizontal-source.json` del mismo run: SHA-256 `bc14fac28bf24a47d4abc240d316d75e396207c78dfd3105d930adef1770c9c2`; verdadero PlayMode Unity, 1280×720, cámara ortográfica de tamaño 36, inclinación 42°.
- PR #100 último candidato de puente: `110f2793bd31efd9fa040daa8e840d8c0c015a1a`, **no probado nativamente**.

## Medición geométrica, sin motor

El GLB auténtico contiene una primitiva específica del puente central `Bridge alternate weathered plank.001`: 34 triángulos, posiciones glTF x = −4…4 m; altura 1,9573…3,0935 m; eje Z = 12,4224…45,4224 m. Para esta fuente glTF, Z es el inverso de Z de mundo Unity. El extremo del tablero dirigido al castillo corresponde al mínimo Z glTF.

A 720 px y zoom ortográfico de 36, la escala es **10 px/m**. Proyectando el extremo en la cámara registrada se obtiene su centro previo aproximado en **(640; 428,58 px)**. El cambio `110f2793` propone desplazar el tablero **+3,3 m en X** y **+10,5 m en Y de Blender** respecto al ensayo rechazado, lo que equivale, **sin cambiar altura**, a **(+33; −70,26 píxeles)**: nueva posición proyectada aproximada **(673; 358,32 px)**.

Este cálculo cuantifica el sentido y la magnitud del desplazamiento; no sustituye la captura auténtica. El código del nuevo candidato también cambia la altura de cada sección según el terreno importado y sincroniza los raíles; por ello la coordenada final Y no es certificable en el cálculo anterior. La puerta del castillo es otro asset de Unity y no forma parte de este GLB.

## Dictamen

El movimiento parece **geométricamente plausible** y corrige el sentido erróneo anterior, pero queda **SIN VERIFICAR** si alcanza la puerta, conecta con la orilla, oculta o duplica otra geometría AF/AG y pasa las pruebas. Conservar el último mundo plenamente certificado: ejecución [40069960813](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40069960813), 57 PlayMode + 26 EditMode + 44 controles.

Próxima operación segura: **una sola** ejecución nativa para el HEAD, únicamente cuando un ping PowerShell real del runner complete todos sus pasos y la reserva World4X continúe vigente. Después, revisar seis parejas de PNG Unity idénticas en cámara/aspecto, visibilidad de ambos apoyos, intersección de la puerta, continuidad de tablones y altura de barandillas. Materializar BLEND/GLB y ejecutar 57/26/44 antes de cualquier aceptación. PR #100 debe permanecer DRAFT/NO MERGE.

Esta nota evita nuevas correcciones a ciegas y no modifica ningún objeto jugable, guardado o recurso.
