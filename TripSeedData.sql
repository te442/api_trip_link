USE TripLink;
GO

ALTER TABLE Categories_of_Destination NOCHECK CONSTRAINT FK_Categories_of_Destination_Categories;
ALTER TABLE Categories_to_trip NOCHECK CONSTRAINT FK_Categories_to_trip_Categories;
ALTER TABLE Des_of_trip NOCHECK CONSTRAINT FK_Des_of_trip_Destination;
GO

INSERT INTO Difficulty_level (level_id, level_type) VALUES
(1, N'קל'),
(2, N'בינוני'),
(3, N'קשה');

INSERT INTO TypeTraveler (Traveler_id, TypeTraveler) VALUES
(1, N'מבוגרים'),
(2, N'בני נוער'),
(3, N'זוגות'),
(4, N'ילדים ועגלות'),
(5, N'קבוצות');

SET IDENTITY_INSERT Categories ON;
INSERT INTO Categories (categories_id, categories_name) VALUES
(1, N'שמורת טבע'),
(2, N'מסלולי הליכה יבש'),
(3, N'מסלול רטוב'),
(4, N'אתר היסטורי'),
(5, N'נקודת עניין'),
(6, N'תצפית'),
(7, N'מסלול טיפוס');
SET IDENTITY_INSERT Categories OFF;

SET IDENTITY_INSERT FeatureTypes ON;
INSERT INTO FeatureTypes (Feature_id, Feature) VALUES
(1, N'כניסה חינם'),
(2, N'נגישות לנכים ועגלות'),
(3, N'שירותים'),
(4, N'מקלט צל פיקניק'),
(5, N'כניסה בתשלום');
SET IDENTITY_INSERT FeatureTypes OFF;

