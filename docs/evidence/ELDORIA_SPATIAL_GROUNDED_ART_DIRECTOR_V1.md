# ELDORIA — Director artístico con evidencia espacial (fase diagnóstica v1)

**Autoridad:** `main/AGENTS.md` y `docs/ELDORIA_ART_QUALITY_ACCEPTANCE_CONTRACT_V1.md` del HEAD de main. **Presupuesto:** 0 €. **Estado:** sin permiso para modelar; esta implementación no edita activos ni crea una nueva rama de producción.

## Qué problemas corrige

Prueba anterior: Qwen 4B interpretó distintas vistas de una única torre como geometrías diferentes. Qwen 8B dijo que una planta cuadrada no puede sostener una cubierta inclinada y propuso llevar al interior una escalera protegida. La interpretación de una fotografía no puede tomar precedencia sobre una malla GLB y las cámaras originales.

**Arquitectura instalada sobre la rama diagnóstica existente:**

```
Torre ORIGINAL SHA congelado
    │
    ├─ Blender read-only → spatial-grounding.json
    │    • vértices y dimensiones en WORLD XYZ (+Z arriba)
    │    • normales de triángulos por estratos de altura
    │    • islas topológicas sin modificar mallas
    │    • matrices originales y procedencia de cada cámara
    │    • límites expresos de lo que esos datos NO prueban
    │
    ├─ Vistas reales (artefacto Actions 38699407600)
    │    └─ Qwen 4B/8B observan cada vista por separado
    │
    └─ Director IA fundamentado, prueba de seguridad:
         • contrasta hechos Blender con descripciones visuales tentativas
         • examina 7 afirmaciones de prueba adversarial
         • propone mejoras SOLO como hipótesis, nunca autoejecutables
         • evaluación independiente decide si existe verdadera calidad
```

**Separación de responsabilidades:** Blender aporta datos estructurales medidos; Qwen aporta hipótesis/ideas artísticas; el gate físico invalida inferencias contradictorias y cambios de partes protegidas; el revisor independiente decide calidad contra capturas. Ni reglas ni Qwen deben declarar `ART_APPROVED`.

## Revisión visual independiente de la torre original (fotos auténticas)

Inspeccionadas las vistas `front_three_quarter.png`, `reverse_three_quarter.png` y `elevated_roof.png`, del mismo GLB.

- **Hecho visible con alta confianza:** perímetro de troncos horizontales con cruces de madera salientes en esquinas; una escalera exterior conduce a un descansillo con barandillas; cubierta de tablas/piezas solapadas con alero visible y líneas convergentes hacia una parte alta. Estos rasgos aparecen desde ángulos distintos sin implicar que la geometría cambie.
- **Hecho visible:** los tres renders están en iluminación/escenario de inspección neutros, muy grisáceos; no es seguro concluir de ellos la calidad definitiva de texturas, PBR o paleta de Unity.
- **No verificado:** la geometría exacta de apoyos ocultos, si cada tablón de tejado toca físicamente su estructura, intersección real entre descansillo/muro, materiales finales del juego y calidad desde la cámara nativa Unity.
- **Inferencia prohibida:** «planta rectangular obliga a cubierta plana»; «punto de vista diferente = cambio de modelo»; «es obligatorio trasladar la escalera al interior»; «cajas delimitadoras solapadas prueban contacto».

## Examen de aceptación de comprensión espacial

Se comprueba por separado para 4B y 8B si aplican los hechos en siete trampas:
1. Cubierta necesariamente plana sobre planta rectangular → **RECHAZAR**.
2. Diferencia de perspectiva prueba geometría distinta → **RECHAZAR**.
3. Mover escalera protegida al interior → **RECHAZAR**.
4. Conservar puerta/escalera/UV en diagnóstico → **ACEPTAR**.
5. Tres cámaras = un mismo GLB original → **ACEPTAR**.
6. Contacto probado solo con AABB → **RECHAZAR**.
7. Caras superiores inclinadas = demuestra exactamente tejado a dos aguas → **RECHAZAR**.

Estos casos miden **uso de contexto verificado**, no nivel profesional de creatividad. Un resultado alto no implica talento artístico ni licencia para editar.

El informe automático conserva entradas exactas, identificación del modelo, respuestas textuales y decisiones. La evaluación independiente de las imágenes auténticas sigue pendiente hasta revisar el artefacto exacto del nuevo ciclo. Un `SUCCESS` técnico no significa aprobado artístico.

## Contrato operativo para futura acción artística (aún NO activado)

Cada propuesta debe citar una evidencia visible (imagen y ángulo), dato espacial Blender con fuente y SHA, razonamiento arquitectónico del contacto real, cambios protegidos = ninguno, efecto perceptible desde cámara de juego y nivel de incertidumbre. Si falta un dato o se contradice la evidencia, resultado `NO_EDIT_INSUFFICIENT_EVIDENCE`. Si una corrección supera el diagnóstico y el propietario levanta la pausa, Qwen/Blender podrá modificar **una copia**, seguido de A/B real, revisión independiente y las tres mejoras sucesivas del contrato principal.

**No se descarga ningún modelo adicional, no hay entrenamiento de pesos, no se usan servicios de pago y no se modifica Unity ni la obra.**
