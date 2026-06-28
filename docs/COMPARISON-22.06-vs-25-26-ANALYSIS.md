# ניתוח השוואה: גרסת 22.06.2026 מול גרסה נוכחית (25–26.06.2026)

**תאריך ניתוח:** 26.06.2026  
**מקור גרסה תקינה (22.06):** `C:\Users\User\Desktop\גיבוי 22.06.2026\פרויקט טיול\API_trip_link`  
**מקור גרסה נוכחית:** `C:\Users\User\Desktop\פרויקט טיול\API_trip_link`

---

## סיכום מנהלים

| מדד | ערך |
|-----|-----|
| קבצי מקור שהשווינו (`.cs`, `.ts`, `.json`, `.sql`, …) | 165 (22.06) → 202 (נוכחי) |
| קבצים חדשים בלבד בנוכחי | **37** |
| קבצים שהוסרו | **0** |
| קבצים שהשתנו | **81** |

### מצב ריצה בפרויקט הנוכחי (לפי `CHANGELOG-2026-06-25-26.md`)

- **קוד פעיל = 25.06** — שינויי 26.06 שמורים בהערות `/* [26.06.2026] */` ואינם רצים.
- חלק מהפרונטנד שוחזר לגרסת 22.06, בעוד שהבקאנד מכיל את שינויי 25.06 — **יש חוסר התאמה בין Frontend ל-Backend** (optimize סינכרוני מול 202 Accepted).

### נושאים חוצי-מערכת (25.06 פעיל)

| נושא | 22.06 | 25–26.06 |
|------|-------|----------|
| **מודל זמן** | `departure + BusHours + WalkHours` | לוחות Google: `BestDepartureTime` / `ArrivalTime` |
| **קשתות חזרה** | לא מפורשות בטבלה | שכבת `(dest→origin)` + `ComputeReturnArc` |
| **הגבלת API** | סריקה רחבה, concurrency 6 | חלון יציאה מהמקור, `MaxApiCallsPerArc`, concurrency 2 |
| **Bootstrap תחנות** | לא קיים | `StationLinkBootstrapService` + Google persistence |
| **Optimize API** | סינכרוני — `POST` מחזיר תוצאה | אסינכרוני — `202` + poll `GET optimize/result/{traceId}` |
| **Travelers** | `Destination.TravelerId` (1:1) | M:N דרך `TravelersOfDestination` |
| **26.06 (מבוטל)** | — | delta streaming, post-gate, סנכרון arrival ב-Step6 |

### מפת סיכון להעברה ל-22.06

| רמת סיכון | אזורים |
|-----------|--------|
| 🔴 גבוה | `GoogleMapsTransitApiService`, `Step2_ScoreTableBuilder`, `TransitScheduleCollector`, schema DB (Travelers, StationToDestination) |
| 🟡 בינוני | `RouteBuilder`, `ArcCostCalculator`, `Step0_InputLoader`, async API + Frontend |
| 🟢 נמוך | `Configuration.cs`, `ReturnFeasibility`, `ArcScheduleHelper`, `OptimizerRejectionTracker`, DTOs additive |

### סדר העברה מומלץ ל-22.06

1. `Configuration/Configuration.cs`
2. `ArcScheduleHelper.cs` + `ReturnFeasibility.cs`
3. `GoogleMapsTransitApiService.cs` + מודלי Transit
4. `WeightCalculator` → `TransitScheduleCollector` → `ArcCostCalculator`
5. `ScoreTable.cs` → `Step2_ScoreTableBuilder.cs`
6. `RouteBuilder.cs` + Step4/5
7. `Step0_InputLoader` + Google bootstrap (אם DB מוכן)
8. Async API + Frontend
9. `Step6_TripItineraryBuilder` + DTOs

---

## מקרא לטבלאות הניתוח

| עמודה | משמעות |
|-------|--------|
| **פונקציונלי/קוסמטי** | F = פונקציונלי, C = קוסמטי, F/C = מעורב |
| **אלגוריתם** | השפעה על SA / RouteBuilder / בחירת מסלול |
| **טבלת זמנים** | השפעה על תוכן/מבנה תאי `(i,j,h)` |
| **Google Directions** | השפעה על כמות/סוג קריאות API |
| **טבלה 3D** | השפעה על בניית/מילוי הטבלה התלת-ממדית |
| **בטוח ל-22.06** | ✅ כן / ⚠️ חלקי / ❌ לא (ללא תלויות) |

---

# חלק א': קבצים חדשים (37)

## Backend — C#

### `Configuration/Configuration.cs`

| # | ניתוח |
|---|--------|
| 1. מה השתנה | קובץ חדש — `AppConfiguration` עם Routing, Scoring, ScoreTable, Transit, OptimizerSteps, DataAccess |
| 2. למה | ריכוז magic numbers שהיו מפוזרים ב-22.06 |
| 3. F/C | C (רוב הערכים זהים ל-literals ישנים; concurrency ב-Step2: 6→2 דרך config) |
| 4. אלגוריתם | 🟡 עקיף — tunables |
| 5. טבלת זמנים | 🟡 עקיף — caps, חלון מקור |
| 6. Google | 🟡 עקיף — `MaxApiCallsPerArc` |
| 7. טבלה 3D | 🟡 עקיף — `MaxTotalCells`, `MaxTimeFrame` |
| 8. בטוח ל-22.06 | ✅ **כן** — מומלץ כשלב ראשון |

---

