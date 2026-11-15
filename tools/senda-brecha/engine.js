/* La Senda de la Brecha — autonomous, deterministic event simulation.
 * No game-state writes, payments or permanent troop losses.
 * Production T1–T5 resolver is deliberately NOT replaced by this arcade model.
 */
(function (root, factory) {
  const api = factory();
  if (typeof module === 'object' && module.exports) module.exports = api;
  if (root) root.SendaBrecha = api;
})(typeof globalThis !== 'undefined' ? globalThis : this, function () {
  'use strict';
  const ROUNDS = Object.freeze([
    {
      name: 'El bosque silencioso', enemyName: 'Acechadores de la Brecha', enemyKind: 'stalkers',
      enemyHP: 330, enemyDPS: 12,
      gates: [
        { icon: '⚔', name: 'Guardia de Valoria', detail: '+12 INFANTERÍA', key: 'vanguard', amount: 12 },
        { icon: '➶', name: 'Arqueros errantes', detail: '+9 ARQUEROS', key: 'archer', amount: 9 }
      ]
    },
    {
      name: 'Las ruinas del juramento', enemyName: 'Guardianes corruptos', enemyKind: 'guards',
      enemyHP: 670, enemyDPS: 16,
      gates: [
        { icon: '✦', name: 'Forja de campaña', detail: '+40 % ATAQUE', key: 'attack', amount: 0.4 },
        { icon: '⬡', name: 'Escudos consagrados', detail: '−40 % DAÑO', key: 'guard', amount: 0.4 }
      ]
    },
    {
      name: 'El umbral violeta', enemyName: 'Guardián de la Brecha', enemyKind: 'boss',
      enemyHP: 650, enemyDPS: 15,
      gates: [
        { icon: '✚', name: 'Último destacamento', detail: '+16 INFANTERÍA', key: 'vanguard', amount: 16 },
        { icon: '➶', name: 'Flechas rúnicas', detail: '+16 ARQUEROS', key: 'archer', amount: 16 }
      ]
    }
  ]);
  const MAX_STEP = 0.05;
  const GATE_SECONDS = 6.0;
  const UNIT_HP = Object.freeze({ vanguard: 12, archer: 7 });
  function finite(n) { return typeof n === 'number' && Number.isFinite(n); }
  class EventRun {
    constructor() { this.reset(); }
    reset() {
      this.phase = 'intro'; this.roundIndex = 0; this.lane = 0;
      this.time = 0; this.phaseTime = 0;
      this.vanguard = 16; this.archer = 6; this.totalLost = 0;
      this.attackBuff = 1; this.damageReduction = 0; this.wound = 0;
      this.enemyHP = 0; this.enemyMaxHP = 0; this.enemyDPS = 0;
      this.chosen = []; this.events = []; this.damageDealt = 0;
      this.rewards = null; this.reason = null;
      return this;
    }
    start() {
      if (this.phase !== 'intro') return false;
      this.phase = 'choice'; this.phaseTime = 0;
      this.events.push('El destacamento entra en el bosque.');
      return true;
    }
    get round() { return ROUNDS[Math.min(this.roundIndex, ROUNDS.length - 1)]; }
    get soldiers() { return this.vanguard + this.archer; }
    get attackDPS() { return (this.vanguard * 1.45 + this.archer * 2.55) * this.attackBuff; }
    get choiceLeft() { return Math.max(0, GATE_SECONDS - this.phaseTime); }
    get enemyFraction() { return this.enemyMaxHP > 0 ? Math.max(0, this.enemyHP / this.enemyMaxHP) : 1; }
    select(side) {
      if (this.phase !== 'choice' || (side !== 0 && side !== 1)) return false;
      this.lane = side;
      return true;
    }
    advance(dt) {
      if (this.phase !== 'choice' && this.phase !== 'combat') return;
      if (!finite(dt) || dt <= 0) return;
      // The fixed-step cap prevents background-tab jumps from skipping encounters.
      let left = Math.min(dt, 0.25);
      while (left > 0.00000001 && (this.phase === 'choice' || this.phase === 'combat')) {
        const step = Math.min(left, MAX_STEP);
        left -= step; this.time += step; this.phaseTime += step;
        if (this.phase === 'choice' && this.phaseTime >= GATE_SECONDS) this.resolveGate();
        else if (this.phase === 'combat') this.fight(step);
      }
    }
    resolveGate() {
      if (this.phase !== 'choice') return false;
      const gate = this.round.gates[this.lane];
      if (gate.key === 'vanguard') this.vanguard += gate.amount;
      if (gate.key === 'archer') this.archer += gate.amount;
      if (gate.key === 'attack') this.attackBuff += gate.amount;
      if (gate.key === 'guard') this.damageReduction = Math.min(0.65, this.damageReduction + gate.amount);
      this.chosen.push({ round: this.roundIndex + 1, side: this.lane, label: gate.name });
      this.events.push(gate.name + ': ' + gate.detail);
      this.phase = 'combat'; this.phaseTime = 0;
      this.enemyHP = this.round.enemyHP;
      this.enemyMaxHP = this.round.enemyHP;
      this.enemyDPS = this.round.enemyDPS;
      return true;
    }
    fight(dt) {
      if (this.phase !== 'combat') return;
      const dealt = this.attackDPS * dt;
      const damageToEnemy = Math.min(this.enemyHP, dealt);
      this.enemyHP = Math.max(0, this.enemyHP - dealt);
      this.damageDealt += damageToEnemy;
      // Enemy attacks simultaneously; winning squad takes the final blow too.
      this.suffer(this.enemyDPS * dt * (1 - this.damageReduction));
      if (this.soldiers <= 0) {
        this.phase = 'defeat'; this.rewards = null;
        this.reason = 'El destacamento cayó ante ' + this.round.enemyName + '.';
        this.events.push(this.reason); return;
      }
      if (this.enemyHP <= 0) {
        this.events.push(this.round.enemyName + ' derrotados.');
        if (this.roundIndex >= ROUNDS.length - 1) {
          this.phase = 'victory'; this.reason = 'Valoria ha recuperado el sendero.';
          // Rewards are VISUAL ONLY, never stored in a user's game/account.
          this.rewards = { fragments: 45, seal: 1 };
        } else {
          this.roundIndex += 1; this.phase = 'choice'; this.phaseTime = 0;
          this.enemyHP = 0; this.enemyMaxHP = 0; this.lane = 0;
        }
      }
    }
    suffer(damage) {
      this.wound += damage;
      while (this.soldiers > 0) {
        const type = this.vanguard > 0 ? 'vanguard' : 'archer';
        const hp = UNIT_HP[type];
        if (this.wound + 1e-9 < hp) break;
        this.wound -= hp;
        this[type] -= 1; this.totalLost += 1;
      }
      if (this.soldiers === 0) this.wound = 0;
    }
    drainEvents() { const events = this.events.slice(); this.events.length = 0; return events; }
    snapshot() {
      return {
        phase: this.phase, stage: this.roundIndex + 1, lane: this.lane,
        soldiers: this.soldiers, infantry: this.vanguard, archers: this.archer,
        attackDPS: this.attackDPS, attackBuff: this.attackBuff,
        damageReduction: this.damageReduction, lost: this.totalLost,
        time: this.time, choiceLeft: this.choiceLeft,
        enemyName: this.round.enemyName, enemyHP: this.enemyHP,
        enemyFraction: this.enemyFraction, location: this.round.name,
        gates: this.round.gates.map(g => ({ ...g })),
        selected: this.chosen.map(g => ({ ...g })),
        rewards: this.rewards ? { ...this.rewards } : null,
        reason: this.reason
      };
    }
  }
  return Object.freeze({ EventRun, ROUNDS, GATE_SECONDS });
});
