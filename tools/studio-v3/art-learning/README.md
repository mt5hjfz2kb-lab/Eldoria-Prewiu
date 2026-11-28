# Studio V3 — aprendizaje artístico verificable

## Alcance
Módulo aislado para el estudio, no cambio de pesos del modelo ni promoción automática.
La biblioteca `cases.json` contiene decisiones históricas verificadas; `guard.py` ofrece
`role_brief(role, holdout=None)`, `plan_gate(plan)` y `outcome_gate(result)`.
El benchmark `calibrate.py` utiliza el mismo modelo y las mismas imágenes anónimas en
ambos brazos. El brazo aprendido excluye el caso examinado y reglas derivadas de él.
Es retrospectivo: otros casos pueden ser relacionados; no demuestra generalización.

## Contrato de adopción
1. Cargar la memoria curada antes de emitir encargos al director, artista, integrador y auditor.
2. Exigir hipótesis causal visible, tres fortalezas protegidas, dos alternativas,
   análisis de vecinos/accesos y previsualización móvil. Validar plan_gate con
   procedencia y presupuesto comprobados por el host, no por la palabra del modelo.
3. Generar solo candidatos reversibles. Medir en Unity nativo, cámara y fixture
   emparejados, y actores controlados en evaluación estática. Separar vídeo de prueba estática.
4. Exigir revisión ciega de la escena, no solo sprites, con regresiones materiales explícitas.
   outcome_gate devuelve reject u owner_review; jamás aprueba ni promueve.
   Umbral propuesto: +0,30 y >=8,5 para considerar revisión, objetivo artístico 9.
   Si la elección es expresamente local del propietario (aserradero), documentarla
   como tal; no extrapolar a toda la escena ni usar el gate numérico como veto al propietario.
5. Si dos mecanismos fracasan, dejar de gastar en variaciones del mismo mecanismo.
   El siguiente reto debe usar un objetivo nuevo/holdout antes de afirmar mejora general.
6. Registrar nueva evidencia en memoria únicamente tras revisión humana/host;
   no transformar las afirmaciones de un agente en instrucciones automáticamente.
   
## Entrega al titular del Studio V3
El estudio canónico está reclamado por la misión PR198. Este módulo vive en la rama
de investigación Issue200 para proteger el trabajo concurrente. El titular debe
incorporar los archivos y enchufar los contratos a la ruta activa de cada rol;
los scripts anteriores que no importan este módulo no reciben sus reglas.
No se modifica el juego, no se ocupa Windows ni se reutiliza el presupuesto PR198.

## Calibración realizada el 28-11-2026
- [Ensayo de memoria contextual](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/46128313606): 14 pruebas offline, seis llamadas reales al mismo modelo con imágenes idénticas por par. Control y memoria coinciden 1/3 con las decisiones históricas; no se demuestra ganancia solo por aportar ejemplos. En el caso de Peso, el auditor detectó la oclusión del pozo y aun así recomendó el cambio. El aserradero expresa una preferencia del propietario y no una verdad objetiva sobre calidad.
- [Ensayo de arbitraje de decisión](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/46128482649): 21 pruebas offline, dos llamadas reales a auditoría estructurada sobre las mismas imágenes. Se aplica `decision_gate.arbitrate` después de revisión ciega. Los dos candidatos rechazados son bloqueados por regresiones de hitos/legibilidad y función respectivamente; el aserradero previamente elegido se conserva por bloqueo de decisión del propietario (sin llamada ni prueba de capacidad del modelo). Coincidencia de decisiones operativas 3/3, no precisión general del modelo. Artefacto 15256817111, digest sha256:dbf7f44c4dea305ceb982e6ebff54630e21df85e6b8e066f61cec13078e058a4.
- Coste estimado de los dos ensayos: 0.091758 USD, acumulado Issue200 1.6126543 USD, factura no conciliada. No se creó otra escena ni se usó Windows.
- Nuevo contrato: conservar decisiones del propietario; evaluación de identidad, función, hitos, acceso, integración y legibilidad móvil como dimensiones protegidas. Un defecto material nuevo bloquea el candidato; información incierta devuelve insufficient_evidence. Solo si no hay regresión y existe preferencia por candidato, continuar al gate cuantitativo nativo. Los resultados no autorizan promoción ni otro gasto. El próximo benchmark debe ser un problema no usado en los casos históricos, con control y protocolo ciego comparable.

