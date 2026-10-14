"""Executed Blender cycles are not artistic successes when protections reject them."""
import pathlib,sys,unittest
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parents[1]))
from artist_loop import summarize_real_artist_cycles

class HonestExecutionTests(unittest.TestCase):
    def test_three_real_protected_access_rejections_report_no_review_candidate(self):
        records=[{"cycle":i,"status":"PROTECTED_ACCESS_OCCLUSION_REJECTED"} for i in (1,2,3)]
        result=summarize_real_artist_cycles(records)
        self.assertEqual(result["real_blender_execution_count"],3)
        self.assertEqual(result["qa_review_candidate_count"],0)
        self.assertEqual(result["protected_access_rejection_count"],3)
    def test_screen_rejected_not_art_candidate(self):
        result=summarize_real_artist_cycles([{"status":"SCREENSPACE_REJECTED"}])
        self.assertEqual(result["real_blender_execution_count"],1)
        self.assertEqual(result["qa_review_candidate_count"],0)
    def test_pending_independent_review_is_not_a_certification(self):
        result=summarize_real_artist_cycles([
            {"status":"ART_REVIEW_REQUIRED","critique":{"clearly_failed":True}},
            {"status":"BLENDER_REJECTED"}])
        self.assertEqual(result["real_blender_execution_count"],1)
        self.assertEqual(result["qa_review_candidate_count"],1)
        self.assertEqual(result["model_self_critique_failed_count"],1)

if __name__=="__main__":unittest.main()
