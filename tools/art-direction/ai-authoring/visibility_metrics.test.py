#!/usr/bin/env python3
import unittest
from visibility_metrics import visible_change

class VisibleChangeGateTests(unittest.TestCase):
 def blank(self):
  return [0.2,0.2,0.2,1.0]*(40*30)
 def test_identical_images_are_rejected(self):
  a=self.blank()
  v=visible_change(a,a,40,30)
  self.assertEqual(v['changed_pixels'],0)
  self.assertEqual(v['camera_visibility_gate'],'REJECT_NOT_VISIBLE')
 def test_tiny_corner_move_is_not_visual_improvement(self):
  a=self.blank();b=a.copy()
  for n in (0,5):
   b[n*4:n*4+3]=[0.5,0.5,0.5]
  v=visible_change(a,b,40,30)
  self.assertEqual(v['changed_pixels'],2)
  self.assertEqual(v['camera_visibility_gate'],'REVIEW_REQUIRED' if 2/1200>=.001 else 'REJECT_NOT_VISIBLE')
  strict=visible_change(a,b,40,30,min_visible_fraction=.01)
  self.assertEqual(strict['camera_visibility_gate'],'REJECT_NOT_VISIBLE')
 def test_large_visible_sculpt_can_proceed_to_independent_unity_review(self):
  a=self.blank();b=a.copy()
  for n in range(80):b[n*4:n*4+3]=[0.8,0.8,0.8]
  v=visible_change(a,b,40,30)
  self.assertEqual(v['changed_pixels'],80)
  self.assertEqual(v['camera_visibility_gate'],'REVIEW_REQUIRED')
  self.assertIn('NOT',v['note'])
 def test_noise_under_threshold_is_ignored(self):
  a=self.blank();b=[(x+0.002 if (i%4)<3 else x) for i,x in enumerate(a)]
  v=visible_change(a,b,40,30)
  self.assertEqual(v['changed_pixels'],0)
 def test_mismatched_frame_sizes_refused(self):
  with self.assertRaises(ValueError):visible_change(self.blank(),self.blank()[:-2],40,30)
 def test_small_dimensions_refused(self):
  with self.assertRaises(ValueError):visible_change([0.1]*16,[0.1]*16,2,2)

if __name__=='__main__':unittest.main()
