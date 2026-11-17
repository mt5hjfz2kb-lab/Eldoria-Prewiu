#!/usr/bin/env python3
"""Static HTTP server for the isolated *downloaded* Unity WebGL trial.
Use only on your own trusted Wi-Fi. Never exposes your main game or saves.
"""
import argparse
import os
from pathlib import Path
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer

class UnityHandler(SimpleHTTPRequestHandler):
    extensions_map = {
        **SimpleHTTPRequestHandler.extensions_map,
        ".html": "text/html; charset=utf-8",
        ".js": "text/javascript; charset=utf-8",
        ".wasm": "application/wasm",
        ".data": "application/octet-stream",
        ".json": "application/json",
    }
    def end_headers(self):
        self.send_header("Cache-Control", "no-cache, no-store, must-revalidate")
        self.send_header("X-Content-Type-Options", "nosniff")
        super().end_headers()

def main():
    parser=argparse.ArgumentParser(description="Valoria PremiumV6 standalone WebGL test")
    parser.add_argument("--host",default="0.0.0.0")
    parser.add_argument("--port",type=int,default=8765)
    args=parser.parse_args()
    folder=Path(__file__).resolve().parent
    if not (folder/"index.html").is_file():
        raise SystemExit("Extract the entire GitHub Actions WebGL build first.")
    os.chdir(folder)
    print("Valoria 2.5D isolated preview on http://localhost:%d/"%args.port,flush=True)
    print("From your iPhone on the SAME trusted Wi-Fi: http://<IP_DEL_PC>:%d/"%args.port,
          flush=True)
    print("Press Ctrl+C when finished. This serves only the extracted trial folder.",flush=True)
    ThreadingHTTPServer((args.host,args.port),UnityHandler).serve_forever()
if __name__=="__main__": main()
