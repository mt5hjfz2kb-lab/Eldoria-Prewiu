/* Eldoria v0.24 canonical timestamp task primitives. No timers are authoritative. */
(()=>{'use strict';const E=window.ELDORIA=window.ELDORIA||{};
const create=(key,title,target,seconds,now=Date.now())=>({key,title,target,start:now,end:now+Math.max(0,seconds)*1000});
const finished=(tasks,now=Date.now())=>(tasks||[]).filter(t=>now>=t.end);
const pending=(tasks,now=Date.now())=>(tasks||[]).filter(t=>now<t.end);
const remaining=(task,now=Date.now())=>Math.max(0,Math.ceil((task.end-now)/1000));
E.tasks={create,finished,pending,remaining};
})();