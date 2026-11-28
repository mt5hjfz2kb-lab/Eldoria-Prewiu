# Ciudad y mundo · auditoría del código y la escena

Main d74f1a29d5472e9037d8534142768d4c13667ddd. Experimental 6057c2604de84ae27f47ede44eafa294c96bcd24. Análisis estático de los objetos y métodos de ambos árboles. Capturas, comportamiento de selección y memoria residente requieren ejecución.

## Escena y raíces serializadas de Main

Unity/ProjectSettings/EditorBuildSettings.asset:8–10 habilita Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity. En la tabla se cuentan todos los descendientes por m_Father en el YAML de esa escena, incluyendo la raíz y los objetos inactivos. El número de renderizadores representa componentes serializados, no draw calls. La fila WorldRoot incluye las tres siguientes.

| Enlace en la escena | fileID GameObject | Objetos | MeshRenderer | SkinnedMeshRenderer | MeshCollider + BoxCollider |
|---|---:|---:|---:|---:|---:|
| Ciudad Bastión I |1998548337|2|2|0|1|
| Ciudad Bastión II |1590401695|3|2|0|1|
| Aserradero construido |126412279|8|6|0|0|
| Aserradero en obras |2026306124|5|3|0|0|
| Cuartel construido |2065097092|10|7|0|0|
| WorldRoot |1816922689|868|515|3|6|
| Bastión I en mapa |540662847|2|1|0|0|
| Bastión II en mapa |1917807149|2|1|0|0|
| Aserradero en mapa |298479635|6|5|0|0|

La escena tiene 1.104 GameObjects, 651 MeshRenderer, tres SkinnedMeshRenderer, dos cámaras, 16 MeshCollider y 11 BoxCollider serializados. Se han identificado en el YAML los enlaces de ciudad (líneas 36824–36830), parcelas (36854–36884) y mundo (36919–36927). El valor WorldHome del componente serializado es (1018,0,30.000002), distinto del inicializador C#. Los campos Ground, Available y Target de ambas parcelas están vacíos en esta escena. SceneSplats también está vacío. No deducir un visual o un collider activo leyendo únicamente el tipo C#.

## Puntos de sustitución y sus dependencias

Las rutas siguientes son relativas a Unity/Assets/Eldoria, salvo los scripts de ciudad donantes que están bajo Unity/Assets/Experimentos/ValoriaModular25D/Scripts/.

| Elemento | Instanciación y selección en Main | Equivalente en Experimental y límites |
|---|---|---|
| Bastión I–IV | ArtTests/ValoriaReferenceClean/ValoriaReferenceCleanGameplay.cs:111–114 enciende las dos raíces serializadas. Integration/BastionIIIWorldPresentation.cs:29–41,202–210 añade acopios de nivel y elección de IV. | Modular25DArt.cs:121–138 asigna bastion_i_hd y Progression/bastion_2 a bastion_5; conservar estado y parcela nativos. Medir individualmente colisionadores de las raíces I y II antes de retirar sus MeshRenderer. |
| Aserradero | Scripts/Presentation/ValoriaParcelPresentation.cs:36–79 actualiza ramas según ParcelBuildingStates; los componentes de muestra vienen de la escena. | Modular25DArt.cs:275–281,322–346 y Modular25DWorld.cs:313–325 identifican sawmill_i_hd y sawmill_under_construction. «Disponible» carece de enlace visual en el contrato serializado y «bloqueado» no equivale a «disponible». |
| Cuartel, granero, cantera | El cuartel usa la raíz de parcela. Integration/BastionIIIWorldPresentation.cs:21–28 crea y selecciona el granero y sus existencias. Integration/BastionIVVWorldPresentation.cs:22–36,79–92 crea el hotspot stoneworks y encamina los estados I–V. | Modular25DArt.cs:129–133 y Modular25DWorld.cs:82–84 apuntan a Progression/barracks, granary, stoneworks. Comprobar la distinción de Main entre base, mejoras, disponibilidad y nivel V antes de retirar assets. |
| Edificios de nivel V | Integration/BastionIVVWorldPresentation.cs:22–49,70–90 carga los GLB V y calcula posición y ancho desde los límites del arte anterior. En el mapa adapta el tamaño del collider a la representación según el zoom. | La colocación experimental deriva de ValoriaUrbanPlan y no preserva por sí sola ni centros ni dimensiones. Fijar las referencias numéricas originales de foco y de selección antes de retirar geometría; la equivalencia debe medirse en el juego. |
| Cámara, selección y navegación | Integration/ValoriaIntegratedWorld.cs:124–207 conserva el presentador, desactiva Canvas y raycaster heredados y configura las dos cámaras. :454 alterna la cámara por mapa; :625–703 usa exclusivamente MeshRenderer/SkinnedMeshRenderer para el enfoque. :714–737 usa raycast capa 8 y límites del engendro. La ciudad usa ValoriaReferenceCleanGameplay.cs:124–130. | Modular25DWorld.cs:56–109 crea su cámara, su Input, su HUD y su sesión. No importar la raíz. Un SpriteRenderer por sí solo deja de contribuir a los mismos límites de enfoque en Main. |
| Terreno, montañas, bosque y ríos 4X | World4XCanon/RegionOneCanonicalLandscape.cs:153–190 configura la capa 29, la cámara y la carga. :239–285 construye RegionOneBlenderLandscape, ReferenceNodes, ResourcePlacement, ResourceModels y CreaturePopulation. :393 restaura renderizadores anteriores al destruirse. | World4XCanon/RegionOneModularArt.cs:18–25 fuerza forceRenderingOff sobre el arte antiguo y construye otra representación. No copiar este instalador; conservar por separado las mallas imprescindibles para consultas y retirar las texturas y los materiales obsoletos de la ruta empaquetada. |
| Nodos y poblaciones | World4XCanon/RegionOneResourcePlacement.cs:72,139–150,336–354 usa producción/región, consultas sobre el terreno y crea WorldHotspot y BoxCollider para cada ID. Integration/BastionIIIWorldPresentation.cs:43–60,202–221 crea y gobierna los cinco encuentros del capítulo, sus temporizadores y colisiones. | RegionOneModularArt.cs:53–68 asigna Timber, Quarry, Wolf, Boar, HunterCache, AshStalker, Fault, Camp, Spawn y Ruin a sus IDs. Conservar cada ID, transform, máscara de selección, condición de aparición y cantidad de unidades. |
| Ciudad estratégica y puente | World4XCanon/RegionOneReferenceNodes.cs:25–31,67–92 carga recursos y anclajes, genera las mallas e instala el puente. Integration/ValoriaIntegratedWorld.cs:384 activa las variantes y el aserradero. | RegionOneModularArt.cs:37–49 define puntos de contacto para uso mundial. Su material y su geometría orientada a cámara (70–118) tienen límites artificialmente amplios: no pueden alimentar el enfoque determinista de Main sin medición separada. |

Los paths Integration/... de la tabla parten de ArtTests/ValoriaReferenceClean/. Para demostrar la paridad espacial faltan snapshots por fase y por escala de los pares ID/posición/hitbox, las matrices de ambas cámaras al seleccionar y cerrar, los trayectos con igual reloj/estado, la lista efectiva de renderizadores/materiales y las dependencias incluidas en cada player. La única geometría candidata identificada expresamente como soporte técnico es el collider de consulta del terreno; corresponde verificar por separado qué mallas adicionales requieren las colisiones y el enfoque de cada entidad antes de retirar su representación. Ninguna exención de empaquetado se da por aprobada.
