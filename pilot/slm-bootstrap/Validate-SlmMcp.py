#!/usr/bin/env python3
"""Read-only, loopback-only MCP test after daemon restart; no secrets are logged."""
import json
import sys
import urllib.error
import urllib.request

ENDPOINT = 'http://127.0.0.1:8765/mcp/'
PROTOCOL = '2025-06-18'


def unpack(raw, content_type, request_id):
    data = raw.decode('utf-8-sig')
    if 'text/event-stream' in content_type:
        replies = [
            json.loads(line[5:].strip())
            for line in data.splitlines()
            if line.startswith('data:') and line[5:].strip() not in ('', '[DONE]')
        ]
    else:
        replies = [json.loads(data)]
    for obj in replies:
        if isinstance(obj, dict) and obj.get('id') == request_id:
            if 'error' in obj or not isinstance(obj.get('result'), dict):
                raise RuntimeError(f'MCP JSON-RPC error or missing result (id={request_id})')
            return obj['result']
    raise RuntimeError(f'MCP response missing (id={request_id})')


def rpc(method, request_id, params=None):
    body = {'jsonrpc': '2.0', 'id': request_id, 'method': method}
    if params is not None:
        body['params'] = params
    request = urllib.request.Request(
        ENDPOINT,
        data=json.dumps(body).encode('utf-8'),
        headers={
            'Content-Type': 'application/json',
            'Accept': 'application/json, text/event-stream',
            'MCP-Protocol-Version': PROTOCOL,
        },
        method='POST',
    )
    try:
        with urllib.request.urlopen(request, timeout=40) as response:
            return unpack(response.read(2_000_000), response.headers.get('Content-Type', ''), request_id)
    except urllib.error.HTTPError as exc:
        raise RuntimeError(f'MCP local HTTP error {exc.code}') from exc


def recall_check(query, expected, request_id):
    result = rpc('tools/call', request_id, {'name': 'recall', 'arguments': {'query': query, 'limit': 8}})
    if result.get('isError'):
        raise RuntimeError(f'MCP recall failed #{request_id}')
    texts = [
        part.get('text', '') for part in result.get('content', [])
        if isinstance(part, dict) and part.get('type') == 'text'
    ]
    if not texts or expected.casefold() not in '\n'.join(texts).casefold():
        raise RuntimeError(f'Persisted source-reviewed memory not found via MCP #{request_id}')


def main():
    init = rpc('initialize', 1, {
        'protocolVersion': PROTOCOL,
        'capabilities': {},
        'clientInfo': {'name': 'eldoria-slm-readonly-validation', 'version': '1.0.0'},
    })
    if not init.get('serverInfo'):
        raise RuntimeError('MCP initialize failed')
    tools = rpc('tools/list', 2)
    names = {t.get('name') for t in tools.get('tools', []) if isinstance(t, dict)}
    if 'recall' not in names:
        raise RuntimeError('recall tool not exposed in MCP core profile')
    recall_check('Unity main runtime v0220 prototype', 'Unity/', 3)
    recall_check('Qwen Ollama art discontinued October 2026', 'Qwen', 4)
    print('SLM_MCP_LOOPBACK_PASS: tools/list and two persisted read-only tools/call recalls')


if __name__ == '__main__':
    try:
        main()
    except (ValueError, RuntimeError, OSError) as exc:
        print('SLM_MCP_LOOPBACK_FAIL: ' + str(exc), file=sys.stderr)
        sys.exit(1)