SET IDENTITY_INSERT Destination ON;
INSERT INTO Destination (Des_id, Name_des, Rregion, level_id, Traveler_id, Time_des, lat, lon, image_url) VALUES
(1, N'מצדה', N'ים המלח ומדבר יהודה', 2, 3, '03:30', 31.3157, 35.3535, 'https://upload.wikimedia.org/wikipedia/commons/0/0c/Masada_from_above.jpg'),
(2, N'שמורת עין גדי', N'ים המלח ומדבר יהודה', 2, 1, '03:00', 31.453, 35.387, 'https://upload.wikimedia.org/wikipedia/commons/5/5f/Ein_Gedi_Reserve.jpg'),
(3, N'נחל דוד', N'ים המלח ומדבר יהודה', 2, 4, '02:30', 31.4579, 35.3939, 'https://upload.wikimedia.org/wikipedia/commons/9/92/Nahal_David_waterfall.jpg'),
(4, N'נחל ערוגות', N'ים המלח ומדבר יהודה', 3, 2, '04:00', 31.4458, 35.3847, 'https://upload.wikimedia.org/wikipedia/commons/2/26/Nahal_Arugot.jpg'),
(5, N'עין בוקק', N'ים המלח ומדבר יהודה', 1, 1, '02:00', 31.2003, 35.3636, 'https://upload.wikimedia.org/wikipedia/commons/8/8d/Ein_Bokek_stream.jpg'),
(6, N'נחל אוג', N'ים המלח ומדבר יהודה', 3, 2, '04:30', 31.7969, 35.3461, 'https://upload.wikimedia.org/wikipedia/commons/d/d0/Nahal_Og.jpg'),
(7, N'עין פרת', N'ירושלים והרי יהודה', 2, 1, '03:00', 31.8264, 35.3178, 'https://upload.wikimedia.org/wikipedia/commons/0/0f/Ein_Prat.jpg'),
(8, N'עין מבוע', N'ירושלים והרי יהודה', 1, 4, '01:30', 31.8561, 35.3006, 'https://upload.wikimedia.org/wikipedia/commons/a/a8/Ein_Mabua.jpg'),
(9, N'עין פואר', N'ירושלים והרי יהודה', 2, 2, '02:30', 31.8081, 35.2825, 'https://upload.wikimedia.org/wikipedia/commons/7/72/Ein_Fawar.jpg'),
(10, N'מצפה ים המלח מצפה שלם', N'ים המלח ומדבר יהודה', 1, 5, '01:00', 31.5714, 35.3972, 'https://upload.wikimedia.org/wikipedia/commons/4/45/Dead_Sea_viewpoint.jpg'),
(11, N'מכתש רמון', N'הנגב', 2, 3, '04:00', 30.5786, 34.8086, 'https://upload.wikimedia.org/wikipedia/commons/1/13/Makhtesh_Ramon_panorama.jpg'),
(12, N'הר גמל', N'הנגב', 3, 2, '03:30', 30.6072, 34.8047, 'https://upload.wikimedia.org/wikipedia/commons/7/71/Mount_Camel_Mitzpe_Ramon.jpg'),
(13, N'חניון בארות מסלול צבעים', N'הנגב', 2, 1, '03:00', 30.6219, 34.8625, 'https://upload.wikimedia.org/wikipedia/commons/b/b6/Be_erot_campground.jpg'),
(14, N'עין עקב', N'הנגב', 3, 2, '05:30', 30.8531, 34.7722, 'https://upload.wikimedia.org/wikipedia/commons/0/08/Ein_Akev_pool.jpg'),
(15, N'עין שביב', N'הנגב', 2, 5, '03:30', 30.875, 34.7892, 'https://upload.wikimedia.org/wikipedia/commons/1/11/Ein_Sheviv.jpg'),
(16, N'נחל חווארים', N'הנגב', 1, 4, '02:00', 30.9306, 34.7731, 'https://upload.wikimedia.org/wikipedia/commons/9/98/Nahal_Havarim.jpg'),
(17, N'עבדת', N'הנגב', 1, 3, '02:00', 30.7928, 34.7744, 'https://upload.wikimedia.org/wikipedia/commons/2/2e/Avdat_ruins.jpg'),
(18, N'ממשית', N'הנגב', 1, 3, '02:00', 31.0247, 35.0556, 'https://upload.wikimedia.org/wikipedia/commons/6/63/Mamshit.jpg'),
(19, N'שבטה', N'הנגב', 1, 5, '02:00', 30.8917, 34.6319, 'https://upload.wikimedia.org/wikipedia/commons/e/e3/Shivta.jpg'),
(20, N'חלוצה', N'הנגב', 1, 5, '01:30', 31.1142, 34.5167, 'https://upload.wikimedia.org/wikipedia/commons/8/8a/Haluza_site.jpg'),
(21, N'נחל צין', N'ים המלח ומדבר יהודה', 3, 2, '05:00', 30.8247, 34.7911, 'https://upload.wikimedia.org/wikipedia/commons/e/e1/Nahal_Zin.jpg'),
(22, N'עין עבדת', N'הנגב', 2, 1, '03:00', 30.8228, 34.7786, 'https://upload.wikimedia.org/wikipedia/commons/c/c6/Ein_Avdat_canyon.jpg'),
(23, N'מצפה רמון טיילת המכתש', N'הנגב', 1, 5, '01:30', 30.6108, 34.8019, 'https://upload.wikimedia.org/wikipedia/commons/a/aa/Mitzpe_Ramon_promenade.jpg'),
(24, N'נחל ברק', N'הערבה', 3, 2, '04:30', 30.0603, 35.0508, 'https://upload.wikimedia.org/wikipedia/commons/5/55/Nahal_Barak.jpg'),
(25, N'נחל צפית', N'הנגב', 3, 2, '04:30', 30.9586, 35.2253, 'https://upload.wikimedia.org/wikipedia/commons/f/f4/Nahal_Tzafit.jpg'),
(26, N'שמורת הבניאס', N'רמת הגולן', 1, 1, '02:00', 33.2478, 35.6939, 'https://upload.wikimedia.org/wikipedia/commons/0/0b/Banias_Reserve.jpg'),
(27, N'מפל הבניאס', N'רמת הגולן', 1, 4, '01:30', 33.2531, 35.6925, 'https://upload.wikimedia.org/wikipedia/commons/a/a2/Banias_falls.jpg'),
(28, N'שמורת תל דן', N'הגליל העליון', 1, 1, '02:00', 33.2494, 35.6517, 'https://upload.wikimedia.org/wikipedia/commons/1/10/Tel_Dan_stream.jpg'),
(29, N'נחל שניר', N'הגליל העליון', 2, 1, '03:00', 33.2247, 35.6044, 'https://upload.wikimedia.org/wikipedia/commons/3/35/Nahal_Snir.jpg'),
(30, N'מבצר נמרוד', N'רמת הגולן', 2, 3, '02:30', 33.2536, 35.7167, 'https://upload.wikimedia.org/wikipedia/commons/7/7f/Nimrod_fortress_view.jpg'),
(31, N'שמורת החולה', N'הגליל העליון', 1, 1, '02:00', 33.0897, 35.5922, 'https://upload.wikimedia.org/wikipedia/commons/6/67/Hula_nature_reserve.jpg'),
(32, N'אגמון החולה', N'הגליל העליון', 1, 5, '02:30', 33.0917, 35.5983, 'https://upload.wikimedia.org/wikipedia/commons/2/24/Agamon_Hula.jpg'),
(33, N'הר מירון שביל הפסגה', N'הגליל העליון', 2, 1, '03:00', 33.0058, 35.4144, 'https://upload.wikimedia.org/wikipedia/commons/5/55/Mount_Meron_trail.jpg'),
(34, N'נחל עמוד', N'הגליל העליון', 3, 2, '04:30', 32.9028, 35.4947, 'https://upload.wikimedia.org/wikipedia/commons/4/4a/Nahal_Amud_lower.jpg'),
(35, N'הר ארבל', N'הגליל התחתון', 3, 3, '03:30', 32.8097, 35.4978, 'https://upload.wikimedia.org/wikipedia/commons/3/3a/Mount_Arbel_cliffs.jpg'),
(36, N'מצוק הארבל תצפית', N'הגליל התחתון', 2, 5, '01:30', 32.8122, 35.4964, 'https://upload.wikimedia.org/wikipedia/commons/e/e5/Arbel_view.jpg'),
(37, N'נחל כזיב', N'הגליל העליון', 2, 2, '04:00', 33.0372, 35.2378, 'https://upload.wikimedia.org/wikipedia/commons/6/65/Nahal_Kziv.jpg'),
(38, N'מבצר יחיעם', N'הגליל העליון', 1, 3, '01:45', 33.0411, 35.2489, 'https://upload.wikimedia.org/wikipedia/commons/b/b0/Yehiam_fortress.jpg'),
(39, N'יער ביריה תצפית', N'הגליל העליון', 1, 5, '01:30', 32.9897, 35.5042, 'https://upload.wikimedia.org/wikipedia/commons/4/43/Biriya_forest.jpg'),
(40, N'נחל תבור', N'הגליל התחתון', 2, 2, '03:30', 32.6811, 35.4433, 'https://upload.wikimedia.org/wikipedia/commons/9/9f/Nahal_Tavor.jpg'),
(41, N'הר תבור שביל הפסגה', N'הגליל התחתון', 2, 1, '02:30', 32.6886, 35.3906, 'https://upload.wikimedia.org/wikipedia/commons/5/58/Mount_Tabor_trail.jpg'),
(42, N'שמורת גמלא', N'רמת הגולן', 2, 1, '03:00', 32.9136, 35.7428, 'https://upload.wikimedia.org/wikipedia/commons/2/2f/Gamla_reserve.jpg'),
(43, N'נחל יהודיה', N'רמת הגולן', 3, 2, '04:30', 32.9383, 35.7258, 'https://upload.wikimedia.org/wikipedia/commons/8/80/Nahal_Yehudiya.jpg'),
(44, N'בריכת המשושים', N'רמת הגולן', 2, 2, '03:00', 32.9342, 35.6697, 'https://upload.wikimedia.org/wikipedia/commons/0/0f/Hexagon_pool.jpg'),
(45, N'שמורת עין אפק', N'מישור החוף', 1, 4, '01:30', 32.8386, 35.1086, 'https://upload.wikimedia.org/wikipedia/commons/7/7a/Ein_Afek.jpg'),
(46, N'נחל התנינים', N'מישור החוף', 1, 1, '02:00', 32.5414, 34.9158, 'https://upload.wikimedia.org/wikipedia/commons/8/87/Nahal_Taninim.jpg'),
(47, N'יער בן שמן', N'השומרון', 1, 5, '02:00', 31.9497, 34.9883, 'https://upload.wikimedia.org/wikipedia/commons/9/9d/Ben_Shemen_forest.jpg'),
(48, N'פארק קנדה שביל המעיינות', N'השומרון', 1, 4, '02:30', 31.8408, 34.9906, 'https://upload.wikimedia.org/wikipedia/commons/1/10/Canada_Park_trail.jpg'),
(49, N'מצפה מודיעין', N'השומרון', 1, 5, '01:15', 31.9192, 35.0103, 'https://upload.wikimedia.org/wikipedia/commons/f/f9/Mitzpe_Modiin.jpg'),
(50, N'שביל קיסריה ההיסטורי', N'מישור החוף', 1, 3, '02:30', 32.5006, 34.8903, 'https://upload.wikimedia.org/wikipedia/commons/3/37/Caesarea_trail.jpg'),
(51, N'תל גזר', N'מישור החוף', 1, 3, '02:00', 31.8586, 34.9144, 'https://upload.wikimedia.org/wikipedia/commons/c/c8/Tel_Gezer.jpg'),
(52, N'יער חורשים', N'מישור החוף', 1, 4, '02:00', 32.1422, 34.9797, 'https://upload.wikimedia.org/wikipedia/commons/4/4e/Horshim_forest.jpg'),
(53, N'סתף', N'ירושלים והרי יהודה', 2, 1, '03:00', 31.7714, 35.1294, 'https://upload.wikimedia.org/wikipedia/commons/0/07/Sataf_springs.jpg'),
(54, N'עין חנדק', N'ירושלים והרי יהודה', 2, 2, '02:30', 31.7719, 35.1369, 'https://upload.wikimedia.org/wikipedia/commons/2/21/Ein_Handak.jpg'),
(55, N'נחל קטלב', N'ירושלים והרי יהודה', 2, 2, '03:30', 31.7417, 35.1169, 'https://upload.wikimedia.org/wikipedia/commons/9/94/Nahal_Ktalav.jpg'),
(56, N'עין לבן', N'ירושלים והרי יהודה', 1, 4, '01:45', 31.7447, 35.1942, 'https://upload.wikimedia.org/wikipedia/commons/b/b8/Ein_Lavan.jpg'),
(57, N'עיר דוד שביל המעיין', N'ירושלים והרי יהודה', 1, 3, '02:30', 31.7714, 35.2361, 'https://upload.wikimedia.org/wikipedia/commons/5/57/City_of_David_tunnel.jpg'),
(58, N'טיילת החומות', N'ירושלים והרי יהודה', 1, 3, '02:00', 31.7767, 35.2328, 'https://upload.wikimedia.org/wikipedia/commons/6/67/Jerusalem_ramparts_walk.jpg'),
(59, N'יער ירושלים תצפית', N'ירושלים והרי יהודה', 1, 5, '01:30', 31.7892, 35.1914, 'https://upload.wikimedia.org/wikipedia/commons/8/85/Jerusalem_forest_view.jpg'),
(60, N'שמורת נחל חלילים', N'ירושלים והרי יהודה', 1, 4, '02:00', 31.8117, 35.1672, 'https://upload.wikimedia.org/wikipedia/commons/a/ad/Nahal_Halilim.jpg'),
-- הנגב והערבה – יעדים נוספים
(61, N'עמודי שלמה', N'הערבה', 1, 3, '01:30', 29.7872, 34.9847, 'https://upload.wikimedia.org/wikipedia/commons/a/a2/Timna-park-solomons-pillars-a.jpg'),
(62, N'מכתש הקטן', N'הנגב', 2, 2, '04:00', 30.9061, 34.9292, 'https://upload.wikimedia.org/wikipedia/commons/1/13/Makhtesh_Ramon_panorama.jpg'),
(63, N'קניון אדום', N'הערבה', 2, 2, '02:30', 29.6811, 34.8758, 'https://upload.wikimedia.org/wikipedia/commons/d/db/Red-Canyon-200910-5013.jpg'),
(64, N'נחל בוקר', N'הנגב', 2, 2, '03:30', 30.8722, 34.7872, 'https://upload.wikimedia.org/wikipedia/commons/f/f1/Nahal_HaBoqer_002.jpg'),
(65, N'הר כרכום', N'הנגב', 3, 1, '05:00', 30.6342, 34.845, 'https://upload.wikimedia.org/wikipedia/commons/8/87/Karkom_Gulch%2C_Mount_Karkom%2C_Negev%2C_Israel_%D7%A0%D7%97%D7%9C_%D7%9B%D7%A8%D7%9B%D7%95%D7%9D%2C_%D7%94%D7%A8_%D7%9B%D7%A8%D7%9B%D7%95%D7%9D%2C_%D7%94%D7%A0%D7%92%D7%91_-_panoramio_%281%29.jpg'),
(66, N'בורות מצוקה', N'הנגב', 2, 3, '03:00', 30.7083, 34.9383, 'https://upload.wikimedia.org/wikipedia/commons/b/b6/Be_erot_campground.jpg'),
(67, N'עין ירקעם', N'הנגב', 2, 4, '03:00', 31.0822, 35.0761, 'https://upload.wikimedia.org/wikipedia/commons/4/4f/Ein_Yorke%27am_%28997009157668805171%29.jpg'),
-- הגליל – יעדים נוספים
(68, N'נחל השופט', N'חיפה והכרמל', 1, 4, '02:00', 32.5831, 35.0642, 'https://upload.wikimedia.org/wikipedia/commons/7/7f/Nahal_Hashofet.jpg'),
(69, N'ראש הנקרה', N'הגליל העליון', 1, 4, '01:30', 33.0892, 35.1042, 'https://upload.wikimedia.org/wikipedia/commons/6/6d/Rosh_Hanikra_grottoes.jpg'),
(70, N'חורשת טל', N'הגליל העליון', 1, 4, '02:00', 33.21, 35.64, 'https://upload.wikimedia.org/wikipedia/commons/6/68/Hurshat_Tal_%28997009325230505171.jpg'),
(71, N'נחל עיון – מפלים', N'הגליל העליון', 2, 2, '03:00', 33.265, 35.545, 'https://upload.wikimedia.org/wikipedia/commons/4/47/Tanur_waterfall.jpg'),
(72, N'קרני חיטין', N'הגליל התחתון', 2, 3, '02:30', 32.8061, 35.4481, 'https://upload.wikimedia.org/wikipedia/commons/a/ab/Horns_of_Hattin.JPG'),
(73, N'תל חצור', N'הגליל העליון', 1, 5, '02:00', 32.9972, 35.5681, 'https://upload.wikimedia.org/wikipedia/commons/9/9a/Tel_Hazor2.jpg'),
(74, N'נחל איון', N'הגליל העליון', 1, 4, '02:00', 33.28, 35.57, 'https://upload.wikimedia.org/wikipedia/commons/a/a7/Ayun_Stream.jpg'),
(75, N'סוסיתא', N'הגליל העליון', 2, 3, '03:00', 32.7781, 35.6472, 'https://upload.wikimedia.org/wikipedia/commons/d/d0/Susita-761801.jpg'),
(76, N'נחל דישון', N'הגליל העליון', 2, 2, '04:00', 32.95, 35.42, 'https://upload.wikimedia.org/wikipedia/commons/7/79/Nahal_Dishon_001.jpg'),
(77, N'נחל בצת', N'הגליל העליון', 2, 2, '03:30', 33.05, 35.13, 'https://upload.wikimedia.org/wikipedia/commons/e/e5/Nahal_Betzet.jpg'),
(78, N'מונפורט', N'הגליל העליון', 2, 3, '02:30', 32.9961, 35.2892, 'https://upload.wikimedia.org/wikipedia/commons/8/8f/Montfort053.jpg'),
-- חיפה והכרמל
(79, N'נחל אורן – עמק עירון', N'חיפה והכרמל', 2, 2, '03:00', 32.7042, 35.0261, 'https://upload.wikimedia.org/wikipedia/commons/6/6e/Nahal_Oren_01.jpg'),
(80, N'בחן הכרמל – שווייץ הקטנה', N'חיפה והכרמל', 1, 4, '02:00', 32.75, 35.0472, 'https://upload.wikimedia.org/wikipedia/commons/5/5c/Switzerland_trail_in_Carmel_mountains.jpg'),
(81, N'מצפה דליית אל-כרמל', N'חיפה והכרמל', 1, 5, '01:30', 32.6942, 35.0561, 'https://upload.wikimedia.org/wikipedia/commons/1/1a/Daliat_el-Karmel.jpg'),
(82, N'נחל כלח', N'חיפה והכרמל', 2, 2, '03:30', 32.7381, 35.015, 'https://upload.wikimedia.org/wikipedia/commons/8/8a/Nahal_Kelah.jpg'),
(83, N'חורשת הכרמל', N'חיפה והכרמל', 1, 1, '02:00', 32.7661, 35.0383, 'https://upload.wikimedia.org/wikipedia/commons/0/0d/Carmel_027.jpg'),
-- רמת הגולן
(84, N'מצפה הר בנטל', N'רמת הגולן', 1, 5, '01:00', 33.1311, 35.7822, 'https://upload.wikimedia.org/wikipedia/commons/5/5d/Bental578.JPG'),
(85, N'נחל אל על', N'רמת הגולן', 2, 2, '04:00', 33.18, 35.7361, 'https://upload.wikimedia.org/wikipedia/commons/3/33/El-al002.jpg'),
(87, N'נחל גלבון', N'רמת הגולן', 2, 2, '04:30', 32.9381, 35.7892, 'https://upload.wikimedia.org/wikipedia/commons/a/a1/Gilabon_032.jpg'),
(88, N'מפל בני יהודה', N'רמת הגולן', 1, 4, '01:30', 33.1131, 35.695, 'https://upload.wikimedia.org/wikipedia/commons/2/28/Benei_Yehuda_waterfall.jpg'),
-- הגליל התחתון
(89, N'כוכב הירדן – מבצר בלוויאר', N'הגליל התחתון', 1, 3, '02:00', 32.5161, 35.5222, 'https://upload.wikimedia.org/wikipedia/commons/4/4b/Crac_de_Belvoir.jpg'),
(90, N'מקורות עין מודיע', N'הגליל התחתון', 1, 4, '02:00', 32.5181, 35.4992, 'https://upload.wikimedia.org/wikipedia/commons/7/7a/Ein_Afek.jpg'),
(91, N'גשר – אתר היסטורי', N'הגליל התחתון', 1, 5, '01:30', 32.5442, 35.5072, 'https://upload.wikimedia.org/wikipedia/commons/f/f7/Gesher_railway_bridge.jpg'),
(92, N'עין השופט', N'חיפה והכרמל', 1, 4, '01:30', 32.6311, 35.125, 'https://upload.wikimedia.org/wikipedia/commons/3/3b/Ein_Hashofet_003.jpg'),
-- הגליל התחתון – כנרת
(93, N'טיילת טבריה', N'הגליל התחתון', 1, 1, '02:00', 32.795, 35.5311, 'https://upload.wikimedia.org/wikipedia/commons/4/4e/Sea_of_Galilee.jpg'),
(94, N'כפר נחום – שביל ארכי', N'הגליל התחתון', 1, 3, '02:00', 32.8808, 35.5494, 'https://upload.wikimedia.org/wikipedia/commons/1/13/Capernaum_Synagogue.jpg'),
(95, N'חמאת טבריה – מעיינות', N'הגליל התחתון', 1, 1, '02:00', 32.7681, 35.5481, 'https://upload.wikimedia.org/wikipedia/commons/8/8b/Hamei_Tveria_hot_springs.jpg'),
(96, N'מצפה ארמון נפתלי', N'הגליל התחתון', 1, 5, '01:30', 33.0292, 35.6142, 'https://upload.wikimedia.org/wikipedia/commons/e/e5/Arbel_view.jpg'),
-- מישור החוף
(97, N'נחל שורק – ראשון לציון', N'מישור החוף', 1, 4, '02:30', 31.8831, 34.7681, 'https://upload.wikimedia.org/wikipedia/commons/5/58/Nahal_Sorek.jpg'),
(98, N'חולות ראשון לציון', N'מישור החוף', 1, 4, '02:00', 31.9892, 34.715, 'https://upload.wikimedia.org/wikipedia/commons/1/1e/Rishon_LeZion_sand_dunes.jpg'),
(99, N'נחל שיקמה', N'מישור החוף', 2, 2, '03:30', 31.665, 34.7122, 'https://upload.wikimedia.org/wikipedia/commons/6/6a/Nahal_Shikma.jpg'),
(100, N'מקורות הירקון', N'מישור החוף', 1, 4, '02:00', 32.095, 34.8892, 'https://upload.wikimedia.org/wikipedia/commons/9/9e/Yarkon_Sources.jpg'),
-- יעדים נוספים 101–112
(101, N'נחל דרגה – מדבר יהודה', N'ים המלח ומדבר יהודה', 3, 2, '05:00', 31.475, 35.3722, 'https://upload.wikimedia.org/wikipedia/commons/1/1b/Nahal_Darga.jpg'),
(102, N'עין תמר', N'ים המלח ומדבר יהודה', 1, 4, '02:00', 31.05, 35.3661, 'https://upload.wikimedia.org/wikipedia/commons/4/4a/Ein_Tamar.jpg'),
(103, N'תל באר שבע', N'הנגב', 1, 3, '02:00', 31.2417, 34.8397, 'https://upload.wikimedia.org/wikipedia/commons/3/3f/Tel_Be%27er_Sheva.jpg'),
(104, N'חולות כסיפה', N'הערבה', 1, 4, '02:00', 30.9611, 34.9283, 'https://upload.wikimedia.org/wikipedia/commons/6/6d/Complex_Ripples_in_Sand_Dune_in_Negev_Desert.jpg'),
(105, N'נחל צלמון', N'הגליל העליון', 2, 2, '03:30', 32.9822, 35.4122, 'https://upload.wikimedia.org/wikipedia/commons/4/4a/Nahal_Amud_lower.jpg'),
(106, N'מערות הנחל בכרמל', N'חיפה והכרמל', 1, 4, '02:00', 32.6931, 34.96, 'https://upload.wikimedia.org/wikipedia/commons/9/96/Prehistoric_Man_Museum.jpg'),
(107, N'נחל מגדים', N'חיפה והכרמל', 1, 1, '02:00', 32.7031, 34.93, 'https://upload.wikimedia.org/wikipedia/commons/4/4f/Nahal_Magadim.jpg'),
(108, N'נחל אלכסנדר', N'מישור החוף', 1, 4, '02:30', 32.2411, 34.9183, 'https://upload.wikimedia.org/wikipedia/commons/5/58/Nahal_Alexander.jpg'),
(109, N'גן לאומי הכרמון', N'חיפה והכרמל', 1, 3, '02:00', 32.6711, 34.96, 'https://upload.wikimedia.org/wikipedia/commons/0/0d/Carmel_027.jpg'),
(110, N'נחל רפאים', N'ירושלים והרי יהודה', 2, 2, '03:00', 31.7531, 35.1681, 'https://upload.wikimedia.org/wikipedia/commons/8/8e/Nahal_Refaim.jpg'),
(111, N'כורזים', N'הגליל התחתון', 1, 3, '02:00', 32.8806, 35.55, 'https://upload.wikimedia.org/wikipedia/commons/1/1b/Korazim060.jpg'),
(112, N'מצפה כלניות', N'רמת הגולן', 1, 5, '01:00', 33.065, 35.81, 'https://upload.wikimedia.org/wikipedia/commons/a/a2/Banias_falls.jpg'),
-- יעדים מלמטייל (113–136)
(113, N'פארק נשר – גשרים תלויים בנחל קטיע', N'חיפה והכרמל', 1, 4, '02:00', 32.7511, 35.0348, 'https://upload.wikimedia.org/wikipedia/commons/8/8d/Nesher_Park_002.jpg'),
(114, N'נחל גלים', N'חיפה והכרמל', 2, 2, '04:00', 32.7422, 35.0181, 'https://upload.wikimedia.org/wikipedia/commons/8/8a/Nahal_Kelah.jpg'),
(115, N'נחל ציפורי', N'הגליל התחתון', 1, 4, '02:30', 32.7465, 35.2811, 'https://upload.wikimedia.org/wikipedia/commons/4/4d/Zippori003.jpg'),
(116, N'שמורת חוף דור – חוף הבונים', N'מישור החוף', 1, 4, '02:00', 32.6392, 34.9172, 'https://upload.wikimedia.org/wikipedia/commons/6/6d/Habonim_beach.jpg'),
(117, N'נחל יגור', N'חיפה והכרמל', 2, 2, '04:30', 32.6881, 35.0581, 'https://upload.wikimedia.org/wikipedia/commons/7/7f/Nahal_Hashofet.jpg'),
(118, N'תל אפק – גן לאומי ירקון', N'מישור החוף', 1, 3, '02:30', 32.1072, 34.9311, 'https://upload.wikimedia.org/wikipedia/commons/4/4c/Tel_Afek_Antipatris.jpg'),
(119, N'נחל כרמילה ונחל כסלון', N'ירושלים והרי יהודה', 2, 2, '03:30', 31.7681, 35.1581, 'https://upload.wikimedia.org/wikipedia/commons/9/94/Nahal_Ktalav.jpg'),
(120, N'שמורת חוף השרון', N'מישור החוף', 1, 4, '01:30', 32.2211, 34.8361, 'https://upload.wikimedia.org/wikipedia/commons/3/3a/Sharon_National_Park.jpg'),
(121, N'שמורת מג''רסה – בקעת בית צידה', N'הגליל התחתון', 1, 4, '01:30', 32.8242, 35.635, 'https://upload.wikimedia.org/wikipedia/commons/1/10/Tel_Dan_stream.jpg'),
(122, N'נחל צאלים', N'ים המלח ומדבר יהודה', 3, 2, '05:00', 31.1822, 35.3061, 'https://upload.wikimedia.org/wikipedia/commons/e/e1/Nahal_Zin.jpg'),
(123, N'פארק בריטניה – מסלול הבורות', N'ירושלים והרי יהודה', 2, 4, '02:30', 31.8814, 34.6781, 'https://upload.wikimedia.org/wikipedia/commons/2/2c/Britannia_Park_001.jpg'),
(124, N'רמת הנדיב', N'חיפה והכרמל', 1, 5, '02:30', 32.5436, 34.9386, 'https://upload.wikimedia.org/wikipedia/commons/1/1a/Ramat_Hanadiv5030.JPG'),
(125, N'שפך נחל שורק וחוף פלמחים', N'מישור החוף', 2, 4, '03:00', 31.9481, 34.7061, 'https://upload.wikimedia.org/wikipedia/commons/4/4e/Palmachim_beach.jpg'),
(126, N'עין מטע – חורבת חנות', N'ירושלים והרי יהודה', 2, 2, '03:00', 31.75, 35.1781, 'https://upload.wikimedia.org/wikipedia/commons/6/6b/Ein_Mata.jpg'),
(127, N'נחל זוויתן תחתון', N'רמת הגולן', 3, 2, '04:00', 33.1958, 35.6825, 'https://upload.wikimedia.org/wikipedia/commons/0/0f/Hexagon_pool.jpg'),
(128, N'שלוחת איילה – נחל שמרי', N'חיפה והכרמל', 2, 2, '03:00', 32.625, 34.9181, 'https://upload.wikimedia.org/wikipedia/commons/8/87/Nahal_Taninim.jpg'),
(129, N'נחל שיח', N'חיפה והכרמל', 1, 4, '01:30', 32.8, 34.985, 'https://upload.wikimedia.org/wikipedia/commons/7/7f/Nahal_Hashofet.jpg'),
(130, N'נחל זוויתן עליון – קצרין', N'רמת הגולן', 2, 2, '04:00', 32.9342, 35.6697, 'https://upload.wikimedia.org/wikipedia/commons/0/0f/Hexagon_pool.jpg'),
(131, N'מוחרקה – נחל רקפת', N'חיפה והכרמל', 2, 2, '04:00', 32.6772, 35.0611, 'https://upload.wikimedia.org/wikipedia/commons/0/0d/Carmel_027.jpg'),
(132, N'נחל גחר – רמות מנשה', N'חיפה והכרמל', 1, 4, '02:00', 32.6092, 35.1281, 'https://upload.wikimedia.org/wikipedia/commons/7/7f/Nahal_Hashofet.jpg'),
(133, N'גן לאומי אפולוניה – תל ארשף', N'מישור החוף', 1, 3, '01:30', 32.1942, 34.8081, 'https://upload.wikimedia.org/wikipedia/commons/8/8a/Apollonia_National_Park_2007.jpg'),
(134, N'נחל בשור – פארק אשכול', N'הנגב', 1, 4, '04:00', 31.635, 34.6081, 'https://upload.wikimedia.org/wikipedia/commons/6/6e/Nahal_Besor.jpg'),
(135, N'הר הרוח – שביל נוף יתלה', N'ירושלים והרי יהודה', 1, 5, '01:30', 31.8281, 35.0211, 'https://upload.wikimedia.org/wikipedia/commons/8/85/Jerusalem_forest_view.jpg'),
(136, N'עין בוקק – נחל בוקק וצוק תמרור', N'ים המלח ומדבר יהודה', 2, 2, '04:30', 31.1781, 35.3581, 'https://upload.wikimedia.org/wikipedia/commons/8/8d/Ein_Bokek_stream.jpg'),
-- יעדים מקק"ל (137–154)
(137, N'שביל הבריחה – יער חוף הכרמל', N'חיפה והכרמל', 1, 4, '01:30', 32.6461, 34.9681, 'https://upload.wikimedia.org/wikipedia/commons/6/6d/Rosh_Hanikra_grottoes.jpg'),
(138, N'שביל סובב מישר – יער ביריה', N'הגליל העליון', 1, 4, '02:00', 32.9922, 35.4981, 'https://upload.wikimedia.org/wikipedia/commons/4/43/Biriya_forest.jpg'),
(139, N'מצפה עופר – יער חוף הכרמל', N'חיפה והכרמל', 1, 5, '01:00', 32.6506, 34.9661, 'https://upload.wikimedia.org/wikipedia/commons/6/6d/Rosh_Hanikra_grottoes.jpg'),
(140, N'יער צרעה – דרך הפסלים', N'ירושלים והרי יהודה', 2, 1, '03:00', 31.7281, 34.9631, 'https://upload.wikimedia.org/wikipedia/commons/9/9d/Ben_Shemen_forest.jpg'),
(141, N'פארק הולנד אילת', N'הערבה', 1, 4, '02:00', 29.5722, 34.9611, 'https://upload.wikimedia.org/wikipedia/commons/a/a2/Timna-park-solomons-pillars-a.jpg'),
(142, N'מצפה הראל – דרך בורמה', N'ירושלים והרי יהודה', 1, 5, '01:00', 31.8383, 34.9517, 'https://upload.wikimedia.org/wikipedia/commons/f/f9/Mitzpe_Modiin.jpg'),
(143, N'יער עמוקה', N'הגליל העליון', 1, 4, '02:00', 32.7122, 35.245, 'https://upload.wikimedia.org/wikipedia/commons/5/55/Mount_Meron_trail.jpg'),
(144, N'גבעת הרקפות – פארק רמת מנשה', N'חיפה והכרמל', 1, 4, '02:00', 32.5981, 35.1222, 'https://upload.wikimedia.org/wikipedia/commons/7/7f/Nahal_Hashofet.jpg'),
(145, N'מפלי רז – פארק רמת מנשה', N'מישור החוף', 1, 4, '02:00', 32.5642, 35.066, 'https://upload.wikimedia.org/wikipedia/commons/8/87/Nahal_Taninim.jpg'),
(146, N'מאגר גלעד – פארק רמת מנשה', N'חיפה והכרמל', 1, 4, '01:30', 32.585, 35.105, 'https://upload.wikimedia.org/wikipedia/commons/7/7f/Nahal_Hashofet.jpg'),
(147, N'פארק הירדן – טחנות הקמח', N'הגליל התחתון', 1, 4, '02:00', 32.8942, 35.5872, 'https://upload.wikimedia.org/wikipedia/commons/1/10/Tel_Dan_stream.jpg'),
(148, N'יער חורשות אילון', N'השומרון', 1, 4, '02:00', 32.1311, 34.9422, 'https://upload.wikimedia.org/wikipedia/commons/4/4e/Horshim_forest.jpg'),
(149, N'יער קסם', N'השומרון', 1, 4, '02:00', 31.8781, 34.7281, 'https://upload.wikimedia.org/wikipedia/commons/4/4e/Horshim_forest.jpg'),
(150, N'יער ניר עציון', N'חיפה והכרמל', 1, 4, '02:00', 32.6978, 34.9936, 'https://upload.wikimedia.org/wikipedia/commons/9/9d/Ben_Shemen_forest.jpg'),
(151, N'יער לב הגליל', N'הגליל התחתון', 1, 4, '02:00', 32.8042, 35.2542, 'https://upload.wikimedia.org/wikipedia/commons/5/58/Mount_Tabor_trail.jpg'),
(152, N'מצפור הר יבנית – יער ביריה', N'הגליל העליון', 1, 5, '01:30', 32.9881, 35.5081, 'https://upload.wikimedia.org/wikipedia/commons/4/43/Biriya_forest.jpg'),
(153, N'יער הזורע – פארק רמת מנשה', N'חיפה והכרמל', 1, 1, '02:00', 32.5622, 35.0822, 'https://upload.wikimedia.org/wikipedia/commons/7/7f/Nahal_Hashofet.jpg'),
(154, N'יער אילת', N'הערבה', 1, 4, '02:00', 29.5581, 34.9281, 'https://upload.wikimedia.org/wikipedia/commons/a/a2/Timna-park-solomons-pillars-a.jpg'),
-- יעדים מרשות הטבע והגנים (157–170)
(157, N'גן לאומי עין חמד', N'ירושלים והרי יהודה', 1, 4, '02:00', 31.8056, 35.1272, 'https://upload.wikimedia.org/wikipedia/commons/4/4e/PikiWiki_Israel_48613_Ein_Hemed_park_near_Jerusalem.jpg'),
(158, N'שמורת עינות צוקים (עין פשחה)', N'ים המלח ומדבר יהודה', 1, 4, '02:00', 31.7378, 35.4624, 'https://upload.wikimedia.org/wikipedia/commons/8/8c/Einot-Zukim001.JPG'),
(159, N'גן לאומי בית שאן', N'הגליל התחתון', 1, 3, '02:30', 32.5189, 35.5014, 'https://upload.wikimedia.org/wikipedia/commons/5/5d/Beit_Shean772.JPG'),
(160, N'גן לאומי חוף אכזיב', N'הגליל העליון', 1, 4, '02:00', 33.0486, 35.1036, 'https://upload.wikimedia.org/wikipedia/commons/2/2f/Achziv_beach.jpg'),
(161, N'גן לאומי מעיין חרוד', N'הגליל התחתון', 1, 4, '02:00', 32.4372, 35.4311, 'https://upload.wikimedia.org/wikipedia/commons/3/3e/Ma%27ayan_Harod.jpg'),
(162, N'גן לאומי תל ערד', N'הנגב', 1, 3, '02:00', 31.2781, 35.2122, 'https://upload.wikimedia.org/wikipedia/commons/a/a1/Tel_Arad001.jpg'),
(163, N'גן לאומי השלושה', N'הגליל התחתון', 1, 4, '02:00', 32.5074, 35.4492, 'https://upload.wikimedia.org/wikipedia/commons/f/f4/Sachne001.jpg'),
(164, N'גן לאומי כורסי', N'הגליל התחתון', 1, 3, '01:30', 32.8236, 35.6483, 'https://upload.wikimedia.org/wikipedia/commons/7/72/Kursi001.jpg'),
(165, N'גן לאומי קומראן', N'ים המלח ומדבר יהודה', 1, 3, '01:30', 31.7414, 35.4608, 'https://upload.wikimedia.org/wikipedia/commons/7/74/Qumran.jpg'),
(166, N'שמורת טבע נחל אבוב', N'הנגב', 2, 2, '03:00', 31.2785, 35.3535, 'https://upload.wikimedia.org/wikipedia/commons/f/f4/Nahal_Tzafit.jpg'),
(167, N'גן לאומי אשקלון', N'הנגב', 1, 3, '02:00', 31.6622, 34.5472, 'https://upload.wikimedia.org/wikipedia/commons/4/4e/Ashkelon_national_park001.jpg'),
(168, N'גן לאומי תל מגידו', N'הגליל התחתון', 1, 3, '02:00', 32.5856, 35.1847, 'https://upload.wikimedia.org/wikipedia/commons/1/19/Megiddo009.jpg'),
(169, N'גן לאומי ברעם', N'הגליל העליון', 1, 3, '01:30', 33.0492, 35.4331, 'https://upload.wikimedia.org/wikipedia/commons/5/58/Bar%27am001.jpg'),
(170, N'גן לאומי ציפורי', N'הגליל התחתון', 1, 3, '02:00', 32.7456, 35.2814, 'https://upload.wikimedia.org/wikipedia/commons/4/4d/Zippori003.jpg');
SET IDENTITY_INSERT Destination OFF;

