@echo off
chcp 65001 >nul

:: =========================================================
::   Gemini Outlook Translate - Tek Tikla Kurulum
::   Zafer Bilgisayar by Auto-System
:: =========================================================

:: Admin degilse otomatik yetki yukselt (UAC)
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo Yonetici yetkisi isteniyor...
    powershell -Command "Start-Process cmd -ArgumentList '/c \"%~f0\"' -Verb RunAs"
    exit /b
)

title Gemini Translate - Kurulum
color 0B

echo.
echo =========================================================
echo   Gemini Outlook Translate Add-in - Kurulum
echo   Zafer Bilgisayar by Auto-System
echo =========================================================
echo.
echo Bu kurulum iki islem yapar:
echo   1) Imza sertifikasini guvenilir listeye ekler
echo   2) Eklentiyi kurar (setup.exe)
echo.

:: Sertifika dosyasini bul
set "CERT_FILE="
if exist "%~dp0GeminiTranslate-CodeSigning.cer" (
    set "CERT_FILE=%~dp0GeminiTranslate-CodeSigning.cer"
    echo Sertifika bulundu: GeminiTranslate-CodeSigning.cer
) else if exist "%~dp0*.cer" (
    for %%f in ("%~dp0*.cer") do set "CERT_FILE=%%f"
    echo Sertifika bulundu: %CERT_FILE%
) else (
    echo [UYARI] .cer sertifika dosyasi bulunamadi!
    echo Sertifika olmadan devam ediliyor...
    echo ClickOnce imza uyarisi gorunebilir.
    echo.
    goto :SETUP
)

echo.
echo [1/2] Sertifika guvenilir listeye ekleniyor...
echo.

:: Trusted Root Certificate Authorities
certutil -addstore "Root" "%CERT_FILE%" >nul 2>&1
if %errorLevel% equ 0 (
    echo    Trusted Root Certificate Authorities - OK
) else (
    echo    Trusted Root - Zaten ekli veya hata olustu
)

:: Trusted Publishers (eklenti bloklanmasini onler)
certutil -addstore "TrustedPublisher" "%CERT_FILE%" >nul 2>&1
if %errorLevel% equ 0 (
    echo    Trusted Publishers - OK
) else (
    echo    Trusted Publishers - Zaten ekli veya hata olustu
)

echo.
echo Sertifika yukleme tamamlandi!
echo.

:SETUP
:: VSTO eklentiyi kur (VSTOInstaller veya setup.exe)
set "VSTO_FILE=%~dp0GeminiOutlookTranslateAdd-in.vsto"
set "VSTOINSTALLER=%CommonProgramFiles%\Microsoft Shared\VSTO\10.0\VSTOInstaller.exe"

if exist "%VSTO_FILE%" (
    if exist "%VSTOINSTALLER%" (
        echo [2/2] Eklenti kuruluyor (VSTOInstaller)...
        echo.
        "%VSTOINSTALLER%" /Install "%VSTO_FILE%"
    ) else (
        echo [2/2] .vsto dosyasi ile kuruluyor...
        echo.
        start "" "%VSTO_FILE%"
    )
) else if exist "%~dp0setup.exe" (
    echo [2/2] setup.exe baslatiliyor...
    echo.
    start "" "%~dp0setup.exe"
) else (
    echo [HATA] Ne .vsto ne de setup.exe bulunamadi!
    echo Lutfen ZIP'i bir klasore cikarip tekrar deneyin.
)

echo.
echo =========================================================
echo   Kurulum baslatildi!
echo.
echo   Kurulum tamamlandiktan sonra:
echo   1. Outlook'u yeniden baslatin
echo   2. "Ceviri" tabinda API Key'inizi girin
echo =========================================================
echo.
pause
