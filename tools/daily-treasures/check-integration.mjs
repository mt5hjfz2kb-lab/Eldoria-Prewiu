import fs from 'node:fs';
import assert from 'node:assert/strict';
const domain=fs.readFileSync('Unity/Assets/Eldoria/Scripts/Domain/GameState.cs','utf8');
const gateway=fs.readFileSync('Unity/Assets/Eldoria/Scripts/Application/LocalGateway.cs','utf8');
const rules=fs.readFileSync('Unity/Assets/Eldoria/Scripts/Domain/DailyTreasureRules.cs','utf8');
assert.match(domain,/DailyTreasureState DailyTreasures/);
assert.match(gateway,/DailyTreasures=s\.DailyTreasures\?\.Copy\(\)/);
assert.match(gateway,/case "ClaimDailyTreasure":/);
assert.match(gateway,/DailyTreasureRules\.TryClaim\(/);
assert.match(gateway,/EventId="daily-treasure:"\+state\.DailyTreasures\.LastClaimUtcDay/);
assert.match(gateway,/RewardReceiptId=command\.Id/);
assert.match(gateway,/state\.Mailbox\.Receive\(/);
assert.match(gateway,/state\.CompletedCommandIds\.Add\(command\.Id\);/);
const executableRules=rules.replace(/\/\/[^\n]*/g,''); // comments are documentation, not executable dependencies
assert.doesNotMatch(executableRules,/\bPlayerPrefs\b|\bUnityEngine\b|\bSystem\.Random\b/);
console.log('PASS daily treasures gateway integration source invariants');
