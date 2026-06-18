@echo off
taskkill /IM API_trip_link.exe /F 2>nul
timeout /t 1 /nobreak >nul
dotnet run
