import os, sys
from http.server import ThreadingHTTPServer, SimpleHTTPRequestHandler

class Handler(SimpleHTTPRequestHandler):
    def guess_type(self,path):
        return super().guess_type(path.removesuffix('.gz').removesuffix('.br'))
    def end_headers(self):
        if self.path.endswith('.gz'): self.send_header('Content-Encoding','gzip')
        if self.path.endswith('.br'): self.send_header('Content-Encoding','br')
        super().end_headers()
    def log_message(self,*args): pass
os.chdir(sys.argv[1])
ThreadingHTTPServer(('127.0.0.1',int(sys.argv[2])),Handler).serve_forever()
