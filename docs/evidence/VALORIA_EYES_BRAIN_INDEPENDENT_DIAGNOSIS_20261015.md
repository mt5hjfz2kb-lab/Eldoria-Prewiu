# VALORIA — Veredicto independiente del examen de ojos y cerebro (15-10-2026)

**Estado:** DIAGNÓSTICO TERMINADO / PERCEPCIÓN PARCIAL DEMOSTRADA / DIRECCIÓN ARTÍSTICA AUTÓNOMA RECHAZADA.
**Autorización:** solo lectura; sin construir, editar ni publicar modelos. Se conserva la pausa artística de `main/AGENTS.md`.
**Fuente congelada:** `Valoria_Tower_CohV1.glb`, SHA-256 `e7592eaaf10824dadf6a272551518456c1dc054fdbd24f769fae90d0b4b76869`.
**GitHub Actions:** 
- [Prueba inicial multivista](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38697797097): ambos modelos devolvieron esqueletos JSON vacíos.
- [Prueba multivista en lenguaje natural](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38698360100): ambos declararon no poder ver las cuatro imágenes.
- [Control de color real](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38698927339): ambos identificaron correctamente rectángulo rojo sobre fondo azul. Ollama confirmó capacidad `vision`. La decodificación de imágenes aparece en sus registros nativos.
- [Ensayo de observación de torre única](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38699375691): ambos reconocen forma de torre, troncos, cubierta y escalera exterior, pero vuelven a rechazar la consulta conjunta de cuatro imágenes.
- [Ensayo final de cuatro vistas por separado y síntesis de razonamiento](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38699407600): ambos redactaron cuatro descripciones y tres propuestas, SIN ediciones Blender. El flujo técnico terminó SUCCESS, pero eso NO certifica juicio artístico.

## Imágenes genuinas y trazabilidad final

Los tres PNG generados con cámara independiente se conservan en el artefacto del run 38699407600:
- `front_three_quarter.png`: `b3ed7c16463fec3f14d593c31cde8a61881c0f500460b44616c6255fbbeb6145`
- `reverse_three_quarter.png`: `7e545017808900824ce3f24875afdebedeb18321e488eef135664418cfc74327`
- `elevated_roof.png`: `c84326835d21eacb52c5accdcf1a80696ef522a2aa78698022c53c0e08bcdbf5`
- Referencia de estilo aprobada utilizada por los modelos: SHA-256 `8ae6fb0e6949dd4f7d37b38767282e5f1fb6089e117a29f362945c1edfa66689`.

Los textos brutos completos con sus hashes figuran en el artefacto de Actions:
- 4B síntesis: SHA-256 `e70a4b67a0db53d1e894014f8cb8a7ff5cf87d73d38ebaf7110234d3d358e251`
- 8B síntesis: SHA-256 `c1d032c3ce5cfa749fbd63c941f98382e6f886151201291d5326742c20b236`

**Nota de integridad:** los manifiestos `diagnosis-*.json` del run final incluyen al final un escape literal `\\n` después del cierre del objeto. Es un error menor de serialización del evaluador, no del modelo; corregir en futuras ejecuciones o retirar ese sufijo antes de analizarlos. No alterar ni ocultar las respuestas brutas ni sus hashes.

## Comprobaciones independientes frente a las imágenes

**Ojos / observación:** se acredita que ambos modelos pueden ver un control de colores y reconocer una torre en una imagen; también describieron rasgos visibles en las tres vistas individualmente (troncos horizontales, tejado de madera inclinado, escalera exterior, barandillas y plataforma). En una consulta multimodal compleja de cuatro imágenes simultáneas, ambos fallaron: negativa genérica a mirar o JSON vacío. El sistema no es todavía robusto a esa composición de contexto.

**Cerebro / criterio arquitectónico:** RECHAZADO para los dos modelos. El 4B confundió diferencias entre descripciones de perspectivas distintas con cambios físicos del mismo tejado y de la escalera; infirió sin evidencia que se hubieran modelado versiones distintas por vista. El 8B afirmó falsamente que una base cuadrada/cúbica no admite cubierta inclinada y propuso eliminar la escalera exterior en favor de una interior. Las imágenes muestran una estructura común, techo inclinado con cúspide y la misma escalera exterior desde diferentes ángulos. Una base rectangular SÍ admite múltiples formas de cubierta inclinada; no se debe rediseñar la pieza basándose en esas inferencias.

**Revisión de cambios recomendados:** las propuestas de modificar radicalmente tejado, mover escalera dentro y tratar la barandilla como varias estructuras incompatibles carecen de sustento geométrico y pueden dañar componentes protegidos. No son planes aceptables para editar Blender. El 8B, aunque utiliza más terminología, no demuestra mejor criterio espacial que 4B. No hay evidencia de aprendizaje de pesos, validación Unity, ni mejoras A/B sobre esta torre.

## Diagnóstico causal y siguiente configuración exigible

El bloqueo NO es ausencia de conexión con Windows, Blender, Ollama, ni incapacidad general de ver píxeles. Se localiza en **interpretación multivista y verificación de hipótesis espaciales**, con alucinaciones producidas al sintetizar descripciones parciales y confundir perspectiva con geometría. La capa actual de calidad confunde «respuesta extensa» con «razonamiento correcto» si no hay revisión externa.

Antes de volver a modelar:
1. Mantener una imagen por inferencia y observaciones con procedencia explícita, pero NO dar por cierta cada descripción: contrastarla con la topología real, cámaras y vistas antes de sintetizar.
2. Exigir un **revisor/director artístico independiente, más capaz de razonar sobre arquitectura 3D**, que invalide errores físicos (por ejemplo, «una base cuadrada no admite tejado inclinado»), hipótesis que contradicen fotografías y cambios en partes protegidas.
3. Pasar al ejecutor únicamente instrucciones visuales específicas y respaldadas por hechos, con veto a operaciones destructivas por afirmaciones no verificadas. Guardar el caso de error como memoria de revisión, sin afirmar reentrenamiento.
4. Repetir un benchmark de criterio ciego y comprobar reducción de errores espaciales antes de levantar la pausa y autorizar edición sobre una copia.

**Decisión final:** `PERCEPTION_PARTIAL_PASS` / `SPATIAL_REASONING_FAIL` / `ART_AUTHORING_NOT_AUTHORIZED`. Ningún `ART_APPROVED` ni `CALIBRATION_VALIDATED`. No reanudar por éxito de CI ni por longitud de respuesta.
