USE InfralabsDB;
GO

DROP TABLE positions;
GO

CREATE TABLE positions (
    Pos_name NVARCHAR(64) PRIMARY KEY NOT NULL,
    Lat FLOAT NOT NULL,
    Lon FLOAT NOT NULL
);
GO