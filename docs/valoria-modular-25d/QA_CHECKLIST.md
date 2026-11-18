# Gates de aceptación (NO marcar sin evidencia)
- [ ] Editor Unity 6000.3.23f1 importa y compila la escena real.
- [ ] El runtime crea 441 células sin juntas visibles al mover cámara.
- [ ] Cámara horizontal/vertical, ratón y pantalla táctil: pan, zoom y hit test correctos.
- [ ] No aparece segundo presentador o HUD legacy (bootstrap guard).
- [ ] Obra construida con `Build:sawmill`, coste real, temporizador UTC, transición de parcela.
- [ ] Rechazo auténtico del gateway por madera insuficiente; sin consumo.
- [ ] Persistencia efectiva tras cerrar/reabrir juego; no doble comando ni doble pago.
- [ ] Personaje pasa delante y detrás del mismo árbol con oclusión coherente.
- [ ] Los habitantes evitan obstáculos visuales; no flota ninguno.
- [x] Implementación fuente: estados previo / obra / construido tienen sprites de autoría separados conectados al estado canónico. **Sin validación de render ni PlayMode.**
- [ ] Capturas Unity reales 390×844 y 1170×2532, horizontal y acercamientos.
- [ ] Capturas reales disponibles/obra/construido y personaje/oclusiones.
- [ ] Vídeo auténtico de cámara pan/zoom, recorrido de personaje; no movimiento de imagen plana.
- [ ] UI minimalista sin solapamientos y legible a tamaño móvil real.
- [ ] Build WebGL aislado, ejecutado e interactuado.
- [ ] Perfil iPhone/Android: medición de memoria, CPU, GPU, drawcalls y resolución.
- [ ] Revisión artística por el propietario con capturas y build; aprobado o reprobar con causas.
- [ ] Solo si todo supera la evaluación: decidir congelación del método (NO antes).

## Estado de entrega a fecha 2026-11-18
- **Escrito:** cuatro scripts runtime y editor con llamada a Unity, fuente gráfica original, escena YAML.
- **Pendiente de verificación:** todo lo anterior, incluida compilación, aspecto, estado y vídeo.
- **Bloqueo para CI nativa:** `windows-runner-heavy` reservada por otro trabajo vivo; no se invade.
