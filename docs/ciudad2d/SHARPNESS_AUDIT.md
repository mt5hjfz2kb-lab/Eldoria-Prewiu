# Auditoría de nitidez 2.5D — 17 noviembre 2026

## Evidencia auténtica, sin extrapolaciones

- Fuente A: Unity Editor, RenderTexture original 1170×2532, workflow [44013497619](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44013497619), `3d4d2b4`, importación CompressedHQ.
- Fuente B: Unity Editor, RenderTexture original 1170×2532, workflow [44018257414](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44018257414), `f1d3e6e`, importación **Uncompressed** + sin mipmaps; 12 PNG con SHA verificados, 4 sprites importados auditados.
- Misma escena, cámara y resolución de salida para el par de referencia. Diferencia absoluta promedio RGB en Bastión ≈0.022/255; en Cuartel ≈0.711/255; Granero ≈0.558/255. El cambio en varianza del Laplaciano: Bastión **+0.04 %**, Cuartel **+0.48 %**, Granero **+0.47 %**, terreno **+0.01 %**. Mediciones sobre recortes equivalentes; son indicadores de cambios locales de bordes, **no una puntuación de calidad percibida**.
- **Conclusión validada:** la compresión no explica por sí sola la suavidad que se observa en varios edificios. Hace falta mejorar o reautorizar el arte fuente y considerar su tamaño proyectado en pantalla. No se ha demostrado una ganancia visual grande mediante un cambio simple de importación.

## Experimento siguiente (no aprobado automáticamente)

- Se ha añadido `City2DArtBake.cs`: genera **solo en el sandbox** cuatro copias RGBA `Generated/*_Crisp.png` desde los PNG existentes, con contraste de bordes alfa-consciente de intensidad moderada. Los originales permanecen intactos. No aumenta la información real que pueda contener la fuente.
- `City2DSceneBuilder` enlaza esas copias a Cuartel, Aserradero, Granero y Cantera, conservando Bastión e interacciones y la progresión de parcelas. `City2DNativeEvidence` audita los assets importados y genera además una captura de detalle con **zoom nativo 1,42×** a 1170×2532 (no un zoom de imagen en Photoshop).
- La prueba nativa debe completar **13 capturas con SHA** y controles de snapshot/read-only, y marcar explícitamente `art_approved=false` y `device_qa=PENDING`. Si se detectan contornos artificiales, ruido o efecto de «pegatina», **revertir o reducir** el tratamiento sin tocar el arte principal.
- Cinco ilustraciones de edificios de mayor riqueza arquitectónica se han creado **fuera de Unity como candidatos** (Cuartel, Aserradero, Granero, Cantera y futuro Salón de Héroes) y mantienen un ZIP de revisión. **No se han instalado en la escena ni aprobado**, y el Salón sigue sin una construcción desbloqueable persistida.

## Pendientes antes de aprobar arte de producción

1. Capturas nativas original vs. contraste, inspección de fuentes y recortes 1:1, no solo «CI verde».
2. Ajustes de consistencia entre edificios, suelo, luz, sombras de contacto y eliminación de bases ortogonales artificiales.
3. Prueba real en iPhone y Android de legibilidad, presupuesto de memoria y compresión de producción (ASTC).
4. Integración de candidatos arquitectónicos únicamente tras verificar recortes y render en Unity, sin inventar lógica ni reglas de desbloqueo.
