# Inventario visual para una futura migración

| Capa | Presentación candidata | Representación a retirar o redirigir en el punto de entrada |
|---|---|---|
| Parcela y edificio | ValoriaUrbanPlan.City y los SpriteRenderer de Modular25DWorld | Anclajes fijos en la escena urbana anterior y coordenadas históricas del Aserradero. Asignar cada selección al mismo identificador canónico. |
| Bastión I–V | bastion_i_hd y Progression/bastion_2…bastion_5 | Desactivar el presentador visual de la ciudad sustituida al instalar el presentador modular. Conservar los originales archivados y sus GUID. |
| Cuartel, Granero, Cantera | Progression/barracks, granary y stoneworks | Suprimir representaciones duplicadas y desplazar los patios asociados a las nuevas parcelas. |
| Aserradero | Aserradero disponible, obra y terminado según ParcelBuildingStates | Retirar el rectángulo de selección absoluto y la colocación independiente de operario y acopio. |
| Terreno y calles | BastionMeadow, HaulersEarth, OrganicCobbleRoad, DistrictPaving y materiales UrbanRoad y DistrictGround | Cesar el muestreo de DistrictStreets.png y retirar las cintas, caminos y suelos de la presentación anterior. |
| Entorno | Piezas recuperadas de arboleda, muro, pozo, huerto, carro, valla y lonja, inscritas en el plano | Retirar CasasDeOficio del patio de armas, el material de huerto antes de su etapa y las distribuciones anteriores de matojos y escombrera. |
| Cámara e indicadores | Cámara ortográfica 2.5D, selección y nombres anclados al edificio, guante anclado al siguiente control visible | Retirar el requisito de encuadrar todos los edificios al inicio y cualquier posición de interfaz absoluta asociada a la escena sustituida. |

Una futura integración debe comenzar por el presentador que realmente activa la escena de ciudad de main y verificar que existe un solo HUD y una sola autoridad visual. Cambiar una referencia del arte no implica cambiar el servicio de guardado, las reglas de construcción ni las localizaciones del mundo compartido.

Las casas del camino, la armería y el puesto de abastos son tres ilustraciones originales creadas para Valoria el 23 de noviembre de 2026 usando como referencia las cinco arquitecturas importadas del proyecto. Cada una tiene su propia textura con transparencia, registro de contacto y escala. La armería requiere un Cuartel terminado y el puesto requiere un Granero terminado en Bastión IV o V.
