USE ft20efcore;
ALTER TABLE rtustations
ADD COLUMN LastMeasurementTimestamp datetime default CURRENT_TIMESTAMP;
