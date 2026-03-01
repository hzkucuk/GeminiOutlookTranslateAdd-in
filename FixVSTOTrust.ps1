# Bu betik, VSTO eklentisinin yüklenmesine izin vermek için ClickOnce güven ayarlarýný düzenler.
# Sorun yaþanan bilgisayarda "Yönetici Olarak Çalýþtýr" seçeneði ile çalýþtýrýlmalýdýr.

$paths = @(
    "HKLM:\SOFTWARE\MICROSOFT\.NETFramework\Security\TrustManager\PromptingLevel",
    "HKLM:\SOFTWARE\WOW6432Node\MICROSOFT\.NETFramework\Security\TrustManager\PromptingLevel"
)

foreach ($path in $paths) {
    if (!(Test-Path $path)) {
        New-Item -Path $path -Force | Out-Null
    }

    $settings = @("MyComputer", "LocalIntranet", "Internet", "TrustedSites", "UntrustedSites")

    foreach ($setting in $settings) {
        Set-ItemProperty -Path $path -Name $setting -Value "Enabled"
    }
}

Write-Host "--------------------------------------------------------" -ForegroundColor Cyan
Write-Host "ClickOnce Gven Ayarlar Gncellendi!" -ForegroundColor Green
Write-Host "Artk .vsto dosyasn tekrar altrabilirsiniz." -ForegroundColor White
Write-Host "Ykleme sýrasýnda 'Yayýncý Doðrulanamadý' uyarýsý gelirse 'Yükle' (Install) diyebilirsiniz." -ForegroundColor Yellow
Write-Host "--------------------------------------------------------" -ForegroundColor Cyan
pause
