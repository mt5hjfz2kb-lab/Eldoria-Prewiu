# Aldric pasea por Valoria — candidato aislado

Objetivo: una instancia **visual** de Sir Aldric, sin crear otro héroe lógico y sin alterar partidas guardadas, combate, la UI o la cámara. Entra en la ciudad por la escena integrada, busca el MeshCollider real `Axial_Paving` del prefab `Paths` y solo camina cuando tres puntos y todos sus segmentos tienen calzada y espacio libre. En ausencia de una ruta validada no se crea el personaje.

Modelo autorizado obligatorio: `Unity/Assets/StreamingAssets/Eldoria/Aldric/SirAldric_Approved_RiggedAnimated_v1.glb`, SHA-256 `c28a65b6040225256ae047d8fd773849771895ac378041e0882f279517e78c8b`, 10.140.372 bytes, 26 joints, 57.332 triángulos, clips `Aldric_Idle_v1`, `Aldric_Walk_v1`, `Aldric_SwordAttack_v1`, `Aldric_Hurt_v1`, `Aldric_Defeat_v1`. La fuente exacta está recuperada en el paquete de entrega del propietario; **ya incorporado directamente a esta rama** tras reconstrucción exacta del artefacto Tripo y las recetas Blender/rig. El Git blob se ha validado como `5173328f4fa66cf1f050dba5a9000b44256ff086`. Si la carga o ruta falla, el actor no aparece: no se usa ningún sustituto.

El código usa glTFast 6.14.1 (ya instalado), una sola entidad visual, escala 1,85 m verificada contra bounds, pies a nivel de calzada, pausa y recorrido, ocultación según encuadre ortográfico (no distancia fija de 95 m), `SkinnedMeshRenderer.updateWhenOffscreen=false`. **Antes de importar verifica el SHA-256 y tamaño exactos del GLB cargado desde StreamingAssets**, y tras la importación exige 26 huesos y los cinco clips aprobados. Una identidad incorrecta o importación fallida aborta sin actor. Aldric desaparece de la ciudad al viajar a Mundo, salir en una marcha propia o investigar la fisura; reaparece al regresar. No altera `GameState`, `LocalGateway`, ni `ValoriaIntegratedWorld`.

Para promocionar a main: (1) terminar PR120, (2) el GLB exacto ya está versionado; no usar el modelo estático ni los PNG de diálogo; (3) ejecutar Unity nativo PlayMode/EditMode en el SHA exacto con capturas reales horizontal/vertical, probar `Axial_Paving`, obstrucciones, pausas y ausencia en misión; (4) inspeccionar escala, piel/capa animada y perfil Android. No declarar aprobado hasta esas pruebas. No publicar APK sin ellas.

## Entrega verificable y procedimientos

- Fuente binaria de la sesión: `Eldoria_Aldric_Modelo_animado_exacto_para_PR122.zip`, con la ruta de `Unity/Assets/StreamingAssets/Eldoria/Aldric/` preservada. Se generó fuera de GitHub; **incluida como GLB en esta PR**, reconstruida en la ejecución 40711186631. No marcar integración completa hasta verificar en el repositorio remoto.
- Después de trasladar la fuente al proyecto Unity, ejecutar `python3 tools/aldric-city/verify_approved_glb.py`: fallo (salida 2) si el archivo falta o difiere, éxito únicamente para el modelo original de cuerpo entero (GLB2, SHA, 26 articulaciones, 10 mallas y 5 clips).
- La rutina de calle valida MeshCollider `Axial_Paving`, pendientes y cada segmento a intervalos de 0,35 m; prueba opcionalmente una ruta más extensa de 4 puntos. Si no hay recorrido seguro, **Aldric no aparece** (no fingir que está funcionando).
- Revisar la orientación frontal del personaje: el código aplica una corrección preliminar de -90 grados; la orientación solo se aprueba tras grabar marcha real vista desde la cámara de ciudad.
- Comprobar horizontal/vertical y reposo/andar, ausencia durante viajes de Aldric y la investigación, guardar/cargar sin cambios, tiempo de carga y memoria/FPS reales en Android.
- El trabajo no incluye navegación NPC general ni generación de rutas sobre una ciudad remodelada. Tampoco modifica los edificios, la UI, PR 120, ni los guardados. `ART_APPROVED` y `APK_PUBLISHED` quedan pendientes.