INSERT INTO Categories_of_Destination (Categories_id, Des_id) VALUES
(4,1),(5,1),(6,1),
(1,2),(2,2),(3,2),
(1,3),(3,3),
(1,4),(3,4),(7,4),
(2,5),(3,5),
(2,6),(7,6),
(1,7),(3,7),
(1,8),(3,8),
(2,9),(3,9),
(5,10),(6,10),
(1,11),(2,11),(6,11),
(2,12),(6,12),(7,12),
(1,13),(2,13),
(1,14),(2,14),(3,14),
(1,15),(2,15),
(2,16),(5,16),
(4,17),(5,17),
(4,18),(5,18),
(4,19),(5,19),
(4,20),(5,20),
(1,21),(2,21),(7,21),
(1,22),(2,22),(3,22),
(5,23),(6,23),
(2,24),(7,24),
(2,25),(7,25),
(1,26),(3,26),
(1,27),(3,27),(5,27),
(1,28),(3,28),
(1,29),(2,29),(3,29),
(4,30),(6,30),
(1,31),(5,31),(6,31),
(1,32),(6,32),
(1,33),(2,33),(7,33),
(1,34),(2,34),(3,34),
(2,35),(6,35),(7,35),
(5,36),(6,36),
(1,37),(2,37),(3,37),
(4,38),(5,38),
(1,39),(6,39),
(1,40),(2,40),(3,40),
(2,41),(6,41),(7,41),
(1,42),(2,42),(6,42),
(1,43),(3,43),(7,43),
(1,44),(3,44),(5,44),
(1,45),(3,45),
(1,46),(2,46),(3,46),
(2,47),(5,47),
(2,48),(3,48),(5,48),
(5,49),(6,49),
(2,50),(4,50),(5,50),
(2,51),(4,51),(5,51),
(1,52),(2,52),
(1,53),(2,53),(3,53),
(2,54),(3,54),
(1,55),(2,55),(7,55),
(1,56),(3,56),
(3,57),(4,57),(5,57),
(2,58),(4,58),(6,58),
(1,59),(6,59),
(1,60),(2,60);

