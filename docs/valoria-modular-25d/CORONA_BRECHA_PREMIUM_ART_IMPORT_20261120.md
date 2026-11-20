# Corona de la Brecha — 15 sprites de acabado ilustrado (source-backed)
Fuente: imagen original generada expresamente para Eldoria y separada en sprites RGBA independientes, con originales SVG conservados como kit de seguridad. No contiene arte de Whiteout Survival, Kingshot ni Top Heroes.
La carpeta `CoronaBrechaPremium` registra 15 PNG binarios cuyos SHA-1 Git fueron verificados al cargar cada blob y conserva GUIDs y 9-slice. Requisitos mínimos:
- Integración posterior en `Modular25DInterface` exclusivamente; no alterar `LocalGateway`, cámara, mundo, economía ni guardado.
- Inspeccionar los ocho contornos 9-slice a tamaños UI reales, especialmente portrait ring 27px, world rune 34px, resource 43px y CTA 42px.
- `artApproved:false` y `unityRuntimeVerified:false` hasta ejecutar Unity 6000.3 + Chromium sobre el HEAD exacto.
- Mantener comparación real con la interfaz SVG anterior y no dar por mejor una textura únicamente por ser más detallada.
