# Eldoria — evidencia artística entre sesiones

Complemento de memoria de la única ruta de autoría Qwen → Blender → Unity, no un nuevo generador ni evaluador de belleza.

Uso desde la raíz:

    node tools/art-direction/learning-evidence/learning.mjs audit
    node tools/art-direction/learning-evidence/learning.mjs report
    node tools/art-direction/learning-evidence/learning.mjs brief --target bastion1
    node --test tools/art-direction/learning-evidence/learning.test.mjs

Registrar un nuevo ciclo en una copia nueva, nunca editar la fuente mediante ingest:

    node tools/art-direction/learning-evidence/learning.mjs ingest --event /tmp/cycle.json --out /tmp/proposed-observations.json

El evento entrante debe usar source.evidence_level = "pending_verification"; el operador original verificará después los hashes originales y el resultado en Unity antes de promocionarlo al catálogo. Un GitHub Action SUCCESS no equivale a ART_APPROVED. La memoria no reentrena pesos de Qwen.

El propietario de Bastión I conserva la autoridad sobre author_brief.py y el diario artístico existentes: esta intervención no conecta el módulo al flujo activo sin su coordinación.

Contrato detallado: docs/ELDORIA_ART_LEARNING_EVIDENCE_V1.md.
