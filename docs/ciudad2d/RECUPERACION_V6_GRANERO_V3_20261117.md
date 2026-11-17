# Recuperación y revisión Valoria 2.5D V6 → Granero V3 · 17/11/2026

**Ámbito:** rama aislada `art/ciudad-2d-hitos-1-5-20261116`, carpeta `Unity/Assets/Experimentos/Ciudad2D/`. No se cambia `main`, las escenas de producción, los datos, la economía, el combate, los guardados ni las reglas de construcción. La revisión artística sigue pendiente; no se ha publicado ninguna beta.

## Autoridad técnica recuperada y conservada

- Ejecución Unity real: [44105205248](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44105205248), SHA fuente `747b24ffe5c47b9f1070c06d2285fbb0aaf1dbe0`. CI terminó bien, incluyó una escena jugable separada PremiumV2, PlayMode real, selección/HUD/sombras y comparativas A–F.
- Archivo V6 sin repetir Editor: [44109813247](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44109813247); commit derivado `14dc9a6c835706e1d55fb724d73c1dec209f3ce8`. `QA/ArtQualityV6_NATIVE_EVIDENCE.json` conserva 60 SHA256 de los PNG auténticos del render de Unity, 10 PNG originales sin alterar y siete montajes etiquetados como derivados.
- Originales y referencias permanentes en `ArtBible/Masters`; candidatos `Sprites/Candidates/{Cuartel_Premium_V2,Aserradero_Premium_V2,Granero_Premium_V1,Cantera_Premium_V1}.png`; máscaras, fuentes y sombras siguen independientes.
- El Bastión V **desbloquea la Cantera, no la construye**: `stoneworks` aparece `Available`; los frames de Cantera terminada son un fixture de presentación QA etiquetado y no alteran partida ni política.
- El archivo nativo completo supera la descarga directa; su autenticidad y hashes se auditaron en la ejecución Ubuntu de archivo sin volver a invocar Unity.

## Diagnóstico de arte observado en capturas nativas V6

- **Bastión**: jerarquía central sólida y acabado piedra/tejado coherente con los maestros. No está autorizado rediseñarlo.
- **Cuartel**: el objetivo/entrenamiento existe en la fuente, pero es pequeño en vista global; revisar lectura funcional sin hacer pasar un edificio residencial por cuartel.
- **Aserradero**: troncos y bancada de trabajo visibles; la sustitución del plinto del control A por la silueta premium ha mejorado de forma clara el contacto. No generar de nuevo sin defecto justificado.
- **Granero**: la fuente V1 ya contiene sacos, fardos, trigo y carro. El defecto en vista a 390×844 es la **lectura aparente** de una residencia/posada, no ausencia de esos elementos en la fuente. Evitar añadir un cultivo rectangular artificial.
- **Cantera**: la fuente V1 tiene grúa y frente de piedra. El control A presenta una losa rectangular aparente; F mejora la integración. La cantera terminada únicamente existe en el fixture explícito.
- **Conjunto**: diferencia aún perceptible de exposición/contraste entre el Bastión más pétreo y los edificios premium más cálidos; no solucionar con filtros globales. Densidad 23 vs 32 props ya contrastada y cambios imperceptibles de terreno descartados anteriormente.

## Experimento V3 — exclusivamente presentación, no promocionado

- Clase nueva `Scripts/Editor/City2DPremiumPreviewV3.cs`, copia aislada del contrato PremiumV2; escena nueva `QA/Experimento_Ciudad2D_PremiumV3.unity` (generada por Unity, nunca sustituye original ni V2).
- **Una sola variable artística:** `Granero_Premium_V1.png` se muestra con factor de escala aparente `1.15` y desplazamiento horizontal de `-0.23` unidades mundo respecto a V2; reajuste del apoyo inferior y de su sombra. RGBA fuente, reglas de fase, colisionadores y otros tres edificios inalterados.
- Pruebas implementadas: `QA/run_premium_preview_v3_native.ps1` y `QA/PlayModeProof/City2DPremiumPlayModeTestsV3.cs`, con 11 capturas en 390×844, 1170×2532 y 844×390, detalle por edificio, y prueba real PlayMode de HUD del Granero, visibilidad por etapa, Cantera construida exclusivamente en QA y cuatro sombras.
- Ejecución inicial: [44112680434](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44112680434). Error de fuente V3 detectado en la revisión estática y corregido (directorio de evidencia V3 y salto de comentario C#) antes de su siguiente prueba; no reutilizar el primer resultado como validación de la corrección.
- Ejecución con fuente corregida: [44113135720](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44113135720). **Estado de aceptación: pendiente de inspección real Unity A/B.** Si recorta la estructura, se pisa el camino, interfiere con parcelas o no mejora la lectura móvil, **rechazar la variante** y conservar V2.

## Puertas de aprobación no satisfechas automáticamente

1. Evaluación visual humana A/B con igual cámara/etapa, no numérica por brillo o nitidez.
2. Verdadera interacción y sombreado bajo Unity PlayMode de **la variante final**.
3. Estabilidad y memoria en iPhone físico y, si procede, Android físico (todavía pendientes).
4. No fusionar con `main` ni publicar beta sin autorización.
