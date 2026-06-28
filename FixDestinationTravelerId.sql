-- Restore Destination.Traveler_id for 22.06 schema (after 25-26 M:N migration)
USE TripLink;
GO

IF COL_LENGTH('Destination', 'Traveler_id') IS NULL
    ALTER TABLE Destination ADD Traveler_id INT NULL;
GO

IF COL_LENGTH('Destination', 'Traveler_id') IS NOT NULL
BEGIN
    UPDATE d
    SET d.Traveler_id = t.Traveler_id
    FROM Destination d
    INNER JOIN (
        SELECT Des_id, MIN(Traveler_id) AS Traveler_id
        FROM Travelers_of_Destination
        GROUP BY Des_id
    ) t ON t.Des_id = d.Des_id;

    IF NOT EXISTS (
        SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Destination_TypeTraveler'
    )
        ALTER TABLE Destination
            ADD CONSTRAINT FK_Destination_TypeTraveler
            FOREIGN KEY (Traveler_id) REFERENCES TypeTraveler(Traveler_id);

    PRINT 'Traveler_id column restored on Destination.';
END
GO

SELECT Des_id, Name_des, Traveler_id FROM Destination;
GO
