-- ============================================================
--  AlterNatureTrip.sql
--  הוספת עמודת Region לטבלת Nature_trip
-- ============================================================
USE TripLink;
GO

-- הוספת עמודת Region אם לא קיימת
IF NOT EXISTS (
    SELECT 1
    FROM   sys.columns
    WHERE  object_id = OBJECT_ID(N'Nature_trip')
      AND  name      = N'Region'
)
BEGIN
    ALTER TABLE Nature_trip
        ADD Region NVARCHAR(50) NULL;
    PRINT 'Column Region added to Nature_trip.';
END
ELSE
BEGIN
    PRINT 'Column Region already exists in Nature_trip.';
END
GO
