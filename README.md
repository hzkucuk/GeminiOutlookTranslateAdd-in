<div align="center">

# 📧 Gemini Outlook Translate Add-in

**Outlook e-postalarınızı Google Gemini AI ile anında çevirin.**

[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.8-blue?logo=dotnet)](https://dotnet.microsoft.com/)
[![Outlook VSTO](https://img.shields.io/badge/Outlook-VSTO%20Add--in-0078D4?logo=microsoftoutlook)](https://learn.microsoft.com/tr-tr/visualstudio/vsto/)
[![Gemini API](https://img.shields.io/badge/Google-Gemini%20API-4285F4?logo=google)](https://aistudio.google.com/)
[![Lisans](https://img.shields.io/badge/Lisans-MIT-green)](LICENSE)
[![Sürüm](https://img.shields.io/badge/S%C3%BCr%C3%BCm-1.4.1-orange)](CHANGELOG.md)

<br/>

Türkçe ↔ İngilizce çift yönlü çeviri · Otomatik imla düzeltme · HTML format koruma · AES-256 şifreleme

[📦 MSI İndir](https://github.com/hzkucuk/GeminiOutlookTranslateAdd-in/releases) · [📁 ZIP İndir](https://github.com/hzkucuk/GeminiOutlookTranslateAdd-in/releases)

</div>

---

## 📖 İçindekiler

- [Özellikler](#-özellikler)
- [Gereksinimler](#-gereksinimler)
- [Kurulum](#-kurulum)
- [Kullanım](#-kullanım)
- [Güvenlik ve Gizlilik](#-güvenlik-ve-gizlilik)
- [Yapılandırma](#%EF%B8%8F-yapılandırma)
- [Teknik Mimari](#%EF%B8%8F-teknik-mimari)
- [Sorun Giderme](#-sorun-giderme)
- [Sınırlamalar](#%EF%B8%8F-sınırlamalar)
- [Katkıda Bulunma](#-katkıda-bulunma)
- [Lisans](#-lisans)

---

## ✨ Özellikler

### Çeviri

| Özellik | Açıklama |
|---------|----------|
| 🔄 Çift yönlü çeviri | Türkçe → İngilizce ve İngilizce → Türkçe |
| ✏️ Otomatik imla düzeltme | TR → EN çevirisinde yazım hataları otomatik düzeltilir |
| 📝 Noktalama düzeltme | Eksik virgül, nokta ve soru işaretleri eklenir |
| 🎨 HTML format koruma | Bold, italic, renkler, tablolar ve linkler korunur |
| 📌 Orijinal metin koruma | Çeviri üstte, orijinal metin altta gösterilir |
| 🔁 Yeniden çeviri desteği | Önceki çeviri otomatik kaldırılır, sadece orijinal metin kullanılır |
| 🛡️ Kısaltma ve imza koruma | CEO, API, HZK, [ABC] gibi ifadeler korunur |
| 🚫 Küfür filtresi | Uygunsuz içerik `[FILTERED]` ile değiştirilir |
| ⏹️ İptal butonu | Uzun çevirileri anında durdurabilirsiniz |
| 📏 Karakter limiti | 8000 karakter üzeri için kullanıcı onayı istenir |

### Güvenlik

| Özellik | Açıklama |
|---------|----------|
| 🔐 AES-256 şifreleme | API anahtarı güvenli şekilde şifrelenir |
| 🖥️ Makine bazlı anahtar | Her bilgisayar + kullanıcı için farklı şifreleme |
| 📦 Registry saklama | `HKEY_CURRENT_USER` altında güvenli depolama |
| 🙈 Log maskeleme | API anahtarı loglarda maskeli gösterilir (`AIza...tcmw`) |
| 🚫 Hardcoded şifre yok | Statik şifre kullanılmaz, entropy tabanlı |

### Performans

| Özellik | Açıklama |
|---------|----------|
| ⚡ Tek API çağrısı | İmla düzeltme + çeviri birleştirildi (2x hızlı) |
| 💾 Model cache | İlk çağrıdan sonra model bilgisi cache'lenir |
| 🔒 Thread-safe | Eşzamanlı işlem desteği |
| 🔄 Async/Await | Outlook arayüzü çeviri sırasında donmaz |

---

## 📋 Gereksinimler

| Bileşen | Minimum Sürüm |
|---------|---------------|
| .NET Framework | 4.8 |
| Microsoft Outlook | VSTO destekli (2013 / 2016 / 2019 / 365) |
| Visual Studio | 2019+ (derleme için) |
| VSTO Runtime | Visual Studio Tools for Office |
| İnternet | Gemini API erişimi için gerekli |
| Google Gemini API Key | [Ücretsiz alın →](https://aistudio.google.com/) |

---

## 🚀 Kurulum

İki kurulum yöntemi mevcuttur:

### Yöntem 1 — MSI Installer (Önerilen)

1. [Releases](https://github.com/hzkucuk/GeminiOutlookTranslateAdd-in/releases) sayfasından `GeminiTranslate-v*-setup.msi` dosyasını indirin
2. MSI dosyasını çift tıklayarak çalıştırın
3. Kurulum sihirbazını takip edin

> MSI, imza sertifikasını ve Outlook registry kaydını **otomatik** yapar.

### Yöntem 2 — ZIP Paketi

1. [Releases](https://github.com/hzkucuk/GeminiOutlookTranslateAdd-in/releases) sayfasından `*-setup.zip` dosyasını indirin
2. ZIP'i bir klasöre çıkarın
3. **`KUR.bat`** dosyasına çift tıklayın (yönetici izni otomatik istenir)

`KUR.bat` şunları yapar:
- Sertifikayı Trusted Root + Trusted Publisher store'a yükler
- VSTOInstaller ile eklentiyi Outlook'a kaydeder

### API Anahtarı Kaydetme

> ⚠️ API anahtarı `App.config` dosyasında **tutulmaz**. Ribbon üzerinden kaydedin.

1. Outlook'u açın (yeniden başlatmanız gerekebilir)
2. Herhangi bir mail açın
3. **Eklentiler** sekmesinde **"Zafer Bilgisayar Çeviri"** grubunu bulun
4. **API Key** metin kutusuna [Google AI Studio](https://aistudio.google.com/) üzerinden aldığınız anahtarı yapıştırın
5. **Kaydet** butonuna tıklayın
6. ✅ *"API Key başarıyla kaydedildi ve şifrelendi"* mesajını görmelisiniz

### Geliştirici Kurulumu (Kaynak Koddan)

```bash
git clone https://github.com/hzkucuk/GeminiOutlookTranslateAdd-in.git
cd GeminiOutlookTranslateAdd-in
copy GeminiOutlookTranslateAdd-in\App.config.template GeminiOutlookTranslateAdd-in\App.config
```

1. Visual Studio 2022'de `GeminiOutlookTranslateAdd-in.slnx` dosyasını açın
2. NuGet paketlerinin geri yüklenmesini bekleyin
3. **F5** ile debug modunda başlatın — Outlook otomatik açılacaktır

---

## 🎯 Kullanım

### Temel Çeviri Adımları

```
1. Outlook'ta mail açın (yeni, yanıt veya iletme)
2. Mail içeriğini yazın
3. Ribbon'da çeviri butonuna basın:
   • "Türkçe → İngilizce"  →  İmla düzeltme + çeviri
   • "İngilizce → Türkçe"  →  Doğrudan çeviri
4. Çeviri tamamlanır (ortalama 3-5 saniye)
5. Manuel kaydedin (Ctrl+S)
```

### Türkçe → İngilizce (İmla Düzeltme Dahil)

```
Girdi:   "merhba nasılsnız bu projeyi begendiniz mi"
           ↓ İmla düzeltme + çeviri (tek API çağrısı)
Çıktı:   "Hello, how are you? Did you like this project?"
```

Düzeltilen hata türleri:
- Yazım hataları (merhba → merhaba)
- Noktalama eksiklikleri (virgül, nokta, soru işareti)
- Büyük/küçük harf düzeltmeleri

### İptal Etme

Çeviri devam ederken **Durdur** butonuna basabilirsiniz:
- Çeviri anında durdurulur
- Konu alanındaki "(Çevriliyor...)" etiketi kaldırılır
- Mail eski haline döner

### Yeniden Çeviri

Daha önce çevrilmiş bir mail'i tekrar çevirirseniz önceki çeviri otomatik kaldırılır ve sadece orijinal metin kullanılır.

---

## 🔒 Güvenlik ve Gizlilik

### API Anahtarı Şifreleme Mimarisi

```
┌────────────────────────────────────────────┐
│  API Key (düz metin)                       │
│        ↓                                   │
│  Machine Name + User Name + Static Entropy │
│        ↓  SHA-256                          │
│  32-byte Benzersiz Anahtar                 │
│        ↓  AES-256 + Rastgele IV            │
│  Şifreli Veri (Base64)                     │
│        ↓                                   │
│  Registry: HKCU\Software\                  │
│            ZaferBilgisayar\GeminiTranslate  │
└────────────────────────────────────────────┘
```

**Güvenlik garantileri:**
- Her bilgisayar + kullanıcı kombinasyonu için farklı şifreleme anahtarı
- IV her kayıtta yeniden oluşturulur (replay attack koruması)
- Kaynak kodda hardcoded şifre bulunmaz

### Veri Gönderimi

| Gönderilen | Gönderilmeyen |
|-----------|---------------|
| Mail gövdesi (HTML veya düz metin) | Alıcı / gönderen bilgileri |
| Mail konusu (RE: ile başlamıyorsa) | Ekler (attachments) |
| | Mail thread geçmişi (otomatik kesilir) |

> ⚠️ Mail içerikleri Google Gemini API'ye HTTPS ile gönderilir.
> Hassas bilgiler içeren mailleri çevirmeden önce şirket politikalarınızı kontrol edin.

Detaylı güvenlik politikası için: [SECURITY.md](SECURITY.md)

---

## ⚙️ Yapılandırma

### App.config

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <appSettings>
    <!-- API Key burada TUTULMAZ — Ribbon üzerinden kaydedin -->
    <add key="GeminiModel" value="gemini-2.0-flash-exp" />
  </appSettings>
</configuration>
```

### Model Seçenekleri

| Model | Hız | Kalite | Açıklama |
|-------|-----|--------|----------|
| `gemini-2.0-flash-exp` | ⚡⚡⚡ | ★★★ | Varsayılan — en hızlı |
| `gemini-1.5-flash` | ⚡⚡ | ★★★ | Kararlı, hızlı |
| `gemini-1.5-pro` | ⚡ | ★★★★★ | Yavaş ama daha kaliteli |

---

## 🏗️ Teknik Mimari

### Proje Yapısı

```
GeminiOutlookTranslateAdd-in/
├── RibbonCeviri.cs              # Ana iş mantığı (çeviri, API, şifreleme)
├── RibbonCeviri.Designer.cs     # Ribbon UI tanımları (otomatik üretilir)
├── RibbonCeviri.resx            # Ribbon kaynakları
├── RibbonIconHelper.cs          # Programatik ikon üretimi (GDI+)
├── ThisAddIn.cs                 # VSTO giriş noktası
├── ThisAddIn.Designer.cs        # VSTO tasarımcı (otomatik üretilir)
├── App.config.template          # Yapılandırma şablonu
└── Properties/
    ├── AssemblyInfo.cs           # Derleme bilgileri (versiyon kaynağı)
    ├── Resources.resx            # Gömülü kaynaklar
    └── Settings.settings         # Uygulama ayarları

Deployment/
├── Build-Release.ps1            # Otomatik release pipeline (ZIP + MSI + Git)
├── GeminiTranslate-CodeSigning.cer  # İmza sertifikası (public)
├── KUR.bat                      # Son kullanıcı tek-tık kurulum
├── Kur-Sertifika-ve-Addin.bat   # Sertifika + eklenti kurulum
├── KURULUM-REHBERİ.txt          # Kurulum kılavuzu
└── MSI/
    ├── GeminiTranslate.wxs      # WiX 6 MSI tanımı
    ├── Build-MSI.ps1            # MSI derleme scripti
    └── install-cert.cmd         # Sertifika kurulum komutu
```

### Kod Mimarisi

```
RibbonCeviri.cs
├── API Key Yönetimi
│   ├── SaveEncryptedApiKey()           → AES-256 şifreleme
│   ├── LoadEncryptedApiKey()           → AES-256 çözme
│   ├── DeriveKeyFromMachineEntropy()   → Makine bazlı anahtar türetme
│   └── MaskApiKey()                    → Güvenli loglama
├── Çeviri İşlemleri
│   ├── TranslateCurrentMailAsync()     → Ana orkestratör
│   ├── TranslateHtmlBodyAsync()        → Segment bazlı HTML çeviri
│   ├── TranslateTextAsync()            → Düz metin çevirisi
│   └── SendTranslationRequestAsync()   → Gemini API iletişimi
├── Yardımcı Metodlar
│   ├── ExtractOriginalHtml/Text()      → Orijinal içerik ayıklama
│   ├── BuildTranslatedHtmlBody()       → HTML birleştirme
│   ├── CleanTranslatedText()           → Son işleme
│   ├── MaskSignaturesAndAbbreviations()→ Kısaltma koruma
│   └── FilterProfanity()              → Küfür filtresi
└── UI Event Handlers
    ├── btnTranslate_Click()            → TR → EN
    ├── btnTranslateToTurkish_Click()   → EN → TR
    └── btnCancelTranslation_Click()    → İptal
```

### HTML Segment İşleme

```html
<!-- Girdi -->
<p>Merhaba</p>        →  [[SEG0]]Merhaba[[/SEG0]]
<b>Nasılsınız?</b>    →  [[SEG1]]Nasılsınız?[[/SEG1]]
                         ↓ Gemini API
                       [[SEG0]]Hello[[/SEG0]]
                       [[SEG1]]How are you?[[/SEG1]]
                         ↓ Geri yerleştirme
<!-- Çıktı -->
<p>Hello</p>
<b>How are you?</b>
```

Korunan HTML elementleri: `<script>`, `<style>`, `<head>` → çevrilmez.

### Thread Safety

```csharp
// CancellationToken — eşzamanlı erişim koruması
private static readonly object CancellationLock = new object();
lock (CancellationLock) { GlobalCancellationSource = localCancellationSource; }

// Model Cache — volatile ile thread-safe okuma/yazma
private static volatile string CachedModelName;
```

---

## 🔧 Sorun Giderme

### "API Key kayıtlı değil" Hatası

1. Ribbon üzerinden API anahtarını kaydedin
2. Doğru anahtarı kopyaladığınızdan emin olun
3. İnternet bağlantınızı kontrol edin

### "API model listesi alınamadı" Hatası

| Olası Neden | Çözüm |
|-------------|-------|
| API anahtarı geçersiz | [aistudio.google.com](https://aistudio.google.com/) üzerinden kontrol edin |
| İnternet bağlantısı yok | Bağlantınızı kontrol edin |
| Firewall/Proxy engeli | Gemini API'yi beyaz listeye ekleyin |
| API kotası dolmuş | 1 dakika bekleyip tekrar deneyin |

### Eski API Key Temizleme (v1.0.0'dan yükseltme)

```powershell
# PowerShell (Yönetici olarak)
Remove-Item -Path "HKCU:\Software\ZaferBilgisayar\GeminiTranslate" -Recurse -Force
```

Ardından Ribbon'dan yeniden kaydedin.

### Sertifika / Güvenilirlik Sorunu

Outlook eklentiyi blokluyor veya güvenilirlik uyarısı veriyorsa:

1. **MSI ile kurdunuz:** Sertifika otomatik yüklenir, Outlook'u yeniden başlatın
2. **ZIP ile kurdunuz:** `KUR.bat`'ı yönetici olarak çalıştırın
3. **Manuel çözüm:** PowerShell'i yönetici olarak açın:

```powershell
certutil -addstore Root "GeminiTranslate-CodeSigning.cer"
certutil -addstore TrustedPublisher "GeminiTranslate-CodeSigning.cer"
```

### Eklenti Outlook'ta Görünmüyor

1. Outlook → **Dosya** → **Seçenekler** → **Eklentiler**
2. "Devre Dışı Bırakılan Uygulama Eklentileri" listesini kontrol edin
3. Eklenti oradaysa **Etkinleştir** butonuna tıklayın
4. Outlook'u yeniden başlatın

### Release Build

Otomatik pipeline için:

```powershell
.\Deployment\Build-Release.ps1
```

---

## ⚠️ Sınırlamalar

| Kısıtlama | Detay |
|-----------|-------|
| Karakter limiti | 8000 karakter (onay ile kısmi çeviri yapılır) |
| Subject çevirisi | `RE:` ile başlayan konular çevrilmez |
| HTML desteği | Çok kompleks yapılarda format kaybı olabilir |
| API kotası | Gemini free tier: 60 istek/dakika |
| Çevrimdışı | İnternet bağlantısı olmadan çalışmaz |

---

## 🤝 Katkıda Bulunma

Öneriler ve hata bildirimleri için [issue açabilirsiniz](https://github.com/hzkucuk/GeminiOutlookTranslateAdd-in/issues).

1. Bu projeyi forklayın
2. Feature branch oluşturun (`git checkout -b feature/yeni-ozellik`)
3. Değişikliklerinizi commit edin (`git commit -m 'Yeni özellik eklendi'`)
4. Branch'inizi push edin (`git push origin feature/yeni-ozellik`)
5. Pull Request açın

---

## 📜 Lisans

Bu proje [MIT Lisansı](LICENSE) ile lisanslanmıştır.

Copyright (c) 2025 Zafer Bilgisayar

---

<div align="center">

**Zafer Bilgisayar** tarafından geliştirilmiştir.

Sürüm 1.4.1 · [📋 Değişiklik Günlüğü](CHANGELOG.md) · [🐛 Hata Bildir](https://github.com/hzkucuk/GeminiOutlookTranslateAdd-in/issues) · [💡 Özellik İste](https://github.com/hzkucuk/GeminiOutlookTranslateAdd-in/issues)

</div>
