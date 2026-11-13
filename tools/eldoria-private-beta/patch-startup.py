from pathlib import Path

def patch_startup(out):
    play=out/'play.js'
    text=play.read_text()
    replacements={
      '    enabled: true': '''    recordStartup: function(kind, value) {
      if (!/^(start|manifest|loader|part|progress|pre-run|post-run|ready|failure|heap-mib)$/.test(kind)) return;
      add("startup-" + kind, Number.isFinite(value) ? String(Math.round(value)) : "");
    },
    enabled: true''',
      'let starting=false;': '''let starting=false;
function startupCheckpoint(kind, value) {
 window.EldoriaLocalDiagnostic?.recordStartup?.(kind, value);
}
let startupProgressBucket=-1;''',
      'const part=await originalFetch(new URL(list[index].url,location.href).href,init);': 'window.EldoriaLocalDiagnostic?.recordStartup?.("part",index);\n        const part=await originalFetch(new URL(list[index].url,location.href).href,init);',
      ' console.error("Valoria playtest:",error);': ' startupCheckpoint("failure");\n console.error("Valoria playtest:",error);',
      ' if(starting)return;starting=true;play.hidden=true;loading.hidden=false;': ' if(starting)return;starting=true;play.hidden=true;loading.hidden=false;\n startupCheckpoint("start");',
      'const build=await response.json();installExactDecodedTransport': 'const build=await response.json();startupCheckpoint("manifest");installExactDecodedTransport',
      '  game.hidden=false;': '  startupCheckpoint("loader");\n  game.hidden=false;',
      '   dataUrl:build.data,frameworkUrl:build.framework,codeUrl:build.code,': '''   preRun:[function(module){startupCheckpoint("pre-run");if(module?.HEAPU8)startupCheckpoint("heap-mib",module.HEAPU8.byteLength/1048576);}],
   postRun:[function(module){startupCheckpoint("post-run");if(module?.HEAPU8)startupCheckpoint("heap-mib",module.HEAPU8.byteLength/1048576);}],
   dataUrl:build.data,frameworkUrl:build.framework,codeUrl:build.code,''',
      '   progress.value=value;': '   const bucket=Math.floor(value*10);if(bucket!==startupProgressBucket){startupProgressBucket=bucket;startupCheckpoint("progress",bucket*10);}\n   progress.value=value;',
      '  window.valoriaGame=instance;': '  startupCheckpoint("ready");\n  window.valoriaGame=instance;'
    }
    for before,after in replacements.items():
        assert text.count(before)==1, f'unexpected bootstrap: {before}'
        text=text.replace(before,after)
    text=text.replace('button.textContent = "Diagnóstico";','button.textContent = "Diagnóstico v3";')
    text=text.replace('add("boot", typeOfNavigation());','add("boot", typeOfNavigation()); add("diagnostic", "v3");')
    play.write_text(text)
    # Content-addressed script prevents reuse of a previous bootstrap in HTTP cache.
    import hashlib
    name='play-'+hashlib.sha256(text.encode()).hexdigest()[:16]+'.js'
    (out/name).write_text(text)
    index=out/'index.html'
    html=index.read_text()
    assert html.count('src="play.js"')==1
    html=html.replace('src="play.js"','src="'+name+'"')
    index.write_text(html)
    # Fresh document URL also avoids an old cached HTML entry. Same directory/save scope.
    (out/'diagnostico-v3.html').write_text(html)
    assert (out/name).read_text()==play.read_text()


if __name__=='__main__':
    import sys
    patch_startup(Path(sys.argv[1]))
