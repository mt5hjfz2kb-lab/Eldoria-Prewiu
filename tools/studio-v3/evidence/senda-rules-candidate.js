/* La Senda de la Brecha — standalone deterministic event rules.
 * Zero effect on Eldoria's canonical combat, currencies, troop roster or saves.
 * Browser global + CommonJS for repeatable Node tests, no dependencies. */
(function (root, factory) {
  const api = factory();
  if (typeof module === 'object' && module.exports) module.exports = api;
  root.SendaRules = api;
})(typeof globalThis !== 'undefined' ? globalThis : this, function () {
  'use strict';
  const TYPES = Object.freeze({
    recruit: 'REFUERZOS',
    power: 'FURIA',
    ward: 'PROTECCIÓN',
    sabotage: 'EMBOSCADA'
  });
  const WAVES = Object.freeze([
    { name: 'EL UMBRAL', enemy: 14, strength: 1.00, boss: false, gates: [
      {type:'recruit',value:8,title:'+8',hint:'soldados'},
      {type:'power',value:22,title:'+22 %',hint:'ataque'},
      {type:'ward',value:6,title:'+6',hint:'escudos'} ] },
    { name: 'BOSQUE MARCHITO', enemy: 21, strength: 1.14, boss: false, gates: [
      {type:'sabotage',value:9,title:'−9',hint:'enemigos'},
      {type:'recruit',value:7,title:'+7',hint:'soldados'},
      {type:'ward',value:8,title:'+8',hint:'escudos'} ] },
    { name: 'EL PUENTE ROTO', enemy: 26, strength: 1.22, boss: false, gates: [
      {type:'power',value:26,title:'+26 %',hint:'ataque'},
      {type:'recruit',value:8,title:'+8',hint:'soldados'},
      {type:'ward',value:9,title:'+9',hint:'escudos'} ] },
    { name: 'LAS RUINAS', enemy: 31, strength: 1.32, boss: false, gates: [
      {type:'sabotage',value:12,title:'−12',hint:'enemigos'},
      {type:'ward',value:11,title:'+11',hint:'escudos'},
      {type:'power',value:30,title:'+30 %',hint:'ataque'} ] },
    { name: 'ANTES DE LA GRIETA', enemy: 39, strength: 1.45, boss: false, gates: [
      {type:'recruit',value:12,title:'+12',hint:'soldados'},
      {type:'ward',value:13,title:'+13',hint:'escudos'},
      {type:'power',value:34,title:'+34 %',hint:'ataque'} ] },
    { name: 'GUARDIÁN DE LA BRECHA', enemy: 53, strength: 1.64, boss: true, gates: [
      {type:'recruit',value:14,title:'+14',hint:'soldados'},
      {type:'power',value:40,title:'+40 %',hint:'ataque'},
      {type:'ward',value:17,title:'+17',hint:'escudos'} ] }
  ]);
  function initialState() {
    return {stage:0, units:22, attackBonus:0, shields:0, totalLost:0,
      totalRecruited:0, score:0, finished:false, victory:false, history:[]};
  }
  function checkedState(source) {
    if (!source || !Number.isInteger(source.stage) || source.stage < 0 || source.stage >= WAVES.length || source.finished) {
      throw new Error('La partida no admite otro encuentro');
    }
    if (!Number.isFinite(source.units) || source.units <= 0 || !Number.isFinite(source.attackBonus) || !Number.isFinite(source.shields)) {
      throw new Error('Estado de la expedición inválido');
    }
    if (!Number.isSafeInteger(source.units) || source.units > 96 || !Number.isSafeInteger(source.attackBonus) || source.attackBonus < 0 || source.attackBonus > 125 || !Number.isSafeInteger(source.shields) || source.shields < 0 || source.shields > 50 || !Number.isSafeInteger(source.totalLost) || source.totalLost < 0 || !Number.isSafeInteger(source.totalRecruited) || source.totalRecruited < 0 || !Number.isSafeInteger(source.score) || source.score < 0 || !Array.isArray(source.history) || source.history.length !== source.stage || source.finished !== false || source.victory !== false) throw new Error('Estado de la expedición inválido');
  }
  function playWave(state, lane) {
    checkedState(state);
    if (!Number.isInteger(lane) || lane < 0 || lane > 2) throw new RangeError('Carril inválido');
    const wave = WAVES[state.stage];
    const gate = wave.gates[lane];
    const next = {...state, history:state.history.slice()};
    const priorUnits = next.units;
    let enemies = wave.enemy;
    switch (gate.type) {
      case 'recruit': next.units = Math.min(96, next.units + gate.value); next.totalRecruited += next.units-priorUnits; break;
      case 'power': next.attackBonus = Math.min(125, next.attackBonus + gate.value); break;
      case 'ward': next.shields = Math.min(50, next.shields + gate.value); break;
      case 'sabotage': enemies = Math.max(1, enemies - gate.value); break;
      default: throw new Error('Puerta desconocida');
    }
    const attack = next.units * (1 + next.attackBonus/100);
    const resistance = enemies * wave.strength;
    const insufficientForce = attack < resistance * (wave.boss ? 0.42 : 0.31);
    const baseLoss = Math.max(1, Math.ceil(
      enemies * wave.strength * (wave.boss ? 0.47 : 0.33) /
      (1 + attack / 45)
    ));
    const penalty = insufficientForce ? Math.ceil((resistance*(wave.boss?0.42:0.31)-attack)/2.4) : 0;
    const incoming = baseLoss + Math.max(0, penalty);
    const absorbed = Math.min(incoming, next.shields);
    next.shields -= absorbed;
    const lost = Math.min(next.units, incoming - absorbed);
    next.units -= lost;
    next.totalLost += lost;
    const survived = next.units > 0 && !insufficientForce;
    next.score += survived ? (95 + state.stage*35 + Math.min(30, next.units)) : 15;
    const report = {stage:state.stage+1, zone:wave.name, lane, gate:gate.type,
      gateValue:gate.value, enemies, enemyInitial:wave.enemy, enemyStrength:wave.strength,
      before:priorUnits, after:next.units, lost, absorbed, incoming,
      attackBonus:next.attackBonus, remainingShields:next.shields, survived, boss:wave.boss};
    next.history.push(report);
    next.stage += 1;
    if (!survived || next.stage === WAVES.length) {
      next.finished = true;
      next.victory = survived && next.stage === WAVES.length;
      if (next.victory) next.score += 380 + next.units*10 + next.shields*3;
    }
    return {state:next,report};
  }
  function summary(state) {
    return {victory:state.victory,wavesCleared:state.history.filter(r=>r.survived).length,
      units:state.units,lost:state.totalLost,recruited:state.totalRecruited,score:state.score,
      grade:state.victory ? (state.units>=35?'S':state.units>=20?'A':'B') : '—'};
  }
  return Object.freeze({TYPES,WAVES,initialState,playWave,summary});
});