### `Services/Optimizer/ReturnFeasibility.cs`

| # | ניתוח |
|---|--------|
| 1. מה השתנה | קובץ חדש: `GetMinReturnHours`, `CanPossiblyFinish(leave + minReturn ≤ tripEnd)` |
| 2. למה | חיתוך מסלולים שלא ניתן לחזור מהם הביתה בזמן |
| 3. F/C | F |
| 4. אלגוריתם | ✅ RouteBuilder מדלג על יעדים לא-חוקיים |
| 5. טבלת זמנים | ❌ (post-gate ב-Step2 מבוטל ב-26.06) |
| 6. Google | ❌ |
| 7. טבלה 3D | ❌ (פעיל); ✅ אם post-gate 26.06 מופעל |
| 8. בטוח ל-22.06 | ✅ **כן** — helper קטן; צריך wiring ב-Step2 + RouteBuilder |

---

### `Services/Optimizer/ArcScheduleHelper.cs`

| # | ניתוח |
|---|--------|
| 1. מה השתנה | קובץ חדש: `TryApplySchedule`, `LeaveAfterVisit`, `TryWallClockHours` |
| 2. למה | החלפת `dep + bus + walk` בזמני Google אמיתיים |
| 3. F/C | F |
| 4. אלגוריתם | ✅ propagation של זמנים ב-RouteBuilder, Step6, Collector |
| 5. טבלת זמנים | ✅ `ArcCost.ArrivalTime` מאוכלס |
| 6. Google | ❌ |
| 7. טבלה 3D | ✅ תא נדחה אם schedule לא תקף |
| 8. בטוח ל-22.06 | ✅ **כן** — יחד עם Collector + ArcCostCalculator |

---

### `Services/Optimizer/MapPointBuilder.cs`

| # | ניתוח |
|---|--------|
| 1. מה השתנה | קובץ חדש — בניית נקודות מפה (start/destination/home) |
| 2. למה | תצוגת מפה עשירה ב-itinerary |
| 3. F/C | C (output/display) |
| 4–7. | ❌ לכל |
| 8. בטוח ל-22.06 | ✅ **כן** — עם `ItineraryService` + DTOs |

---

### `Services/StationLinkBootstrapService.cs`

| # | ניתוח |
|---|--------|
| 1. מה השתנה | קובץ חדש — לפני optimize, bootstrap קישורי תחנה↔יעד דרך Google |
| 2. למה | יעדים ללא קישור walking/transit לא ייכנסו לאופטימיזציה |
| 3. F/C | F |
| 4. אלגוריתם | ✅ מצמצם `n` (מספר יעדים) |
| 5. טבלת זמנים | ✅ walking hours נכנסים לארכים |
| 6. Google | ✅ N קריאות transit access ליעדים קרים |
| 7. טבלה 3D | ✅ `(n+1)³` קטן יותר |
| 8. בטוח ל-22.06 | ⚠️ **חלקי** — דורש DB + `GoogleTransitPersistenceService` + Step0 |

---

### `Services/StationLinkPersistence.cs`

| # | ניתוח |
|---|--------|
| 1. מה השתנה | קובץ חדש — persist/update של `StationToDestination` |
| 2. למה | שמירת תוצאות Google walking ב-DB |
| 3. F/C | F |
| 4–7. | 🟡 עקיף — מאכלס נתוני walking |
| 8. בטוח ל-22.06 | ⚠️ **חלקי** — עם schema + repositories |

---

### `Services/GoogleTransitPersistenceService.cs`

| # | ניתוח |
|---|--------|
| 1. מה השתנה | קובץ חדש (~483 שורות) — persist stations, buses, agencies, links מ-Google |
| 2. למה | cache ב-SQL — פחות קריאות חוזרות |
| 3. F/C | F |
| 4. אלגוריתם | ✅ מאכלס קווי אוטובוס ו-walking links |
| 5. טבלת זמנים | ✅ |
| 6. Google | ✅ צורך transit access results |
| 7. טבלה 3D | ✅ |
| 8. בטוח ל-22.06 | ❌ **לא standalone** — schema Agency/Bus + bootstrap |

---

### `Services/StationWalkingRouteService.cs` + `IStationWalkingRouteService.cs`

| # | ניתוח |
|---|--------|
| 1. מה השתנה | קבצים חדשים — refresh walking route לקישור קיים |
| 2. למה | עדכון נתוני הליכה ישנים |
| 3. F/C | F (maintenance) |
| 4–7. | 🟡 עקיף |
| 8. בטוח ל-22.06 | ✅ **כן** — כלי אופציונלי |

---

### `Services/Transit/GooglePlacesAutocompleteService.cs` + `IPlacesAutocompleteService.cs`

| # | ניתוח |
|---|--------|
| 1. מה השתנה | קבצים חדשים — proxy ל-Google Places Autocomplete |
| 2. למה | כתובת התחלה מאומתת בצד שרת |
| 3. F/C | F (UX + input quality) |
| 4–7. | 🟡 עקיף — geocoding טוב יותר → arcs טובים יותר |
| 8. בטוח ל-22.06 | ✅ **כן** — עם `PlacesController` |

---

### `Controllers/ConfigController.cs`

| # | ניתוח |
|---|--------|
| 1. מה השתנה | `GET api/config/maps` — מחזיר Maps API key |
| 2. למה | Frontend טוען Maps בלי key ב-bundle |
| 3. F/C | F |
| 4–7. | ❌ |
| 8. בטוח ל-22.06 | ✅ **כן** — בדיקת אבטחה מומלצת |

