# Eldoria — contrato artístico y técnico 2.5D (candidato)

**Estado:** propuesta de producción, no decisión de migración. **Origen:** experimento aislado `art/ciudad-2d-hitos-1-5-20261116`. **No merge**, no beta nueva y sin cambios en guardados, comandos, economía o combate. Los PNG y las capturas de navegador no certifican Unity/iPhone.

## 1. Identidad que debe sobrevivir al cambio de técnica
Fantasía medieval adulta, arquitectura construible (piedra cálida, madera, pizarra azul-gris, hierro, azul/oro heráldico), luz de atardecer, humo y antorchas discretos, heridas de guerra y amenaza geológica violeta de **La Brecha**. La ciudad es un refugio que se reconstruye, no un tablero vacío ni un collage de imágenes. Se exige silueta diferenciada por función y variación orgánica de calles, sombras y terreno.

## 2. Autoridad funcional: no dibujar edificios antes de tiempo
El estado canónico proviene de `ICommandGateway.Snapshot()` / `PlayerState`; la presentación solo lee. Mantener la conversión `City2DProgressionPolicy.FromSnapshot`, sin duplicar datos ni temporizadores. El nivel real mejora siempre; el sprite cambia **únicamente** en umbrales 1,5,10,15… con arte producido.

| Edificio | Nivel de Bastión que lo permite | Estado en pantalla antes de terminar |
|---|---:|---|
| Bastión | 1 | visible desde el inicio; ilustraciones para 1–4 y 5–9 |
| Aserradero | 1 | parcela disponible; obra en curso; construido tras finalizar |
| Cuartel | 2 | bloqueado hasta B2; después parcela/obra/construido |
| Granero | 3 | bloqueado hasta B3; después parcela/obra/construido |
| Cantera productiva (`stoneworks`) | 5 | bloqueada hasta B5; después parcela/obra/construido |
| Salón de Héroes | sin nivel propio de edificio | **no instanciar como edificio construido** sin un contrato canónico verificable |

Los cuatro estados `Locked / Available / UnderConstruction / Built` se expresan con el mismo layout tanto en navegador como en Unity. Los escenarios de muestra por capítulos no son guardados reales.

## 3. Escena y expansión de territorio
- Fuente espacial: `Unity/Assets/Experimentos/Ciudad2D/QA/city_layout.json` (941×1672; coordenadas en píxeles canónicos). Todos los frontends consumen **la misma geometría y estados**.
- Terrenos A/B: `Terreno.png` preservado, `Terreno_Organico.png` candidato. No convertir el fondo en textura estirada ni pintar sobre los sprites de edificios.
- Para ampliar Valoria, añadir **distritos de terreno nuevos**, con su propia textura/atlas, origen en coordenadas de mundo, geometría de transición y colisión/selección independientes. La cámara debe calcular límites de la unión de los distritos activos y restringir el zoom al contenido que existe. No renderizar distritos reservados sin arte real.
- Mantener separados: fondo; edificios; parcelas/obras; calles y vegetación modular; objetos animados; efectos; UI. El orden visual se basa en el punto de apoyo/"pies" de cada sprite.
- Optimización móvil: atlas por distrito/uso, texturas dimensionadas a la resolución visible, mipmaps según zoom y dispositivo, perfiles de compresión comprobados en iPhone/Android, no asumir que bajar PNG en disco reduce RAM de GPU.

## 4. Pantallas: técnica prevista, sin adelantarse a implementarlas

| Pantalla existente o prevista | Sistema visual 2.5D propuesto | Dependencia crítica |
|---|---|---|
| Ciudad y gestión de edificios | sprites isométricos; paneles compactos 9-slice | niveles, construcción, recursos y guardado reales |
| Mundo 4X (cerca/región/panorámica) | mapas modulares, niebla, nodos, marchas y enemigos con capas | estados, navegación, reposición y zoom canónicos |
| Héroes (Aldric, Lyra, futuro tercer héroe) | retratos 2D de alta calidad; animación localizada | experiencia, desbloqueos y composición |
| Cuartel, tropas, formación e informes | fichas de tropas, iconografía, animación táctica breve | combate automático, tipos T1–T5, heridas/PvE/PvP |
| Relicario y cartas | ilustraciones por carta + marco por rareza + efectos de ceremonia | obtención por misión, Indestructible, valores y usos |
| Vigilantes y Exploradores | paneles ilustrados y tableros con hitos; sellos | progreso y recompensas del gateway |
| Buzón, bolsa, tesoro diario | kit 2D de superficies, monedas/recursos y estados | idempotencia de cobros y visibilidad persistida |
| Tutorial, nick, objetivos, HUD | componentes compactos, guía contextual y jerarquía consistente | acciones/iconos reales, safe area móvil |
| Alianzas, naval y demás expansión | preparar solo gramática visual, no producir contenido ficticio | contrato de gameplay aún no disponible |

## 5. Evaluación antes de migrar
1. **Arte:** comparar A/B en exactamente cuatro resoluciones, cámara y capítulos idénticos; inspección humana de bases, perspectiva, densidad, escala, paleta y lectura táctil. No basta con que el PNG exista.
2. **Unity nativo:** importa y compila `6000.3.23f1`; seis objetos reales; secuencia I–V; cambio de sprite 4→5; parcelas canónicas; captura RenderTexture con manifiesto y SHA. Diferenciar estas capturas de PlayMode.
3. **Interacción:** PlayMode/táctil sobre cada edificio y parcela habilitada; deshabilitar áreas bloqueadas; HUD no tapa los objetivos. Sin acciones falsas ni permisos de construcción inventados.
4. **Memoria y dispositivo:** perfil en WebGL, iPhone Safari y Android físico; memoria de texturas, fps, zoom, ciclo de vida y entradas/salidas sin reinicios.
5. **Separación:** no cambiar `main`, las escenas de Valoria reales, `ProjectSettings`, códigos de economía/combate ni publicar una beta hasta revisión.

## Defectos abiertos y pruebas conocidas
- Las bases preilustradas de algunos edificios aún parecen elevadas sobre el terreno; unificar sombras/contacto y escala, **sin ocultar el defecto con filtros globales**.
- El terreno orgánico tiene mayor riqueza visual, pero su iluminación puede diferir de ciertos edificios; comparar en capturas Unity y dispositivo.
- El Salón no tiene contrato de construcción independiente.
- La extensión occidental/oriental/norteña existe solo como reserva en JSON, no como terreno construido.
- Pruebas de navegador y validaciones de archivos no son pruebas nativas de Unity ni una aprobación artística.

**Criterio de promoción:** no reconstruir todas las pantallas hasta superar primero el piloto ciudad con prueba física, estabilidad y calidad visual aprobada.
