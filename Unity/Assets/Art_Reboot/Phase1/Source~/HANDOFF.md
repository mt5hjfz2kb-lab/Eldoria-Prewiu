# Visual Reboot — piloto real Unity, 16 noviembre 2026

## Resultado y alcance

Rama única: `art/visual-reboot`. Piloto original de tres estados: intacto, ruina y corrupción. No es Ashen Citadel I–V, no integra mecánicas nuevas y no modifica main salvo coordinación autorizada.

Run nativo **43779933745**, fuente **d74be4ee7d90c07a11becc62c480f37358596e0e**, Unity6000.3.23f1 / URP17.3.0: **SUCCESS**. Editor compila, importa, captura cuatro resoluciones y cierra con código0; limpieza de su scratch verificada. Artefacto14188480494, ZIP SHA256 `3d8174fcee473165bcf612de067e9a9c3df885af1eec1bc8b62ce16a6e3ebc09` descargado y verificado.

Las cuatro PNG originales y pruebas están en `Source~/Evidence/native-43779933745/`. La repetición de1100×1250 al final coincide píxel a píxel con la primera (diferencia media/máxima0). Inspección visual realizada en1100×1250,390×844,1600×900 y844×390: los tres edificios completos, sin atlas extendido, cuadrados de sombra en tejado ni suelo cuadriculado. No sustituir estas imágenes con renders Blender.

## Abrir y reproducir

Escena montada: `Assets/Art_Reboot/Phase1/Scenes/Eldoria_Visual_Sandbox.unity`. Materiales, perfil de volumen, renderer y pipeline propios incluidos bajo `Materials/`. La jerarquía histórica copiada permanece inactiva; la escena original mantiene sus bytes.

Para reproducir el aspecto certificado, usar `Art Reboot/Build and capture phase 1`, que selecciona temporalmente el pipeline propio y restaura el anterior. Abrir sólo la escena utiliza el pipeline activo del proyecto y no garantiza el mismo postprocesado. El menú exige escenas guardadas y Edit Mode; no se debe desactivar el guard de Play/build para eludir V4.1.

Fuente: .blend + FBX + GLB, cuatro familias de malla,21865triángulos, atlas256² y dos materiales, colores por vértice, cámara50°. Fuente FBX reimportada en Blender con UV por cara contenida en una sola casilla y todas las caras triangulares. Transferencias binarias verificadas por hash Git.

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

No probado: rendimiento móvil físico, Play/WebGL/Android, integración completa del juego ni partículas. El laboratorio conserva intencionadamente fuera las dependencias de gameplay/V4.1. Esas verificaciones no se infieren de capturas de Editor.

## Coordinación

Windows se libera al terminar cada ejecución; conservar propietario y fuentes de Ashen Citadel. Releer registro vivo y ejecuciones antes de reclamar un nuevo turno; no desplazar un retry ya en cola. No crear supervisiones periódicas ni mezclar ramas. La autorización de alternancia y coordinación en main ya fue concedida.
