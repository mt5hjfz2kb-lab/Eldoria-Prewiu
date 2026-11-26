// Frame-rate-independent critically damped motion, not game rules.
// The requested lane is selected immediately; this spring controls presentation.
export const LANE_OMEGA=22;
export function stepLane(x,v,target,dt,limit){
 if(dt<=0)return [x,v];
 const e=x-target,k=v+LANE_OMEGA*e,q=Math.exp(-LANE_OMEGA*dt);
 let nx=target+(e+k*dt)*q,nv=(v-LANE_OMEGA*k*dt)*q;
 if(nx>limit){nx=limit;nv=Math.min(0,nv)}
 if(nx< -limit){nx=-limit;nv=Math.max(0,nv)}
 if(Math.abs(nx-target)<.002&&Math.abs(nv)<.035)return [target,0];
 return [nx,nv];
}