---

### `Controllers/PlacesController.cs`

| # | ניתוח |
|---|--------|
| 1. מה השתנה | `GET api/places/autocomplete?input=` |
| 2. למה | autocomplete כתובת |
| 3. F/C | F |
| 4–7. | 🟡 Places API, לא Directions |
| 8. בטוח ל-22.06 | ✅ **כן** |

---

### `Data/Repositories/IStationToDestinationRepository.cs` + `StationToDestinationRepository.cs`

| # | ניתוח |
|---|--------|
| 1. מה השתנה | repository חדש — upsert station link עם Google walking |
| 2. למה | abstraction ליצירת קישורים |
| 3. F/C | F |
| 4–7. | ✅ walking data → score table |
| 8. בטוח ל-22.06 | ⚠️ **חלקי** — עם schema + Google service |

---

### `Models/Agency.cs`, `Models/PlaceSuggestionDto.cs`, `Models/TravelersOfDestination.cs`

| # | ניתוח |
|---|--------|
| 1. מה השתנה | entities/DTOs חדשים — Agency (Google), PlaceSuggestion, M:N travelers |
| 2. למה | schema חדש + Places API |
| 3. F/C | F |
| 4. אלגוריתם | ✅ TravelersOfDestination → filter ב-OptimizerDataRepository |
| 5–7. | 🟡 |
| 8. בטוח ל-22.06 | ❌ **לא** — דורש SQL migrations |

---

## SQL חדש

| קובץ | מה | למה | F/C | Alg | TT | Google | 3D | 22.06 |
|------|-----|-----|-----|-----|-----|--------|-----|-------|
| `AlterAgency.sql` | טבלת Agency | Google agency metadata | F | 🟡 | 🟡 | ✅ | 🟡 | ❌ |
| `AlterDestinationTravelers.sql` | M:N travelers | multi-traveler per dest | F | ✅ | ✅ | ❌ | ✅ | ❌ |
| `AlterNatureTripTraveler.sql` | TravelerId on NatureTrip | העדפת מטייל בטיול | F | ✅ | ✅ | ❌ | ✅ | ⚠️ |
| `CreateAgency.sql` | create Agency | — | F | 🟡 | 🟡 | ✅ | 🟡 | ❌ |
| `TripSeedStations.sql` | seed stations | — | F | 🟡 | 🟡 | ❌ | 🟡 | ⚠️ |
| `UpdateDestinationTravelers.sql` | data migration | — | F | ✅ | ✅ | ❌ | ✅ | ❌ |
| `UpdateAllDestinationTravelers.sql` | bulk update | — | F | ✅ | ✅ | ❌ | ✅ | ❌ |

---

## Frontend — חדש

### `score-table-grid.component.ts`

| # | ניתוח |
|---|--------|
| 1. מה השתנה | קומפוננטה חדשה — sort, filter, CSV, עמודות arcKind/rejectionReason |
| 2. למה | תצוגת טבלת score 3D לניתוח |
| 3. F/C | C (presentation) |
| 4–6. | ❌ |
| 7. טבלה 3D | ✅ UI ייעודי |
| 8. בטוח ל-22.06 | ✅ **כן** — orphaned; דורש models + backend fields |

---

### `api-normalize.ts`

| # | ניתוח |
|---|--------|
| 1. מה השתנה | normalize PascalCase↔camelCase + שדות חדשים |
| 2. למה | ASP.NET vs Angular field mapping |
| 3. F/C | F |
| 4–7. | 🟡 מבטיח שדות rejection/arc מגיעים ל-UI |
| 8. בטוח ל-22.06 | ✅ **כן** — wire ל-trip.service |

---

### `address-autocomplete.component.ts`

| # | ניתוח |
|---|--------|
| 1. מה השתנה | combobox עם Places proxy |
| 2. למה | כתובת התחלה מאומתת |
| 3. F/C | F |
| 4–7. | 🟡 input quality |
| 8. בטוח ל-22.06 | ✅ **כן** — orphaned; wire ל-wizard |

---

### `destination-card.component.ts`

| # | ניתוח |
|---|--------|
| 1. מה השתנה | כרטיס יעד עשיר (תמונה, region, travelers) |
| 2. למה | UX |
| 3. F/C | C |
| 4–7. | ❌ |
| 8. בטוח ל-22.06 | ⚠️ **חלקי** — דורש `getRegionImageUrl` + styles |

---

### `google-maps-loader.service.ts` + `places.service.ts`

| # | ניתוח |
|---|--------|
| 1. מה השתנה | loader מרכזי + Places proxy client |
| 2. למה | Maps/Directions/Places בצורה מאובטחת |
| 3. F/C | F |
| 4–7. | 🟡 Directions ב-trip-result |
| 8. בטוח ל-22.06 | ✅ **כן** — עם ConfigController |

---

### קבצי `.new.ts` (ארכיון)

| קובץ | הערה |
|------|------|
| `*.new.ts` | עותקי ארכיון 25–26; לא wired ב-build |

---

## Docs / אחר

| קובץ | F/C | Alg | TT | Google | 3D | 22.06 |
|------|-----|-----|-----|--------|-----|-------|
| `docs/CHANGELOG-2026-06-25-26.md` | C | — | — | — | — | ✅ |
| `docs/RESTORE-REPORT-24-06-2026.md` | C | — | — | — | — | ✅ |
| `.cursor/plans/...plan.md` | C | — | — | — | — | ✅ |

