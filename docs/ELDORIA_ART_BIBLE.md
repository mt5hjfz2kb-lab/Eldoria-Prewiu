# Eldoria — Biblia de Arte y Assets

Estado: dirección artística canónica para la migración Unity  
Política actual de adquisición: **presupuesto 0 €**  
Ámbito: vertical slice Unity y base reutilizable de producción

## 1. Norte visual

Eldoria debe sentirse como una **fantasía medieval estilizada, seria, ligeramente oscura, decadente y épica**, con lectura clara en móvil.

No perseguimos fotorealismo ni el aspecto típico de un juego low-poly alegre. La referencia de calidad es un mundo con volumen, profundidad y atmósfera, pero con formas y siluetas suficientemente limpias para que edificios, unidades, recursos, amenazas y acciones se entiendan rápidamente en una pantalla pequeña.

### Valoria
Valoria es un gran reino caído que vuelve a levantarse, no una aldea medieval genérica. Debe comunicar:
- piedra antigua y envejecida;
- arquitectura con cierta monumentalidad;
- murallas, torres, desniveles, puentes y ruinas;
- madera y metal gastados;
- vegetación recuperando zonas destruidas;
- humo, fuego cálido, polvo y vida ambiental;
- profundidad mediante bosque, relieve, montañas y atmósfera;
- crecimiento físico visible conforme progresa el jugador.

### La Brecha
La Brecha debe romper deliberadamente el lenguaje natural de Eldoria. Su firma visual base es violeta/magenta y puede usar distorsión, partículas, iluminación anómala, corrupción del terreno y materiales imposibles. No debe reducirse a un portal morado genérico: debe sentirse como una herida que altera el mundo.

### Luz y legibilidad
La atmósfera puede ser oscura, pero el juego no debe ser visualmente negro ni perder información. Se priorizan contraste, jerarquía y lectura mobile-first. Los puntos interactivos y objetivos importantes deben destacar sin romper la ficción.

## 2. Principio de producción: Eldoria Art Layer

Los packs externos son materia prima, no la identidad final.

No se mezclarán assets de distintas fuentes sin un proceso de unificación. Todo recurso aceptado debe pasar, cuando corresponda, por una capa artística común:

**escala → silueta → paleta → materiales → desgaste → vegetación/props → iluminación → niebla/atmósfera → VFX → cámara → postprocesado.**

El objetivo es que el jugador vea Eldoria y no reconozca un collage de packs gratuitos.

Reglas:
1. Evitar mezclar modelos estilizados con materiales fotorrealistas sin adaptación.
2. Reducir saturaciones alegres o aspecto de juguete cuando contradigan el tono.
3. Reutilizar una familia limitada de piedra, madera, metal, tierra y vegetación.
4. Mantener proporciones coherentes entre arquitectura, personajes, props y caminos.
5. URP y rendimiento móvil son requisitos, no optimizaciones opcionales al final.
6. Atlas, materiales compartidos, LOD y batching deben considerarse desde la importación.
7. Un asset gratuito que no pueda integrarse visualmente es peor que una geometría simple bien dirigida.

## 3. Familias de assets necesarias

### P0 — necesarias para el benchmark de Valoria
- Arquitectura modular: Bastión, murallas, torres, puertas, puentes, casas, Cuartel, Granero, Aserradero, Cantera, Forja y ruinas.
- Entorno: terreno, caminos, rocas, acantilados, árboles, arbustos, hierba y fondo montañoso.
- Props/ambientación: escombros, cajas, barriles, carros, herramientas, banderas, antorchas, braseros.
- Materiales: piedra envejecida, madera oscura/gastada, metal, tierra, roca, musgo.
- VFX ambientales: fuego, humo, niebla, polvo/partículas.
- Firma inicial de La Brecha: fisura/corrupción, partículas, luz y material anómalo.

### P1 — necesarias para una slice jugable representativa
- Aldric y Lyra con siluetas diferenciadas.
- Soldados/tropas representativas.
- Habitantes/trabajadores básicos.
- Criaturas PvE representativas.
- Animaciones: idle, caminar, correr, ataque, impacto, muerte y trabajo/recolección.
- UI: marcos, botones, iconografía, recursos, paneles y tipografía coherentes.
- VFX de combate y feedback.

### P2 — ampliar después de fijar el lenguaje visual
- Variantes arquitectónicas y civiles.
- Bestiario amplio.
- Animaciones especializadas.
- FX avanzados de Brecha.
- Audio ambiental, combate, interfaz y criaturas.
- Sistemas visuales de Consejo, Alianzas, Oráculos, Puerto y demás capas post-prólogo.

## 4. Fuentes gratuitas preferentes

