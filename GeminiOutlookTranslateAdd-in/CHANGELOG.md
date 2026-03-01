# Changelog

Tüm önemli deðiþiklikler bu dosyada belgelenmiþtir.

Format [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) standardýna uygundur.

## [1.1.0] - 2025-01-XX

### ?? Güvenlik (Security)
- **KRÝTÝK:** Hardcoded þifreleme password'ü (`"951753"`) kaldýrýldý
- **KRÝTÝK:** Machine Name + User Name + Entropy bazlý þifreleme eklendi
- **KRÝTÝK:** App.config'den API key kaldýrýldý (güvenlik açýðý kapatýldý)
- API key maskeleme eklendi (loglarda güvenli gösterim)
- Exception handling iyileþtirildi (veri sýzmasý önlendi)
- Registry encryption AES-256 ile güçlendirildi

### ? Performans (Performance)
- Ýmla düzeltme + çeviri tek API çaðrýsýnda birleþtirildi (2x daha hýzlý)
- Thread-safe CancellationToken implementasyonu
- Thread-safe Model Cache (`volatile` keyword)
- Word COM API dependency kaldýrýldý (imla kontrolü için Gemini kullanýlýyor)

### ? Yeni Özellikler (Added)
- Otomatik Türkçe imla düzeltme (Türkçe ? Ýngilizce çevirisinde)
- Otomatik noktalama düzeltme (virgül, nokta, soru iþareti ekleme)
- Mail otomatik save kaldýrýldý (kullanýcý kontrol)
- API key Registry'den yönetimi (Ribbon UI)

### ?? Deðiþiklikler (Changed)
- `SaveEncryptedApiKey()` ? Machine-specific key generation
- `LoadEncryptedApiKey()` ? Entropy-based decryption
- `SendTranslationRequestAsync()` ? App.config baðýmlýlýðý kaldýrýldý
- `DeriveKeyFromPassword()` ? `DeriveKeyFromMachineEntropy()` ile deðiþtirildi
- Hata mesajlarý güncellendi (App.config referansý kaldýrýldý)

### ?? Düzeltmeler (Fixed)
- Thread safety sorunlarý (CancellationToken race condition)
- Model cache thread safety
- Boþ catch bloklarý düzeltildi (exception loglama eklendi)
- API key loglama güvenliði (maskeleme)

### ??? Kaldýrýlanlar (Removed)
- `EncryptionPassword` constant (hardcoded "951753")
- `GeminiApiKeySettingKey` constant (App.config kullanýlmýyor)
- `RegistryIVName` constant (IV artýk ayrý key)
- `DeriveKeyFromPassword()` metodu
- Word COM API baðýmlýlýðý (imla kontrolü için)
- App.config'den API key okuma kodu

### ?? Kod Kalitesi (Code Quality)
- Magic number `50` ? `MinEmailThreadPosition` constant
- Exception handling iyileþtirildi (specific types)
- Debug loglarý iyileþtirildi (structured logging)
- Constants organizasyonu düzenlendi

### ?? Dokümantasyon (Documentation)
- README.md tamamen yeniden yazýldý
- Güvenlik bölümü eklendi (þifreleme detaylarý)
- Sorun giderme rehberi geniþletildi
- Teknik detaylar eklendi (þifreleme algoritmasý)
- API key migration rehberi eklendi

### ?? Breaking Changes
- **UYARI:** Eski API key'ler çalýþmayacak!
  - v1.0.0'da kaydedilmiþ API key'ler yeni þifreleme ile uyumsuz
  - Migration: Registry temizleyin ve yeniden kaydedin
  ```powershell
  Remove-Item -Path "HKCU:\Software\ZaferBilgisayar\GeminiTranslate" -Recurse -Force
  ```

### ?? Güvenlik Tavsiyeleri
1. **App.config'deki eski API key'i silin** (açýk metin)
2. **Google Gemini'de eski API key'i iptal edin** (revoke)
3. **Yeni API key oluþturun ve Ribbon'dan kaydedin**
4. **Git history'den eski key'i temizleyin** (BFG Repo-Cleaner)

---

## [1.0.0] - 2025-01-XX (Ýlk Sürüm)

