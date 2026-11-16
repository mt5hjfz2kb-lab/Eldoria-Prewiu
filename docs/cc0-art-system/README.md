# ELDORIA · SISTEMA ARTÍSTICO GRATUITO REPRODUCIBLE — contrato de producción

**Estado:** decisión de producción del propietario (16-11-2026); implementación experimental, **NO** aprobación artística de un build.  
**Workstream-ID:** `eldoria-cc0-art-system-20261116`  
**Rama única:** `experiment/eldoria-cc0-art-system-20261116`  
**Base inicial:** creada desde HEAD de main, 16-11-2026. No modificar `main`, `art/visual-reboot`, `experiment/nocturne-rebuild-bastion-i-v-20261115`, PR #196 ni ramas de beta.  
**Prioridad:** cero euros, licencia comercial acreditada, lenguaje único, integración verificable en Unity 6000.3.23f1 + URP 17.3.0, legibilidad y estabilidad en iPhone.  
**Fuente de verdad de prioridades:** propietario > AGENTS.md actual y decisiones posteriores > este contrato **solo para esta rama**. Si cambia de chat, el siguiente responsable debe leer el contrato, el manifiesto y el estado de evidencia antes de escribir.

## 1. Hipótesis y decisión visual

**Hipótesis:** una familia 3D estilizada única + compuestos específicos de Valoria + una única familia de UI alcanzan mayor calidad/coherencia y menor coste que ensamblar modelado heterogéneo o producir cada pieza desde cero. **No se considera probada** hasta la comparación visual y funcional en Unity.

- Siluetas sólidas, arquitectura modular escalonada, proporciones consistentes y detalle perceptible desde cámaras de móvil.
- Palacio/fortaleza y ruinas **adultos**: piedra pizarra #777D84 / sombras #353C49, madera #594436, musgo #566C58, luz ámbar #C69A62, corrupción #7951A1; saturación moderada, microvariación y desgaste controlados, no colores planos.
- Atardecer cálido (sol bajo), sombras azul-violeta, ventanas cálidas mediante emisivos, neblina ligera en distancia; máximo dos luces puntuales justificadas en piloto, resto emisivo. Evitar bloom quemado, textura fotográfica mezclada, monstruos infantiles.
- Valoria es reconstrucción en ruinas imperiales, no urbanización de casas ni tablero de parcelas visibles. **Prohibido reutilizar `Valoria_HeroBastion_v1.glb` vetado**, incluso si existe.
- **Un mundo 4X**, tres distancias de cámara con geografía y posiciones estables; La Brecha es fractura geológica, no torre/portal de fantasía colocado.

## 2. Asignación por área — decisión congelada para la prueba

| Sistema | Familia base | Transformación requerida | Cierre |
| --- | --- | --- | --- |
| Bastión I–V | Quaternius Ultimate Fantasy RTS + Ultimate Modular Ruins | Arquitectura propia compuesta: 5 niveles de misma fortaleza, silueta y reparación crecientes; puntos de interacción estables | Capturas I y V desde cámara móvil, diferencia importante, sin alterar guardados |
| Aserradero / Cantera | Ultimate Fantasy RTS + Fantasy Props MegaKit | Lotes de madera, poleas/herramientas, extracciones, trabajadores y variante visual | Legibles en vista cercana |
| Granero | Medieval Village Pack + props + cultivos CC0 cuando disponibles | Sacos, grano, barro, cubiertas, evolución | Legible y diferenciado |
| Cuartel | Ultimate Fantasy RTS + ruinas modulares | Patio, armas, guardias, estructura defensiva | No parece una vivienda |
| Forja / Hospital / Salón de Héroes | Medieval Village Pack + ruinas + props | Conjunto diferenciado y servicios identificables | UI y acceso coherentes |
| Terreno y zoom del mundo | Geometría propia Unity/Blender + Ultimate Stylized Nature | Terreno continuo, parches, caminos orgánicos, nodos; LOD/representaciones según zoom | Mismas coordenadas en 3 cámaras |
| Bosque / rocas / nodos | Ultimate Stylized Nature | Paleta master, escalas y repoblación visual | Reconocibles |
| Animales | Ultimate Animated Animals | Normalización, animación, distancia | Caza real; jabalí si falta se diseña específicamente |
| Héroes/ciudadanos/tropas | Universal Base Characters + Modular Character Outfits Fantasy + Universal Animation Library | **Una sola familia humanoide**, rig y escala; Aldric/Lyra/Maelis con identidad propia | Comprobar rig/retarget y gestos |
| Enemigos corrientes | Bestiary Dungeon Monsters (solo archivos gratuitos acreditados) | Sombra de corrupción, silueta propia | No simular contenido ausente del zip |
| Jefes y La Brecha | Geometría/VFX propios | Rasgos exclusivos, fisura integrada en suelo | Sin falso asset genérico |
| HUD/paneles/bolsa/buzón | **Cinder UI** como candidato único, con fallback UI propia/CC0 Kenney | Paneles 9-slice, botones y estados, tipografías Lora/Inter con licencias archivadas | No instalar Cinder en repositorio público si su licencia prohíbe redistribuir las fuentes |
| Cartas/Relicario | Arte aprobado existente + 3D Card Kit Fantasy cuando el archivo sea realmente gratuito | Rarezas, indestructible, N/S/E/O, ceremonia | No degradar los diseños aprobados |
| Inventario/objetos | Props Quaternius + render del modelo 3D | Mismas formas entre UI y mundo | Asegurar procedencia |
| Sonido | Kenney Interface Sounds/RPG Audio CC0; Sonniss previo examen licencia | Normalización y compresión | No introducir música Mixkit prohibida para juegos |

