"""Read four exact existing contract blobs into runner TEMP, not into runtime or another game authority."""
from pathlib import Path
import hashlib, json, os, time, urllib.request

SOURCE = '773f4f24c09445bf4fe9f16d6f8d98949ac5ae6f'
FILES = {
    'Domain/GameState.cs': 'c1741c112a4c6bb904d446d6fb976c145c89afdc',
    'Domain/ParcelBuildingState.cs': '4fe63c6d56103ccf86fdb530f610c42de42560b5',
    'Domain/SliceContentProfiles.cs': '8767bd75b373c474c034508dc6695734f86d15e5',
    'Application/LocalGateway.cs': '7cdd62bb287213172ab5247141dc5b09d93be3f9',
}
root = Path(__file__).resolve().parents[2]
destination = (Path(os.environ['RUNNER_TEMP']) / 'royal-contracts' / 'Scripts').resolve()
if destination == root or root in destination.parents:
    raise RuntimeError('Contract test material must stay outside the repository runtime')
records = []
for relative, expected in FILES.items():
    url = 'https://raw.githubusercontent.com/mt5hjfz2kb-lab/Eldoria-Prewiu/' + SOURCE + '/Unity/Assets/Eldoria/Scripts/' + relative
    for attempt in range(3):
        try:
            request = urllib.request.Request(url, headers={'User-Agent': 'Eldoria-RoyalGold-Contract-Test'})
            with urllib.request.urlopen(request, timeout=30) as response:
                data = response.read(200001)
            if len(data) > 200000:
                raise RuntimeError('Unexpectedly large contract response')
            break
        except Exception:
            if attempt == 2:
                raise
            time.sleep(2)
    blob = hashlib.sha1(b'blob ' + str(len(data)).encode() + b'\0' + data).hexdigest()
    if blob != expected:
        raise RuntimeError('Contract byte mismatch: ' + relative)
    path = destination / relative
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_bytes(data)
    records.append({'path': relative, 'git_blob': blob, 'bytes': len(data), 'sha256': hashlib.sha256(data).hexdigest()})
evidence = root / 'docs/ui-royal-gold-v1/consumed-valoria-contracts.json'
evidence.write_text(json.dumps({'source_sha': SOURCE, 'ui_source': os.environ.get('GITHUB_SHA'), 'temporary_test_inputs_only': True, 'files': records}, indent=2))
print('PINNED_REAL_CONTRACTS_READ_PASS', len(records), 'files;', sum(r['bytes'] for r in records), 'bytes')
