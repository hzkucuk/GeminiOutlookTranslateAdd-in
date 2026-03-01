# =====================================================
# Create Self-Signed Certificate for Code Signing
# =====================================================
# Bu script kod imzalama sertifikasý oluþturur
# GELÝÞTÝRÝCÝ makinesinde YÖNETÝCÝ olarak çalýþtýrýn!
# =====================================================

param(
    [string]$OutputPath = "C:\Temp",
    [string]$Password = "GeminiTranslate2025!",
    [string]$CertName = "Zafer Bilgisayar Auto-System"
)

function Show-Banner {
    Write-Host ""
    Write-Host "========================================================" -ForegroundColor Cyan
    Write-Host " Code Signing Certificate Generator" -ForegroundColor Yellow
    Write-Host " Gemini Outlook Translate - Zafer Bilgisayar" -ForegroundColor Gray
    Write-Host "========================================================" -ForegroundColor Cyan
    Write-Host ""
}

function Test-IsAdmin {
    $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($currentUser)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

# =====================================================
# MAIN
# =====================================================

Show-Banner

# Admin kontrolü
if (-not (Test-IsAdmin)) {
    Write-Host "??  UYARI: Bu script YÖNETÝCÝ yetkisi gerektirir!" -ForegroundColor Red
    Write-Host "PowerShell'i 'Yönetici olarak çalýþtýr' ile açýn." -ForegroundColor Yellow
    exit 1
}

Write-Host "?? Self-Signed Code Signing Certificate oluþturuluyor..." -ForegroundColor Yellow
Write-Host ""

try {
    # Sertifika oluþtur
    Write-Host "[1/4] Sertifika oluþturuluyor..." -ForegroundColor Cyan
    $cert = New-SelfSignedCertificate `
        -Type CodeSigningCert `
        -Subject "CN=$CertName" `
        -KeyUsage DigitalSignature `
        -FriendlyName "$CertName - Code Signing" `
        -CertStoreLocation "Cert:\CurrentUser\My" `
        -NotAfter (Get-Date).AddYears(5) `
        -KeyExportPolicy Exportable
    
    Write-Host "   ? Thumbprint: $($cert.Thumbprint)" -ForegroundColor Green
    Write-Host "   ? Expires: $($cert.NotAfter.ToString('yyyy-MM-dd'))" -ForegroundColor Green
    Write-Host ""
    
    # Output klasörü oluþtur
    Write-Host "[2/4] Output klasörü hazýrlanýyor..." -ForegroundColor Cyan
    if (-not (Test-Path $OutputPath)) {
        New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
    }
    Write-Host "   ? $OutputPath" -ForegroundColor Green
    Write-Host ""
    
    # PFX olarak dýþa aktar
    Write-Host "[3/4] PFX dosyasý oluþturuluyor..." -ForegroundColor Cyan
    $pfxPath = Join-Path $OutputPath "ZaferBilgisayar-CodeSigning.pfx"
    $pfxPassword = ConvertTo-SecureString -String $Password -Force -AsPlainText
    
    Export-PfxCertificate `
        -Cert $cert `
        -FilePath $pfxPath `
        -Password $pfxPassword | Out-Null
    
    Write-Host "   ? PFX: $pfxPath" -ForegroundColor Green
    Write-Host "   ?? Password: $Password" -ForegroundColor Yellow
    Write-Host ""
    
    # CER olarak da dýþa aktar (public key)
    Write-Host "[4/4] CER dosyasý oluþturuluyor (public key)..." -ForegroundColor Cyan
    $cerPath = Join-Path $OutputPath "ZaferBilgisayar-CodeSigning.cer"
    Export-Certificate -Cert $cert -FilePath $cerPath | Out-Null
    Write-Host "   ? CER: $cerPath" -ForegroundColor Green
    Write-Host ""
    
    # Özet bilgi
    Write-Host "========================================================" -ForegroundColor Cyan
    Write-Host "?? BAÞARILI! Sertifika oluþturuldu." -ForegroundColor Green
    Write-Host "========================================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "DOSYALAR:" -ForegroundColor Yellow
    Write-Host "  ?? PFX (Private): $pfxPath" -ForegroundColor Gray
    Write-Host "  ?? CER (Public):  $cerPath" -ForegroundColor Gray
    Write-Host ""
    Write-Host "SONRAKI ADIMLAR:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  [A] GELÝÞTÝRÝCÝ MAKÝNESÝNDE (Visual Studio):" -ForegroundColor Cyan
    Write-Host "      1. Solution Explorer ? Proje sað týk ? Properties" -ForegroundColor Gray
    Write-Host "      2. Signing sekmesi" -ForegroundColor Gray
    Write-Host "      3. ? Sign the ClickOnce manifests" -ForegroundColor Gray
    Write-Host "      4. Select from File ? $pfxPath" -ForegroundColor Gray
    Write-Host "      5. Password: $Password" -ForegroundColor Gray
    Write-Host "      6. Publish!" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  [B] HEDEF MAKÝNELERDE:" -ForegroundColor Cyan
    Write-Host "      Seçenek 1: PowerShell (Yönetici) ile:" -ForegroundColor Gray
    Write-Host "         Import-PfxCertificate -FilePath '$pfxPath' \" -ForegroundColor DarkGray
    Write-Host "           -CertStoreLocation Cert:\LocalMachine\Root \" -ForegroundColor DarkGray
    Write-Host "           -Password (ConvertTo-SecureString '$Password' -AsPlainText -Force)" -ForegroundColor DarkGray
    Write-Host ""
    Write-Host "      Seçenek 2: Manuel:" -ForegroundColor Gray
    Write-Host "         - ZaferBilgisayar-CodeSigning.cer'e çift týkla" -ForegroundColor DarkGray
    Write-Host "         - 'Install Certificate...' ? Local Machine" -ForegroundColor DarkGray
    Write-Host "         - 'Place all certificates in: Trusted Root...' seç" -ForegroundColor DarkGray
    Write-Host ""
    Write-Host "========================================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "??  GÜVENLÝK NOTU:" -ForegroundColor Red
    Write-Host "   - PFX dosyasýný GÜVENLÝ bir yerde saklayýn!" -ForegroundColor Yellow
    Write-Host "   - Password'u paylaþmayýn!" -ForegroundColor Yellow
    Write-Host "   - CER dosyasýný hedef makinelere daðýtabilirsiniz" -ForegroundColor Green
    Write-Host ""
    
    # Sertifikayý Trusted Root'a da ekle (bu makine için)
    Write-Host "Bu makinede sertifikayý Trusted Root'a eklemek ister misiniz? (Y/N)" -ForegroundColor Cyan
    $addToRoot = Read-Host
    
    if ($addToRoot -eq "Y" -or $addToRoot -eq "y") {
        Write-Host "Trusted Root'a ekleniyor..." -ForegroundColor Yellow
        Import-PfxCertificate -FilePath $pfxPath `
            -CertStoreLocation Cert:\LocalMachine\Root `
            -Password $pfxPassword | Out-Null
        Write-Host "? Bu makinede güvenilir sertifika listesine eklendi!" -ForegroundColor Green
    }
    
}
catch {
    Write-Host ""
    Write-Host "? HATA: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    exit 1
}

Write-Host ""
Read-Host "Çýkmak için Enter'a basýn"
