"""Evaluate one generated pure function through a bounded AST interpreter; never exec it."""
import ast
import hashlib
import json
import pathlib
import sys
import urllib.request

MODEL = 'qwen2.5-coder:3b'
BASE = 'http://127.0.0.1:11434'

def request(path, payload=None):
    data = None if payload is None else json.dumps(payload).encode()
    req = urllib.request.Request(BASE + path, data=data, headers={'Content-Type': 'application/json'})
    with urllib.request.build_opener(urllib.request.ProxyHandler({})).open(req, timeout=180) as res:
        return json.load(res)

def check(source):
    if len(source) > 4000:
        raise ValueError('Oversized output')
    tree = ast.parse(source)
    allowed = (ast.Module, ast.FunctionDef, ast.arguments, ast.arg, ast.If, ast.Return,
               ast.Compare, ast.Lt, ast.Gt, ast.LtE, ast.GtE, ast.Eq, ast.NotEq,
               ast.Name, ast.Load, ast.Constant)
    nodes = list(ast.walk(tree))
    if len(nodes) > 100 or any(not isinstance(n, allowed) for n in nodes):
        raise ValueError('Only bounded pure comparisons and returns permitted')
    if len(tree.body) != 1 or not isinstance(tree.body[0], ast.FunctionDef):
        raise ValueError('Exactly one function required')
    fn = tree.body[0]
    if fn.name != 'clamp' or [a.arg for a in fn.args.args] != ['value', 'low', 'high']:
        raise ValueError('Wrong function signature')
    if fn.decorator_list or fn.args.defaults or fn.args.kw_defaults or fn.args.vararg or fn.args.kwarg or fn.args.posonlyargs or fn.args.kwonlyargs or fn.returns:
        raise ValueError('Signature extensions prohibited')
    def expr(node, env):
        if isinstance(node, ast.Name):
            return env[node.id]
        if isinstance(node, ast.Constant) and type(node.value) in (int, bool):
            return node.value
        if isinstance(node, ast.Compare):
            vals = [expr(node.left, env)] + [expr(v, env) for v in node.comparators]
            operations = {ast.Lt: lambda a,b:a<b, ast.Gt:lambda a,b:a>b,
                          ast.LtE:lambda a,b:a<=b, ast.GtE:lambda a,b:a>=b,
                          ast.Eq:lambda a,b:a==b, ast.NotEq:lambda a,b:a!=b}
            return all(operations[type(op)](vals[i], vals[i+1]) for i,op in enumerate(node.ops))
        raise ValueError('Unsupported expression')
    def block(stmts, env):
        for node in stmts:
            if isinstance(node, ast.Return):
                return True, expr(node.value, env)
            if isinstance(node, ast.If):
                returned, value = block(node.body if expr(node.test, env) else node.orelse, env)
                if returned:
                    return returned, value
            else:
                raise ValueError('Unsupported statement')
        return False, None
    cases = [(-3,0,10,0),(5,0,10,5),(14,0,10,10),(0,0,10,0),
             (10,0,10,10),(-7,-10,-2,-7),(-20,-10,-2,-10),(20,-10,-2,-2),(5,3,3,3)]
    for value, low, high, expected in cases:
        returned, actual = block(fn.body, dict(value=value, low=low, high=high))
        if not returned or type(actual) is not int or actual != expected:
            raise AssertionError((value,low,high,expected,actual))
    return len(cases)

def main(out):
    out.mkdir(parents=True, exist_ok=True)
    tags = request('/api/tags')
    model = next(m for m in tags['models'] if m['name'] == MODEL)
    payload = {'model':MODEL, 'stream':False, 'keep_alive':'30s', 'format':'json',
        'options':{'temperature':0,'num_ctx':2048,'num_predict':512,'num_thread':4},
        'prompt':'Return only a JSON object with key code containing Python source. Write def clamp(value, low, high): returning low if value < low, high if value > high, otherwise value. Use if statements and return, only comparisons and parameter names. No annotations, comments, strings, calls, imports, assignments or extra functions. Assume low <= high.'}
    response = request('/api/generate', payload)
    (out/'response.json').write_text(json.dumps(response, indent=2), encoding='utf-8')
    if not response.get('done') or response.get('model') != MODEL:
        raise ValueError('Incomplete/wrong model response')
    source = json.loads(response['response'])['code']
    (out/'generated-clamp.txt').write_text(source, encoding='utf-8')
    passed = check(source)
    ps = request('/api/ps')
    proof = {'status':'PASS','model':MODEL,'digest':model['digest'],'test_cases_passed':passed,
             'endpoint':BASE,'external_api_calls':0,'arbitrary_generated_code_executed':False,
             'method':'bounded AST interpretation of generated pure Python function',
             'source_sha256':hashlib.sha256(source.encode()).hexdigest(),
             'eval_count':response.get('eval_count'),'eval_duration_ns':response.get('eval_duration'),
             'loaded_models':ps}
    (out/'proof.json').write_text(json.dumps(proof,indent=2), encoding='utf-8')
    request('/api/generate', {'model':MODEL,'keep_alive':0})
    if request('/api/ps').get('models'):
        raise ValueError('Model remains loaded after explicit unload')
    print(json.dumps(proof))

if __name__ == '__main__':
    main(pathlib.Path(sys.argv[1]))
