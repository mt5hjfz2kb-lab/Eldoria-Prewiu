# ELDORIA — Ronda 1, línea D: QA móvil independiente (cierre real)

**Autorización:** Dirección General Issue #23, Ronda 1 comentario #6060629819, línea D. **Propietario QA:** `chat-dg-r1-mobile-qa-20261008`. Sin cambios de Unity, arte, economía ni publicación; M07 no intervenido; cero APIs/servicios de pago.

## Orden y despacho auténticos
- Claim en `pipeline/active-workstreams.json` (commit `7b287f2c21542c909d16e0de8e019307acf7937e`) con alcance, propietario, evidencia exigida y recursos vacíos.
- **Coordinador existente** `.github/workflows/eldoria-coordinator-v1.yml`, no uno nuevo: `triage` → `authorize-mobile-qa` → `mobile-published-qa` → `mobile-qa-independent-report` mediante `needs`; entrada por push `ops(qa-mobile): dispatch authorized round1 published probe` de main, commit `0134e2f917cc3b3b9ee8e740b7529abbe96df688`. No botones, chats cerrados permitidos, no Work.
- Ejecutor: **el mismo verificador existente** `.github/workflows/unity-webgl-startup.yml` expuesto por `workflow_call`, sin reescribir sus pruebas ni tocar juego; Chromium con emulación táctil y vista landscape/portrait del WebGL publicado.

## Resultado real, verificado independientemente
- GitHub Actions: https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/37833205383 — **completed / success**, fuente exacta `0134e2f917cc3b3b9ee8e740b7529abbe96df688`.
- Trabajos: triage `113503634372` SUCCESS; authorization `113504030329` SUCCESS; published real probe `113504072425` SUCCESS; independent report `113507288193` SUCCESS.
- Artifact: `eldoria-webgl-startup-evidence`, **ID 11574682143**, SHA256 `1e1ca661ae0d5c249df068ce0306959d33ef426a33aac3ecac5375deb692eb11`, incluye `report.json` y capturas. Verificador independiente exigió `failure=null`, `runtimeErrors=[]`, `coverage.fullBastionIToIIPass=true`, `checks.navigationGatherPersistencePass=true`, dos orientaciones con `pass`, `persistencePass` y `resetPass` verdaderos.
- Mensaje a DG **automático**, Issue #23 comment #6067746424, veredicto `PASS_MOBILE_PUBLISHED_TECH`. El propietario **no retransmitió** trabajo, no ejecutó GH Actions, no empleó Work.
- Pasaron navegación/touch/pan y gather World Region1, retorno, la progresión Bastion I–II, guardado/reload/reset en ambas orientaciones, sin errores fatales detectados por el probe. **Esto NO certifica iPhone físico ni aceptación visual de M07.**

## Restricciones y continuación del estudio
M07 sigue bajo owner propio bloqueado hasta seis gates y review visual; M11 y su QA de arte no se lanzaron. Sin recursos Windows/Unity ni auto-merge; no nueva ronda. Este workstream QA D sí ha completado el alcance móvil técnico autorizado y puede liberar sus recursos. Si surgieran fallos posteriores, abrir el circuito M04 conservando evidencias y reclamando scope, no reescribir historial.
