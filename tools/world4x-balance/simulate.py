"""Offline design experiment. Never imports Unity, writes saves, or generates runtime worlds."""
import argparse, collections, hashlib, json, math, pathlib, random, re

ROOT = pathlib.Path(__file__).resolve().parents[2]
PROFILE = ROOT / 'Unity/Assets/Eldoria/Scripts/Domain/SliceContentProfiles.cs'

def economy():
    text = PROFILE.read_text()
    block = text.split('public static class OwnerIiiCandidate')[1].split('/// <summary>')[0]
    values = {k: int(v) for k,v in re.findall(r'public const int (\w+) = (\d+);', block)}
    return values, hashlib.sha256(text.encode()).hexdigest()

def progression_ledger(values):
    """Economic feasibility of the existing winning I-II path, not combat QA."""
    source=(ROOT/'Unity/Assets/Eldoria/Scripts/Application/LocalGateway.cs').read_text()
    grants={}
    for name,arguments in re.findall(r'Grant\("([^"]+)",([^;]+)\);',source):
        grants[name]={k:int(v) for k,v in re.findall(r'(wood|stone):\s*(\d+)',arguments)}
    wallet={'wood':values['InitialWood'],'stone':values['InitialStone']}; ledger=[]
    def step(name,wood=0,stone=0,reward=None):
        wallet['wood']+=wood;wallet['stone']+=stone
        if min(wallet.values())<0:raise AssertionError('Unaffordable progression: '+name)
        if reward:
            for k,v in grants[reward].items():wallet[k]+=v
        ledger.append({'step':name,**wallet})
    # Combat payouts are independently asserted against source by unit tests.
    step('build_sawmill',-values['SawmillWoodCost'],reward='c1-sawmill')
    step('gather_wood',math.ceil(values['Chapter1GatherWood']/values['ForestLoad'])*values['ForestLoad'],reward='c1-wood')
    step('gather_stone',stone=math.ceil(values['Chapter1GatherStone']/values['QuarryLoad'])*values['QuarryLoad'],reward='c1-stone')
    step('win_scout_and_return',80,70,reward='c1-route')
    step('bastion_two',-values['Bastion2WoodCost'],-values['Bastion2StoneCost'],reward='chapter-1')
    step('barracks',-values['BarracksWoodCost'],-values['BarracksStoneCost'],reward='c2-barracks')
    web=PROFILE.read_text().split('public static class WebContract')[1]
    costs={k:int(v) for k,v in re.findall(r'public const int (Recruit\w+PerArcher) = (\d+);',web)}
    count=values['Chapter2TrainArchers']
    step('train_twenty',-count*costs['RecruitWoodPerArcher'],-count*costs['RecruitStonePerArcher'],reward='c2-train')
    step('win_spawnling_and_return',120,100,reward='c2-spawnling')
    return {'assumption':'Canonical story encounters won; full state machine and combat remain Unity-tested',
            'source_sha256':hashlib.sha256(source.encode()).hexdigest(),'steps':ledger}

def distances(cells, start):
    if start not in cells: return {}
    d = {start: 0}; q = collections.deque([start])
    while q:
        x,y = q.popleft()
        for p in ((x-1,y),(x+1,y),(x,y-1),(x,y+1)):
            if p in cells and p not in d:
                d[p] = d[(x,y)] + 1; q.append(p)
    return d

def assignment(options, capacities):
    """Capacitated bipartite matching: no node stock is counted for two players."""
    slots = [(n,i) for n,c in enumerate(capacities) for i in range(c)]
    owners = {}
    def augment(player, seen):
        for node in options[player]:
            for slot in (s for s in slots if s[0] == node):
                if slot in seen: continue
                seen.add(slot)
                if slot not in owners or augment(owners[slot], seen):
                    owners[slot] = player; return True
        return False
    for player in range(len(options)): augment(player, set())
    return {player:slot[0] for slot,player in owners.items()}

