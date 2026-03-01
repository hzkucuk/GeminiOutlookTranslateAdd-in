# Gemini Outlook Translate Add-in

Bu eklenti, Outlook içindeki e-postalarý Gemini API ile Türkçe ? Ýngilizce çevirir ve çeviriyi orijinal metnin üstüne ekler. Konu alaný da ("RE:" yoksa) ayný yönde çevrilir.

## ?? Özellikler

### Çeviri Özellikleri
- ? **Türkçe ? Ýngilizce çift yönlü çeviri**
- ? **Otomatik imla ve noktalama düzeltme** (Türkçe ? Ýngilizce için)
- ? **HTML gövdelerde format koruma** (segment bazlý iþleme)
- ? **Orijinal içerik korunur**, çeviri üstte görünür
- ? **Yeniden çeviride önceki çeviri otomatik kaldýrýlýr**
- ? **Kýsaltma ve imza koruma** (CEO, API, HZK, [ABC] vb.)
- ? **Küfür filtresi** (uygunsuz içerik engellenir)
- ? **Ýptal butonu** (uzun çevirileri durdurabilirsiniz)
- ? **8000 karakter limiti** (kullanýcý onayý ile devam)

### Güvenlik Özellikleri
- ?? **Machine + User bazlý þifreleme** (her bilgisayarda farklý)
- ?? **Hardcoded password yok** (entropy kullanýlýr)
- ?? **Registry ile güvenli saklama** (HKEY_CURRENT_USER)
- ?? **API key maskeleme** (loglarda güvenli)
- ?? **App.config'de açýk metin yok**

### Performans
- ? **Ýmla + çeviri tek API çaðrýsýnda** (2x daha hýzlý)
- ? **Model cache** (ikinci çaðrýdan itibaren hýzlý)
- ? **Thread-safe** (eþzamanlý iþlem desteði)
- ? **Async/await** (Outlook donmaz)