**Estado del corte actual:** código en PR 122 + verificador fuente preparados, fuente auténtica empaquetada, comprobación local del modelo posible. **No hay PlayMode ni Android físico certificado y el binario está versionado en esta PR. Falta certificar la escena real en Unity.**


## Actualización verificada — 28/10/2026

**Transferencia del modelo resuelta.** GitHub Actions [40711186631](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40711186631) reconstruyó la geometría aprobada y los cinco clips desde el [artefacto original Tripo](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40527678020), la receta del art pass azul y los cuatro fragmentos SHA-fijados del rig. Coste añadido: 0 €. Comprobaciones SHA-256 exactas PASS.

- El GLB original de 10.140.372 bytes ya está en `Unity/Assets/StreamingAssets/Eldoria/Aldric/`, con GUID estables.
- El QA de fuente [40712097411](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40712097411) pasó; **no prueba Unity render ni rendimiento móvil**.
- Test nativo listo: `Unity/Assets/Eldoria/Tests/PlayMode/AldricCityRoamingNativeTests.cs`; workflow `.github/workflows/aldric-city-native.yml`: prueba real de escalado, paseo y 4 PNG de cámara real (paisaje/retrato, andando/parado).
- Dependencia vigente: PR #120 ocupa `windows-runner-heavy`. No iniciar Unity ni fusionar hasta que termine y se libere esa reserva.
- Estado: **MODEL_IN_GITHUB=PASS / SOURCE_QA=PASS / REAL_UNITY_PLAYMODE=PENDING / MOBILE_QA=PENDING / MAIN_NOT_MERGED**.

## Corrección de integración posterior al QA de fuente

- Se comprobó el `PlayerCamera` auténtico en `Valoria_Reference_Clean.unity`: origen aproximado (46, 77, -104). La distancia a la plaza de Valoria supera 95 m, **aunque la plaza está en su encuadre**. Un culling basado en distancia habría ocultado a Aldric. Reparado con `Camera.WorldToViewportPoint`, comprobando profundidad y un margen de 5 % de pantalla, sin alterar la cámara real.
- El componente se adhiere al inicio y al volver a cargar la escena integrada (`SceneManager.sceneLoaded`) para que las pruebas y el reinicio no eliminen al caminante. Se evita duplicar el componente.
- La consulta de guardado para saber si Aldric está en marcha o investigando se limita a una cada 0,2 s; el componente solo consulta el mismo gateway sin escribirlo.
- El intento histórico de recuperación del modelo usando la máquina Windows no encontró el archivo. **La reconstrucción posterior en GitHub sí funcionó**, por lo que se retiró ese workflow obsoleto.
- Los tests de fuente en Ubuntu siguen siendo **no-Unity**; la prueba nativa de paseo y las cuatro capturas están preparadas, no ejecutadas. No marcar `PLAYMODE_PASS` ni `ART_APPROVED` por esos tests.

## Continuación de fuente — 28/10/2026

El mismo encargo canónico se reclama en main como `eldoria-aldric-city-roaming-20261028`, sin reserva de Unity mientras PR120 siga validándose. Revisión independiente encontró una colisión C# real: `Application.streamingAssetsPath` se resuelve al namespace hermano `Eldoria.Application`. Un ensayo mínimo con el compilador Roslyn confirmó CS0234. Se utiliza explícitamente `UnityEngine.Application.streamingAssetsPath`. Las esperas de inicio/importación/prueba pasan de contar fotogramas a plazos de tiempo real: un equipo que renderiza rápidamente no debe agotar la espera antes de cargar la escena o el GLB.

El binario aprobado se ha vuelto a leer desde el objeto Git y coincide en tamaño10.140.372, SHA256c28a65b6040225256ae047d8fd773849771895ac378041e0882f279517e78c8b,26articulaciones y los5clips originales. No hay nueva generación ni gasto. Esta verificación de fuente no es una prueba de Unity ni permite declarar aceptación visual. Las pruebas nativas siguen pendientes hasta acabar PR120.


## Fuente preparada mientras termina PR120

