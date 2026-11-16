# Visual Reboot — piloto real Unity, 16 noviembre 2026

## Resultado y alcance

Rama única: `art/visual-reboot`. Piloto original de tres estados: intacto, ruina y corrupción. No es Ashen Citadel I–V, no integra mecánicas nuevas y no modifica main salvo coordinación autorizada.

Run nativo **43779933745**, fuente **d74be4ee7d90c07a11becc62c480f37358596e0e**, Unity6000.3.23f1 / URP17.3.0: **SUCCESS**. Editor compila, importa, captura cuatro resoluciones y cierra con código0; limpieza de su scratch verificada. Artefacto14188480494, ZIP SHA256 `3d8174fcee473165bcf612de067e9a9c3df885af1eec1bc8b62ce16a6e3ebc09` descargado y verificado.

Las cuatro PNG originales y pruebas están en `Source~/Evidence/native-43779933745/`. La repetición de1100×1250 al final coincide píxel a píxel con la primera (diferencia media/máxima0). Inspección visual realizada en1100×1250,390×844,1600×900 y844×390: los tres edificios completos, sin atlas extendido, cuadrados de sombra en tejado ni suelo cuadriculado. No sustituir estas imágenes con renders Blender.

## Reapertura de la escena guardada

Run **43780662139**, fuente guardada **a41c07323b07539bdadda483a06b4266d420e55c**: SUCCESS, compilación/captura/cierre/limpieza correctos. Artefacto14188780604, ZIP SHA256 `91c23827a508d0802b1e3fa07330157ab3789a161c444bf872072c741d5a1ac4`, descargado y verificado. Las cuatro PNG coinciden byte a byte con las imágenes revisadas del run43779933745. No hace falta volver a ejecutar Unity por esta actualización exclusivamente documental.

## Abrir y reproducir

Escena montada: `Assets/Art_Reboot/Phase1/Scenes/Eldoria_Visual_Sandbox.unity`. Materiales, perfil de volumen, renderer y pipeline propios incluidos bajo `Materials/`. La jerarquía histórica copiada permanece inactiva; la escena original mantiene sus bytes.

Para reproducir el aspecto certificado, usar `Art Reboot/Build and capture phase 1`, que selecciona temporalmente el pipeline propio y restaura el anterior. Abrir sólo la escena utiliza el pipeline activo del proyecto y no garantiza el mismo postprocesado. El menú exige escenas guardadas y Edit Mode; no se debe desactivar el guard de Play/build para eludir V4.1.

Fuente inicial certificada: .blend + FBX + GLB, cuatro familias de malla, 21.865 triángulos, atlas 256², dos materiales y cámara 50°. Revisión artística regenerada: cuatro familias, 22.927 triángulos, tres materiales diferenciados; su prueba Unity definitiva sigue pendiente. Fuente FBX reimportada en Blender con UV por cara contenida en una sola casilla y todas las caras triangulares. Transferencias binarias verificadas por hash Git.

## Técnicas verificadas y errores que no repetir

- No generar `.meta` de textura con sólo GUID: Unity la interpretó como cubemap. Importador Texture2D explícito; metadatos nativos correctos conservados.
- FBX debe usar `useFileScale=true`: el archivo declaró centímetros. Sin esa conversión la cámara quedaba dentro de una superficie gigantesca. Terreno importado comprobado a48×48m.
- Exportar FBX triangulado; rechazar mallas inválidas antes del export. Bandera ahora tiene grosor; no usar polígonos opuestos coincidentes. Tejado con caras orientadas correctamente.
- Compilación de shaders síncrona, sonda ambiente explícita y tres peticiones de calentamiento por captura. Primera/repetida idénticas.
- Sombras suaves mediante campo serializado comprobado `m_SoftShadowsSupported` de la COPIA del pipeline; la propiedad pública es de sólo lectura en esta versión.
- Ajustar cámara con límites proyectados de los tres edificios; no un ancho fijo supuesto.
- Guardar también PNG rechazadas y métricas; conservar excepción primaria. No restaurar SceneSetup vacío en batchmode.
- Limpieza sólo del scratch propio ya copiado a evidencia, con ruta extendida para las rutas largas de PackageCache. No borrar proyectos ajenos.

