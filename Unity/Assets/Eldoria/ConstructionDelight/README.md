# Eldoria / Construction Delight v1 — candidata no aprobada artísticamente

## Qué implementa
Actividad visual en los edificios reales de Valoria: polvo fino, virutas y apertura discreta al comenzar una obra; movimiento ligero durante la obra; al finalizar, polvo, motas de oro envejecido, un aro suave y un realce cálido que dura aproximadamente 1,7 segundos.

El componente ValoriaConstructionDelightV1 se incorpora automáticamente al host jugable de la ciudad. Lee el mismo LocalGateway, PlayerState y modelos/colisionadores existentes. No cambia el gameplay, el HUD, el tiempo de las obras, el esquema de guardado, modelos, materiales del edificio ni las escenas. La primera candidata usaba `ParticleSystem`, pero se sustituyó por siete emisores de sprites en mallas dinámicas agrupadas (`MeshRenderer`), con shader URP propio y texturas procedurales pequeñas; gasto cero. Esto evita añadir el módulo `UnityEngine.ParticleSystemModule`, que no está habilitado en el manifiesto canónico, y no modifica los archivos que controla la PR #136.

La lógica ConstructionDelightState reconoce construcción inicial del aserradero y cuartel, las mejoras de Bastión y las obras/mejoras c3 de aserradero, cuartel, granero y cantera. Solo dispara la celebración cuando aumenta el nivel real. Una partida cargada a mitad de obra reanuda la actividad, pero no muestra inicio ni finalización falsos. Desde mundo 4X el efecto se mantiene pendiente durante un breve plazo y no se dibuja sobre el mapa.

## Estrategia de calidad
Inspiración: reparación medieval y renacimiento de Valoria. Colores ocre, polvo, ámbar y oro envejecido; sin flashes blancos, confeti, hologramas gigantes ni sacudidas de cámara. Los efectos se centran en las posiciones auténticas de los edificios; son de baja emisión y se suspenden al entrar al mundo.

No se ha añadido un sonido sintetizado de baja calidad. Un efecto sonoro original de calidad necesitará escucha y revisión independientes.

## Compatibilidad con trabajo activo
Esta candidata se desarrolla en alcance y PR separados de la PR #136: no modifica SlicePresenter, cartelas, integración, escenas, modelos, referencias, Main ni archivos del otro propietario de Valoria. La integración final requerirá reconciliar el estado canónico y la calidad de ambos trabajos.

## Pruebas obligatorias pendientes
- Ejecutar Unity 6000.3.23f1 real y ConstructionDelightStateTests en la revisión exacta.
- Capturar vídeo auténtico A/B en 390x844 y 844x390 de inicio, bucle y fin en aserradero y cuartel; comprobar el brillo y la geometría en la cámara aprobada.
- Verificar mejoras c3, Bastión, carga de obra, caducidad, retorno de mundo y no alteración de guardados.
- Comprobar que los materiales están soportados en URP WebGL y Android, sin quads rosas, overdraw molesto ni partículas ocultas por Gaussian splats.
- Medir rendimiento de la escena y probar toque y gestos sin interferencias.
- Rechazar y reajustar el arte si no resulta claramente bonito en movimiento; compilar no es ART APPROVED.

## Incidencia y continuación comprobadas — 04/11/2026

**Implementado en PR #152, no certificado ni ART_APPROVED.** La lógica de estados, los siete emisores de mallas, el shader URP y el capturador nativo con 26 vistas/franjas de tiempo están en la rama `feature/eldoria-construction-delight-20261104`.

Las dos primeras pruebas nativas (runs `41746271850` y `41746717881`) llegaron a Unity, pero fallaron en compilación porque el proyecto no tenía activado `UnityEngine.ParticleSystemModule`. Se cambió de técnica sin tocar el `manifest.json` compartido.

La prueba `41793699479` arrancó el Editor 6000.3.23f1 en Windows pero salió con **198 / No valid Unity Editor license / com.unity.editor.headless not found**, antes de compilar los emisores nuevos.

La lectura aislada `41796258446` verificó que `UnityEntitlementLicense.xml` estaba presente y que Unity Hub/Unity Licensing Client no estaban iniciados. `41796661851` arrancó el Unity Hub instalado, minimizado y sin introducir credenciales; lanzó el servicio de licencias, pero GitHub finalizó esos procesos al cerrar el job. En `41797026292` se repitió dentro de **un mismo job**, con Hub y Licensing Client activos y un proyecto temporal vacío; el Editor volvió a terminar con **198 por licencia no válida**. No se leyeron secretos ni se reinició el PC.

