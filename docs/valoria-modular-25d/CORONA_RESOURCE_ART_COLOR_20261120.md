# Corona — conservar el color original de los iconos de recursos

Revisión 20/11/2026. Los iconos de la nueva familia son ilustraciones materiales completas, no pictogramas monocromáticos. El HUD heredado asignaba `woodAccent.color` a `AntiqueBrass` o `Red`, que **multiplica cada canal de la imagen original** por el tinte elegido. Esto destruye la diferencia de madera, cuerda y detalles dorados dentro del icono premium.

Se conserva `woodAccent.color=Color.white` en todos los estados y se utiliza sólo el color del **número** para indicar escasez (marfil cuando hay suficiente, rojo cuando falta). El número de madera se desplaza 4 pt para evitar que tape el aro de su icono. Los estados de la economía y Build se calculan igual, sin efectos en dinero, guardado ni mecánicas.

NUnit nativo verifica que el icono de madera real se importa y se mantiene blanco en el Canvas y que el valor deriva del `LocalGateway`. El aspecto artístico deberá aprobarse con imagen auténtica del SHA actualizado.
