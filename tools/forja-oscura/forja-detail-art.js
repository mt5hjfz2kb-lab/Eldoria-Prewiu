/* Eldoria Forja Oscura — deterministic first-party brushed-metal UI art.
 * Generates independent RGBA PNGs, no game backgrounds, screenshots, text or
 * third-party sprite sheets. Browserless implementation uses 8-bit indexed PNG
 * and uncompressed zlib; safe to rerun in CI with node >=18.
 * Nine-slice border values are managed by the existing Unity .meta importer.
 */
'use strict';
function makeAsset(kind,w,h,seed=17,ornamentation=true){
 const pal=[[0,0,0,0]];
 const ramp=(n,a,b)=>{const s=pal.length;for(let i=0;i<n;i++){let t=i/(n-1);pal.push([...a.map((v,k)=>Math.round(v+(b[k]-v)*t)),255]);}return [s,s+n-1];};
 const dark=ramp(88,[10,16,23],[54,66,79]), gold=ramp(70,[48,35,27],[240,185,96]), black=ramp(19,[2,4,8],[32,38,46]), ember=ramp(30,[137,53,22],[255,162,79]), silver=ramp(22,[48,55,61],[134,144,150]), ivory=ramp(26,[160,134,94],[255,235,179]);
 const clamp=(v,l,u)=>Math.max(l,Math.min(u,v));
 function N(x,y){let v=Math.sin(x*127.1+y*311.7+seed*53.9)*43758.5453;return v-Math.floor(v);}
 function smooth(x,y){let ix=Math.floor(x),iy=Math.floor(y),dx=x-ix,dy=y-iy,sx=dx*dx*(3-2*dx),sy=dy*dy*(3-2*dy);return (N(ix,iy)*(1-sx)+N(ix+1,iy)*sx)*(1-sy)+(N(ix,iy+1)*(1-sx)+N(ix+1,iy+1)*sx)*sy;}
 function noise(x,y){return .52*smooth(x/17,y/12)+.28*smooth(x/5,y/3)+.2*N(x,y);}
 function segdist(x,y,xa,ya,xb,yb){let dx=xb-xa,dy=yb-ya,t=clamp(((x-xa)*dx+(y-ya)*dy)/(dx*dx+dy*dy),0,1);return Math.hypot(x-(xa+dx*t),y-(ya+dy*t));}
 // Every visible corner and chamfer must fit entirely in the fixed
 // 9-slice corners. Otherwise the mobile center strip crushes gold rivets
 // and diagonal ornaments when the native uGUI rect becomes 44–80 units tall.
 const slice=kind==='panel'?[56,20]:kind==='quest'?[56,22]:
   kind==='chip'?[36,14]:(kind==='nav'||kind==='selected')?[48,22]:[56,15];
 const cornerX=slice[0],cornerY=slice[1];
 const c=kind==='panel'?18:kind==='quest'?19:kind==='chip'?12:
   (kind==='nav'||kind==='selected')?17:14;
 const poly=[[c,0],[w-c,0],[w,c],[w,h-c],[w-c,h],[c,h],[0,h-c],[0,c]];
 const raw=new Uint8Array((w+1)*h), isPanel=(kind==='panel'||kind==='quest'||kind==='chip');
 let minB=kind==='chip'?9:12;
 for(let y=0;y<h;y++){
  for(let x=0;x<w;x++){
   let k=0,px=x+.5,py=y+.5,inside=true;
   for(let j=0;j<8;j++){const a=poly[j],b=poly[(j+1)%8];if((b[0]-a[0])*(py-a[1])-(b[1]-a[1])*(px-a[0])<0){inside=false;break;}}
   if(inside){
    let d=1e10;for(let j=0;j<8;j++){let a=poly[j],b=poly[(j+1)%8];d=Math.min(d,segdist(px,py,...a,...b));}
    let texture=noise(x,y),light=(.5-texture)*.8,top=y/h,diag=(1-top)*.38;
    if(d<1.5)k=black[0]+4;
    else if(d<3.8)k=gold[0]+clamp(Math.round(30+diag*30+light*17),1,69);
    else if(d<5.2)k=black[0]+9;
    else if(d<8)k=gold[0]+clamp(Math.round(13+diag*23+light*12),1,69);
    else if(d<10.8)k=black[0]+8;
    else if(d<minB)k=gold[0]+clamp(Math.round(12+diag*8+light*9),1,69);
    else {
     // Reference: charcoal metal, bronze-selected navigation, ember-lit CTA.
     // These palettes are native to the existing 8-bit first-party source.
     const selected=kind==='selected';
     const m=selected?gold:
       (kind==='button'||kind==='hover'||kind==='pressed')?ember:
       kind==='disabled'?silver:kind==='nav'?dark:gold;
     if(isPanel){
      let q=36+7*(.5-top)+9*(texture-.5)+6*(smooth(x/43,y/29)-.5)+5*(1-Math.min(y,150)/150);
      if(d<18)q+=7*(1-top);
      if(y>h*.9)q-=5;
      k=dark[0]+clamp(Math.round(q),3,84);
      if(N(x+11,y*.8)>.988&&y>20&&y<h-20)k=dark[0]+clamp(Math.round(q+11),3,84);
     }else{
      let q=(kind==='nav'?27:selected?12:kind==='button'?17:kind==='hover'?20:kind==='pressed'?13:12)+
        (kind==='nav'?12:8)*(texture-.5)+
        (kind==='nav'?22:selected?7:6)*(.54-Math.abs(top-.46))+
        6*(smooth(x/63,y/37)-.5);
      if(y<15)q+=kind==='nav'?10:4;
      if(y>h*.84)q-=kind==='nav'?12:4;
      if(kind==='hover')q+=6;
      if(kind==='pressed')q-=5;
      if(kind==='disabled')q=12+4*(texture-.5);
      k=m[0]+clamp(Math.round(q),0,m[1]-m[0]);
      // Small amber selection rim, no full-screen glow or fake gameplay state.
      if(selected&&d<21)
       k=ember[0]+clamp(Math.round(7+(21-d)*2.6+light*4),2,29);
     }
     if(kind==='button'&&Math.abs(y-h*.27)<1.3&&x>25&&x<w-25)k=gold[0]+55;
     if(kind==='hover'&&Math.abs(y-h*.27)<1.3&&x>25&&x<w-25)k=ivory[0]+13;
     if(kind==='selected'&&y>h-16&&x>w*.39&&x<w*.61)k=ember[0]+25;
    }
     const nx=Math.min(x,w-1-x),ny=Math.min(y,h-1-y);
     // Ornamental marks are baked ONLY into the fixed 9-slice corner.
     // Stretchable bands retain brushed metal and continuous trim, never
     // a circular rivet or diagonal fleur that could collapse in portrait.
     if(ornamentation&&nx>12&&nx<cornerX-2&&ny>2&&ny<cornerY-2){
      const ax=Math.max(c+2,cornerX-33),ay=cornerY-4;
      const bx=cornerX-5,by=Math.max(3,cornerY-11);
      const stroke=segdist(nx,ny,ax,ay,bx,by);
      if(stroke<2.5)k=black[0]+4;
      if(stroke<1.05)k=gold[0]+clamp(26+Math.round((cornerY-ny)*.7),5,67);
      const rx=cornerX-17,ry=cornerY-7;
      const radius=Math.hypot(nx-rx,ny-ry);
      if(radius<4.7&&radius>3.0)k=black[0]+5;
      if(radius<=3.0)k=gold[0]+clamp(Math.round(27+(ry-ny)*6),5,67);
      if(radius<1.3)k=ivory[0]+12;
     }
   }
   raw[y*(w+1)+x+1]=k;
  }
 }
 const be32=n=>[(n>>>24)&255,(n>>>16)&255,(n>>>8)&255,n&255];
 const uint=a=>new Uint8Array(a);
 const table=new Uint32Array(256);
 for(let i=0;i<256;i++){let v=i;for(let k=0;k<8;k++)v=(v&1)?(0xedb88320^(v>>>1)):(v>>>1);table[i]=v>>>0;}
 const crc=bytes=>{let v=0xffffffff;for(let i=0;i<bytes.length;i++)v=table[(v^bytes[i])&255]^(v>>>8);return(v^0xffffffff)>>>0;};
 function cat(arr){let n=arr.reduce((s,a)=>s+a.length,0),o=new Uint8Array(n),i=0;for(const a of arr){o.set(a,i);i+=a.length;}return o;}
 function chunk(name,payload){let marker=uint(name.split('').map(ch=>ch.charCodeAt(0))),content=cat([marker,payload]);return cat([uint(be32(payload.length)),content,uint(be32(crc(content)))]);}
 let parts=[uint([0x78,0x01])],aa=1,bb=0;
 for(let i=0;i<raw.length;i++){aa=(aa+raw[i])%65521;bb=(bb+aa)%65521;}
 for(let p=0;p<raw.length;){let n=Math.min(65535,raw.length-p),last=p+n>=raw.length,inv=(~n)&65535,block=new Uint8Array(n+5);block.set([last?1:0,n&255,n>>>8,inv&255,inv>>>8],0);block.set(raw.subarray(p,p+n),5);parts.push(block);p+=n;}
 parts.push(uint(be32(((bb<<16)|aa)>>>0)));
 const plt=new Uint8Array(256*3),alpha=new Uint8Array(256).fill(255);alpha[0]=0;
 for(let i=0;i<256;i++)plt.set((pal[i]||[0,0,0,255]).slice(0,3),i*3);
 const png=cat([uint([137,80,78,71,13,10,26,10]),chunk('IHDR',uint([...be32(w),...be32(h),8,3,0,0,0])),chunk('PLTE',plt),chunk('tRNS',alpha),chunk('IDAT',cat(parts)),chunk('IEND',new Uint8Array())]);
 const alphabet="ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
 let b64="";for(let i=0;i<png.length;i+=3){let d=(png[i]<<16)|((png[i+1]??0)<<8)|(png[i+2]??0);b64+=alphabet[(d>>>18)&63]+alphabet[(d>>>12)&63]+(i+1<png.length?alphabet[(d>>>6)&63]:'=')+(i+2<png.length?alphabet[d&63]:'=');}
 return {png,b64,kind,w,h,seed,bevel:c};
}
function variants(){return [
 ['panel_dark','panel',640,560,29],['panel_quest','quest',800,186,32],
 ['resource_chip','chip',440,112,40],['nav_tile_normal','nav',256,164,41],
 ['nav_tile_selected','selected',256,164,42],
 ['button_primary_normal','button',640,168,17],
 ['button_primary_hover','hover',640,168,19],
 ['button_primary_pressed','pressed',640,168,21],
 ['button_primary_disabled','disabled',640,168,23]
];}
if(typeof module!=='undefined')module.exports={makeAsset,variants};
