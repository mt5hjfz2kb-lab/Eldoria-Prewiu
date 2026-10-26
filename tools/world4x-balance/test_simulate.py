import unittest
from simulate import distances, assignment, economy, ROOT, world, repaired, measure

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

if __name__=='__main__':unittest.main()
