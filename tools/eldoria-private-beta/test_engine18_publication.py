import copy
import unittest
from verify_engine18_owner_publication import validate_desktop, NEW_SOURCE, NEW_BUNDLE


class DesktopPublicationEvidence(unittest.TestCase):
    def setUp(self):
        self.report = dict(passed=True, errors=[], entry=dict(reloaded=True),
                           lazyWorldDeferredToFirstEntryVerified=True,
                           identity=dict(source_sha=NEW_SOURCE, candidate_bundle_sha256=NEW_BUNDLE,
                                         release_for_external_testers=False),
                           storyAdvanceAttempt=dict(actualCanvasClicks=[{}] * 5))

    def test_exact_staged_reload_evidence_is_required(self):
        validate_desktop(self.report)
        self.report['entry']['reloaded'] = False
        with self.assertRaises(AssertionError):
            validate_desktop(self.report)

    def test_wrong_engine_and_browser_errors_cannot_be_promoted(self):
        for change in ('engine', 'errors', 'gate'):
            report = copy.deepcopy(self.report)
            if change == 'engine':
                report['identity']['source_sha'] = 'previous engine'
            elif change == 'errors':
                report['errors'] = ['runtime error']
            else:
                report['identity']['release_for_external_testers'] = True
            with self.assertRaises(AssertionError):
                validate_desktop(report)


if __name__ == '__main__':
    unittest.main()
