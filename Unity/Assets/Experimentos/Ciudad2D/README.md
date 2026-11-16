# Experimento Ciudad2D (rama aislada)

Ruta real del proyecto: `Unity/Assets/Experimentos/Ciudad2D`. NO se ha añadido al Build Settings, modificado escenas antiguas ni cambiado ProjectSettings.

**Fuente de imágenes:** siete PNG de la conversación (Terreno + Bastion, Cuartel, SalonHeroes, Aserradero, Granero, Cantera). Los siete archivos PNG binarios originales están ahora versionados en esta rama Git con sus bytes verificados mediante los SHA-1 de objeto Git. Los scripts y la escena marcadora están también registrados en la rama.

Al importar los siete PNG ya versionados en `Sprites/`, Unity reconstruye esta escena automáticamente mediante `City2DSceneBuilder`, sin tocar otras escenas. Existe el comando `Eldoria > Experimentos > Ciudad 2D > Reconstruir escena`.

**Cámara:** ortográfica, terreno completo, sorting de transparencia **local de la cámara** con eje (0,1,0); el proyecto exige no modificar `ProjectSettings/GraphicsSettings.asset`.
**Sprites:** textura Sprite 2D/UI, compresión HQ, mipmaps desactivados, pivot Bottom en edificios y Center en terreno.
**Interacción:** Input System UI module + Physics2DRaycaster + Collider2D + IPointerClickHandler; consola `[Ciudad2D] Nombre — nivel 10` y rebote.
**Posiciones:** Centro Bastión, superiores Cuartel y Salón, medios Aserradero y Granero, inferior Cantera.

**Estado comprobado:** los 7 PNG y alfa están comprobados con Pillow; montaje 2D generado para inspección visual. **No hay captura de Unity ni Play mode probado**; la escena real y sus interacciones requieren validación en Unity Editor; los ficheros están integrados pero todavía no hay certificación Play Mode. El archivo .unity de la rama contiene únicamente una marca, para no fingir que la escena fue serializada y comprobada por Unity.

`CityBuilding.cs` no fue adjuntado como archivo C#; se escribió explícitamente según los requisitos.
