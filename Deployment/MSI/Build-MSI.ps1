# =====================================================
# Gemini Outlook Translate - MSI Build Script
# =====================================================
# WiX 6 ile MSI paketi olusturur.
#
# Kullanim:
#   .\Deployment\MSI\Build-MSI.ps1
# =====================================================

$ErrorActionPreference = "Stop"

# Yollar
$MSIDir       = Split-Path -Parent $MyInvocation.MyCommand.Path
$SolutionRoot = Split-Path -Parent (Split-Path -Parent $MSIDir)
$BinDir       = Join-Path $SolutionRoot "GeminiOutlookTranslateAdd-in\bin\Release"
$CertDir      = Join-Path $SolutionRoot "Deployment"
$WxsFile      = Join-Path $MSIDir "GeminiTranslate.wxs"
$ReleasesDir  = Join-Path $SolutionRoot "releases"

# Versiyon oku
$assemblyInfo = Get-Content (Join-Path $SolutionRoot "GeminiOutlookTranslateAdd-in\Properties\AssemblyInfo.cs") -Raw
if ($assemblyInfo -match 'AssemblyVersion\("(\d+\.\d+\.\d+)\.(\d+)"\)') {
    $version3 = $Matches[1]
    $version4 = "$($Matches[1]).$($Matches[2])"
}
else {
    Write-Host "HATA: AssemblyInfo.cs'de versiyon bulunamadi!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================================" -ForegroundColor Cyan
Write-Host " Gemini Translate - MSI Build" -ForegroundColor Yellow
Write-Host " Versiyon: $version3" -ForegroundColor Gray
Write-Host "========================================================" -ForegroundColor Cyan
Write-Host ""

# DLL kontrolu
$mainDll = Join-Path $BinDir "GeminiOutlookTranslateAdd-in.dll"
if (-not (Test-Path $mainDll)) {
    Write-Host "HATA: bin\Release'de DLL bulunamadi! Once projeyi build edin." -ForegroundColor Red
    exit 1
}

$dllVer = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($mainDll).FileVersion
Write-Host "DLL versiyon: $dllVer" -ForegroundColor Gray

# WiX kontrolu
$wixCmd = Get-Command wix -ErrorAction SilentlyContinue
if (-not $wixCmd) {
    Write-Host "HATA: WiX CLI bulunamadi! 'dotnet tool install --global wix' ile yukleyin." -ForegroundColor Red
    exit 1
}

Write-Host "WiX: $(wix --version)" -ForegroundColor Gray

# releases klasoru
if (-not (Test-Path $ReleasesDir)) {
    New-Item -ItemType Directory -Path $ReleasesDir -Force | Out-Null
}

# MSI olustur
$msiPath = Join-Path $ReleasesDir "GeminiTranslate-v$version3-setup.msi"

Write-Host ""
Write-Host "MSI olusturuluyor..." -ForegroundColor Yellow

wix build `
    -d "BinDir=$BinDir" `
    -d "CertDir=$CertDir" `
    -d "ScriptDir=$MSIDir" `
    -d "Version=$version4" `
    -o "$msiPath" `
    "$WxsFile"

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "HATA: MSI olusturulamadi!" -ForegroundColor Red
    exit 1
}

$msiSize = [math]::Round((Get-Item $msiPath).Length / 1MB, 1)

Write-Host ""
Write-Host "========================================================" -ForegroundColor Cyan
Write-Host " MSI BASARIYLA OLUSTURULDU!" -ForegroundColor Green
Write-Host "========================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Dosya:    $msiPath" -ForegroundColor White
Write-Host "  Boyut:    $msiSize MB" -ForegroundColor White
Write-Host "  Versiyon: $version3" -ForegroundColor White
Write-Host ""
Write-Host "  Kurulum:  msiexec /i `"$msiPath`"" -ForegroundColor Gray
Write-Host "  Sessiz:   msiexec /i `"$msiPath`" /qn" -ForegroundColor Gray
Write-Host ""
