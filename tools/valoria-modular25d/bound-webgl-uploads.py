"""Restrict WebGL transfer views to the bytes the engine actually requested."""
import gzip
import re
import sys
from pathlib import Path


def bounded_uploads(js):
    count=0
    def compressed(m):
        nonlocal count
        count+=1
        return m[1]+',HEAPU8.slice(data,data+imageSize))'
    js = re.sub(r'(GLctx\.compressedTex(?:Sub)?Image[23]D\([^)]*?),HEAPU8,data,imageSize\)',compressed,js)
    assert count==4,count
    for old,new in [
        ('GLctx.bufferData(target,HEAPU8,usage,data,size)','GLctx.bufferData(target,HEAPU8.slice(data,data+size),usage)'),
        ('GLctx.bufferSubData(target,offset,HEAPU8,data,size)','GLctx.bufferSubData(target,offset,HEAPU8.slice(data,data+size))'),
    ]:
        assert js.count(old)==1,old
        js=js.replace(old,new)
    return js


if __name__=='__main__':
    path=Path(sys.argv[1])
    path.write_bytes(gzip.compress(bounded_uploads(gzip.decompress(path.read_bytes()).decode()).encode(),compresslevel=5,mtime=0))
