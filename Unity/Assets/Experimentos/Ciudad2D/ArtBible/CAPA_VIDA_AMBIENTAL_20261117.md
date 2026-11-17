# Valoria 2.5D — Capa de vida ambiental v1
**Alcance:** experimento aislado en `Unity/Assets/Experimentos/Ciudad2D/`, no en escenas jugables, `main` ni guardados.  
**Estado:** arte generado y cableado en Unity; verificación artística/Unity pendiente de resultados nativos. Esta documentación no es aprobación visual.

## Decisión actual del propietario

Se elimina la necesidad de ilustraciones separadas Bastión II, III y IV. Los cinco estados de **ciudad** continúan siendo I–V y mostrando edificios y parcelas según el estado de construcción; el Bastión utiliza exactamente su ilustración I para los niveles 1–4 y la V para el nivel 5. `ArtBible/BASTION_SHARED_TIERS_ACCEPTED_v2.json` es la decisión vigente; el contrato V1 de cinco Bastiones distintos se conserva como histórico, no como bloqueo.

## Vida implementada sin modificar dibujos originales

1. **Seis capas de humo** sobre tres chimeneas existentes: Cuartel (2), Aserradero (2) y Granero (2). Cada pareja usa variantes visuales y fases diferentes para que no se muevan sincronizadas. Si el edificio está bloqueado, disponible o en obra, sus efectos no aparecen; se activan cuando `CityBuilding.PlotPhase==Built`.
2. **Cuatro motas de brasa** junto a fuegos originales del acceso y Bastión. Reutilizan el PNG `HaloAntorcha.png`, se mueven ligeramente hacia arriba y desaparecen por transparencia, sin simular fuegos nuevos.
3. **Siete halos originales** mantienen exactamente sus fuentes y posiciones, pero reciben un parpadeo con dos frecuencias y fases desplazadas, menos mecánico que el seno único anterior.

Los sprites `Humo_Valoria_A/B.png` se generan con el script reproducible `tools/ciudad2d-ambient/build_ambient_sprites.py`; son 160×240 RGBA y sus hashes exactos constan en `QA/ambient_art_manifest_v1.json`. No se ha invocado un nuevo modelo de arte con créditos. No se borra el humo que ya esté horneado en las ilustraciones: el nuevo humo es una sobrecapa tenue, no una sustitución.

## Arquitectura técnica

`QA/ambient_life_v1.json` contiene las diez posiciones, propietarios, escalas, opacidades, fases y periodos; **no** altera `QA/city_layout.json`. `City2DAmbientSceneFactory.cs` lo valida y crea `SpriteRenderer` sin colliders. `City2DAmbientMotion.cs` no usa física, luces dinámicas, instanciación por fotograma, GPS, comandos de progreso ni persistencia.

`City2DStateBridge` propaga solo el nivel leído del snapshot para decidir qué ambientación mostrar. Las construcciones siguen utilizando `City2DProgressionPolicy` y `LocalGateway`, sin cambios. En el paso de Bastión III deberían verse 4 brasas y 4 humos; en el V, 4 brasas y 6 humos. Un área todavía no construida no emite humo.

## Verificación separada por nivel

- **Asset QA:** dos RGBA transparentes reproducibles, SHA y dimensiones confirmados en CI Ubuntu; no implica una buena textura pictórica.
- **Unity Editor nativo:** `City2DAmbientNativeEvidence.Capture` compara dos tiempos deterministas de animación en exactamente la misma cámara para I, III y V, con 390×844 / 1170×2532 / 844×390 según fase. Conserva PNG auténticos con SHA, comprueba que los instantes producen imágenes distintas y audita los 10 objetos sin interacción.
- **PlayMode real:** el test existente prueba HUD, evento puntero, cambio de niveles, lectura de datos y ahora verifica que los efectos se muestran según cada construcción. No prueba todavía rendimiento o estabilidad física.
- **QA artística:** inspeccionar en capturas si el humo sale del punto correcto, no tapa la arquitectura, evita halos y no se confunde con una plataforma; confirmar la lectura a tamaño de iPhone. **Pendiente de revisión**, no aprobación automática de CI.
- **Hardware:** iPhone/WebGL/Android, consumo de memoria, temperatura, estabilidad y batería siguen pendientes. No afirmar que se han probado.

## Criterios de rechazo

Elimina o reduce el efecto que produzca duplicación desagradable del humo ya dibujado, un halo visible sobre los edificios, cortes bruscos al reiniciar la animación, un punto de origen incorrecto, movimiento sincronizado de toda la ciudad, exceso de decoración sobre calles o bloqueo de eventos de puntero. Mantener el atardecer y los sprites originales sin filtros de color globales.

**Próxima evidencia:** imágenes reales de Unity en dos instantes, de las fases I, III y V, seguidas de una revisión artística. No ampliar a interfaz/pantallas/mundo hasta decidir si la ciudad y esta vida ambiental son suficientemente coherentes.