-- יעדים 61–67 הנגב והערבה
INSERT INTO Categories_of_Destination (Categories_id, Des_id) VALUES
(2,61),(5,61),(6,61),
(1,62),(2,62),(6,62),
(2,63),(5,63),(7,63),
(1,64),(2,64),(3,64),
(2,65),(4,65),(6,65),
(2,66),(6,66),
(1,67),(2,67),(5,67);

-- יעדים 68–78 הגליל
INSERT INTO Categories_of_Destination (Categories_id, Des_id) VALUES
(1,68),(2,68),
(4,69),(5,69),(6,69),
(1,70),(3,70),
(1,71),(3,71),(7,71),
(2,72),(4,72),(6,72),
(4,73),(5,73),
(1,74),(3,74),
(2,75),(4,75),(6,75),
(1,76),(2,76),
(1,77),(2,77),(3,77),
(2,78),(4,78);


-- יעדים 79–100 חיפה והכרמל, גולן, גליל תחתון, מישור החוף
INSERT INTO Categories_of_Destination (Categories_id, Des_id) VALUES
(1,79),(2,79),(3,79),
(1,80),(2,80),(6,80),
(6,81),
(1,82),(2,82),(3,82),
(1,83),(2,83),
(6,84),
(1,85),(2,85),(3,85),(7,85),
(1,87),(2,87),(3,87),
(1,88),(3,88),
(2,89),(4,89),(6,89),
(1,90),(3,90),
(4,91),(5,91),
(1,92),(3,92),
(6,93),
(4,94),(5,94),
(4,95),(5,95),
(6,96),
(1,97),(2,97),(3,97),
(1,98),(6,98),
(1,99),(2,99),(3,99),
(1,100),(3,100);


