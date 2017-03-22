@setlocal enableextensions
@cd /d "%~dp0"

set DIR="C:\Windows\Microsoft.NET\Framework\v4.0.30319"

echo %DIR%

%DIR%\InstallUtil.exe "..\bin\Debug\PoloniexService.exe"

pause
