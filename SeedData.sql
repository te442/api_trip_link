-- ============================================================
--  SeedData.sql  -  נתוני דמה לפרויקט TripLink
-- ============================================================

-- השבתת FK בעייתיים
ALTER TABLE Destination NOCHECK CONSTRAINT FK_Destination_station_to_destination;
ALTER TABLE Categories_of_Destination NOCHECK CONSTRAINT FK_Categories_of_Destination_Categories;
ALTER TABLE Des_of_trip NOCHECK CONSTRAINT FK_Des_of_trip_Destination;
ALTER TABLE Categories_to_trip NOCHECK CONSTRAINT FK_Categories_to_trip_Categories;
GO

-- ────────────────────────────────────────────────────────────
-- 1. Difficulty_level
-- ────────────────────────────────────────────────────────────
INSERT INTO Difficulty_level (level_id, level_type) VALUES
(1, N'קל'),
(2, N'בינוני'),
(3, N'קשה');

-- ────────────────────────────────────────────────────────────
-- 2. TypeTraveler
-- ────────────────────────────────────────────────────────────
INSERT INTO TypeTraveler (Traveler_id, TypeTraveler) VALUES
(1, 'משפחות'),
(2, 'זוגות'),
(3, 'יחידים'),
(4, 'קבוצות');

-- ────────────────────────────────────────────────────────────
-- 3. Categories  (IDENTITY: categories_id)
-- ────────────────────────────────────────────────────────────
SET IDENTITY_INSERT Categories ON;
INSERT INTO Categories (categories_id, categories_name) VALUES
(1, N'טבע      '),
(2, N'היסטוריה '),
(3, N'חוף      '),
(4, N'עיר      '),
(5, N'הרים     ');
SET IDENTITY_INSERT Categories OFF;

-- ────────────────────────────────────────────────────────────
-- 4. FeatureTypes  (IDENTITY: Feature_id)
-- ────────────────────────────────────────────────────────────
SET IDENTITY_INSERT FeatureTypes ON;
INSERT INTO FeatureTypes (Feature_id, Feature) VALUES
(1, 'חניה חינם'),
(2, 'נגיש לנכים'),
(3, 'כניסה חינם'),
(4, 'מסעדות'),
(5, 'לילדים');
SET IDENTITY_INSERT FeatureTypes OFF;

-- ────────────────────────────────────────────────────────────
-- 5. Agency  (IDENTITY: agency_id)
-- ────────────────────────────────────────────────────────────
SET IDENTITY_INSERT Agency ON;
INSERT INTO Agency (agency_id, external_agency_id, agency_name, phon) VALUES
(1, 101, 'אגד',          '03-6948888'),
(2, 102, 'נתיב אקספרס', '04-8545555'),
(3, 103, 'דן',           '03-6394444'),
(4, 104, 'קווים',        '03-5614444'),
(5, 105, 'מטרופולין',    '04-6464646');
SET IDENTITY_INSERT Agency OFF;

-- ────────────────────────────────────────────────────────────
-- 6. Station
-- ────────────────────────────────────────────────────────────
INSERT INTO Station (Station_num, Statoin_code, Station_name, area, lat, lon, government_stop_id) VALUES
(1001, 'S-001', N'תחנה מרכזית באר שבע',  N'דרום',  31.2430, 34.7925, 'GOV-1001'),
(1002, 'S-002', N'תחנת מצדה',            N'דרום',  31.3156, 35.3536, 'GOV-1002'),
(1003, 'S-003', N'תחנת עין בוקק',        N'דרום',  31.1980, 35.3620, 'GOV-1003'),
(1004, 'S-004', N'תחנת מכתש רמון',       N'דרום',  30.6050, 34.8020, 'GOV-1004'),
(1005, 'S-005', N'תחנה מרכזית טבריה',    N'צפון',  32.7940, 35.5310, 'GOV-1005'),
(1006, 'S-006', N'תחנת נחל עמוד',        N'צפון',  32.9100, 35.4800, 'GOV-1006'),
(1007, 'S-007', N'תחנת קיסריה',          N'מרכז',  32.5000, 34.9060, 'GOV-1007'),
(1008, 'S-008', N'תחנת נמל יפו',         N'מרכז',  32.0530, 34.7520, 'GOV-1008'),
(1009, 'S-009', N'תחנת שער יפו',         N'מרכז',  31.7760, 35.2290, 'GOV-1009'),
(1010, 'S-010', N'תחנת עין גדי',         N'דרום',  31.4620, 35.3890, 'GOV-1010');

