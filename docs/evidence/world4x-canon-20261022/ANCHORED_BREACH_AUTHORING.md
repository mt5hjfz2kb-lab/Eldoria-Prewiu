# Brecha anclada independientemente

Problema: la pintura anterior invade Valoria y dibuja una red grande de grietas bajo las fracturas geométricas existentes. Dos correcciones generadas desplazaron el centro pese a pedir coordenadas exactas; se descartaron antes de integrar.

Cambio de técnica: fondo sin grieta y pigmento transparente separado. Ambos creados con imagegen integrado, sin créditos externos ni Qwen. Archivos fuente: `exec-0941e0b0-4532-4b89-a84c-fee75beae596.png` y `exec-1cc1b187-9fae-47fe-aa71-ac1a2f1fc205.png`. Prompts: borrar solo la cicatriz violeta del atlas existente; crear una fractura natural cenital y plana, ramas finas, corazón violeta y bordes transparentes, sin portal, objetos ni texto.

Corazón medido en (633,845;703,467) sobre RGBA 1254². Pivote UV (0,505458;0,439022) coloca ese rasgo en el punto real (8,61). Malla de suelo 16×16, 512 triángulos, ancho 100 unidades; sigue el relieve, excluye agua y atenúa el acceso protegido entre radios 32 y 48. Textura importada a 512 píxeles. Vista regional/panorámica utiliza este pigmento y oculta las fracturas antiguas duplicadas. Cerca/fallback conserva geometría anterior. No se añaden colliders ni objetivos jugables.

Prueba requerida: siete PlayMode existentes, aserciones del anclaje y de ausencia de doble fractura, capturas genuinas comparables. La validación física móvil sigue pendiente.
