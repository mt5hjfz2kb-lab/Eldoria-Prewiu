"""Repackage the Unity player transport only, preserving decoded asset hashes."""
import gzip, hashlib, json, shutil, struct, sys, zipfile
from pathlib import Path
src,dst=Path(sys.argv[1]),Path(sys.argv[2])
dst.mkdir(parents=True,exist_ok=True)
assets=dst/'Build'
assets.mkdir(exist_ok=True)
manifest={}
with zipfile.ZipFile(src) as z:
    for name in z.namelist():
        if name.endswith('/') or name=='index.html':continue
        if not (name.startswith('Build/') or name.startswith('TemplateData/')):continue
        p=dst/name;p.parent.mkdir(parents=True,exist_ok=True)
        data=z.read(name)
        if name.endswith(('.data.gz','.wasm.gz')):
            logical=name.removesuffix('.gz')
            record={'parts':[],'size':struct.unpack('<I',data[-4:])[0],'mime':'application/wasm' if '.wasm' in logical else 'application/octet-stream'}
            assert len(gzip.decompress(data))==record['size']
            manifest[logical]=record
            for index in range(0,len(data),4*1024*1024):
                part=data[index:index+4*1024*1024]
                dest=assets/(hashlib.sha256(part).hexdigest()+'.bin')
                dest.write_bytes(part)
                record['parts'].append('Build/'+dest.name)
        elif name.endswith('.js.gz'):
            p.with_suffix('').write_bytes(gzip.decompress(data))
        else:p.write_bytes(data)
    html=z.read('index.html').decode()
    for ext in ['.js','.data','.wasm']:html=html.replace(ext+'.gz',ext)
    html=html.replace('<head>', '<head>\n<meta name="viewport" content="width=device-width, initial-scale=1, viewport-fit=cover">\n<script src="player-transport.js"></script>')
    html=html.replace('<title>Unity Web Player | Unity</title>','<title>Valoria · Corona de la Brecha</title>')
    html=html.replace("canvas.style.width = \"960px\";",'canvas.style.width = "100vw";').replace("canvas.style.height = \"600px\";",'canvas.style.height = "100dvh";')
    html=html.replace('</head>','<style>html,body{margin:0;background:#201b26;overflow:hidden}#unity-footer{display:none}#unity-container{width:100%;height:100%}#unity-canvas{display:block;touch-action:none}</style>\n</head>')
    (dst/'index.html').write_text(html)
transport="""
// Static hosting uses small, content-addressed parts. Stream the original
// compressed Unity bytes in order, decompress once and retain native WASM MIME.
(() => {
  const manifests = MANIFEST;
  const originalFetch = window.fetch.bind(window);
  const files = new Map(Object.entries(manifests).map(([name,value]) =>
    [new URL(name,document.baseURI).href,value]));
  window.fetch = async (input, init) => {
    const url = new URL(input instanceof Request ? input.url : input,document.baseURI).href;
    const asset = files.get(url);
    if (!asset) return originalFetch(input,init);
    const headers = {'Content-Type':asset.mime,'Content-Length':String(asset.size),'Cache-Control':'no-store'};
    if ((init?.method || input.method) === 'HEAD') return new Response(null,{headers});
    let part=0, reader;
    const stream = new ReadableStream({
      async pull(controller) {
        while (part < asset.parts.length || reader) {
          if (!reader) {
            const response = await originalFetch(asset.parts[part++],{signal:init?.signal});
            if (!response.ok) throw new Error('No se pudo cargar un archivo de Valoria ('+response.status+')');
            reader=response.body.getReader();
          }
          const chunk=await reader.read();
          if (!chunk.done) {controller.enqueue(chunk.value);return;}
          reader.releaseLock();reader=null;
        }
        controller.close();
      },
      cancel(reason) {return reader?.cancel(reason);}
    });
    return new Response(stream.pipeThrough(new DecompressionStream('gzip')),{headers});
  };
})();
"""
(dst/'player-transport.js').write_text(transport.replace('MANIFEST',json.dumps(manifest)))
print([(k,v['size'],len(v['parts'])) for k,v in manifest.items()])
