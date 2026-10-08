# M16-A: Catálogo y evaluación determinista
Implementación **pasiva**, sin dispatcher, sin modificación de workstreams y sin Unity.
Ejecutar: `node tools/m16a/evaluate.mjs pipeline/m16a/catalog-v1.json pipeline/active-workstreams.json`.
Tests ligeros: `node --test tests/m16a/evaluate.test.mjs`.
El archivo `pipeline/m16a/catalog-v1.json` es un ejemplo versionado, no cola autorizada para ejecución.
Un job con status ACCEPTED representa una afirmación del catálogo y **no es un certificado M03**; dependientes requieren evidencia externa ACCEPTED con gate PASS e issuer independiente.
Historial: conservar cambios mediante commits Git y evolución del catálogo. No se habilita mutación automática ni runtime ledger (M16-B).
Propiedad: conflictos de scope exactos o prefijos /** y recursos; no reclama ni libera recursos.
M03/M04: entrada `certificates` es un adapter provisional de lectura; no se sustituye su arbitraje futuro.
No invocar chats desde GitHub.