---

# חלק ב': קבצים שהשתנו — Optimizer Core

## `Services/Optimizer/Steps/Step2_ScoreTableBuilder.cs` (+180/-58)

| # | ניתוח |
|---|--------|
| 1. מה השתנה | forward + **return arcs**; `MinReturnHoursByDestId` (driving per dest); caps; concurrency 6→2; `Parallel.ForEachAsync`; `isReturnArc`; rejection summary; post-gate **מבוטל** (26.06 בהערות) |
| 2. למה | מילוי שכבת חזרה; pruning; הגנת memory/API |
| 3. F/C | **F** |
| 4. אלגוריתם | ✅ return cells + min-return ל-RouteBuilder |
| 5. טבלת זמנים | ✅ return rows `(i→0)` |
| 6. Google | ✅ +N driving + return batches; capped |
| 7. טבלה 3D | ✅ שכבת return + minute count |
| 8. בטוח ל-22.06 | ⚠️ **חלקי** — bundle גדול |

---

## `Services/Optimizer/ScoreTable.cs` (+102/-52)

| # | ניתוח |
|---|--------|
| 1. מה השתנה | `RejectionReason`, `ArcKind`; origin search radius מ-config; `FindNearestValidCell` ל-return; `EnumerateFilledCells` כולל invalid+return; `MaxTimeFrame` |
| 2. למה | debug/UI; return lookup; מניעת minute explosion |
| 3. F/C | **F** |
| 4. אלגוריתם | ✅ radius משפיע על בחירת תא |
| 5. טבלת זמנים | ✅ |
| 6. Google | ❌ |
| 7. טבלה 3D | ✅ enumeration + caps |
| 8. בטוח ל-22.06 | ⚠️ **חלקי** |

---

## `Services/Optimizer/RouteBuilder.cs` (+247/-36)

| # | ניתוח |
|---|--------|
| 1. מה השתנה | `minReturnByDestId`; `ReturnFeasibility.CanPossiblyFinish`; `ArcScheduleHelper.LeaveAfterVisit`; `ComputeReturnArc` בסוף; `InitialWaitAtOrigin` |
| 2. למה | feasibility + timing אמיתי + רגל חזרה |
| 3. F/C | **F** |
| 4. אלגוריתם | ✅ **שינוי מהותי** — פחות/שונה routes |
| 5. טבלת זמנים | ❌ (קורא) |
| 6. Google | ✅ return arc live call |
| 7. טבלה 3D | ❌ |
| 8. בטוח ל-22.06 | ⚠️ **חלקי** |

---

## `Services/Optimizer/WeightCalculator.cs` (+52/-27)

| # | ניתוח |
|---|--------|
| 1. מה השתנה | constants→config; **הסרת** return-by-trip-end מ-forward rejection; `CanReturnHomeByTime`, `EvaluateReturnArcFeasibility` |
| 2. למה | הפרדת forward vs return scoring |
| 3. F/C | **F** |
| 4. אלגוריתם | ✅ יותר forward cells valid |
| 5. טבלת זמנים | ✅ |
| 6. Google | ❌ |
| 7. טבלה 3D | ✅ ratio שונה |
| 8. בטוח ל-22.06 | ⚠️ **חלקי** — עם Collector |

---

## `Services/Optimizer/ArcCostCalculator.cs` (+108/-38)

| # | ניתוח |
|---|--------|
| 1. מה השתנה | `BuildArcCost` + `ArcScheduleHelper`; `AlightingStationName`; `ComputeReturnArc`; walking=0 ב-API call |
| 2. למה | schedule-based arcs |
| 3. F/C | **F** |
| 4. אלגוריתם | ✅ |
| 5. טבלת זמנים | ✅ |
| 6. Google | ✅ return calls |
| 7. טבלה 3D | 🟡 fallback |
| 8. בטוח ל-22.06 | ⚠️ **חלקי** |

---

## `Services/Optimizer/TransitScheduleCollector.cs` (+156/-56)

| # | ניתוח |
|---|--------|
| 1. מה השתנה | `isReturnArc`; origin window; `MaxApiCallsPerArc`; `ArcScheduleHelper`; `TerminalWalkingHours`; return feasibility; `RejectionReason` |
| 2. למה | ליבת ingestion לטבלה |
| 3. F/C | **F** |
| 4. אלגוריתם | ✅ |
| 5. טבלת זמנים | ✅ |
| 6. Google | ✅ פחות scans, capped |
| 7. טבלה 3D | ✅ **ליבה** |
| 8. בטוח ל-22.06 | ⚠️ **חלקי** |

---

## `Services/Transit/GoogleMapsTransitApiService.cs` (+707/-62)

| # | ניתוח |
|---|--------|
| 1. מה השתנה | `ParseTransitRoute`; TerminalWalking; AlightingStop; walking/geocode caches; `GetWalkingRouteAsync`; stop resolution |
| 2. למה | parsing ריאליסטי; persistence infrastructure |
| 3. F/C | **F** |
| 4. אלגוריתם | ✅ |
| 5. טבלת זמנים | ✅ bus vs walk split |
| 6. Google | ✅ **השפעה מקסימלית** |
| 7. טבלה 3D | ✅ כל cell מגיע מכאן |
| 8. בטוח ל-22.06 | ❌ **סיכון גבוה** — hub מרכזי |

---

## `Services/Optimizer/Steps/Step0_InputLoader.cs` (+146/-86)

