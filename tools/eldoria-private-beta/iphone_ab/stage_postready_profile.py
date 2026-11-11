"""Stage a diagnostic-only beta13 wrapper over an exact, already certified engine.

No new Unity compilation, code/texture/resource transport modifications or publication.
Only opt-in breadcrumbs are added; an archived beta13 payload is required.
"""
import argparse
import hashlib
import json
import re
from pathlib import Path


def sha(data):
    return hashlib.sha256(data).hexdigest()


def stage(site, source):
    site = Path(site)
    source = Path(source)
    identity_file = site / "release-identity.json"
    identity = json.loads(identity_file.read_text())
    assert identity["source_sha"] == "7ae5416613480ba526a839b429459d53629041e5"
    assert identity["revision"] == "native325-mobile13-prebaked-collision-qa"
    assert identity["release_for_external_testers"] is False
    assert identity.get("physical_mobile_tested") is False
    original = {p.relative_to(site).as_posix(): sha(p.read_bytes())
                for p in site.rglob("*") if p.is_file()}
    assert sum(name.startswith("Build/") for name in original) >= 3

    module = source.read_text()
    assert "installEldoriaPostreadyProfile" in module
    assert "eldoria_p0" in module
    raw = (site / "play.js").read_text()
    token = "const instance=await createUnityInstance(canvas,{"
    assert raw.count(token) == 1
    assert "installEldoriaPostreadyProfile(" not in raw
    revised = module + "\n" + raw.replace(
        token, "window.__eldoriaP0Postready=installEldoriaPostreadyProfile(window,document);\n  " + token)
    name = "play-" + sha(revised.encode())[:16] + ".js"
    html = (site / "index.html").read_text()
    html, n = re.subn(r'src="play(?:-[a-f0-9]+)?\.js(?:\?[^"]*)?"',
                      'src="' + name + '"', html)
    assert n == 1, "exactly one versioned script reference required"
    (site / "play.js").write_text(revised)
    (site / name).write_text(revised)
    (site / "index.html").write_text(html)
    identity["p0_local_profile"] = {
        "status": "STAGED_OWNER_ONLY_NOT_PHYSICAL_TESTED",
        "opt_in_params": ["eldoria_p0=1", "eldoria_profile=1"],
        "script": name,
        "sha256": sha(revised.encode()),
        "preserves_engine_and_game_assets": True,
        "physical_iphone_verified": False,
        "external_testers_enabled": False
    }
    identity_file.write_text(json.dumps(identity, indent=2) + "\n")

    after = {p.relative_to(site).as_posix(): sha(p.read_bytes())
             for p in site.rglob("*") if p.is_file()}
    allowed = {"play.js", "index.html", "release-identity.json", name}
    changed = {p for p in set(original) | set(after)
               if original.get(p) != after.get(p)}
    assert changed.issubset(allowed) and {"play.js", "index.html", name} <= changed
    assert all(after.get(path) == digest for path, digest in original.items()
               if path.startswith(("Build/", "payload-decoded/")))
    print(json.dumps({"result": "PROFILE_STAGE_PASS_NOT_IPHONE_CERTIFICATION",
                      "script": name, "changed": sorted(changed),
                      "engine_unchanged": True, "published": False}))
    return name


if __name__ == "__main__":
    p = argparse.ArgumentParser()
    p.add_argument("site")
    p.add_argument("module")
    args = p.parse_args()
    stage(args.site, args.module)
