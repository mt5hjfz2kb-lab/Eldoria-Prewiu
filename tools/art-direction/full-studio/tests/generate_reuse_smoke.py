"""Create ONLY a repeatable licensed-library transport test fixture.

No Qwen authorship is claimed. The fixture re-imports the preceding test
asset through the same hash-locked Blender operation used for real art reuse.
"""
import argparse,hashlib,json,pathlib

def main():
 p=argparse.ArgumentParser()
 p.add_argument("--source",type=pathlib.Path,required=True)
 p.add_argument("--out",type=pathlib.Path,required=True)
 a=p.parse_args()
 a.out.mkdir(parents=True,exist_ok=True)
 source=a.source.resolve()
 digest=hashlib.sha256(source.read_bytes()).hexdigest()
 catalogue={"schema_version":1,"assets":[{
   "relative_path":source.name,"sha256":digest,
   "license":"OWNED","reuse_approved":True,
   "origin":"Previous CI test fixture model, self-authored and local, not Qwen."}]}
 plan={"schema_version":2,"asset_id":"test-licensed-library-reuse",
       "observed_defect":"The new empty scene is missing a previously authored and approved full medieval modular asset.",
       "design_intent":"Import and reuse the existing licensed Blender module rather than regenerate fake architecture.",
       "asset_decision":"REUSE",
       "expected_visual_difference":"Authentic editable tower meshes from the verified source are visible in the exact Blender renderer.",
       "mobile_risks":"Measure imported triangles before approving reuse on a mobile device.",
       "actions":[{"id":"reuse-verified-source-asset","op":"import_asset",
           "target":"Imported Modular Tower","reason":"Reuse the SHA-verified source geometry and its licensed materials instead of redrawing it.",
           "params":{"relative_path":source.name,"sha256":digest,"location":[0,0,0],
                     "rotation":[0,0,0],"scale":[1,1,1]}}]}
 (a.out/"approved-library.json").write_text(json.dumps(catalogue,indent=2)+"\n")
 (a.out/"import-plan.json").write_text(json.dumps(plan,indent=2)+"\n")
 (a.out/"source-sha256.txt").write_text(digest+"\n")
 print("ELDORIA_LICENSED_REUSE_FIXTURE_ONLY",digest)
if __name__=="__main__":main()
