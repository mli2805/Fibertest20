CREATE TABLE ft20efcore.snapshots (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `StreamIdOriginal` char(36) NOT NULL,
  `LastEventNumber` int(11) NOT NULL,
  `LastEventDate` datetime(6) NOT NULL, 
  `Payload` longblob,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8

ALTER TABLE ft20efcore.snapshots CHANGE `AggregateId` `StreamIdOriginal` char(36)