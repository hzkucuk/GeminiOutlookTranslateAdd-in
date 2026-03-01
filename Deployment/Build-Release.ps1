# =====================================================
# Gemini Outlook Translate - Build & Release Script
# =====================================================
# Tek komutla: Versiyon kontrol -> Build -> ZIP -> Commit -> Tag -> Push
#
# Kullanim:
#   .\Deployment\Build-Release.ps1                         # mevcut versiyon ile
#   .\Deployment\Build-Release.ps1 -NewVersion "1.4.0"     # yeni versiyon belirle
#   .\Deployment\Build-Release.ps1 -SkipPush               # push yapma (test icin)
# =====================================================

param(
    [string]$NewVersion = "",
    [switch]$SkipPush,
    [switch]$SkipZip
)

$ErrorActionPreference = "Stop"

# =====================================================
# YAPILANDIRMA
# =====================================================

$SolutionRoot = Split-Path -Parent $PSScriptRoot
if (-not (Test-Path (Join-Path $SolutionRoot "GeminiOutlookTranslateAdd-in.slnx"))) {
    $SolutionRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
}

$AssemblyInfoPath = Join-Path $SolutionRoot "GeminiOutlookTranslateAdd-in\Properties\AssemblyInfo.cs"
$CsprojPath       = Join-Path $SolutionRoot "GeminiOutlookTranslateAdd-in\GeminiOutlookTranslateAdd-in.csproj"
$ChangelogPath    = Join-Path $SolutionRoot "CHANGELOG.md"
$ReleasesDir      = Join-Path $SolutionRoot "releases"
$PublishDir       = Join-Path $SolutionRoot "GeminiOutlookTranslateAdd-in\bin\Release\app.publish"
$DeploymentDir    = Join-Path $SolutionRoot "Deployment"

# =====================================================
# YARDIMCI FONKSIYONLAR
# =====================================================

function Write-Step($step, $message) {
    Write-Host ""
    Write-Host "[$step] $message" -ForegroundColor Cyan
    Write-Host ("-" * 50) -ForegroundColor DarkGray
}

function Write-OK($message) {
    Write-Host "   OK: $message" -ForegroundColor Green
}

function Write-Fail($message) {
    Write-Host "   HATA: $message" -ForegroundColor Red
}

function Get-AssemblyVersion {
    $content = Get-Content $AssemblyInfoPath -Raw
    if ($content -match '\[assembly:\s*AssemblyVersion\("(\d+\.\d+\.\d+)\.\d+"\)\]') {
        return $Matches[1]
    }
    throw "AssemblyInfo.cs'de versiyon bulunamadi!"
}

function Set-AssemblyVersion($ver) {
    $content = Get-Content $AssemblyInfoPath -Raw
    $content = $content -replace '(\[assembly:\s*AssemblyVersion\(")[\d\.]+("\)\])', "`${1}$ver.0`${2}"
    $content = $content -replace '(\[assembly:\s*AssemblyFileVersion\(")[\d\.]+("\)\])', "`${1}$ver.0`${2}"
    Set-Content -Path $AssemblyInfoPath -Value $content -NoNewline
}

function Get-CsprojApplicationVersion {
    $content = Get-Content $CsprojPath -Raw
    if ($content -match '<ApplicationVersion>([\d\.]+)</ApplicationVersion>') {
        return $Matches[1]
    }
    return $null
}

function Set-CsprojApplicationVersion($ver) {
    $content = Get-Content $CsprojPath -Raw
    $content = $content -replace '<ApplicationVersion>[\d\.]+</ApplicationVersion>', "<ApplicationVersion>$ver.0</ApplicationVersion>"
    Set-Content -Path $CsprojPath -Value $content -NoNewline
}

function Test-ChangelogHasVersion($ver) {
    $content = Get-Content $ChangelogPath -Raw
    return $content -match "\[$ver\]"
}

# =====================================================
# BANNER
# =====================================================

Write-Host ""
Write-Host "========================================================" -ForegroundColor Cyan
Write-Host " Gemini Translate - Build & Release Pipeline" -ForegroundColor Yellow
Write-Host " Zafer Bilgisayar by Auto-System" -ForegroundColor Gray
Write-Host "========================================================" -ForegroundColor Cyan

# =====================================================
# ADIM 1: VERSIYON BELIRLEME
# =====================================================

Write-Step "1/7" "Versiyon kontrolu"

