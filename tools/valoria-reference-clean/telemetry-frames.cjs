'use strict';
const PREFIX = 'VALORIA_PLAYTEST ';
const CHUNK = 'VALORIA_PLAYTEST_CHUNK ';
function createTelemetryReader() {
  let pending = null;
  return {
    reset() { pending = null; },
    accepts(text) { return text.includes(PREFIX) || text.includes(CHUNK); },
    read(text) {
      const at = text.indexOf(CHUNK);
      if (at < 0) {
        if (pending) throw Error('Incomplete telemetry frame before legacy frame');
        return JSON.parse(text.slice(text.indexOf(PREFIX) + PREFIX.length).split('\n')[0]);
      }
      const match = /^(\d+) (\d+) (\d+) ([A-Za-z0-9+/]+={0,2})$/.exec(text.slice(at + CHUNK.length).split('\n')[0]);
      if (!match) throw Error('Malformed telemetry chunk');
      const [, id, indexText, countText, data] = match;
      const index = Number(indexText), count = Number(countText);
      if (count < 1 || count > 512 || index >= count || data.length > 4000 ||
          (index < count - 1 && (data.length !== 4000 || data.includes('=')))) throw Error('Invalid telemetry chunk bounds');
      if (!pending) {
        if (index !== 0) throw Error('Telemetry frame missing first chunk');
        pending = { id, count, parts: [] };
      }
      if (pending.id !== id || pending.count !== count || pending.parts.length !== index)
        throw Error('Incomplete, duplicate or out-of-order telemetry frame');
      pending.parts.push(data);
      if (pending.parts.length !== count) return null;
      const encoded = pending.parts.join('');
      const bytes = Buffer.from(encoded, 'base64');
      if (bytes.toString('base64') !== encoded) throw Error('Noncanonical telemetry encoding');
      const json = new TextDecoder('utf-8', { fatal: true }).decode(bytes);
      const frame = JSON.parse(json);
      pending = null;
      return frame;
    }
  };
}
module.exports = { createTelemetryReader };
