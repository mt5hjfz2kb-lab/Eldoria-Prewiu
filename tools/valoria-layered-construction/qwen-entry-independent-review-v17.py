"""V17 SOURCE08 review: actual same-player-camera V13/V17 PNG pixel and independent local vision.
No ART_APPROVED claim is generated. Qwen can err; preserve direct image evidence.
"""
import bpy,base64,hashlib,json,pathlib,time,urllib.request
import numpy as np
root=pathlib.Path.cwd()
ev=root/"docs/evidence/valoria-layered-construction-20261016/entry-zone-v17"
sha=lambda raw:hashlib.sha256(raw).hexdigest()
before=ev/"entry-BEFORE-home.png"
after=ev/"entry-AFTER-home.png"
ref=root/"references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg"
source=ev/"source-source08-material-threequarter.png"
for p in (before,after,ref,source):
 if not p.is_file():raise RuntimeError("Missing genuine capture: "+str(p))
def rgba(path):
 im=bpy.data.images.load(str(path),check_existing=False)
 w,h=im.size
 arr=np.asarray(im.pixels[:],dtype=np.float32).reshape(h,w,4).copy()
 bpy.data.images.remove(im)
 return arr
b=rgba(before);a=rgba(after)
assert b.shape==a.shape==(1024,1536,4)
diff=(np.abs(b[:,:,:3]-a[:,:,:3]).sum(axis=2)*255)
changed=diff>=30
h,w=changed.shape
roi=np.zeros((h,w),bool);roi[h-270:h-64,608:1265]=True
bbox=np.argwhere(changed)
diff_report={
 "comparison":"V13 versus V17 real Unity same HOME-camera BEFORE and AFTER 1536x1024; source unchanged except restored Bastion material roles; threshold sum abs RGB >= 30/255",
 "before_png_sha256":sha(before.read_bytes()),"after_png_sha256":sha(after.read_bytes()),
 "pixels_changed":int(changed.sum()),
 "pixels_changed_upper":int(changed[int(h*.67):].sum()),
 "pixels_changed_bastion_reference_roi":int(np.logical_and(changed,roi).sum()),
 "pixels_changed_outside_reference_roi":int(np.logical_and(changed,~roi).sum()),
 "pixel_bbox_yx":(bbox.min(axis=0).tolist()+bbox.max(axis=0).tolist()) if len(bbox)>0 else [],
 "color_change_is_not_artistic_quality":True,"art_approved":False}
(ev/"genuine-unity-material-difference.json").write_text(json.dumps(diff_report,indent=2)+"\n",encoding="utf8")
side=np.concatenate((b[::2,::2,:],a[::2,::2,:]),axis=1)
assert side.shape==(512,1536,4)
im=bpy.data.images.new("Native_Unity_V13_Left_V17_Right",width=1536,height=512,alpha=True)
im.pixels.foreach_set(side.ravel())
im.filepath_raw=str(ev/"V13-left_V17-right-true-Unity.png");im.file_format="PNG";im.save()
bpy.data.images.remove(im)
# Smaller true-image inspection copies, not generated concept art or revised Unity frames.
for src,name in [
 (before,"V13-authentic-home-visual-review.jpg"),
 (after,"V17-authentic-home-visual-review.jpg"),
 (root/"docs/evidence/valoria-layered-construction-20261016/entry-zone-v13/entry-AFTER-zoom-in.png","V13-authentic-zoom-visual-review.jpg"),
 (ev/"entry-AFTER-zoom-in.png","V17-authentic-zoom-visual-review.jpg")
]:
 if not src.is_file():continue
 im=bpy.data.images.load(str(src),check_existing=False)
 im.scale(1024,683)
 im.filepath_raw=str(ev/name);im.file_format="JPEG"
 im.save()
 bpy.data.images.remove(im)
result={"role":"INDEPENDENT_LOCAL_VISUAL_CRITIQUE_AND_GENUINE_UNITY_DIFF_V17",
 "automatic_art_approval":False,"new_models_downloaded":False,"candidate":"V17","baseline":"V13",
 "genuine_unity_pixel_diff":diff_report}