def contest(occupant, challenger):
    """Exclusive occupant; abstract strength ordering, NOT the live combat formula.
    Tuples are (player_id, strength_rank). On ties this experiment keeps incumbent;
    tie policy is a modelling assumption, not an owner-approved gameplay rule.
    Collected cargo, battle casualties and march timing are deliberately untouched.
    """
    if occupant is None or challenger[1] > occupant[1]: return challenger
    return occupant

def nearest_contested(safe, starts, resources, levels):
    """Adversarial snapshot: stronger rivals occupy each spawn's nearest L1 option.
    Each occupied site has one rival. This does not assume unlimited marches for
    one player or invent a protection rule against winning stronger armies.
    """
    occupied={kind:{} for kind in resources}
    for start in starts:
        d=distances(safe,start)
        for kind,nodes in resources.items():
            eligible=[i for i,p in enumerate(nodes) if levels[kind][i]==1 and p in d]
            if eligible:
                i=min(eligible,key=lambda i:(d[nodes[i]],i))
                occupied[kind][i]=(f'rival-{kind}-{i}',2)
    return occupied

def world(seed, density, broken_bridge=False):
    rng = random.Random(seed); size=40
    # Explicit hypothetical geography: river, two crossing sites, rough ground,
    # and hazardous breach core. This is NOT a scan of actual Region 1 terrain.
    cells={(x,y) for x in range(size) for y in range(size)
           if (x!=20 or y in (8,31)) and rng.random()>.055}
    cells |= {(20,8),(20,31),(19,8),(21,8),(19,31),(21,31)}
    if broken_bridge: cells -= {(20,8),(20,31)}
    hazard={(x,y) for x,y in cells if (x-29)**2+(y-24)**2<25}
    safe=cells-hazard
    available=sorted(p for p in safe if 2<p[0]<37 and 2<p[1]<37)
    rng.shuffle(available); starts=[]
    for p in available:
        if all(abs(p[0]-q[0])+abs(p[1]-q[1])>=4 for q in starts): starts.append(p)
        if len(starts)==density: break
    # Intentionally unconstrained baseline catches clustering and one-bank bias.
    free=sorted(cells-set(starts)); rng.shuffle(free)
    resources={'wood':free[:18], 'stone':free[18:26]}
    enemies=free[26:42]
    return cells,safe,starts,resources,enemies