$currentVersion = Get-AssemblyVersion
Write-Host "   Mevcut versiyon: $currentVersion" -ForegroundColor Gray

if (-not [string]::IsNullOrWhiteSpace($NewVersion)) {
    $version = $NewVersion
    Write-Host "   Yeni versiyon:   $version" -ForegroundColor Yellow
    
    Set-AssemblyVersion $version
    Write-OK "AssemblyInfo.cs guncellendi"
}
else {
    $version = $currentVersion
    Write-Host "   Mevcut versiyon kullanilacak: $version" -ForegroundColor Gray
}

# =====================================================
# ADIM 2: VERSIYON TUTARLILIGI
# =====================================================

Write-Step "2/7" "Versiyon tutarliligi kontrolu"

$hasErrors = $false

# CHANGELOG kontrolu
if (Test-ChangelogHasVersion $version) {
    Write-OK "CHANGELOG.md [$version] iceriyor"
}
else {
    Write-Fail "CHANGELOG.md'de [$version] bulunamadi!"
    Write-Host "   Lutfen CHANGELOG.md'ye [$version] bolumu ekleyin." -ForegroundColor Yellow
    $hasErrors = $true
}

# csproj ApplicationVersion senkronizasyonu
$csprojVer = Get-CsprojApplicationVersion
if ($csprojVer -ne "$version.0") {
    Write-Host "   csproj ApplicationVersion senkronize ediliyor: $csprojVer -> $version.0" -ForegroundColor Yellow
    Set-CsprojApplicationVersion $version
    Write-OK "csproj ApplicationVersion guncellendi"
}
else {
    Write-OK "csproj ApplicationVersion zaten senkron"
}

if ($hasErrors) {
    Write-Host ""
    Write-Host "HATA: Versiyon tutarsizligi! Yukaridaki hatalari duzeltin." -ForegroundColor Red
    exit 1
}

# =====================================================
# ADIM 3: BUILD (Release) — VSTO icin devenv.exe kullanilir
# =====================================================

Write-Step "3/7" "Release build"

$devenvPaths = @(
    "${env:ProgramFiles}\Microsoft Visual Studio\2022\Community\Common7\IDE\devenv.exe",
    "${env:ProgramFiles}\Microsoft Visual Studio\2022\Professional\Common7\IDE\devenv.exe",
    "${env:ProgramFiles}\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\devenv.exe"
)

$devenv = $null
foreach ($p in $devenvPaths) {
    if (Test-Path $p) { $devenv = $p; break }
}

if (-not $devenv) {
    Write-Fail "Visual Studio bulunamadi!"
    exit 1
}

Write-Host "   DevEnv: $devenv" -ForegroundColor Gray

# .slnx veya .sln bul
$slnFile = Get-ChildItem $SolutionRoot -Filter "*.slnx" -File | Select-Object -First 1
if (-not $slnFile) {
    $slnFile = Get-ChildItem $SolutionRoot -Filter "*.sln" -File | Select-Object -First 1
}
if (-not $slnFile) {
    Write-Fail "Solution dosyasi bulunamadi!"
    exit 1
}

Write-Host "   Solution: $($slnFile.Name)" -ForegroundColor Gray

# devenv /build Release + Publish
$buildLog = Join-Path $env:TEMP "GeminiBuild.log"
$buildProcess = Start-Process -FilePath $devenv -ArgumentList "`"$($slnFile.FullName)`" /build Release /out `"$buildLog`"" -Wait -PassThru -NoNewWindow

if ($buildProcess.ExitCode -ne 0) {
    Write-Fail "Build basarisiz!"
    if (Test-Path $buildLog) { Get-Content $buildLog | Write-Host }
    exit 1
}

Write-OK "Release build basarili"

# =====================================================
# ADIM 4: ZIP OLUSTURMA
# =====================================================

