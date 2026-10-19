# Eldoria — cierre eficaz sin perder calidad (V1)

**Decisión del propietario:** 19-10-2026. **Objetivo:** reducir fallos tardíos y recompilaciones innecesarias en el juego integrado. **Alcance:** apoyo metodológico; no sustituye AGENTS.md, el registro active, la base Unity canónica ni los gates artísticos/funcionales.

## No interferir con el desarrollo activo

- Única base jugable: la escena integrada de main (Valoria + Royal Gold + Zona1), con su workstream vigente. No reiniciar, sustituir ni invadir su trabajo. Presupuesto 0 €.
- No ocupar Windows ni alterar los requests del trabajo en curso. El ejecutor propietario debe verificar HEAD, claims y jobs antes de cualquier acción que use el runner.
- Este protocolo es de **lectura y comprobación**: no introduce nuevas escenas, ramas artísticas, pipelines alternativos, recompensas, economía ni APK. No cancela trabajos externos.
- La mejora solo se declara finalizada cuando los mismos gates de siempre quedan aprobados sobre el artefacto exacto: compilación, mecánicas, progresión Bastión I–II, guardados, UI/touch y orientaciones; arte, rendimiento, plataforma móvil y entrega publicada cuando correspondan. Ningún CI verde aislado basta.

## Secuencia operativa de cierre

1. **Congelar el alcance funcional.** Definir qué mejora concreta ve el jugador y qué checks corresponden. No agregar funcionalidades mientras se intenta cerrar el bloque, salvo corregir regresiones dentro del encargo.
2. **Identidad exacta.** Registrar SHA de fuente jugable validada, SHA-256 del artefacto candidato, versión/escena/paquetes y el test fallido. No mezclar resultados de compilaciones distintas.
3. **Preflight barato.** Antes de una reproducción cara, comprobar archivo y hash del artefacto, Playwright, Edge/Chromium, FFmpeg, sintaxis y condiciones de runner; para Unity/Android verificar paquete/módulos, rutas y firma con sus gates específicos existentes.
4. **Triaje de la primera evidencia.** Clasificar por separado errores de entorno (FFmpeg, índice Search de Unity), infraestructura/test (timing Canvas, área del gesto), compilación (CS1069, AndroidJNI), integridad (fingerprint), interacción funcional y arte. Una prueba fallida no demuestra automáticamente la causa raíz.
5. **Reparar la causa y repetir solo el test afectado.** Si cambia únicamente una prueba o herramienta y la fuente/bytes permanecen idénticos, reutilizar el mismo artefacto. Si cambia código, escena, assets o dependencias, reconstruir, comprobar nuevos hashes y pasar gates. Nunca simular un toque usando Button.onClick.Invoke ni quitar aserciones.
6. **Después ejecutar la regresión completa y publicar únicamente si está autorizada.** Mantener la prueba a velocidad real y en ambas orientaciones; verificar guardados, límites de escena, menús, diálogos, navegación y progreso. Un PASS focalizado no cierra todo el juego.
7. **Cierre demostrable:** registrar HEAD, run, SHA del artefacto, log/vídeo/capturas, estado de cada gate y pendientes. Ante fallo, OPEN/BLOCKED con diagnóstico. Dos fallos de la misma hipótesis requieren revisar esa hipótesis, no un tercer retry idéntico.

## Herramienta nueva, read-only y sin Windows

Se incorpora el CLI de biblioteca estándar Node:

~~~bash
# Clasifica observaciones reales en un log existente.
node tools/closeout/closeout.mjs triage --log /path/to/job.log --change qa_only

# Alternativa: informe JSON del recorrido automatizado.
node tools/closeout/closeout.mjs triage --report /path/to/browser-result.json --change product

# Comprobaciones ANTES de recorrer WebGL: hashes reales y herramientas locales.
# --artifact acepta UN fichero, no un directorio extraído.
node tools/closeout/closeout.mjs preflight-browser \
  --artifact /path/to/exact-candidate.zip \
  --sha256 HASH_REAL_DE_64_CARACTERES \
  --source-sha COMMIT_CANDIDATO_40_CARACTERES \
  --validated-source-sha COMMIT_JUEGO_VALIDADO_40_CARACTERES \
  --script tools/valoria-reference-clean/playtest_web.cjs

# Tests ligeros del clasificador sin Unity ni descargas.
node --test tools/closeout/closeout.test.mjs
~~~

- Resultado de preflight: READY_FOR_TARGETED_QA_NOT_CERTIFIED o BLOCKED_BEFORE_EXPENSIVE_QA (exit 2). Solo mide requisitos **en la máquina que lo ejecuta**; NO prueba que el runner remoto esté libre.
- Resultado de triage: categorías y siguiente comprobación; no es un juicio definitivo de causa raíz. No lanza jobs, no edita estado, no genera una aprobación artística ni permite release.
- La opción --change qa_only declara una intención, NO verifica el diff por sí misma. Confirmar siempre los cambios reales en Git antes de reutilizar una compilación.
- El SHA-256 esperado tiene que venir de la misma evidencia del artefacto que se va a probar. No adivinarlo ni usar el SHA de un ZIP para un fichero extraído. El SHA de juego validado no es necesariamente el último HEAD documental de main.

## Decisión por tipo de fallo

| Evidencia real | Primer paso | ¿Nueva build? |
| --- | --- | --- |
| FFmpeg, Playwright, Editor Search corrupto | Reparar el entorno/acceso; repetir la prueba fallida | No, si los bytes del juego siguen iguales |
| Touch/ratón no regresan a la ciudad | Capturar punto, Canvas, raycast, evento, vídeo y estado antes/después del MISMO WebGL | Solo si se modifica código jugable |
| Android CS1069, módulo JNI | Corregir paquete o fuente y ejecutar compilación Android real | Sí |
| Fingerprint o SHA del artefacto distinto | Detener promoción, conciliar los bytes/dependencias | Según cambio real; no aceptar diferencia sin prueba |
| Arte visible pero no mejor | Mantener último asset aprobado y corregir causa, Unity A/B | Solo para un candidato validado |
| Prueba específica verde | Ejecutar las demás puertas de entrega, no declarar CLOSED directamente | No, si es idéntico el artefacto |

## Contexto concreto del 19-10-2026

La ejecución Windows Chromium [39237921230](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39237921230) mantuvo el WebGL exacto, mostró zoom correcto, pero registró Return to city: timed out after 10s tras un touch-hold-release de 160 ms en vertical. El estado seguía inWorld=true; ese log no registró errores de runtime ni de red. **Diagnóstico inicial INTERACTION_UNRESOLVED.** No corresponde declarar bug de Unity o fallo del test sin cotejar eventos reales/raycast y comparación touch vs mouse sobre esos mismos bytes.

Este procedimiento puede aplicarse al siguiente paso pertinente del trabajo propietario, sin cambiar la tarea vigente, sin exigencia de relanzar pruebas ya superadas y sin enviar automáticamente instrucciones a otras conversaciones.
