# =====================================================
# Gemini Outlook Translate - Trust Error Fix Script
# =====================================================
# Bu script ClickOnce trust hatasını düzeltir
# Hedef makinede YÖNETİCİ olarak çalıştırın!
# =====================================================

param(
    [switch]$TrustedLocation,
    [switch]$BypassSecurity,
    [switch]$Both
)

function Show-Banner {
    Write-Host ""
    Write-Host "=================================================" -ForegroundColor Cyan
    Write-Host " Gemini Translate - Trust Error Fix" -ForegroundColor Yellow
    Write-Host " Zafer Bilgisayar by Auto-System" -ForegroundColor Gray
    Write-Host "=================================================" -ForegroundColor Cyan
    Write-Host ""
}

function Add-TrustedLocation {
    Write-Host "[1/2] Outlook Trusted Location ekleniyor..." -ForegroundColor Yellow
    
    try {
        $trustPath = "HKCU:\Software\Microsoft\Office\16.0\Outlook\Security\Trusted Locations\Location30"
        
        # Klasör oluştur
        New-Item -Path $trustPath -Force | Out-Null
        
        # Downloads klasörünü güvenilir yap
        $downloadsPath = "$env:USERPROFILE\Downloads\GeminiOutlookTranslateAdd-in\"
        Set-ItemProperty -Path $trustPath -Name "Path" -Value $downloadsPath
        Set-ItemProperty -Path $trustPath -Name "AllowSubfolders" -Value 1 -Type DWord
        Set-ItemProperty -Path $trustPath -Name "Description" -Value "Gemini Translate Add-in"
        
        Write-Host "✅ Trusted Location eklendi: $downloadsPath" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "❌ HATA: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Set-ClickOnceTrust {
    Write-Host "[2/2] ClickOnce güvenlik ayarları yapılandırılıyor..." -ForegroundColor Yellow
    
    try {
        $regPath = "HKCU:\Software\Microsoft\.NETFramework\Security\TrustManager\PromptingLevel"
        
        # Registry path oluştur
        New-Item -Path "HKCU:\Software\Microsoft\.NETFramework\Security\TrustManager" -Force -ErrorAction SilentlyContinue | Out-Null
        New-Item -Path $regPath -Force | Out-Null
        
        # Trust seviyelerini ayarla
        Set-ItemProperty -Path $regPath -Name "MyComputer" -Value "Enabled" -Type String
        Set-ItemProperty -Path $regPath -Name "LocalIntranet" -Value "Enabled" -Type String
        Set-ItemProperty -Path $regPath -Name "Internet" -Value "AuthenticodeRequired" -Type String
        Set-ItemProperty -Path $regPath -Name "UntrustedSites" -Value "Disabled" -Type String
        Set-ItemProperty -Path $regPath -Name "TrustedSites" -Value "Enabled" -Type String
        
        Write-Host "✅ ClickOnce trust ayarlandı!" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "❌ HATA: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
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
    Write-Host "⚠️  UYARI: Bu script YÖNETİCİ yetkisi gerektirir!" -ForegroundColor Red
    Write-Host "PowerShell'i 'Yönetici olarak çalıştır' ile açın." -ForegroundColor Yellow
    Write-Host ""
    Read-Host "Devam etmek için Enter'a basın"
    exit 1
}

# Parametre kontrolü
if (-not $TrustedLocation -and -not $BypassSecurity -and -not $Both) {
    Write-Host "KULLANIM:" -ForegroundColor Cyan
    Write-Host "  .\Fix-TrustError.ps1 -TrustedLocation    # Sadece Trusted Location ekle" -ForegroundColor Gray
    Write-Host "  .\Fix-TrustError.ps1 -BypassSecurity     # Sadece ClickOnce trust ayarla" -ForegroundColor Gray
    Write-Host "  .\Fix-TrustError.ps1 -Both               # Her ikisini de yap (ÖNERİLEN)" -ForegroundColor Green
    Write-Host ""
    
    $choice = Read-Host "Hangi seçeneği kullanmak istersiniz? (1=TrustedLocation, 2=BypassSecurity, 3=Both)"
    
    switch ($choice) {
        "1" { $TrustedLocation = $true }
        "2" { $BypassSecurity = $true }
        "3" { $Both = $true }
        default { 
            Write-Host "❌ Geçersiz seçenek!" -ForegroundColor Red
            exit 1
        }
    }
}

# İşlemleri yap
$success = $true

if ($Both) {
    $success = (Add-TrustedLocation) -and (Set-ClickOnceTrust)
}
elseif ($TrustedLocation) {
    $success = Add-TrustedLocation
}
elseif ($BypassSecurity) {
    $success = Set-ClickOnceTrust
}

# Sonuç
Write-Host ""
Write-Host "=================================================" -ForegroundColor Cyan

if ($success) {
    Write-Host "🎉 BAŞARILI! Artık kurulum yapabilirsiniz." -ForegroundColor Green
    Write-Host ""
    Write-Host "SONRAKI ADIM:" -ForegroundColor Yellow
    Write-Host "  1. setup.exe'yi tekrar çalıştırın" -ForegroundColor Gray
    Write-Host "  2. Kurulum tamamlanacak!" -ForegroundColor Gray
}
else {
    Write-Host "❌ Bazı işlemler başarısız oldu!" -ForegroundColor Red
    Write-Host "Lütfen hata mesajlarını kontrol edin." -ForegroundColor Yellow
}

Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""
Read-Host "Çıkmak için Enter'a basın"