| # | ניתוח |
|---|--------|
| 1. מה השתנה | geocode start; `StationLinkBootstrapService`; traveler filter; reachable-only destinations; walking mapping |
| 2. למה | input quality + bootstrap |
| 3. F/C | **F** |
| 4. אלגוריתם | ✅ `n` משתנה |
| 5. טבלת זמנים | 🟡 |
| 6. Google | ✅ geocode + bootstrap |
| 7. טבלה 3D | ✅ |
| 8. בטוח ל-22.06 | ⚠️ **חלקי** |

---

## `Services/Optimizer/OptimizerServiceImpl.cs` (+63/-50)

| # | ניתוח |
|---|--------|
| 1. מה השתנה | `Task.Run` background; `GetResult(traceId)`; fire-and-forget |
| 2. למה | optimize ארוך בלי timeout |
| 3. F/C | **F** (API contract) |
| 4–7. | ❌ (orchestration only) |
| 8. בטוח ל-22.06 | ✅ **כן** — עם controller + frontend |

---

## `Services/Optimizer/Steps/Step4_InitialRouteBuilder.cs` (+3/-1)

| # | ניתוח |
|---|--------|
| 1. מה השתנה | העברת `minReturnByDestId: ctx.MinReturnHoursByDestId` ל-RouteBuilder |
| 2. למה | wiring return feasibility |
| 3. F/C | **F** (wiring) |
| 4. אלגוריתם | ✅ |
| 5–7. | 🟡 |
| 8. בטוח ל-22.06 | ✅ עם RouteBuilder |

---

## `Services/Optimizer/Steps/Step5_SaOptimizer.cs` (+23/-17)

| # | ניתוח |
|---|--------|
| 1. מה השתנה | `minReturnByDestId` בכל קריאות RouteBuilder; config constants |
| 2. למה | SA משתמש ב-return-aware RouteBuilder |
| 3. F/C | **F** |
| 4. אלגוריתם | ✅ תוצאות SA שונות |
| 5–7. | ❌ |
| 8. בטוח ל-22.06 | ✅ עם RouteBuilder |

---

## `Services/Optimizer/Steps/Step6_TripItineraryBuilder.cs`

| # | ניתוח |
|---|--------|
| 1. מה השתנה | `ArcScheduleHelper`; return narrative; walking from arc; `AlightingStationName`; delay sync **מבוטל** (26.06) |
| 2. למה | output תואם Google schedule |
| 3. F/C | **F** (output) |
| 4–7. | ❌ |
| 8. בטוח ל-22.06 | ⚠️ **חלקי** — DTOs |

---

## `Services/Optimizer/OptimizationProgressStore.cs`

| # | ניתוח |
|---|--------|
| 1. מה השתנה | lock; `Complete`+`GetResult`; cell upsert; delta streaming **מבוטל** (26.06) |
| 2. למה | async polling |
| 3. F/C | **F** |
| 4–7. | ❌ |
| 8. בטוח ל-22.06 | ✅ |

---

## `Services/Optimizer/OptimizeResultMapper.cs` (+70/-60)

| # | ניתוח |
|---|--------|
| 1. מה השתנה | `ReturnLeg`, coords, rejection summary, arc metadata, walking fields |
| 2. למה | DTO עשיר ל-frontend |
| 3. F/C | **F** (API) |
| 4–7. | ❌ |
| 8. בטוח ל-22.06 | ⚠️ **חלקי** — עם models.ts |

---

## `Services/Optimizer/OptimizerPipeline.cs` (+18/-19)

| # | ניתוח |
|---|--------|
| 1. מה השתנה | refactoring steps wiring; config references |
| 2. למה | תחזוקה |
| 3. F/C | F/C |
| 4–7. | ❌ |
| 8. בטוח ל-22.06 | ✅ |

---

## `Services/Optimizer/OptimizerRejectionTracker.cs`

| # | ניתוח |
|---|--------|
| 1. מה השתנה | `BuildSummary()` ל-API |
| 2. למה | rejection reasons ב-result |
| 3. F/C | F/C |
| 4–7. | ❌ |
| 8. בטוח ל-22.06 | ✅ |

---

## `Services/Optimizer/OptimizerDebugTrace.cs`, `OptimizerLog.cs`, `AgentDebugLog.cs`

| # | ניתוח |
|---|--------|
| 1. מה השתנה | config constants; minor logging |
| 2. למה | debug |
| 3. F/C | **C** |
| 4–7. | ❌ |
| 8. בטוח ל-22.06 | ✅ |

---

## `Services/Optimizer/TripScheduleDateHelper.cs`

| # | ניתוח |
|---|--------|
| 1. מה השתנה | helper לתאריכי schedule (קיים ב-22.06, עודכן) |
| 2. למה | cross-day trips |
| 3. F/C | **F** |
| 4–7. | 🟡 |
| 8. בטוח ל-22.06 | ✅ |

---

# חלק ג': Transit & Services

## `Services/Transit/ITransitApiService.cs` (+61/-3)

| # | ניתוח |
|---|--------|
| 1. מה השתנה | `TransitQueryResult`/`TransitDepartureOption` extended; `GetWalkingRouteAsync`, `GeocodeAddressAsync`, `GetTransitAccessToDestinationAsync` |
| 2. למה | contract לכל Google services |
| 3. F/C | **F** |
| 4–7. | ✅ Google + walking |
| 8. בטוח ל-22.06 | ⚠️ **חלקי** — breaking interface |

