// Bundled with esbuild from pinned Apache-2.0 @ably/vcdiff-decoder 1.0.6.
// Do not fetch a mutable third-party CDN at browser runtime.
import * as VCDIFF from '@ably/vcdiff-decoder';
const fn=VCDIFF.decode || (VCDIFF.default && VCDIFF.default.decode);
if(typeof fn!=='function')throw Error('Eldoria pinned VCDIFF decoder unavailable');
window.EldoriaVCDIFF=Object.freeze({
  decode:(delta,source)=>fn(delta,source)
});
