#!/usr/bin/env python3
"""Quantitative *rejection* gate for identical-camera Blender art proofs.

A high changed-pixel fraction is NOT ART APPROVED: only demonstrates enough
camera-visible change to merit independent Unity comparison.
"""
import math

def visible_change(before, after, width, height, threshold=0.012, min_visible_fraction=0.001):
    if not isinstance(width, int) or not isinstance(height, int) or width<8 or height<8:
        raise ValueError('Both rendered frames must have a valid nontrivial resolution')
    if len(before)!=len(after) or len(before)!=width*height*4:
        raise ValueError('Images must have EXACT same camera resolution and RGBA layout')
    if not (0 < threshold < 1) or not (0 < min_visible_fraction < 1):
        raise ValueError('Invalid camera visibility rejection thresholds')
    changed=0
    for i in range(0,len(before),4):
        if any(abs(before[i+j]-after[i+j])>threshold for j in range(3)):
            changed+=1
    total=width*height
    fraction=changed/total
    return {'width':width,'height':height,'total_pixels':total,
            'changed_pixels':changed,'changed_fraction':round(fraction,8),
            'channel_difference_threshold':threshold,
            'required_changed_fraction':min_visible_fraction,
            'camera_visibility_gate':'REVIEW_REQUIRED' if fraction>=min_visible_fraction else 'REJECT_NOT_VISIBLE',
            'note':'Even REVIEW_REQUIRED is NOT an artistic or Unity visual PASS'}
