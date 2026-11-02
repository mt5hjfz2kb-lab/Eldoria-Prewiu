import io, sys, tempfile, unittest, zipfile
from pathlib import Path
from unittest.mock import patch
from PIL import Image
sys.path.insert(0, str(Path(__file__).resolve().parent))
import compare

def png(color=(0,0,0),size=(12,8)):
    b=io.BytesIO()
    Image.new('RGB',size,color).save(b,format='PNG')
    return b.getvalue()
def archive(items):
    b=io.BytesIO()
    with zipfile.ZipFile(b,'w') as z:
        for k,v in items.items(): z.writestr(k,v)
    return b.getvalue()
META={'before_sha':'a'*40,'after_sha':'b'*40,'branch':'test','verified_github_metadata':False}
class Tests(unittest.TestCase):
    def test_real_change_no_art_approval(self):
        a=archive({'x.png':png()}); b=archive({'x.png':png((255,255,255))})
        with tempfile.TemporaryDirectory() as t:
            r=compare.analyze(a,b,META,Path(t))
            self.assertFalse(r['art_approved'])
            self.assertEqual(r['screens_compared'],1)
            self.assertEqual(r['rows'][0]['artistic_verdict'],'NO EVALUADO')
            self.assertTrue(Path(t,'index.html').is_file())
    def test_identical(self):
        a=archive({'a.png':png()})
        with tempfile.TemporaryDirectory() as t:
            r=compare.analyze(a,a,META,Path(t))
            self.assertTrue(r['rows'][0]['identical_file'])
    def test_reject_path_escape(self):
        with self.assertRaises(ValueError):
            compare.pngs(archive({'../x.png':png()}))
    def test_reject_duplicate(self):
        b=io.BytesIO()
        with zipfile.ZipFile(b,'w') as z:
            z.writestr('a.png',png())
            z.writestr('a.png',png())
        with self.assertRaises(ValueError):
            compare.pngs(b.getvalue())
    def test_reject_same_sha(self):
        with tempfile.TemporaryDirectory() as t:
            with self.assertRaises(ValueError):
                compare.analyze(archive({'x.png':png()}),archive({'x.png':png()}),
                                {**META,'after_sha':'a'*40},Path(t))
    def test_mismatched_dimensions(self):
        with tempfile.TemporaryDirectory() as t:
            r=compare.analyze(archive({'x.png':png()}),
                              archive({'x.png':png(size=(22,12))}),META,Path(t))
            self.assertEqual(r['technical_status'],'INCOMPLETE')
    def test_html_escaped(self):
        a=archive({'<img src=x onerror=alert(1)>.png':png()})
        with tempfile.TemporaryDirectory() as t:
            compare.analyze(a,a,META,Path(t))
            self.assertNotIn('<img src=x',Path(t,'index.html').read_text())
    def test_pair_rejects_untrusted_branch(self):
        def fake(repo,token,path,binary=False):
            return {'status':'completed','conclusion':'success',
                    'head_repository':{'full_name':repo},'head_branch':'other',
                    'head_sha':'a'*40}
        with patch.dict('os.environ',{'GITHUB_REPOSITORY':'foo/bar','GITHUB_TOKEN':'fake'}):
            with patch.object(compare,'api',side_effect=fake):
                with self.assertRaises(ValueError): compare.source_pair(12)
    def test_no_native_artifact_is_explicit(self):
        with tempfile.TemporaryDirectory() as t, patch.object(compare,'source_pair',side_effect=ValueError('Current run lacks exact run-ID V4.1 screenshot artifact')):
            with patch.object(sys,'argv',['compare','--run-id','12','--output',t]):
                compare.main()
            import json
            r=json.loads(Path(t,'report.json').read_text())
            self.assertEqual(r['technical_status'],'NO_COMPARABLE_EVIDENCE')
            self.assertFalse(r['art_approved'])

if __name__=='__main__':unittest.main()
