# TripLink — Frontend (Angular)

אפליקציית הלקוח של מערכת TripLink, ממוקמת בתוך תיקיית ה-API.

## הרצה

```bash
# טרמינל 1 — API (HTTPS)
cd ..
dotnet run --launch-profile https

# טרמינל 2 — Angular (HTTPS)
npm start
```

- API: https://localhost:7271
- Angular: https://localhost:4200

> בפעם הראשונה: `dotnet dev-certs https --trust` (אישור תעודת פיתוח)
> ואז מתוך `trip-planner-app`: `npm run setup-certs` (תעודת HTTPS ל-Angular)

## בעיות נפוצות

### ERR_CERT_AUTHORITY_INVALID בדפדפן

Angular צריך את **אותה תעודת פיתוח** שכבר אישרת ל-API:

```bash
npm run setup-certs
```

ואז הפעל מחדש: `npm start` → פתח **https://localhost:4200**

### "Port 4200 is already in use" / Angular נתקע

זה קורה כשיש **מופע ישן** של `ng serve` שעדיין רץ.

1. בטרמינל התקוע: לחץ `Ctrl+C`
2. שחרר את הפורט (PowerShell):

```powershell
Get-NetTCPConnection -LocalPort 4200 -State Listen | ForEach-Object { Stop-Process -Id $_.OwningProcess -Force }
```

3. הרץ שוב: `npm start`

### סדר הרצה נכון

1. **טרמינל 1** — מתיקיית `API_trip_link`:
   `dotnet run --launch-profile https`
2. **טרמינל 2** — מתיקיית `trip-planner-app`:
   `npm start`
3. פתח בדפדפן: **https://localhost:4200** (לא http)

## זרימת מסכים

1. `/login` / `/register` — התחברות
2. `/my-trips` — הטיולים שלי
3. `/plan` — אשף תכנון (3 שלבים: פרטים → העדפות → זמנים)
4. `/plan/optimize/:tripId` — חישוב מסלול
5. `/trips/:id/result` — תוצאות: מפה + timeline + יעדים עם תמונות

## Google Maps

הגדר מפתח ב-`src/environments/environment.ts`:
```typescript
googleMapsApiKey: 'YOUR_KEY'
```
והפעל **Maps JavaScript API** ב-Google Cloud Console.

## מבנה

- `src/app/components/` — מסכי האפליקציה
- `src/app/services/` — שירותי API
- `src/app/guards/` — AuthGuard
- `src/app/interceptors/` — JWT interceptor
