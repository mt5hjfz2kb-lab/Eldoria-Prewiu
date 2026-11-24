\set ON_ERROR_STOP on
CREATE ROLE eldoria_rank_writer NOLOGIN;
CREATE ROLE eldoria_rank_reader NOLOGIN;
CREATE ROLE eldoria_untrusted_client NOLOGIN;
\ir schema.sql
-- All fixtures exist only in ephemeral CI DB.
SET ROLE eldoria_rank_writer;
INSERT INTO eldoria_legacy.player VALUES ('w','a','A'),('w','b','B'),('w','c','C'),('other','a','Other');
INSERT INTO eldoria_legacy.period(world_id,period_id,category,cadence,starts_at,ends_at)
 VALUES ('w','2020-W02','hunters','weekly','2020-01-06 00:00:00+00','2020-01-13 00:00:00+00'),
 ('w','2020-W02','explorers','weekly','2020-01-06 00:00:00+00','2020-01-13 00:00:00+00'),
 ('other','2020-W02','hunters','weekly','2020-01-06 00:00:00+00','2020-01-13 00:00:00+00'),
 ('w','s1','power','season','2020-01-06 00:00:00+00','2020-01-13 00:00:00+00');
DO $$ BEGIN
 IF NOT eldoria_legacy.record_receipt('w','a','h1','hunt','boar','2020-01-06 00:00:00+00',1,5) THEN RAISE EXCEPTION 'new event'; END IF;
 IF eldoria_legacy.record_receipt('w','a','h1','hunt','boar','2020-01-06 00:00:00+00',1,5) THEN RAISE EXCEPTION 'duplicate'; END IF;
 BEGIN
  PERFORM eldoria_legacy.record_receipt('w','a','h1','hunt','boar','2020-01-06 00:00:00+00',1,4);
  RAISE EXCEPTION 'mismatch accepted';
 EXCEPTION WHEN raise_exception THEN IF SQLERRM<>'receipt identity conflict' THEN RAISE; END IF; END;
 PERFORM eldoria_legacy.record_receipt('w','b','h2','hunt','boar','2020-01-07 00:00:00+00',1,5);
 PERFORM eldoria_legacy.record_receipt('w','a','h3','hunt','boar','2020-01-13 00:00:00+00',1,5);
 PERFORM eldoria_legacy.record_receipt('other','a','h1','hunt','boar','2020-01-06 00:00:00+00',1,5);
 PERFORM eldoria_legacy.record_receipt('w','a','d1','discovery','frontier','2020-01-06 00:00:00+00',1,0);
 PERFORM eldoria_legacy.record_receipt('w','a','d2','discovery','frontier','2020-01-07 00:00:00+00',1,0);
 PERFORM eldoria_legacy.record_receipt('w','a','d3','discovery','breach','2020-01-08 00:00:00+00',1,0);
 PERFORM eldoria_legacy.record_receipt('w','b','d1','discovery','frontier','2020-01-06 00:00:00+00',1,0);
 IF NOT eldoria_legacy.record_score('w','s1','power','a',10,'2020-01-06 00:00:00+00',1) THEN RAISE EXCEPTION 'snapshot'; END IF;
 IF NOT eldoria_legacy.record_score('w','s1','power','a',10,'2020-01-08 00:00:00+00',2) THEN RAISE EXCEPTION 'same score new revision'; END IF;
 IF eldoria_legacy.record_score('w','s1','power','a',10,'2020-01-08 00:00:00+00',2) THEN RAISE EXCEPTION 'duplicate unchanged score revision'; END IF;
 IF eldoria_legacy.record_score('w','s1','power','a',99,'2020-01-06 00:00:00+00',1) THEN RAISE EXCEPTION 'stale revision'; END IF;
 PERFORM eldoria_legacy.record_score('w','s1','power','b',10,'2020-01-07 00:00:00+00',1);
 BEGIN
  PERFORM eldoria_legacy.record_score('w','2020-W02','hunters','b',99,'2020-01-06 00:00:00+00',1);
  RAISE EXCEPTION 'client score accepted';
 EXCEPTION WHEN raise_exception THEN IF SQLERRM<>'receipt-derived category' THEN RAISE; END IF; END;
