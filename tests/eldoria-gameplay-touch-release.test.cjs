const test=require('node:test');
const assert=require('node:assert/strict');
const fs=require('node:fs');
const s=fs.readFileSync('Unity/Assets/Eldoria/Scripts/Presentation/SlicePresenter.cs','utf8');
const begin=s.indexOf('if(pointerActive&&released)');
const end=s.indexOf('\n        }\n\n#if UNITY_WEBGL',begin);
const release=s.slice(begin,end);
const index=(needle)=>release.indexOf(needle);

test('a rapid mobile swipe is classified on pointer RELEASE before any building tap',()=>{
 assert(begin>=0&&end>begin,'actual release handler must exist');
 assert(index('IsPanGesture(pointerStart,point)')>=0,'final displacement must be checked');
 assert(index('IsPanGesture(pointerStart,point)')<index('bool shouldSelect='),'gesture must precede tap eligibility');
 assert(index('bool shouldSelect=!pointerStartedOverUi&&!pointerDragged')>=0,'dragged pointers must not select');
});
test('last swipe segment pans only when gesture began away from UI',()=>{
 assert(index('if(pointerDragged&&!pointerStartedOverUi)')>=0);
 assert(index('PanCameraByScreenDelta(point-pointerLast)')>index('if(pointerDragged&&!pointerStartedOverUi)'));
 assert(index('PanCameraByScreenDelta(point-pointerLast)')<index('pointerLast=point;'));
});
test('touch reset and navigation fallbacks cannot interpret a swipe as a tap',()=>{
 assert(index('if(usingTouch&&!pointerDragged)')>index('bool shouldSelect='));
 assert(index('TryScheduleWebBottomNavFallback(point)')>index('if(usingTouch&&!pointerDragged)'));
});
