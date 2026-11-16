# Fase 2 — Kit modular. Entrega real y evaluación pendiente

**Estado:** implementación y pruebas técnicas completadas. **Aprobación artística del propietario pendiente; no entrar en Fase 3 ni fusionar con `main`.** La Fase 1 está aprobada y sus edificios/atlas históricos permanecen intactos.

## Fuentes y certificación

- **Rama aislada:** `art/visual-reboot`. Fuente original del render: `805ac1a333e0bab076ac0794e422f2ef3d2ecf1c`. Commit del render y escenas importadas `3e1d8003a8347be123bf58a2caca6a730f2b321a`.
- **Blender:** 31 modelos FBX originales en `Kit/Models/`, generados mediante `Kit/Source~/author_modular_kit.py`, con `Kit/Source~/modular_kit.blend` y verificaciones independientes `Kit/Source~/verify_kit.py`. Total **37.968 triángulos en 31 piezas no instanciadas**. Ocho familias de catálogo; más de 15 variantes/funciones solicitadas.
- **Módulos:** cuadrícula 4m, origen en base `(0,0,0)`, biseles, variaciones tonales por vértice. **Solo tres materiales de kit** `Kit_Surface`, `Kit_WarmLight`, `Kit_Corruption`, atlas histórico exacto `Phase1/Textures/Hero_Atlas_256.png` de **256×256**, SHA-256 `2ada56d94ed8e3f3c690fa787a73a0cd38d986d8997c84e125c1cd66b84c0efe`. UV fraccionales interiores, mismo swatch por polígono tras exportación e importación FBX.
- **Unity:** ejecución genuina [43906347228](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43906347228) **SUCCESS**, Unity `6000.3.23f1`, original artefacto GitHub `14246505598`. Prueba técnica `Source~/Evidence/native-43906347228/kit-native-proof.json`. **61 instancias arquitectónicas**, **74.534 triángulos instanciados en edificios** (no confundir con 37.968 triángulos de catálogo).
- **Escenas Unity commit real:**
  - `Kit/Scenes/Phase2_Kit_Grid.unity`: **31 piezas alineadas en cuadrícula 8×4**.
  - `Kit/Scenes/Eldoria_Kit_Sandbox.unity`: casa intacta con torre/campana, taller distinto de tejado bajo/cobertizo/chimenea, edificio dañado con torre rota/cristales/raíces. **Solo instancias Kit FBX para los tres edificios**; 44 pinos en tres alturas, claros y senderos conectados.
- **Cámara e iluminación:** en la captura de poblado, inclinación **42°**, yaw **−13°**, coherente con Fase 1. Luz direccional ámbar baja, relleno azul-violeta, niebla distanciada, cielo degradado. La imagen fija valida **render en Unity**, no FPS ni animación de gameplay en dispositivo físico.

## Evidencias originales en la rama

`Assets/Art_Reboot/Kit/Source~/Evidence/native-43906347228/`

- `kit-grid-2400x1350.png` — kit completo (PNG auténtico de Unity)
- `village-1600x900.png` — nueva escena con cámara aprobada (PNG auténtico de Unity)
- `village-1100x1250.png` — vista vertical de la escena
- `corrupt-kit-detail.png` — detalle del edificio corrupto
- `kit-polygons.csv` — lista real por pieza, 31 entradas y 37.968 triángulos
- `kit-grid-index.csv` — nombres y posiciones (fila/columna) para la cuadrícula
- `kit-native-proof.json`, `native-proof.json` — versiones, SHA, protecciones, conteo y procedencia de archivos
- `*.png.metrics.txt` — rango de luminancia y porcentajes de colores de cada captura

**Observación visual real:** la tercera revisión corrigió los mosaicos UV, el velo rosa (el área verde pasó de ~3,8 % a ~40,8 % bajo la métrica reproducible), la conexión entre casa/taller/ruina y los arcos de puertas y ventanas. El conjunto conserva intencionadamente piezas modulares y, comparado con el edificio héroe de Fase 1, sigue mostrando un lenguaje más repetitivo; **no declarar calidad artística premium ni aprobación del usuario** sin su valoración. Probar kit en una escena real no acredita rendimiento móvil, gameplay, ni integración en `main`.

**Coordinación:** recurso Windows devuelto a Ashen inmediatamente después de la ejecución [43906347228](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43906347228), con CAS en `main:pipeline/active-workstreams.json`; no alterar otros experimentos.

## Regla de siguiente fase

Esperar aprobación artística explícita. En caso de revisión de estilo, modificar solo presentación y piezas en esta rama, ejecutar Blender FBX gate y luego prueba real de Unity antes de entregar revisión. Sin autorización, NO abrir Fase 3, NO publicar, NO sustituir la Fase 1 ni fusionar con `main`.
