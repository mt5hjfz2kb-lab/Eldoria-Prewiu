"""Eldoria V4.1 evolution (offline, deterministic, non-production).

This is the same V4.1 mathematical duel extended from 3x3 to 3x5, without
any visual combat or tier victory gates. Outcomes depend on damage and HP.
No state or resource awards are performed here. All combat events go to mail.
"""
from __future__ import annotations
from dataclasses import dataclass, field, asdict
from copy import deepcopy
from collections import Counter
from typing import Optional
import math
import numpy as np

RULES_VERSION = 'v4.1-evolution-candidate-t1-t5'
PRODUCTION_APPROVED = False
FAMILIES = ('Paladines','Arqueros','Brujos')
TIERS = np.array([1.,1.55,2.40,3.72,5.77],dtype=float)
BASE = np.array([[9.,16.,72.,3.],[13.,7.,42.,7.],[11.,8.,49.,17.]],dtype=float)
COUNTER = np.array([[0,1,-1],[-1,0,1],[1,-1,0]],dtype=int)
COUNTER_BONUS = 1.40
TACTICAL_BONUS = .65
GUARD = .24
GUARD_BUDGET = .35
WARLOCK_BYPASS = .75
EPS = 1e-9
MAX_ROUNDS = 120  # Calculation is instant, not a visual timeline.
HEROES = ('aldric','lyra') # No invented third hero.

@dataclass(frozen=True)
class Force:
    troops: tuple[tuple[int, ...], ...]
    heroes: tuple[Optional[str], Optional[str], Optional[str]] = ('aldric',None,None)
    def __post_init__(self):
        if len(self.troops)!=3 or any(len(x)!=5 for x in self.troops):
            raise ValueError('Exactly 3 families x 5 tiers are required')
        if any(not isinstance(n,int) or isinstance(n,bool) or n<0 or n>1_000_000 for row in self.troops for n in row):
            raise ValueError('Invalid troop count')
        if not any(n>0 for row in self.troops for n in row):
            raise ValueError('Army has no troops')
        if len(self.heroes)!=3 or self.heroes[0] not in HEROES:
            raise ValueError('A real available leader is required')
        supplied=[h for h in self.heroes if h is not None]
        if len(set(supplied))!=len(supplied) or any(h not in HEROES for h in supplied):
            raise ValueError('Unknown or duplicated hero')
    @staticmethod
    def one(family:int,tier:int,n:int,heroes=('aldric',None,None)) -> 'Force':
        if family not in range(3) or tier not in range(1,6):
            raise ValueError('Invalid family/tier')
        arr=[[0]*5 for _ in range(3)];arr[family][tier-1]=n
        return Force(tuple(tuple(r) for r in arr),tuple(heroes))
    def matrix(self) -> np.ndarray:
        return np.array(self.troops,dtype=np.int64)

@dataclass(frozen=True)
class RoundReport:
    number: int
    dealt_a: float
    dealt_b: float
    remaining_a: float
    remaining_b: float

@dataclass(frozen=True)
class BattleReport:
    rules_version: str
    outcome: str   # victory / defeat / draw, from real remaining HP
    rounds: int
    reason_code: str
    remaining_a: float
    remaining_b: float
    damage_by_family_a: tuple[float,float,float]
    damage_by_family_b: tuple[float,float,float]
    damaged_hp_a: tuple[tuple[float,...], ...] # 3x5, not invented kills
    damaged_hp_b: tuple[tuple[float,...], ...]
    blocked_by_aldric_a: float
    blocked_by_aldric_b: float
    first_strike_lyra_a: float
    first_strike_lyra_b: float
    rupture_effective: tuple[float,...]
    counter_advantage: float
    rounds_detail: tuple[RoundReport,...]
    def to_dict(self): return asdict(self)

class _Side:
    def __init__(self,f:Force):
        counts=f.matrix().reshape(-1).astype(float)
        scale=np.tile(TIERS,3)
        self.hp_per=np.repeat(BASE[:,2],5)*scale
        self.hp=counts*self.hp_per
        self.initial=self.hp.copy()
        self.atk=np.repeat(BASE[:,0],5)*scale
        self.df=np.repeat(BASE[:,1],5)*scale
        self.ru=np.repeat(BASE[:,3],5)*scale
        self.heroes=f.heroes
        if 'lyra' in f.heroes:self.atk[5:10]*=1.03
        if 'aldric' in f.heroes:self.df[:5]*=1.03
        self.dealt=np.zeros(3)
        self.blocked=0.
        self.first_strike=0.
    @property
    def remaining(self):return float(np.sum(self.hp))
    @property
    def initial_total(self):return float(np.sum(self.initial))

