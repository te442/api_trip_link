USE TripLink;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Destination') AND name = 'opening_time')
BEGIN
    ALTER TABLE Destination ADD opening_time TIME NOT NULL
        CONSTRAINT DF_Destination_opening_time DEFAULT '08:00:00';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Destination') AND name = 'closing_time')
BEGIN
    ALTER TABLE Destination ADD closing_time TIME NOT NULL
        CONSTRAINT DF_Destination_closing_time DEFAULT '17:00:00';
END
GO

-- ── מילוי שעות לפי סוג יעד ──────────────────────────────────────────────────

UPDATE Destination
SET opening_time = '08:00:00', closing_time = '16:00:00'
WHERE Name_des LIKE N'%גן לאומי%'
   OR Name_des LIKE N'%שמורת%'
   OR Name_des LIKE N'%פארק נשר%'
   OR Des_id IN (
        1, 10, 17, 18, 19, 20, 30, 38, 42, 50, 51, 57, 58, 61, 69, 75, 78,
        89, 94, 111, 103, 133, 118, 109, 157, 158, 159, 160, 161, 162, 163,
        164, 165, 167, 168, 169, 170
   );

UPDATE Destination
SET opening_time = '06:00:00', closing_time = '19:00:00'
WHERE opening_time = '08:00:00' AND closing_time = '17:00:00';

UPDATE Destination
SET opening_time = '09:00:00', closing_time = '18:00:00'
WHERE Des_id IN (9, 93);

PRINT N'Destination opening hours updated.';
GO
