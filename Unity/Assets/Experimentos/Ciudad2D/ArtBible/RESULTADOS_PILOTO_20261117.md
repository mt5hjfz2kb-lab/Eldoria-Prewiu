# Valoria 2.5D — Resultado verificable del cambio de método, 17/11/2026

**Decisión: ilustración IA con referencias = prometedora, PERO todavía NO certificada para producción. Mantener Blender como herramienta complementaria, no método obligatorio. ART_APPROVED=false.**

## Lo que está implementado y probado
- Seis imágenes maestras originales de Valoria copiadas como blobs inmutables (SHA-256); Biblia Visual, reglas materiales y receta versionadas en esta rama. [Verificación automática PASS 44037050637](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44037050637).
- Normalizador no destructivo de PNG RGBA con pivote de apoyo; rechaza por diseño un JPG con fondo opaco y sin máscara auditada. Su smoke-test sobre el Bastión maestro pasó en CI 44037050637.
- Resultados de la técnica anterior certificados: [Unity PlayMode 44028864959](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44028864959), [Blender PBR a Unity 44033293154](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44033293154). Esto NO certifica la nueva terna IA.
- Código de escena aislada City2DAIPilotGallery.cs preparado para comparación A/B 390x844, 1170x2532 y 844x390 con cámara y terreno bloqueados. Se niega a certificar si no hay 3 sprites RGBA individuales. **Código aún no compilado ni ejecutado en Unity tras añadirse.**

## Pruebas de generación de nuevos recursos, con fallos honestos
1. Forja — Runway nano-banana-2-lite, referencia maestra compuesta, task 512b4dd8-4197-474f-8758-d06ab28e06b0: imagen recibida en JPG. Pendiente de máscara transparente revisada e integración.
2. Observatorio — mismo modelo y hoja maestra, task 1aba8e1c-1d9c-4ae9-ab08-ec825470db9e: JPG recibido. Pendiente recorte RGBA e integración.
3. Archivo de Cartógrafos — motor gen-4-image-turbo: falló INTERNAL.BAD_OUTPUT.CODE01 en cuatro intentos con referencias. **No existe tercer sprite válido.**
4. Forja alternativa local obtenida con otro motor, imagegen_5edf67a4-08b3-4c16-b2f1-cf78b4f3678f: PNG con alfa validado localmente y SHA c60a82bd5a5b7895e1f46888f1ddb2c437ae86e5149e3ae06ca20ffb97149846. No usa el mismo procedimiento y no se ha probado dentro de Unity. Fuente disponible en ZIP de la conversación, NO en la rama GitHub.

**Bloqueo actual del experimento exacto de tres edificios:** se observaron 2 créditos restantes en la cuenta Runway; el modelo que sí produjo resultados cuesta 4 créditos a 1K. El motor alternativo más barato devolvió error interno repetido. No gastar ni pedir cargas automáticas.

## Conclusión comparativa sin favoritismo

| Criterio | Referencias + ilustración IA | Blender → sprites |
| --- | --- | --- |
| Riqueza artística | Bastión original notable; una Forja alternativa visualmente prometedora, **sin 3 nuevas pruebas equivalentes** | Dos modelos prerenderizados en Unity, estilo más plano y geométrico que los originales |
| Perspectiva e iluminación | Riesgo de deriva entre generaciones, pendiente de medir en 3 | Cámara, geometría y luces controladas en fuente |
| Reproducibilidad | Maestro+SHA+prompt+modelo+task documentados; no igualdad garantizada píxel a píxel | Fuente .blend, materiales, cámara y mallas regenerables |
| Transparencia y suelo | JPG sin alfa en ambos resultados Runway; una alternativa tiene alfa real pero no prueba integrada | RGBA y sombra separada demostradas, pero apariencia insuficiente |
| Juego y Unity | **0 capturas auténticas con la nueva terna** | Capturas A/B y PlayMode certificados del par anterior |
| Riesgo | generación inconsistente, máscaras, costes, transporte de archivos | esfuerzo 3D, monotonía de materiales, siluetas genéricas |

El nuevo método tiene potencial para mayor riqueza visual, pero no se demuestra viabilidad de catálogo reproducible. **No promover** hasta que los 3 edificios alcancen la Biblia Visual y exista comparación real en Unity; no comenzar Bastión 10/15 ni nuevos distritos.

## Pendientes estrictos
- Completar tercera generación bajo el mismo método y conservar 3 fuentes en binario.
- Obtener máscaras RGBA auditadas, recorte, sombra y pivotes; registrar SHA y versión de cada una.
- Transferir los 3 PNG originales a Sprites/AIPilot en esta rama; un enlace de tarea o una URL firmada expirable NO equivale a versionar el archivo.
- Ejecutar City2DAIPilotGallery.Capture en Unity con reserva vigente: 6 capturas de editor A/B y PlayMode por separado, sin alterar lógica ni guardados.
- Revisión artística independiente frente a Bastión V y secundarios y verificación real iPhone/Android si se decide promocionar.

**Estado final de esta revisión: TECH BIBLIA PASS / TEST THREE BUILDINGS INCOMPLETE / NEW UNITY A-B NOT RUN / ART NOT APPROVED. No se ha modificado main.**
