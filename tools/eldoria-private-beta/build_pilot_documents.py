#!/usr/bin/env python3
"""Build the pilot documents from an explicitly verified delivery record."""
import argparse
import json
import re
from pathlib import Path
from xml.sax.saxutils import escape
from reportlab.lib import colors
from reportlab.lib.enums import TA_LEFT
from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import ParagraphStyle
from reportlab.platypus import SimpleDocTemplate, Paragraph, Spacer, PageBreak, Table, TableStyle


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--evidence", type=Path, required=True)
    ap.add_argument("--output", type=Path, required=True)
    args = ap.parse_args()
    e = json.loads(args.evidence.read_text())
    if not re.fullmatch(r"[a-f0-9]{40}", e["source"]):
        raise ValueError("Exact verified source required")
    if not all(isinstance(e[k], int) and e[k] > 0 for k in ("native_run", "construction_run", "certificate_run")):
        raise ValueError("Exact successful evidence runs required")
    if e["published"] is not True or not e["published_url"].startswith("https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/"):
        raise ValueError("Do not generate a launch guide for an unpublished candidate")
    args.output.mkdir(parents=True, exist_ok=True)
    ink = colors.HexColor("#25212c")
    muted = colors.HexColor("#615a69")
    purple = colors.HexColor("#382947")
    gold = colors.HexColor("#b89251")
    styles = {
        "title": ParagraphStyle("title", fontName="Helvetica-Bold", fontSize=25, leading=29, textColor=purple, spaceAfter=13),
        "lead": ParagraphStyle("lead", fontName="Helvetica", fontSize=11.5, leading=17, textColor=muted, spaceAfter=16),
        "h": ParagraphStyle("h", fontName="Helvetica-Bold", fontSize=12, leading=16, textColor=purple, spaceBefore=13, spaceAfter=6),
        "body": ParagraphStyle("body", fontName="Helvetica", fontSize=10.5, leading=15, textColor=ink, spaceAfter=7),
        "small": ParagraphStyle("small", fontName="Helvetica", fontSize=9, leading=12.5, textColor=muted, spaceAfter=6),
        "q": ParagraphStyle("q", fontName="Helvetica-Bold", fontSize=11, leading=15, textColor=ink, spaceBefore=14, spaceAfter=5),
    }
    def p(text, role="body"):
        return Paragraph(text, styles[role])
    def page(canvas, doc):
        w, h = A4
        canvas.setFillColor(purple); canvas.rect(0, h-21, w, 21, fill=1, stroke=0)
        canvas.setFillColor(gold); canvas.rect(42, h-44, 34, 3, fill=1, stroke=0)
        canvas.setStrokeColor(colors.HexColor("#ded8e4"));canvas.line(42, 40, w-42, 40)
        canvas.setFillColor(muted);canvas.setFont("Helvetica", 8)
        canvas.drawString(42, 26, "ELDORIA  |  Piloto Bastión I-V  |  "+e["date"])
        canvas.drawRightString(w-42, 26, str(doc.page))
    def build(name, flow):
        doc = SimpleDocTemplate(str(args.output/name), pagesize=A4,
            leftMargin=42, rightMargin=42, topMargin=62, bottomMargin=55,
            title="Eldoria - "+name.replace("_", " "), author="Eldoria")
        doc.build(flow, onFirstPage=page, onLaterPages=page)
    url = escape(e["published_url"], {'"': '&quot;'})
    guide = [p("Tu primera partida en Eldoria", "title"),
        p("Queremos saber cómo se siente jugar sin ayuda. Evaluamos el juego, no a ti. Esta prueba pequeña abarca Bastión I-V.", "lead"),
        p("Duración orientativa: 25-35 minutos y 2 minutos de opinión. Puedes parar antes; no es necesario completar todo."),
        p('Acceso: <a href="'+url+'" color="#583d78">abrir la beta de Eldoria</a>. No necesitas instalar una APK. Es un enlace público de GitHub Pages compartido con el grupo; no está protegido por invitación.'),
        p("Antes de empezar", "h"),
        p("Usa el dispositivo acordado con el responsable. Elige un seudónimo para el nick. Si aparece una opción de estadísticas, participar es opcional."),
        p("Si ya tienes una partida, avisa al responsable. No borres los datos del navegador para reiniciarla por tu cuenta."),
        p("Juega a tu manera", "h"),
        p("Durante los primeros 10 minutos intenta avanzar sin instrucciones externas. Construye, mejora y explora lo que el juego vaya desbloqueando. Anota cualquier momento en el que no sepas qué hacer."),
        p("Cuando hayas completado una acción y sea seguro, cierra la pestaña y vuelve a entrar una vez. Comprueba que se mantienen tu nick y tu progreso."),
        p("Si se congela o reinicia", "h"),
        p("Anota qué estabas haciendo, la pantalla, el dispositivo y el navegador. Si puedes, toma una captura del juego. Vuelve a entrar una vez y comprueba el progreso; conserva los datos del navegador."),
        p("Al terminar", "h"),
        p("Responde a las preguntas de la siguiente página con el código que te indique el responsable (P01-P15). Devuelve las respuestas por el canal acordado. No necesitas incluir nombre, teléfono ni correo."),
        p("Una explicación como «no entendí para qué servía» es más útil que una respuesta amable.", "small"),
        PageBreak(), p("Tu opinión en 2 minutos", "title"),
        p("Código de prueba: __________    Dispositivo y navegador: __________________", "small")]
    questions = [
        ("1. ¿Entendiste qué hacer durante los primeros 5 minutos?", "Sí / Más o menos / No"),
        ("2. ¿Dónde te sentiste perdido o bloqueado?", "Indica la pantalla, qué esperabas y qué intentaste."),
        ("3. ¿Qué te interesó más?", "Construcción, mundo, combate, personajes, historia, expediciones u otra cosa."),
        ("4. ¿Se congeló, reinició o cerró el juego?", "No / Sí. Si ocurrió, ¿en qué momento?"),
        ("5. ¿Se conservaron el nick y el progreso al volver?", "Sí / No / No lo probé"),
        ("6. ¿Cómo valorarías la diversión y el aspecto visual?", "Diversión: 1-5. Visual: 1-5. Explica brevemente por qué."),
        ("7. ¿Volverías a jugar otro día?", "Sí / Quizá / No. ¿Qué te haría volver?"),
        ("8. ¿Qué cambiarías primero?", "Una sola mejora, por pequeña que sea.")]
    for title, detail in questions:
        guide.extend([p(title, "q"), p(detail, "small"), Spacer(1, 11)])
    build("eldoria_guia_piloto_para_jugadores.pdf", guide)
    phone = e.get("physical_iphone_verified") is True
    internal = [p("Salida del piloto", "title"),
        p("Uso interno. Estado de la entrega publicada y condiciones para la primera tanda de jugadores.", "lead"),
        p("DECISIÓN: "+("ABRIR PRIMERA TANDA DE 2-3 JUGADORES" if phone else "VERSIÓN PREPARADA; VALIDACIÓN FÍSICA DEL IPHONE PENDIENTE"), "h"),
        p("La prueba de WebKit automatizada no se presenta como una prueba física en Safari de iPhone."),
        p("Entrega identificada", "h"),
        p("Fuente del motor: "+escape(e["source"]), "small"),
        p('Canal: <a href="'+url+'" color="#583d78">GitHub Pages - beta integrada Vigilia</a>. Enlace público, no acceso privado individual.'),
        p("Pruebas verificadas", "h"),
        p("Unity y campaña/guardado I-V: ejecución "+str(e["native_run"])+". Capturas reales del nick, Vigilantes y Exploradores revisadas por separado."),
        p("WebKit: nick, avance del diálogo, construcción y persistencia al recargar. Ejecución "+str(e["construction_run"])+"."),
        p("Certificado de regresión sobre la fuente exacta: ejecución "+str(e["certificate_run"])+"."),
        p("Dispositivo y lanzamiento", "h"),
        p("iPhone físico: "+("confirmado para esta entrega." if phone else "pendiente en esta entrega. Comprobar entrada, ciudad/mundo, orientación, volver desde segundo plano y conservación del progreso.")),
        p("Tras esa comprobación, enviar a 2-3 personas. Revisar bloqueos y guardado antes de ampliar a 5 y después a 10-15."),
        p("Seguimiento", "h"),
        p("Utilizar los códigos P01-P15 y la hoja de seguimiento existente. Registrar versión, dispositivo, punto de abandono y respuesta. La guía recoge opinión manual; no presupone que haya telemetría o encuestas automáticas activas."),
        p("Un bloqueo que impida avanzar o una pérdida de progreso detiene la ampliación. Las observaciones visuales y de comprensión alimentan la siguiente mejora.", "small")]
    build("eldoria_checklist_interno_antes_de_testers.pdf", internal)


if __name__ == "__main__":
    main()