-- ────────────────────────────────────────────────────────────
-- 7. station_to_destination
-- ────────────────────────────────────────────────────────────
INSERT INTO station_to_destination
    (Des_id, Station_num, Direction_Type, Walking_distance, [Walking time], [Walking instructions], level_id, Feature_id) VALUES
(1,  1002, 'הלוך', 0.3, '00:05', 'לך ישר מהתחנה לכניסה הראשית',        2, 1),
(2,  1003, 'הלוך', 0.5, '00:08', 'פנה שמאלה מהתחנה לכיוון החוף',       1, 3),
(3,  1004, 'הלוך', 1.0, '00:15', 'עקוב אחרי השלטים למרכז המבקרים',     2, 1),
(4,  1010, 'הלוך', 0.8, '00:12', 'ירידה בשביל המסומן לנחל',            2, 2),
(5,  1005, 'הלוך', 0.4, '00:06', 'לך לכיוון הטיילת',                   1, 3),
(6,  1006, 'הלוך', 1.2, '00:18', 'עקוב אחרי הסימון הכחול-לבן',         2, 1),
(8,  1007, 'הלוך', 0.6, '00:09', 'כניסה לאתר מהצד הצפוני',             1, 1),
(9,  1008, 'הלוך', 0.2, '00:03', 'הנמל נמצא ממש ליד התחנה',            1, 3),
(10, 1009, 'הלוך', 0.5, '00:07', 'עבור את שער יפו ופנה ימינה',         1, 2);

-- ────────────────────────────────────────────────────────────
-- 8. Destination  (IDENTITY: Des_id)
-- ────────────────────────────────────────────────────────────
SET IDENTITY_INSERT Destination ON;
INSERT INTO Destination (Des_id, Name_des, Rregion, level_id, Traveler_id, Time_des) VALUES
(1,  'מצדה',               'דרום',  2, 3, '03:00'),
(2,  'ים המלח - עין בוקק', 'דרום',  1, 1, '02:30'),
(3,  'מכתש רמון',          'דרום',  2, 3, '04:00'),
(4,  'נחל דוד - עין גדי',  'דרום',  2, 1, '03:00'),
(5,  'הכנרת - טבריה',      'צפון',  1, 1, '02:00'),
(6,  'נחל עמוד',           'צפון',  2, 3, '03:30'),
(7,  'הר מירון',           'צפון',  3, 3, '05:00'),
(8,  'קיסריה',             'מרכז',  1, 2, '02:30'),
(9,  'נמל יפו',            'מרכז',  1, 2, '02:00'),
(10, 'ירושלים - עיר עתיקה','מרכז',  1, 4, '04:00');
SET IDENTITY_INSERT Destination OFF;

UPDATE Destination SET opening_time = '08:00:00', closing_time = '16:00:00' WHERE Des_id IN (1, 4, 8, 10);
UPDATE Destination SET opening_time = '06:00:00', closing_time = '19:00:00' WHERE Des_id IN (2, 3, 6, 7);
UPDATE Destination SET opening_time = '09:00:00', closing_time = '18:00:00' WHERE Des_id IN (5, 9);

-- ────────────────────────────────────────────────────────────
-- 9. Categories_of_Destination
-- ────────────────────────────────────────────────────────────
INSERT INTO Categories_of_Destination (Categories_id, Des_id) VALUES
(2,  1),
(1,  2), (3,  2),
(1,  3), (5,  3),
(1,  4),
(1,  5), (3,  5),
(1,  6),
(5,  7),
(2,  8), (3,  8),
(2,  9), (4,  9),
(2, 10), (4, 10);