-- יעדים 101–112
INSERT INTO Categories_of_Destination (Categories_id, Des_id) VALUES
(1,101),(2,101),(7,101),
(1,102),(3,102),
(4,103),(5,103),
(1,104),(6,104),
(1,105),(2,105),(3,105),
(1,106),(3,106),
(1,107),(2,107),(3,107),
(1,108),(2,108),(3,108),
(1,109),(4,109),(5,109),
(1,110),(2,110),(3,110),
(4,111),(5,111),
(6,112);


-- יעדים 113–136 מלמטייל
INSERT INTO Categories_of_Destination (Categories_id, Des_id) VALUES
(1,113),(2,113),(6,113),
(1,114),(2,114),(3,114),
(1,115),(2,115),(3,115),
(1,116),(6,116),
(1,117),(2,117),(3,117),
(1,118),(4,118),(5,118),
(1,119),(2,119),(3,119),
(1,120),(6,120),
(1,121),(3,121),
(1,122),(2,122),(7,122),
(1,123),(2,123),
(1,124),(6,124),
(1,125),(2,125),(3,125),
(1,126),(2,126),(3,126),
(1,127),(3,127),(7,127),
(1,128),(2,128),(3,128),
(1,129),(2,129),(3,129),
(1,130),(2,130),(3,130),
(1,131),(2,131),(3,131),(6,131),
(1,132),(2,132),(3,132),
(1,133),(4,133),(5,133),
(1,134),(2,134),(3,134),
(1,135),(2,135),(6,135),
(1,136),(2,136),(3,136);



