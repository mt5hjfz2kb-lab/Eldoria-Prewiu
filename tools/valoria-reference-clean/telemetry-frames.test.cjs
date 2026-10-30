const test = require('node:test');
const assert = require('node:assert/strict');
const { createTelemetryReader } = require('./telemetry-frames.cjs');
function lines(value, id = 1) {
  const encoded = Buffer.from(JSON.stringify(value)).toString('base64');
  const parts = encoded.match(/.{1,4000}/g);
  return parts.map((part, i) => `VALORIA_PLAYTEST_CHUNK ${id} ${i} ${parts.length} ${part}`);
}
test('large current saves preserve all UTF-8 bytes across bounded native log lines', () => {
  const original = { save: JSON.stringify({ text: 'Bastión 🏰'.repeat(5000) }), ready: true };
  const reader = createTelemetryReader(), chunks = lines(original);
  assert.ok(chunks.length > 3);
  let actual;
  for (const [i, line] of chunks.entries()) {
    assert.ok(Buffer.byteLength(line) < 8192);
    actual = reader.read(line);
    if (i < chunks.length - 1) assert.equal(actual, null);
  }
  assert.deepEqual(actual, original);
});
test('missing, duplicate and reordered chunks fail instead of accepting partial saves', () => {
  const chunks = lines({ save: 'x'.repeat(9000) });
  for (const bad of [chunks[0], chunks[2], lines({ ready: true }, 2)[0]]) {
    const reader = createTelemetryReader(); reader.read(chunks[0]);
    assert.throws(() => reader.read(bad));
  }
  assert.throws(() => createTelemetryReader().read(chunks[1]));
});
test('truncation, malformed encoding and unbounded declarations fail', () => {
  const chunk = lines({ save: 'x'.repeat(9000) })[0];
  for (const line of [chunk.slice(0, -1), 'VALORIA_PLAYTEST_CHUNK 1 0 999999 e30=',
    'VALORIA_PLAYTEST_CHUNK 1 0 1 ====']) assert.throws(() => createTelemetryReader().read(line));
});
test('navigation reset discards stale partial frames and retains legacy compatibility', () => {
  const reader = createTelemetryReader(); reader.read(lines({ save: 'x'.repeat(9000) })[0]);
  reader.reset(); assert.deepEqual(reader.read(lines({ ready: true })[0]), { ready: true });
  assert.deepEqual(reader.read('VALORIA_PLAYTEST {"ready":true}\nstack'), { ready: true });
});
