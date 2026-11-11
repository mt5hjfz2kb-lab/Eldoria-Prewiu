# Native compiler failure and off-Windows correction

Run43105242525, native job130806789737, sourcec7a060d4b921a76bc5e20c94969dcf02a03206ec failed before Capture. Artifact13893884188 contains only the real Editor log (ZIP SHA256812437bd2d1b682ed599169c8b7587d6e9faae3fd0df26fc274836476ea3b5e2), no native artwork screenshots.

Actual errors: Issue186ArtAudit.cs49 and Issue186NativeEvidence.cs441/447/484/495 resolved unqualified Application.dataPath/isBatchMode to the Eldoria.Application namespace, producing five CS0234 failures. All five now explicitly name UnityEngine.Application; no art, quantities, guards or gameplay behavior changed. Existing source syntax checks alone could not detect name resolution.

The actual Roslyn source auditor now inspects MemberAccess syntax nodes for this known namespace collision, ignoring comments/string contents, while remaining explicit that it does NOT typecheck the complete Unity API. Negative control on the actual failing source rejects with ISSUE186_UNQUALIFIED_APPLICATION_API; the five qualified corrections pass all four real source parsers/guard. UNITY_6000_0_OR_NEWER is also enabled alongside the existing6000_3 symbol in this source audit. Real Unity must still compile/render at the next legitimate turn.

The same Editor log exposed an unrelated malformed DepthB10/layer_index.json.meta externalObjects double-brace mapping. Its previously unclaimed repair was assigned to PR173, which fixed only that YAML mapping in3b944b02c4ae0f81c94cdb09f7eada4dddf9b399, preserving GUID13b0ee326a7f41cb92bddaa4c5fbb138. Do not silently claim that repair exists on this branch or overwrite its owner. Treat old imported-depth evidence carefully until the correct source is integrated.

Windows was released to PR173 under main4fb5e240e2f73e608d492d66d4d5032751c5701d (resource writer7, UI94), retaining source writers and transferring shared reward-domain scope181→187. No retry during the next area's batch, no native/art/mobile PASS or main publication from this correction.
