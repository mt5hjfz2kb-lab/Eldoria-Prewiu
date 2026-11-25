"""One deliberately authorized image draft. Never writes to Unity or publishes."""
import argparse
import base64
import hashlib
import json
import os
from pathlib import Path
import sys
import unittest
import urllib.error
import urllib.request
from studio import NoRedirect, read_limited

MODEL = "gpt-image-2.6-flare"
URL = "https://api.openai.com/v1/images/generations"
PNG = b"\x89PNG\r\n\x1a\n"

def make_payload(brief):
    if not brief.strip() or len(brief.encode("utf-8")) > 8000:
        raise ValueError("Brief vacío o superior al límite")
    if any(marker in brief for marker in ("sk-proj-", "ghp_", "github_pat_", "-----BEGIN PRIVATE KEY-----")):
        raise ValueError("Posible secreto en el brief")
    return {"model":MODEL, "prompt":"Eldoria: candidato artístico original, medieval fantástico oscuro, adulto, coherente con un juego móvil de estrategia. No copiar IP ajena. Seguir el brief: "+brief,
            "n":1,"size":"1024x1024","quality":"low","complexity":"low","output_format":"png"}

def generate(payload, key):
    if not key or not key.strip():
        raise ValueError("Falta OPENAI_API_KEY en el almacén seguro")
    req = urllib.request.Request(URL, data=json.dumps(payload).encode("utf-8"),
        headers={"Authorization":"Bearer "+key,"Content-Type":"application/json"},method="POST")
    try:
        with urllib.request.build_opener(NoRedirect()).open(req,timeout=180) as r:
            raw=r.read(24*1024*1024+1)
            if len(raw)>24*1024*1024:
                raise RuntimeError("Respuesta superior al límite")
            return json.loads(raw)
    except urllib.error.HTTPError as e:
        raise RuntimeError("API HTTP "+str(e.code)+"; no reintentar automáticamente") from None
    except (urllib.error.URLError, TimeoutError):
        raise RuntimeError("Conexión fallida; verificar consumo antes de reintentar") from None

def unpack(result):
    data=result.get("data")
    if not isinstance(data,list) or len(data)!=1:
        raise ValueError("Se esperaba exactamente una imagen")
    raw=base64.b64decode(data[0]["b64_json"],validate=True)
    if not raw.startswith(PNG):
        raise ValueError("La respuesta no contiene PNG")
    return raw,{"model":MODEL,"image_sha256":hashlib.sha256(raw).hexdigest(),
                "usage":result.get("usage"),"billing_verified":False,
                "art_approved":False,"quality":"low","size":"1024x1024"}

def main():
    p=argparse.ArgumentParser()
    p.add_argument("--brief",required=True)
    p.add_argument("--out-dir",required=True)
    p.add_argument("--live",action="store_true",help="Autoriza una única generación facturable")
    args=p.parse_args()
    try:
        brief=read_limited(args.brief,8000).decode("utf-8")
        payload=make_payload(brief)
        if not args.live:
            print("DRY_RUN: ninguna solicitud de imagen")
            return 0
        key=os.environ.get("OPENAI_API_KEY")
        if not key:
            raise ValueError("Falta OPENAI_API_KEY")
        dest=Path(args.out_dir)
        # Unique output directory required; no overwrite or automatic promotion.
        dest.mkdir(parents=True,exist_ok=False)
        raw,meta=unpack(generate(payload,key))
        meta["brief_sha256"]=hashlib.sha256(brief.encode("utf-8")).hexdigest()
        (dest/"candidate.png").write_bytes(raw)
        (dest/"receipt.json").write_text(json.dumps(meta,indent=2),encoding="utf-8")
        print("CANDIDATE_ONLY: revisar visualmente; consultar facturación real")
        return 0
    except (OSError,ValueError,RuntimeError,KeyError,TypeError) as e:
        print("ERROR:",type(e).__name__,file=sys.stderr)
        return 1

class Tests(unittest.TestCase):
    def test_one_image(self):
        p=make_payload("Icono de una cantera")
        self.assertEqual(p["n"],1)
        self.assertEqual(p["quality"],"low")
        self.assertEqual(p["model"],MODEL)
    def test_secret_guard(self):
        with self.assertRaises(ValueError):
            make_payload("sk-proj-test")
    def test_response(self):
        raw,meta=unpack({"data":[{"b64_json":base64.b64encode(PNG+b"test").decode()}]})
        self.assertEqual(raw,PNG+b"test")
        self.assertFalse(meta["art_approved"])
    def test_multiple_rejected(self):
        with self.assertRaises(ValueError):
            unpack({"data":[{},{}]})
    def test_key_missing(self):
        with self.assertRaises(ValueError):
            generate({},None)

if __name__=="__main__":
    if sys.argv[1:]==["--self-test"]:
        unittest.main(argv=[sys.argv[0]],verbosity=2)
    else:
        sys.exit(main())
