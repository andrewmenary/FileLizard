@ECHO OFF

REM The following directory is for .NET 4.0+
set DOTNETFX4=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319
set PATH=%PATH%;%DOTNETFX4%

echo Installing FileLizard Service...
echo ---------------------------------------------------
InstallUtil /i FileLizard.exe
echo ---------------------------------------------------
echo Done.
pause
