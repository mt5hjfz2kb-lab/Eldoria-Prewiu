#!/usr/bin/env python3
"""Portable public-data SLM memory handoff across two separate GitHub-hosted runners.
NOT a ChatGPT web connector, NOT production ingestion, never use private data.
"""
import asyncio, hashlib, json, os, pathlib, shutil, socket, subprocess, sys, tarfile, time
from datetime import datetime, timezone

ROLE = sys.argv[1]
ROOT=pathlib.Path(os.environ["RUNNER_TEMP"]) / "eldoria-slm-handoff"
HOME=ROOT/"home"
DB=HOME/"data"
OUT=pathlib.Path(os.environ["RUNNER_TEMP"]) / ("eldoria-writer-result" if ROLE=="writer" else "eldoria-reader-result")
OUT.mkdir(parents=True,exist_ok=True)
HOME.mkdir(parents=True,exist_ok=True)
os.environ.update({"HOME":str(HOME), "SLM_HOME":str(DB), "SLM_MCP_PROFILE":"core", "SLM_SKIP_LAYA":"1", "HF_HUB_DISABLE_TELEMETRY":"1"})
def run(*args, timeout=150, check=True):
    t=time.monotonic()
    x=subprocess.run(["slm",*args],text=True,capture_output=True,timeout=timeout,env=os.environ)
    if check and x.returncode:
        raise RuntimeError(f"slm {args[0]} exit={x.returncode}: {x.stderr[-850:]} {x.stdout[-500:]}")
    return {"rc":x.returncode,"out":x.stdout,"err":x.stderr,"seconds":round(time.monotonic()-t,3)}
def result(payload,name):
    (OUT/name).write_text(json.dumps(payload,indent=2,ensure_ascii=False),encoding="utf-8")
def extract_text(obj):
    return "\n".join(getattr(x,"text",str(x)) for x in obj.content)
def assert_known(content, source_fragment):
    if source_fragment.casefold() not in content.casefold():
        raise AssertionError("A reviewed source reference was not recovered")
MEMORIES=[
"ELDORIA-PILOT-SOURCE: README.md; state=reviewed; Eldoria main playable project is Unity/; v0220 is a historical web prototype, not the Unity runtime.",
"ELDORIA-PILOT-SOURCE: AGENTS.md; state=reviewed; Since 2026-10-22 Qwen/Ollama is removed from the required Eldoria art production workflow; historical Qwen instructions are superseded.",
"ELDORIA-PILOT-SOURCE: docs/ELDORIA_PARALLEL_WORKSTREAM_PROTOCOL.md; state=reviewed; A technical PASS and a provisional artistic acceptance do not mean ART_APPROVED or a published game.",
"ELDORIA-PILOT-SOURCE: QA_AND_DEPLOY.md; state=reviewed; Native Unity runs and comparable A/B captures are required for Unity change evidence; inherited JavaScript prototypes do not certify Unity."
]
stamp={"role":ROLE,"timestamp_utc":datetime.now(timezone.utc).isoformat(),"hostname":socket.gethostname(),
       "job":os.environ.get("GITHUB_JOB"),"run_id":os.environ.get("GITHUB_RUN_ID"),"package_pin":"4.2.12",
       "no_windows":True,"no_unity":True,"no_personal_memories":True}
