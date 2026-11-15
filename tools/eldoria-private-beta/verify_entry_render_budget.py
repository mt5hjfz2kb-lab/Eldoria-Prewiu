"""Verify a measured resource change, never infer physical-device acceptance."""
import json,pathlib,re,sys
root=pathlib.Path(sys.argv[1])
rows=[]
for name in ('control','candidate'):
    report=json.loads((root/name/'report.json').read_text())
    assert report.get('passed') and report.get('worldEntryAndReentryVerified'),name
    assert report.get('source')=='ca5e1c3d52a58d8c6d9c9a9114d2d35445a18029',name
    log=(root/name/'console.log').read_text()
    targets={phase:dict(created=int(count),multisampled=int(msaa),unity_bytes=int(size))
             for phase,count,msaa,size in re.findall(
        r'ELDORIA_P0_ENTRY_RENDER_TARGETS phase=(\S+) created=(\d+) multisampled=(\d+) unity_bytes=(\d+)',log)}
    if name=='candidate':
        assert set(targets)=={'before','before-terrain'},targets
        before,after=targets['before'],targets['before-terrain']
        assert after['unity_bytes']<before['unity_bytes'],targets
        assert after['multisampled']<before['multisampled'],targets
        prepared=re.search(r'ELDORIA_P0_ENTRY_RENDER_BUDGET_PREPARED retired=(\d+) frames=(\d+)',log)
        assert prepared and int(prepared[1])>0,'No actual old camera MSAA targets retired'
        assert log.index(prepared[0])<log.index('ELDORIA_P0_LAZY_WORLD_READY_FOR_ENTRY')
        assert 'ELDORIA_P0_ENTRY_RENDER_BUDGET_RESTORED' in log
    else:
        assert not targets,'Control accidentally enabled candidate'
    rss=[];gpu=[]
    for entry in report.get('worldEntries',[]):
        for sample in entry.get('samples',[]):
            web_total=gpu_total=0
            for line in sample.get('processes',[]):
                m=re.match(r'\s*\d+\s+\d+\s+(\d+)\s+(.*)',line)
                if not m:continue
                if 'WebContent' in m[2]:web_total+=int(m[1])
                elif 'GPU' in m[2]:gpu_total+=int(m[1])
            if web_total:rss.append(web_total)
            if gpu_total:gpu.append(gpu_total)
    rows.append(dict(name=name,targets=targets,peakSampledWebContentTotalKiB=max(rss,default=None),
                     peakSampledGpuTotalKiB=max(gpu,default=None)))
result=dict(scope='DESKTOP_WEBKIT_NOT_PHYSICAL_IPHONE',target_retirement_measured=True,rows=rows)
(root/'comparison.json').write_text(json.dumps(result,indent=2)+'\n')
print(json.dumps(result))