## 3. Licencias: protocolo bloqueante

- La documentación pública de Quaternius muestra **CC0 en muchas fichas históricas**, pero también existe **QAL v1.0 actualizada el 28-08-2026**: admite juegos comerciales y restringe redistribuir los assets sueltos. Por tanto, **la licencia del ZIP descargado concreto manda**. Guardar nombre, descarga, fecha, SHA-256, licencia interna, versión, URL fuente y evidencia privada antes de integrar.
- **Cinder UI:** declara uso comercial y modificación, pero prohíbe redistribución como asset pack. NO subir sus PNG/SVG originales a este GitHub público; solo integrar si se establece una ubicación privada de importación legítima y reproducible. Fallback: gráficos propios y Kenney CC0.
- **Quaternius y paquetes Unity Asset Store ya presentes:** no publicar ni añadir originales nuevos a GitHub si no existe autorización explícita para redistribuir fuentes. El archivo fuente prohibido no pasa a permitido por estar recortado, recoloreado o importado dentro del editor.
- **CC BY:** solo después de generar NOTICE y atribuciones. **CC BY-NC, licencias personales o sin constancia:** excluidas. Licencia del catálogo ≠ derecho certificado de un fichero concreto.
- Nunca marcar `license_certified=true` sin evidencia privada revisable. Permiso en el Player y permiso de publicar fuentes se registran **por separado**.

## 4. Reglas técnicas, integridad y aislamiento

1. Solo escritura dentro de `Unity/Assets/Art_Reboot/CC0System/`, `docs/cc0-art-system/`, `tools/cc0-art-system/` y `pipeline/art-production-requests/eldoria-cc0-art-system-20261116.json` **en esta rama**. No tocar escenas originales, `SliceBoot`, reglas de gameplay, sistemas de guardado, Build Settings ni scripts de otros workstreams.
2. Prototipo debe quedar en `Unity/Assets/Art_Reboot/CC0System/Scenes/CC0_Valoria_Sandbox.unity`. El Editor puede **generar** esta escena determinísticamente con el mismo comando, sin sobrescribir otras escenas. Usar un nombre único. Nada de autoejecución `InitializeOnLoad` ni cambios al abrir la beta.
3. Los modelos externos se importan SOLO después de auditoría; si no hay paquete validado, fabricar una **maqueta técnica original temporal** y etiquetarla claramente, nunca presentarla como arte final ni como uso real de pack.
4. Materiales maestros únicos, escala estándar 1 Unity unit = 1 m, pivotes y nombres estables; sin scripts de gameplay anexos. El mismo modelo no se duplica para versiones de nivel; usar variantes de prefabs y activación de grupos de construcción.
5. No tocar `main`, no fusionar, no publicar builds ni consumir créditos/recursos ajenos. Reservar runner Windows conforme al protocolo vivo y ejecutar solo cuando quede claro que está disponible.
6. Mantener identidad 3D original del Bastión prohibida a salvo pero no usarla; revisar `tools/asset-vault/find-reuse.py` e índices antes de nuevas mallas.