### ? Yeni Özellikler
- Türkçe ? Ýngilizce çift yönlü çeviri
- HTML format koruma
- Segment bazlý HTML iþleme
- Gemini API entegrasyonu
- Ribbon UI (Ýngilizceye Çevir / Türkçeye Çevir butonlarý)
- Ýptal butonu (uzun çeviriler için)
- Orijinal metin koruma
- Subject çevirisi (RE: yoksa)
- Kýsaltma ve imza koruma (CEO, API, [ABC] vb.)
- Küfür filtresi
- 8000 karakter limiti (kullanýcý onayý)
- Mail thread temizleme (From:, Sent: vb.)
- AES-256 þifreleme (App.config'de hardcoded password ile)

### ?? Yapýlandýrma
- App.config ile API key yönetimi
- Model seçimi (Gemini modeli)
- Karakter limiti ayarý

### ?? Bilinen Sorunlar
- ?? App.config'de API key açýk metin (GÜVENLÝK AÇIÐI)
- ?? Hardcoded þifreleme password (GÜVENLÝK AÇIÐI)
- Thread safety sorunlarý (race condition)
- Word COM API baðýmlýlýðý (imla kontrolü için)

---

## Planlanan Özellikler (Roadmap)

### v1.2.0 (Gelecek)
- [ ] Çoklu dil desteði (Ýspanyolca, Fransýzca, Almanca)
- [ ] Kullanýcý sözlüðü (custom translations)
- [ ] Çeviri geçmiþi (history)
- [ ] Batch translation (toplu çeviri)
- [ ] Context menu entegrasyonu (sað týk)

### v2.0.0 (Uzun Vade)
- [ ] Offline mode (cached translations)
- [ ] AI-powered context detection (otomatik dil tespiti)
- [ ] Translation quality feedback
- [ ] Team shared glossary
- [ ] Azure Cloud entegrasyonu

---

## Upgrade Guide (Yükseltme Rehberi)

### v1.0.0 ? v1.1.0

#### 1. API Key Migration (Zorunlu)

**Adým 1: Eski key'i not edin**
- App.config dosyasýný açýn
- `GeminiApiKey` deðerini kopyalayýn

**Adým 2: Registry'yi temizleyin**
```powershell
Remove-Item -Path "HKCU:\Software\ZaferBilgisayar\GeminiTranslate" -Recurse -Force
```

**Adým 3: Google Gemini'de yeni key oluþturun**
1. https://aistudio.google.com/ ? API Keys
2. Eski key'i revoke edin (iptal)
3. Yeni key oluþturun

**Adým 4: Ribbon'dan kaydedin**
1. Outlook'u açýn
2. Mail sekmesi ? Gemini Çeviri grubu
3. API Key metin kutusuna yeni key'i yapýþtýrýn
4. Kaydet butonuna basýn

#### 2. App.config Temizleme (Önerilen)

**Eski:**
```xml
<add key="GeminiApiKey" value="AIzaSy..." />
<add key="GeminiModel" value="gemini-1.5-flash" />
```

**Yeni:**
```xml
<!-- API Key artýk App.config'de tutulmuyor -->
<add key="GeminiModel" value="gemini-2.0-flash-exp" />
```

#### 3. Git History Temizleme (Önemli)

Eðer API key'i Git'e commit ettiyseniz:

```bash
# BFG Repo-Cleaner ile temizleme
git clone --mirror https://github.com/[your-repo].git
bfg --replace-text passwords.txt [your-repo].git
cd [your-repo].git
git reflog expire --expire=now --all
git gc --prune=now --aggressive
git push
```

`passwords.txt`:
```
[REDACTED]==[REMOVED]
```

---

## Versiyon Numaralandýrma

Bu proje [Semantic Versioning](https://semver.org/) kullanýr:

```
MAJOR.MINOR.PATCH

MAJOR: Breaking changes (geriye uyumsuz)
MINOR: Yeni özellikler (geriye uyumlu)
PATCH: Bug fixes (geriye uyumlu)
```

Örnek:
- `1.0.0` ? Ýlk stabil sürüm
- `1.1.0` ? Yeni özellikler eklendi (uyumlu)
- `2.0.0` ? Breaking changes (uyumsuz, migration gerekli)

---

## Destek

- **Issues:** [GitHub Issues](https://github.com/[your-repo]/issues)
- **Discussions:** [GitHub Discussions](https://github.com/[your-repo]/discussions)
- **Email:** [destek@zaferbilgisayar.com]

---

**Son Güncelleme:** 2025-01-XX
