#!/usr/bin/env python3
"""Bounded artistic feedback from real rejected Blender A/B cycles.

This is evidence retrieval and preflight, NOT model fine-tuning or art approval.
Never substitutes invented coordinates, amplitudes, or render verdicts.
"""
import hashlib
import json
import pathlib
import re

MAX_CASES = 4
MAX_FILE_BYTES = 24000
STRUCTURAL = {'sculpt_region', 'sculpt_path', 'reprofile_crest', 'bridge_boundary_loops', 'construct_mesh'}


def sha256(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def _read_json(path):
    path = pathlib.Path(path)
    if not path.is_file() or path.stat().st_size > MAX_FILE_BYTES:
        raise ValueError('Rejected-cycle evidence missing or too large: ' + str(path))
    return json.loads(path.read_text('utf8'))


def load_failed_cycles(plan_paths, review_paths, target):
    """Read paired *original* Qwen plan + camera-locked Blender rejection.

    A pair is usable only if its target, real image hashes and rejection metric
    match the authentic Blender comparison contract. It is never an ART_PASS.
    """
    if len(plan_paths) != len(review_paths) or len(plan_paths) > MAX_CASES:
        raise ValueError('Provide at most four matched --failed-plan/--failed-review pairs')
    failures = []
    for plan_path, review_path in zip(plan_paths, review_paths):
        plan = _read_json(plan_path)
        review = _read_json(review_path)
        if not isinstance(plan, list) or not 1 <= len(plan) <= 8:
            raise ValueError('Failed-cycle artist plan must be the original action array')
        if any(not isinstance(op, dict) or op.get('target') != target or
               op.get('type') not in STRUCTURAL | {'assign_material', 'place_existing_asset'}
               for op in plan):
            raise ValueError('Rejected plan refers to another object or unsupported operation')
        if not isinstance(review, dict) or review.get('target') != target or (
            'Blender clay A/B' not in str(review.get('scope', ''))
        ):
            raise ValueError('Rejected review must be original Blender clay A/B for same target')
        impact = review.get('camera_visibility', {})
        if not isinstance(impact, dict) or impact.get('camera_visibility_gate') != 'REJECT_NOT_VISIBLE':
            raise ValueError('Only an independently measured VISUAL REJECTION may teach this failure gate')
        count, total, fraction = impact.get('changed_pixels'), impact.get('total_pixels'), impact.get('changed_fraction')
        if (not isinstance(count, int) or isinstance(count, bool) or
            not isinstance(total, int) or isinstance(total, bool) or total <= 0 or
            count < 0 or count >= total or not isinstance(fraction, (float, int)) or
            isinstance(fraction, bool) or abs(count / total - fraction) > 0.0000001 or
            not 0 <= fraction < impact.get('required_changed_fraction', 0.001)):
            raise ValueError('Rejected-cycle measured pixels are inconsistent')
        for name in ('before_image_sha256', 'after_image_sha256', 'source_file_sha256', 'result_file_sha256'):
            if not re.fullmatch('[0-9a-f]{64}', str(review.get(name, ''))):
                raise ValueError('Rejected-cycle original image/source SHA missing: ' + name)
        # Verify genuine archived PNG bytes; a JSON manifest with hash-shaped
        # strings alone is insufficient evidence of a measured visual failure.
        for label in ('before', 'after'):
            png = pathlib.Path(review_path).parent / f'{label}-blender-clay.png'
            if not png.is_file() or png.stat().st_size > 10_000_000:
                raise ValueError('Original rejected Blender PNG missing or too large: ' + str(png))
            raw = png.read_bytes()
            if not raw.startswith(b'\x89PNG\r\n\x1a\n'):
                raise ValueError('Rejected-cycle image is not a genuine PNG file: ' + str(png))
            if hashlib.sha256(raw).hexdigest() != review[f'{label}_image_sha256']:
                raise ValueError('Rejected-cycle PNG does not match manifest hash: ' + str(png))
        methods = sorted({op['type'] for op in plan if op['type'] in STRUCTURAL})
        failures.append({
            'methods': methods, 'changed_pixels': count, 'total_pixels': total,
            'plan_sha256': sha256(pathlib.Path(plan_path)),
            'review_sha256': sha256(pathlib.Path(review_path)),
            'camera_gate': impact['camera_visibility_gate'],
        })
    return failures


def feedback_for_failed_cycles(failures):
    if not failures:
        return ''
    lines = ['ORIGINAL CAMERA-LOCKED REJECTED CYCLES (not simulation, not training):']
    for i, failed in enumerate(failures, 1):
        lines.append(
            f"Cycle {i}: Blender operations={','.join(failed['methods'])}; "
            f"camera pixels changed={failed['changed_pixels']}/{failed['total_pixels']}; "
            f"result=REJECT_NOT_VISIBLE; plan_sha256={failed['plan_sha256']}.")
    if len(failures) >= 2:
        previous = failures[-2:]
        methods = [x['methods'] for x in previous]
        if len(methods[0]) == len(methods[1]) == 1 and methods[0] == methods[1]:
            lines.append('TWO failed cycles used the SAME operation type: ' + methods[0][0] +
                         '. You MUST select a DIFFERENT geometry operation family and '
                         'a justified structural/camera-visible treatment, not a new tiny radius/delta.')
        else:
            lines.append('TWO independent revisions failed: reconsider the root cause and select '
                         'a meaningfully different silhouette, contact or composition treatment.')
    return ' '.join(lines)



def forbidden_methods_from_rejected_cycles(failures):
    """Remove only an operation family rejected twice in verified camera A/B.

    This constrains available *types*, not Qwen-authored geometry or values.
    """
    if not isinstance(failures, list) or len(failures)<2:
        return set()
    prev=failures[-2:]
    a,b=(x.get('methods') for x in prev)
    if isinstance(a,list) and len(a)==1 and a==b and a[0] in STRUCTURAL:
        return {a[0]}
    return set()


def distinct_strategy_errors(design, failures):
    """Block a third identical, camera-invisible method, never create a fake plan."""
    if not isinstance(design, dict):
        return []
    forbidden = forbidden_methods_from_rejected_cycles(failures)
    if not forbidden:
        return []
    repeated = next(iter(forbidden))
    actions = design.get('planned_actions')
    if not isinstance(actions, list):
        return []
    structural = {op.get('type') for op in actions if isinstance(op, dict)
                  and op.get('type') in STRUCTURAL}
    if structural == {repeated}:
        return ['REPEATED_INVISIBLE_STRATEGY: two camera-locked failures of ' + repeated +
                ' require the model to author a DISTINCT structural method. '
                'Change topology/silhouette/contact strategy, not only radius/delta.']
    return []


def select_journal_lessons(text, max_chars=1800):
    """Take recent empirical art failures, not arbitrarily the last file bytes."""
    if not isinstance(text, str) or not 300 <= max_chars <= 5000:
        raise ValueError('Journal lesson bounds invalid')
    sections = re.split(r'(?=^##?\s)', text, flags=re.MULTILINE)
    selected = []
    for section in reversed(sections):
        if any(key in section.upper() for key in
               ('ART FAIL', 'VISUAL FAIL', 'VISUAL REJECT', 'REJECT_NOT_VISIBLE', 'ART PENDING')):
            cleaned = ' '.join(section.strip().split())
            selected.append(cleaned[:min(780, max_chars)])
        if len(selected) >= 3:
            break
    return '\n'.join(selected)[:max_chars] if selected else text[-max_chars:]
