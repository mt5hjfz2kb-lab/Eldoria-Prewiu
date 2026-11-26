"""Offline mathematical mirror of the HUD ribbon constraints; not a Unity visual test."""
import unittest
def clamp(x,a,b):return max(a,min(b,x))
def layout(width,name_width):
    strip=min(390,width)
    floor=min(234,max(0,strip-119))
    identity=clamp(name_width+46,102,max(102,strip-floor-17))
    row=min(270,max(15,strip-identity-17))
    cell=(row-12)/3
    icon=clamp(cell*.30,18,26)
    value=max(1,cell-5-icon-4-5)
    return identity,row,cell,icon,value
class Ribbon(unittest.TestCase):
    def test_widths_and_names(self):
        for w in (300,320,360,375,390,414,430,750):
            for name in (0,50,100,150,200,500,1000):
                with self.subTest(width=w,name=name):
                    identity,row,cell,icon,value=layout(w,name)
                    self.assertGreaterEqual(min(390,w)-identity-row-12,4)
                    self.assertGreaterEqual(cell-5-value-(5+icon),3.999999)
                    self.assertGreaterEqual(value,24)
if __name__=="__main__":unittest.main()
