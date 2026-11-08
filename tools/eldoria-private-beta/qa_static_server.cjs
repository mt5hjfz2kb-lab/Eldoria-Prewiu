const http=require('http'),fs=require('fs'),path=require('path');
const root=path.resolve(process.argv[2]||'.'),port=Number(process.argv[3]||18765);
const types={'.html':'text/html; charset=utf-8','.js':'application/javascript','.css':'text/css','.png':'image/png','.json':'application/json','.wasm':'application/wasm'};
http.createServer((req,res)=>{
 let file;
 try {
  const pathname=decodeURIComponent(new URL(req.url,'http://localhost').pathname);
  file=path.resolve(root,'.'+pathname);
  if(file!==root&&!file.startsWith(root+path.sep)){res.writeHead(403);res.end();return;}
  if(fs.statSync(file).isDirectory())file=path.join(file,'index.html');
  const stat=fs.statSync(file);
  if(!stat.isFile())throw Error('not a file');
  res.writeHead(200,{'Content-Type':types[path.extname(file)]||'application/octet-stream','Content-Length':stat.size,'Cache-Control':'no-store'});
  if(req.method==='HEAD'){res.end();return;}
  fs.createReadStream(file).on('error',()=>res.destroy()).pipe(res);
 } catch(e){res.writeHead(404);res.end('Not found');}
}).listen(port,'127.0.0.1',()=>console.log('BETA_LOCAL_SERVER_READY',port));
