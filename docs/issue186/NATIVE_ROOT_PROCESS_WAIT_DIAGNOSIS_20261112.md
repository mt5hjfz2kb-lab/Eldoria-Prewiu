Native process-tree wait diagnosis and proven replacement, 2026-11-12

R run43178513272 started Unity at07:53:27, whose final native shutdown record has UTC07:54:08.881. The workflow returned its result only at08:03:33. UI uses a different launcher on the same machine: run43185167905 observed two complete actual Editor executions at08:43:19→08:44:05 and08:44:05→08:44:50. Both retained real exit checks, fresh reports and all48native/UI/A-B gates.

R/T used Start-Process -Wait; Microsoft documents that this waits for the whole descendant process tree. The UI launcher observes the actual Editor process instead. The exact lingering descendant has not been identified. R now reuses the proven bounded root-process polling method, records its PID/duration/exit code, and refuses to continue while any Unity or Blender process remains. It does not kill a running Editor or infer completion from inactivity. Existing exact source, screenshot, pointer, save and cleanup checks remain mandatory. New R duration is pending actual verification.

Reference: https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.management/start-process?view=powershell-5.1

The next combined art proof pins UI25e893fb9f6245d813e8dceb03865a8b703b8bd2, native43185167905+independentAB SUCCESS. Comparing925→25e confirms no sprite, importer, asset or GUID changes. The same64 exact importer tuples are explicitly repinned; no generic metadata allowance is added. The new real images must verify header quantity clarity, full resource names and the original hand at native aspect before art review.
