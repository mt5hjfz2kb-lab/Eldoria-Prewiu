# Familia modular de Valoria — cierre de fuentes al mínimo Puerta V3

La revisión independiente aprueba las siete fuentes de esta familia al mínimo visual de la Puerta V3 congelada. El dictamen autoritativo es `art-source/valoria/production/asset-family-v2/quality-v3/visual-review.json`, ligado a los SHA-256 de cada .blend, GLB y captura exacta. El candidato revisado es `8fa28681dcd6c4bd62f71713ba02d2a14d981016`; producción final: run 38184974032. Esta aprobación corresponde a fuentes reutilizables; integración en Unity y composición del mundo objetivo 9,5/10 son fases posteriores.

## Entrega y uso

Siete .blend editables, veintiún GLB autosuficientes en LOD0/1/2, texturas PBR, catorce reimportaciones individuales, antes/después y una escena de contactos/repetición. La guía `Valoria-Familia-V3-LEEME.md` y el paquete `Valoria-Familia-V3.zip` se generan únicamente cuando la revisión, autoría genuina y hashes pasan el guard de entrega.

Los módulos usan metros y Z vertical en Blender; GLB usa el eje Y vertical estándar. Encajar con traslaciones y rotaciones. El cuerpo del acantilado tiene extremos coincidentes y cerrados: las caras planas laterales son cortes de unión destinados a repetición o a enterrarse en terreno. La cota superior real del pavimento no supera 25 mm. La heráldica, paños, metal fino y llamas están protegidos en la reducción de LOD. Elegir distancias y colisiones durante la integración en motor.

## Autoría y decisiones

El artista externo Qwen3-VL 4B Instruct produjo siete diagnósticos/diseños, operaciones y proporciones antes de la construcción por BlenderMCP. Los primeros tres diseños válidos se conservaron del run 38181988592; los cuatro restantes se completaron en 38183538588. Se verificó que las siete respuestas originales coinciden con los planes usados y que las siete entradas incluyen la imagen exacta del propietario, SHA-256 `de476e0516f744f9cd899419404971fa5e8d42bd49f2f7db7670d1cb325f8a4f`.

La reconstrucción sustituyó la deformación global de piezas por mampostería trabada con huecos reales, restauración íntegra de la torre/heráldica V3 y pavimento rectangular apoyado. La primera reimportación del escarpe seguía leyendo blanda: la dirección independiente cambió a planos angulares dominantes, fisuras hundidas y escaneo como relieve secundario, conservando las proporciones del artista. El montaje corrige el giro izquierdo y comprueba el camino apoyado. Cinco fuentes ya revisadas permanecieron idénticas en esta pasada final.

El artista no se aprueba a sí mismo. `evidence.json`, `candidate-proof.json` y registros de elaboración conservan su estado previo a revisión; `visual-review.json` es la aprobación final independiente. Las pruebas técnicas provisionales marcadas como fixture no están en la entrega.

## Verificación

- Siete fuentes revisadas contra la Puerta V3 y la referencia exacta del propietario.
- Veintiún GLB sin errores ni advertencias en la comprobación estructural.
- Catorce reimportaciones exactas: LOD0 y LOD2 de cada pieza; heráldica, formas y materiales conservados.
- Núcleo rocoso cerrado y perfiles extremos idénticos comprobados durante la construcción.
- Relieve superior real del pavimento: 22,62 mm; contrato máximo: 25 mm.
- Tres vistas de montaje revisadas, con siete tipos de asset, repeticiones y Puerta V3 intacta. El suelo de apoyo es contexto de comprobación.
- Blender 4.3.2 y BlenderMCP fijado al commit `7a0373ec9199183cb460068c4f96aed9c579fb4f`. Cero créditos de generación pagada.

## Procedencia y continuidad

Materiales, torre, heráldica y braseros proceden de las fuentes V3 existentes de Eldoria. Las nuevas superficies de roca/tierra son PBR portátiles generadas para esta familia. El relieve secundario conserva el donante CC0 verificado de https://opengameart.org/content/free-photogrammetric-rock ; su SHA-256 de fuente queda registrado en la evidencia del acantilado. Los .blend y exportaciones definitivos están conservados en la rama `art/valoria-asset-family-v2-ai-authorship-20261011`.

La producción continúa en el workflow canónico existente. El modo `delivery` empaqueta los archivos revisados sin volver a inferir ni regenerar geometría. El ZIP incorpora manifiesto de hashes y pruebas de integridad. No se ha modificado el juego publicado ni promovido esta rama a main.
