@echo off
REM =====================================================
REM Gemini Outlook Translate - Trust Error Fix Launcher
REM =====================================================
REM Bu .bat dosyasý PowerShell execution policy'yi
REM bypass ederek Fix-TrustError.ps1'i çalýþtýrýr
REM =====================================================

echo.
echo ===================================================
echo  Gemini Translate - Trust Error Fix
echo  Zafer Bilgisayar by Auto-System
echo ===================================================
echo.

REM Admin kontrolü
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo [X] HATA: Bu script YONETÝCÝ yetkisi gerektirir!
    echo.
    echo Sag tik ^> "Yonetici olarak calistir"
    echo.
    pause
    exit /b 1
)

echo [1] PowerShell Execution Policy bypass ediliyor...
echo.

REM PowerShell script'i Bypass ile çalýþtýr
powershell.exe -NoProfile -ExecutionPolicy Bypass -Command ^
    "& '%~dp0Fix-TrustError.ps1' -Both"

echo.
echo ===================================================
echo  Tamamlandi!
echo ===================================================
echo.
pause