-- יעדים 137–154 מקק"ל
INSERT INTO Categories_of_Destination (Categories_id, Des_id) VALUES
(2,137),
(4,137),
(6,137),
(2,138),
(6,138),
(2,139),
(6,139),
(2,140),
(5,140),
(6,140),
(1,141),
(2,141),
(2,142),
(6,142),
(1,143),
(2,143),
(1,144),
(2,144),
(6,144),
(1,145),
(3,145),
(1,146),
(6,146),
(1,147),
(2,147),
(4,147),
(1,148),
(2,148),
(1,149),
(2,149),
(1,150),
(2,150),
(1,151),
(2,151),
(2,152),
(6,152),
(1,153),
(2,153),
(1,154),
(2,154);



-- יעדים 157–170 רשות הטבע והגנים
INSERT INTO Categories_of_Destination (Categories_id, Des_id) VALUES
(1,157),
(2,157),
(4,157),
(1,158),
(2,158),
(3,158),
(1,159),
(4,159),
(5,159),
(1,160),
(6,160),
(1,161),
(2,161),
(4,161),
(1,162),
(4,162),
(5,162),
(1,163),
(3,163),
(1,164),
(4,164),
(1,165),
(4,165),
(5,165),
(1,166),
(3,166),
(7,166),
(1,167),
(4,167),
(1,168),
(4,168),
(5,168),
(1,169),
(4,169),
(1,170),
(4,170),
(5,170);

