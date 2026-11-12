# Actual relative-path recovery and artifact isolation

Windows43131672590 (73e5777b) stopped beforeUnity. I passed the known source path relative to the Actions workspace into git-C's separate warm checkout; Git resolved it under the wrong directory. The guarded function refused before restoring any source. The previous real PlayMode pass remains43127449523, not this run.

Resolve-Path now makes the known source file absolute BEFORE git-C. The real Git/PowerShell fixture includes the actual relative-tools case;32route recovery checks pass (prior26 plus the new case).58material checks are retained. This still requires actual Windows recovery/native verification.

The failed run also uploaded old full-native folders because upload paths used wildcards. Source identity checks prevented packaging or false certification. Uploads now include ONLY this run and attempt; missing current full-native results correctly produce no full-native artifact. The packager already discovers marker files recursively with exactly-one validation, so flat versus nested artifact roots are supported.

No original dirty audit/baseline was changed. Next turn after181and187 must verify actual recovery, fullWorldI-V/WebGL and byte-exact restoration; no physicalphone, publictester, art ormain claims.
