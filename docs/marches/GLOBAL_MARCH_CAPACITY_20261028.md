# Eldoria — capacidad y tres marchas simultáneas

## Regla del propietario (28/10/2026)
- **Máximo inicial: exactamente tres operaciones de marcha en curso** en conjunto. Son ranuras del reino, no tres por subsistema.
- La **capacidad de tropas es por marcha**, ligada al héroe seleccionado y al Cuartel. Tropas entrenadas del reino y capacidad de mando son magnitudes distintas.
- Las tropas reservadas para una expedición dejan de estar disponibles para otras, vuelven al finalizar y no deben duplicarse tras recarga ni reintento de comandos.
- Los aumentos futuros por investigaciones y talentos se incorporarán a **esta regla compartida**, no a contadores paralelos por modo. No hay investigaciones ficticias desbloqueadas en este commit.
- Valores actuales de capacidad como **balance provisional**, sin nuevo gasto: `40 + 20 × nivel del Cuartel + 5 × (nivel del héroe − 1)`. Héroes nivel1–20; nivel efectivo procede de experiencia existente (80 XP por nivel, Lyra también considera su progreso narrativo). La progresión general final de héroes aún se debe unificar.

## Implementación de fuente
- `MarchCapacityRules` en `GameState.cs` es la única autoridad de capacidad/ranuras. Se conserva `PlayerState.March` para partidas antiguas y se añaden **dos** `ExtraMarches` guardadas.
- `LocalGateway` reserva, verifica y devuelve unidades en las marchas del mundo sin saltar el límite; la marcha configurada acepta también la sintaxis `march-main:T1:T2:T3:heroe` a nivel de gateway para permitir composiciones futuras. Las pantallas existentes siguen usando su selector T1 y héroe.
- La interfaz actual de preparación muestra cupo calculado, nivel, poder y **ranuras ocupadas / 3**. El mapa desbloquea la salida de una segunda/tercera marcha y conserva marcadores independientes de ruta y tiempo.
- `VigilantBattle` usa la marcha preparada si es válida o un destacamento limitado si no se ha preparado ninguna; se bloquea si las tres ranuras están ocupadas, sin duplicar una reserva de tropas ya activa.
- `ScoutDispatch` ocupa una ranura hasta su finalización real UTC y **reserva 12 tropas por encargo** como valor temporal de diseño. `Synchronize` devuelve las tropas exactamente una vez al terminar (sin exigir que se recoja el botín). Guardados antiguos sin reserva son compatibles.
- El Poder Total sigue contando tropas en marcha, no solo aquellas que siguen disponibles en Valoria.

## Evidencia y pendientes obligatorios
- `.github/workflows/march-capacity-contract.yml`: compila el dominio y gateway reales con .NET8. Demuestra 3 expediciones simultáneas con 3 lotes de tropas distintos, 4ª rechazada, vuelta offline, recompensas idempotentes, composición fuera de capacidad rechazada, aumento por héroe/Cuartel, tres contratos simultáneos y devolución única.
- Los tests originales de retención y gestión se mantienen, no se relajan ni desactivan.
- **Aún debe superarse PlayMode real de Unity** con marcadores 1/2/3 simultáneos y los flujos en la escena existente, y reconciliar la rama con PR120/122 en `main` antes de decir implantación final / distribuir APK.
- No se han creado nuevos edificios, monedas, héroes ni niveles de arte. Ninguna de las tres marchas actuales es un ejército separado ni una recompensa ficticia.