INSERT INTO Users (user_id, FullName, Phon) VALUES
(N'U001', N'נועם לוי', N'050-1111111'),
(N'U002', N'שירה כהן', N'052-2222222'),
(N'U003', N'אורי מזרחי', N'054-3333333'),
(N'U004', N'תמר גולן', N'053-4444444'),
(N'U005', N'איתן ברק', N'058-5555555');

SET IDENTITY_INSERT Trip ON;
INSERT INTO Trip (Trip_id, Trip_name, user_id, Trip_Date, Address_start, Start_time, End_time, trip_cost) VALUES
(1, N'מדבר יהודה ומצדה', N'U001', '2026-04-12', N'ערד', '07:00', '18:00', 120),
(2, N'מכתשים ונחלי הנגב', N'U002', '2026-05-08', N'מצפה רמון', '06:30', '19:30', 90),
(3, N'גליל עליון ומקורות הירדן', N'U003', '2026-05-21', N'קרית שמונה', '08:00', '18:30', 110),
(4, N'רכס מירון וארבל', N'U004', '2026-06-03', N'צפת', '07:30', '17:30', 70),
(5, N'מרכז חופי ונחלים', N'U005', '2026-06-15', N'חדרה', '09:00', '17:00', 60),
(6, N'ירושלים מעיינות והיסטוריה', N'U001', '2026-06-27', N'ירושלים', '08:00', '18:00', 50);
SET IDENTITY_INSERT Trip OFF;

