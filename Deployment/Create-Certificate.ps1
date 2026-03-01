# =====================================================
# Create Self-Signed Certificate for Code Signing
# =====================================================
# Bu script kod imzalama sertifikası oluşturur
# GELİŞTİRİCİ makinesinde YÖNETİCİ olarak çalıştırın!
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
    Write-Host "??  UYARI: Bu script YÖNETİCİ yetkisi gerektirir!" -ForegroundColor Red
    Write-Host "PowerShell'i 'Yönetici olarak çalıştır' ile açın." -ForegroundColor Yellow
    exit 1
}

Write-Host "?? Self-Signed Code Signing Certificate oluşturuluyor..." -ForegroundColor Yellow
Write-Host ""

try {
    # Sertifika oluştur
    Write-Host "[1/4] Sertifika oluşturuluyor..." -ForegroundColor Cyan
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
    
    # Output klasörü oluştur
    Write-Host "[2/4] Output klasörü hazırlanıyor..." -ForegroundColor Cyan
    if (-not (Test-Path $OutputPath)) {
        New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
    }
    Write-Host "   ? $OutputPath" -ForegroundColor Green
    Write-Host ""
    
    # PFX olarak dışa aktar
    Write-Host "[3/4] PFX dosyası oluşturuluyor..." -ForegroundColor Cyan
    $pfxPath = Join-Path $OutputPath "ZaferBilgisayar-CodeSigning.pfx"
    $pfxPassword = ConvertTo-SecureString -String $Password -Force -AsPlainText
    
    Export-PfxCertificate `
        -Cert $cert `
        -FilePath $pfxPath `
        -Password $pfxPassword | Out-Null
    
    Write-Host "   ? PFX: $pfxPath" -ForegroundColor Green
    Write-Host "   ?? Password: $Password" -ForegroundColor Yellow
    Write-Host ""
    
    # CER olarak da dışa aktar (public key)
    Write-Host "[4/4] CER dosyası oluşturuluyor (public key)..." -ForegroundColor Cyan
    $cerPath = Join-Path $OutputPath "ZaferBilgisayar-CodeSigning.cer"
    Export-Certificate -Cert $cert -FilePath $cerPath | Out-Null
    Write-Host "   ? CER: $cerPath" -ForegroundColor Green
    Write-Host ""
    
    # Özet bilgi
    Write-Host "========================================================" -ForegroundColor Cyan
    Write-Host "?? BAŞARILI! Sertifika oluşturuldu." -ForegroundColor Green
    Write-Host "========================================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "DOSYALAR:" -ForegroundColor Yellow
    Write-Host "  ?? PFX (Private): $pfxPath" -ForegroundColor Gray
    Write-Host "  ?? CER (Public):  $cerPath" -ForegroundColor Gray
    Write-Host ""
    Write-Host "SONRAKI ADIMLAR:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  [A] GELİŞTİRİCİ MAKİNESİNDE (Visual Studio):" -ForegroundColor Cyan
    Write-Host "      1. Solution Explorer ? Proje sağ tık ? Properties" -ForegroundColor Gray
    Write-Host "      2. Signing sekmesi" -ForegroundColor Gray
    Write-Host "      3. ? Sign the ClickOnce manifests" -ForegroundColor Gray
    Write-Host "      4. Select from File ? $pfxPath" -ForegroundColor Gray
    Write-Host "      5. Password: $Password" -ForegroundColor Gray
    Write-Host "      6. Publish!" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  [B] HEDEF MAKİNELERDE:" -ForegroundColor Cyan
    Write-Host "      Seçenek 1: PowerShell (Yönetici) ile:" -ForegroundColor Gray
    Write-Host "         Import-PfxCertificate -FilePath '$pfxPath' \" -ForegroundColor DarkGray
    Write-Host "           -CertStoreLocation Cert:\LocalMachine\Root \" -ForegroundColor DarkGray
    Write-Host "           -Password (ConvertTo-SecureString '$Password' -AsPlainText -Force)" -ForegroundColor DarkGray
    Write-Host ""
    Write-Host "      Seçenek 2: Manuel:" -ForegroundColor Gray
    Write-Host "         - ZaferBilgisayar-CodeSigning.cer'e çift tıkla" -ForegroundColor DarkGray
    Write-Host "         - 'Install Certificate...' ? Local Machine" -ForegroundColor DarkGray
    Write-Host "         - 'Place all certificates in: Trusted Root...' seç" -ForegroundColor DarkGray
    Write-Host ""
    Write-Host "========================================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "??  GÜVENLİK NOTU:" -ForegroundColor Red
    Write-Host "   - PFX dosyasını GÜVENLİ bir yerde saklayın!" -ForegroundColor Yellow
    Write-Host "   - Password'u paylaşmayın!" -ForegroundColor Yellow
    Write-Host "   - CER dosyasını hedef makinelere dağıtabilirsiniz" -ForegroundColor Green
    Write-Host ""
    
    # Sertifikayı Trusted Root + Trusted Publishers'a ekle (bu makine için)
    Write-Host "Bu makinede sertifikayı guvenilir listeye eklemek ister misiniz? (Y/N)" -ForegroundColor Cyan
    $addToRoot = Read-Host

    if ($addToRoot -eq "Y" -or $addToRoot -eq "y") {
        Write-Host "Trusted Root'a ekleniyor..." -ForegroundColor Yellow
        Import-PfxCertificate -FilePath $pfxPath `
            -CertStoreLocation Cert:\LocalMachine\Root `
            -Password $pfxPassword | Out-Null
        Write-Host "   Trusted Root'a eklendi!" -ForegroundColor Green

        Write-Host "Trusted Publishers'a ekleniyor..." -ForegroundColor Yellow
        Import-PfxCertificate -FilePath $pfxPath `
            -CertStoreLocation Cert:\LocalMachine\TrustedPublisher `
            -Password $pfxPassword | Out-Null
        Write-Host "   Trusted Publishers'a eklendi!" -ForegroundColor Green
        Write-Host ""
        Write-Host "ClickOnce imza uyarisi artik gorunmeyecek!" -ForegroundColor Green
    }
    
}
catch {
    Write-Host ""
    Write-Host "? HATA: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    exit 1
}

Write-Host ""
Read-Host "Çıkmak için Enter'a basın"
