# Eldoria — GPT-6 frente a Qwen: ensayo real de dirección artística de torre (15/10/2026)

## Ámbito, autorización y protección
Ensayo aislado de coste 0 autorizado por el propietario: evaluar si GPT-6, a través del estudio existente y GitHub Actions/Blender, puede diseñar una mejora artística más fiable que Qwen local. **No implica reanudación de producción, ni aprobación de candidatos, ni Unity.**

- `main` autoritativa verificada: `c8b75a9534fb18a7568d6cde520722973f5c4369`.
- Rama exclusiva: `diagnostic/gpt6-vs-qwen-valoria-tower-20261015`; sin integración ni alteraciones en `main` o la rama de otro artista.
- Torre congelada: `e5fc3715356378abc9a5d472071499c738f5e8ed`, `Valoria_Tower_CohV1.glb`, SHA-256 `e7592eaaf10824dadf6a272551518456c1dc054fdbd24f769fae90d0b4b76869`.
- Referencia visual: `references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg`.
- Cámara de revisión fija: `[6,-9,6]` mirando a `[0,0,0.5]`, ortográfica 6, render 960×720.
- Único mesh heredado: 11.400 vértices / 5.338 caras; límites WORLD XYZ: x=[-1,224,1,224] m, y=[-1,325,1,325] m, z=[0,3,271] m.
- Preflight y vistas reales confirmadas mediante [run 38772456928](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38772456928). Los dos candidatos GPT-6 surgieron tras comprobar esas vistas originales.

## Candidatos, resultados y evaluación real

| Versión | Proveniencia | Blender ejecutado | Impacto A/B | Protecciones | Evaluación artística |
|---|---|---|---|---|---|
| Qwen histórico (14B) | [Run 38754996414](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38754996414), [artefacto 11982678775](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38754996414/artifacts/11982678775) | SÍ | 2,56858% (umbral de canal >10) | fuente original conservada | RECHAZADO: dos maderas previstas resultaron un bloque beige de aspecto chimenea y escasa coherencia |
| GPT-6 intento 1, fascia bajo alero y dos soportes | [Run 38773419997](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38773419997), [artefacto 11992911632](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38773419997/artifacts/11992911632), `gpt6-plan.json` | SÍ, seis operaciones, GLB `960089b3260dd768a7a76dcfef43aea4331975273ee8459e6ab4e53593e8fb0b` | 0,9219% (umbral por píxel: diferencia RGB máx. >12) | ambas zonas protegidas PASS, original sin cambios | RECHAZADO: banda rectangular oscura sobre borde del tejado, diagonales apenas legibles; crítico Qwen-VL coincide en falta de integración visual |
| GPT-6 intento 2, montantes verticales y dintel frontal (cambio **de hipótesis**, no mover el primer alero) | [Run 38774194109](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38774194109), [artefacto 11993315174](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38774194109/artifacts/11993315174), `gpt6-plan-r2.json` | SÍ, seis operaciones, GLB `c24ff05f367e72b30daa85cfc5856b4289987315287e3fac331f6df6db8f6096` | 3,0773% (umbral >12) | ambas zonas protegidas PASS, original sin cambios | **NO APROBADO**: intervención visible, pero tres prismas muy simples; no evidencia de acabado profesional ni validación Unity |

**Advertencia A/B:** El antiguo Qwen y GPT-6 no son un ensayo simultáneo ciego con idéntico brief y presupuesto de inferencia; comparten fuente y cámara, pero sus hipótesis y autorías difieren. Los umbrales de píxel no son idénticos entre ambos detectores. Los porcentajes demuestran visibilidad, **no** belleza, y no deben interpretarse como ranking de modelos. Los dos GPT-6 sí usaron la misma configuración de comparación.

## Hallazgo forense: el revisor visual también comete errores
- En la segunda ronda, la revisión independiente emitida por `qwen3-vl:8b-instruct-q4_K_M` describió «diagonal supports» y «new fascia» como causa de rechazo. Sin embargo, el plan **r2 solo contiene dos postes verticales y un dintel**; no contiene diagonales ni la fascia antigua. Las imágenes reales de r2 corroboran esos objetos.
- El chequeo geométrico de Blender calculó contacto real, con distancia al mesh fuente aprox. de 0,00156 m (dintel), 0,00383 m (post izquierdo), 0,00608 m (post derecho), frente a tolerancia 0,07 m. El evaluador los llamó flotantes. **No confundir medición de proximidad con unión estructural perfecta**, pero su conclusión no estaba demostrada y la descripción de la geometría era falsa.
- Consecuencia: `independent-critic-raw.json` de r2 es una evaluación provisional **NO FIABLE como certificación visual**; impone nueva verificación de correspondencia PLAN ↔ RENDER ↔ VEREDICTO con objetos/atributos concretos, prueba de error de identificación y criterio independiente adicional. No reintentar automáticamente prompts de critic ni autoaprobar el candidato.

## Conclusión técnica y artística
1. **Hipótesis técnica confirmada:** ChatGPT/GPT-6 puede aportar planes originales por GitHub y hacerlos ejecutar **en Blender del runner Windows** sin API de pago, a través del ejecutor tipado existente; salida BLEND, GLB y A/B reales. La fuente quedó intacta. Esta prueba no requiere BlenderMCP; su paridad con Blender directo ya estaba demostrada en [run 38766521237](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38766521237).
2. **Hipótesis artística NO confirmada:** ninguno de los candidatos supera de forma verificada el mínimo profesional. GPT-6 mejoró frente a un bloque beige en la inteligibilidad de elementos intencionados, pero esto es un juicio cualitativo no ciego, no una victoria medida.
3. **Cuello de botella precisado:** calidad de las operaciones/modelos semánticos (prismas muy simples) y fiabilidad de revisión visual, además de la toma de decisiones espaciales. No es una prueba de incapacidad de ejecutar Blender.
4. **Estado obligatorio:** `ART_APPROVED=false`, `CALIBRATION_VALIDATED=false`, `ART_PRODUCTION_SCALABLE=false`; cero Unity, publicación ni merge a `main`. El contrato requiere tres mejoras consecutivas certificadas, nota >=8/10 y evaluación Unity cuando se autorice.
5. Próxima hipótesis, solo tras coordinación con el propietario de la línea: uniones/juntas de madera de formas reales y texturas/UV específicas, más revisor visual que deba enumerar objetos realmente presentes con procedencia, abstenerse cuando falle la correspondencia y permitir una segunda revisión independiente. Medir calidad, no solo píxeles.

Todo el ensayo, scripts/planes y pruebas están aislados. No interferir con `diagnostic/valoria-ai-eyes-brain-20261015` ni cambiar su ownership.
