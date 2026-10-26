import unittest
from simulate import distances, assignment, economy, progression_ledger, ROOT, world, repaired, measure, contest

class BalanceTests(unittest.TestCase):
    def test_river_detour_is_not_euclidean(self):
        cells={(x,y) for x in range(5) for y in range(5) if x!=2 or y==4}
        self.assertEqual(distances(cells,(1,0))[(3,0)],10)
        cells.remove((2,4));self.assertNotIn((3,0),distances(cells,(1,0)))
    def test_shared_stock_cannot_be_promised_twice(self):
        self.assertEqual(len(assignment([[0],[0]],[1])),1)
    def test_alternative_allocation_avoids_greedy_failure(self):
        a=assignment([[0,1],[0]],[1,1]);self.assertEqual(len(a),2);self.assertEqual(a[1],0)
    def test_live_capacity_contract(self):
        source=(ROOT/'Unity/Assets/Eldoria/Scripts/Application/LocalGateway.cs').read_text()
        self.assertIn('1250:5200',source)
        v,_=economy();self.assertEqual(v['Chapter1GatherWood'],600);self.assertEqual(v['Chapter1GatherStone'],500)
    def test_repair_preserves_geography_and_does_not_share_stock(self):
        v,_=economy();c,s,sp,r,e=world(7,24,True);original=set(c);rr,ee=repaired(c,s,sp,r,e)
        m=measure(c,s,sp,rr,ee,v,8);self.assertEqual(c,original);self.assertEqual(m['admitted'],24)
        self.assertEqual(len(set(m['stock_reservations']['wood'].values())),24)
        self.assertLessEqual(max(m['round_trip_grid_edges']),12)
    def test_economic_path_matches_native_final_wallet(self):
        v,_=economy();ledger=progression_ledger(v)
        self.assertEqual(ledger['steps'][-1]['wood'],1030)
        self.assertEqual(ledger['steps'][-1]['stone'],995)
        source=(ROOT/'Unity/Assets/Eldoria/Scripts/Application/LocalGateway.cs').read_text()
        self.assertIn('rewardWood = 80; rewardStone = 70;',source)
        self.assertIn('rewardWood = 120; rewardStone = 100;',source)

    def test_quarry_stock_does_not_create_multiple_occupancy_slots(self):
        v,_=economy();cells={(x,y) for x in range(6) for y in range(6)}
        m=measure(cells,cells,[(0,0),(0,4)],{'wood':[(1,0),(1,4)],'stone':[(2,2)]},[(3,3)],v,12)
        self.assertEqual(m['admitted'],1)
        self.assertEqual(len(m['stock_reservations']['stone']),1)

    def test_stronger_replaces_occupant_without_a_second_collector(self):
        occupant=contest(None,('first',1))
        self.assertEqual(contest(occupant,('stronger',2)),('stronger',2))
        self.assertEqual(contest(('stronger',2),('weaker',1)),('stronger',2))
        self.assertEqual(contest(('incumbent',2),('equal',2)),('incumbent',2))

    def test_higher_tiers_and_stronger_occupants_do_not_count_as_free_l1_supply(self):
        v,_=economy();cells={(x,y) for x in range(6) for y in range(6)}
        resources={'wood':[(1,0)],'stone':[(2,2)]};starts=[(0,0)];enemies=[(3,3)]
        m=measure(cells,cells,starts,resources,enemies,v,12,{'wood':[2],'stone':[1]})
        self.assertEqual(m['admitted'],0)
        m=measure(cells,cells,starts,resources,enemies,v,12,occupied={'wood':{0:('rival',2)},'stone':{}})
        self.assertEqual(m['admitted'],0)

if __name__=='__main__':unittest.main()
