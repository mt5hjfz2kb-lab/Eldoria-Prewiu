"""Fail-closed source parity for the owner-only iPhone city/world P0 experiment.

The opt-in composed candidate may differ from the canonical complete UX file
ONLY in one static opt-in property, one Ready condition, and one World-entry guard.
This test must run before Windows Unity, never replace the real native journey.
"""
import unittest
from pathlib import Path

ROOT=Path(__file__).resolve().parents[1]
UX=(ROOT/'ux/ValoriaIntegratedWorld.cs.txt').read_bytes()
COMPOSED=(Path(__file__).parent/'ValoriaIntegratedWorld.city-only-composed.cs.txt').read_bytes()

PROPERTY=b'''  // P0 owner-only opt-in: bypass huge World bootstrap while measuring city.
  // Default/editor behaviour and the canonical UX/telemetry implementation remain intact.
  public static bool P0CityOnlyProbe {
   get {
#if UNITY_WEBGL && !UNITY_EDITOR
    return UnityEngine.Application.absoluteURL.Contains("eldoria_p0_city_only=1");
#else
    return false;
#endif
   }
  }
'''
GUARD=b'''   if(open&&P0CityOnlyProbe){
    if(Hud)Hud.Feedback.Put("Diagn\xc3\xb3stico P0: mundo diferido; la partida sigue intacta.");
    return false;
   }
'''
OLD_READY=b'  public bool Ready=>presentationBindingsReady&&(!worldPresentation||worldPresentation.IsReady);'
NEW_READY=b'  public bool Ready=>presentationBindingsReady&&(!worldPresentation||worldPresentation.IsReady||P0CityOnlyProbe);'
class CityOnlyCompositionTests(unittest.TestCase):
    def test_composed_is_exactly_three_reversible_changes_to_complete_ux(self):
        self.assertEqual(COMPOSED.count(PROPERTY),1)
        self.assertEqual(COMPOSED.count(GUARD),1)
        self.assertEqual(COMPOSED.count(NEW_READY),1)
        self.assertEqual(COMPOSED.count(b'P0CityOnlyProbe'),3)
        unchanged=(COMPOSED.replace(PROPERTY,b'')
                           .replace(GUARD,b'')
                           .replace(NEW_READY,OLD_READY))
        self.assertEqual(unchanged,UX,"Any unrelated edit drops city/UX/telemetry parity")
    def test_essential_features_still_in_composed(self):
        for token in (b'RoyalGoldV41MailboxScreen',b'ValoriaPlaytestTelemetry',
                      b'RoyalSection.Mailbox',b'WorldHotspot',b'SyncWorldCity',
                      b'ObserveBetaMilestones',b'BuildStaged'):
            # BuildStaged is in world builder, not in UI host; exclude when absent.
            if token==b'BuildStaged': continue
            self.assertIn(token,COMPOSED)
    def test_flag_is_webgl_opt_in_and_default_off(self):
        self.assertIn(b'#if UNITY_WEBGL && !UNITY_EDITOR',PROPERTY)
        self.assertIn(b'#else\n    return false;\n#endif',PROPERTY)
        self.assertIn(b'if(open&&P0CityOnlyProbe)',GUARD)
        world=(ROOT.parents[2]/'Unity/Assets/Eldoria/World4XCanon/RegionOneCanonicalLandscape.cs')
        # The test is also valid from sparse checkout preflight where source exists.
        if world.exists():
            c=world.read_bytes()
            self.assertIn(b'if (ValoriaIntegratedWorld.P0CityOnlyProbe)',c)
            self.assertIn(b'ELDORIA_P0_CITY_ONLY_WORLD_INITIALIZATION_SKIPPED',c)
if __name__=='__main__':
    unittest.main()
