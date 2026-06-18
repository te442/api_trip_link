# TripLink — תרשימי UML מלאים

מסמך לספר פרויקט. כל התרשימים בפורמט Mermaid — ניתן להעתיק ל-[mermaid.live](https://mermaid.live) או לייצא כ-PNG/SVG.

---

## 0. תרשים זרימה ≠ תרשים UML — מה לשים בספר?

תרשים עם קופסות וחצים **מלמעלה למטה** (משתמש → Angular → Pipeline → שלבים → DB) הוא **תרשים ארכיטקטורה / זרימה** — שימושי להסבר, אבל **לא** תרשים UML תקני.

| מה יש בתרשים הידני | למה זה לא UML | מה להשתמש במקום |
|---------------------|---------------|------------------|
| קופסות ללא סוג (class/interface/component) | UML דורש סוג ישות ויחסים מוגדרים | **Class** או **Component** Diagram |
| כל השלבים «יוצאים» מה-Pipeline במקביל | Pipeline רץ **בסדר** 0→2→4→5→6 | **Sequence** Diagram או Component עם תלות מסודרת |
| לולאה אדומה על Step4/Step5 | לא סימון UML סטנדרטי | ב-Sequence: `loop` / `alt`; ב-Activity: צומת |
| חצים דו-כיווניים בכל מקום | Sequence = הודעות בזמן, לא קשר קבוע | חץ אחד = קריאה; חץ חוזר = תשובה |

**לסעיף 7 בספר — מומלץ:**

1. **תרשים רצף (Sequence)** — סעיף 7.2 (התרשים התקני לזרימת אופטימיזציה).
2. **תרשים רכיבים (Component)** — סעיף 7.1 (מבנה שכבות + DB/API).
3. **לא** להעתיק תרשים זרימה כללי כ«תרשים UML».

---

## 1. תרשים ארכיטקטורה (Component Diagram)

שכבות המערכת ותלויות חיצוניות.

```mermaid
flowchart TB
    subgraph Client["שכבת לקוח — Angular"]
        UI[Components<br/>Login, Wizard, Optimize, Result]
        SvcFE[Services<br/>Auth, Trip, Destination, Lookup]
        Guard[AuthGuard + Interceptor]
    end

    subgraph API["שכבת API — ASP.NET Core"]
        Ctrl[Controllers<br/>Auth, Trips, Destinations, Users, Lookups]
        AppSvc[Application Services<br/>Trip, Auth, Itinerary, Destination, Lookup]
        Opt[Optimizer Module<br/>Pipeline + Steps 0,2,4,5,6]
        Repo[Repositories<br/>OptimizerData, Lookup]
    end

    subgraph Data["שכבת נתונים"]
        EF[TripContext — EF Core]
        DB[(SQL Server<br/>TripLink)]
    end

    subgraph External["שירותים חיצוניים"]
        GM[Google Maps API<br/>Transit + Directions]
    end

    UI --> SvcFE
    Guard --> UI
    SvcFE -->|HTTP REST + JWT| Ctrl
    Ctrl --> AppSvc
    Ctrl --> Opt
    AppSvc --> EF
    Opt --> Repo
    Opt --> GM
    Repo --> EF
    EF --> DB
    AppSvc --> GM
```

---

## 2. תרשים פריסה (Deployment Diagram)

```mermaid
flowchart LR
    subgraph UserDevice["מחשב המשתמש"]
        Browser[דפדפן Chrome/Edge]
        Angular[trip-planner-app<br/>localhost:4200]
    end

    subgraph Server["שרת פיתוח"]
        DotNet[API_trip_link<br/>localhost:3000]
        SQL[(SQL Server)]
    end

    subgraph Cloud["ענן"]
        Google[Google Maps Platform]
    end

    Browser --> Angular
    Angular -->|REST JSON| DotNet
    DotNet --> SQL
    DotNet -->|HTTPS| Google
```

---

## 3. תרשים ישויות — מסד נתונים (ER Diagram)

```mermaid
erDiagram
    Users ||--o{ Trip : owns
    Trip ||--o{ DesOfTrip : contains
    Destination ||--o{ DesOfTrip : visited_in
    Trip ||--o{ CategoriesToTrip : has
    Category ||--o{ CategoriesToTrip : selected
    Trip ||--o{ FeatureToTrip : requires
    FeatureType ||--o{ FeatureToTrip : type
    Trip ||--o{ NatureTrip : difficulty_pref
    DifficultyLevel ||--o{ NatureTrip : level
    DifficultyLevel ||--o{ Destination : level
    TypeTraveler ||--o{ Destination : traveler_type
    Destination ||--o{ CategoriesOfDestination : categorized
    Category ||--o{ CategoriesOfDestination : category
    Destination ||--o{ DestinationFeature : has_feature
    FeatureType ||--o{ DestinationFeature : feature
    Destination ||--o{ StationToDestination : near_station
    Station ||--o{ StationToDestination : serves
    Station ||--o{ BusStation : on_line
    Bus ||--o{ BusStation : route

    Users {
        string user_id PK
        string FullName
        string Email
        string PasswordHash
        string Phon
    }

    Trip {
        int Trip_id PK
        string Trip_name
        string user_id FK
        datetime Trip_Date
        string Address_start
        time Start_time
        time End_time
    }

    Destination {
        int Des_id PK
        string Name_des
        string Rregion
        int level_id FK
        int Traveler_id FK
        decimal lat
        decimal lon
    }

    DesOfTrip {
        int Trip_id PK,FK
        int Des_id PK,FK
        int visit_number
    }

    Station {
        int StationNum PK
        string StationName
        string area
    }

    Bus {
        int BusId PK
        int BusNumber
        string Direction
    }
```

---

## 4. תרשים מחלקות — Controllers ו-Services (Backend)

```mermaid
classDiagram
    direction TB

  class AuthController {
        -AuthService _authService
        +Register(RegisterDto) AuthResponseDto
        +Login(LoginDto) AuthResponseDto
    }

    class TripsController {
        -TripService _tripService
        -IOptimizerService _optimizerService
        -ItineraryService _itineraryService
        +GetAll() List~TripDto~
        +GetById(id) TripDto
        +Create(CreateTripDto) TripDto
        +Delete(id)
        +Optimize(OptimizeRequestDto) OptimizeResultDto
        +GetItinerary(id) TripItineraryDto
        +SaveRoute(id, destinationIds)
    }

    class DestinationsController {
        -DestinationService _service
        +GetAll() List~DestinationDto~
    }

    class UsersController {
        -UserService _service
    }

    class LookupsController {
        -LookupService _service
    }

    class AuthService {
        +RegisterAsync(RegisterDto) AuthResponseDto
        +LoginAsync(LoginDto) AuthResponseDto
    }

    class TripService {
        +GetAllTripsAsync()
        +CreateTripAsync(CreateTripDto)
        +DeleteTripAsync(id)
        +SaveOptimizedRouteAsync(id, ids)
    }

    class ItineraryService {
        +GetItineraryAsync(tripId)
        +EnrichWithImagesAsync(result)
    }

    class DestinationService {
        +GetAllAsync()
    }

    class LookupService {
        +GetCategoriesAsync()
        +GetDifficultyLevelsAsync()
    }

    class IOptimizerService {
        <<interface>>
        +OptimizeTripAsync(request) OptimizeResultDto
    }

    class OptimizerServiceImpl {
        -OptimizerPipeline _pipeline
        -ItineraryService _itineraryService
        +OptimizeTripAsync(request)
    }

    AuthController --> AuthService
    TripsController --> TripService
    TripsController --> IOptimizerService
    TripsController --> ItineraryService
    DestinationsController --> DestinationService
    OptimizerServiceImpl ..|> IOptimizerService
    OptimizerServiceImpl --> OptimizerPipeline
    OptimizerServiceImpl --> ItineraryService
    TripService --> TripContext
    AuthService --> TripContext
```

---

## 5. תרשים מחלקות — מודול האופטימייזר

```mermaid
classDiagram
    direction TB

    class OptimizerPipeline {
        -IReadOnlyList~IOptimizerStep~ _steps
        +RunAsync(request) OptimizationContext
    }

    class IOptimizerStep {
        <<interface>>
        +StepNumber int
        +StepName string
        +ExecuteAsync(ctx)
    }

    class Step0_InputLoader {
        +StepNumber = 0
        +ExecuteAsync(ctx)
    }

    class Step2_ScoreTableBuilder {
        +StepNumber = 2
        +ExecuteAsync(ctx)
    }

    class Step4_InitialRouteBuilder {
        +StepNumber = 4
        +ExecuteAsync(ctx)
    }

    class Step5_SaOptimizer {
        +StepNumber = 5
        +GetNeighborRoute()
        +AcceptSolution()
        +CombinedCost()
    }

    class Step6_TripItineraryBuilder {
        +StepNumber = 6
        +FindBusLinesAsync()
        +BuildNarrative()
    }

    class OptimizationContext {
        +Request OptimizeRequestDto
        +Params OptimizerParams
        +Destinations List~OptimizerDestination~
        +ScoreTable ScoreTable
        +InitialRoute OptimizerRoute
        +BestRoute OptimizerRoute
        +SaResult SaLoopResult
        +TripPlan TripPlan
    }

    class ScoreTable {
        -ArcTransitionRecord[,,] _cells
        +Get(i,j,h) ArcTransitionRecord
        +GetBestInColumn(from, hour, candidates)
        +TimeToHourIndex(time)
        +DestIdToIndex(destId)
    }

    class RouteBuilder {
        +Build(scoreTable, allDest, allowed)
    }

    class ArcCostCalculator {
        +Compute(from, to, arrivalTime) ArcCost
    }

    class WeightCalculator {
        <<static>>
        +CalculateDestinationOptimality()
        +CheckTimeWindow()
        +CalculateTransitEfficiency()
    }

    class ArcCostCalculator {
        -ITransitApiService _transitApi
        -OptimizerParams _params
    }

    class IOptimizerDataRepository {
        <<interface>>
        +GetTripForOptimizationAsync()
        +GetDestinationsForOptimizationAsync()
        +GetBusLinesForStationAsync()
    }

    class ITransitApiService {
        <<interface>>
        +GetTransitTimeAsync(from, to, ...) TransitQueryResult
    }

    class GoogleMapsTransitApiService {
        +GetTransitTimeAsync()
    }

    class OptimizerRoute {
        +Destinations List
        +ArcCosts List
        +TotalScore double
        +TransitEfficiency double
        +Copy() OptimizerRoute
    }

    class ArcCost {
        +BusTransitHours double
        +BestDepartureTime DateTime
        +TransitEfficiency double
        +WalkingHours double
    }

    OptimizerPipeline o-- IOptimizerStep
    IOptimizerStep <|.. Step0_InputLoader
    IOptimizerStep <|.. Step2_ScoreTableBuilder
    IOptimizerStep <|.. Step4_InitialRouteBuilder
    IOptimizerStep <|.. Step5_SaOptimizer
    IOptimizerStep <|.. Step6_TripItineraryBuilder

    OptimizerPipeline --> OptimizationContext
    Step0_InputLoader --> IOptimizerDataRepository
    Step2_ScoreTableBuilder --> ArcCostCalculator
    Step2_ScoreTableBuilder --> WeightCalculator
    Step2_ScoreTableBuilder --> ScoreTable
    Step4_InitialRouteBuilder --> RouteBuilder
    Step5_SaOptimizer --> RouteBuilder
    RouteBuilder --> ScoreTable
    RouteBuilder --> ArcCostCalculator
    ArcCostCalculator --> ITransitApiService
    GoogleMapsTransitApiService ..|> ITransitApiService
    Step6_TripItineraryBuilder --> IOptimizerDataRepository
    OptimizationContext --> OptimizerRoute
    OptimizationContext --> ScoreTable
    OptimizationContext --> TripPlan
    OptimizerRoute --> ArcCost
```

---

## 6. תרשים מחלקות — Frontend Angular

```mermaid
classDiagram
    direction TB

    class AppComponent {
        +router Router
    }

    class LoginComponent {
        +login()
    }

    class RegisterComponent {
        +register()
    }

    class MyTripsComponent {
        +trips Trip[]
        +deleteTrip(id)
        +viewResult(id)
    }

    class TripWizardComponent {
        +step number
        +createTrip()
    }

    class OptimizeScreenComponent {
        +optimize()
    }

    class TripResultComponent {
        +itinerary TripItinerary
        +mapPoints MapPoint[]
    }

    class DestinationsListComponent {
        +destinations Destination[]
    }

    class AuthService {
        +login(credentials)
        +register(data)
        +getToken()
        +logout()
    }

    class TripService {
        +getTripsByUser(userId)
        +createTrip(dto)
        +optimize(request)
        +getItinerary(tripId)
        +deleteTrip(id)
    }

    class DestinationService {
        +getAll()
    }

    class LookupService {
        +getCategories()
        +getDifficultyLevels()
    }

    class AuthGuard {
        +canActivate()
    }

    class AuthInterceptor {
        +intercept(req, next)
    }

    LoginComponent --> AuthService
    RegisterComponent --> AuthService
    MyTripsComponent --> TripService
    TripWizardComponent --> TripService
    TripWizardComponent --> LookupService
    OptimizeScreenComponent --> TripService
    TripResultComponent --> TripService
    DestinationsListComponent --> DestinationService
    AuthGuard --> AuthService
    AuthInterceptor --> AuthService
    TripService ..> Trip : uses
    TripService ..> OptimizeResult : uses
```

---

## 7. אופטימיזציה — תרשימי UML לסעיף בספר

### 7.1 תרשים רכיבים (Component Diagram) — מבנה, לא זמן

תרשים UML תקני ל**מי מדבר עם מי** (לא סדר הרצה). מתאים להחליף תרשים הזרימה הידני.

```mermaid
flowchart TB
    subgraph Presentation["«Presentation»"]
        User((משתמש))
        FE["«component»<br/>OptimizeScreen"]
    end

    subgraph API["«API Layer»"]
        Ctrl["«component»<br/>TripsController"]
        Svc["«component»<br/>OptimizerServiceImpl"]
        Pipe["«component»<br/>OptimizerPipeline"]
    end

    subgraph OptimizerSteps["«Optimizer Steps»"]
        S0["«component»<br/>Step0_InputLoader"]
        S2["«component»<br/>Step2_ScoreTableBuilder"]
        S4["«component»<br/>Step4_InitialRouteBuilder"]
        S5["«component»<br/>Step5_SaOptimizer"]
        S6["«component»<br/>Step6_TripItineraryBuilder"]
        RB["«component»<br/>RouteBuilder"]
    end

    subgraph Data["«Data»"]
        DB[(«database»<br/>SQL Server)]
    end

    subgraph External["«External»"]
        GM["«service»<br/>Google Maps API"]
    end

    User --> FE
    FE --> Ctrl
    Ctrl --> Svc
    Svc --> Pipe
    Pipe --> S0
    Pipe --> S2
    Pipe --> S4
    Pipe --> S5
    Pipe --> S6
    S0 --> DB
    S2 --> GM
    S6 --> DB
    S5 --> RB
    S4 --> RB
    Svc --> DB
```

**סדר הרצה בפועל** (לא מופיע בתרשים רכיבים — רק בתרשים רצף):  
`S0 → S2 → S4 → S5 → S6`

---

### 7.2 תרשים רצף (Sequence Diagram) — **מתוקן לקוד (Steps 0→2→4→5→6)**

תרשים UML תקני, מותאם ל-`Program.cs` ול-Pipeline הנוכחי.  
**העתיקי את הבלוק למטה** ל-[mermaid.live](https://mermaid.live) לייצוא PNG.

```mermaid
sequenceDiagram
    autonumber
    actor User as משתמש
    participant FE as Angular<br/>OptimizeScreen
    participant Ctrl as TripsController
    participant Svc as OptimizerServiceImpl
    participant Pipe as OptimizerPipeline
    participant S0 as Step0_InputLoader
    participant S2 as Step2_ScoreTableBuilder
    participant S4 as Step4_InitialRouteBuilder
    participant S5 as Step5_SaOptimizer
    participant S6 as Step6_TripItineraryBuilder
    participant Itin as ItineraryService
    participant DB as SQL Server
    participant GM as Google Maps API

    User->>FE: לחיצה «חשב מסלול אופטימלי»
    FE->>Ctrl: POST /api/trips/optimize<br/>(OptimizeRequestDto)
    Ctrl->>Svc: OptimizeTripAsync(request)
    Svc->>Pipe: RunAsync(request)

    Note over Pipe: new OptimizationContext { Request }

    Pipe->>S0: ExecuteAsync(ctx)
    S0->>DB: GetTripForOptimizationAsync(tripId)
    DB-->>S0: Trip
    S0->>DB: GetDestinationsForOptimizationAsync<br/>(region, level, categories, features)
    DB-->>S0: List Destination
    S0->>S0: בניית OptimizerParams + סינון
    S0-->>Pipe: ctx.Destinations, ctx.Params

    Pipe->>S2: ExecuteAsync(ctx)
    loop לכל מקור i, יעד j, שכבת שעה h
        S2->>GM: GetTransitTimeAsync(from, to, departureTime)
        GM-->>S2: TransitQueryResult
        S2->>S2: ArcCostCalculator.Compute
        S2->>S2: WeightCalculator.CalculateDestinationOptimality
        S2->>S2: שמירה ב-ScoreTable[i,j,h]
    end
    S2-->>Pipe: ctx.ScoreTable

    Pipe->>S4: ExecuteAsync(ctx)
    S4->>S4: RouteBuilder.Build(scoreTable, destinations)
    Note over S4: GetBestInColumn — מסלול גרעיני (ללא SA)
    S4-->>Pipe: ctx.InitialRoute

    Pipe->>S5: ExecuteAsync(ctx)
    loop i ≤ MaxIterations && T > MinTemperature
        S5->>S5: GetNeighborRoute<br/>(הוסף / הסר / swap)
        S5->>S5: RouteBuilder.Build(allowedList)
        alt neighbor.IsValid && AcceptSolution(delta, T)
            S5->>S5: currentRoute = neighbor
            S5->>S5: עדכון bestRoute אם CombinedCost טוב יותר
        end
        S5->>S5: temperature *= (1 - CoolingRate)
    end
    S5-->>Pipe: ctx.BestRoute, ctx.SaResult

    Pipe->>S6: ExecuteAsync(ctx)
    loop לכל יעד ב-BestRoute
        S6->>DB: GetBusLinesForStationAsync(stationNum)
        DB-->>S6: BusStations + Buses
        S6->>S6: TripLeg + TransitSegment + זמנים
    end
    S6->>S6: BuildNarrative()
    S6-->>Pipe: ctx.TripPlan

    Pipe-->>Svc: OptimizationContext
    Svc->>Svc: OptimizeResultMapper.Map(ctx)
    Svc->>Itin: EnrichWithImagesAsync(result)
    Itin->>DB: תמונות יעדים (ImageUrl)
    DB-->>Itin: URLs
    Itin-->>Svc: OptimizeResultDto (מעושר)
    Svc-->>Ctrl: OptimizeResultDto
    Ctrl-->>FE: 200 OK (JSON)
    FE-->>User: ניווט ל-/trips/{id}/result

    opt שמירה מסלול ב-DB (נפרד — לא בתוך OptimizeTripAsync)
        FE->>Ctrl: POST /api/trips/{id}/save-route
        Ctrl->>DB: SaveOptimizedRouteAsync
    end
```

**מה הוסר מהתבנית הישנה (לא בקוד):** Step3_TravelTimeBuilder, Step4_TripOptimizer, Step5_TripSummaryBuilder, GetDistMatrixAsync, SaveResultToDbAsync בתוך אופטימיזציה.

---

### 7.3 תרשים רצף מקוצר (לספר — קל לציור ב-Word / draw.io)

פחות participants — מתאים לעמוד אחד בספר.

```mermaid
sequenceDiagram
    actor U as משתמש
    participant FE as Angular
    participant API as TripsController
    participant Opt as OptimizerService
    participant DB as SQL Server
    participant GM as Google Maps

    U->>FE: חשב מסלול
    FE->>API: POST /optimize
    API->>Opt: OptimizeTripAsync()

    Opt->>DB: Step0: טיול + יעדים מסוננים
    DB-->>Opt: Destinations

    loop Step2: בניית טבלה
        Opt->>GM: זמני נסיעה
        GM-->>Opt: TransitResult
    end

    Opt->>Opt: Step4: מסלול גרעיני

    loop Step5: SA
        Opt->>Opt: שכן + Accept/Reject
    end

    Opt->>DB: Step6: קווי אוטובוס
    DB-->>Opt: BusLines

    Opt-->>API: OptimizeResultDto
    API-->>FE: JSON
    FE-->>U: מסך תוצאות
```

---

## 8. תרשים רצף — זרימת משתמש (Use Case Flow)

```mermaid
sequenceDiagram
    autonumber
    actor User as משתמש
    participant FE as Angular
    participant Auth as AuthController
    participant Trips as TripsController

    User->>FE: הרשמה / התחברות
    FE->>Auth: POST register / login
    Auth-->>FE: JWT token

    User->>FE: תכנון טיול (אשף 3 שלבים)
    FE->>Trips: POST /api/trips
    Trips-->>FE: TripId

    User->>FE: חשב מסלול
    FE->>Trips: POST /api/trips/optimize
    Trips-->>FE: OptimizeResultDto
    FE->>Trips: POST save-route (אופציונלי)

    User->>FE: צפייה במסלול (מאוחר)
    FE->>Trips: GET /api/trips/{id}/itinerary
    Trips-->>FE: TripItineraryDto
```

---

## 9. תרשים פעילות — Pipeline האופטימייזר (Activity Diagram)

```mermaid
flowchart TD
    Start([קלט: OptimizeRequestDto]) --> S0

    S0[שלב 0: סינון יעדים<br/>אזור, קושי, קטגוריות, מאפיינים]
    S0 --> S2

    S2[שלב 2: בניית טבלה 3D<br/>ArcCost + Optimality לכל תא]
    S2 --> S4

    S4[שלב 4: מסלול ראשוני גרעיני<br/>RouteBuilder + GetBestInColumn]
    S4 --> S5

    S5{שלב 5: SA Loop<br/>temperature > min?}
    S5 -->|כן| Neighbor[יצירת שכן:<br/>הוסף / הסר / swap]
    Neighbor --> Rebuild[RouteBuilder.Build מחדש]
    Rebuild --> Accept{AcceptSolution?}
    Accept -->|כן| UpdateBest[עדכון BestRoute אם טוב יותר]
    Accept -->|לא| Cool
    UpdateBest --> Cool[קירור טמפרטורה]
    Cool --> S5

    S5 -->|לא| S6
    S6[שלב 6: בניית TripPlan<br/>Legs, BusLines, Narrative]
    S6 --> Map[OptimizeResultMapper]
    Map --> End([פלט: OptimizeResultDto])

    S0 -->|0 יעדים| Err([שגיאה / מסלול ריק])
```

---

## 10. תרשים מקרי שימוש (Use Case Diagram)

```mermaid
flowchart LR
    subgraph System["TripLink"]
        UC1[הרשמה / התחברות]
        UC2[יצירת טיול]
        UC3[אופטימיזציה מסלול]
        UC4[צפייה בתוצאות]
        UC5[ניהול טיולים]
        UC6[צפייה ביעדים]
        UC7[חיפוש Lookups]
    end

    User((משתמש)) --> UC1
    User --> UC2
    User --> UC3
    User --> UC4
    User --> UC5
    User --> UC6
    User --> UC7

    UC3 -.->|include| GM[Google Maps]
    UC3 -.->|include| DB[(מסד נתונים)]
```

---

## 11. מפת נתיבים — Frontend Routes

| נתיב | Component | Auth |
|------|-----------|------|
| `/login` | LoginComponent | — |
| `/register` | RegisterComponent | — |
| `/my-trips` | MyTripsComponent | ✓ |
| `/plan` | TripWizardComponent | ✓ |
| `/plan/optimize/:tripId` | OptimizeScreenComponent | ✓ |
| `/trips/:id/result` | TripResultComponent | ✓ |
| `/destinations` | DestinationsListComponent | ✓ |

---

## 12. מפת API Endpoints

| Method | Endpoint | Controller |
|--------|----------|------------|
| POST | `/api/auth/register` | AuthController |
| POST | `/api/auth/login` | AuthController |
| GET | `/api/trips` | TripsController |
| GET | `/api/trips/{id}` | TripsController |
| GET | `/api/trips/user/{userId}` | TripsController |
| POST | `/api/trips` | TripsController |
| DELETE | `/api/trips/{id}` | TripsController |
| POST | `/api/trips/optimize` | TripsController |
| GET | `/api/trips/{id}/itinerary` | TripsController |
| POST | `/api/trips/{id}/save-route` | TripsController |
| GET | `/api/destinations` | DestinationsController |
| GET | `/api/users` | UsersController |
| GET | `/api/lookups/*` | LookupsController |

---

*גרסה 1.0 — מעודכן ל-Pipeline: Step 0 → 2 → 4 → 5 → 6*
