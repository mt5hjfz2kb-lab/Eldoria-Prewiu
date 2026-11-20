# Corona de la Brecha — corrección causal del falso fallo WebGL (20-11-2026)

**Origen verificable:** prueba real de Chromium/Unity [44802943006](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44802943006), compilación exacta `70444c18217d3a519f313c840e9fb3b13f2fa033`, capturas `390x844-real-sawmill-build-panel.png` y `390x844-real-build-command.png`.

## Observado, distinto de lo supuesto

La imagen del panel disponible muestra **230 de madera** y la posterior al comando muestra **150** y la obra iniciada. El comando funcionó y el saldo cambió; el test de navegador informó erróneamente `woodBalanceBuildChange.ratio = 0.000` porque analizaba filas entre 10% y 13% de altura, por encima de los glifos numéricos.

Hash SHA256 de las capturas genuinas:
- Panel antes de gasto: `aa1181ce5a599a1e2d7f62abdfa0477f89ae837d612839d444f3c57ec8f0cb06`
- Panel después de gasto: `9357cf99870a4d56978f3199dd930d61199874a961a208da026e3400b2dabdc4`

## Corrección implementada y comprobación matemática

Se mantiene x=13%..24% y se mueve el intervalo vertical a **12%..15%**. Comparación de RGB por píxel con criterio `sum(abs(delta RGB)) > 80`:

| Región | Píxeles cambiados | Total | Proporción |
| --- | ---: | ---: | ---: |
| Anterior (10–13%) | 0 | 1075 | 0% |
| Corregida (12–15%) | **59** | 1075 | **5.49%** |

El umbral de GitHub Browser QA es al menos 25 píxeles y 3.4%, por lo que este **par de capturas** cumple. La comprobación de persistencia tras recarga y el segundo tamaño **requieren ejecución real del navegador**; no se deducen del éxito de este cálculo.

## Regla a futuro

No confundir falso fallo del ROI con fallo de construcción ni declarar browser PASS sin recarga y capturas completas. Si se modifica la geometría del HUD, revisar el ROI a partir de imágenes auténticas. No modificar LocalGateway, saldo canónico ni `main`. Se prepara una prueba independiente de Chromium sobre el artefacto WebGL previamente generado, sin reservar de nuevo el runner Windows.