## Revisión artística y límites

Piloto visual revisable, no ART_APPROVED. Mejora comprobada sobre el estado recuperado: materiales y escala correctos, luz estable, tejado reparado, suelo continuo y coníferas más llenas; los tres estados se distinguen. La arquitectura y vegetación siguen estilizadas y sencillas; no se declara calidad comercial ni nota9/10.

La primera y segunda corrección de la Fase 1 simularon ParticleSystem en el Editor (13 y 12 partículas, respectivamente); NO se ha verificado el movimiento durante Play ni en móvil. No probado: rendimiento móvil físico, Play/WebGL/Android ni integración completa del juego. El laboratorio conserva intencionadamente fuera las dependencias de gameplay/V4.1. Esas verificaciones no se infieren de capturas de Editor.

## Coordinación

Windows se libera al terminar cada ejecución; conservar propietario y fuentes de Ashen Citadel. Releer registro vivo y ejecuciones antes de reclamar un nuevo turno; no desplazar un retry ya en cola. No crear supervisiones periódicas ni mezclar ramas. La autorización de alternancia y coordinación en main ya fue concedida.

## Revisión artística solicitada el 16/11/2026 — SOLO Fase 1

El propietario exigió ocho correcciones: ocaso cálido con sol bajo; ventanas/antorchas emisivas; suelo de tres verdes, tierra y musgo; corrupción violeta confinada; piedra vieja/reparada; tejado de pizarra legible; coníferas reducidas con tocones; sombra suave móvil. Misma cámara de 50° y cuatro grupos FBX; no tocar Fase 2, main (salvo coordinación) ni reglas de juego.

### Evidencia y fallos de calidad, no confundir con éxito técnico

- **Primera prueba de correcciones:** Unity 6000.3.23f1, run [43834060945](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43834060945), SHA `237dfc52`, ZIP artefacto 14210471705, PASS técnico. Rechazo visual por tierra demasiado oscura/violeta, corrupción casi blanca y tejas con contraste cuadriculado.
- **Segunda prueba:** run [43835673589](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43835673589), SHA `b9acb3a7`, ZIP artefacto 14210836999, PASS técnico. Revisada la captura real: entorno excesivamente sepia, corrupción magenta y huecos cuadrados artificiales; NO art approved.
- **Tercera pasada de fuente preparada:** equilibrar mezcla de cielo y niebla a azul-gris neutro con sol ámbar; verdes más explícitos por vértice; cristal de paleta azul-violeta menos saturada; eliminar huecos geométricos artificiales del tejado (las tejas caídas siguen junto al edificio); suavizar fuerza de sombras a 0,62 y una luz local de antorchas a 0,52. Blender CI regenerado con FBX reimportado y UV por cara comprobadas; **no afirmar calidad final hasta examinar una captura Unity de este SHA**.

### Parámetros previstos de la tercera pasada (hasta que Unity los certifique)

| Efecto | Valor de fuente |
|---|---|
| Sol principal | Elevación 16°, Y -35°, intensidad 1,65, RGB (1; 0,735; 0,46) |
| Sombra | URP Soft, 2 cascadas, shadowmap 2048, distancia 42m, fuerza 0,62 |
| Ambiente cielo / horizonte / suelo | (0,42;0,40;0,42) / (0,29;0,31;0,38) / (0,16;0,20;0,22) |
| Sonda ambiental explícita | (0,28; 0,30; 0,33) |
| Luz local | Una sola Point Light, intensidad 0,52, alcance 3,5m; 2 antorchas emisivas |
| Niebla lineal | 20–58 m, RGB (0,36;0,38;0,43) |
| Niebla volumétrica fingida | Dos mallas con alfa de vértice, alturas 0,19 y 0,55m |
| Bloom | Threshold 1,05; intensity 0,21; scatter 0,66 |
| Color grading | ACES, contraste +3, saturación 0, sombras RGB (0,43;0,40;0,53), luces (1;0,81;0,59), balance -8 |
| Emisión cálida y corrupta | _Glow 1,75 y 1,35; corrupción únicamente en torre y entorno |

**Estado de aprobación:** pendiente. No hay prueba iPhone físico, performance, integración de gameplay, publicación, mezcla a main, ni permiso para Fase 2.