## Producción prospectiva y reparación localizada — 28-11-2026
- [Benchmark ToneleriaDelPostigo](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/46129312263): mismo donante, modelo generador y configuración; control vs memoria+ejemplos visuales+contrato. Auditor ciego de sprites aislados: original 8.7, control 9.0, aprendido 8.6. No demuestra mejora de producción por memoria; la variante aprendida pierde lectura de duelas en montaje. Un solo caso, variabilidad del generador, no generalizar. El primer run 46129117635 falló por margen de imagen; se recuperaron ambos planes y la imagen pagada sin repetir gasto. Artefacto 15257127458, SHA256 856c7c0bc76d50c10df880b72568821d299fe6d949f1d846d7b32099fb7a4643.
- [Reparación de región protegida](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/46129541166): `protected_region.py` conserva alfa, lienzo y todos los píxeles fuera de polígono autorizado. 7920 píxeles cambiados de 312134, cero fuera. Selftest PASS. Nueva evaluación centrada en lectura de pieza en construcción: original 9, edición completa 7.5, reparación protegida 8. Sin costuras visibles según auditor, pero la función empeora respecto al original; conservar original. No comparar notas absolutas entre ambos ensayos por criterios distintos. Artefacto 15257152793, SHA256 cc8b1a3c0eb3a6180bcc3b633106b11c55931beaebcdd23bf29cecfba38b7407.
- Lección: antes de editar, describir visualmente el significado funcional de la pieza y definir una región autorizada, con ejemplos positivos/negativos del mecanismo específico. La máscara garantiza preservación técnica, no calidad artística. Cuando el original comunica mejor la actividad, no editar ni promover por obligación. Para el siguiente ensayo usar otro objetivo y comparar reparación de región con control, sin utilizar estos resultados como etiquetas de un caso supuestamente nuevo.
- Coste estimado adicional 0.186137 USD; acumulado 1.7987913 USD de 5, facturación no conciliada. No se alteró el juego, no se ejecutó Unity, no se entrenaron pesos. El módulo y el mecanismo siguen aislados; integración en el estudio común pendiente de coordinación con su titular.

## Academia artística: lectura y práctica verificable — 28-11-2026
- Currículo público versionado en `academy/curriculum.json`: 10 referencias Unity Learn, manual Unity 6.3, manual Blender y Ctrl+Paint. Se recuperaron extractos pertinentes de 6 páginas; las tres páginas de Blender devolvieron 403 desde el runner y el índice Ctrl+Paint no ofreció texto pertinente. Para esas fuentes se entregaron notas contrastadas por el director con documentación pública, no se afirma completar cursos ni visionar vídeos. El tutor elaboró 8 técnicas, revisadas y conservadas en `academy/techniques.json`.
- Consulta selectiva en `guard.role_brief(role, academy=True, competencies=[...])` y `academy/context.py` (opt-in; no contaminación de holdouts históricos). [27 pruebas offline](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/46135456204) superadas. Aún no conectado al estudio canónico PR198, cuyo titular conserva la reserva.
- [Estudio y laboratorio](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/46135342847): el primer intento 46135161367 falló por compilación de Blender sin OpenImageDenoiser, tras completar las llamadas de estudio. Se reutilizaron las respuestas pagadas y renderizó con Cycles CPU sin denoising. 4 imágenes Blender 4.0.2 verificadas por hash (artefacto 15259606609, SHA256 8a4ce0d90e1dc4c13b565559955b131f94fc8dab07e19724fe8153eed5baa02a). Prueba técnica de PPU/pivote/proyección: 3/3 en ambos. Evaluación ciega de laboratorio: materiales 8.5→8.5; patio 8.3→8.0, por separación visual insuficiente del pozo. No hay mejora general atribuible al estudio teórico. Siguiente ejercicio: medir siluetas proyectadas y optimizar posición de pozo/carro con geometría real; no inferir calidad de números solos.
- Coste adicional estimado hasta laboratorio 0.0631195 USD; acumulado 1.8619108 USD de 5. No facturación conciliada ni cursos certificados. Sin cambios de juego, Unity ni pesos del modelo.