**Bloqueo humano demostrado:** activar/actualizar la licencia con la cuenta correcta en Unity Hub; para Unity Personal, el inicio de sesión en Hub es el método oficial. No hay bypass legítimo ni prueba visual sustitutiva. El runner Windows se liberó en `main/pipeline/active-workstreams.json` revisión 6 y se conservaron la rama y PR #152 como borrador.

**Continuación técnica exacta una vez restablecida la licencia:** reclamar runner en el mismo ID canónico `eldoria-construction-delight-v1-20261104`, sincronizar registro main/PR, actualizar `tools/construction-delight/native-request.json`, ejecutar `.github/workflows/eldoria-construction-delight-native.yml`, revisar las **26 capturas Unity reales** vertical/horizontal y la secuencia de finalización, corregir defectos artísticos y repetir QA de guardados/móvil. Nunca publicar, mezclar PR o declarar calidad >=8 antes de comprobar imágenes y movimiento. Hasta entonces, **compilación nueva pendiente, A/B real inexistente, arte no validado**.


## Investigación acumulativa y pruebas sin Unity — 04/11/2026, posterior al error 198

Se investigó el riesgo de que GitHub Actions lance Hub en una sesión de servicio (recomendación de GameCI). La prueba de Windows `41798743304` **descartó esa hipótesis para este ejecutor**: PowerShell del runner, `explorer.exe`, Unity Hub y Unity Licensing Client estaban todos en **SessionId 2**. No usar arreglos de Session 0 para este caso.

El ejecutable oficial Unity CLI sí está instalado. El chequeo read-only `41799120407` produjo `auth status` **exit 3** y `license status` **exit 4**, sin cuenta activa ni licencia disponible para CLI. Unity-Technologies documenta estos códigos. Un segundo chequeo `41799334303` devolvió `hub_session_adoptable=False`: no había una autenticación existente de Hub que la CLI pudiera adoptar automáticamente. No se leyeron tokens ni se cambió de cuenta. No confundir un XML de licencia presente con una sesión autenticada actual. No solicitar reactivación ciega sin explicar esta evidencia. La última ejecución nativa verdadera, `41793699479`, continúa bloqueada por exit 198 antes de compilar la nueva versión.

**Progreso verificable independiente de la licencia:** la ejecución Ubuntu `41799938783` compiló en .NET el archivo original `ConstructionDelightState.cs` con una simulación estructural mínima de `PlayerState` e hizo 34 comprobaciones, **PASS**. La ejecución conjunta `41800430492` repitió los 34 assertions y analizó la sintaxis C# con Roslyn de los tres archivos principales (lógica, renderizador y captura), **0 errores de sintaxis, PASS**. Esta última prueba usa el SDK .NET 10 para coincidir con la versión de las bibliotecas Roslyn instaladas, evitando los conflictos de versión detectados en una primera candidata. No equivale a compilar los métodos de Unity ni a representar píxeles.

**Prevención de bucles:** el workflow nativo incorpora un preflight de licencia contra un proyecto temporal de Unity antes de descargar el repositorio completo. Ante exit 198 debe fallar explícitamente, sin reinstalar, reiniciar, tocar credenciales, aceptar nuevas condiciones ni repetir builds pesados. El diagnóstico de SessionId no debe volverse a ejecutar sin indicios nuevos.

**Único bloqueo externo actual:** abrir Unity Hub en el escritorio autenticado y restablecer una licencia válida mediante el flujo oficial de Unity. No usar los métodos de activación CLI Pro/Enterprise para Unity Personal; Unity exige una sesión válida y consentimiento a las condiciones. Este paso no lo puede superar el agente legítimamente con las herramientas y autenticación disponibles.

Reanudación exacta: revisar `main/pipeline/active-workstreams.json`, reclamar el mismo ID `eldoria-construction-delight-v1-20261104` para `windows-runner-heavy` mediante CAS solo cuando realmente esté libre; sincronizar snapshot al PR, actualizar `tools/construction-delight/native-request.json`, ejecutar la captura nativa y examinar las 26 imágenes A/B y fotogramas secuenciales antes de decidir si el diseño es bonito. El rendimiento Android/WebGL y la versión instalada siguen sin validar. **No ART_APPROVED, no merge, no APK.**