La reserva Windows sigue siendo exclusivamente de PR120; no se ha iniciado Unity122.
El caminante ahora comprueba el volumen completo con OverlapCapsuleNonAlloc contra
los colliders actuales, en lugar de tres puntos de una lista tomada al arrancar.
Un búfer lleno bloquea el paso. La prueba nativa añade una obstrucción física
estrecha después de validar la ruta, verifica que el paseo no modifica el estado,
y ejecuta navegación a Mundo, marcha real de recolección y la investigación real,
con ocultación/reaparición y recarga persistida. Solo se modifica una partida
VALORIA_GAMEPLAY_PROBE comprobada. Ocho capturas auténticas previstas, sin contar
ninguna como existente antes de ejecutar Unity. El workflow prepara los mismos
recursos canónicos III y restaura Search.index de forma reversible.
Referencia de API: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Physics.OverlapCapsuleNonAlloc.html
Estas son correcciones de fuente, no aceptación visual ni certificación móvil.

La prueba preparada también exige deformación efectiva de los26huesos entre
fotogramas durante Walk y materiales soportados sin shader de error. Cuatro
capturas diagnósticas desde ángulos opuestos para cara/capa se suman a las ocho de cámara
jugable/ausencia/retorno. La cámara de producción se conserva intacta; las
diagnósticas son exclusivamente del probe y no sustituyen las vistas jugables.

Revisión adicional del ciclo de regreso: se corrige una interrupción que podía
saltar el destino pendiente o dejar al actor inmóvil al rechazar tramos restantes
menores de0,75m. Ahora reanuda el tramo y se detiene en su hito real. La prueba
exige esa pausa tras volver de Mundo y movimiento efectivo después de terminar
la investigación y recargar. Es una corrección de funcionamiento; necesita su
propia ejecución nativa antes de aceptación.

La primera ejecución nativa40761888936 (fuente29ee8179) falló al compilar
la prueba, antes de PlayMode: Eldoria.Tests.PlayMode no puede referenciar
los tipos de Assembly-CSharp directamente. Se aplica el mismo acceso a API
pública por reflexión que RoyalGoldV3IntegratedAcceptanceTests; no se cambian
asmdefs ni la arquitectura para resolverlo. No produjo capturas ni prueba de
paseo. La siguiente certificación compartida ejecutará todos los tests y el
caso de Aldric, conservando además sus12imágenes originales y fuente exacta.

La certificación completa40764789611 compiló, pasó EditMode y72de73PlayMode.
El único fallo es real y de integración: ALDRIC_CITY_NO_SAFE_PAVED_ROUTE;
no se creó ningún actor ni captura, por tanto NO hay aceptación visual.
El buscador anterior probaba solo cinco puntos cercanos al centro del bounds
de una malla irregular, que puede caer bajo el Bastión o fuera del pavimento.
Se sustituye por muestreo acotado de la superficie REAL de todos los colliders
Axial_Paving, repartido entre fotogramas, priorizando el encuadre real.
Mantiene cápsula completa, pendiente, tres puntos separados dos metros y
verificación cada0,35m; no se elimina ningún bloqueo ni se toca la ciudad.
Añade telemetría de superficie/ocupación/primer obstáculo para diagnosticar
la causa si ninguna ruta cumple. Pendiente de repetir la prueba nativa.

Revisión de orientación durante la segunda ejecución: el GLB aprobado tiene
la capa detrás del eje-X y el frontal+X, sin rotación adicional del nodo raíz.
La documentación oficial de glTFast4+ confirma conversión por inversión deX:
https://github.com/Unity-Technologies/com.unity.cloud.gltfast/blob/main/Packages/com.unity.cloud.gltfast/Documentation~/UpgradeGuides.md
Por tanto el frontal importado es-X y necesita+90°, no la corrección preliminar
-90°. Se prepara esa corrección y una aserción física independiente: centro de
la capa real detrás del cuerpo real respecto al desplazamiento actual. Las
cuatro vistas diagnósticas se alinean con ese desplazamiento, conservando
siempre la cámara jugable. No se atribuye aceptación sin la nueva prueba.