---

## `Services/Transit/MockTransitApiService.cs` (+25/-2)

| # | ניתוח |
|---|--------|
| 1. מה השתנה | mock implementations ל-methods חדשים |
| 2. למה | tests/dev |
| 3. F/C | **F** |
| 4–7. | 🟡 mock only |
| 8. בטוח ל-22.06 | ✅ עם interface |

---

## `Services/Transit/GoogleMapsApiException.cs`

| # | ניתוח |
|---|--------|
| 1. מה השתנה | exception type (קיים, עודכן) |
| 2. למה | error handling |
| 3. F/C | **F** |
| 4–7. | ❌ |
| 8. בטוח ל-22.06 | ✅ |

---

## `Services/DestinationService.cs`

| # | ניתוח |
|---|--------|
| 1. מה השתנה | `TravelerTypes` list; image resolver signature |
| 2. למה | M:N travelers |
| 3. F/C | **F** (API breaking) |
| 4–7. | ❌ |
| 8. בטוח ל-22.06 | ❌ **לא** — schema |

---

## `Services/ItineraryService.cs`

| # | ניתוח |
|---|--------|
| 1. מה השתנה | geocode; `MapPointBuilder`; image resolver |
| 2. למה | map enrichment |
| 3. F/C | **F** |
| 4–7. | 🟡 geocode on read |
| 8. בטוח ל-22.06 | ⚠️ **חלקי** |

---

## `Services/TripService.cs`

| # | ניתוח |
|---|--------|
| 1. מה השתנה | require `AddressStart`; `TravelerId` on NatureTrip; cascade delete |
| 2. למה | validation + FK fix |
| 3. F/C | **F** |
| 4. אלגוריתם | 🟡 travelerId |
| 5–7. | 🟡 |
| 8. בטוח ל-22.06 | ⚠️ delete fix=✅; rest=schema |

---

## `Services/DestinationImageResolver.cs` (+70/-7)

| # | ניתוח |
|---|--------|
| 1. מה השתנה | region-based fallback images |
| 2. למה | UX |
| 3. F/C | **C** |
| 4–7. | ❌ |
| 8. בטוח ל-22.06 | ✅ |

---

## `Services/IOptimizerService.cs`

| # | ניתוח |
|---|--------|
| 1. מה השתנה | `OptimizeTripAsync` → `Task`; `GetResult(traceId)` |
| 2. למה | async API |
| 3. F/C | **F** (breaking) |
| 4–7. | ❌ |
| 8. בטוח ל-22.06 | ❌ **לא standalone** |

---

## `Services/OptimizeResultCache.cs`

| # | ניתוח |
|---|--------|
| 1. מה השתנה | magic 0 → config constant |
| 2. למה | centralize |
| 3. F/C | **C** |
| 4–7. | ❌ |
| 8. בטוח ל-22.06 | ✅ |

---

## `Services/AuthService.cs`, `Middleware/HttpsOnlyMiddleware.cs`, `Services/Security/HttpsEnforcingHttpMessageHandler.cs`

| # | ניתוח |
|---|--------|
| 1. מה השתנה | config string constants; minor |
| 2. למה | centralize |
| 3. F/C | **C** |
| 4–7. | ❌ |
| 8. בטוח ל-22.06 | ✅ |

---

# חלק ד': Models & Data

| קובץ | מה השתנה | למה | F/C | Alg | TT | Google | 3D | 22.06 |
|------|-----------|-----|-----|-----|-----|--------|-----|-------|
| `ArcCost.cs` | +ArrivalTime, WaitBeforeDeparture, AlightingStationName | schedule metadata | F | 🟡 | 🟡 | ❌ | ❌ | ⚠️ |
| `OptimizationContext.cs` | +MinReturnHoursByDestId, RejectionSummary | pruning/debug | F | ✅ | ✅ | ❌ | ✅ | ⚠️ |
| `OptimizeResultDto.cs` | +ReturnLeg, StartLat/Lon | API | F | ❌ | ❌ | 🟡 | ❌ | ✅ |
| `ScoreTableStatsDto.cs` | +FilledCells, RejectionSummary | stats | F | ❌ | C | ❌ | C | ✅ |
| `StationToDestination.cs` | NOT NULL, RouteId, defaults | schema | F | 🟡 | ✅ | ✅ | ✅ | ❌ |
| `TripItineraryDto.cs` | +ReturnLeg, walking, MapPoint.kind | display | F | ❌ | ❌ | 🟡 | ❌ | ⚠️ |
| `Destination.cs` | הסרת TravelerId ישיר | M:N | F | ✅ | ✅ | ❌ | ✅ | ❌ |
| `DestinationDto.cs` | TravelerTypes list | API | F | ❌ | ❌ | ❌ | ❌ | ❌ |
| `NatureTrip.cs` | +TravelerId | preference | F | ✅ | ✅ | ❌ | ✅ | ⚠️ |
| `Bus.cs` | +Agency relation fields | Google | F | 🟡 | 🟡 | ✅ | 🟡 | ❌ |
| `CreateTripDto.cs` | validation fields | input | F | 🟡 | 🟡 | 🟡 | 🟡 | ⚠️ |
| `OptimizeRequestDto.cs` | minor | — | F/C | 🟡 | ❌ | ❌ | ❌ | ✅ |
| `OptimizerParams.cs` | config defaults | — | F/C | 🟡 | 🟡 | ❌ | 🟡 | ✅ |
| `OptimizerRoute.cs` | ReturnArc, wait fields | return leg | F | ✅ | 🟡 | 🟡 | ❌ | ⚠️ |
| `OptimizerDestination.cs` | walking fields | bootstrap | F | 🟡 | ✅ | ✅ | ✅ | ⚠️ |
| `GoogleMapsResponseModels.cs` | extended parsing models | Google | F | 🟡 | ✅ | ✅ | ✅ | ⚠️ |
| `OptimizationStepTraceDto.cs` | extended trace fields | debug | F/C | ❌ | C | ❌ | C | ✅ |
| `TripPlan.cs`, `TypeTraveler.cs`, `Bus.cs` | minor | — | F/C | ❌ | ❌ | ❌ | ❌ | ✅ |
| `TripContext.cs` | TravelersOfDestination, Agency, RouteId | schema | F | ✅ | ✅ | ✅ | ✅ | ❌ |
| `IOptimizerDataRepository.cs` | +travelerId param | filter | F | ✅ | ✅ | 🟡 | ✅ | ⚠️ |
| `OptimizerDataRepository.cs` | M:N travelers, walking>0 filter | filter | F | ✅ | ✅ | 🟡 | ✅ | ❌ |
| `ILookupRepository.cs`, `LookupRepository.cs` | **זהים** | — | — | — | — | — | — | N/A |

