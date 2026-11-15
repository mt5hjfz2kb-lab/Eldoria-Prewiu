# La Senda de la Brecha — prototype 3D aislado Unity

**Fuente:** `Unity/Assets/Eldoria/SendaBrecha/SendaBrechaPrototype.cs` y `Editor/SendaBrechaNativeProof.cs`.
**Estado 15/11/2026:** implementación fuente de prueba artística/funcional, **NO** escena nativa compilada, **NO** captura certificada de Unity, **NO** integración, **NO** aprobación artística, **NO** prueba física Safari. No altera el juego ni los guardados.

## Objetivo

Conservar la mecánica que el propietario aprobó al ver el vídeo del prototipo 2.5D: un escuadrón medieval avanza por un camino en perspectiva, cambia de carril, cruza puertas numéricas integradas en el mundo, dispara contra enemigos y crece o sufre pérdidas visibles. El vídeo del juego comercial sirve como referencia de *dinámica*, no como fuente de sus personajes, assets, UI ni código.

## Qué se ha implementado en una carpeta nueva

- Juego exclusivamente virtual de seis oleadas y tres carriles; entrada, pausa, reintento, victoria y derrota.
- Cámara Unity **perspectiva** y geometría 3D: calzada de pizarra, márgenes de roca, bosque, ruinas/columnas, soldados con casco, escudo y lanza, enemigos y puertas físicas con cifras flotantes.
- Formaciones visuales de hasta **14** soldados de un ejército virtual de hasta **100**; evitan instanciar un GameObject por cada soldado lógico. Cámara, materiales y geometría pertenecen a un único root aislado, sin usar assets prohibidos.
- Arquería simulada con un conjunto fijo de doce proyectiles visibles; gestos táctiles laterales, flechas del teclado, botón de pausa y HUD temporal nativo.
- Resolución automática basada en las reglas candidatas del piloto, **sin** aplicar cambios al motor T1–T5, a economía, misiones, correo, recursos o guardados.
- Fixture nativo de Editor capaz de generar capturas GPU reales a `390×844` y `844×390`; no se confunden con imágenes conceptuales. El runner requiere GPU real y permiso exclusivo sobre `windows-runner-heavy`.

## Por qué esta técnica

La demo web inicial resultó aceptable como referencia de mecánica en vídeo, pero mostraba soldados y puertas en un Canvas 2D. Para evaluar el salto artístico real hay que pasar a geometría y materiales de **Unity URP**, cámara perspectiva, profundidad, oclusiones y animación de objetos en pantalla. Este prototipo deliberadamente utiliza primitivas muy ligeras como sustituto técnico; **su apariencia no debe presentarse como acabado comercial**. Tras las capturas nativas habrá que evaluar sustitución selectiva de esos proxies por arte original y autorizado de la biblioteca, con comprobación de origen/licencia y rig/LOD.

## Prueba repetible sin tocar main

En el editor Unity 6000.3.23f1, ejecutar el menú `Eldoria → Prototipos → Senda de la Brecha (crear escena temporal)`. El menú abre una **escena nueva sin guardar**; no modificar la escena de producción. Entrar en Play, pulsar `COMENZAR EXPEDICIÓN`, mover con las flechas laterales, observar los seis tramos y confirmar reinicio. La cámara y todo el contenido se crean únicamente dentro de la escena temporal. En ejecución CI desechable, usar `-executeMethod Eldoria.SendaBrecha.Editor.SendaBrechaNativeProof.Run` sin `-nographics`, con `SENDA_BRECHA_CAPTURE_DIR` para redirigir resultados. Ese fixture genera siete PNG y un `proof.json` con estado y no avala por sí solo nivel artístico.

## Gates de calidad

1. Compilación de C# en Unity 6000.3.23f1 con URP, sin warnings relevantes, importación correcta de `.meta` y 0 errores.
2. Fixture nativo **exact HEAD**: escenas a las dos resoluciones, puertas y personajes sin recortes, resultado de seis oleadas, pruebas de selección y recompensas siempre virtuales.
3. Revisión visual A/B con los fotogramas del vídeo comercial: misma **legibilidad de la acción**, identidad completamente propia, personajes visibles, crecimiento materializado, disparos y enemigo final con impacto. Si se parece solo en matemáticas y no en experiencia, rechazar y rehacer presentación.
4. Integrar arte autorizado de la biblioteca (sin fabricar GLB nuevos antes de buscar reutilizables), animación convincente y HUD coherente con Vigilia, sin invadir los otros workstreams.
5. Construir **entrega web independiente** que pueda abrirse desde un enlace https en Safari, comprobar almacenamiento y gestos, evitar recarga de iPhone y medir rendimiento/memoria.
6. Después de la aprobación de jugabilidad/artística, decidir si el evento se incorpora en el juego y su desbloqueo/recompensa. **No lanzar ni invitar testers mientras no se cierre P0 iPhone de beta principal.**

## Evidencia y límites presentes

- La versión **web 2.5D**, no Unity, ya se ejecutó en navegador remoto mediante raw.githack: confirmación de seguridad, `JUGAR AHORA` y botones de carril funcionaron. No se ha probado en Safari iPhone físico.
- La lógica port C# tiene test **estático** reproducible `python tools/senda-brecha/tests/unity_3d_contract.py` para 729 recorridos; ese script no compila C# ni renderiza Unity.
- En el momento de redactar el archivo, el runner Windows está reservado por `eldoria-stylized-direction-proof-20261115`; **no se inicia Unity ni se cancela su trabajo**.
- El prototipo Unity se guarda en PR #195 como borrador; no se fusiona ni publica y no sustituye el videojuego actual.
