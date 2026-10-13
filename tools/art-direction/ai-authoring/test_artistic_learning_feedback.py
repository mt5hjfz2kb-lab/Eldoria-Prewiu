#!/usr/bin/env python3
"""Zero-cost regression: original rejection evidence must change next decisions."""
import json
import base64
import hashlib
import pathlib
import tempfile
import unittest
from artistic_learning_feedback import (load_failed_cycles, feedback_for_failed_cycles,
                                       distinct_strategy_errors, select_journal_lessons,
                                       forbidden_methods_from_rejected_cycles)

TARGET = 'Original terrain'
SHA = 'a' * 64
def action(method='sculpt_region'):
    return {'id':'edit','type':method,'target':TARGET,'parameters':{
        'center':[0,-4,0],'radius':1,'delta':[0.2,0,0]}}

def review(changed=0, total=307200):
    return {'target': TARGET,'scope':'Blender clay A/B only, NOT Unity, NOT ART_APPROVED',
      'before_image_sha256':SHA,'after_image_sha256':'b'*64,
      'source_file_sha256':'c'*64,'result_file_sha256':'d'*64,
      'camera_visibility':{'camera_visibility_gate':'REJECT_NOT_VISIBLE',
        'changed_pixels':changed,'total_pixels':total,
        'changed_fraction':round(changed/total,8),'required_changed_fraction':.001}}

class LearningEvidenceTests(unittest.TestCase):
 def setUp(self):
    self.temp=tempfile.TemporaryDirectory()
    self.addCleanup(self.temp.cleanup)
    self.root=pathlib.Path(self.temp.name)
 def cycle(self, i, typ='sculpt_region', changed=0, forged=None):
    folder=self.root/str(i);folder.mkdir(exist_ok=True)
    p=folder/'artist-plan.json';r=folder/'comparison.json'
    p.write_text(json.dumps([action(typ)]),'utf8')
    png=base64.b64decode('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+/iFsAAAAASUVORK5CYII=')
    for name in ('before','after'):
        (folder/f'{name}-blender-clay.png').write_bytes(png)
    v=forged if forged is not None else review(changed)
    for name in ('before','after'):
        if f'{name}_image_sha256' in v:
            v[f'{name}_image_sha256']=hashlib.sha256(png).hexdigest()
    r.write_text(json.dumps(v),'utf8')
    return p,r
 def load(self, pairs):
    return load_failed_cycles([p for p,_ in pairs],[r for _,r in pairs],TARGET)
 def test_two_camera_invisible_sculpts_force_different_decision(self):
    f=self.load([self.cycle(1,changed=0),self.cycle(2,changed=29)])
    self.assertIn('TWO failed cycles', feedback_for_failed_cycles(f))
    self.assertIn('REPEATED_INVISIBLE_STRATEGY',
      '|'.join(distinct_strategy_errors({'planned_actions':[action()]},f)))
    self.assertFalse(distinct_strategy_errors({'planned_actions':[action('reprofile_crest')]},f))
    self.assertFalse(distinct_strategy_errors({'planned_actions':[action('sculpt_path')]},f))
 def test_schema_bans_only_twice_rejected_original_method(self):
    f=self.load([self.cycle(1,changed=0),self.cycle(2,changed=12)])
    self.assertEqual(forbidden_methods_from_rejected_cycles(f),{'sculpt_region'})
    from author_brief import cpu_artist_schema
    allowed=cpu_artist_schema(TARGET,'existing',2,forbidden_methods_from_rejected_cycles(f))['properties']['planned_actions']['items']['properties']['type']['enum']
    self.assertEqual(set(allowed),{'sculpt_path','reprofile_crest'})
    self.assertNotIn('sculpt_region',allowed)
    only_one=self.load([self.cycle(3)])
    self.assertEqual(forbidden_methods_from_rejected_cycles(only_one),set())
    different=self.load([self.cycle(4),self.cycle(5,typ='sculpt_path')])
    self.assertEqual(forbidden_methods_from_rejected_cycles(different),set())
 def test_one_failure_does_not_force_a_strategic_switch(self):
    f=self.load([self.cycle(1)])
    self.assertEqual(distinct_strategy_errors({'planned_actions':[action()]},f),[])
 def test_two_different_failed_methods_do_not_force_arbitrary_third_method(self):
    f=self.load([self.cycle(1),self.cycle(2,'sculpt_path')])
    self.assertEqual(distinct_strategy_errors({'planned_actions':[action()]},f),[])
 def test_reject_false_pass_or_unmatched_target(self):
    v=review();v['camera_visibility']['camera_visibility_gate']='REVIEW_REQUIRED'
    with self.assertRaises(ValueError):self.load([self.cycle(1,forged=v)])
    v=review();v['target']='unrelated'
    with self.assertRaises(ValueError):self.load([self.cycle(2,forged=v)])
 def test_reject_faked_pixel_fraction_and_missing_provenance(self):
    v=review(29);v['camera_visibility']['changed_fraction']=.8
    with self.assertRaises(ValueError):self.load([self.cycle(1,forged=v)])
    v=review();del v['before_image_sha256']
    with self.assertRaises(ValueError):self.load([self.cycle(2,forged=v)])
 def test_missing_or_tampered_original_png_fails_closed(self):
    p,r=self.cycle(1)
    (r.parent/'before-blender-clay.png').unlink()
    with self.assertRaises(ValueError):self.load([(p,r)])
    p,r=self.cycle(2)
    (r.parent/'after-blender-clay.png').write_bytes(b'not a PNG')
    with self.assertRaises(ValueError):self.load([(p,r)])
    p,r=self.cycle(3)
    (r.parent/'after-blender-clay.png').write_bytes((r.parent/'after-blender-clay.png').read_bytes()+b'changed')
    with self.assertRaises(ValueError):self.load([(p,r)])
 def test_no_plan_or_different_target_rejected(self):
    p,r=self.cycle(1);p.write_text(json.dumps([{'type':'sculpt_region','target':'another'}]),'utf8')
    with self.assertRaises(ValueError):self.load([(p,r)])
    with self.assertRaises(ValueError):
      load_failed_cycles([p],[],TARGET)
 def test_relevant_camera_failures_retrieved_from_long_journal(self):
    source='## Important real camera rejection\nART FAIL source cliff remains square.\n'+(
      '## Unrelated scheduling\n'+'no relevant evidence\n'*150)
    lesson=select_journal_lessons(source,900)
    self.assertIn('cliff remains square',lesson)
    self.assertNotIn('Unrelated scheduling',lesson)
if __name__=='__main__':unittest.main()