def measure(cells,safe,starts,resources,enemies,values,budget,levels=None,occupied=None):
    ds=[distances(safe,s) for s in starts]
    required={'wood':math.ceil(values['Chapter1GatherWood']/values['ForestLoad'])*values['ForestLoad'],
              'stone':math.ceil(values['Chapter1GatherStone']/values['QuarryLoad'])*values['QuarryLoad']}
    stocks={'wood':1250,'stone':5200} # LocalGateway canonical capacities, checked by tests.
    levels=levels or {kind:[1]*len(nodes) for kind,nodes in resources.items()}
    occupied=occupied or {kind:{} for kind in resources}
    matches={}; nearest={}; blocked=[]
    for kind,nodes in resources.items():
        # L1 is the comparison tier for initial progression, not a hard unlock
        # imposed on the game. Higher-tier yields/rates are not defined yet.
        options=[sorted((n for n,p in enumerate(nodes) if levels[kind][n]==1 and
                  d.get(p,math.inf)<=budget and contest(occupied[kind].get(n),(f'player-{i}',1))[0]==f'player-{i}'),
                  key=lambda n:d[nodes[n]]) for i,d in enumerate(ds)]
        # Stock can finance several visits, but ONE occupant only may gather at
        # a node at once. A quarry's seven reserves are NOT seven parallel slots.
        capacity=[min(1,stocks[kind]//required[kind])]*len(nodes)
        matches[kind]=assignment(options,capacity)
        # Once supply can cover everyone, minimize the largest one-way route.
        # This fixes a real weakness of capacity-only matching: reassignment can
        # push an earlier player to a distant node despite a local alternative.
        if len(matches[kind])==len(starts):
            low,high=0,budget
            while low<high:
                middle=(low+high)//2
                limited=[[n for n in opts if ds[i][nodes[n]]<=middle] for i,opts in enumerate(options)]
                if len(assignment(limited,capacity))==len(starts):high=middle
                else:low=middle+1
            matches[kind]=assignment([[n for n in opts if ds[i][nodes[n]]<=low] for i,opts in enumerate(options)],capacity)
        nearest[kind]=[min((d.get(p,math.inf) for n,p in enumerate(nodes) if levels[kind][n]==1),default=math.inf) for d in ds]
    # A route to an appropriate enemy and an accessible expansion footprint are
    # separately checked; enemy positions here stand for a level-one family.
    for i,d in enumerate(ds):
        reasons=[]
        for kind in resources:
            if i not in matches[kind]: reasons.append(kind+'_level_slot_stock_route_or_stronger_occupant')
        if not any(d.get(p,math.inf)<=budget for p in enemies): reasons.append('safe_enemy_route')
        if sum(1 for p,v in d.items() if 1<=v<=4 and all(abs(p[0]-s[0])+abs(p[1]-s[1])>1 for s in starts))<1: reasons.append('expansion_space')
        if reasons: blocked.append({'spawn':i,'reasons':reasons})
    # Common admitted set is rechecked after independently matching each resource.
    admitted=[i for i in range(len(starts)) if all(i in matches[k] for k in resources) and not any(b['spawn']==i for b in blocked)]
    trip_counts={'wood':math.ceil(values['Chapter1GatherWood']/values['ForestLoad']), 'stone':math.ceil(values['Chapter1GatherStone']/values['QuarryLoad'])}
    routes=[]
    for i in admitted:
        routes.append(sum(2*trip_counts[k]*ds[i][resources[k][matches[k][i]]] for k in resources))
    return {'candidates':len(starts),'admitted':len(admitted),'rejected':blocked,
            'round_trip_grid_edges':routes,'unreachable_basic_resource_positions':sum(any(not math.isfinite(nearest[k][i]) for k in resources) for i in range(len(starts))),
            'required_stock_per_spawn':required,'stock_reservations':{k:{str(i):n for i,n in matches[k].items() if i in admitted} for k in resources}}

def repaired(cells,safe,starts,resources,enemies):
    """Hypothetical distribution repair on eligible ground, NOT a runtime edit.
    Provision each candidate with separate initial resource opportunities, avoiding
    city cells and other sites. If no such site exists, admission must reject it.
    """
    result={k:list(v) for k,v in resources.items()}; enemies=list(enemies)
    used=set(starts)|set(enemies)|set(sum(result.values(),[]))
    for s in starts:
        d=distances(safe,s)
        eligible=sorted((p for p in d if p not in used and 2<=d[p]<=4),key=lambda p:(d[p],p))
        # Two distinct initial options per resource allow one nearby node to be
        # contested; no immunity, phantom player or runtime generation is added.
        for kind in ('wood','stone','wood','stone','enemy'):
            if not eligible: break
            p=eligible.pop(0);used.add(p)
            if kind=='enemy':enemies.append(p)
            else:result[kind].append(p)
    return result,enemies

def run(seeds):
    values,sha=economy(); rows=[]
    # Budget sweep is an assumption, expressed in path edges, not a claimed live
    # production time. 8m cells reuse current route sampling; scale is tested.
    for seed in range(seeds):
        for density in (12,24,36):
            for broken in (False,True):
                c,s,sp,r,e=world(seed,density,broken); rr,ee=repaired(c,s,sp,r,e)
                tiers={k:[1+(i+seed)%3 for i in range(len(nodes))] for k,nodes in r.items()}
                repaired_tiers={k:tiers[k]+[1]*(len(rr[k])-len(r[k])) for k in r}
                blocked=nearest_contested(s,sp,r,tiers)
                repaired_blocked=nearest_contested(s,sp,rr,repaired_tiers)
                for budget in (8,16):
                    for mode,res,enemy,lev,occ in (
                        ('baseline',r,e,tiers,None),('local_supply_repair',rr,ee,repaired_tiers,None),
                        ('baseline_contested',r,e,tiers,blocked),
                        ('local_supply_repair_contested',rr,ee,repaired_tiers,repaired_blocked)):
                        m=measure(c,s,sp,res,enemy,values,budget,lev,occ)
                        rows.append({'seed':seed,'density':density,'bridges_closed':broken,'path_budget_edges':budget,'mode':mode,**m})
    summary={}
    for mode in ('baseline','local_supply_repair','baseline_contested','local_supply_repair_contested'):
        subset=[x for x in rows if x['mode']==mode]; routes=sorted(v for x in subset for v in x['round_trip_grid_edges'])
        summary[mode]={'configurations':len(subset),'candidate_evaluations':sum(x['candidates'] for x in subset),
                       'admitted':sum(x['admitted'] for x in subset),'rejected':sum(len(x['rejected']) for x in subset),
                       'route_edges_min':min(routes) if routes else None,'route_edges_median':routes[len(routes)//2] if routes else None,
                       'route_edges_p95':routes[math.ceil(len(routes)*.95)-1] if routes else None,'route_edges_max':max(routes) if routes else None}
    trips=math.ceil(values['Chapter1GatherWood']/values['ForestLoad'])+math.ceil(values['Chapter1GatherStone']/values['QuarryLoad'])
    # Persist compact replay coordinates and distributions, not thousands of
    # duplicated world snapshots. Each case is regenerated by world(seed,...).
    compact=[]
    for row in rows:
        routes=sorted(row['round_trip_grid_edges'])
        compact.append({k:row[k] for k in ('seed','density','bridges_closed','path_budget_edges','mode','candidates','admitted')} | {
            'rejection_reasons':dict(collections.Counter(reason for item in row['rejected'] for reason in item['reasons'])),
            'route_edges_min':min(routes) if routes else None,'route_edges_max':max(routes) if routes else None,
            'future_seconds_max_at_4_8_16mps':[max(routes)*8/s+trips*values['GatherSeconds'] for s in (4,8,16)] if routes else [],
            'unreachable_basic_resource_positions':row['unreachable_basic_resource_positions']})
    return {'schema':2,'kind':'HYPOTHETICAL_OFFLINE_DESIGN_NOT_REGION1_CERTIFICATION','profile_sha256':sha,
            'profile':values,'winning_progression_economic_ledger':progression_ledger(values),'assumptions':{'grid_cell_m':8,'future_speed_m_s':[4,8,16],'path_budgets_edges':[8,16],
            'current_live_resource_loop_seconds':trips*(2*values['TravelSeconds']+values['GatherSeconds']),
            'owner_resource_rule':'Resources have levels; one occupant per node; stronger challenger displaces weaker occupant',
            'resource_levels':[1,2,3],'initial_comparison_level':1,'higher_level_economy':'UNDEFINED_NOT_INVENTED',
            'exclusive_concurrent_slots_per_node':1,'abstract_strength_ranks':[1,2],
            'tie_assumption':'Incumbent stays; not a finalized gameplay rule',
            'contested_case':'Stronger rivals occupy nearest L1 nodes; reevaluate alternate routes and exclusive capacity',
            'not_modelled':['sustained PvP blockade over time','server concurrency implementation','actual Unity slopes/bridges','combat formula, casualties and gathered cargo settlement','combat outcomes and full state-machine timing','measured mobile streaming']},
            'summary':summary,'cases':compact}

if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('--seeds',type=int,default=24);p.add_argument('--output',type=pathlib.Path,required=True);a=p.parse_args()
    if a.seeds<1: p.error('seeds must be positive')
    result=run(a.seeds);a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_text(json.dumps(result,indent=2)+'\n')
    print(json.dumps(result['summary'],indent=2))
