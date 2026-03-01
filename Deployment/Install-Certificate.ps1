# =====================================================
# Install Certificate on Target Machine
# =====================================================
# Bu script sertifikayý hedef makineye yükler
# HEDEF MAKÝNEDE YÖNETÝCÝ olarak çalýþtýrýn!
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
    Write-Host "??  UYARI: Bu script YÖNETÝCÝ yetkisi gerektirir!" -ForegroundColor Red
    Write-Host "PowerShell'i 'Yönetici olarak çalýþtýr' ile açýn." -ForegroundColor Yellow
    exit 1
}

# Cert path kontrolü
if ([string]::IsNullOrWhiteSpace($CertPath)) {
    Write-Host "Sertifika dosyasýný seçin:" -ForegroundColor Yellow
    Write-Host "  1. ZaferBilgisayar-CodeSigning.pfx (Private - þifre gerekli)" -ForegroundColor Gray
    Write-Host "  2. ZaferBilgisayar-CodeSigning.cer (Public - þifre gerekmez)" -ForegroundColor Gray
    Write-Host ""
    
    Add-Type -AssemblyName System.Windows.Forms
    $openFileDialog = New-Object System.Windows.Forms.OpenFileDialog
    $openFileDialog.Filter = "Certificate Files (*.pfx;*.cer)|*.pfx;*.cer|All Files (*.*)|*.*"
    $openFileDialog.Title = "Sertifika Dosyasýný Seçin"
    
    if ($openFileDialog.ShowDialog() -eq [System.Windows.Forms.DialogResult]::OK) {
        $CertPath = $openFileDialog.FileName
    }
    else {
        Write-Host "? Sertifika dosyasý seçilmedi!" -ForegroundColor Red
        exit 1
    }
}

# Dosya varlýk kontrolü
if (-not (Test-Path $CertPath)) {
    Write-Host "? HATA: Sertifika dosyasý bulunamadý!" -ForegroundColor Red
    Write-Host "   Yol: $CertPath" -ForegroundColor Gray
    exit 1
}

Write-Host "?? Sertifika: $CertPath" -ForegroundColor Cyan
Write-Host ""

try {
    $fileExtension = [System.IO.Path]::GetExtension($CertPath).ToLower()
    
    if ($fileExtension -eq ".pfx") {
        # PFX (Private Key)
        Write-Host "?? PFX dosyasý tespit edildi (private key)" -ForegroundColor Yellow
        Write-Host ""
        
        # Þifre sor
        if ([string]::IsNullOrWhiteSpace($Password)) {
            $securePassword = Read-Host "PFX þifresini girin" -AsSecureString
        }
        else {
            $securePassword = ConvertTo-SecureString -String $Password -AsPlainText -Force
        }
        
        Write-Host "Sertifika Trusted Root Certificate Authorities'e yükleniyor..." -ForegroundColor Yellow
        
        Import-PfxCertificate `
            -FilePath $CertPath `
            -CertStoreLocation Cert:\LocalMachine\Root `
            -Password $securePassword `
            -Exportable | Out-Null
        
        Write-Host "? PFX sertifikasý baþarýyla yüklendi!" -ForegroundColor Green
    }
    elseif ($fileExtension -eq ".cer") {
        # CER (Public Key)
        Write-Host "?? CER dosyasý tespit edildi (public key)" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Sertifika Trusted Root Certificate Authorities'e yükleniyor..." -ForegroundColor Yellow
        
        Import-Certificate `
            -FilePath $CertPath `
            -CertStoreLocation Cert:\LocalMachine\Root | Out-Null
        
        Write-Host "? CER sertifikasý baþarýyla yüklendi!" -ForegroundColor Green
    }
    else {
        Write-Host "? HATA: Desteklenmeyen dosya formatý: $fileExtension" -ForegroundColor Red
        Write-Host "   Sadece .pfx veya .cer dosyalarý desteklenir." -ForegroundColor Yellow
        exit 1
    }
    
    Write-Host ""
    Write-Host "========================================================" -ForegroundColor Cyan
    Write-Host "?? BAÞARILI! Sertifika güvenilir listeye eklendi." -ForegroundColor Green
    Write-Host "========================================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "SONRAKI ADIM:" -ForegroundColor Yellow
    Write-Host "  1. setup.exe'yi çalýþtýrýn" -ForegroundColor Gray
    Write-Host "  2. Kurulum artýk hatasýz tamamlanacak!" -ForegroundColor Gray
    Write-Host ""
    Write-Host "NOT: Sertifika þu konumda:" -ForegroundColor Cyan
    Write-Host "  Cert:\LocalMachine\Root (Trusted Root Certificate Authorities)" -ForegroundColor Gray
    Write-Host ""
    
    # Sertifika detaylarýný göster
    Write-Host "Yüklenen sertifika detaylarý:" -ForegroundColor Cyan
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
    Write-Host "Olasý nedenler:" -ForegroundColor Yellow
    Write-Host "  - Yanlýþ PFX þifresi" -ForegroundColor Gray
    Write-Host "  - Bozuk sertifika dosyasý" -ForegroundColor Gray
    Write-Host "  - Yetersiz yetki" -ForegroundColor Gray
    Write-Host ""
    exit 1
}

Write-Host ""
Read-Host "Çýkmak için Enter'a basýn"
