# Corona — icono de misión duplicado en obra premium

La ilustración activa `objective_rune.png` del kit Corona ya trae integrado un pendón de pergamino con el símbolo violeta del reino y un recuadro oscuro para texto. La interfaz colocaba además un `icon_task` de 28×28 sobre ese mismo pendón. La repetición oscurecía una de las pocas piezas con personalidad artística y se alejaba del acabado comercial de una misión de estrategia.

Se elimina exclusivamente el Sprite/Image adicional `Original real objective scroll` de `Modular25DInterface`. Permanecen el icono pintado en el propio archivo original, los textos vinculados a la progresión real, el `Button` y el callback `OpenSawmill`; no se tocan costes, recompensas ni escena. NUnit PlayMode impide introducir nuevamente un segundo `icon_*` sobre la placa de misión. Validación de calidad pendiente de capturas auténticas exact-SHA.
