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
