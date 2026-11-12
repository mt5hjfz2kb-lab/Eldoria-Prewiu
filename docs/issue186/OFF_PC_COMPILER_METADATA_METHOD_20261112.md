# Stop spending native turns on syntax-only confidence

The resource pipeline repeatedly passed source parsing but encountered real Unity compiler errors at assembly boundaries. Roslyn parsing cannot establish symbol/API/assembly validity. The native startup overhead is approximately ten minutes in recent resource captures; its cause is not asserted without timestamps. Repeating source-only parsing followed by another full native attempt is inadequate.

The next owned resource batch exports read-only metadata from the actual installed Unity engine/API assemblies and a strict allowlist of existing project dependency DLLs. Copy only DLLs with verified SHA256 and record installed editor ProductVersion, source commit, run, files and missing dependencies. Exclude executables, licenses, credentials, settings, logs, PDBs, player data and whole-project archives. Keep the isolated Unity import cache. No tool installation, paid service or project-art mutation.

Upload these exact metadata files before Unity capture begins. This permits real C# semantic compilation off-PC against the installed Unity6000.3 dependencies while the PC performs graphics work. Include the complete current Royal Gold V2 runtime plus its newly joined Issue186 runtime sources; exclude any stale compiled V2 copy. Distinguish this actual API/compiler check from Unity import, GPU rendering, real pointer dispatch, player flow, art and physical-device proof.

PowerShell parser passes locally. Export and semantic compilation remain pending the next legitimate187Windows turn after181. Retain actual native verification; this augments it, rather than waiving graphics or command acceptance.
