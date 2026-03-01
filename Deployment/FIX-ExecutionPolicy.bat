@echo off
REM =====================================================
REM PowerShell Execution Policy Fixer
REM =====================================================
REM Bu script PowerShell execution policy'yi düzeltir
REM =====================================================

echo.
echo ===================================================
echo  PowerShell Execution Policy Duzelticisi
echo  Gemini Translate - Zafer Bilgisayar
echo ===================================================
echo.

REM Admin kontrolü
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo [X] HATA: YONETÝCÝ yetkisi gerekli!
    echo.
    echo Sag tik ^> "Yonetici olarak calistir"
    echo.
    pause
    exit /b 1
)

echo [1] Mevcut ayarlar kontrol ediliyor...
echo.
powershell -Command "Get-ExecutionPolicy -List"
echo.

echo [2] Execution Policy ayarlaniyor (RemoteSigned)...
echo.
powershell -Command "Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser -Force"

if %errorLevel% equ 0 (
    echo.
    echo ===================================================
    echo  [^] BASARILI! PowerShell script'leri artik calisir
    echo ===================================================
    echo.
    echo Yeni ayar: RemoteSigned (CurrentUser)
    echo.
    echo Artik Fix-TrustError.ps1'i calistirabilirsiniz:
    echo   1. Fix-TrustError.ps1'e sag tik
    echo   2. "PowerShell ile calistir" secin
    echo.
) else (
    echo.
    echo [X] HATA: Ayar degistirilemedi!
    echo.
)

echo.
pause
