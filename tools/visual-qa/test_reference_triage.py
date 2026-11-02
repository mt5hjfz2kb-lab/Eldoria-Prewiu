import io,sys,tempfile,unittest,zipfile
from pathlib import Path
from PIL import Image
sys.path.insert(0,str(Path(__file__).resolve().parent))
import reference_triage as qa
def png(color,size):
    b=io.BytesIO();Image.new('RGB',size,color).save(b,'PNG');return b.getvalue()
def write_zip(path,data):
    with zipfile.ZipFile(path,'w') as z:
        for key,value in data.items():z.writestr(key,value)
class Tests(unittest.TestCase):
    def test_approved_reference_identity(self):
        self.assertEqual(qa.APPROVED['training']['sha256'][:12],'a110ba0ffd2a')
    def test_identical_pixels(self):
        im=Image.new('RGB',(20,20),'green')
        self.assertTrue(qa.compare(im,im)['identical_pixels'])
    def test_major_visual_change(self):
        a=Image.new('RGB',(20,20),'white');b=Image.new('RGB',(20,20),'black')
        self.assertGreater(qa.compare(a,b)['changed_pixels_pct'],90)
    def test_mismatched_geometry_fails(self):
        with self.assertRaises(ValueError):qa.compare(Image.new('RGB',(10,10)),Image.new('RGB',(11,10)))
    def test_same_sha_fails(self):
        with tempfile.TemporaryDirectory() as t:
            with self.assertRaises(ValueError):qa.run('','','',Path(t),'a'*40,'a'*40)
    def test_absent_reference_fails(self):
        with tempfile.TemporaryDirectory() as t:
            f=Path(t)/'reference.zip'
            write_zip(f,{'random.png':png('green',(20,20))})
            with zipfile.ZipFile(f) as z:
                with self.assertRaises(ValueError):qa.item(z,qa.APPROVED['march']['name'])
    def test_reference_must_match_original_hash(self):
        with tempfile.TemporaryDirectory() as t:
            p=Path(t); data={x['capture']:png('black',x['size']) for x in qa.APPROVED.values()}
            write_zip(p/'before.zip',data);write_zip(p/'after.zip',data)
            write_zip(p/'reference.zip',{x['name']:png('black',x['size']) for x in qa.APPROVED.values()})
            with self.assertRaises(ValueError):qa.run(p/'before.zip',p/'after.zip',p/'reference.zip',p/'out','a'*40,'b'*40)
if __name__=='__main__':unittest.main()
