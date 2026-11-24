-- PostgreSQL 16 reference migration. Run only on an isolated test DB until integration.
-- Roles must be provisioned by the operator as NOLOGIN roles. Never grant either
-- to an untrusted game client; identity and realm authorization belong to the API.
BEGIN;
CREATE SCHEMA eldoria_legacy;
REVOKE ALL ON SCHEMA eldoria_legacy FROM PUBLIC;
GRANT USAGE ON SCHEMA eldoria_legacy TO eldoria_rank_writer, eldoria_rank_reader;

CREATE TABLE eldoria_legacy.player (
 world_id text NOT NULL CHECK (length(world_id) BETWEEN 1 AND 128),
 player_id text NOT NULL CHECK (length(player_id) BETWEEN 1 AND 128),
 display_name text NOT NULL CHECK (length(btrim(display_name)) BETWEEN 1 AND 80),
 PRIMARY KEY(world_id,player_id)
);
CREATE TABLE eldoria_legacy.period (
 world_id text NOT NULL, period_id text NOT NULL,
 category text NOT NULL CHECK (category IN ('power','kingdom','army','vigilants','hunters','explorers')),
 cadence text NOT NULL CHECK(cadence IN ('permanent','weekly','season')),
 starts_at timestamptz NOT NULL, ends_at timestamptz NOT NULL,
 closed_at timestamptz,
 PRIMARY KEY(world_id,period_id,category),
 CHECK (starts_at<ends_at), CHECK (closed_at IS NULL OR closed_at>=ends_at),
 CHECK (cadence<>'weekly' OR (
 starts_at=date_trunc('week',starts_at AT TIME ZONE 'UTC') AT TIME ZONE 'UTC'
 AND ends_at=starts_at+interval '168 hours')),
 CHECK (cadence<>'permanent' OR category NOT IN ('hunters','explorers'))
);
CREATE TABLE eldoria_legacy.receipt (
 world_id text NOT NULL, player_id text NOT NULL, event_id text NOT NULL CHECK(length(event_id) BETWEEN 1 AND 256),
 kind text NOT NULL CHECK(kind IN ('hunt','discovery','gather','vigilant')),
 subject_id text NOT NULL CHECK(length(subject_id) BETWEEN 1 AND 128),
 occurred_at timestamptz NOT NULL,
 amount bigint NOT NULL CHECK(amount>0),
 level int NOT NULL,
 PRIMARY KEY(world_id,player_id,event_id),
 FOREIGN KEY(world_id,player_id) REFERENCES eldoria_legacy.player(world_id,player_id),
 CHECK ((kind='hunt' AND level BETWEEN 1 AND 5 AND amount=1)
 OR (kind='discovery' AND level=0 AND amount=1)
 OR (kind='gather' AND level=0)
 OR (kind='vigilant' AND level BETWEEN 1 AND 10000 AND amount=1))
);
CREATE INDEX receipt_window ON eldoria_legacy.receipt(world_id,kind,occurred_at,player_id);
CREATE TABLE eldoria_legacy.score (
 world_id text NOT NULL, period_id text NOT NULL, category text NOT NULL,
 player_id text NOT NULL, value bigint NOT NULL CHECK(value>=0),
 achieved_at timestamptz NOT NULL, observed_at timestamptz NOT NULL, revision bigint NOT NULL CHECK(revision>=0),
 PRIMARY KEY(world_id,period_id,category,player_id),
 FOREIGN KEY(world_id,period_id,category) REFERENCES eldoria_legacy.period(world_id,period_id,category),
 FOREIGN KEY(world_id,player_id) REFERENCES eldoria_legacy.player(world_id,player_id)
);
CREATE TABLE eldoria_legacy.frozen (
 world_id text NOT NULL, period_id text NOT NULL, category text NOT NULL,
 position bigint NOT NULL CHECK(position>0), player_id text NOT NULL,
 display_name text NOT NULL, value bigint NOT NULL, achieved_at timestamptz NOT NULL,
 PRIMARY KEY(world_id,period_id,category,position),
 UNIQUE(world_id,period_id,category,player_id),
 FOREIGN KEY(world_id,period_id,category) REFERENCES eldoria_legacy.period(world_id,period_id,category)
);
-- An award is an immutable recognition entitlement, not a wallet payment.
CREATE TABLE eldoria_legacy.award (
 world_id text NOT NULL, period_id text NOT NULL, category text NOT NULL,
 player_id text NOT NULL, award_id text NOT NULL,
 PRIMARY KEY(world_id,period_id,category,player_id),
 FOREIGN KEY(world_id,period_id,category,player_id)
 REFERENCES eldoria_legacy.frozen(world_id,period_id,category,player_id)
);

