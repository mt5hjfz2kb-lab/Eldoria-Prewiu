# Registro canónico de aprobaciones artísticas verificables

Esta carpeta solo contiene manifiestos JSON **formalmente aprobados**, nunca resultados de simple TECH_PASS o candidatos pendientes. Todos se someten al contrato de `docs/ELDORIA_ART_QUALITY_ACCEPTANCE_CONTRACT_V1.md` y al guard `tools/art-visual-acceptance/verify.mjs`.

No hay ninguna aprobación artística nueva declarada por crear esta carpeta: su ausencia se informa como `NO_ART_APPROVAL_DECLARED`, no como PASS estético. Para certificar debe existir evidencia real: PNG de Blender y PNG de Unity del candidato final, SHA reales, revisión independiente y, si se declara calibración, tres intervenciones encadenadas sobre la misma pieza.

Comprobar, en un checkout con los bytes PNG originales materializados:

```bash
node --test tools/art-visual-acceptance/verify.test.mjs
node tools/art-visual-acceptance/verify.mjs --directory pipeline/art-visual-approvals
node tools/art-visual-acceptance/verify.mjs --manifest pipeline/art-visual-approvals/NOMBRE.json
```

La comprobación de sintaxis/hash/puntuación no sustituye evaluación visual humana/independiente. GitHub Actions automático tampoco puede inyectar estas reglas en un chat anterior a su creación; ese agente debe leer `main/AGENTS.md` antes de su siguiente ciclo.