---

# חלק ה': Controllers & Program

## `Controllers/TripsController.cs` (+28/-9)

| # | ניתוח |
|---|--------|
| 1. מה השתנה | `POST optimize` → **202 Accepted** + traceId; `GET optimize/result/{traceId}` |
| 2. למה | async optimize |
| 3. F/C | **F** (breaking API) |
| 4–7. | ❌ |
| 8. בטוח ל-22.06 | ❌ **דורש frontend** |

---

## `Program.cs` (+17/-7)

| # | ניתוח |
|---|--------|
| 1. מה השתנה | DI: Google persistence, bootstrap, Places, StationRepo; JSON compact; config constants |
| 2. למה | wire new services |
| 3. F/C | **F** |
| 4–7. | 🟡 indirect |
| 8. בטוח ל-22.06 | ⚠️ **חלקי** |

---

# חלק ו': Frontend (81 changed — עיקריים)

## ⚠️ מצב היברידי בפרויקט הנוכחי

| קובץ | מצב פעיל | הערה |
|------|----------|------|
| `optimize-screen.component.ts` | **22.06 (restored)** | sync optimize — **לא תואם backend async** |
| `trip-result.component.ts` | **22.06 (restored)** | ללא ScoreTableGrid / Directions |
| `trip.service.ts` | **22.06 (restored)** | מצפה ל-sync POST |
| `models.ts` | **22.06 (restored)** | חסרים שדות 25.06 |
| `score-table-grid.component.ts` | **25.06 (orphaned)** | לא imported |
| `api-normalize.ts` | **25.06 (orphaned)** | לא used |

---

## `optimize-screen.component.ts`

| # | 22.06 | 25–26.06 |
|---|-------|----------|
| 1 | scroll log; sync POST | ScoreTableGrid; async poll; mergeScoreTableCells (26.06 מבוטל) |
| 2 | basic progress | async UX + grid |
| 3 | F | F |
| 4–7 | ❌ display only | ❌ |
| 8 | ✅ active | ⚠️ port with backend |

---

## `trip-result.component.ts`

| # | 22.06 | 25–26.06 |
|---|-------|----------|
| 1 | gated on itinerary; basic map | DirectionsRenderer; grid; no-itinerary view (26.06 מבוטל) |
| 2 | minimal | rich map + debug |
| 3 | F | F |
| 6. Google | Maps JS only | **Directions API** |
| 8 | ✅ active | ⚠️ port bundle |

---

## `trip.service.ts` + `trip-state.service.ts`

| # | 22.06 | 25–26.06 |
|---|-------|----------|
| 1 | sync optimize | 202 + getOptimizeResult; per-trip cache |
| 3 | F | F |
| 8 | ⚠️ **broken vs current backend** | ✅ should port |

---

## `models.ts`

| # | 22.06 | 25–26.06 |
|---|-------|----------|
| 1 | basic types | +rejectionReason, arcKind, returnLeg, walking, coords |
| 8 | ✅ active | ✅ required before other FE ports |

---

## קומפוננטות אחרות

| קובץ | שינוי עיקרי | F/C | Alg | TT | Google | 3D | 22.06 |
|------|-------------|-----|-----|-----|--------|-----|-------|
| `destinations-list` | table→cards (inferred) | C | ❌ | ❌ | ❌ | ❌ | ⚠️ |
| `trip-create/wizard` | autocomplete (inferred) | F | 🟡 | 🟡 | Places | ❌ | ⚠️ |
| `destination.service.ts` | +getRegionImageUrl (missing) | F | ❌ | ❌ | ❌ | ❌ | ⚠️ |
| `optimizer.component.ts` | minor UX | C | ❌ demo | ❌ | ❌ | ❌ | ✅ |
| `login/register/my-trips` | minor | C | ❌ | ❌ | ❌ | ❌ | ✅ |
| `styles.scss` | design tokens | C | ❌ | ❌ | ❌ | ❌ | ⚠️ |
| `environment.ts` | maps key | C | ❌ | ❌ | 🟡 | ❌ | ✅ |
| `index.html`, `app.component.ts` | branding | C | ❌ | ❌ | ❌ | ❌ | ✅ |

