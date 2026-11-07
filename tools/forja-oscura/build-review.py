#!/usr/bin/env python3
"""Offline review of genuine Unity native A/B evidence. NEVER creates gameplay screenshots.
Generated HTML keeps provenance visible and does not award artistic approval.
"""
from pathlib import Path
import argparse, hashlib, html, json, struct, tempfile, zlib

RESOLUTIONS=((1600,900),(900,1600),(844,390),(390,844))
PREFIXES=("native-before","native-after")

def png_size(path):
    with open(path,"rb") as f:header=f.read(24)
    if len(header)<24 or header[:8]!=b"\x89PNG\r\n\x1a\n" or header[12:16]!=b"IHDR":
        raise ValueError(f"Not a PNG: {path}")
    return struct.unpack(">II",header[16:24])

def check_and_create(folder):
    folder=Path(folder)
    report_path=folder/"ForjaNativeEvidence.json"
    if not report_path.exists():raise FileNotFoundError("Missing native Unity report; no review permitted")
    report=json.loads(report_path.read_text(encoding="utf-8"))
    if report.get("status")!="UNITY_NATIVE_CAPTURED_VISUAL_REVIEW_PENDING":
        raise ValueError("Unity report is not in the required native-captured state")
    if report.get("artApproved") is True:
        raise ValueError("This tool must not turn technical evidence into artistic approval")
    if report.get("screenshotCount")!=8 or report.get("orientationsWithDifference")!=4:
        raise ValueError("Expected eight screenshots and four native before/after changes")
    if report.get("dataBefore") != report.get("dataAfter") or not report.get("dataBefore"):
        raise ValueError("Actual player HUD values changed or were never sampled")
    metrics={m.get("resolution"):m for m in report.get("nativeTouchTargets",[])}
    regions={m.get("resolution"):m for m in report.get("uiRegionDifferences",[])}
    if len(metrics)!=4 or len(regions)!=4:
        raise ValueError("Unity evidence lacks four actual click-target and UI region measurements")
    legibility={m.get("resolution"):m for m in report.get("nativeReadability",[])}
    if len(legibility)!=4:
        raise ValueError("Unity evidence lacks font-pixel and clipping checks in four orientations")
    for w,h in RESOLUTIONS:
        key=f"{w}x{h}"
        measure=metrics.get(key, {})
        diff=regions.get(key, {})
        if not measure.get("hasFiveVisibleActions"):
            raise ValueError(f"Unity failed to verify five working navigation hit targets at {key}")
        if measure.get("minWidthPx",0)<44 or measure.get("minHeightPx",0)<44:
            raise ValueError(f"Native touch area below 44px at {key}")
        if measure.get("minGraphicWidthPx",0)<44 or measure.get("minGraphicHeightPx",0)<44:
            raise ValueError(f"Actual raycast graphic below 44px at {key}")
        if measure.get("buildingCardWasVisible") and measure.get("buildingCardClearancePx",-1)<0.5:
            raise ValueError(f"Building cartouche overlaps the actual dock at {key}")
        if diff.get("navigationChanged",0)<40 or diff.get("headerChanged",0)<40:
            raise ValueError(f"Missing visible HUD change in a real native capture at {key}")
        text=legibility.get(key,{})
        icon_only = text.get("iconOnlyNavigation",False)
        if (not icon_only and not text.get("navigationLabelsFit")) or not text.get("resourceValuesFit"):
            raise ValueError(f"Mobile HUD has a clipped caption or numeric resource at {key}")
        if icon_only and measure.get("navigationRows")!=1:
            raise ValueError(f"Owner forbids stacked navigation at {key}")
        if icon_only and (text.get("visibleNavigationIcons",0)!=5 or text.get("minNavigationIconPx",0)<32):
            raise ValueError(f"Missing readable icon-only navigation at {key}")
        if (not icon_only and text.get("minNavigationGlyphPx",0)<12) or text.get("minResourceGlyphPx",0)<13:
            raise ValueError(f"Mobile HUD text below native 12px/13px minimum at {key}")
        if text.get("minResourceIconWidthPx",0)<24 or text.get("minResourceIconHeightPx",0)<24:
            raise ValueError(f"Painted resource icon below native 24px minimum at {key}")
    routes=report.get("navigationActions",[])
    if len(routes)!=5 or len({r.get("destination") for r in routes})!=5:
        raise ValueError("Actual pointer route evidence must cover five canonical destinations")
    tested=[r for r in routes if r.get("status")=="NATIVE_POINTER_ROUTE_PASS"]
    if len(tested)<3 or any(not r.get("originalEventDelivered") or not r.get("destinationVisible") for r in tested):
        raise ValueError("Authentic unlocked navigation pointer routes did not reach destinations")
    if any(r.get("status") not in ("NATIVE_POINTER_ROUTE_PASS","LOCKED_NOT_EXERCISED") for r in routes):
        raise ValueError("Unrecognized/failed navigation route status")
    close=report.get("contextClose") or {}
    if close.get("status")!="NATIVE_POINTER_CONTEXT_CLOSE_PASS" or not close.get("selectionCleared") or not close.get("originalDataRestored"):
        raise ValueError("Original visible contextual CLOSE has no native pointer + restoration evidence")
    offer=report.get("buildingOffer") or {}
    if offer.get("status")!="NATIVE_POINTER_BUILD_AND_PROBE_SAVE_PASS" or not offer.get("gatewayStateChanged") or not offer.get("isolatedSaveChanged"):
        raise ValueError("Original visible BUILD has no authentic disposable gateway+save evidence")
    artifacts={x["name"]:x.get("sha256") for x in report.get("artifacts",[])}
    if len(artifacts)!=8:
        raise ValueError("Missing native image hashes, cannot verify provenance")
    rows=[]
    for w,h in RESOLUTIONS:
        names=[f"{prefix}-{w}x{h}.png" for prefix in PREFIXES]
        for name in names:
            p=folder/name
            if not p.is_file():raise FileNotFoundError(f"Missing native screenshot {name}")
            if png_size(p)!=(w,h):
                raise ValueError(f"Native PNG resolution mismatch: {name}")
            digest=hashlib.sha256(p.read_bytes()).hexdigest()
            if artifacts.get(name)!=digest:
                raise ValueError(f"SHA256 mismatch or unreported image: {name}")
        rows.append({"size":f"{w} × {h}","before":names[0],"after":names[1],"width":w,"height":h})
    payload=json.dumps(rows,ensure_ascii=False).replace("<","\\u003c")
    html_page=r"""<!doctype html><html lang="es"><head><meta charset="utf-8">
<meta name="viewport" content="width=device-width,initial-scale=1">
<title>Eldoria · Forja Oscura · revisión nativa A/B</title>
<style>
:root{color-scheme:dark;--b:#090e14;--card:#121d27;--line:#344453;--gold:#ebbb77}
*{box-sizing:border-box}body{font:16px system-ui,Arial,sans-serif;background:var(--b);color:#edf1f1;margin:0}
main{max-width:1450px;margin:auto;padding:20px 16px 45px}
h1{font-size:24px;margin:0 0 7px}p{color:#a8bac4;line-height:1.5;max-width:85ch}
.info{border:1px solid #67553b;background:#171912;padding:12px 16px;border-radius:8px;color:#e9d0a5}
.controls{display:flex;flex-wrap:wrap;gap:16px;align-items:center;margin:20px 0}
select,input{accent-color:var(--gold)}select{background:#192530;border:1px solid var(--line);padding:9px;color:white;border-radius:7px}
.compare{position:relative;width:100%;border:1px solid var(--line);border-radius:10px;overflow:hidden;aspect-ratio:1600/900;background:#080c13}
.before,.after{position:absolute;inset:0;overflow:hidden}.before img,.after img{height:100%;width:100%;display:block;object-fit:fill}
.after{clip-path:inset(0 50% 0 0)}.line{position:absolute;top:0;bottom:0;left:50%;width:2px;background:var(--gold);pointer-events:none}
.label{position:absolute;top:10px;padding:5px 9px;background:#080f19de;border-radius:5px;font-weight:700;font-size:12px}
.label.left{left:10px}.label.right{right:10px}
aside{margin-top:15px;display:grid;grid-template-columns:repeat(auto-fit,minmax(220px,1fr));gap:12px}
article{background:var(--card);border:1px solid var(--line);border-radius:8px;padding:12px;font-size:14px}
article strong{color:var(--gold)}.afterlink{font-weight:500}
</style></head><body><main><h1>ELDORIA · Forja Oscura · comparación nativa</h1>
<div class="info"><strong>ESTADO: EVIDENCIA TÉCNICA, NO APROBACIÓN ARTÍSTICA.</strong>
 Las dos imágenes corresponden a Unity en la misma sesión de prueba, verificadas
 contra los SHA-256 incluidos en el informe. La fidelidad al concepto elegido aún requiere revisión visual.
</div><div class="controls"><label>Resolución <select id="which"></select></label>
<label>Mostrar diseño nuevo: <output id="pct">50%</output> <input id="split" type="range" min="0" max="100" value="50"></label></div>
<section id="viewport" class="compare">
<div class="before"><img id="before" alt="Captura original del HUD en Unity"></div>
<div class="after" id="afterClip"><img id="after" alt="Captura nativa del HUD Forja Oscura en Unity"></div>
<div class="line" id="line"></div><div class="label left">ANTES · original</div>
<div class="label right">DESPUÉS · Forja Oscura</div></section>
<aside><article><strong>01 · Arte</strong><p>Riqueza, jerarquía, iconos, acabados y semejanza con el concepto elegido.</p></article>
<article><strong>02 · Legibilidad</strong><p>Sin textos recortados, paneles que oculten el mundo ni acciones ambiguas.</p></article>
<article><strong>03 · Interacción</strong><p>Revisar informe JSON de clics, estados y recursos reales; la imagen sola no lo prueba.</p></article>
<article><strong>04 · Móvil</strong><p>Examinar las dos orientaciones y contrastar posteriormente con un dispositivo físico.</p></article></aside>
<p>Archivos de evidencia: <code>ForjaNativeEvidence.json</code>. Esta página se genera únicamente después de verificar los ocho PNG auténticos.</p>
</main><script>
const rows=__ROWS__;
const sel=document.getElementById("which"),slider=document.getElementById("split");
rows.forEach((r,i)=>{const o=document.createElement("option");o.value=i;o.textContent=r.size;sel.appendChild(o)});
function update(){const r=rows[Number(sel.value)||0];document.getElementById("before").src=r.before;
document.getElementById("after").src=r.after;document.getElementById("viewport").style.aspectRatio=r.width+"/"+r.height}
function split(){let v=Number(slider.value);document.getElementById("afterClip").style.clipPath="inset(0 "+(100-v)+"% 0 0)";
document.getElementById("line").style.left=v+"%";document.getElementById("pct").value=v+"%"}
sel.addEventListener("change",update);slider.addEventListener("input",split);update();split();
</script></body></html>""".replace("__ROWS__",payload)
    out=folder/"ForjaNativeReview.html"
    out.write_text(html_page,encoding="utf-8")
    return out