### Quaternius — base geométrica estilizada
Fuente preferente para geometría modular, naturaleza, props, personajes y animaciones cuando encaje. Candidatos iniciales:
- Medieval Village MegaKit / contenido gratuito disponible.
- Medieval Village Pack.
- Stylized Nature MegaKit / contenido gratuito disponible.
- Fantasy Props MegaKit / contenido gratuito disponible.
- packs gratuitos de personajes/fantasía compatibles con la dirección.
- librerías gratuitas de animación que sean técnicamente compatibles.

Uso: principalmente volumen, modularidad y prototipado de producción. **No asumir que el aspecto original del pack es el acabado de Eldoria.**

Licencia: verificar y registrar la licencia vigente de cada descarga antes de incorporarla al repositorio.

### Poly Haven — materiales, HDRI y apoyo ambiental
Fuente preferente para materiales PBR, HDRI y modelos concretos cuando sean compatibles con el estilo. Priorizar piedra, roca, tierra, madera, metal y superficies naturales.

Uso: elevar materiales e iluminación sin llevar el proyecto al fotorrealismo. Los materiales deben ajustarse a la dirección estilizada y al presupuesto mobile.

Licencia: CC0 en el catálogo publicado como tal; conservar trazabilidad de fuente.

### Unity Asset Store — uso quirúrgico
Solo assets gratuitos y únicamente si cubren un hueco concreto mejor que las fuentes anteriores. No construir la identidad de Eldoria alrededor de un único pack genérico. Registrar paquete, versión, licencia y función.

## 5. Política de coste y licencias

Durante esta fase:
- **No comprar assets.**
- No introducir dependencias de pago “temporales”.
- No usar contenido extraído de otros juegos, fan art, modelos sin licencia clara ni recursos cuya licencia comercial sea dudosa.
- Cada asset externo que pase a producción debe tener fuente y licencia registradas.
- Antes de gastar en el futuro, el benchmark visual debe demostrar qué carencia concreta no podemos resolver razonablemente con recursos propios/gratuitos.

## 6. Valoria Art Benchmark

Antes de extender arte a todo el juego se construirá una escena benchmark pequeña pero convincente que fije el lenguaje visual.

Debe contener, como mínimo:
- Bastión/pieza monumental reconocible;
- muralla parcialmente dañada;
- Cuartel;
- Granero;
- Aserradero;
- camino y transición de terreno;
- bosque/vegetación;
- relieve o fondo montañoso;
- ruinas y props;
- fuego/humo/atmósfera;
- una señal distante o localizada de La Brecha;
- escala humana mediante habitantes/soldados provisionales.

Criterio de salida: no basta con que la escena “funcione”. Debe poder mirarse y afirmar **“esto es Eldoria”** antes de multiplicar ese lenguaje por el resto del producto.

Una vez aprobada, sus reglas de escala, cámara, materiales, iluminación, paleta, densidad de props, vegetación, niebla y postprocesado pasan a ser baseline artística.

## 7. Flujo obligatorio para nuevos assets

Para cada incorporación:
1. Definir qué necesidad concreta cubre.
2. Buscar primero en la biblioteca gratuita aprobada.
3. Verificar licencia y compatibilidad técnica.
4. Importar en una zona de prueba, no directamente en escenas canónicas.
5. Evaluar escala, silueta, material, coste móvil y coherencia.
6. Aplicar Eldoria Art Layer.
7. Validar en la cámara y resolución objetivo.
8. Solo entonces promoverlo a asset de producción.

Registrar al menos: **asset / función / fuente / licencia / formato / adaptación requerida / coste móvil-LOD / prioridad / provisional o producción**.

## 8. Antiobjetivos visuales

Evitar explícitamente:
- aldea medieval genérica sin identidad;
- low-poly alegre/saturado como acabado final;
- fotorealismo incoherente con personajes o UI;
- assets reconocibles pegados tal cual desde varios packs;
- oscuridad que impida leer el juego;
- exceso de partículas/postprocesado que tape gameplay;
- construir todo el mundo antes de aprobar el benchmark;
- perseguir detalle de escritorio sacrificando rendimiento o claridad móvil.

## 9. Relación con la migración Unity

La consolidación Unity no debe usar primitivas como sustituto indefinido del arte. Las primitivas siguen siendo válidas para validar mecánicas, escala o arquitectura técnica, pero la siguiente referencia visual de producción es el **Valoria Art Benchmark**.

La arquitectura y los sistemas deben permitir sustituir assets sin reescribir gameplay. Arte, presentación y lógica deben permanecer desacoplados.

## 10. Decisiones todavía abiertas

