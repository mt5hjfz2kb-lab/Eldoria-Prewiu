# Evidencia forestal original

[Descargar Eldoria_Forest_AB_20261026.zip](https://chatgpt.com/library?file_id=libfile_46d29ff672108191a9477a7855ea34b7&account_id=personal).
24 PNG originales con JSON nativo, las seis comparaciones finales verificadas,
primera tentativa rechazada, juicio independiente, XML finales y44checks integrados.
24.160.613bytes; SHA256 `3ab88532c2077b93004ed4e575551359df964355a6959dd9fb923a6c06630189`.
No se duplican capturas ni fuentes binarias en el historial Git.

## Verificar desde el repositorio

Descargar el ZIP, comprobar su SHA256 y extraer solo
`docs/world4x-multiplayer/evidence/**` en el checkout. Conservar los documentos y
el manifiesto vigentes de main. Los dos BLEND históricos citados por el manifiesto
siguen en Git; un checkout parcial debe materializarlos antes del gate.

```sh
python docs/world4x-multiplayer/verify_capture_pairs.py docs/world4x-multiplayer/evidence/final-control docs/world4x-multiplayer/evidence/candidate --out /tmp/forest-camera-check.json
node tools/world4x-artistic-process/verify.mjs .
```

Resultado ejecutado sobre el mergec448cb7: `WORLD4X_ARTISTIC_PROCESS_EVIDENCE_PASS
(NOT ART_APPROVED)`. La procedencia Blender/MCP es histórica; la intervención
actual modifica únicamente visibilidad Unity. No se invocó Blender ni se falsea
una nueva sesión MCP. Las capturas finales comparables son40387268720/40394742870;
la repetición técnica corregida40399630175 tiene exactamente el mismo código
forestal y su árbol completo coincide con el merge en main.

No interpretar el gate como aprobación artística global ni medición de rendimiento.