## ?? Gereksinimler
- .NET Framework 4.8
- Microsoft Outlook (VSTO desteði olan sürüm)
- Google Gemini API anahtarý ([Ücretsiz alýn](https://aistudio.google.com/))

## ?? Kurulum

### 1. API Anahtarý Alma
1. https://aistudio.google.com/ adresine gidin
2. Google hesabýnýzla giriþ yapýn
3. **"Get API Key"** butonuna týklayýn
4. API anahtarýnýzý kopyalayýn

### 2. API Anahtarýný Kaydetme

?? **ÖNEMLÝ:** API anahtarý artýk `App.config` dosyasýnda tutulmuyor!

**Outlook Ribbon üzerinden kaydedin:**

1. **Outlook'u açýn**
2. Herhangi bir mail açýn (yeni veya mevcut)
3. **Mail** sekmesinde **"Gemini Çeviri"** grubunu bulun
4. **API Key Status** altýndaki metin kutusuna API anahtarýnýzý yapýþtýrýn
5. **Kaydet** butonuna týklayýn
6. ? **"API Key baþarýyla kaydedildi ve þifrelendi"** mesajýný görmelisiniz

**API Anahtarýnýz:**
- Windows Registry'de saklanýr (`HKEY_CURRENT_USER\Software\ZaferBilgisayar\GeminiTranslate`)
- AES-256 ile þifrelenir
- Machine Name + User Name + Static Entropy ile korunur
- Baþka kullanýcýlar okuyamaz

### 3. Model Yapýlandýrmasý (Opsiyonel)

`App.config` dosyasýnda varsayýlan model:

```xml
<add key="GeminiModel" value="gemini-2.0-flash-exp" />
```

**Diðer model seçenekleri:**
- `gemini-2.0-flash-exp` (varsayýlan - en hýzlý, deneysel)
- `gemini-1.5-flash` (kararlý, hýzlý)
- `gemini-1.5-pro` (geliþmiþ, yavaþ ama daha kaliteli)

## ?? Kullaným

### Temel Çeviri Adýmlarý

1. **Outlook'ta mail açýn** (yeni mail, yanýt veya iletme)
2. **Mail içeriðini yazýn**
3. **Ribbon'da çeviri butonuna basýn:**
   - **Ýngilizceye Çevir** ? Türkçe mail Ýngilizce'ye çevrilir
   - **Türkçeye Çevir** ? Ýngilizce mail Türkçe'ye çevrilir
4. **Çeviri tamamlanýr** (3-5 saniye)
5. **Manuel kaydedin** (Ctrl+S veya Kaydet butonu)

### Türkçe ? Ýngilizce Çeviri (Otomatik Ýmla Düzeltme)

Türkçe mail yazarken imla veya noktalama hatasý yaptýysanýz:

```
Örnek mail içeriði:
"merhba nasýlsnýz bu projeyi begendiniz mi"
                     ?
Ýmla düzeltme + Çeviri:
"Hello, how are you? Did you like this project?"
```

**Düzeltilen hatalar:**
- ? Yazým hatalarý (merhba ? merhaba)
- ? Noktalama eksiklikleri (virgül, nokta, soru iþareti)
- ? Büyük/küçük harf düzeltmeleri

### Ýptal Etme

Çeviri sýrasýnda **Ýptal** butonuna basabilirsiniz:
- Çeviri durdurulur
- Konu alanýndaki "(Çevriliyor...)" kaldýrýlýr
- Mail eski haline döner

### Yeniden Çeviri

Daha önce çevrilmiþ bir mail'i tekrar çevirirseniz:
- ? Önceki çeviri otomatik kaldýrýlýr
- ? Sadece orijinal metin kullanýlýr
- ? Yeni çeviri eklenir

## ?? Güvenlik ve Gizlilik

### API Anahtarý Güvenliði

**Þifreleme Yöntemi:**
```
1. Machine Name (bilgisayar adý)
2. User Name (kullanýcý adý)
3. Static Entropy (16 byte)
   ? SHA-256
4. 32-byte AES Key
   ? AES-256 Encryption
5. Registry'de Base64 saklanýr
```

**Güvenlik Özellikleri:**
- ? Her bilgisayarda farklý þifreleme anahtarý
- ? Ayný bilgisayarda farklý kullanýcýlar okuyamaz
- ? Kaynak kodda hardcoded password yok
- ? Reverse engineering zorlaþtýrýldý

### Veri Gönderimi

?? **ÖNEMLÝ:** Mail içerikleri Google Gemini API'ye HTTPS ile gönderilir.

**Ne gönderilir:**
- Mail body (HTML veya Text)
- Mail subject (RE: yoksa)

**Ne gönderilmez:**
- Alýcý bilgileri
- Gönderen bilgileri
- Ekler (attachments)
- Mail thread geçmiþi (otomatik kesilir)

**Dikkat:**
- ?? Hassas/gizli bilgiler içeren mail'leri çevirmeden önce düþünün
- ?? Þirket politikalarýna uygun kullanýn
- ?? API key'inizi kimseyle paylaþmayýn

### Küfür ve Uygunsuz Ýçerik Filtresi

Gemini API'den gelen çevirilerde:
- ? Küfür tespit edilirse `[FILTERED]` ile deðiþtirilir
- ? 8 yaygýn Ýngilizce küfür kelimesi filtrelenir
- ? Prompt injection korumasý var

## ?? Yapýlandýrma

### App.config Dosyasý

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <appSettings>
    <!-- API Key artýk App.config'de tutulmuyor. Ribbon üzerinden kaydedin. -->
    <add key="GeminiModel" value="gemini-2.0-flash-exp" />
  </appSettings>
</configuration>
```

**Ayarlar:**
- `GeminiModel`: Kullanýlacak Gemini model adý (opsiyonel, API'den otomatik algýlanýr)

## ?? Sýnýrlamalar

### Karakter Limiti
- **8000 karakter** (API quota için)
- Limit aþýlýrsa kullanýcýya sorulur:
  - "Ýlk 8000 karakteri çevirmek ister misiniz?"
  - Hayýr ? Ýptal
  - Evet ? Ýlk 8000 karakter çevrilir

### Subject Çevirisi
- **"RE:" ile baþlayan konular çevrilmez** (yanýt mail'leri)
- Yeni mail veya iletmelerde subject çevrilir

### HTML Desteði
- ? Formatlar korunur (bold, italic, renkler vb.)
- ? Linkler bozulmaz
- ? Tablolar korunur
- ? Imzalar korunur
- ?? Çok kompleks HTML'lerde format kaybý olabilir

## ?? Sorun Giderme

### "API Key kayýtlý deðil" Hatasý

**Çözüm:**
1. Ribbon üzerinden API anahtarýný kaydedin
2. Doðru anahtarý kopyaladýðýnýzdan emin olun
3. Internet baðlantýnýzý kontrol edin

### "API model listesi alýnamadý" Hatasý

**Olasý nedenler:**
- ? API anahtarý geçersiz
- ? Internet baðlantýsý yok
- ? Firewall/Proxy Gemini API'yi engelliyor
- ? API quota dolmuþ

**Çözüm:**
1. https://aistudio.google.com/ ? API anahtarýnýzý kontrol edin
2. Yeni API anahtarý oluþturun
3. Ribbon'dan kaydedin

### "Çeviri iptal edildi" Hatasý

**Neden:**
- Mail çok uzun (8000+ karakter)
- Kullanýcý "Hayýr" seçeneðini seçti

**Çözüm:**
- Mail'i kýsaltýn veya bölün
- "Evet" seçeneðine basýn (ilk 8000 karakter çevrilir)

### Eski API Key Temizleme

Eski þifreleme yöntemiyle kaydedilmiþ API key varsa (v1.0.0):

**PowerShell (Yönetici olarak):**
```powershell
Remove-Item -Path "HKCU:\Software\ZaferBilgisayar\GeminiTranslate" -Recurse -Force
```

Sonra Ribbon'dan yeniden kaydedin.

### Çeviri Çalýþmýyor

**Kontrol listesi:**
1. ? API key kaydedilmiþ mi? (Ribbon'da "? API Key kayýtlý" yazmalý)
2. ? Internet baðlantýsý var mý?
3. ? Mail açýk mý? (Inspector penceresi)
4. ? Mail içeriði boþ deðil mi?

**Debug loglarý:**
- Visual Studio Output penceresinde debug loglarýný kontrol edin
- `[TranslateCurrentMailAsync]`, `[SendTranslationRequestAsync]` etiketlerine bakýn

## ?? Teknik Detaylar

### Þifreleme Algoritmasý

**AES-256 Encryption:**
```
Input: API Key (plain text)
   ?
Machine Name + User Name + Static Entropy
   ? SHA-256
32-byte Unique Key (her bilgisayarda farklý)
   ? AES-256 + Random IV
Encrypted Data (Base64)
   ?
Registry: HKEY_CURRENT_USER\Software\ZaferBilgisayar\GeminiTranslate
```

**Avantajlarý:**
- Hardcoded password yok
- Her makine + kullanýcý için farklý key
- IV her kayýtta deðiþir (replay attack korumasý)
- Reverse engineering zorlaþtýrýldý

### Çeviri Akýþý

**Türkçe ? Ýngilizce:**
```
1. Mail okunur (HTML veya Text)
2. Önceki çeviriler temizlenir
3. Mail thread'leri kesilir (From:, Sent: vb.)
4. Ýmla + noktalama + çeviri ? Gemini API (TEK ÇAÐRI)
5. Subject imla + çeviri ? Gemini API (TEK ÇAÐRI)
6. HTML formatý korunur
7. Çeviri + Separator + Orijinal birleþtirilir
8. Mail güncellenir (manuel kayýt gerekli)
```

**Ýngilizce ? Türkçe:**
```
1-3. Ayný
4. Direkt çeviri ? Gemini API (imla düzeltme yok)
5. Subject çeviri ? Gemini API
6-8. Ayný
```

### Performans

**Ortalama süre:**
- **3-5 saniye** (normal mail, 500-2000 karakter)
- **5-10 saniye** (uzun mail, 5000-8000 karakter)

**API Çaðrýsý sayýsý:**
- **Türkçe ? Ýngilizce:** 2 çaðrý (body + subject)
- **Ýngilizce ? Türkçe:** 2 çaðrý (body + subject)

**Model Cache:**
- Ýlk çaðrý: +1 saniye (model listesi alýnýr)
- Sonraki çaðrýlar: Cache kullanýlýr (ekstra süre yok)

### HTML Ýþleme

**Segment Bazlý Çeviri:**
```html
<p>Merhaba</p>          [[SEG0]]Merhaba[[/SEG0]]
<b>Nasýlsýnýz?</b>  ?   [[SEG1]]Nasýlsýnýz?[[/SEG1]]
<i>Ýyi misiniz?</i>     [[SEG2]]Ýyi misiniz?[[/SEG2]]
        ? Gemini API
[[SEG0]]Hello[[/SEG0]]
[[SEG1]]How are you?[[/SEG1]]
[[SEG2]]Are you well?[[/SEG2]]
        ?
<p>Hello</p>
<b>How are you?</b>
<i>Are you well?</i>
```

**Korunan elementler:**
- `<script>`, `<style>`, `<head>` ? Çevrilmez
- HTML attribute'larý ? Korunur
- Linkler ? Bozulmaz

## ?? Sýnýrlamalar

### Karakter Limiti
- **8000 karakter** (Gemini API quota için makul limit)
- Limit aþýlýrsa:
  - Kullanýcýya uyarý gösterilir
  - Ýlk 8000 karakter çevrilir (onay ile)
  - Veya iptal edilir

### Subject Kýsýtlamalarý
- **"RE:" ile baþlayan konular çevrilmez** (yanýt chain'i korumak için)
- **"FW:" veya "FWD:"** ile baþlayanlar çevrilir

### HTML Kýsýtlamalarý
- Çok kompleks HTML yapýlarýnda format kaybý olabilir
- Nested table'larda dikkatli olun
- CSS stilleri korunur ama garantisi yok

### API Kýsýtlamalarý
- **Gemini API quota** (günlük/dakikalýk limit)
- **Rate limiting** (çok hýzlý ardýþýk istek yapýlýrsa 429 hatasý)
- **Network baðýmlýlýðý** (offline çalýþmaz)

## ??? Geliþtirici Notlarý

### Debug Loglarý

Visual Studio **Output** penceresinde loglarý görebilirsiniz:

```
[TranslateCurrentMailAsync] Baþladý
[TranslateCurrentMailAsync] isHtmlBody: True
[TranslateHtmlBodyAsync] Text node sayýsý: 12
[SendTranslationRequestAsync] API Key yüklendi: AIza...tcmw
[SendTranslationRequestAsync] Model: models/gemini-2.0-flash-exp
[TranslateHtmlBodyAsync] Tamamlandý, sonuç uzunluðu: 456
```

### Kod Yapýsý

```
RibbonCeviri.cs
??? API Key Yönetimi
?   ??? SaveEncryptedApiKey() - AES þifreleme
?   ??? LoadEncryptedApiKey() - AES decrypt
?   ??? DeriveKeyFromMachineEntropy() - Key generation
?   ??? MaskApiKey() - Güvenli loglama
??? Çeviri Ýþlemleri
?   ??? TranslateCurrentMailAsync() - Ana orchestrator
?   ??? TranslateHtmlBodyAsync() - Segment bazlý HTML
?   ??? TranslateTextAsync() - Plain text çeviri
?   ??? SendTranslationRequestAsync() - Gemini API
??? Yardýmcý Metodlar
?   ??? ExtractOriginalHtml/Text() - Orijinal ayýklama
?   ??? BuildTranslatedHtmlBody() - HTML birleþtirme
?   ??? CleanTranslatedText() - Post-processing
?   ??? MaskSignaturesAndAbbreviations() - Koruma
?   ??? FilterProfanity() - Küfür filtresi
??? UI Event Handlers
    ??? btnTranslate_Click() - TR ? EN
    ??? btnTranslateToTurkish_Click() - EN ? TR
    ??? btnCancelTranslation_Click() - Ýptal
```

### Thread Safety

**CancellationToken:**
```csharp
private static readonly object CancellationLock = new object();

lock (CancellationLock)
{
    GlobalCancellationSource = localCancellationSource;
}
```

**Model Cache:**
```csharp
private static volatile string CachedModelName; // Thread-safe
```

## ?? Changelog

### v1.1.0 (2025-01-XX) - Güvenlik ve Performans Güncellemesi

#### Güvenlik Ýyileþtirmeleri
- ? **KRÝTÝK:** Hardcoded þifreleme password'ü kaldýrýldý
- ? **KRÝTÝK:** Machine + User bazlý entropy ile þifreleme
- ? **KRÝTÝK:** App.config'den API key kaldýrýldý
- ? API key maskeleme (loglar güvenli)
- ? Exception handling iyileþtirildi (veri sýzmasý önlendi)

#### Performans Ýyileþtirmeleri
- ? Ýmla düzeltme + çeviri tek API çaðrýsýnda (2x hýzlý)
- ? Thread-safe CancellationToken
- ? Thread-safe Model Cache (volatile)
- ? Word COM API kaldýrýldý (imla için Gemini kullanýlýyor)

#### Yeni Özellikler
- ? Otomatik imla ve noktalama düzeltme (Türkçe ? Ýngilizce)
- ? Otomatik save kaldýrýldý (kullanýcý kontrol)

#### Kod Kalitesi
- ? Magic numbers ? constants (MinEmailThreadPosition)
- ? Boþ catch bloklarý düzeltildi
- ? Specific exception types kullanýlýyor
- ? App.config baðýmlýlýðý kaldýrýldý

### v1.0.0 (Ýlk Sürüm)
- Temel Türkçe ? Ýngilizce çeviri
- HTML format koruma
- Segment bazlý iþleme
- Gemini API entegrasyonu
- Ribbon UI

## ?? Bilinen Sorunlar

1. **Çok uzun HTML mail'ler** (8000+ karakter)
   - Kýsmi çeviri yapýlýr
   - Alternatif: Mail'i böl veya kýsalt

2. **Kompleks HTML yapýlarý**
   - Bazý özel CSS'ler kaybolabilir
   - Nested table'lar bozulabilir

3. **API Quota**
   - Gemini free tier: 60 request/minute
   - Aþýlýrsa 429 hatasý (1 dakika bekleyin)

## ?? Katkýda Bulunma

Öneriler ve hata bildirimleri için issue açabilirsiniz.

## ?? Lisans

Copyright © 2026 Zafer Bilgisayar by Auto-System

---

## ?? Güvenlik Uyarýlarý

### ?? API Key Güvenliði
1. **API key'inizi kimseyle paylaþmayýn**
2. **GitHub'a commit etmeyin** (`.gitignore` kullanýn)
3. **Eski key'i deðiþtirin** (önceki sürümlerde App.config'deydi)
4. **Düzenli olarak rotate edin** (3-6 ayda bir)

### ?? Gizlilik
1. **Hassas bilgiler içeren mail'leri çevirmeden önce düþünün**
2. **Þirket politikalarýna uyun**
3. **GDPR/KVKK uyumluluðunu kontrol edin**

---

## ?? Ýpuçlarý

### Daha Ýyi Çeviri Ýçin
1. **Kýsa ve net cümleler kullanýn**
2. **Jargon/argoya dikkat** (bazen çevrilmeyebilir)
3. **Kýsaltmalarý açýn** (örn: "mrb" yerine "merhaba")

### Performans Ýçin
1. **Çok uzun mail'leri bölün**
2. **Model cache ilk çaðrýdan sonra hýzlanýr**
3. **Internet hýzýnýz önemli** (3-5 saniye sürer)

---

**?? Ýyi çeviriler! inþaAllah iþinizi kolaylaþtýrýr.**

**Destek:** [GitHub Issues](https://github.com/[your-repo]/issues)
