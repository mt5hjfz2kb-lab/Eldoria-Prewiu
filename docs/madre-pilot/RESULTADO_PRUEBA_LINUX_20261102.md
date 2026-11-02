# MADRE · Resultado real del primer ensayo aislado (2026-11-02)

## Veredicto
**PASS de arranque y HTTP local en Linux temporal GitHub; NO CERTIFICADO para Windows ni para uso multiagente.**

- Fuente ejecutada: paquete npm `@jossuealcala/madre@1.1.1`.
- Máquina: GitHub-hosted `ubuntu-latest`, Linux efímero, separado del PC doméstico. Comprobación explícita de `RUNNER_ENVIRONMENT=github-hosted` y `RUNNER_OS=Linux` aprobada.
- Node: v22.23.4, disponible en job.
- Comando `npx --yes @jossuealcala/madre@1.1.1 doctor`: la herramienta inicia, reconoce versión 1.1.1 y enumera Codex, Claude, Gemini, OpenCode y Cursor. Su salida 1 avisa correctamente que no hay ninguna cuenta/agente conectado en la máquina temporal; NO indica fallo de carga de MADRE.
- Comando `npx --yes @jossuealcala/madre@1.1.1 start --no-open --project <directorio_temporal>`: interfaz HTTP local accesible en `127.0.0.1:4317` durante el job; marker `MADRE_LOCALHOST_HTTP_PASS`.
- Ejecución final: **SUCCESS**: https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/41426500624
- Dos ensayos iniciales `41426269971` y `41426393170` fallaron porque se trataba como error el diagnóstico sin agentes (y primero no se capturaba su log). El código se ajustó y el ensayo final pasó. No atribuir esos fallos al instalador Windows ni a Unity.
- Repo público + runner hospedado estándar = sin coste por minutos de ejecución, según documentación oficial https://docs.github.com/en/actions/reference/runners/github-hosted-runners .
- No hubo inicio de sesión, acceso a repo del juego en el entorno de ejecución, modificación de archivos de Eldoria, prueba con IA reales, compra ni facturación de API. No es una certificación de seguridad integral del paquete.

## Prohibición estricta de reinicio de PC
El workflow utiliza `runs-on: ubuntu-latest`, **jamás** el runner autohospedado Windows. No contiene `wsl --install`, `Restart-Computer`, `shutdown` ni habilitación de componentes Windows. No se ejecutaron comandos PowerShell en el PC.
Confirmación posterior: a esta comprobación GitHub aún presenta pruebas UI en progreso/pendientes y el registro canónico vuelve a reservar `windows-runner-heavy` al trabajo de fidelidad de héroes. NO realizar instalaciones ni diagnósticos sobre el PC mientras persista dicha situación.

## Próximas puertas
1. Esperar liberación auténtica de reserva Windows y ausencia de ejecuciones pendientes/nativas; **no enviar trabajo que compita por el runner**.
2. En una ventana de mantenimiento separada, ejecutar solo `tools/madre-pilot/windows-readonly-preflight.ps1`; ninguna activación de WSL, ninguna instalación ni reinicio.
3. Solo si existe WSL2 ya activo y hay margen de memoria/disco, evaluar sesión de prueba MADRE SIN acciones de Windows que pidan reiniciar, tras aprobación explícita.
4. Incorporar una IA con permisos de solo lectura en un ensayo auténtico; repetir con dos agentes solo si la segunda es gratuita en el uso concreto. No dar acceso a carpetas de Unity compartidas hasta aceptación.

## Lo que todavía NO se ha demostrado
- Que el Windows doméstico tenga WSL2 preparado, RAM/disco adecuados o sesión de acceso.
- Que MADRE pueda coordinar de verdad dos IA sobre Eldoria.
- Que mejore calidad gráfica, velocidad o integración de Unity.
- Que su uso remoto desde iPhone esté configurado y sea seguro.
