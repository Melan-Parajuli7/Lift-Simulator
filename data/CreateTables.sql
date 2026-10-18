-- OPTIONAL: the app creates all of this automatically on first start.
-- Run this in SSMS only if you want to create the database yourself.

IF DB_ID('ElevatorDB') IS NULL
    CREATE DATABASE ElevatorDB;
GO

USE ElevatorDB;
GO

IF OBJECT_ID('dbo.LiftLogs', 'U') IS NULL
CREATE TABLE dbo.LiftLogs
(
    Id         INT IDENTITY(1,1) PRIMARY KEY,
    LogDate    DATE          NOT NULL,
    LogTime    TIME(0)       NOT NULL,
    ActionText NVARCHAR(200) NOT NULL
);
GO

SELECT * FROM dbo.LiftLogs;
