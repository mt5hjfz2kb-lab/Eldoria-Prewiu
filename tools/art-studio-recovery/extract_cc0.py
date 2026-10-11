"""Extract the historically validated CC0 donor using system libarchive."""
import ctypes,ctypes.util,pathlib,sys
lib=ctypes.CDLL(ctypes.util.find_library('archive'))
lib.archive_read_new.restype=ctypes.c_void_p
for name in ['archive_read_support_filter_all','archive_read_support_format_all','archive_read_free']:
 getattr(lib,name).argtypes=[ctypes.c_void_p]
lib.archive_read_open_filename.argtypes=[ctypes.c_void_p,ctypes.c_char_p,ctypes.c_size_t]
lib.archive_read_next_header.argtypes=[ctypes.c_void_p,ctypes.POINTER(ctypes.c_void_p)]
lib.archive_entry_pathname.argtypes=[ctypes.c_void_p];lib.archive_entry_pathname.restype=ctypes.c_char_p
lib.archive_read_data.argtypes=[ctypes.c_void_p,ctypes.c_void_p,ctypes.c_size_t];lib.archive_read_data.restype=ctypes.c_ssize_t
src=sys.argv[1];out=pathlib.Path(sys.argv[2]).resolve();out.mkdir(parents=True,exist_ok=True)
a=lib.archive_read_new();lib.archive_read_support_filter_all(a);lib.archive_read_support_format_all(a)
assert lib.archive_read_open_filename(a,src.encode(),65536)==0
entry=ctypes.c_void_p();buf=ctypes.create_string_buffer(65536)
while lib.archive_read_next_header(a,ctypes.byref(entry))==0:
 name=lib.archive_entry_pathname(entry).decode();dest=(out/name).resolve()
 if out not in dest.parents:raise ValueError('archive traversal')
 if name.endswith('/'):dest.mkdir(parents=True,exist_ok=True);continue
 # Only the useful mobile donor and its existing PBR maps; no giant archive persistence.
 selected='/LOD4/' in name.replace('\\','/') or '/LOD3/' in name.replace('\\','/')
 if selected:
  dest.parent.mkdir(parents=True,exist_ok=True)
  with dest.open('wb') as f:
   while True:
    n=lib.archive_read_data(a,buf,65536)
    if n<0:raise RuntimeError('archive decode failed')
    if n==0:break
    f.write(buf.raw[:n])
  print(dest)
lib.archive_read_free(a)
