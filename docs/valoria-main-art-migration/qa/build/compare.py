"""Compare two locally retained, hash-verified private build measurements for the same target."""
import argparse
from collections import defaultdict
from pathlib import Path
from private_build import digest, measure, need, read, write


def load(directory):
    summary = read(directory/"manifest.json")
    need(summary["status"]=="PRIVATE_PACKAGE_VERIFIED", "Unverified package: "+str(directory))
    player = directory/("player.apk" if summary["target"]=="Android" else "player")
    measured=measure(player)
    need(all(summary[k]==v for k,v in measured.items()),"Package files changed after build.")
    packed=directory/"packed-assets.json"
    need(summary["packedSha256"]==digest(packed), "Packed asset evidence changed.")
    totals=defaultdict(int)
    for row in read(packed)["entries"]:
        totals[(row["source"],row["guid"])]+=row["bytes"]
    return summary,totals


def main():
    p=argparse.ArgumentParser(description=__doc__)
    p.add_argument("--main",required=True,type=Path)
    p.add_argument("--candidate",required=True,type=Path)
    p.add_argument("--output",required=True,type=Path)
    a=p.parse_args()
    base,old=load(a.main)
    candidate,new=load(a.candidate)
    need(all(base[k]==candidate[k] for k in ("target","unityExpected","scene","launcherSha256","observerSha256")),
         "Comparison needs the same target, Unity version, scene and build instrumentation.")
    need(base["source"]!=candidate["source"],"Both packages identify the same checkout.")
    diffs=[]
    for key in sorted(old.keys() | new.keys()):
        if key not in new or key not in old or new[key]!=old[key]:
            diffs.append({"source":key[0],"guid":key[1],"main":old.get(key),"candidate":new.get(key),
                          "deltaBytes":new.get(key,0)-old.get(key,0)})
    need(not a.output.exists(),"Select a fresh comparison path.")
    write(a.output,{"main":base["source"],"candidate":candidate["source"],"target":base["target"],
                    "mainPackageBytes":base["packageBytes"],"candidatePackageBytes":candidate["packageBytes"],
                    "deltaPackageBytes":candidate["packageBytes"]-base["packageBytes"],
                    "packedAssetChanges":diffs})
if __name__=="__main__":
    main()
