// Fail-closed, zero-dependency Unity PNG A/B frame decoder + pixel impact gate.
// A material pixel difference permits independent artistic review, NEVER grants ART_APPROVED.
import zlib from 'node:zlib';
const signature=Buffer.from('89504e470d0a1a0a','hex');
export const MIN_VISIBLE_UNITY_FRACTION=0.001; // >=0.10% materially changed pixels
export const MIN_UNITY_CHANNEL_DIFF=5;       // of 255; avoids near-noise
function pngCRC(bytes){
 let c=0xffffffff;
 for(const byte of bytes){
  c^=byte;
  for(let k=0;k<8;k++)c=(c>>>1)^((c&1)?0xedb88320:0);
 }
 return(c^0xffffffff)>>>0;
}
export function decodeUnityPng(bytes){
 if(!Buffer.isBuffer(bytes)||bytes.length<45||!bytes.subarray(0,8).equals(signature))
   throw Error('Unity A/B requires genuine PNG images, not JPEG or a fake signature');
 let pos=8,width=0,height=0,bpp=0,seenIHDR=false,seenIEND=false,idat=[];
 while(pos+12<=bytes.length){
  const len=bytes.readUInt32BE(pos),type=bytes.toString('ascii',pos+4,pos+8);
  if(len>268435456||pos+12+len>bytes.length)throw Error('Truncated/oversized PNG chunk');
  const data=bytes.subarray(pos+8,pos+8+len);
  const expectedCRC=bytes.readUInt32BE(pos+8+len);
  if(pngCRC(bytes.subarray(pos+4,pos+8+len))!==expectedCRC)throw Error('PNG chunk checksum invalid');
  if(type==='IHDR'){
   if(seenIHDR||len!==13||pos!==8)throw Error('Malformed PNG header');
   width=data.readUInt32BE(0);height=data.readUInt32BE(4);
   const bitDepth=data[8],colorType=data[9];
   if(width<8||height<8||width>8192||height>8192||
      bitDepth!==8||![2,6].includes(colorType)||
      data[10]!==0||data[11]!==0||data[12]!==0)
     throw Error('Unity A/B only accepts noninterlaced 8-bit RGB/RGBA PNG up to 8192x8192');
   bpp=colorType===2?3:4;seenIHDR=true;
  }else if(type==='IDAT'){if(!seenIHDR||seenIEND)throw Error('Invalid PNG IDAT order');idat.push(data)}
  else if(type==='IEND'){seenIEND=true;break}
  pos+=12+len;
 }
 if(!seenIHDR||!seenIEND||idat.length===0)throw Error('Missing real PNG image data');
 const stride=width*bpp,expected=(stride+1)*height;
 if(expected>256*1024*1024)throw Error('Unity PNG expands beyond memory safety limit');
 let raw;try{raw=zlib.inflateSync(Buffer.concat(idat),{maxOutputLength:expected})}
 catch(err){throw Error('PNG compressed pixel stream invalid: '+err.message)}
 if(raw.length!==expected)throw Error('PNG pixel stream length mismatch');
 const rgb=Buffer.allocUnsafe(width*height*3),prev=Buffer.alloc(stride),cur=Buffer.allocUnsafe(stride);
 let offset=0,rgbOff=0;
 for(let y=0;y<height;y++){
  const filter=raw[offset++];
  if(filter>4)throw Error('Invalid PNG scanline filter');
  for(let i=0;i<stride;i++){
   const a=i>=bpp?cur[i-bpp]:0,b=prev[i],c=i>=bpp?prev[i-bpp]:0;
   let predict=0;
   if(filter===1)predict=a;
   else if(filter===2)predict=b;
   else if(filter===3)predict=Math.floor((a+b)/2);
   else if(filter===4){
    const p=a+b-c,da=Math.abs(p-a),db=Math.abs(p-b),dc=Math.abs(p-c);
    predict=da<=db&&da<=dc?a:db<=dc?b:c;
   }
   cur[i]=(raw[offset++]+predict)&255;
  }
  for(let i=0;i<stride;i+=bpp){
   rgb[rgbOff++]=cur[i];rgb[rgbOff++]=cur[i+1];rgb[rgbOff++]=cur[i+2];
  }
  cur.copy(prev);
 }
 return{width,height,rgb};
}
export function measureUnityVisualChange(beforeBytes,afterBytes){
 const a=decodeUnityPng(beforeBytes),b=decodeUnityPng(afterBytes);
 if(a.width!==b.width||a.height!==b.height)
   throw Error('Matched Unity A/B requires identical image dimensions and frozen camera framing');
 let changed=0;
 for(let i=0;i<a.rgb.length;i+=3){
  if(Math.abs(a.rgb[i]-b.rgb[i])>MIN_UNITY_CHANNEL_DIFF||
     Math.abs(a.rgb[i+1]-b.rgb[i+1])>MIN_UNITY_CHANNEL_DIFF||
     Math.abs(a.rgb[i+2]-b.rgb[i+2])>MIN_UNITY_CHANNEL_DIFF)changed++;
 }
 const total=a.width*a.height,ratio=changed/total;
 return{width:a.width,height:a.height,total_pixels:total,changed_pixels:changed,
         changed_fraction:ratio,minimum_fraction:MIN_VISIBLE_UNITY_FRACTION,
         channel_difference_threshold:MIN_UNITY_CHANNEL_DIFF,
         gate:ratio>=MIN_VISIBLE_UNITY_FRACTION?'INDEPENDENT_REVIEW_REQUIRED':'VISUAL_REJECTED',
         note:'INDEPENDENT_REVIEW_REQUIRED is not an artistic, gameplay or mobile PASS'};
}
export function certifyUnityAB(beforeBytes,afterBytes){
 const proof=measureUnityVisualChange(beforeBytes,afterBytes);
 if(proof.gate!=='INDEPENDENT_REVIEW_REQUIRED')
   throw Error('UNITY_AB_VISUAL_REJECTED: only '+proof.changed_pixels+'/'+proof.total_pixels+
               ' pixels change materially ('+(100*proof.changed_fraction).toFixed(5)+'%); requires >= '+
               (100*MIN_VISIBLE_UNITY_FRACTION).toFixed(2)+'% and independent artistic review');
 return proof;
}
