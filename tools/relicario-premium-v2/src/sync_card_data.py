from pathlib import Path
import json
p=Path(__file__).resolve().parent.parent
v=json.loads((p/'card_data.json').read_text())
assert v['id']=='bosque-valoria' and v['values']=={'N':6,'E':4,'S':3,'O':2} and v['effectKey']=='wood20m'
(p/'card-data.js').write_text('window.ELDORIA_RELICARIO_DATA='+json.dumps(v,ensure_ascii=False,separators=(',',':'))+';\n')