No bloquear la producción por estas decisiones; resolverlas mediante el benchmark:
- paleta exacta y valores de iluminación;
- grado final de estilización de materiales;
- cámara/ángulo/FOV definitivos;
- densidad exacta de vegetación y props;
- shader final de La Brecha;
- apariencia final de héroes y criaturas;
- familia definitiva de UI e iconografía.

Estas decisiones se cierran por comparación visual dentro de Unity, no por texto aislado.


## 11. Destino de producto: aplicación móvil nativa (decisión del propietario, 2026-10-10)

**Eldoria es un videojuego para Android e iOS creado en Unity.** La compilación WebGL móvil y su enlace de GitHub Pages son un **canal auxiliar de pruebas y distribución rápida de candidatos**, no el producto final ni la plataforma que define por sí sola el techo de calidad artística. No crear un segundo juego: un solo proyecto Unity y variantes de compilación.

- Priorizar la dirección artística, interacción táctil, legibilidad y presupuesto de rendimiento de **aplicaciones móviles nativas**. Validar capacidades reales con builds y perfiles en dispositivos Android/iOS; no atribuir a nativo capacidades ilimitadas.
- Mantener WebGL como prueba útil de flujos, progreso, persistencia y acceso rápido. Una limitación específica de Safari/WebGL no justifica degradar globalmente assets ni rechazar SHARP/Gaussian Splatting u otra técnica que pueda funcionar en nativo. Registrar limitaciones y fallbacks por plataforma cuando corresponda.
- Diferenciar explícitamente en informes: **calidad artística nativa objetivo**, **funcionalidad WebGL verificada**, **rendimiento nativo medido** y **rendimiento WebGL medido**. No declarar equivalencias no probadas.
- Antes de cerrar una decisión de gran alcance sobre 3D, efectos o renderizado, probar una escena representativa en Android nativo y, cuando sea viable, iOS nativo; no suponer que una captura de WebGL demuestra desempeño nativo.
- El pipeline WebGL, las pruebas publicadas y la arquitectura de gameplay se conservan. Esta es una norma de producto y evaluación artística; **no autoriza** cambios inmediatos del proyecto, CI o lanzamientos ni modifica los ámbitos de trabajo activos.

### Tripo: productos diferentes y rutas existentes (verificación 2026-10-10)

No confundir las siguientes vías:

1. **Tripo Studio (aplicación web interactiva):** generación manual o asistida en el navegador, con créditos y suscripción **de Studio**. Es la ruta usada con el navegador del propietario. Eldoria ya tiene un puente Playwright/CDP por Edge y runner Windows: `.github/workflows/tripo-studio-bridge.yml`, `tools/tripo-studio-bridge.mjs`, `pipeline/tripo-studio-request.json`. Admite validación exacta, `stage_upload` sin gasto y una generación separada bajo autorización específica. La solicitud actual permanece `enabled:false`. Las últimas pruebas consignadas en ella fallaron durante el handshake CDP antes de generar: **puente implementado, disponibilidad operativa no certificada**.
2. **Tripo API / plataforma de desarrolladores:** acceso programático por API con **cuenta de facturación y créditos de API independientes**; los créditos Studio no pagan llamadas API. Eldoria dispone de la ruta de generación remota/CLI del pipeline canónico de módulos (`docs/TRIPO_MODULE_PIPELINE.md`, `pipeline/tripo-module-request.json`, workflow `tripo-module-pipeline.yml`). Distinguir ejecución programática autenticada de control de la web de Studio; no asumir saldo, suscripción ni equivalencia de modelos/precios.
3. **TripoSR de código abierto:** experimento de reconstrucción distinto del servicio comercial Tripo Studio/API. Conservar su resultado histórico de fallo visual para arquitectura principal; no usar ese FAIL para declarar rechazados los modelos comerciales recientes.

**Regla de producción:** evaluar por separado calidad del modelo, coste/saldo de **la ruta específica**, licencia comercial, automatización, fidelidad multiángulo, retopología/materiales, integración Unity y rendimiento Android/iOS nativo. No crear nuevos puentes sin antes recuperar y validar los existentes. Ninguna generación de pago se autoriza implícitamente. Los recursos de Studio Free son solo evaluación/no comerciales según la licencia actual. Consultar precios y condiciones justo antes de una prueba.

**Hipótesis para próximo gate:** Studio manual/asistido para iterar 1–3 props o módulos de Región 1 sin pagar API; si la calidad cumple, comparar con API para lotes automatizables. Reusar el pipeline Blender → Unity existente y sus gates visuales, sin alterar workstreams en curso. Una conexión CDP, exportación o TECH PASS no sustituye ART PASS ni prueba rendimiento nativo.
