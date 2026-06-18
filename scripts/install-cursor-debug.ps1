# פתיחת דף התקנת הרחבת C# Dev Kit ב-Cursor
$cursor = "$env:LOCALAPPDATA\Programs\cursor\Cursor.exe"
$workspace = Split-Path -Parent $MyInvocation.MyCommand.Path | Split-Path -Parent

Write-Host "מנסה להתקין C# Dev Kit..."
& $cursor --install-extension ms-dotnettools.csdevkit 2>&1
& $cursor --install-extension ms-dotnettools.csharp 2>&1

Write-Host ""
Write-Host "פותח את חלונית ההרחבות ב-Cursor..."
Start-Process $cursor -ArgumentList @(
    $workspace,
    "--reuse-window"
)

Write-Host ""
Write-Host "ב-Cursor:"
Write-Host "  1. Ctrl+Shift+X  -> Extensions"
Write-Host "  2. חפשי: C# Dev Kit"
Write-Host "  3. Install"
Write-Host "  4. Ctrl+Shift+D  -> Debug"
Write-Host "  5. F5 -> .NET: Launch API"