-- Administrative provisioning of identities and periods is separate from clients.
GRANT SELECT, INSERT, UPDATE ON eldoria_legacy.player TO eldoria_rank_writer;
GRANT SELECT, INSERT ON eldoria_legacy.period TO eldoria_rank_writer;

CREATE FUNCTION eldoria_legacy.record_receipt(
 w text,p text,e text,k text,s text,t timestamptz,a bigint,l int)
 RETURNS boolean LANGUAGE plpgsql SECURITY DEFINER SET search_path=pg_catalog AS $$
 DECLARE old eldoria_legacy.receipt%ROWTYPE;
 BEGIN
  IF t IS NULL OR t>clock_timestamp() THEN RAISE EXCEPTION 'invalid receipt time'; END IF;
  INSERT INTO eldoria_legacy.receipt(world_id,player_id,event_id,kind,subject_id,occurred_at,amount,level)
   VALUES(w,p,e,k,s,t,a,l) ON CONFLICT DO NOTHING;
  IF FOUND THEN RETURN true; END IF;
  SELECT * INTO STRICT old FROM eldoria_legacy.receipt
   WHERE world_id=w AND player_id=p AND event_id=e;
  IF ROW(old.kind,old.subject_id,old.occurred_at,old.amount,old.level)
     IS DISTINCT FROM ROW(k,s,t,a,l) THEN RAISE EXCEPTION 'receipt identity conflict'; END IF;
  RETURN false;
 END $$;

CREATE FUNCTION eldoria_legacy.record_score(
 w text,per text,cat text,p text,v bigint,t timestamptz,rev bigint)
 RETURNS boolean LANGUAGE plpgsql SECURITY DEFINER SET search_path=pg_catalog AS $$
 DECLARE win eldoria_legacy.period%ROWTYPE; old eldoria_legacy.score%ROWTYPE;
 BEGIN
  IF cat IN ('hunters','explorers') THEN RAISE EXCEPTION 'receipt-derived category'; END IF;
  SELECT * INTO STRICT win FROM eldoria_legacy.period
   WHERE world_id=w AND period_id=per AND category=cat FOR SHARE;
  IF win.closed_at IS NOT NULL OR t IS NULL OR t<win.starts_at OR t>=win.ends_at OR t>clock_timestamp()
   OR v IS NULL OR v<0 OR rev IS NULL OR rev<0 THEN RAISE EXCEPTION 'invalid snapshot'; END IF;
  INSERT INTO eldoria_legacy.score(world_id,period_id,category,player_id,value,achieved_at,observed_at,revision)
   VALUES(w,per,cat,p,v,t,t,rev)
   ON CONFLICT(world_id,period_id,category,player_id) DO NOTHING;
  IF FOUND THEN RETURN true; END IF;
  SELECT * INTO STRICT old FROM eldoria_legacy.score
   WHERE world_id=w AND period_id=per AND category=cat AND player_id=p FOR UPDATE;
  IF rev<old.revision THEN RETURN false; END IF;
  IF rev=old.revision THEN
   IF old.value<>v OR old.observed_at<>t THEN RAISE EXCEPTION 'snapshot identity conflict'; END IF;
   RETURN false;
  END IF;
  IF t<old.observed_at THEN RAISE EXCEPTION 'snapshot time regression'; END IF;
  UPDATE eldoria_legacy.score SET value=v, achieved_at=CASE WHEN old.value=v THEN old.achieved_at ELSE t END,observed_at=t,revision=rev
   WHERE world_id=w AND period_id=per AND category=cat AND player_id=p;
  RETURN true;
 END $$;

CREATE FUNCTION eldoria_legacy.live_rows(w text,per text,cat text)
 RETURNS TABLE(position bigint,player_id text,display_name text,value bigint,achieved_at timestamptz)
 LANGUAGE sql STABLE SET search_path=pg_catalog AS $$
 WITH win AS (
  SELECT * FROM eldoria_legacy.period q WHERE q.world_id=w AND q.period_id=per AND q.category=cat
 ), hunt AS (
  SELECT r.player_id,sum(r.level)::bigint value,max(r.occurred_at) achieved_at
  FROM eldoria_legacy.receipt r JOIN win q ON q.world_id=r.world_id
  WHERE cat='hunters' AND r.kind='hunt' AND r.occurred_at>=q.starts_at AND r.occurred_at<q.ends_at
  GROUP BY r.player_id
 ), first_discovery AS (
  SELECT r.player_id,r.subject_id,min(r.occurred_at) achieved_at
  FROM eldoria_legacy.receipt r JOIN win q ON q.world_id=r.world_id
  WHERE cat='explorers' AND r.kind='discovery' AND r.occurred_at>=q.starts_at AND r.occurred_at<q.ends_at
  GROUP BY r.player_id,r.subject_id
 ), discovery AS (
  SELECT d.player_id,count(*)::bigint value,max(d.achieved_at) achieved_at
  FROM first_discovery d GROUP BY d.player_id
 ), scores AS (
  SELECT s.player_id,s.value,s.achieved_at FROM eldoria_legacy.score s JOIN win q
  ON q.world_id=s.world_id AND q.period_id=s.period_id AND q.category=s.category
  WHERE cat NOT IN ('hunters','explorers')
  UNION ALL SELECT * FROM hunt UNION ALL SELECT * FROM discovery
 )
 SELECT row_number() OVER(ORDER BY s.value DESC,s.achieved_at,s.player_id COLLATE "C"),
 s.player_id,p.display_name,s.value,s.achieved_at
 FROM scores s JOIN eldoria_legacy.player p ON p.world_id=w AND p.player_id=s.player_id
 ORDER BY 1
 $$;

