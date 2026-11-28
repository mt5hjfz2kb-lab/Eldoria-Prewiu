"""Protected region repair: retain exact original RGBA outside the authorised region.
This is a compositing/registration method, not model weight training."""
from PIL import Image,ImageDraw,ImageFilter,ImageChops
def repair(original,proposal,polygon,feather=4):
    if original.mode!="RGBA" or proposal.mode!="RGBA" or original.size!=proposal.size:raise ValueError("Exact original canvas required")
    if len(polygon)<3 or not all(0<=x<original.width and 0<=y<original.height for x,y in polygon):raise ValueError("Invalid authorised region")
    if not 0<=feather<=8:raise ValueError("Invalid feather")
    mask=Image.new("L",original.size,0);ImageDraw.Draw(mask).polygon(polygon,fill=255)
    # Feather strictly INWARD: no modified pixel outside the approved polygon.
    blend=ImageChops.multiply(mask,mask.filter(ImageFilter.GaussianBlur(feather)))
    blend=ImageChops.multiply(blend,original.getchannel("A"))
    blend=ImageChops.multiply(blend,proposal.getchannel("A"))
    result=Image.composite(proposal,original,blend);result.putalpha(original.getchannel("A"))
    a,b=result.tobytes(),original.tobytes();allowed=mask.tobytes()
    changed=[i for i in range(len(allowed)) if a[4*i:4*i+4]!=b[4*i:4*i+4]]
    if not changed or any(not allowed[i] for i in changed):raise ValueError("Empty repair or protected pixel regression")
    if result.getchannel("A").tobytes()!=original.getchannel("A").tobytes():raise ValueError("Alpha changed")
    return result,mask,{"changed_pixels":len(changed),"total_pixels":len(allowed),"changed_outside_region":0,"alpha_identical":True,"canvas_identical":True,"allowed_pixels":sum(x>0 for x in allowed)}
if __name__=="__main__":
    a=Image.new("RGBA",(32,32),(10,20,30,255));b=Image.new("RGBA",a.size,(40,50,60,255))
    r,m,q=repair(a,b,[(8,8),(24,8),(24,24),(8,24)],2)
    assert r.getpixel((0,0))==a.getpixel((0,0)) and r.getpixel((16,16))!=a.getpixel((16,16))
    assert q["changed_outside_region"]==0 and q["alpha_identical"]
    try:repair(a,b,[(1,1),(50,1),(1,5)]);raise AssertionError("Invalid polygon accepted")
    except ValueError:pass
    try:repair(a,a,[(1,1),(5,1),(1,5)]);raise AssertionError("Empty repair accepted")
    except ValueError:pass
    print("PROTECTED_REGION_SELFTEST_PASS")