INSERT INTO Des_of_trip (Trip_id, Des_id, visit_number) VALUES
(1,1,1),(1,3,2),(1,5,3),(1,10,4),(1,158,5),(1,165,6),
(2,11,1),(2,14,2),(2,22,3),(2,62,4),(2,64,5),(2,141,6),(2,154,7),(2,162,8),(2,166,9),
(3,28,1),(3,29,2),(3,73,3),(3,76,4),(3,75,5),(3,34,6),(3,138,7),(3,160,8),(3,169,9),
(4,35,1),(4,36,2),(4,40,3),(4,72,4),(4,147,5),(4,159,6),(4,163,7),(4,161,8),(4,170,9),
(5,46,1),(5,116,2),(5,125,3),(5,97,4),(5,124,5),(5,137,6),(5,144,7),
(6,53,1),(6,56,2),(6,57,3),(6,58,4),(6,157,5);

INSERT INTO Categories_to_trip (categories_id, trip_id) VALUES
(2,1),(4,1),(6,1),
(1,2),(2,2),(7,2),
(1,3),(3,3),(6,3),
(1,4),(2,4),(6,4),
(2,5),(3,5),(4,5),
(2,6),(3,6),(4,6);

INSERT INTO Feature_to_trip (Feature_id, trip_id) VALUES
(5,1),(3,1),
(1,2),(4,2),
(5,3),(3,3),
(1,4),(4,4),
(1,5),(2,5),
(1,6),(2,6),(3,6);

SET IDENTITY_INSERT Nature_trip ON;
INSERT INTO Nature_trip (natureTrip_id, trip_id, Num_break, Min_num_des, Max_num_des, level_id, Region) VALUES
(1, 1, 1, 3, 5, 2, N'ים המלח ומדבר יהודה'),
(2, 2, 1, 3, 5, 3, N'הנגב'),
(3, 3, 1, 3, 5, 2, N'הגליל העליון'),
(4, 4, 1, 3, 4, 2, N'הגליל התחתון'),
(5, 5, 1, 2, 4, 1, N'מישור החוף'),
(6, 6, 1, 3, 5, 1, N'ירושלים והרי יהודה');
SET IDENTITY_INSERT Nature_trip OFF;

-- שעות פתיחה לכל יעד (ראה גם AlterDestinationOpeningHours.sql)
UPDATE Destination SET opening_time = '08:00:00', closing_time = '18:00:00'
WHERE Name_des LIKE N'%גן לאומי%' OR Name_des LIKE N'%שמורת%' OR Name_des LIKE N'%פארק נשר%'
   OR Des_id IN (1,10,17,18,19,20,30,38,42,50,51,57,58,61,69,75,78,89,94,111,103,133,118,109,157,158,159,160,161,162,163,164,165,167,168,169,170);
UPDATE Destination SET opening_time = '06:00:00', closing_time = '19:00:00'
WHERE opening_time = '08:00:00' AND closing_time = '17:00:00';
UPDATE Destination SET opening_time = '09:00:00', closing_time = '18:00:00' WHERE Des_id IN (9, 93);

ALTER TABLE Categories_of_Destination CHECK CONSTRAINT FK_Categories_of_Destination_Categories;
ALTER TABLE Categories_to_trip CHECK CONSTRAINT FK_Categories_to_trip_Categories;
ALTER TABLE Des_of_trip CHECK CONSTRAINT FK_Des_of_trip_Destination;
GO

PRINT N'TripSeedData inserted successfully.';
GO