try:
 tags=json.load(urllib.request.urlopen("http://127.0.0.1:11434/api/tags",timeout=10))
 installed=[x["name"] for x in tags["models"]]
 model=next(name for name in ["qwen3-vl:8b-instruct-q4_K_M","qwen3-vl:4b-instruct"] if name in installed)
 paths=[ref,before,after,source]
 images=[p.read_bytes() for p in paths]
 prompt=(
  "Act as independent critical art reviewer for Eldoria/Valoria; actual images in order: APPROVED TARGET art, V13 real Unity HOME camera BEFORE, "
  "V17 real Unity HOME camera AFTER in identical player camera and lighting, and V17 real Blender studio view showing source materials. "
  "Both Unity images have the same Bastion geometry; exactly nine SOURCE08 original material roles were restored onto the upper Bastion only. "
  "V13 had wrongly assigned generic ground/rock/trim to timber, heraldry, masonry and metal in the nine Bastion meshes. "
  "Do NOT claim geometry, source modeling, camera or contact improved; those are unchanged in this A/B. "
  "Examine strictly what is actually visible: blue/gold flags, roof/wood/stone/metal semantic material identity and whether newly restored hierarchy "
  "is preferable from player camera, whether the upper Bastion now visually harmonizes with lower fortification, and whether it looks like premium "
  "authored architecture rather than generic blockout. Distinguish a semantic restoration from an objectively better-looking full frame. "
  "Discuss any awkward contacts, flat terrain, missing roof detail, seams, excessive flatness, materials that look like plastic or mismatched light. "
  "Important: an earlier automatic reviewer incorrectly said V7 and V12 were geometrically identical even when Unity proved differences, "
  "so do not assert pixel equality without evidence. A pixel difference alone does NOT indicate artistic quality. "
  "Return concise VALID JSON with keys verdict (LOCAL_ART_IMPROVEMENT, NO_VISIBLE_IMPROVEMENT, ART_REJECTED), scores_v13 and scores_v17 objects "
  "(each 0-10 geometry_silhouette, architecture_contact, materials_detail, Valoria_style, gameplay_readability), "
  "positives, regressions, outstanding_defects, next_artistic_modelling_action, uncertainty. No ART_APPROVED or gameplay/mobile PASS."
 )
 (ev/"independent-review-input.json").write_text(json.dumps({
  "model":model,"prompt_sha256":sha(prompt.encode()),"images":[{"path":str(p.relative_to(root)),"sha256":sha(raw)} for p,raw in zip(paths,images)],
  "visible_pixel_difference":diff_report},indent=2),encoding="utf8")
 req={"model":model,"stream":False,"format":"json","messages":[
  {"role":"system","content":"Independent senior Blender/Unity environment art critique. Distinguish materials from geometry. Ground statements only in supplied real PNG. Never issue ART_APPROVED."},
  {"role":"user","content":prompt,"images":[base64.b64encode(raw).decode() for raw in images]}],
  "options":{"temperature":.1,"num_ctx":8192,"num_predict":1200},"keep_alive":"0s"}
 start=time.monotonic()
 raw=json.load(urllib.request.urlopen(urllib.request.Request(
  "http://127.0.0.1:11434/api/chat",data=json.dumps(req).encode(),headers={"Content-Type":"application/json"}),timeout=480))
 (ev/"independent-review-raw.json").write_text(json.dumps(raw,indent=2),encoding="utf8")
 result.update(status="REAL_LOCAL_MODEL_CRITIQUE_RECORDED",model=model,elapsed_seconds=round(time.monotonic()-start,2),review=json.loads(raw["message"]["content"]))
except Exception as exc:
 result.update(status="REVIEW_UNAVAILABLE_NOT_ART_PASS",error=str(exc))
(ev/"independent-review-status.json").write_text(json.dumps(result,indent=2)+"\n",encoding="utf8")
print("VALORIA_V17_MATERIAL_VISUAL_CRITIQUE_COMPLETE",json.dumps({"status":result["status"],"pixel_delta":diff_report["pixels_changed"],"roi_delta":diff_report["pixels_changed_bastion_reference_roi"],"model":result.get("model")}))
