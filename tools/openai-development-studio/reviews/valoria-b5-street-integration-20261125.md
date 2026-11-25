# Primera misión real V2 — continuidad urbana de Bastión V

**Estado:** diagnóstico aceptado con reservas; no mejora artística implementada ni aprobada.

- Fuente vigente al encargar: `5ef2aee67cf8fde57ba2fc4b3d929b517ee67944`, rama experimental Valoria.
- Evidencia original: [run Unity 45638738009](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/45638738009), capturas `stage-5-calle-mayor.png` y `stage-5-postigo.png`, 780×1040. SHA de imágenes y código contrastados antes de gastar.
- [Run Studio 45683014884](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/45683014884), review y archive exitosos. [Recibo permanente](../receipts/run-45683014884.json). Perfil económico; 4 solicitudes reales y 0 costes desconocidos. Duración API medida 69 s; job de revisión 89 s, sin Windows. No representa tiempo humano ahorrado ni facturación total.

| Rol | Modelo | USD estimados |
|---|---|---:|
| Dirección | Terra | 0,00729675 |
| Programación | Sol | 0,0186355 |
| Auditoría | Terra | 0,0119655 |
| Coordinación | Luna | 0,000811925 |
| Total | | 0,038709675 |

Acumulado: 19 solicitudes, 0,228557225 USD estimados; 4,31 USD en reservas cerradas del tope piloto 5 USD. Saldo de reserva 0,69 USD, no saldo de la cuenta. El CSV recibido solo contiene intervalos UTC, no importes, moneda ni proyecto: conciliación oficial no posible. CSV no publicado.

## Diagnóstico contrastado

Las dos capturas reales muestran una calle mayor legible y un frente del Postigo donde el arco, la sombra y el encuentro con la calzada resultan menos claros. Se acepta como prioridad local, no como prueba de que toda la ciudad presente el mismo defecto. El auditor no confirma ninguna franja de hierba separando la entrada; no debe inventarse un criterio de 0 píxeles. El coordinador Luna mantuvo esa reserva correctamente, sin transformar hipótesis en aprobación.

## Contraste adicional de fuente (sin nuevas llamadas)

- `ValoriaUrbanPlan.cs`: `BoroughStreets[0][2]` y `BoroughStreets[5][0]` coinciden en `PostigoLane(0)`; la segunda secuencia pasa por `UpperRow`. Los centros de calle sí coinciden en planta. `PosternWalk()` recorre ese arco; conservar todos sus puntos.
- `Modular25DLandscape.cs:113–119`: Calle del Postigo y Paso bajo el postigo se crean como mallas separadas, anchos 1,04 y 0,66 respectivamente. `AddRoad` (223–304) reduce alfa en inicio y fin de cada cinta; a inicio multiplica por 0,04 y recupera opacidad en 0,63 unidades. Orden de dibujo -4100.
- `Modular25DArchitecturalShadow.cs:14–23,65,103–133`: los tres módulos del Postigo disponen de perfiles de contacto; la sombra proyectada se dibuja a -3900. La rotación por `StreetAxis` se aplica al módulo PortalesDeOficios, no al Postigo. No inferir que sea un error: los módulos pueden haberse dibujado ya con esa orientación.
- Las dos fuentes ofrecen hipótesis comprobables, no una causa establecida en cámara. No procede añadir otro sprite de suelo, modificar posiciones ni cambiar indiscriminadamente la sombra.

## Próxima intervención del equipo Valoria

En su workstream canónico, preparar una prueba A/B diagnóstica: mismo SHA base, escena, cámara y recorrido; identificar en el render la malla de calle y sombra de `CalleDelPostigo`. Comparar original frente a dos variantes aisladas: continuidad alfa únicamente en la unión existente y sombra proyectada desactivada únicamente para diagnosticar. Revertir ambas, escoger solo la causa sustentada por evidencia y después implementar la corrección mínima. Si ninguna cambia materialmente la lectura, descartar y examinar huella/arte de origen. Preservar coordenadas, rutas, controles y lógica; probar el mensajero y verificar en cámara fija y movimiento. No aprobar por cantidad de píxeles ni reutilizar el cambio rechazado MILL_YARD.

**Resultado de la fase:** el flujo V2 económico ha realizado un encargo actual, archivado automáticamente sus costes y conservado las reservas del auditor. No hay comparativa controlada de Luna frente a Sol sobre esta misma misión; no afirmar ahorro medido ni calidad artística ganada. No se modificó la rama jugable, no se publicó ni usó el PC.
