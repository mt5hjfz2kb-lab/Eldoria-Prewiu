# Studio visibility failure and reserved layer investigation

Actual native43162955876 compiled and entered the real studio path, restored original renderer/light/ambient/fog state, then failed the unchanged first1600x900GPU visibility check:198/2304lit samples,60color bins. Artifact13918554048 SHA77ddf103462f881f4c03d486d8a91f1aeccd171c6e7fb25bc1d2bb100dd45d8d preserves the failure. No actual studio beauty image was written before the assertion, so the result cannot be judged as finished art.

Investigation found our supposedly unused geometry layer31 is reserved by Unity6000.3 for the Editor Preview window. Official create-layers documentation explicitly says not to use it. The candidate now searches only layers30…8 for geometry and grading, and initializes the temporary URP lights through GetUniversalAdditionalLightData. This is a technical correction to a documented invalid layer choice; the next actual native render must verify whether it explains the visibility failure. No exposure, lighting intensity, sampling region or pixel threshold is relaxed.

Both real GPU guards now preserve an unmodified full-resolution readback of the original RenderTexture before throwing on failure, with exact dimensions and SHA256 under FailedGpuReadback. This diagnostic is explicitly FAILED, never an art pass. It permits inspection of actual geometry/lighting if the next render still fails. The same native20images/8genuinebuilt/8restoration/8perimeter contracts remain required.

Primary documentation: https://docs.unity.com/en-us/engine/6000.3/manual/working-with-gameobjects/layers/create and universal-additional-light-data. Pending source/native/image review; no main art or phone approval.
