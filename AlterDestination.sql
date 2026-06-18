USE TripLink;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Destination') AND name = 'lat')
BEGIN
    ALTER TABLE Destination ADD lat DECIMAL(9,6) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Destination') AND name = 'lon')
BEGIN
    ALTER TABLE Destination ADD lon DECIMAL(9,6) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Destination') AND name = 'image_url')
BEGIN
    ALTER TABLE Destination ADD image_url NVARCHAR(1000) NULL;
END
GO

ALTER TABLE Destination ALTER COLUMN Name_des NVARCHAR(100) NOT NULL;
GO

ALTER TABLE Destination ALTER COLUMN Rregion NVARCHAR(50) NOT NULL;
GO

ALTER TABLE Destination ALTER COLUMN image_url NVARCHAR(1000) NULL;
GO

ALTER TABLE Categories ALTER COLUMN categories_name NVARCHAR(50) NOT NULL;
GO

ALTER TABLE Trip ALTER COLUMN Trip_name NVARCHAR(100) NOT NULL;
GO

ALTER TABLE Nature_trip ALTER COLUMN Region NVARCHAR(50) NOT NULL;
GO

PRINT N'Destination schema updated.';
GO
