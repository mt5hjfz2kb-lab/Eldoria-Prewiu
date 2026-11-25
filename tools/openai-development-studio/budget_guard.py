"""Check versioned cumulative reservations; default deny new spend and cycle rollover."""
import datetime as dt
from decimal import Decimal, InvalidOperation
import json
from pathlib import Path
import sys
import unittest

def validate(data, day):
    cycle=data["cycle"]
    if cycle.get("automatic_reset") is not False:
        raise ValueError("Automatic cycle reset forbidden")
    start=dt.date.fromisoformat(cycle["starts"])
    end=dt.date.fromisoformat(cycle["ends_exclusive"])
    if not start<=day<end: raise ValueError("Billing cycle ended; reconcile before new spend")
    cap=Decimal(cycle["pilot_cap_usd"])
    if not cap.is_finite() or cap<=0 or cap>Decimal("5.00"): raise ValueError("Pilot cap invalid")
    ids=set(); runs=set(); total=Decimal(0)
    for row in data["reservations"]:
        v=Decimal(row["reserve_usd"])
        if not v.is_finite() or v<=0 or row["id"] in ids or row["run_id"] in runs:
            raise ValueError("Invalid or duplicate authorization")
        if row["status"] not in ("authorized_once","executed","failed_reserved","closed_reserved"):
            raise ValueError("Unknown reservation status")
        ids.add(row["id"]);runs.add(row["run_id"]);total+=v
    if total>cap: raise ValueError("Cumulative pilot cap exceeded")
    return {"cap_usd":str(cap),"reserved_usd":str(total),"unreserved_usd":str(cap-total),"global_billing_cap":False}

def authorize(data, day, authorization, run_id, attempt, required):
    validate(data,day)
    if attempt!=1: raise ValueError("Retries do not receive spending authorization")
    matches=[x for x in data["reservations"] if x["id"]==authorization and str(x["run_id"])==str(run_id)]
    if len(matches)!=1 or matches[0]["status"]!="authorized_once" or Decimal(matches[0]["reserve_usd"])<required:
        raise ValueError("No valid pre-reserved authorization")
    return True

class Tests(unittest.TestCase):
    def setUp(self):
        self.day=dt.date(2026,11,25)
        self.data={"cycle":{"starts":"2026-11-25","ends_exclusive":"2026-12-17","pilot_cap_usd":"5.00","automatic_reset":False},
        "reservations":[{"id":"a","run_id":1,"reserve_usd":"1.45","status":"authorized_once"}]}
    def test_remaining(self): self.assertEqual(validate(self.data,self.day)["unreserved_usd"],"3.55")
    def test_duplicate(self):
        self.data["reservations"]*=2
        with self.assertRaises(ValueError): validate(self.data,self.day)
    def test_cap(self):
        self.data["reservations"][0]["reserve_usd"]="5.01"
        with self.assertRaises(ValueError): validate(self.data,self.day)
    def test_expiry(self):
        with self.assertRaises(ValueError): validate(self.data,dt.date(2026,12,17))
    def test_authorize(self):
        self.assertTrue(authorize(self.data,self.day,"a",1,1,Decimal("1.45")))
        for args in [("a",1,2,Decimal("1.45")),("a",2,1,Decimal("1.45")),("a",1,1,Decimal("1.46"))]:
            with self.assertRaises(ValueError): authorize(self.data,self.day,*args)
    def test_closed(self):
        self.data["reservations"][0]["status"]="executed"
        with self.assertRaises(ValueError): authorize(self.data,self.day,"a",1,1,Decimal("1.45"))

if __name__=="__main__":
    if sys.argv[1:]==["--self-test"]: unittest.main(argv=[sys.argv[0]],verbosity=2)
    else:
        p=Path(__file__).with_name("budget-cycle.json")
        print(json.dumps(validate(json.loads(p.read_text()),dt.datetime.now(dt.timezone.utc).date()),indent=2))
