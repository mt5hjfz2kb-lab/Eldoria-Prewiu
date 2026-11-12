# Daily resource rewards — saved gateway change in PR187

Daily prizes now issue their exact wood/stone/food as persistent resource packets in a locked reward email. A daily claim advances the seven-day cycle but does not add resources to the wallet. Opening or reading the mail does not collect. Explicit ClaimResourceRewardMail puts all packets in the bag, and UseResourceRewardPacket credits the exact amount once, with used receipt tombstones saved. Only speedups bypass packets.

The stable letter and receipt base is daily-treasure:<UTC day>. Existing/duplicate mail keys, already-issued bundle components, command replays and overflows fail without orphaning a partially advanced cycle; packet bundles are staged on an independent copy before modifying the live state. An informational acceleration-only day has no fake resource packet. Existing production from nodes, hunts and buildings is unchanged.

The source-only real IStateStore JSON gateway suite now tests day 1, day 4, day 5, claim/read/use separation, replay, load/save, multi-resource atomicity and full-wallet behavior. Historical daily-only projects without the resource feature retain their original compiled branch; Unity and the dedicated reward suite use packet behavior.

NOT yet validated: real Unity compilation of the new gateway, bag/mail UI presentation, source-to-live-UI binding, mobile playability, art reference A/B or owner approval. This is not approval to merge or publish. #181 art/UI remains owned by its original workstream.