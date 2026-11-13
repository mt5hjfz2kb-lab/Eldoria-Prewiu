"""Safari/iOS Simulator diagnostic. Never certifies a physical iPhone.

Uses Apple's native SafariDriver, with isolated automation sessions. Records
actual capabilities, screenshots, process samples and document identity.
No fabricated nickname/gameplay success: startup and input are separate stages.
"""
import base64, json, os, pathlib, subprocess, time, urllib.request

OUT = pathlib.Path(os.environ.get('QA_OUT', 'ios-evidence'))
OUT.mkdir(parents=True, exist_ok=True)
report = {'scope': 'IOS_SIMULATOR_NOT_PHYSICAL_IPHONE', 'passed': False,
          'gameplay_verified': False, 'samples': [], 'errors': []}
session = None

def save():
    (OUT/'report.json').write_text(json.dumps(report, indent=2))

def request(path, data=None, method=None):
    body = None if data is None else json.dumps(data).encode()
    req = urllib.request.Request('http://127.0.0.1:4723'+path, data=body,
        method=method or ('POST' if data is not None else 'GET'),
        headers={'Content-Type':'application/json'})
    try:
        with urllib.request.urlopen(req, timeout=180) as response:
            value=json.load(response)['value']
    except urllib.error.HTTPError as e:
        raise RuntimeError(e.read().decode()[:4000]) from e
    if isinstance(value,dict) and value.get('error'):
        raise RuntimeError(str(value))
    return value

def command(path, data=None, method=None):
    return request('/session/'+session+path, data, method)

def js(script):
    return command('/execute/sync', {'script':script,'args':[]})

def snapshot(name):
    try:
        payload=command('/screenshot')
        (OUT/(name+'.png')).write_bytes(base64.b64decode(payload))
    except Exception as error:
        report.setdefault('screenshot_warnings',[]).append(str(error));save()

try:
    report['phase']='create_session';save()
    caps={'platformName':'iOS','browserName':'Safari',
          'safari:useSimulator':True,
          'safari:deviceUDID':os.environ['QA_SIM_UDID']}
    for attempt in range(2):
        try:
            result=request('/session',{'capabilities':{'alwaysMatch':caps,'firstMatch':[{}]}})
            break
        except RuntimeError as error:
            if attempt or 'session not created' not in str(error): raise
            report['cold_session_error']=str(error);save()
            time.sleep(5)  # Retry the now-initialized Safari, not another cold simulator.
    session=result['sessionId']; report['capabilities']=result.get('capabilities')
    save()
    report['phase']='navigate';save()
    command('/url',{'url':os.environ['QA_URL']})
    report['page']=js('return {url:location.href,ua:navigator.userAgent,width:innerWidth,height:innerHeight};')
    assert report['page']['url'].startswith('http://127.0.0.1:18975/'), 'Unexpected origin'
    assert 'iPhone' in report['page']['ua'], 'Not iPhone Safari'
    js("window.__qaDocument=Date.now()+'-'+Math.random(); window.__qaErrors=[]; window.addEventListener('error',e=>window.__qaErrors.push(String(e.message))); window.addEventListener('unhandledrejection',e=>window.__qaErrors.push(String(e.reason)));")
    js("""window.__qaEngine=[];window.__qaGL=[];
      const oldLog=console.log;
      console.log=function(...args){const text=args.map(String).join(' ');
        if(/ELDORIA_MEMORY_PHASE|REGION1_|WebGL|GL_INVALID|out of memory/i.test(text)){
          window.__qaEngine.push({at:performance.now(),text:text.slice(0,1500)});
          if(window.__qaEngine.length>100)window.__qaEngine.shift();
        }return oldLog.apply(this,args);};
      for(const C of [window.WebGLRenderingContext,window.WebGL2RenderingContext]){
        if(!C)continue;const original=C.prototype.createShader;
        C.prototype.createShader=function(type){const shader=original.call(this,type);
          if(!shader)window.__qaGL.push({at:performance.now(),type,lost:this.isContextLost(),error:this.getError()});
          return shader;};
      }""")
    report['document']=js('return window.__qaDocument;')
    element=command('/element',{'using':'css selector','value':'#play'})
    eid=element['element-6066-11e4-a52e-4f735466cecf']
    command('/element/'+eid+'/click',{})
    time.sleep(2)
    started=js("return !!window.EldoriaLocalDiagnostic?.read?.().some(e=>e.kind==='startup-start');")
    report['trusted_click_started_loading']=bool(started)
    if not started:
        # Explicit engine-startup diagnostic, NOT a physical tap test.
        report['startup_trigger']='PROGRAMMATIC_CLICK_NOT_TOUCH_CERTIFICATION'
        js("document.getElementById('play').click();return true;")
    else:
        report['startup_trigger']='WEBDRIVER_ELEMENT_CLICK'
    report['phase']='observe_startup';save()
    start=time.monotonic(); ready_at=None
    while time.monotonic()-start<300:
        sample=js("return {document:window.__qaDocument,url:location.href,ready:!!window.valoriaGame,visible:!document.getElementById('game')?.hidden,error:document.getElementById('error')?.textContent,errors:window.__qaErrors,metrics:window.valoriaGame?.GetMetricsInfo?.(),engine:window.__qaEngine,gl:window.__qaGL,diagnostic:window.EldoriaLocalDiagnostic?.read?.(),width:innerWidth,height:innerHeight};")
        sample['elapsed']=time.monotonic()-start
        sample['processes']=subprocess.check_output(['ps','-axo','pid=,rss=,comm='],text=True)
        sample['processes']='\n'.join(x for x in sample['processes'].splitlines() if any(s in x for s in ['WebContent','MobileSafari','WebKit.GPU']))
        report['samples'].append(sample);save()
        assert sample['document']==report['document'], 'Unexpected document replacement'
        if sample['ready'] and ready_at is None:
            ready_at=time.monotonic()
        if ready_at is not None:
            assert sample['ready'] and sample['visible'], 'Unity disappeared after ready'
            if time.monotonic()-ready_at>=120:
                report['startup_survived_seconds']=time.monotonic()-ready_at
                report['passed']=True;break
        time.sleep(3)
    assert report['passed'], 'Unity did not become ready and survive observation'
except Exception as e:
    report['errors'].append(str(e))
    # Do not block observation or miscount screenshot time as healthy samples.
    # A report without successful post-ready samples cannot pass.
finally:
    save()
    if session:
        try: command('',method='DELETE')
        except Exception as e: report['cleanup_error']=str(e);save()
    print(json.dumps({k:v for k,v in report.items() if k not in ['samples','capabilities']},indent=2))
    if not report['passed']: raise SystemExit(1)
