"""Source-pinned Corona secondary UI missions. No direct game writes."""
import json,sys,hashlib,struct,subprocess
from pathlib import Path
REPO="mt5hjfz2kb-lab/Eldoria-Prewiu"
SHA="5ef2aee67cf8fde57ba2fc4b3d929b517ee67944"
RUN=45638738009
SPECS=json.loads("[{\"id\":\"hero-training\",\"images\":[\"sistema-heroes.png\",\"sistema-training.png\"],\"objective\":\"Héroes y entrenamiento: priorizar identidad, retrato, coste, tiempo y cantidades; no sustituir retratos ni alterar tropas.\",\"files\":[[\"Unity/Assets/Eldoria/UI/ValoriaRoyalHUDV2/Runtime/RoyalGoldV3ManagementScreens.cs\",\"void ApplyMilitaryComposition\"]]},{\"id\":\"scout-vigilant\",\"images\":[\"sistema-scouts.png\",\"sistema-vigilants.png\"],\"objective\":\"Exploradores y Vigilantes: preservar mapa, enemigos y composición; mejorar contraste de indicadores, estados, temporizadores y panel inferior.\",\"files\":[[\"Unity/Assets/Eldoria/UI/ValoriaRoyalHUDV2/Runtime/RoyalGoldV3EarlyRetentionScreens.cs\",\"static RoyalButton Button\"]]},{\"id\":\"daily-name\",\"images\":[\"sistema-daily.png\",\"nombre-inicial.png\"],\"objective\":\"Tesoros diarios y nombre: dar protagonismo al cofre y a la arquitectura, sin rediseñar ni alterar los premios, teclado o confirmación; proponer pruebas de secuencia real.\",\"files\":[[\"Unity/Assets/Eldoria/DailyTreasures/Runtime/DailyTreasurePanel.cs\",\"static Sprite LoadUnifiedResourceEmblem\"],[\"Unity/Assets/Experimentos/ValoriaModular25D/Scripts/Modular25DInterface.cs\",\"void OpenNamesIfNeeded\"]]},{\"id\":\"mail-construction\",\"images\":[\"sistema-mail.png\",\"temporizador-en-parcela.png\"],\"objective\":\"Buzón y construcción: jerarquía de mensajes, fecha, estados, adjuntos, legibilidad de temporizadores y materiales discretos. Conservar acciones, datos y guía.\",\"files\":[[\"Unity/Assets/Eldoria/UI/ValoriaRoyalHUDV2/Runtime/RoyalGoldV41MailboxScreen.cs\",\"void Layout()\"],[\"Unity/Assets/Experimentos/ValoriaModular25D/Scripts/Modular25DInterface.cs\",\"private void Refresh(PlayerState s)\"]]}]")
def sha(b):return hashlib.sha256(b).hexdigest()
def gh(p):return json.loads(subprocess.check_output(["gh","api",p],timeout=60))
def main(ident,root,out):
    root,out=Path(root),Path(out)
    spec=next(s for s in SPECS if s["id"]==ident)
    run=gh(f"repos/{REPO}/actions/runs/{RUN}")
    assert run["head_sha"]==SHA and run["status"]=="completed" and run["conclusion"]=="success"
    assert any(a["id"]==15037184906 and a["name"]=="ciudad-region-recorrido" and not a["expired"] for a in gh(f"repos/{REPO}/actions/runs/{RUN}/artifacts?per_page=100")["artifacts"])
    assert json.loads((root/"journey.json").read_text())["viewport"]==[390,844]
    out.mkdir()
    images=[]
    for name in spec["images"]:
        raw=(root/name).read_bytes()
        assert raw[:8]==b"\x89PNG\r\n\x1a\n" and struct.unpack(">II",raw[16:24])==(390,844)
        (out/name).write_bytes(raw)
        images.append({"path":name,"sha256":sha(raw)})
    parts=[]
    for path,marker in spec["files"]:
        raw=subprocess.check_output(["gh","api",f"repos/{REPO}/contents/{path}?ref={SHA}","-H","Accept: application/vnd.github.raw+json"],timeout=60).decode()
        start=raw.index(marker)
        parts.append(path+"\n"+raw[max(0,start-600):start+9000//len(spec["files"])])
    source="\n\n".join(parts).encode()[:11800]
    source=source.decode("utf-8",errors="ignore").encode()
    (out/"source.txt").write_bytes(source)
    packet={"schema":1,"target_id":("CORONA_"+ident.upper().replace("-","_")),"source_sha":SHA,"source_run":RUN,"mode":"review","routing_profile":"standard",
      "objective":"Español. "+spec["objective"]+" Propón una intervención de alto impacto verificable por pantalla, con implementación precisa y medidas del resultado.",
      "scope":"Capturas WebGL reales 390x844 históricas y código de esa misma revisión, no de la última revisión. El candidato actual ya incorpora tipografía, respuesta de botones e iconografía compartidas y algunos ajustes: no repetirlos. No modificar sistemas, botones, navegación, arte aprobado ni Main. WOS y Call of Dragons son referentes, no capturas adjuntas. Nunca inferir funcionamiento o calidad de una animación a partir de una imagen.",
      "acceptance":"Director: evidencia y prioridad concreta. Programador: cambio reversible, símbolos existentes, riesgo y prueba de no regresión. Auditor: cuestiona cambios, distingue observación de hipótesis y rechaza puntuación sin ver el candidato. Coordinador: secuencia y criterio de aceptación. Proponer solamente, no afirmar integración ni ejecución.",
      "source":{"path":"source.txt","sha256":sha(source)},"images":images}
    (out/"packet.json").write_text(json.dumps(packet,ensure_ascii=False,indent=2),encoding="utf-8")
if __name__=="__main__":main(*sys.argv[1:])