---

# חלק ז': Config & SQL & Project

| קובץ | מה | F/C | Alg | TT | Google | 3D | 22.06 |
|------|-----|-----|-----|-----|--------|-----|-------|
| `API_trip_link.csproj` | packages/refs | F/C | ❌ | ❌ | ❌ | ❌ | ⚠️ |
| `appsettings.json` | Google keys, timeouts | F | ❌ | ❌ | ✅ | ❌ | ⚠️ |
| `appsettings.Development.json` | dev settings | F/C | ❌ | ❌ | 🟡 | ❌ | ✅ |
| `ClearData.sql`, `SeedData.sql`, `TripSeedData.sql` | data changes | F | 🟡 | 🟡 | ❌ | 🟡 | ⚠️ |
| `.vscode/tasks.json` | tasks | C | ❌ | ❌ | ❌ | ❌ | ✅ |
| `trip-planner-app/dist/.../index.html` | build artifact | C | ❌ | ❌ | ❌ | ❌ | ❌ ignore |

---

# חלק ח': שינויי 26.06 (מבוטלים — לא פעילים)

| קובץ | שינוי | F/C | Alg | TT | Google | 3D | בטוח ל-22.06 |
|------|--------|-----|-----|-----|--------|-----|--------------|
| `OptimizationProgressStore.cs` | delta streaming | F | ❌ | ❌ | ❌ | ❌ | ✅ optional |
| `Step6_TripItineraryBuilder.cs` | arrival delay sync | F | ❌ | ❌ | ❌ | ❌ | ⚠️ test first |
| `Step2_ScoreTableBuilder.cs` | post-gate origin cells | F | ✅ | ✅ | ❌ | ✅ | ⚠️ test first |
| `Step2_ScoreTableBuilder.cs` | CountTheoreticalCells | F/C | ❌ | C | ❌ | C | ✅ |
| `optimize-screen.component.ts` | mergeScoreTableCells | F | ❌ | C | ❌ | C | ✅ |
| `optimize-screen.component.ts` | navigate with 0 dests | F | ❌ | ❌ | ❌ | ❌ | ✅ |
| `trip-result.component.ts` | show without itinerary | F | ❌ | C | ❌ | C | ✅ |

**הערה:** שינויי 26.06 נמצאים בהערות `[26.06.2026]` — לא רצים ב-production הנוכחי.

---

# נספח: מטריצת השפעה מרוכזת (קבצי Optimizer + Transit)

| קובץ | Alg | TT | Google | 3D | Port |
|------|:---:|:--:|:------:|:--:|:----:|
| Step2_ScoreTableBuilder | ✅ | ✅ | ✅ | ✅ | ⚠️ |
| ScoreTable | ✅ | ✅ | ❌ | ✅ | ⚠️ |
| RouteBuilder | ✅ | ❌ | ✅ | ❌ | ⚠️ |
| WeightCalculator | ✅ | ✅ | ❌ | ✅ | ⚠️ |
| ArcCostCalculator | ✅ | ✅ | ✅ | 🟡 | ⚠️ |
| TransitScheduleCollector | ✅ | ✅ | ✅ | ✅ | ⚠️ |
| GoogleMapsTransitApiService | ✅ | ✅ | ✅ | ✅ | ❌ |
| Step0_InputLoader | ✅ | 🟡 | ✅ | ✅ | ⚠️ |
| ReturnFeasibility | ✅ | ❌ | ❌ | ❌ | ✅ |
| ArcScheduleHelper | ✅ | ✅ | ❌ | ✅ | ✅ |
| OptimizerServiceImpl | ❌ | ❌ | ❌ | ❌ | ✅ |
| StationLinkBootstrapService | ✅ | ✅ | ✅ | ✅ | ⚠️ |

---

# מסקנות

1. **השינוי הגדול ביותר** הוא מעבר ממודל זמן סינתטי (`dep+bus+walk`) למודל מבוסס לוחות Google, עם שכבת **קשתות חזרה** בטבלה 3D.

2. **Google Directions** — עלייה בכמות ובסוגי הקריאות (transit batches, walking, geocode, driving baseline), אך עם **caps** חדשים שמגבילים סריקה.

3. **האלגוריתם (SA + RouteBuilder)** — משתנה מהותית בגלל return feasibility, schedule propagation, ו-return arc בסוף מסלול.

4. **Frontend–Backend mismatch** — הפרויקט הנוכחי מריץ backend 25.06 (async) עם frontend 22.06 (sync) — **לא יציב ל-production**.

5. **העברה ל-22.06** — אין קובץ בודד "בטוח"; יש **חבילות port**:
   - **חבילה A (infra):** Configuration, async API, progress store
   - **חבילה B (Google):** GoogleMapsTransitApiService + persistence + bootstrap
   - **חבילה C (algorithm):** Step2, ScoreTable, Collector, WeightCalculator, RouteBuilder, ArcCostCalculator
   - **חבילה D (schema):** SQL migrations + TripContext + repositories
   - **חבילה E (frontend):** models, api-normalize, trip.service, optimize-screen, trip-result, score-table-grid

6. **שינויי 26.06** — שמורים בהערות; ניתן להפעיל בנפרד לאחר ייצוב 25.06.

---

*נוצר אוטומטית — השוואת hash בין `גיבוי 22.06.2026\פרויקט טיול\API_trip_link` לפרויקט הנוכחי. לא בוצעו שינויים בקוד.*