def tiny_png(w,h,color=(40,40,40)):
    # Used ONLY inside temporary test data, never presented as Unity evidence.
    def chunk(kind,data):
        return struct.pack(">I",len(data))+kind+data+struct.pack(">I",zlib.crc32(kind+data)&0xffffffff)
    scan=b"\x00"+bytes(color)*w
    compressed=zlib.compress(scan*h,9)
    return b"\x89PNG\r\n\x1a\n"+chunk(b"IHDR",struct.pack(">IIBBBBB",w,h,8,2,0,0,0))+chunk(b"IDAT",compressed)+chunk(b"IEND",b"")

def selftest():
    with tempfile.TemporaryDirectory() as t:
        path=Path(t)
        artifacts=[]
        for w,h in RESOLUTIONS:
            for prefix in PREFIXES:
                name=f"{prefix}-{w}x{h}.png"
                data=tiny_png(w,h,(30,50,70) if prefix=="native-before" else (40,60,80))
                (path/name).write_bytes(data)
                artifacts.append({"name":name,"sha256":hashlib.sha256(data).hexdigest()})
        report={"status":"UNITY_NATIVE_CAPTURED_VISUAL_REVIEW_PENDING","screenshotCount":8,
                "orientationsWithDifference":4,"dataBefore":"wood=500|stone=300",
                "dataAfter":"wood=500|stone=300","artApproved":False,"artifacts":artifacts,
                "nativeTouchTargets":[{"resolution":f"{w}x{h}","minWidthPx":50,
                    "minHeightPx":48,"minGraphicWidthPx":50,"minGraphicHeightPx":48,
                    "buildingCardWasVisible":True,"buildingCardClearancePx":12,
                    "hasFiveVisibleActions":True} for w,h in RESOLUTIONS],
                "nativeReadability":[{"resolution":f"{w}x{h}",
                    "minNavigationGlyphPx":13.2,"minResourceGlyphPx":14.1,
                    "minResourceIconWidthPx":26,"minResourceIconHeightPx":26,
                    "navigationLabelsFit":True,"resourceValuesFit":True} for w,h in RESOLUTIONS],
                "navigationActions":[{"destination":n,"status":"NATIVE_POINTER_ROUTE_PASS",
                    "originalEventDelivered":True,"destinationVisible":True}
                    for n in ["Bastion","Inventory","Missions","Army","World"]],
                "contextClose":{"status":"NATIVE_POINTER_CONTEXT_CLOSE_PASS",
                    "selectionCleared":True,"originalDataRestored":True},
                "buildingOffer":{"status":"NATIVE_POINTER_BUILD_AND_PROBE_SAVE_PASS",
                    "gatewayStateChanged":True,"isolatedSaveChanged":True},
                "uiRegionDifferences":[{"resolution":f"{w}x{h}",
                    "navigationChanged":500,"headerChanged":450} for w,h in RESOLUTIONS]}
        (path/"ForjaNativeEvidence.json").write_text(json.dumps(report))
        page=check_and_create(path)
        assert page.is_file() and "__ROWS__" not in page.read_text()
        (path/artifacts[0]["name"]).write_bytes(b"corrupted")
        try:check_and_create(path)
        except ValueError:pass
        else:raise RuntimeError("Validator failed to reject corrupt source image")
        (path/artifacts[0]["name"]).write_bytes(tiny_png(RESOLUTIONS[0][0],RESOLUTIONS[0][1],(30,50,70)))
        report["dataAfter"]="wood=999|stone=300"
        (path/"ForjaNativeEvidence.json").write_text(json.dumps(report))
        try:check_and_create(path)
        except ValueError:pass
        else:raise RuntimeError("Validator accepted gameplay data corruption")
        report["dataAfter"]=report["dataBefore"]
        report["nativeTouchTargets"][0]["minWidthPx"]=20
        (path/"ForjaNativeEvidence.json").write_text(json.dumps(report))
        try:check_and_create(path)
        except ValueError:pass
        else:raise RuntimeError("Validator accepted inaccessible tap target")
        report["nativeTouchTargets"][0]["minWidthPx"]=52
        report["nativeReadability"][0]["minNavigationGlyphPx"]=7.5
        (path/"ForjaNativeEvidence.json").write_text(json.dumps(report))
        try:check_and_create(path)
        except ValueError:pass
        else:raise RuntimeError("Validator accepted unreadable navigation text")
        for reading in report["nativeReadability"]:
            reading.update(iconOnlyNavigation=True,visibleNavigationIcons=5,
                minNavigationIconPx=44,minNavigationGlyphPx=0,navigationLabelsFit=False)
        for metric in report["nativeTouchTargets"]:metric["navigationRows"]=1
        (path/"ForjaNativeEvidence.json").write_text(json.dumps(report))
        check_and_create(path)
        report["nativeTouchTargets"][0]["navigationRows"]=2
        (path/"ForjaNativeEvidence.json").write_text(json.dumps(report))
        try:check_and_create(path)
        except ValueError:pass
        else:raise RuntimeError("Validator accepted stacked icon navigation")
        report["nativeTouchTargets"][0]["navigationRows"]=1
        report["nativeReadability"][0]["minNavigationIconPx"]=20
        (path/"ForjaNativeEvidence.json").write_text(json.dumps(report))
        try:check_and_create(path)
        except ValueError:pass
        else:raise RuntimeError("Validator accepted unreadable navigation icons")
        print("FORJA_OFFLINE_VIEWER_SELFTEST_PASS; rejects corrupt PNG, changed data, small buttons and illegible native text; synthetic fixtures discarded")

if __name__=="__main__":
    p=argparse.ArgumentParser()
    p.add_argument("folder",nargs="?")
    p.add_argument("--selftest",action="store_true")
    args=p.parse_args()
    if args.selftest:selftest()
    else:
        if not args.folder:p.error("Provide actual Unity evidence folder or --selftest")
        print(check_and_create(args.folder))
