# ELDORIA / Valoria — recuperación reproducible de 63 assets, 16-10-2026

**Autoridad:** rama `art/valoria-63-individual-quality-uplift-20261015`. Alcance *solo modelos individuales*, sin escenas Unity, Bastión I, World4X, despliegue ni `main`. Presupuesto cero.

## Evidencia recuperada y límites

- Biblioteca original de 63 GLB: run **38810636076**, artifact **12008716092**.
- Primera biblioteca 63 mejorada con capturas: run **38813966671**, artifact **12010106460**.
- Referencia real `references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg`: exportación de solo lectura run **38924618827**, artifact **12055019631**.
- 63 GLB seleccionados **provisionalmente**, cada uno con SHA256 validado y estructura glTF2 comprobada contra sus artefactos exactos. Índice independiente de 63 filas, galería de 63 triples (antes/render actual/recorte de contexto) y 9 paneles se generaron externamente como `VALORIA_BIBLIOTECA_63_PROVISIONAL_GALERIA_20261016.zip`.
- Nueva valoración conservadora de estudio: **29/63** candidatos con nota **>=6/10**, **34/63** aún por debajo; 0 ART_APPROVED y 0 pruebas Unity individuales certificadas. Las notas de estudio son orientativas, no equivalen al umbral formal 8/10 ni a aprobación de escena.
- Los recortes de la referencia son **contextuales**: muchas piezas no se distinguen separadas en la ilustración. Cuando se usa el render de la tanda 63 como imagen anterior, **no** equivale al A/B de una misma cámara de la iteración ganadora; está marcado.
- Licencias: preservadas menciones de CC0 cuando proceden de informes originales; **no afirmar verificación completa de materiales/donantes individuales sin documento original**.

## Intervenciones ejecutadas en Blender y evaluadas

| Asset | Run / Artefacto | SHA256 del nuevo GLB | Evaluación visual |
| --- | --- | --- | --- |
| `cliff_rock_column` | [38935700073](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38935700073) / 12061076273 | `5a3fa128f87a51851aaf98b7269461db23536d31302e79b6ba7d2beb7bc761ae` | **6.2** provisional (antes ~4.8), escaneo geológico real, fuente donor 38825975637; validar escala no uniforme/contactos |
| `stair_step` | [38937382306](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38937382306) / 12060709135 | `851ca00382139537f69bd0be6ff8264ea7b4e4c34f54b5de484928a1ded52b6f` | **6.1** provisional (antes ~5.0); cinco paver_flagstone de piedra independientes, 4m, macro real |
| `pine_small` | [38937297723](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38937297723) / 12060379900 | `87615440df8b19191a128512e81dd61c8d6d067d131e47a6a04d77808c49e50b` | **5.9**, más frondoso pero sigue estilizado, no pasar 6 |
| `pine_tall` | [38937297723](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38937297723) / 12060379900 | `49626e2aae2b513121c94ccc772f78aed66d14e3a4c41990c0d64b76d1c72026` | **5.9**, más frondoso pero sigue estilizado, no pasar 6 |
| `mortar_joint_recess` | [38927018153](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38927018153) / 12056429237 | `9f3ec2c79c141f1bca709ee10f39127c52ace5c9ae056611077b19a06c7c9499` | **5.8**, V13 eliminó vetas longitudinales y añadió 270 granos, pero continúa banda poco natural; V9/V11/V12 conservadas |

## Lecciones que no deben perderse

1. `mortar_joint_recess`: V9 tiene geometría estrecha válida; V11 raya el material, V12 alisa, V13 añade granos reales pero no obtiene 6; **cambiar hipótesis** hacia un mortero empacado en contacto con sillares y escala perceptiva, no repetir colores y microgranos.
2. `cliff_rock_column`: usar roca fotogramétrica CC0 real es una mejora visible respecto al poliedro. V7 ajustó dimensiones exactas; **no presupone** contacto, pivote, optimización o URP correctos.
3. `stair_step`: reemplazar una tira con textura de tabla por cinco sólidos físicos con juntas cambió su lectura funcional. Preflight detectó y corrigió la confusión Y-up glTF / Z-up Blender en el primer intento; PASS real en el segundo run.
4. Pinos: repetir/densificar una copa estilizada no resuelve automáticamente calidad de referencia, mantener notas por debajo de 6.
5. Selección de artefactos NO por fecha: dovelas V9 (#38906155308) vencen a V10 (#38907273374) por falta de cambio visual significativo; agua V10 de #38907369609 tiene GLB real válido pese a FAILURE global de la parte mortero, 6.1 provisional, URP aún no validado.
6. El archivo Excel y JSON son una **instantánea de decisiones provisionales** y no constituyen ART_APPROVED. Conservan SHA de cada vencedor y originales intactos.

## Próximo ciclo dentro de esta MISMA rama

Quedan **34** bajo 6 en la revisión estricta actual. Priorizar `merlon_low`, `arrow_slit_frame` (el panel negro de estudio no se exportó), `merlon_tall`, `brazier_pedestal` y `mortar_joint_recess` con mejoras geométricas/materiales reales y PNG A/B de la misma cámara. La comparación individual en Unity URP sigue fuera del alcance de esta orden de biblioteca y no se debe afirmar como completada.

**Hito pendiente, no alcanzado:** 63/63 >=6 provisional, luego acercarse a 8; ninguna modificación a Unity ni promoción a main.