## 5. Orden de implementación

**E0. Contrato:** este README + `asset-lock.json` + request de workstream. Fecha, SHA base, fuentes, licencias, reglas, handoff.  
**E1. Primer autor real:** escena sandbox generada por Editor, ciudad cerca del Bastión con suelo, ruina, camino, arquitectura, follaje, luz; materiales, geometría y cámaras. Preferir asset validado; fallback geométrico **declarado**.  
**E2. Completar 5 estados de Bastión** con salto importante IV–V y misma base; edificios de recursos; sin lógica de juego alterada.  
**E3. Integrar fuentes gratuitas auditadas** en su canal permitido, personajes y animaciones compatibles, UI; sustituir mocks; registrar hash/versión.  
**E4. QA:** captura Unity 390x844, 844x390, 1600x900 y 1100x1250; comparar cercana/regional/panorámica; medir import warnings, render, draw calls y memoria; probar iPhone WebGL y Android físico cuando el entorno permita, sin presumir pass.
**E5. Revisión estética independiente:** verificar silueta, composición, contraste, coherencia de UI, ausencia de solapamiento e identidad de juego; iterar hasta umbral visual observable. Si dos rondas de microajustes no lo alcanzan, reconsiderar la técnica.

## 6. Criterios de aceptación y no aceptación

- **NO APROBADO** por existir C#, un .unity, CI verde, screenshot de Blender, arte conceptual generado, o assets localizados en internet.
- Aceptación del primer corte visual: screenshot **real Unity** de la escena y SHA exacto, prueba de que la cámara no recorta o confunde la fortaleza, diferencias V1/V5 comprobables, caminos y edificios legibles en vertical, sin mezclar estilos, fuentes rastreables y resultados de pruebas registrados. Luego pruebas funcionales.
- Entregable visual debe incluir capturas reales accesibles, URLs/referencias con HEAD, defectos abiertos y próximo paso autónomo.
- **No certificar iPhone** sin ejecución física/real del dispositivo.
- PR DRAFT únicamente si se verifica la titularidad y no existe PR para el mismo ID; incluir `Workstream-ID: eldoria-cc0-art-system-20261116`. Nunca merge automático.

## 7. Continuidad entre chats

Al reanudar: abrir `main/AGENTS.md`, `pipeline/active-workstreams.json` y PR abiertas, después **esta rama y los ficheros** `README.md`, `asset-lock.json`, `STATUS.md`. Comprobar HEAD y runs, no asumir permisos por historial. Continuar desde `STATUS.md` distinguiendo implementado, probado, pendiente. Respetar otras ramas de experimentos gráficos activas.

## 8. Fuentes primarias de documentación

- Quaternius Ultimate Fantasy RTS: https://quaternius.com/packs/ultimatefantasyrts.html
- Ultimate Modular Ruins: https://quaternius.com/packs/ultimatemodularruins.html
- Ultimate Stylized Nature: https://quaternius.com/packs/ultimatestylizednature.html
- Medieval Village: https://quaternius.com/packs/medievalvillage.html
- Fantasy Props MegaKit: https://quaternius.com/packs/fantasypropsmegakit.html
- Universal Base Characters: https://quaternius.com/packs/universalbasecharacters.html
- Outfits Fantasy: https://quaternius.com/packs/modularcharacteroutfitsfantasy.html
- Universal Animation Library: https://quaternius.com/packs/universalanimationlibrary.html
- Licencia Quaternius QAL: https://quaternius.com/license.html
- Cinder UI: https://ashbloomgames.itch.io/cinder-ui
- Unity 6.3 Asset Database: https://docs.unity.com/en-us/engine/6000.0/manual/assets-and-media/asset-database/customizing-workflow
- Unity 6.3 Prefab Utility: https://docs.unity.com/en-us/engine/6000.3/script-reference/unityeditor/prefabutility
