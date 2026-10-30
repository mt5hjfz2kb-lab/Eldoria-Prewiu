# Corrección concreta del candidato, sin rebajar pruebas

Native40231234029: source gatePASS;60/62 PlayModePASS. El shader nuevo contenía una secuencia literal de barra inversa+n en una línea HLSL; se sustituye por un salto real. El fondo de Brecha dio error0,052788 m frente al límite0,005 m, que se mantiene.

Diagnóstico local sobre GLB bf3c0238: todos los vértices de fondo coinciden en X/Z con el terreno y están elevados0,012 m. No es pérdida de precisión: la consulta de los triángulos exportados en float32 y float64 encuentra una superficie superior0,04978 m sobre un vértice en(-150,-9,0604,-231,333). Algunas triangulaciones de las caras talladas se solapan ligeramente. La corrección de BlenderMCP proyecta solo las dos superficies minerales sobre el triángulo realmente más alto y eleva0,015 m. Se conserva íntegro el terreno, sus máscaras, pigmento, árboles y mecánicas; ningún nuevo objetivo/collider.

Se incluye la corrección acotada de sincronización: el render fuerza representación al cambiar zoom/formato o activar la vista; los objetivos conservan sus temporizadores normales. Así se evita añadir snapshots de gateway en cada fotograma ocioso. Las pruebas existentes de Camera.Render sin LateUpdate y geometría/persistencia se repetirán sobre el candidato final.

Se conserva el caché importado de Unity durante el taller MCP mediante clean:false; el paso de importación fría anterior tardó unos10 minutos, no fue un bloqueo. No cambia el contenido certificado ni una prueba.
