# Eldoria — hiperautomatización segura: mejoras operativas del 26-10-2026

**Autoridad:** complementa, no reemplaza `AGENTS.md`, `pipeline/active-workstreams.json`, `docs/ELDORIA_PARALLEL_WORKSTREAM_PROTOCOL.md`, las aprobaciones artísticas, las normas de gasto o los contratos de Unity.

**Propósito:** reutilizar los flujos canónicos existentes. Sin nuevo orquestador, nuevo PR, cola paralela, cambios de escena, gasto Tripo o ocupación de Windows. Esto NO declara hiperautomatización autónoma de extremo a extremo.

## 1. Coordinación: reducir «¿continúa?»

- `tools/work-status/report.mjs` ahora emite `operational_signal` por workstream: pausa explícita, disponibilidad explícitamente liberada, falta de propietario, observación desconocida, error CI, ejecución `action_required`, trabajo externo observado, reserva Windows por reconciliar y éxito técnico pendiente de evaluación.
- El informe se ejecuta mediante el workflow YA EXISTENTE `eldoria-work-status.yml`; compara las ramas y los últimos runs observables y guarda JSON + Markdown en Actions. Su campo `next_safe_action` es orientativo.
- La ausencia de jobs NO libera recursos, NO traslada propiedad de chats, NO demuestra que un ordenador esté libre. `status:success` tampoco cierra la misión ni equivale a `ART_APPROVED`.
- Para alcanzar continuidad genuinamente autónoma hará falta conectar un ejecutor IA autorizado, idempotente y con leases válidos; GitHub Actions solo encadena tareas fijas. No prometer persistencia de un chat cerrado.

## 2. Arte: cambiar método cuando falta mejora visible

- Mantener el planificador, contrato de aceptación, filtro visual A/B y el gate World4X existentes. No tocar los archivos ni workflows reservados por PR #100.
- El orden correcto es: referencia aprobada → defecto visible más importante en las tres cámaras → consulta de biblioteca → edición acotada → vista previa barata comparable → rechazo o revisión independiente → Unity focalizado → pruebas de jugabilidad → aprobación documentada si procede.
- Al segundo ciclo sin progreso visible, **cambiar hipótesis o escala de intervención**, no producir cuatro commits de micropintura ni ampliar compilaciones. El gate existente de estrategia World4X no equivale por sí mismo a calidad 7,5.
- **Pendiente por propiedad activa**: integrar pruebas de impacto en el workflow artístico de PR #100 después de coordinación con su responsable, sobre el mismo PR, nunca desde un segundo escritor. No modificar aprobación artística, regla del propietario ni asset antiguo de Bastión expresamente vetado.

## 3. Memoria artística: demostrar consumo, no solo almacenamiento

- Conservar `tools/art-direction/learning-evidence/observations.json`, `cycle-hooks.mjs` y sus hashes sin alterar la evidencia histórica.
- Antes de una inferencia del Bastión I, el responsable prepara el resumen mediante `cycle-hooks.mjs prepare --target bastion1 --out /ruta/nueva/lessons.txt`. Esto escribe el archivo y `.provenance.json`.
- El nuevo control de solo lectura `verify-brief-use.mjs` puede recibir **exactamente** esos dos archivos, el catálogo usado y el prompt de entrada ya construido:

```bash
node tools/art-direction/learning-evidence/verify-brief-use.mjs \
  --brief /ruta/nueva/lessons.txt \
  --provenance /ruta/nueva/lessons.txt.provenance.json \
  --prompt /ruta/nueva/prompt-enviado.txt \
  --catalog tools/art-direction/learning-evidence/observations.json
```

- Si un hash, catálogo, referencia, número de casos o inclusión textual no coincide, devuelve error. PASS significa únicamente **brief presente en el archivo de prompt**, NO ejecución inferencial ni mejora estética. La prueba real del modelo requiere traza posterior y revisión independiente.
- **Pendiente por propiedad activa:** conectar ese control al artista Qwen/Blender donde se construye el prompt real, mediante su dueño; no simular conexión modificando un gate ajeno.

## 4. Documentación: priorizar sin destruir historia

- Ejecutar `node tools/documentation-triage/report.mjs --format markdown`. Lee el catálogo canónico y presenta el inventario, los pendientes y un orden de revisión; NO altera estados, borra archivos o asigna autoridad a documentación antigua.
- La revisión manual se prioriza sobre normas operativas, automatización, ingeniería, infraestructura, gobernanza y QA, frente a material puramente histórico.
- Este informe también se incorpora a la ejecución Ubuntu existente `repository-architecture-guard.yml`, que corre pruebas de integridad del triaje y de la memoria. El catálogo `docs/documentation-catalog.json` sigue reservado para PR #100; no tocarlo desde esta intervención.

## Cómo comprobar que se ha aplicado

```bash
node --test tools/work-status/*.test.mjs
node --test tools/documentation-triage/report.test.mjs
node --test tools/art-direction/learning-evidence/verify-brief-use.test.mjs
node tools/documentation-triage/report.mjs --format markdown
```

La verificación en GitHub es **Repository architecture guard** sobre el commit exacto. Mantener separadas las validaciones de código, la eficacia operativa medida (intervenciones humanas, reintentos, tiempos) y la calidad jugable/visual. No declarar una reducción porcentual real de tiempos sin un antes/después comparado.