# Smooth diminishing returns, never a hard plateau at T3.
def rupture_fraction(rupture:np.ndarray|float):
    return .64 * np.asarray(rupture)/(np.asarray(rupture)+45.)

def _coherence(a:Force,b:Force) -> float:
    m=a.matrix().sum(axis=1).astype(float); n=b.matrix().sum(axis=1).astype(float)
    return float(np.sum((m/m.sum())[:,None]*COUNTER*(n/n.sum())[None,:]))

def _outgoing(source:_Side,target:_Side,tactical:float):
    enemy=target.hp/target.hp_per
    living=source.hp/source.hp_per
    factor=np.where(COUNTER[np.arange(15)[:,None]//5,np.arange(15)[None,:]//5]>0,COUNTER_BONUS,1.)
    weight=factor**.6*enemy[None,:]
    den=weight.sum(axis=1)
    mat=np.divide(living[:,None]*source.atk[:,None]*weight,den[:,None],out=np.zeros((15,15)),where=den[:,None]>EPS)
    # Actual Paladin HP caps the amount redirected; Warlock penetration remains.
    ph=float(target.hp[:5].sum())
    if ph>EPS:
        redirect=mat[:,5:]*GUARD*np.array([1.]*5+[1.-WARLOCK_BYPASS]*5)[None,:]
        total=float(redirect.sum())
        if total>EPS:
            move=redirect*min(1.,ph*GUARD_BUDGET/total)
            mat[:,5:]-=move
            mat[:,:5]+=move.sum(axis=1)[:,None]*target.hp[:5][None,:]/ph
    pierce=rupture_fraction(source.ru)
    mitigation=100./(100.+4*target.df[None,:]*(1-pierce[:,None]))
    return mat*factor*tactical*mitigation

def _apply(src:_Side,dst:_Side,m:np.ndarray):
    total=m.sum(axis=0)
    cap=np.divide(np.minimum(dst.hp,total),total,out=np.zeros(15),where=total>EPS)
    dealt=m*cap[None,:]
    src.dealt+=dealt.sum(axis=1).reshape(3,5).sum(axis=1)
    dst.hp=np.maximum(0,dst.hp-total)
    return float(dealt.sum())

def _lyra_boost(mat:np.ndarray,enemy_initial:float):
    raw=float(mat.sum()); gain=min(.025*enemy_initial,.25*raw)
    if raw>EPS:mat*=1+gain/raw
    return gain if raw>EPS else 0.

def _aldric_block(mat:np.ndarray,our_initial:float):
    raw=float(mat.sum());block=min(.45*raw,.028*our_initial)
    if raw>EPS:mat*=1-block/raw
    return block if raw>EPS else 0.

def resolve(a:Force,b:Force,max_rounds:int=MAX_ROUNDS,hero_skills:bool=True) -> BattleReport:
    if not 1<=max_rounds<=1000:raise ValueError('Invalid max_rounds')
    sa=_Side(a);sb=_Side(b);coherence=_coherence(a,b)
    trail=[]
    for round_no in range(1,max_rounds+1):
        if sa.remaining<=EPS or sb.remaining<=EPS:break
        ab=_outgoing(sa,sb,1+TACTICAL_BONUS*coherence)
        ba=_outgoing(sb,sa,1-TACTICAL_BONUS*coherence)
        if hero_skills and round_no==1:
            if 'lyra' in a.heroes:sa.first_strike=_lyra_boost(ab,sb.initial_total)
            if 'lyra' in b.heroes:sb.first_strike=_lyra_boost(ba,sa.initial_total)
            if 'aldric' in a.heroes:sa.blocked=_aldric_block(ba,sa.initial_total)
            if 'aldric' in b.heroes:sb.blocked=_aldric_block(ab,sb.initial_total)
        dealt_a=_apply(sa,sb,ab)
        dealt_b=_apply(sb,sa,ba)
        trail.append(RoundReport(round_no,dealt_a,dealt_b,sa.remaining,sb.remaining))
    ra=sa.remaining;rb=sb.remaining
    # No magical power thresholds or forced wins. The time-limit can be a draw.
    if rb<=EPS and ra>EPS:outcome='victory'
    elif ra<=EPS and rb>EPS:outcome='defeat'
    else:outcome='draw'
    reason='enemy_eliminated' if outcome=='victory' else ('army_eliminated' if outcome=='defeat' else ('mutual_elimination' if ra<=EPS and rb<=EPS else 'round_limit'))
    diff_a=(sa.initial-sa.hp).reshape(3,5)
    diff_b=(sb.initial-sb.hp).reshape(3,5)
    return BattleReport(RULES_VERSION,outcome,len(trail),reason,ra/sa.initial_total,rb/sb.initial_total,
        tuple(map(float,sa.dealt)),tuple(map(float,sb.dealt)),
        tuple(tuple(map(float,row)) for row in diff_a),tuple(tuple(map(float,row)) for row in diff_b),
        sa.blocked,sb.blocked,sa.first_strike,sb.first_strike,
        tuple(map(float,rupture_fraction(np.repeat(BASE[:,3],5)*np.tile(TIERS,3)))),coherence,tuple(trail))

def player_report(result: BattleReport, player: Optional[Force] = None, detail: str = "simple") -> dict:
    """V4.1-approved two-tier disclosure. No predicted win or tactical prescription.

    Result retains full simulation internals for QA, but inbox gets only facts
    observable about the PLAYER's force. Detailed own-family data are opt-in.
    """
    if detail not in ("simple", "advanced"):
        raise ValueError("Invalid report disclosure")
    summary = {
        "result": result.outcome, "rounds": result.rounds,
        "remaining_own_fraction": round(result.remaining_a, 4),
        "enemy_observed_remaining_fraction": round(result.remaining_b, 4),
        "heroes": [h for h in player.heroes if h] if player else [],
        "observations": []
    }
    if player is not None:
        summary["own_composition"] = {
            f"{FAMILIES[f]} T{t+1}": int(player.troops[f][t])
            for f in range(3) for t in range(5) if player.troops[f][t]
        }
    own_dmg = list(result.damage_by_family_a)
    if sum(own_dmg)>0.01:
        best=int(np.argmax(own_dmg))
        summary['observations'].append(
            f"La mayor contribución de daño observada fue de tus {FAMILIES[best]}.")
    if player is not None:
        lost_by_family = [sum(result.damaged_hp_a[f]) for f in range(3)]
        if sum(lost_by_family)>0.01:
            worst=int(np.argmax(lost_by_family))
            summary['observations'].append(
                f"Tus {FAMILIES[worst]} recibieron la mayor cantidad absoluta de daño.")
    if detail=='advanced':
        if player is None:
            raise ValueError('Own roster required for detailed own-family data')
        summary['my_own_damage_by_family'] = {
            FAMILIES[f]:round(float(result.damage_by_family_a[f]),2)
            for f in range(3) if sum(player.troops[f])>0
        }
        summary['my_own_damage_received_by_family'] = {
            FAMILIES[f]:round(float(sum(result.damaged_hp_a[f])),2)
            for f in range(3) if sum(player.troops[f])>0
        }
        summary['my_own_remaining_health_by_family'] = {}
        for f in range(3):
            initial = sum(player.troops[f][t] * BASE[f,2] * TIERS[t] for t in range(5))
            if initial>0:
                lost=sum(result.damaged_hp_a[f])
                summary['my_own_remaining_health_by_family'][FAMILIES[f]]=float(round(max(0.,1-lost/initial),4))
    return summary

@dataclass
class Mailbox:
    messages:dict[str,dict]=field(default_factory=dict)
    event_ids:set[str]=field(default_factory=set)
    def receive(self,event_id:str,report:BattleReport,target:str,occurred_ticks:int,
                receipt_id:str='',player:Optional[Force]=None) -> bool:
        if not event_id or not target or not isinstance(occurred_ticks,int):
            raise ValueError('Missing event info')
        if event_id in self.event_ids:return False
        self.event_ids.add(event_id)
        self.messages[event_id]={
            'event_id':event_id,'category':'informes','type':'battle',
            'title':{'victory':'Victoria','defeat':'Derrota','draw':'Empate'}[report.outcome],
            'target':target,'report_id':event_id,'receipt_id':receipt_id,
            'occurred_utc_ticks':occurred_ticks,
            'simple':player_report(report,player,'simple'),
            'advanced':player_report(report,player,'advanced') if player is not None else None
        }
        return True
    def read(self,event_id):return deepcopy(self.messages[event_id])
    def delete(self,event_id):self.messages.pop(event_id,None)
    def snapshot(self):
        return {'schema_version':1,'messages':deepcopy(self.messages),
                'received_event_ids':sorted(self.event_ids)}
    @classmethod
    def restore(cls,stored):
        state=cls()
        state.messages=deepcopy(stored.get('messages',{}))
        state.event_ids=set(stored.get('received_event_ids',[]))
        return state
