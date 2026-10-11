'use strict';

// QA contract only; does not mutate the game or certify artistic quality.
const PERSISTED_FIELDS = Object.freeze([
  'revision', 'wood', 'stone', 'gatheredWood', 'gatheredStone', 'sawmill', 'bastion', 'march'
]);

function fatalRuntimeLogs(logs) {
  return logs.filter(entry => {
    const type = String(entry.type || '').toLowerCase();
    const text = String(entry.text || '');
    return ['error', 'pageerror', 'dialog'].includes(type) ||
      /is corrupted!|Position out of bounds!|\bBUILD INVALID\b|\bVALORIA_[A-Z_]*FAIL\b/i.test(text);
  });
}

function assertRuntimeClean(logs) {
  const failures = fatalRuntimeLogs(logs);
  if (failures.length) throw new Error('Runtime errors prevent certification: ' +
    failures.map(entry => String(entry.text || '').trim()).join(' | '));
}

function persistenceSnapshot(state) {
  const result = {};
  for (const field of PERSISTED_FIELDS) {
    if (!Object.prototype.hasOwnProperty.call(state, field))
      throw new Error('Missing persisted-state field: ' + field);
    result[field] = state[field];
  }
  return result;
}

function assertPersistedState(before, after) {
  const expected = persistenceSnapshot(before);
  const actual = persistenceSnapshot(after);
  for (const field of PERSISTED_FIELDS) {
    if (actual[field] !== expected[field])
      throw new Error('Persistence mismatch for ' + field + ': ' + expected[field] + ' -> ' + actual[field]);
  }
}

function controlPoint(ui, box, canvasInfo) {
  if (!ui || !Number.isFinite(ui.x) || !Number.isFinite(ui.unityY))
    throw new Error('Named navigation control has no valid published position');
  if (!(canvasInfo.width > 0) || !(canvasInfo.height > 0))
    throw new Error('Canvas dimensions are invalid');
  const point = {
    x: box.x + ui.x / canvasInfo.width * box.width,
    y: box.y + (canvasInfo.height - ui.unityY) / canvasInfo.height * box.height
  };
  if (point.x < box.x || point.x > box.x + box.width ||
      point.y < box.y || point.y > box.y + box.height)
    throw new Error('Named navigation control is outside the visible canvas');
  return point;
}

function certifyMacroloop(report) {
  assertRuntimeClean(report.logs);
  assertPersistedState(report.checks.beforeReload, report.checks.afterReload);
  report.checks.navigationGatherPersistencePass = true;
  report.coverage = {
    platform: 'Chromium mobile emulation with software rendering; not physical Safari/iPhone',
    executed: ['startup', 'landscape touch pan and HOME', 'named MUNDO navigation',
      'forest gather and reward', 'named CIUDAD return', 'exact reload persistence', 'portrait screenshot'],
    notExecuted: ['building construction', 'Bastion I-to-II progression', 'barracks',
      'recruitment', 'march preparation', 'combat', 'owner reset', 'portrait gameplay',
      'physical-device performance', 'human usability and enjoyment'],
    fullBastionIToIIPass: false,
    visualQualityPass: false,
    independentHumanPlaythroughPass: false
  };
}

module.exports = { PERSISTED_FIELDS, fatalRuntimeLogs, assertRuntimeClean,
  persistenceSnapshot, assertPersistedState, controlPoint, certifyMacroloop };