try:
    a=run("init","--auto",timeout=85)
    run("mode","a",timeout=25)
    run("serve","start",timeout=75)
    warm=run("warmup",timeout=190)
    stamp["warmup_status"]="PASS" if ("[PASS]" in warm["out"]) else "UNCONFIRMED"
    if ROLE=="writer":
        for mem in MEMORIES:
            run("remember",mem,"--kind","fact","--tags","eldoria,isolated,public,reproducible",timeout=65)
        rec=run("recall","What is the source for the main playable Unity project?",timeout=90)
        assert_known(rec["out"],"README.md")
        stamp["records_stored"]=len(MEMORIES)
        stamp["verified_pre_snapshot"]=True
        # Never archive a running SQLite database. The source directory is disposable and contains PUBLIC synthetic summaries.
        run("serve","stop",timeout=35)
        all_files=[p for p in DB.rglob("*") if p.is_file() and not p.is_symlink()]
        # Strict allowlist; NEVER upload config, access tokens, auth state, secrets or passwords.
        db_suffixes=(".db",".db-shm",".db-wal",".sqlite",".sqlite-shm",".sqlite-wal",
                     ".sqlite3",".sqlite3-shm",".sqlite3-wal")
        def is_public_data_file(p):
            relative=p.relative_to(DB)
            parts=[s.casefold() for s in relative.parts]
            if any(any(term in s for term in ("secret","auth","token","credential","password","keyfile")) for s in parts):
                return False
            return p.name.casefold().endswith(db_suffixes) or "lance" in parts or "lancedb" in parts
        files=[p for p in all_files if is_public_data_file(p)]
        stamp["non_data_files_excluded_count"]=len(all_files)-len(files)
        stamp["database_files_count"]=sum(p.name.casefold().endswith(db_suffixes) for p in files)
        if not files or not stamp["database_files_count"]:
            raise AssertionError("No SQLite memory DB in safe allowlist; do not upload arbitrary files")
        size=sum(p.stat().st_size for p in files)
        if size > 45_000_000: raise AssertionError(f"Snapshot too large: {size}")
        archive=OUT/"slm-public-memory.tar.gz"
        with tarfile.open(archive,"w:gz") as t:
            for p in files:
                t.add(p,arcname=str(p.relative_to(DB)),recursive=False)
        stamp["snapshot_file_count"]=len(files)
        stamp["snapshot_uncompressed_bytes"]=size
        stamp["snapshot_sha256"]=hashlib.sha256(archive.read_bytes()).hexdigest()
        stamp["snapshot_bytes"]=archive.stat().st_size
        result(stamp,"writer.json")
        print("WRITER_PASS "+json.dumps(stamp))
    elif ROLE=="reader":
        inp=pathlib.Path(os.environ["RUNNER_TEMP"])/"eldoria-handoff-input"
        writer=json.loads((inp/"writer.json").read_text(encoding="utf-8"))
        assert writer["role"]=="writer"
        assert writer["hostname"] != stamp["hostname"], "Writer and reader were not demonstrated on distinct hosts"
        archive=inp/"slm-public-memory.tar.gz"
        assert hashlib.sha256(archive.read_bytes()).hexdigest()==writer["snapshot_sha256"]
        run("serve","stop",timeout=35)
        if DB.exists(): shutil.rmtree(DB)
        DB.mkdir(parents=True)
        with tarfile.open(archive,"r:gz") as t:
            members=t.getmembers()
            if any(not m.isfile() or pathlib.Path(m.name).is_absolute() or ".." in pathlib.Path(m.name).parts for m in members):
                raise AssertionError("Unsafe archive entry")
            t.extractall(DB,filter="data")
        run("serve","start",timeout=75)
        run("warmup",timeout=190)
        # Agent B is a new MCP client on a different host; it must read A's persistent datastore.
        from mcp import Client, StdioServerParameters
        async def probe():
            async with Client(StdioServerParameters(command=shutil.which("slm"), args=["mcp"],env=dict(os.environ))) as client:
                ls=await client.list_tools()
                names=[x.name for x in ls.tools]
                assert "recall" in names and "remember" in names
                checks=[]
                for question,needle in [
                   ("Where is the canonical Eldoria playable Unity project?", "README.md"),
                   ("What did AGENTS.md say about Qwen art from October 22?", "AGENTS.md"),
                   ("Does a technical PASS mean art approval in Eldoria?", "ELDORIA_PARALLEL_WORKSTREAM_PROTOCOL.md"),
                   ("Can JavaScript prototype tests certify the Unity build?", "QA_AND_DEPLOY.md")]:
                    result=await client.call_tool("recall",{"query":question,"limit":10})
                    response=extract_text(result)
                    checks.append({"question":question,"source":needle,"retrieved":needle in response,
                                   "tool_error":bool(result.is_error)})
                # Agent B writes another PUBLIC factual item into the same restored store.
                saved=await client.call_tool("remember",{"content":"ELDORIA-PILOT-SOURCE: README.md; agent=B; state=test; An independently restarted MCP client validated the public-memory handoff.","tags":"eldoria,isolated,handoff"})
                return {"tools":len(names),"checks":checks,"save_success":not bool(saved.is_error),
                        "saved_response_excerpt":extract_text(saved)[:350]}
        resp=asyncio.run(asyncio.wait_for(probe(),timeout=160))
        stamp["mcp_agent_b"]=resp
        assert all(x["retrieved"] and not x["tool_error"] for x in resp["checks"]),"Some sources not recalled"
        assert resp["save_success"], "Agent B write failed"
        new=run("recall","independently restarted MCP client validated the public memory handoff",timeout=70)
        assert_known(new["out"],"agent=B")
        stamp["agent_b_write_visible_to_independent_cli_process"]=True
        stamp["writer_hostname"]=writer["hostname"]
        stamp["distinct_runner_hosts"]=writer["hostname"]!=stamp["hostname"]
        stamp["snapshot_hash_validated"]=True
        run("serve","stop",timeout=35)
        stamp["status"]="PASS_SHARED_MEMORY_HANDOFF"
        result(stamp,"reader.json")
        print("READER_PASS "+json.dumps(stamp))
    else: raise ValueError("role must be writer or reader")
except Exception as e:
    stamp["status"]="FAILED"
    stamp["failure_type"]=type(e).__name__
    stamp["failure"]=str(e)[:1500]
    result(stamp,ROLE+".json")
    print("PILOT_FAILED "+json.dumps(stamp))
    raise