Ejecución40770024166(fuente70eadf34): la ruta sobreAxial_Paving PASA
con510muestras,274sobrepavimento,272libres/visibles y3hitos.
El siguiente bloqueo es ALDRIC_CITY_IMPORT_OR_26_JOINT_RIG_FAILED.
PlayMode73total/69pass/4fail: los cuatro fallos contienen ese mismo error
de importación, también emitido al cargar escenas de otros tres casos.
No existe captura del actor todavía. Se elimina el cálculo inicial por
Renderer.bounds con el actor inactivo: se transforman explícitamente los
bounds locales importados, conservando la medida real del modelo activo
en la aserción nativa. Importación Legacy explícita y diagnóstico de
cada rechazo/Exception real. La próxima ejecución comprueba primero
Aldric y después TODA la escena/EditMode/PlayMode si pasa, sin quitar
ninguna prueba ni ignorar errores. La aceptación sigue pendiente.

## Causa concreta de importación — prueba40773905914

El preflight falló con `Invalid mainBufferType Position, Tangent` en
`GLTFast.MeshGenerator.CreateVertexGenerator`, antes de crear el personaje.
La fuente aprobada omite NORMAL en sus diez mallas, y la armadura usa
normalTexture. La hipótesis inicial de bounds inactivos no era la causa
confirmada: el diagnóstico nuevo identificó este bloqueo previo del importador.
La corrección técnica prepara solamente normales geométricas por triángulo
y las añade como streams al GLB temporal en memoria. El GLB original
permanece byte por byte intacto. CI prueba que todo el BIN original,
posiciones, índices, UV, materiales/texturas, nodos, pesos,26huesos y5clips
son idénticos; solo se añaden NORMAL y sus accessors/views. No es otro modelo
ni otra autoría artística, no hay créditos ni cambios de paquetes o escenas.

`tools/aldric-city/prepare-import-normals.py` regenera/compara los streams y
`docs/aldric-city/normal-import-proof.json` fija ambas identidades. Runtime
verifica original/streams/JSON/GLB temporal antes de importar; el test exige
la identidad temporal real además de medir el cuerpo vivo. Los errores
no se ignoran. La prueba añade contacto real de botas con calzada y la
visibilidad inicial correcta para una partida cargada durante una misión.
El preflight publica evidencia también si pasa, para revisar imágenes reales
mientras continúa TODA la batería compartida obligatoria. Aún pendiente.

## Primeras imágenes reales —40777258036

La compatibilidad de normales resuelve la importación: fuente y GLB temporal
verificados,26huesos/5clips presentes, escala1,85m y dos PNG reales de
ciudad horizontal/vertical. Aún NO hay paseo/retornos ni aceptación completa.
El preflight se detuvo en la medida de botas: Renderer.bounds desplazado
0,1295m, exactamente0,07del pivote raíz multiplicado por1,85. Ese bounds
no identifica necesariamente los vértices animados reales. Se cambia el
enfoque: muestrear el clip Idle original y medir la geometría skinned real
con BakeMesh una vez al importar, con renderers ocultos; normalizar/aterrar
esa pose. La prueba calcula vértices animados reales en Idle/Walk y mantiene
las tolerancias originales0,12/0,15m. No se amplían para obtener PASS.
Referencia API oficial: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/SkinnedMeshRenderer.BakeMesh.html
Las dos imágenes se inspeccionaron: modelo presente en calzada y escala
coherente con edificios. Esperar nuevas12vistas y caso completo antes de
aceptar orientación, detalle de cara/capa y funcionamiento de retornos.

La señal ApprovedVisualReady ahora espera la configuración completa y un
fotograma tras el ajuste, con pausa de reposo ya inicializada. Update no
puede iniciar el paseo mientras continúa el importador asíncrono. Esto evita
medir/renderizar una escala transitoria o llamar reposo a una marcha prematura.

## Paseo real y revisión visual —40779937374

Pasan: importación, escala,26huesos originales, Walk efectivo con deformación,
orientación frontal/capa detrás, cápsula ante obstáculo dinámico, ocultación
en Mundo y visibilidad al volver. Los vértices reales de botas dan offset
+0,000135m Idle y-0,026675m Walk, coherentes con el cálculo de la fuente.
Nueve PNG auténticos inspeccionados individualmente, incluidos cuatro
primeros planos: cara/armadura/capa corresponden al modelo aprobado.
No se acepta aún: falla la comprobación de pausa tras regresar y la pose
de brazos continúa demasiado abierta para un paseo natural.