if (-not $SkipZip) {
    Write-Step "4/7" "ZIP arsivi olusturuluyor"

    # releases klasoru
    if (-not (Test-Path $ReleasesDir)) {
        New-Item -ItemType Directory -Path $ReleasesDir -Force | Out-Null
        Write-Host "   releases/ klasoru olusturuldu" -ForegroundColor Gray
    }

    $zipName = "GeminiOutlookTranslateAdd-in-v$version-setup.zip"
    $zipPath = Join-Path $ReleasesDir $zipName

    # Gecici klasor hazirla
    $tempDir = Join-Path $env:TEMP "GeminiReleaseBuild_$version"
    if (Test-Path $tempDir) { Remove-Item $tempDir -Recurse -Force }
    New-Item -ItemType Directory -Path $tempDir -Force | Out-Null

    # Publish ciktilari
    if (Test-Path $PublishDir) {
        Copy-Item "$PublishDir\*" $tempDir -Recurse -Force
        Write-Host "   Publish dosyalari eklendi" -ForegroundColor Gray
    }
    else {
        Write-Fail "Publish klasoru bulunamadi: $PublishDir"
        exit 1
    }

    # Deployment yardimci dosyalari
    $deployFiles = @(
        "Kur-Sertifika-ve-Addin.bat",
        "Install-Certificate.ps1",
        "KURULUM-REHBERİ.txt"
    )
    foreach ($f in $deployFiles) {
        $src = Join-Path $DeploymentDir $f
        if (Test-Path $src) {
            Copy-Item $src $tempDir -Force
            Write-Host "   + $f" -ForegroundColor DarkGray
        }
    }

    # ZIP olustur
    if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
    Compress-Archive -Path "$tempDir\*" -DestinationPath $zipPath -Force

    # Temizle
    Remove-Item $tempDir -Recurse -Force

    $zipSize = [math]::Round((Get-Item $zipPath).Length / 1MB, 1)
    Write-OK "$zipName ($zipSize MB) -> releases/"
}
else {
    Write-Host "   [ATLA] ZIP olusturma atlanacak (-SkipZip)" -ForegroundColor DarkGray
}

# =====================================================
# ADIM 5: GIT STAGED KONTROLU
# =====================================================

Write-Step "5/7" "Git degisiklik kontrolu"

Set-Location $SolutionRoot

$gitStatus = git status --short 2>&1
if ([string]::IsNullOrWhiteSpace($gitStatus)) {
    Write-Host "   Commit edilecek degisiklik yok" -ForegroundColor Gray
}
else {
    Write-Host "   Degisen dosyalar:" -ForegroundColor Gray
    $gitStatus | ForEach-Object { Write-Host "     $_" -ForegroundColor DarkGray }
    
    git add -A 2>&1 | Out-Null
    $commitMsg = "release: v$version"
    git commit -m $commitMsg 2>&1 | Out-Null
    Write-OK "Commit: $commitMsg"
}

# =====================================================
# ADIM 6: GIT TAG
# =====================================================

Write-Step "6/7" "Git tag"

$existingTag = git tag -l "v$version" 2>&1
if (-not [string]::IsNullOrWhiteSpace($existingTag)) {
    Write-Host "   Tag v$version zaten var, siliniyor..." -ForegroundColor Yellow
    git tag -d "v$version" 2>&1 | Out-Null
    if (-not $SkipPush) {
        git push origin ":refs/tags/v$version" 2>&1 | Out-Null
    }
}

git tag -a "v$version" -m "release: v$version" 2>&1 | Out-Null
Write-OK "Tag: v$version"

# =====================================================
# ADIM 7: GIT PUSH
# =====================================================

Write-Step "7/7" "Git push"

if ($SkipPush) {
    Write-Host "   [ATLA] Push atlanacak (-SkipPush)" -ForegroundColor Yellow
}
else {
    git push origin master 2>&1 | Out-Null
    git push origin "v$version" 2>&1 | Out-Null
    Write-OK "Push tamamlandi (master + v$version)"
}

# =====================================================
# OZET
# =====================================================

Write-Host ""
Write-Host "========================================================" -ForegroundColor Cyan
Write-Host " RELEASE TAMAMLANDI!" -ForegroundColor Green
Write-Host "========================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Versiyon:  v$version" -ForegroundColor White
Write-Host "  Tag:       v$version" -ForegroundColor White
if (-not $SkipZip) {
    Write-Host "  ZIP:       releases\$zipName" -ForegroundColor White
}
Write-Host "  Git:       $(if ($SkipPush) { 'PUSH YAPILMADI' } else { 'origin/master' })" -ForegroundColor White
Write-Host ""
Write-Host "  GitHub Releases'a ZIP yuklemek icin:" -ForegroundColor Gray
Write-Host "  https://github.com/hzkucuk/GeminiOutlookTranslateAdd-in/releases/new" -ForegroundColor DarkGray
Write-Host ""
