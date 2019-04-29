CREATE TABLE ft20efcore.sorfiles2 LIKE ft20efcore.sorfiles;
ALTER TABLE ft20efcore.sorfiles2 DISABLE KEYS;
INSERT INTO ft20efcore.sorfiles2 SELECT * FROM ft20efcore.sorfiles;
ALTER TABLE ft20efcore.sorfiles2 ENABLE KEYS;
DROP TABLE ft20efcore.sorfiles;
ALTER TABLE ft20efcore.sorfiles2 RENAME ft20efcore.sorfiles;
ANALYZE TABLE ft20efcore.sorfiles;