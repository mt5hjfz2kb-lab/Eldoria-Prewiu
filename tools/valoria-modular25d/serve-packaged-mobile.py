import os,sys
from http.server import SimpleHTTPRequestHandler,ThreadingHTTPServer
os.chdir(sys.argv[1])
class Serve(SimpleHTTPRequestHandler):
    def end_headers(self):
        if self.path.endswith(('.unityweb','.gz')): self.send_header('Content-Encoding','gzip')
        super().end_headers()
    def guess_type(self,p):
        if '.wasm.' in p:return 'application/wasm'
        if '.js.' in p:return 'application/javascript'
        return super().guess_type(p)
ThreadingHTTPServer(('127.0.0.1',int(sys.argv[2])),Serve).serve_forever()
