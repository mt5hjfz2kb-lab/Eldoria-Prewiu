# Saved resource bag on the active HUD canvas

The legacy RoyalHUD canvas can be disabled by the PR173 independent canvas. Keeping the bag panel under that disabled canvas makes an otherwise compiled bag invisible. The optional bag now exposes OpenOnCanvas(Canvas) and RestorePresentation() so the active HUD can present the same saved bag without creating a second Canvas or EventSystem. All owned UI objects use UI layer 5 for the active camera.

NewPacketCount(kind) and ClaimableMailCount() project the saved gateway state; they do not conflate unread mail with unclaimed rewards. The dismiss-help action sends the existing saved DismissGuidance command and never uses a packet or marks first use completed. Only the existing UsePacket command credits resources. Safe-area scaling and pointer raycasting use the panel's actual parent canvas. Teardown destroys only this component's externally hosted panel.

Implemented source candidate only. Source CI must pass; runtime attachment, PR173 capsule binding, actual bag rendering and first-hit/modal input on the combined HUD are still required. Native resource-card captures from earlier source do not certify this integration. No main merge, visual approval or physical iPhone claim.
