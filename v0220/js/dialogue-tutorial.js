/* Eldoria v0.24 dialogue/tutorial primitives shared by narrator and character presentation. */
(()=>{'use strict';const E=window.ELDORIA=window.ELDORIA||{};
E.dialogue={plain:html=>String(html||'').replace(/<[^>]+>/g,' ').replace(/\s+/g,' ').trim(),typingDelay:ch=>ch==='.'?130:ch===','?65:22};
E.tutorial={isQA:()=>new URLSearchParams(location.search).get('qa')==='1'};
})();