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
