#!/usr/bin/env python3
"""Pre-Unity composition of FOUR independently illustrated ceremonial props.

Reuses EXACT original Tesoros nine-slice material as a transparent ORNAMENT,
never pastes a UI screenshot over official Lyra/city, and never grants loot.
This does not decide subjective art quality or import into Unity.
"""
import hashlib
import json
from pathlib import Path
from PIL import Image

ROOT=Path("tools/daily-treasures/art-candidates")
OWNER_FRAME=Path("Unity/Assets/Eldoria/DailyTreasures/Resources/DailyTreasureArt/valoria_frame_main_512.png")
EVENTS={
    "lyra":("HeroesApprovedDestination","canonical_hero"),
    "explorers":("EldoriaRetention/scouts-crest","canonical_discovery"),
    "vigilants":("EldoriaRetention/vigilants-compass","canonical_insignia"),
    "bastion":("Valoria_City_World_UI.unity","actual_city"),
}
SIZE=(1376,768)
CORNER=54

def sha(p):
    return hashlib.sha256(p.read_bytes()).hexdigest()

def assemble_original_nine_slice():
    if not OWNER_FRAME.is_file():
        raise ValueError("The original approved Eldoria chest-frame source is unavailable")
    with Image.open(OWNER_FRAME) as source:
        source=source.convert("RGBA")
    if source.size!=(512,512):
        raise ValueError("Original approved frame changed unexpectedly")
    out=Image.new("RGBA",SIZE,(0,0,0,0))
    side=CORNER
    W,H=SIZE
    def portion(box,dest):
        tile=source.crop(box).resize((dest[2],dest[3]),Image.Resampling.LANCZOS)
        out.alpha_composite(tile,(dest[0],dest[1]))
    # Preserve exact hand-illustrated CORNERS and rail thickness.
    # Only the *long rails* stretch; center scene stays truly transparent.
    portion((0,0,side,side),(0,0,side,side))
    portion((512-side,0,512,side),(W-side,0,side,side))
    portion((0,512-side,side,512),(0,H-side,side,side))
    portion((512-side,512-side,512,512),(W-side,H-side,side,side))
    portion((side,0,512-side,side),(side,0,W-2*side,side))
    portion((side,512-side,512-side,512),(side,H-side,W-2*side,side))
    portion((0,side,side,512-side),(0,side,side,H-2*side))
    portion((512-side,side,512,512-side),(W-side,side,side,H-2*side))
    # Original source is an opaque black framed square, NOT a full screenshot:
    # An actually transparent center is required so the true game world shows.
    if out.getpixel((W//2,H//2))[3]!=0:
        raise AssertionError("Nine-slice erroneously hid the real Eldoria scene")
    path=ROOT/"shared_ornament.png"
    path.parent.mkdir(parents=True,exist_ok=True)
    out.save(path,optimize=True)
    return path

def main():
    for scene in EVENTS:
        path=ROOT/f"{scene}_stage.png"
        receipt=ROOT/f"{scene}_stage.provenance.json"
        if not path.exists() or not receipt.exists():
            raise ValueError("No complete genuine image/provenance pair: "+scene)
        record=json.loads(receipt.read_text())
        if record.get("sha256")!=sha(path) or record.get("art_approved") is not False:
            raise ValueError("Corrupt or improperly pre-approved source art: "+scene)
        if record.get("scene")!=scene or record.get("generated_illustration_not_authentic_gameplay") is not True:
            raise ValueError("Art provenance absent for: "+scene)
    ornament=assemble_original_nine_slice()
    manifest={"schema":1,"art_approved":False,"unity_native_verified":False,"ceremonies":{}}
    for scene,(identity,slot) in EVENTS.items():
        stage=ROOT/f"{scene}_stage.png"
        manifest["ceremonies"][scene]={
            "identity_source":identity,"identity_slot":slot,
            "baked_text":False,"contains_fake_rewards":False,
            "renders_real_identity":False,
            "layers":[
                {"role":"stage","path":stage.name,"sha256":sha(stage)},
                {"role":"ornament","path":ornament.name,"sha256":sha(ornament)},
            ],
        }
    target=ROOT/"ceremony-art-production.json"
    target.write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+"\n")
    # Source integrity and transparent pixel coverage; does NOT judge art.
    import importlib.util
    checker=Path("tools/daily-treasures/preflight_ceremony_art.py")
    spec=importlib.util.spec_from_file_location("daily_source_preflight",checker)
    module=importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    print(json.dumps(module.audit(target),ensure_ascii=False,indent=2))
    print("Four candidate sources prepared; ART_APPROVED=false, NO Unity verification")

if __name__=="__main__":main()
