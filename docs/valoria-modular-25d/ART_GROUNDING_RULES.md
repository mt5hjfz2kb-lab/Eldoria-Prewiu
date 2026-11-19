# Eldoria — guía de terreno y huellas 2.5D (Bastión I)

**Estado:** implementado como candidato en rama aislada. **Sin aprobación artística ni móvil.** La referencia conceptual sirve para dirección visual, no como imagen gigante de escena.

## Motivo del cambio

Las capturas reales de Unity del run 44524701160 mostraban edificios pintados con materiales elaborados pero suelo verde casi uniforme y caminos realizados con tres LineRenderer rígidos. Piedras, árboles y edificios parecían capas de distinta calidad. Aumentar solo la sombra de los edificios no resuelve la discontinuidad cromática ni los bordes duros de los caminos.

## Referencias investigadas

Se consultaron las páginas oficiales de Whiteout Survival, Kingshot y Top Heroes para trasladar principios generales de diseño móvil: jerarquía visual, terreno limpio pero con masas de color, construcciones reconocibles, parcelas vinculadas a caminos, escala y luz constantes. No se afirma conocer sus tecnologías internas ni se copian modelos, texturas o personajes.

https://www.whiteoutsurvival.com/
https://www.kingshot.com/
https://www.topheroes.com/en/landing.html?page_id=26

## Contrato de producción reproducible

| Elemento | Archivo | Implementación |
|---|---|---|
| Pradera | Modular25DGround.cs | 81 mallas independientes 8x8 m; material compartido con atlas RGBA 768² repetible, hierba oliva con masas de verde/ocre, trazos finos y algunas flores |
| Camino | Modular25DLandscape.cs | Dos mallas procedentes de splines con 9 puntos transversales; una textura alpha compartida 256x512, bordes erosionados, gravilla y roderas |
| Huella del Bastión | Modular25DLandscape.cs | Sprite 512x320 con tierra pisada, piedra difusa en acceso, hierba erosionada en bordes |
| Huella del Aserradero | Modular25DLandscape.cs | Sprite 512x320 distinto: suelo seco y serrín. Se mantiene en parcela, construcción y edificio terminado |
| Vegetación local | Modular25DLandscape.cs | Arbustos, pequeñas piedras y transición vegetal como objetos independientes junto a huellas |
| Edificios y habitantes | Modular25DArt.cs y Modular25DWorld.cs | Recursos originales independientes, pivotes de contacto y orden de profundidad por posición de los pies |
| Lógica | Modular25DGameplay.cs | LocalGateway, costes, progreso de obra y guardado canónicos. Ninguna regla económica nueva |

Paleta: verdes oliva de sombras aproximados RGB 60/85/43, hierba central 80/105/49, altas luces 130/140/74; tierra y caminos ocre/madera 150/122/83; piedra cálida de los edificios. Los valores son objetivos iniciales de autoría, no una medición automática de calidad.

La iluminación se mantiene desde el noroeste con sombras frías bajo objetos y cálidas sobre piedra y tierra. Los caminos pueden desplazarse y cambiar de extensión sin editar la imagen del suelo. Las huellas van unidas al anclaje espacial de cada edificio y no deben crear plataformas artificiales.

## Reglas de integración

1. La cámara es fija, ortográfica; no hay rotación libre.
2. El terreno es un material repetible. No se importa la imagen conceptual como fondo ni se recortan construcciones desde ella.
3. Los caminos se trazan por geometría real en el mundo; nunca con fotogramas falsos o un desplazamiento de lienzo.
4. Dos superficies de huella independientes conectan los edificios a la pradera; ambas tienen transición alpha y detalle contextual.
5. Se conserva el orden FootOrder(y), la oclusión real y las rutas de actores.
6. Las texturas se producen una vez por escena, con semillas estables, y se liberan al destruirla.
7. Todo el trabajo vive en Assets/Experimentos/ValoriaModular25D. No se toca main ni Premium V6.

## Presupuesto preliminar, no medición GPU

- Pradera RGBA 768²: ~2,25 MiB.
- Camino RGBA 256x512: ~0,50 MiB.
- Dos huellas RGBA 512x320: ~1,25 MiB.
- Total nuevas texturas sin compresión: ~4 MiB, aparte de edificios, árboles y props.
- Pendiente: comprobar memoria de inicio, generación de texturas, GC, overdraw y draw calls en dispositivo.

## Validación honesta

Se requiere nueva captura **auténtica de Unity** identificada por SHA y manifiesto, en 390x844, 1170x2532, horizontal y con zoom. Deben verificarse las tres fases del Aserradero, las uniones camino-parcela, el pan y el movimiento. A continuación PlayMode/UI, WebGL y prueba física en iPhone. El hecho de que el proyecto compile no certifica calidad visual, y un render externo no demuestra integración. No congelar la técnica hasta revisión y aprobación del propietario.
