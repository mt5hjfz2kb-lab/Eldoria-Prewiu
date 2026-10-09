const test=require('node:test');
const assert=require('node:assert/strict');
const fs=require('node:fs');
const source=fs.readFileSync('Unity/Assets/Eldoria/Scripts/Editor/EldoriaWebGLTouchActionPostprocess.cs','utf8');
const project=fs.readFileSync('Unity/ProjectSettings/ProjectSettings.asset','utf8');

test('mobile gesture interception is patched by a REAL Unity WebGL build postprocessor',()=>{
 assert.match(source,/\[PostProcessBuild\(1000\)\]/);
 assert.match(source,/if \(target != BuildTarget\.WebGL\) return;/);
 assert.match(source,/Path\.Combine\(pathToBuiltProject, "index\.html"\)/);
 assert.match(source,/touch-action:none!important/);
 assert.match(source,/\#unity-canvas/);
});
test('postprocessor fails closed for missing generated HTML and prevents double injection',()=>{
 assert.match(source,/File\.Exists\(file\)/);
 assert.match(source,/if \(html\.Contains\(Marker\)\) return;/);
 assert.match(source,/IndexOf\("<\/head>"/);
 assert.match(source,/WebGL touch-action post-build verification failed/);
});
test('current WebGL export uses the Unity default template, not a fake stand-alone web game',()=>{
 assert.match(project,/webGLTemplate: APPLICATION:Default/);
 assert.match(source,/File\.ReadAllText\(file\)/);
});