Se observa la contribución visible(weight) de las animaciones originales,
no solo IsPlaying de un estado legado registrado con peso cero. El test
rinde la cámara ciudad durante la observación batch, exige posición a0,05m
del destino(por debajo de los0,12m anteriores), movimiento detenido, Idle
visible y Walk sin contribución. Registra posición/destino/pesos/pausa/
colisión/tiempo y, solo si falla, una captura diagnóstica del retorno.
No se elimina ninguna acción de marcha/fisura/guardado pendiente.

La corrección de reposo es una capa aditiva técnica en memoria de los dos
brazos, separada de los cinco clips originales: convierte la postura base
enA en reposo de ciudad. No cambia el GLB, rig, pesos, materiales ni claves
de los clips aprobados; no hay otro modelo ni animación fuente generada.
La prueba exige que los brazos reales cuelguen en reposo. La mejora visual
sigue pendiente de nuevas imágenes reales; sin ART_PASS por anticipado.


## Bloqueo real de apoyo —40784676062

La nueva prueba pasa la alineación real de brazos en Idle, importación,
altura, deformación Walk, botas, orientación, obstáculo y Mundo/visibilidad.
No pasa el retorno completo: posición(-1,24;0,46;17,45), destino
(-1,24;0,46;15,78), detenido, Idle1/Walk0; tiempo avanza, cápsula y
segmento grueso libres, pausa reiniciada cada6s. Esto descarta reloj
congelado y estado Walk sin peso como causa del bloqueo actual.

La calzada tiene losas separadas y juntas visibles en los PNG reales.
Hipótesis: el rayo puntual del paso fino cae en una junta que las muestras
cada0,35m no detectan. Se cambia el criterio a huella de apoyo de8cm
sobre el MISMO MeshCollider real: primero rayo central; si falla, al menos
4de8 muestras, dos pares opuestos y desnivel compatible con28grados.
No se sustituye suelo por plano inventado ni se acepta césped/agua.
Se mantiene la cápsula completa y se registra el motivo exacto de cada
paso rechazado: apoyo, cápsula, collider y activación de calzada.
Sin cambios de tiempos, tolerancias ni acciones exigidas por el test.

Arte y certificación completa pendientes;10PNG son evidencia parcial,
no aceptación final ni prueba de dispositivo físico.


## Diagnóstico de enfoque y alternativa macroscópica —40787189864

Falla de nuevo el retorno; el nuevo registro confirma apoyo=False, calzada
activa y cápsula libre. La huella8cm/dos pares exactamente opuestos no basta.
La autoría original usa juntas3–5cm y biseles2,5cm, frente al muestreo35cm:
puede aceptar todos los puntos de una ruta y fallar entre ellos. Se descarta
repetir ajustes de animación, reloj o ampliar plazos; no eran la causa.

Alternativa: polígono físico de apoyo13cm, dentro de la cápsula26cm intacta,
16contactos posibles sobre el mismo MeshCollider. Se exige centro
estrictamente dentro de la envolvente de contactos: hueco angular máximo
menor de180grados. No acepta apoyo solo a un lado de la carretera. Se
mantienen pendiente28grados, desnivel limitado, cápsula y consultas reales.
Se valida cada tramo a2,5cm, por debajo de la junta original mínima.

Comprobación barata de la malla visible exacta Axial_Paving_GI, escena
24c68b8029686c89c6a72f06f3d5151b62e2edf9, malla
5ee4f08d1447d6525b2a3eca7f1c497e50fab149 y transformaciones reales:
121puntos alrededor de(1,43;16,30): criterio antiguo rechaza26/nuevo0;
alrededor de(-1,24;17,45): antiguo43/nuevo0. Es análisis de fuente visible,
NO prueba del collider de Unity ni aceptación del recorrido completo.
El siguiente test exporta ambos meshes reales en coordenadas mundo,
solo como evidencia binaria del probe, para reproducir cualquier fallo
restante sin adivinar ni repetir iteraciones pesadas sin diagnóstico.
Mantiene íntegros todos los tests y las doce capturas exigidas.
