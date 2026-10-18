# Valoria — revisión visual independiente de la escena ABCD

Evidencia de Unity real: workflow [38974099294](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38974099294); commit de escena `faaea9fa97934207665251c2ba8919e19c583d38`; captura `master-gate-D-home-12.png` contra `master-home-12.png` de ABC, cámara ortográfica histórica exacta (NO cámara SHARP canónica).

## Veredicto
- TECH PASS: importación y escena ABCD validada en Unity 6000.3.23f1; 22 footprints, 12 treads, 19 raycast del ABC, 6 GLB distintos y 11 instancias de puerta, prefab persistente.
- LOCAL ART CANDIDATE (no aprobación formal): desde el mismo encuadre, la puerta inferior cambia los volúmenes cúbicos por dos torres y murallas de sillería, arco y rastrillo con hueco real. Las piezas reales y su aspecto se distinguen claramente frente al ABC. Sin revisión independiente formal suficiente para `ART_APPROVED`.
- FULL-SCENE ART FAIL: el paisaje continúa con plataformas/límites rectangulares, río sin orillas naturalistas, madera/plataformas planas, Bastión superior como bloque gris y ausencia de cabaña, campamento, vegetación, atmósfera y fondo. No se parece todavía globalmente a la referencia.
- MOBILE VISIBILITY FAIL en `master-gate-D-portrait-12.png`: parte de la puerta queda fuera de cuadro. Este encuadre es una prueba ortográfica histórica y **no** la cámara real del runtime SHARP, pero prohíbe declarar adaptabilidad móvil de ABCD.

## Defectos arquitectónicos e inmediatos
1. A — terreno: enorme plano plano y dos plataformas rectilíneas visibles. El soporte de colisión es correcto, el arte geológico real falta; NO añadir materiales para tapar la silueta.
2. B — puente: continuidad geométrica/raycasts PASS en el soporte provisional, pero cuerpo visible de cemento/planos y apoyos GREYBOX. Sustituir visual solamente tras comprobar puente modular genuino.
3. C — Bastión superior: bloque GREYBOX de volumen grande que domina el cuadro; requiere ensamblaje arquitectónico de una sola fortificación coherente dentro de huella real.
4. D — puerta modular: materiales oscuros/bandas repetidas y sin piedra cálida semejante a la referencia; mejorar malla/tileables/URP por comparación A/B, nunca por RGB estadístico aislado.
5. F/G — arbolado, bosques, costa, iluminaciones ambientales no iniciados a calidad final; no tapar vacío de parcelas con props.
6. H — sin PlayerState / LocalGateway / ValoriaParcelPresentation en esta escena experimental, y sin mobile device profiler. **No afirmar gameplay ni performance**.

## Reglas de continuación
Mantener el source branch separado, nunca modificar `ProductionSlice/Runtime/Valoria.unity`. Conservar todos los GLB/hashes, el prefab y la escena ABC. La próxima intervención útil es A+B: integración visual auténtica del puente y las bases geológicas, manteniendo colisiones originales y comparando la escena completa. Un PASS del renderer no es FULL SCENE PASS. La cámara ortográfica histórica no reemplaza a la perspectiva de SHARP; la migración al runtime necesita una correspondencia espacial con transform + proxies validada por separado.
