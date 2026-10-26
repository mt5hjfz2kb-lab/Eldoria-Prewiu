# Revisión forestal independiente — capturas auténticas

Control: main112, fuente `8d3f25053f6817dcbf34d07da426b284fdd12908`,
run40387268720, artefacto12686696359.
Candidata visual: fuente nativa `606663d96147526f0353eb622753f417e303f3b1`
(merge de d7b699 con ed0d1f), run40394742870, artefacto12687737377.
ZIP SHA256 `1af27627f73115fdb38d1b1dfa97e5bd5dd9239c216002efa83dd7620956df56`.
Los seis PNG se descargaron, abrieron y cotejaron con los originales canónicos.
Cada hash nativo y las seis poses, proyecciones y dimensiones coinciden con el control.
El diff Unity entre control y candidata solo contiene la poda y sus dos archivos de
pruebas. El borde atmosférico del PR112 ya está en ambos lados.

## Observación y decisión

El primer intento (run40382253603) ocultaba árboles individuales por hash.
Fue RECHAZADO por el autor y, separadamente, por el agente revisor: apenas mejoraba
el primer plano y perforaba las mismas masas dispersas sin retirar el modelo artificial.
Sus fuentes y capturas se conservan; no se fusiona como resultado aceptado.

La corrección siguiente retira el modelo redondeado reciente y las cohortes completas
Context/Belt/Regional/Castle. No añade geometría ni cambia recursos, anclas o navegación.
Retiene 1.796 árboles continentales y 462 Pilot_Pine, con 31 ecotonos de abeto.

## Veredicto visual independiente

Revisor: agente `/root/forest_visual_review`, distinto del autor de la intervención,
con acceso de lectura a los PNG originales y al tríptico. No utilizó el porcentaje
de poda ni el resultado técnico como criterio estético.

| Vista | Juicio fundamentado |
| --- | --- |
| Cercana horizontal | Mejora clara: desaparecen grandes copas redondeadas del primer plano; río, caminos y acceso al puente se leen mejor. Los pinos superiores mantienen encuadre. |
| Cercana vertical | Sin cambio significativo; conserva bosquetes, torre, castillo y accesos. No se observa regresión. |
| Regional horizontal | Mejora clara: espacios abiertos continuos al sur de Valoria y junto al río; recursos y amenazas destacan sin competir con árboles claros. |
| Regional vertical | Mejora clara: mejor separación entre recursos, criaturas y bosque; río y caminos más fáciles de seguir. |
| Panorámica horizontal | Mejora moderada: se distinguen mejor los claros y las masas conservadas; siguen distribuyéndose bosques por la región. |
| Panorámica vertical | Mejora moderada: menos ruido central, corredores más legibles e identidad territorial conservada. |

**RETENER para el alcance forestal. PASS técnico final confirmado.** El revisor
no considera que toda la región se haya convertido en una pradera vacía: la apertura
es fuerte alrededor de Valoria, pero permanecen bosquetes periféricos, ribera y pinos
cercanos. Castillo, Brecha, ríos y recursos conservan su lectura e identidad.

Persisten repetición de coníferas, masas algo dispersas y aspecto de puntos lejanos.
No se alcanza toda la riqueza compositiva del canon. La mejora concreta no equivale
a aprobación artística global, nota8/10, FPS móvil ni prueba de transiciones animadas.

## Corrección técnica y procedencia

Run40394742870: integración I–II44 checks y guardado intacto; EditMode26/26;
PlayMode63/65. Los dos fallos están identificados: un test seguía exigiendo la poda
anterior25–42%; otro buscaba LODGroup sin incluir padres inactivos cuando el arranque
estaba en ciudad. Se corrigieron en cf174b0ab45dedd0b5baeb2929cf69279d91f2eb,
sin cambiar arte/runtime: censo exacto3251 retirados, parejas near/far conservadas,
462pinos protegidos y consulta explícita de padres inactivos.

La repetición final run40399630175 concluyó SUCCESS:65/65 PlayMode,26/26 EditMode,
44 comprobaciones I–II y guardado preservado. Artefacto12688613577,
ZIP SHA256c5d4138d7f07d3d58d96f83e31efa2475b5f57e0e57ec269ba5f9c26a3782638.
Fuente nativa297fc583ab87c8c280ec8f1f5c90fee64a574fc3; PR113 fusionado como
c448cb783c512377702b452bd3bb6a7459c1c135. Ambos tienen exactamente el mismo árbol
Git d2a71ea45473f5ad07a1882bd30d5d79a65240da: el código integrado es el probado.
El blob de RegionOneProductionLod es fdbbd41366232ca5e226de2a45ccf7e0d16c3712 tanto
en las imágenes revisadas como en el HEAD final. La ejecución visual anterior con
fallos se conserva sin presentarla como suite verde.

La autoría Blender/MCP registrada en ARTISTIC_PROCESS_CURRENT pertenece a la fuente
histórica conservada. Esta intervención trabaja exclusivamente la visibilidad Unity;
no invoca Blender ni inventa una sesión MCP. El inventario de herramientas no ofrecía
BlenderMCP y no hay Blender/Unity locales; las pruebas Unity se ejecutan en el runner
Windows existente. Input histórico BLEND SHA256fd3063a792eedf77d6908fe9f5d894b47514717720a5d03ee1ac4e08f0690cce
y fuente actual SHA2560e5a0a9df65799aaa8ccfccce12680eaf9a8bec9e94090e08991418ae87bcdb9
materializados y verificados. GLB conservado SHA256d23114d40777e014dac5de7b18078143b9d32e66df79798871751b9b1d9c6738.

## Coste y reversión

3251/5047 árboles continentales dejan de renderizarse (64,4%), con decisión idéntica
near/far; no se suman ambos LOD como árboles simultáneos. Los grupos retirados dejan
su LODGroup deshabilitado. Esto reduce trabajo potencial de representación, pero
las mallas y objetos siguen residentes: no se afirma memoria liberada ni FPS medidos.
Las envolventes de navegación se conservan; los claros visuales no son nuevas rutas
ni plazas edificables. Revertir el commit forestal aislado restaura visibilidad sin
recrear fuentes, `.meta`, recursos o guardados. No revertir PR111/112 para ello.