-- Server-only read API. The caller MUST authorize the requested world against
-- the logged-in player; this shared DB role is not a substitute for user auth.
CREATE FUNCTION eldoria_legacy.read_rows(w text,per text,cat text,lim int,offst int)
 RETURNS TABLE(position bigint,player_id text,display_name text,value bigint,achieved_at timestamptz)
 LANGUAGE plpgsql STABLE SECURITY DEFINER SET search_path=pg_catalog AS $$
 DECLARE closed timestamptz;
 BEGIN
  IF lim IS NULL OR lim NOT BETWEEN 1 AND 100 OR offst IS NULL OR offst<0 OR offst>100000
   THEN RAISE EXCEPTION 'invalid page'; END IF;
  SELECT q.closed_at INTO STRICT closed FROM eldoria_legacy.period q
   WHERE q.world_id=w AND q.period_id=per AND q.category=cat;
  IF closed IS NOT NULL THEN
   RETURN QUERY SELECT f.position,f.player_id,f.display_name,f.value,f.achieved_at
   FROM eldoria_legacy.frozen f WHERE f.world_id=w AND f.period_id=per AND f.category=cat
   ORDER BY f.position LIMIT lim OFFSET offst;
  ELSE
   RETURN QUERY SELECT r.* FROM eldoria_legacy.live_rows(w,per,cat) r ORDER BY r.position LIMIT lim OFFSET offst;
  END IF;
 END $$;

CREATE FUNCTION eldoria_legacy.close_period(w text,per text,cat text)
 RETURNS boolean LANGUAGE plpgsql SECURITY DEFINER SET search_path=pg_catalog AS $$
 DECLARE win eldoria_legacy.period%ROWTYPE;
 BEGIN
  -- Lock receipt writes for a consistent final cut. Schedule outside peak
  -- traffic; a production outbox needs an ingestion watermark before closure.
  LOCK TABLE eldoria_legacy.receipt IN SHARE MODE;
  SELECT * INTO STRICT win FROM eldoria_legacy.period
   WHERE world_id=w AND period_id=per AND category=cat FOR UPDATE;
  IF win.closed_at IS NOT NULL THEN RETURN false; END IF;
  IF win.cadence='permanent' OR win.ends_at>clock_timestamp() THEN RAISE EXCEPTION 'period not closable'; END IF;
  INSERT INTO eldoria_legacy.frozen
   SELECT w,per,cat,r.position,r.player_id,r.display_name,r.value,r.achieved_at
   FROM eldoria_legacy.live_rows(w,per,cat) r;
  INSERT INTO eldoria_legacy.award
   SELECT w,per,cat,f.player_id,'champion:'||cat||':'||per
   FROM eldoria_legacy.frozen f WHERE f.world_id=w AND f.period_id=per AND f.category=cat
     AND f.position=1 AND f.value>0;
  UPDATE eldoria_legacy.period SET closed_at=clock_timestamp()
   WHERE world_id=w AND period_id=per AND category=cat;
  RETURN true;
 END $$;

REVOKE ALL ON ALL TABLES IN SCHEMA eldoria_legacy FROM PUBLIC;
REVOKE ALL ON ALL FUNCTIONS IN SCHEMA eldoria_legacy FROM PUBLIC;
GRANT EXECUTE ON FUNCTION eldoria_legacy.record_receipt(text,text,text,text,text,timestamptz,bigint,int),
 eldoria_legacy.record_score(text,text,text,text,bigint,timestamptz,bigint),
 eldoria_legacy.close_period(text,text,text) TO eldoria_rank_writer;
GRANT EXECUTE ON FUNCTION eldoria_legacy.read_rows(text,text,text,int,int) TO eldoria_rank_reader,eldoria_rank_writer;
COMMIT;
