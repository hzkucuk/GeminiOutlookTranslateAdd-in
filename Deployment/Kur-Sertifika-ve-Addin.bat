@echo off
chcp 65001 >nul
title Gemini Translate - Sertifika Yukle ve Kur
color 0B

echo.
echo =========================================================
echo   Gemini Outlook Translate Add-in - Kurulum
echo   Zafer Bilgisayar by Auto-System
echo =========================================================
echo.
echo Bu script iki islem yapar:
echo   1) Imza sertifikasini guvenilir listeye ekler
echo   2) setup.exe ile add-in'i kurar
echo.
echo UYARI: Yonetici yetkisi gereklidir!
echo.

:: Yonetici kontrolu
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo [HATA] Bu script Yonetici olarak calistirilmalidir!
    echo.
    echo Sag tiklayin ^> "Yonetici olarak calistir" secin.
    echo.
    pause
    exit /b 1
)

:: Sertifika dosyasini bul
set "CERT_FILE="
if exist "%~dp0ZaferBilgisayar-CodeSigning.cer" (
    set "CERT_FILE=%~dp0ZaferBilgisayar-CodeSigning.cer"
    echo Sertifika bulundu: ZaferBilgisayar-CodeSigning.cer
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

:: Trusted Publishers (ClickOnce uyarisi icin kritik)
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
:: setup.exe'yi bul ve calistir
if exist "%~dp0setup.exe" (
    echo [2/2] setup.exe baslatiliyor...
    echo.
    start "" "%~dp0setup.exe"
) else (
    echo [BILGI] setup.exe bu klasorde bulunamadi.
    echo Publish klasorundeki setup.exe'yi calistirin.
)

echo.
echo =========================================================
echo   Kurulum tamamlandi!
echo   Outlook'u yeniden baslatin.
echo =========================================================
echo.
pause
