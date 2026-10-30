# Eldoria — ampliación conectada de O, cierre verificable Q → S → T (23-10-2026)

## Entrega y frontera de autorización

- **Candidata retenida: T**, fuente `7a8cf8c380589142b148f942b062f485cefc8d9c`; permanece en el PR #100 **DRAFT**, estudio opt-in `ELDORIA_RIBERA_PILOT=1`. No se ha expandido el resto de Región 1, ni fusionado a `main`, ni publicado.
- Autoridad artística: O adoptada por el propietario; K e I recuperables. N, las pruebas descartadas y las etapas intermedias permanecen como evidencia. **ART_NOT_APPROVED**: T no demuestra aún 7,5–8/10 de calidad artística global.
- Mismo workstream `eldoria-world-region-1-canon-20261022`, relevo owner-directed revisión 13 registrado simultáneamente en `main` y rama candidata sin force, preservando la reserva `windows-runner-heavy` y los alcances concurrentes.

## Proveniencia y comprobaciones de geometría

1. P, `1372d41b...`: se amplió desde O con una sola superficie soldada, **16.405 vértices O originales**, 8.878 nuevos y 193 puntos de unión, tres caminos conectados, 62 nuevos árboles y ocho piedras en la pequeña elevación. O contiene 156 árboles: **218 en total**. No se desplazó el curso del río. El relieve se modeló exclusivamente en la extensión.
2. Q, `1cc169b...`: se sustituyó la transición de pigmento estirada por una clonación de área 2D y plantación dirigida de forestales con claro interior, conservando la malla soldada.
3. R, `d83f41e...`: reapertura y pruebas de selección sobre la fuente Q sin nueva obra artística.
4. S, `3c47c5cafe2389866b3791d3ce57089be15dfac8`: reconstrucción determinista de la máscara de caminos a partir de tres curvas Bézier originales, tras detectar que Blender podía purgar una imagen sin usuario persistente durante save/reopen. Se asigna fake user a las máscaras exportadas. La fuente O permanece inalterada.
5. T, `7a8cf8c380589142b148f942b062f485cefc8d9c`: loma más amplia y rebajada, rocas expuestas proporcionadas (factor 1,65 en lugar de 2,7), cuatro pinceladas de pigmento ajustadas y cobertura del bosque retenida desde Q. **Los 16.405 vértices O siguen idénticos**, los 62 árboles editados son exclusivamente del sector y los 156 antiguos quedan intactos. El autor nativo guardó .blend, .glb, pigmentos, máscaras y manifest `sector-authoring.json` en el artefacto.

## Ejecuciones nativas y evidencia, sin falsear aprobaciones

| Variante | SHA | GitHub Actions | Artefacto | Resultado |
|---|---|---|---|---|
| Q recuperable | `1cc169b...` | [39972701926](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39972701926) | `12510340920` | Fuente Blender Q recuperable y capturas H/V |
| S retenida | `3c47c5c...` | [39977858676](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39977858676) | `12512018068` | Pipeline SUCCESS, **18/18 PlayMode**, imágenes auténticas de Unity |
| T candidata | `7a8cf8c...` | [39979131350](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39979131350) | `12513275626` | Pipeline SUCCESS, **18/18 PlayMode**, cierre normal, imágenes auténticas de Unity |

Pruebas fallidas y reparación preservadas: `39976346632` (faltaba `Q editable path mask`) y `39978781645` (material buscaba el atlas S después del cambio de nombre a T). No se redujo ningún gate para obtener verde. Para T, el XML nativo contiene `total=18, passed=18, failed=0, skipped=0, result=Passed`.

Comparación causal Q/T: se contrastaron seis pares auténticos `Camera.Render`, exactamente misma posición, rotación, orthographic size, formato y resolución (tres escalas × horizontal 1280×720 y vertical 720×1280), verificando igualdad de los seis archivos de cámara JSON. Capturas adicionales de continuidad de zoom y panorama Blender disponibles en el ZIP de cada run. No se han transformado ni generado por IA los píxeles originales: los montajes visuales externos son simples yuxtaposiciones de PNG originales de los artefactos, no ilustraciones sustitutivas. Las diferencias RGB >5/255 ocupan aproximadamente 26,99%, 18,83%, 13,13% H y 37,83%, 16,57%, 13,83% V, respectivamente; **NO son notas artísticas**.

## Crítica artística independiente: Q frente a T

- **Mejora retenida:** la orilla O y el bosque antiguo sobreviven íntegros; el camino principal continúa en el sector, el bosque del cuadrante noroeste deja de parecer tan disgregado y la loma T tiene una transición más suave que la S. El tamaño de las rocas añadidas se vuelve menos dominante.
- **Carencias visibles:** demasiado terreno verde regular entre manchas forestales; la vegetación aún presenta perfiles, escala y densidad poco variados; los caminos siguen muy homogéneos, con bordes limpios; la loma ha ganado continuidad, pero todavía necesita sombreado pictórico y lectura topográfica más convincente. El río de O no se ha repintado; el objetivo visual del agua queda pendiente. En retrato la panorámica es un recorte legible del **sector**, no demuestra composición completa de toda Región 1.
- **Valoración provisional subjetiva:** aproximadamente **7/10 como piloto de sector**, no una puntuación del mundo completo. El salto Q → T es útil pero todavía insuficiente para certificar 7,5–8; permanece **ART_NOT_APPROVED**. Evitar nuevas microiteraciones sin un A/B macroscópico que afecte de verdad al lenguaje pictórico del suelo y la masa forestal.

## Integración, límites y siguiente criterio de decisión

- El flujo nativo usa Blender + Unity 6000.3.23f1 y supera la prueba integrada opt-in con 18/18 comprobaciones, tres escalas en ambas orientaciones y objetos/selección de la escena conservados. Las pruebas automatizadas no sustituyen la evaluación visual ni prueban la fluidez física en iPhone.
- La geometría del sector **es única y está conectada con O**, no son dos parches solapados ni un fondo 2D alternativo. La continuidad se prueba dentro de este piloto, no equivale a haber sustituido ya la geografía de producción de toda Región 1.
- **NO MERGE / NO RELEASE / NO GASTO / NO ART_APPROVED / NO IPHONE PERF CLAIM.** La decisión de expandir toda Región 1 requeriría una autorización nueva, después de alcanzar una calidad de sector verdaderamente convincente en capturas nativas.

## Referencias recuperables

- Archivo de K: `a911bd153713e3aadf7eb7422faa07d91bbf28d8`, run [39957867110](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39957867110).
- O adoptada: `feabc4d514b9ce4eed66f777d70d46b33cf167c8`, run [39965511083](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39965511083).
- I anterior: `docs/evidence/world4x-canon-20261022/RIBERA_I_K_REVIEW_20261023.md`.
- Revisión O: `docs/evidence/world4x-canon-20261022/RIBERA_K_O_REVIEW_20261023.md`.
