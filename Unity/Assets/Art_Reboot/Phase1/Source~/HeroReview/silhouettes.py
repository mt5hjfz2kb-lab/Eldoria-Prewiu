from PIL import Image,ImageDraw,ImageFont
from pathlib import Path
out=Path(__file__).parent
im=Image.new('RGB',(1500,660),'#eeeae2');d=ImageDraw.Draw(im)
font=ImageFont.truetype('/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf',23)
# Design contours, drawn before 3D authoring; all masses solid black.
shapes=[[(65,470),(75,270),(50,260),(155,130),(260,240),(285,240),(285,150),(270,150),(270,105),(295,105),(295,125),(325,125),(325,105),(350,105),(350,150),(340,150),(345,460),(365,485)],
[(45,480),(65,360),(82,355),(83,280),(50,270),(75,245),(135,140),(201,130),(270,205),(278,85),(261,68),(305,30),(348,78),(334,90),(339,340),(365,350),(370,480),(275,480),(270,325),(245,325),(238,470)],
[(55,485),(67,340),(44,320),(90,265),(94,205),(65,190),(180,90),(215,95),(321,220),(300,235),(300,325),(355,360),(352,485)]]
for i,p in enumerate(shapes):
 ox=i*500+50
 d.polygon([(x+ox,y+60) for x,y in p],fill='black')
 if i==1:
  d.polygon([(ox+190,220),(ox+187,156),(ox+211,154),(ox+216,233)],fill='black')
  d.rectangle((ox+330,420,ox+390,438),fill='black')
 d.text((i*500+65,585),['A  Sala y torre almenada','B  Casa de la Campana','C  Gran tejado central'][i],fill='#252525',font=font)
 if i==1:d.line((550,630,940,630),fill='#867348',width=5)
im.save(out/'silhouettes.png')
