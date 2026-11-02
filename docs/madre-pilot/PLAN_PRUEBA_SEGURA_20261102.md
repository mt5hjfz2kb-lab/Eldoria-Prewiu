# MADRE + Eldoria — piloto seguro, coste adicional 0 €

**Estado (2026-11-02): PREPARADO; NO INSTALADO, NO EJECUTADO.**
Responsable: propietario de Eldoria. Coordinación: issue #143. Rama aislada `pilot/madre-safe-readonly-20261102`.
La PR #136 / bloque héroes es prioritaria y reserva `windows-runner-heavy`. NO usar la máquina hasta que el registro canónico y la cola real confirmen liberación.

## Por qué probar MADRE
MADRE es un coordinador de agentes, no otra IA. Potencial: memoria compartida, consulta cruzada entre agentes, menos copias de contexto. No garantiza más velocidad, calidad visual o que Unity funcione.
Fuente oficial 1.1.1 verificada al preparar: https://madre.run/ ; https://github.com/jossuealcacao-exe/madre .
Windows funciona mediante WSL2; Node >= 22.5 dentro de la distribución; se requiere al menos una cuenta compatible autenticada. La interfaz local se sirve en 127.0.0.1:4317 (o siguiente puerto libre).
El programa es gratis para persona/equipo de <=10, pero los proveedores de IA pueden tener costes o cuotas. El instalador Windows aún no está firmado (no permitir bypass automático de SmartScreen).

## Preparación disponible AHORA sin afectar a nadie
- Script de diagnóstico estrictamente **solo lectura**: `tools/madre-pilot/windows-readonly-preflight.ps1`. Revisa Windows, memoria libre/total, disco, CPU, presencia de wsl.exe/Node, y procesos Unity/Blender; solo muestra JSON. No instala, no modifica, no reinicia, no prueba conexión a Internet.
- Repositorio de prueba: **no clonar completo** Eldoria. GitHub indica tamaño del repo ~15 GiB. Usar únicamente documentación y pocos scripts C# recuperados en una carpeta independiente, desde GitHub o sparse checkout con filtros, según espacio real.
- No crear PR ni modificar la rama activa.

## PUERTAS OBLIGATORIAS

### P0 — Antes de usar Windows
1. Leer `pipeline/active-workstreams.json` en **main actual**, confirmar que `windows-runner-heavy` NO está reservado.
2. Confirmar en la cola de GitHub Actions que no haya trabajos nativos Unity activos/pendientes. El registro no basta por sí solo.
3. No tocar checkout, cache Unity, Blender, Modelos Tripo, claves de Android, configuración de runners.
4. Si cualquiera de 1-3 falla: STOP. No ejecutar el piloto ni instalar nada.

### P1 — Diagnóstico de solo lectura
1. Una vez libre el runner y con el mismo contexto operativo autorizado, ejecutar `windows-readonly-preflight.ps1` en PowerShell **sin elevar permisos**, fuera de cualquier carpeta de Unity.
2. Solo publicar resumen de RAM, disco, CPU, WSL y versiones. NO publicar nombres de usuario, rutas privadas, variables de entorno ni secretos.
3. Si RAM disponible o disco son insuficientes, o Unity está ejecutándose: STOP.
4. Confirmar versión Windows y si WSL2 ya está configurado; la presencia de wsl.exe por sí sola NO confirma que WSL esté activo.
5. No se ha ejecutado aún este diagnóstico en el ordenador real.

### P2 — Seguridad de instalación (requiere nueva aprobación)
1. Revisar en el origen oficial la versión **vigente en ese momento**, su release/paquete y archivos; comprobar integridad antes de ejecutar.
2. Si WSL2 necesita habilitar características o reiniciar, solicitar autorización explícita y programar fuera del trabajo Unity.
3. NO ejecutar instalador unsigned con `Run anyway` sin revisión de procedencia/defensa; preferir ruta manual WSL2 solo tras control de procedencia.
4. No activar actualizaciones automáticas, marketplaces, telemetría opcional, API facturable o conexiones externas innecesarias sin consentimiento.
5. Mantener MADRE atado a localhost. NO exponer puerto 4317 en GitHub, router, proxy abierto ni reenvíos públicos.
6. Usar una carpeta sandbox con copia mínima sin secretos y permisos `solo lectura`. NO abrir con modo edición/ejecución total.

### P3 — Prueba con UNA IA
Ejercicio fijado para comparar: auditar (solo leer) 2-3 scripts C# de interfaz + pruebas asociadas, en una copia pequeña de Eldoria.
Salida exigida: 3 problemas o mejoras **verificables con archivo/línea y prueba reproducible**; 0 cambios aplicados; 0 datos inventados.
Se compara contra revisión equivalente hecha sin MADRE, en el mismo alcance.
Medir: tiempo para entregar, errores reales, falsos positivos, claridad de referencias, consumo de cuota y gasto euros. Capturar solo estadísticas no confidenciales.

### P4 — Prueba con DOS IA, solo si P3 pasa
Añadir únicamente un segundo agente si su acceso está realmente disponible con coste extra 0 €. No asumir gratis Claude Code o Gemini CLI.
Encargo: agente A encuentra defectos; agente B contrasta y rechaza falsos positivos; salida de texto, ninguna edición.
Si alguna IA requiere pago o cuenta con tarjeta: saltar el agente y mantener prueba con uno.

### P5 — Decisión final
Aprobar piloto SOLO SI:
- Ninguna interferencia con Unity/Windows ni con tareas activas;
- Facturación adicional 0 €;
- Sin secretos ni cambios en juego, GitHub main, PR136 o ejecutor;
- Por lo menos un hallazgo útil comprobable y mejor coordinación que proceso anterior.
En caso contrario: cerrar piloto con veredicto NO INCORPORAR y desinstalar solo la instalación aislada con aprobación; no borrar cuentas, repositorios o WSL ya existente.

## Acceso desde iPhone
No hay garantía de aplicación móvil MADRE ni servicio 24/7. Primero validar que la interfaz local funciona. Acceso externo SOLO mediante escritorio remoto seguro ya configurado o VPN autenticada; no abrir MADRE a Internet. Requiere que Windows permanezca encendido.

## Qué NO se ha hecho
- Ninguna instalación de WSL, Node, MADRE ni agentes.
- Ninguna ejecución PowerShell sobre el Windows doméstico.
- Ninguna autorización de cuentas ni gastos.
- Ninguna prueba real de interacción MADRE/Unity.
- Ningún cambio en main, PR136 ni workstreams canónicos.

## Fuentes
- https://madre.run/
- https://madre.run/manual/
- https://madre.run/cambios/
- https://github.com/jossuealcacao-exe/madre
- https://learn.microsoft.com/en-us/windows/wsl/install