END $$;
RESET ROLE;
SET ROLE eldoria_rank_reader;
DO $$ DECLARE got text[]; scores bigint[]; BEGIN
 SELECT array_agg(player_id ORDER BY rank_position),array_agg(value ORDER BY rank_position)
 INTO got,scores FROM eldoria_legacy.read_rows('w','2020-W02','hunters',100,0);
 IF got IS DISTINCT FROM ARRAY['a','b'] OR scores IS DISTINCT FROM ARRAY[5::bigint,5::bigint] THEN RAISE EXCEPTION 'hunters ordering/window/world'; END IF;
 SELECT array_agg(value ORDER BY rank_position) INTO scores FROM eldoria_legacy.read_rows('w','2020-W02','explorers',100,0);
 IF scores IS DISTINCT FROM ARRAY[2::bigint,1::bigint] THEN RAISE EXCEPTION 'unique discovery'; END IF;
 SELECT array_agg(player_id ORDER BY rank_position) INTO got FROM eldoria_legacy.read_rows('w','s1','power',100,0);
 IF got IS DISTINCT FROM ARRAY['a','b'] THEN RAISE EXCEPTION 'tie timestamp preserved'; END IF;
 IF (SELECT player_id FROM eldoria_legacy.read_rows('w','2020-W02','hunters',1,1)) IS DISTINCT FROM 'b' THEN RAISE EXCEPTION 'pagination'; END IF;
 BEGIN
  PERFORM eldoria_legacy.record_receipt('w','a','illegal','hunt','boar','2020-01-06 00:00:00+00',1,5);
  RAISE EXCEPTION 'reader can write';
 EXCEPTION WHEN insufficient_privilege THEN NULL; END;
 BEGIN
  PERFORM * FROM eldoria_legacy.receipt;
  RAISE EXCEPTION 'reader can read raw receipts';
 EXCEPTION WHEN insufficient_privilege THEN NULL; END;
END $$;
RESET ROLE;
SET ROLE eldoria_untrusted_client;
DO $$ BEGIN
 BEGIN
  PERFORM * FROM eldoria_legacy.read_rows('w','2020-W02','hunters',100,0);
  RAISE EXCEPTION 'unauthenticated client can read';
 EXCEPTION WHEN insufficient_privilege THEN NULL; END;
END $$;
RESET ROLE;
SET ROLE eldoria_rank_writer;
DO $$ BEGIN
 IF NOT eldoria_legacy.close_period('w','2020-W02','hunters') THEN RAISE EXCEPTION 'close'; END IF;
 IF eldoria_legacy.close_period('w','2020-W02','hunters') THEN RAISE EXCEPTION 'duplicate close'; END IF;
 PERFORM eldoria_legacy.record_receipt('w','b','late','hunt','boar','2020-01-08 00:00:00+00',1,5);
 UPDATE eldoria_legacy.player SET display_name='New A' WHERE world_id='w' AND player_id='a';
 IF (SELECT display_name FROM eldoria_legacy.read_rows('w','2020-W02','hunters',1,0)) IS DISTINCT FROM 'A' THEN RAISE EXCEPTION 'frozen changed'; END IF;
 IF (SELECT value FROM eldoria_legacy.read_rows('w','2020-W02','hunters',1,0)) IS DISTINCT FROM 5::bigint THEN RAISE EXCEPTION 'late event changed final'; END IF;
END $$;
RESET ROLE;
DO $$ BEGIN
 IF (SELECT count(*) FROM eldoria_legacy.award)<>1 OR
    (SELECT player_id FROM eldoria_legacy.award) IS DISTINCT FROM 'a' THEN RAISE EXCEPTION 'winner entitlement'; END IF;
END $$;
SELECT 'PASS: PostgreSQL identity, idempotence, rankings, periods, permissions and historical awards' AS result;
