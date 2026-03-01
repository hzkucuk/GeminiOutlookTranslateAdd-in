# =====================================================
# Install Certificate on Target Machine
# =====================================================
# Bu script sertifikayı hedef makineye yükler
# HEDEF MAKİNEDE YÖNETİCİ olarak çalıştırın!
# =====================================================

param(
    [string]$CertPath = "",
    [string]$Password = "GeminiTranslate2025!"
)

function Show-Banner {
    Write-Host ""
    Write-Host "========================================================" -ForegroundColor Cyan
    Write-Host " Certificate Installer" -ForegroundColor Yellow
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

# Cert path kontrolü
if ([string]::IsNullOrWhiteSpace($CertPath)) {
    Write-Host "Sertifika dosyasını seçin:" -ForegroundColor Yellow
    Write-Host "  1. ZaferBilgisayar-CodeSigning.pfx (Private - şifre gerekli)" -ForegroundColor Gray
    Write-Host "  2. ZaferBilgisayar-CodeSigning.cer (Public - şifre gerekmez)" -ForegroundColor Gray
    Write-Host ""
    
    Add-Type -AssemblyName System.Windows.Forms
    $openFileDialog = New-Object System.Windows.Forms.OpenFileDialog
    $openFileDialog.Filter = "Certificate Files (*.pfx;*.cer)|*.pfx;*.cer|All Files (*.*)|*.*"
    $openFileDialog.Title = "Sertifika Dosyasını Seçin"
    
    if ($openFileDialog.ShowDialog() -eq [System.Windows.Forms.DialogResult]::OK) {
        $CertPath = $openFileDialog.FileName
    }
    else {
        Write-Host "? Sertifika dosyası seçilmedi!" -ForegroundColor Red
        exit 1
    }
}

# Dosya varlık kontrolü
if (-not (Test-Path $CertPath)) {
    Write-Host "? HATA: Sertifika dosyası bulunamadı!" -ForegroundColor Red
    Write-Host "   Yol: $CertPath" -ForegroundColor Gray
    exit 1
}

Write-Host "?? Sertifika: $CertPath" -ForegroundColor Cyan
Write-Host ""

try {
    $fileExtension = [System.IO.Path]::GetExtension($CertPath).ToLower()
    
    if ($fileExtension -eq ".pfx") {
        # PFX (Private Key)
        Write-Host "?? PFX dosyası tespit edildi (private key)" -ForegroundColor Yellow
        Write-Host ""
        
        # Şifre sor
        if ([string]::IsNullOrWhiteSpace($Password)) {
            $securePassword = Read-Host "PFX şifresini girin" -AsSecureString
        }
        else {
            $securePassword = ConvertTo-SecureString -String $Password -AsPlainText -Force
        }
        
        # 1) Trusted Root Certificate Authorities
        Write-Host "[1/2] Sertifika Trusted Root Certificate Authorities'e yukleniyor..." -ForegroundColor Yellow

        Import-PfxCertificate `
            -FilePath $CertPath `
            -CertStoreLocation Cert:\LocalMachine\Root `
            -Password $securePassword `
            -Exportable | Out-Null

        Write-Host "   Trusted Root'a yuklendi!" -ForegroundColor Green

        # 2) Trusted Publishers (ClickOnce uyarisi icin kritik)
        Write-Host "[2/2] Sertifika Trusted Publishers'a yukleniyor..." -ForegroundColor Yellow

        Import-PfxCertificate `
            -FilePath $CertPath `
            -CertStoreLocation Cert:\LocalMachine\TrustedPublisher `
            -Password $securePassword `
            -Exportable | Out-Null

        Write-Host "   Trusted Publishers'a yuklendi!" -ForegroundColor Green
    }
    elseif ($fileExtension -eq ".cer") {
        # CER (Public Key)
        Write-Host "?? CER dosyası tespit edildi (public key)" -ForegroundColor Yellow
        Write-Host ""
        # 1) Trusted Root Certificate Authorities
        Write-Host "[1/2] Sertifika Trusted Root Certificate Authorities'e yukleniyor..." -ForegroundColor Yellow

        Import-Certificate `
            -FilePath $CertPath `
            -CertStoreLocation Cert:\LocalMachine\Root | Out-Null

        Write-Host "   Trusted Root'a yuklendi!" -ForegroundColor Green

        # 2) Trusted Publishers (ClickOnce uyarisi icin kritik)
        Write-Host "[2/2] Sertifika Trusted Publishers'a yukleniyor..." -ForegroundColor Yellow

        Import-Certificate `
            -FilePath $CertPath `
            -CertStoreLocation Cert:\LocalMachine\TrustedPublisher | Out-Null

        Write-Host "   Trusted Publishers'a yuklendi!" -ForegroundColor Green
    }
    else {
        Write-Host "? HATA: Desteklenmeyen dosya formatı: $fileExtension" -ForegroundColor Red
        Write-Host "   Sadece .pfx veya .cer dosyaları desteklenir." -ForegroundColor Yellow
        exit 1
    }
    
    Write-Host ""
    Write-Host "========================================================" -ForegroundColor Cyan
    Write-Host "?? BAŞARILI! Sertifika güvenilir listeye eklendi." -ForegroundColor Green
    Write-Host "========================================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "SONRAKI ADIM:" -ForegroundColor Yellow
    Write-Host "  1. setup.exe'yi çalıştırın" -ForegroundColor Gray
    Write-Host "  2. Kurulum artık hatasız tamamlanacak!" -ForegroundColor Gray
    Write-Host ""
    Write-Host "NOT: Sertifika su konumlarda:" -ForegroundColor Cyan
    Write-Host "  Cert:\LocalMachine\Root (Trusted Root Certificate Authorities)" -ForegroundColor Gray
    Write-Host "  Cert:\LocalMachine\TrustedPublisher (Trusted Publishers)" -ForegroundColor Gray
    Write-Host ""
    
    # Sertifika detaylarını göster
    Write-Host "Yüklenen sertifika detayları:" -ForegroundColor Cyan
    $loadedCert = Get-ChildItem Cert:\LocalMachine\Root | Where-Object { $_.Subject -like "*Zafer*" } | Select-Object -First 1
    if ($loadedCert) {
        Write-Host "  Subject: $($loadedCert.Subject)" -ForegroundColor Gray
        Write-Host "  Thumbprint: $($loadedCert.Thumbprint)" -ForegroundColor Gray
        Write-Host "  Valid Until: $($loadedCert.NotAfter.ToString('yyyy-MM-dd HH:mm'))" -ForegroundColor Gray
    }
    
}
catch {
    Write-Host ""
    Write-Host "? HATA: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Olası nedenler:" -ForegroundColor Yellow
    Write-Host "  - Yanlış PFX şifresi" -ForegroundColor Gray
    Write-Host "  - Bozuk sertifika dosyası" -ForegroundColor Gray
    Write-Host "  - Yetersiz yetki" -ForegroundColor Gray
    Write-Host ""
    exit 1
}

Write-Host ""
Read-Host "Çıkmak için Enter'a basın"
