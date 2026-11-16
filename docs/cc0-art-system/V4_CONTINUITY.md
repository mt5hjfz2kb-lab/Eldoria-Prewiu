# Continuidad — Director V4 de Eldoria CC0 Art System

**Nombre canónico del experimento:** Eldoria CC0 Art System — Sistema Artístico Gratuito Reproducible.  
**Workstream-ID:** `eldoria-cc0-art-system-20261116`.  
**Rama exclusiva:** `experiment/eldoria-cc0-art-system-20261116`.  
**Estado del objetivo:** ensayo de dirección artística, no beta, no arte aprobado.  
**Fecha:** 16 de noviembre de 2026.

## Contexto de decisión

- La composición mixta con paquetes Quaternius reales demostró que los modelos existen, son importables y cubren sistemas de economía, pero **no** unificó calidad y escala. Quaternius se mantiene como referencia/donante condicionado a licencia, no se incorpora en V4.
- La reconstrucción original Director V3 ya tiene cuatro industrias diferenciadas y progresión Bastión I–V, pero las capturas verdaderas de Blender siguen mostrando cubiertas sencillas, mampostería plana, relieve/paisaje con aspecto de maqueta y una silueta central demasiado modular.
- Las imágenes generadas por IA en el chat previo son **referencias de intención**, nunca modelos de producción ni prueba de que Unity alcance ese aspecto.

## Estado técnico de los recursos

**Origen real V3:** [GitHub Actions #43866662062](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43866662062), artifact `eldoria-director-v2-real-art`, fuente editable `DirectorV2_Master.blend`, cinco modelos originales `DirectorV2_ValoriaStage1...5.glb`. Quaternius: 0 modelos en estos GLB.

**Nueva versión V4:**
- `tools/cc0-art-system/director_v4_surface_and_architecture.py` importa físicamente cada GLB V3; modifica UV y materiales; genera texturas propias procedurales de piedra, pizarra, madera, terreno; añade geometría propia en Bastión V, aserradero, granero, cuartel y cantera.
- `.github/workflows/cc0-director-v4.yml` regenera fuentes y capturas con Blender real en Ubuntu público, **sin reclamar Windows ni modificar main**.
- **Primera ejecución V4 solicitada:** [#43870376065](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43870376065). Comprobar la conclusión real antes de citarla como PASS.
- `tools/cc0-art-system/batch_director_v4.py` prepara mezcla de mallas estáticas por distrito/material sin deformar polígonos, para probarla DESPUÉS de cerrar el V4 original.

## Qué debe conservarse

1. Una única escena espacial, misma cámara y coordenadas para Bastión I y V.
2. Cinco modelos físicos por nivel, no dibujos alternativos. `DirectorV4_ValoriaStage1...5.glb`, editable V4 Stage V y texturas.
3. Aserradero, granero, cantera y cuartel como edificios visualmente diferenciados: maquinaria/madera, silos/grano, grúa/roca y defensa/entrenamiento.
4. Caminos orgánicos integrados en la misma malla de terreno; no volver a travesaños negros, parcelas o plataformas artificiales.
5. Paleta coherente: piedra ceniza envejecida, pizarra azul oscuro, madera roble, hierba apagada y luz cálida.
6. Sin importación de imágenes 2D conceptuales como falsos modelos, sin imágenes de IA presentadas como renders Unity/Blender.
7. No publicar archivos de terceros en el repositorio público sin permisos de distribución de fuentes acreditados; no revivir el Bastión vetado `Valoria_HeroBastion_v1.glb`.
8. Sin modificar otras ramas, escenas canónicas, sistemas de juego/guardados ni Windows compartido.

## Próxima decisión autónoma (gates obligatorios)

1. Verificar que run #43870376065 realmente termine, y recuperar sus PNGs/GLBs desde el artifact.
2. Inspeccionar físicamente V4 Bastión I/V en la misma cámara. Marcar defectos visibles y corregirlos de manera causal; no anunciar calidad premium sin evidencia.
3. Probar la agrupación `batch_director_v4.py` con polígonos/UV inalterados y reducir mallas; no confundir eso con FPS ni estabilidad iPhone.
4. Unity real solo cuando Windows esté **legítimamente reservado** por este trabajo, que actualmente NO es el caso. No ejecutarlo ni tomar prestada la reserva de `art/visual-reboot`.
5. Poner en la rama las capturas aplanadas y el SHA real de la fuente, de modo que sobrevivan al vencimiento de los artifacts de GitHub Actions.
6. Reconciliar antes de cambiar de chat `main/AGENTS.md`, `main/pipeline/active-workstreams.json`, el HEAD de esta rama y este documento.

**Resultado afirmable por ahora:** implementación V4 y workflow solicitados. **Resultado no afirmable:** calidad visual V4, import Unity, ritmo en iPhone, certificación comercial integral.
