"""Repair only the QA traversal of the real Council list; no gameplay/fixture injection."""
import sys
from pathlib import Path
def main():
    src,dest=map(Path,sys.argv[1:3])
    s=src.read_text(encoding="utf-8")
    old="""            if not target and scroll:
                # Reset towards the first Council row, then traverse the real
                # masked list far enough to pass accumulated reward packets.
                # A complete I-V journey can legitimately expose more than
                # twenty orders before the canonical system destinations.
                for a,b in [(430,690)]*4 + [(655,300)]*28:
                    await swipe(195,a,195,b)
                    target=find(name)
                    if target: break"""
    new="""            if not target and scroll and state.get('council'):
                # Real user navigation resets the scroll to the top. Fixed
                # swipe counts were not sufficient after the taller art rows.
                # Never manipulate game state, inject commands, or bypass a button.
                await tap('Close state',False)
                await council()
                target=find(name)
                log.append({'council_reopened_for_search':name,'level':state.get('level')})
                last=None
                stable=0
                for attempt in range(48):
                    if target: break
                    visible=[t for t in state.get('taps',[]) if t['name']!='Close state']
                    sig=tuple((t['name'],round(t['y'],3)) for t in visible)
                    if sig==last: stable+=1
                    else: stable=0
                    last=sig
                    if stable>=3: break
                    ys=sorted(t['y']*844 for t in visible)
                    # Use the actual visible rows, not a coordinate outside
                    # the scroll viewport. Overlap adjacent pages.
                    a=ys[-1]-6 if len(ys)>=2 else 650
                    b=ys[0]+8 if len(ys)>=2 else 455
                    if a-b<75: a,b=650,455
                    await swipe(195,a,195,b)
                    target=find(name)
                log.append({'council_search':name,'found':target is not None,'level':state.get('level')})
            elif not target and scroll:
                for a,b in [(430,690)]*4 + [(655,300)]*28:
                    await swipe(195,a,195,b)
                    target=find(name)
                    if target: break"""
    if s.count(old)!=1:raise ValueError("Unexpected source; refuse to modify journey")
    result=s.replace(old,new)
    compile(result,str(dest),"exec")
    dest.write_text(result,encoding="utf-8")
if __name__=="__main__":main()
