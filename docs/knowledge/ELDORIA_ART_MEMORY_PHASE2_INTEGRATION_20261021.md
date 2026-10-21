# Eldoria — Memoria operativa de arte · Fase 2

**Estado de alcance:** integración de planificación y adaptador Qwen sobre los sistemas canónicos existentes. No sustituye al autor Blender/Unity ni al cerebro Qwen. Presupuesto predeterminado 0. El modo de lectura no reclama ownership, no toca escenas, no ejecuta Tripo, no publica y nunca concede `ART_APPROVED`.

## Qué está integrado

1. `tools/plan-art-production.mjs` consulta automáticamente `pipeline/art-knowledge/catalog-v1.json` antes de emitir cada plan; añade `art_knowledge_preflight` como primera etapa y un bloque `art_memory` con SHA, fichas seleccionadas, rechazos, procedencia y estado de entrega.
2. Con el argumento de salida existente, se generan `PLAN.json.knowledge.txt` (entrada Qwen) y `PLAN.json.knowledge.json` (manifiesto de selección, sin instrucciones ejecutables). Las salidas no sobrescriben evidencias previas.
3. `.github/workflows/art-production-plan.yml` conserva los planes y sus inventarios y comprueba hashes y prohibiciones con el runner ligero de Linux.
4. `tools/art-knowledge/run-artist.mjs` usa la entrada `--inventory` **ya existente** en `tools/art-direction/ai-authoring/author_brief.py`. Evita tocar el archivo Qwen reservado por un trabajo activo. En modo `--run`, solo después de una inferencia real y un `artist-inference.json` válido, compara `asset_inventory_sha256` con el texto realmente pasado y crea `knowledge-consumption.json`.
5. Al terminar un ensayo con imágenes reales y revisión, **reutilizar** `tools/art-direction/learning-evidence/cycle-hooks.mjs stage`, que guarda la observación como `pending_verification`. Ningún nuevo resultado entra automáticamente a la memoria canónica sin verificar sus fuentes y veredicto. El catálogo de 54 fichas sigue solo lectura.

### Selección controlada

Se puntúa el parecido de problema/categoría a partir de `asset_or_sector`, `diagnosis` y `failure_class`; los métodos retenidos o probados quedan separados de consejos normativos, métodos históricos y ensayos `art_rejected`. **Jamás** se ofrece una obra rechazada como recomendación ejecutable. El texto se limita a 3700 caracteres para caber en el inventario de Qwen, conservando rutas e identificadores.

No es aprendizaje de pesos: recupera documentación concreta y puede demostrar qué se entregó. Los métodos históricos continúan siendo referencias por revalidar, no autorizaciones para revivir una escena antigua.

## Uso habitual sin gastar recursos

Desde la raíz del repositorio, sobre la solicitud aprobada de un trabajo:

```sh
node tools/plan-art-production.mjs pipeline/art-production-requests/valoria-master-plan-layered-construction-20261016.json /tmp/valoria-art-plan.json
```

Produce un plan y dos archivos de memoria. Si está deshabilitada o no coincide ningún caso, el plan no inventa una técnica. No requiere Unity ni Windows.

Para integrar un nuevo ciclo **autorizado** de Qwen mediante su escritor existente, en un entorno donde estén disponibles Ollama, referencia y captura auténtica:

```sh
node tools/art-knowledge/run-artist.mjs \
  --request pipeline/art-production-requests/eldoria-tripo-art-brain-phase2-20261021.json \
  --memory-prefix /tmp/eldoria-art-memory-unique --dry-run -- \
  --reference references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg \
  --unity-before /ruta/a/la/captura-real.png \
  --target-object Spawnling_Near_Authored \
  --subject-kind creature \
  --out /tmp/resultado-qwen-unique
```

Por defecto hace solamente una **simulación de preparación**. Sustituir `--dry-run` por `--run` únicamente cuando el propietario del ciclo autorice realmente la inferencia y tenga disponibles las imágenes y el runtime de Qwen. Se usa el mismo `author_brief.py` instalado. Si el hash de inventario en `artist-inference.json` no coincide, el adaptador falla y no proclama consumo.

**Importante:** los trabajos ya iniciados bajo otro responsable no se reescriben. Hasta que esos procesos utilicen el adaptador o incorporen sus argumentos a su ejecución, sus inferencias no están automáticamente cubiertas. El planificador sí prepara el inventario de las solicitudes, pero generar el archivo **no certifica** que Qwen haya sido invocado. Para los próximos trabajos, iniciar Qwen desde el adaptador en lugar de invocar directamente al autor.

## Registrar aprendizaje tras una iteración real

El módulo existente ya prepara la memoria histórica y conserva la trazabilidad de una prueba de Unity. Usarlo en la ruta artística autorizada **después** de una captura A/B y una revisión, no durante la planificación:

```sh
node tools/art-direction/learning-evidence/cycle-hooks.mjs stage \
  --target bastion1 --out /tmp/eldoria-memory-proposed.json \
  --run-id RUN_REAL --git-sha SHA_REAL_40 \
  --reference references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg \
  --before /ruta/unity-before-original.png --after /ruta/unity-after-original.png \
  --camera-sha SHA_CAMARA_64 --view-id unity-player-pbr --render-pass unity_pbr \
  --diagnosis "Diagnóstico visual comprobado del defecto de esta fuente" \
  --lesson "Lección observada, vinculada a fuentes y prueba visual" \
  --next-action "Intervención siguiente justificada a partir de la revisión"
```

Los marcadores son deliberados y deben reemplazarse con evidencias auténticas. `stage` no sobrescribe la base existente: devuelve un catálogo candidato `pending_verification` y un archivo de evento. Un supervisor del ciclo debe contrastar los PNG y el veredicto independiente antes de incorporar cualquier caso. **No** es admisión automática.

## Qué se comprueba

- La memoria se prepara automáticamente para las solicitudes del planificador.
- Los métodos rechazados no entran en recomendaciones positivas.
- Cada inventario conserva SHA de catálogo, fichas e identidad del plan.
- El adaptador Qwen transmite ese inventario a la interfaz ya existente y puede verificar su consumo exacto si hay inferencia real.
- Sin pruebas originales, ningún nuevo ensayo se convierte en `ART_APPROVED`; la rutina existente deja el resultado pendiente.
- GitHub Actions prueba el selector, el adaptador y la consistencia de los artefactos sin runner Windows, modelos generativos ni servicios pagos.

## Frontera pendiente para alcanzar automatismo total

Fase 2 instala el **paso automático de memoria en el planificador y un adaptador compatible para futuras inferencias**, pero no introduce unilateralmente el adaptador en los workflows activos del propietario de Valoria/Engendro. Ese relevo debe reconciliarse al cierre de su ejecución, o mediante modificación coordinada, sin pisar scripts/runner ocupados. Tampoco está demostrada una reducción porcentual de tiempo o calidad: corresponde al benchmark de fase 3.

Canon de escena: `main/AGENTS.md` y `docs/VALORIA_VISUAL_REFERENCE_CANON_20261020.md`. Histórico consultado solo como método, sin reintroducir parcelas, barrios, campamentos o acantilados revocados.