-- ────────────────────────────────────────────────────────────
-- 10. Bus  (IDENTITY: bus_id)
-- ────────────────────────────────────────────────────────────
SET IDENTITY_INSERT Bus ON;
INSERT INTO Bus (bus_id, Bus_code, Bus_number, agency_id, Direction, government_route_id) VALUES
(1, 'B-060',  60,  1, N'באר שבע - מצדה',      'ROUTE-060'),
(2, 'B-384', 384,  1, N'באר שבע - ים המלח',   'ROUTE-384'),
(3, 'B-392', 392,  1, N'באר שבע - מכתש רמון', 'ROUTE-392'),
(4, 'B-486', 486,  2, N'טבריה - כנרת',        'ROUTE-486'),
(5, 'B-361', 361,  2, N'עכו - נחל עמוד',      'ROUTE-361'),
(6, 'B-921', 921,  3, N'חדרה - קיסריה',       'ROUTE-921'),
(7, 'B-010',  10,  4, N'תל אביב - יפו',       'ROUTE-010'),
(8, 'B-480', 480,  5, N'ירושלים - עיר עתיקה', 'ROUTE-480');
SET IDENTITY_INSERT Bus OFF;

-- ────────────────────────────────────────────────────────────
-- 11. bus_station
-- ────────────────────────────────────────────────────────────
INSERT INTO bus_station (bus_id, station_id, stop_sequence) VALUES
(1, 1001, 1), (1, 1002, 2),
(2, 1001, 1), (2, 1003, 2),
(3, 1001, 1), (3, 1004, 2),
(4, 1005, 1),
(5, 1006, 1),
(6, 1007, 1),
(7, 1008, 1),
(8, 1009, 1);

-- ────────────────────────────────────────────────────────────
-- 12. Users
-- ────────────────────────────────────────────────────────────
INSERT INTO Users (user_id, FullName, Phon) VALUES
('U001', 'ישראל ישראלי', '050-1111111'),
('U002', 'שרה כהן',      '052-2222222'),
('U003', 'דוד לוי',      '054-3333333');

-- ────────────────────────────────────────────────────────────
-- 13. Trip  (IDENTITY: Trip_id)
-- ────────────────────────────────────────────────────────────
SET IDENTITY_INSERT Trip ON;
INSERT INTO Trip (Trip_id, Trip_name, user_id, Trip_Date, Address_start, Start_time, End_time, Trip_cost) VALUES
(1, 'טיול דרום קלאסי',   'U001', '2025-08-10', 'באר שבע', '08:00', '18:00', 0),
(2, 'סוף שבוע בצפון',    'U002', '2025-09-05', 'טבריה',   '09:00', '17:00', 0),
(3, 'יום בירושלים ויפו', 'U003', '2025-07-20', 'ירושלים', '08:30', '19:00', 0);
SET IDENTITY_INSERT Trip OFF;

-- ────────────────────────────────────────────────────────────
-- 14. Des_of_trip
-- ────────────────────────────────────────────────────────────
INSERT INTO Des_of_trip (Trip_id, Des_id, visit_number) VALUES
(1, 1, 1), (1, 2, 2), (1, 4, 3),
(2, 5, 1), (2, 6, 2),
(3, 10, 1), (3, 9, 2);

-- ────────────────────────────────────────────────────────────
-- 15. Categories_to_trip
-- ────────────────────────────────────────────────────────────
INSERT INTO Categories_to_trip (categories_id, trip_id) VALUES
(1, 1), (2, 1),
(1, 2), (5, 2),
(2, 3), (4, 3);

-- ────────────────────────────────────────────────────────────
-- 16. Feature_to_trip
-- ────────────────────────────────────────────────────────────
INSERT INTO Feature_to_trip (Feature_id, trip_id) VALUES
(1, 1), (3, 1),
(2, 2),
(5, 3);

-- ────────────────────────────────────────────────────────────
-- 17. Nature_trip  (IDENTITY: natureTrip_id)
-- ────────────────────────────────────────────────────────────
SET IDENTITY_INSERT Nature_trip ON;
INSERT INTO Nature_trip (natureTrip_id, trip_id, Num_break, Min_num_des, Max_num_des, level_id) VALUES
(1, 1, 1, 2, 4, 2),
(2, 2, 0, 1, 3, 1),
(3, 3, 1, 2, 3, 1);
SET IDENTITY_INSERT Nature_trip OFF;

-- הפעלה מחדש של ה-FK
ALTER TABLE Destination CHECK CONSTRAINT FK_Destination_station_to_destination;
ALTER TABLE Categories_of_Destination CHECK CONSTRAINT FK_Categories_of_Destination_Categories;
ALTER TABLE Des_of_trip CHECK CONSTRAINT FK_Des_of_trip_Destination;
ALTER TABLE Categories_to_trip CHECK CONSTRAINT FK_Categories_to_trip_Categories;

PRINT 'Seed data inserted successfully!';
GO
