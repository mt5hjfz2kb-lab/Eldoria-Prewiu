# Valoria Reference Clean — entrega reproducible

Escena independiente construida y probada con Unity 6000.3.23f1 y Blender 5.2.2 LTS. Candidata funcional; no se declara ART_APPROVED ni objetivo artístico 8/10 alcanzado.

## Abrir y editar

Usar la rama `construction/valoria-master-plan-layers-20261016` y abrir:
`Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Valoria_Reference_Clean.unity`.

En esta carpeta están materiales, TerrainData, TerrainLayers, prefabs, GLB, shader y componentes de selección/cámara. Fuente editable empaquetada: `docs/evidence/valoria-reference-clean/Valoria_Reference_Clean.blend`. Paquete de intercambio: `Valoria_Reference_Clean.unitypackage`, junto a este documento. Preferir el proyecto completo: el paquete no sustituye Packages/manifest.json y requiere Terrain Physics habilitado, URP 17.3.0 y glTFast 6.14.1.

Se construyó desde una escena vacía, sin heredar geometría de escenas anteriores. Bastión compacto nuevo, puerta modular recuperada, puente con arcos reales, terreno Unity, río, cuatro parcelas vacías, ruinas y ambientación. Un único conjunto activo. Selección del Bastión y parcelas; arrastre y zoom de cámara; parcelas preparadas para futuras construcciones. No implementa un nuevo sistema completo de economía/construcción.

## Evidencia real

- Código ejecutado: `aef30f5e2aa46920311bec2794f4de5d4f3fd9cc`.
- Salida nativa versionada: `d102f3895b39918e4d5480a734a21ee8c497e114`.
- GitHub Actions: https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39064889290 — completado con éxito.
- A-blockout.png; B-architecture.png; C-art-scene.png; D-final.png.
- playmode-home.png, playmode-pan.png, playmode-zoom.png: renderizados auténticos de la cámara Unity durante Play Mode.
- detail-bastion.png y detail-gate-bridge.png: acercamientos nativos.
- playmode.json: 5 objetivos seleccionables, 76/76 comprobaciones del recorrido, 6 escalones, una cámara, pan/zoom verificados, cero excepciones y huella del guardado canónico intacta.
- unity-art.json: 421536 triángulos de arte importado, 24 renderers de arte. Los contadores de UnityStats no están disponibles en batch y sus ceros no significan cero draw calls.
- Medida Windows de render de cámara más lectura GPU a 960×540: 17.90 ms (8 muestras). No representa FPS móvil ni del juego completo. Memoria nativa de objetos Texture de la escena: 62183433 bytes.
- mobile-availability.json: ADB instalado, cero dispositivos Android autorizados. No se ha realizado perfilado físico Android/iOS.

## Integración mínima necesaria

SliceBoot omite su arranque automático solamente cuando la escena activa se llama Valoria_Reference_Clean; evita duplicar cámara, mundo e interfaz antiguos. Se habilitó el módulo integrado gratuito com.unity.modules.terrainphysics 1.0.0: el registro nativo demostró que Unity eliminaba TerrainCollider al entrar en Play Mode mientras estaba deshabilitado. No se actualizó Unity, URP ni glTFast. Pipeline URP específico de esta escena persistido como asset; escenas anteriores conservadas.

## Fuentes y continuidad

Procedencias/licencias y candidatos rechazados: resource-licenses.json y source-art.json. Coste 0 €, sin modelos de pago ni imágenes generadas. Biblioteca modular y procesos previos recuperados; pinos/arbustos y atlas existentes reutilizados y transformados; arquitectura nueva modelada con bpy. BlenderMCP y Ollama no respondieron en esta ejecución; Blender nativo sí produjo las fuentes reales.

Pendiente antes de considerar terminada toda la misión: perfilado real Android/iOS y continuar elevando el acabado respecto a la referencia (naturalidad de orillas y rocas, variación de superficies, vegetación y detalle ornamental). La composición y las estructuras son verificables, pero el parecido artístico no queda certificado por las pruebas funcionales. No se promueve esta candidata a escena canónica ni se reanudan otros trabajos pausados.

Reproducción automatizada: tools/valoria-reference-clean/author_scene.py y .github/workflows/valoria-reference-clean.yml. Antes de cualquier nueva ejecución nativa, recuperar una reserva exclusiva real de windows-runner-heavy y comprobar la actividad del PC; reserva liberada tras el último trabajo completado. No quedan tareas propias en segundo plano.
