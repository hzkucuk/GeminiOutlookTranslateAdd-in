# Degisiklik Gunlugu (Changelog)

Tum onemli degisiklikler bu dosyada belgelenmistir.
Format [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) standardina uygundur.

---

## [1.2.0] - 2025-03-01

### Eklenenler
- Programatik ikon uretimi (`RibbonIconHelper.cs`) — harici gorsel dosyalara bagimliligi kaldirdi
- Flat-design modern buton ikonlari (GDI+ ile runtime'da uretilir)
- `.gitattributes` dosyasi eklendi (tutarli satir sonlari)
- `.editorconfig` dosyasi eklendi (tutarli kod stili)

### Degistirilenler
- Buton ikonlari: Gemini AI uretimi gorseller yerine programatik vektorel ikonlar
- "Durdur" buton etiketi: emoji (`❌`) kaldirildi, temiz ikon ile degistirildi
- Dokumantasyon dosyalari UTF-8 encoding ile yeniden yazildi
- README.md profesyonel GitHub formatiyla yeniden duzenlendi

---

## [1.1.0] - 2025-01-XX

### Guvenlik (Kritik)
- **KRITIK:** Hardcoded sifreleme parolasi (`"951753"`) kaldirildi
- **KRITIK:** Machine Name + User Name + Entropy bazli sifreleme eklendi
- **KRITIK:** App.config'den API key kaldirildi (guvenlik acigi kapatildi)
- API key maskeleme eklendi (loglarda guvenli gosterim)
- Exception handling iyilestirildi (veri sizintisi onlendi)
- Registry encryption AES-256 ile guclendirildi

### Performans
- Imla duzeltme + ceviri tek API cagrisinda birlestirildi (2x daha hizli)
- Thread-safe CancellationToken implementasyonu
- Thread-safe Model Cache (`volatile` keyword)
- Word COM API bagimliligi kaldirildi (imla kontrolu icin Gemini kullaniliyor)

### Yeni Ozellikler
- Otomatik Turkce imla duzeltme (TR -> EN cevirisinde)
- Otomatik noktalama duzeltme (virgul, nokta, soru isareti ekleme)
- Mail otomatik save kaldirildi (kullanici kontrol)
- API key Registry'den yonetimi (Ribbon UI)

### Degistirilenler
- `SaveEncryptedApiKey()` -> Machine-specific key generation
- `LoadEncryptedApiKey()` -> Entropy-based decryption
- `SendTranslationRequestAsync()` -> App.config bagimliligi kaldirildi
- `DeriveKeyFromPassword()` -> `DeriveKeyFromMachineEntropy()` ile degistirildi
- Hata mesajlari guncellendi (App.config referansi kaldirildi)

### Duzeltmeler
- Thread safety sorunlari (CancellationToken race condition)
- Model cache thread safety
- Bos catch bloklari duzeltildi (exception loglama eklendi)
- API key loglama guvenligi (maskeleme)

### Kaldirilanlar
- `EncryptionPassword` constant (hardcoded "951753")
- `GeminiApiKeySettingKey` constant (App.config kullanilmiyor)
- `DeriveKeyFromPassword()` metodu
- Word COM API bagimliligi
- App.config'den API key okuma kodu

### Breaking Changes
- **UYARI:** Eski API key'ler calismayacak!
  - v1.0.0'da kaydedilmis API key'ler yeni sifreleme ile uyumsuz
  - Migration: Registry temizleyin ve yeniden kaydedin
  ```powershell
  Remove-Item -Path "HKCU:\Software\ZaferBilgisayar\GeminiTranslate" -Recurse -Force
  ```

---

## [1.0.0] - 2025-01-XX (Ilk Surum)

### Yeni Ozellikler
- Turkce <-> Ingilizce cift yonlu ceviri
- HTML format koruma
- Segment bazli HTML isleme
- Gemini API entegrasyonu
- Ribbon UI (Ingilizceye Cevir / Turkceye Cevir butonlari)
- Iptal butonu (uzun ceviriler icin)
- Orijinal metin koruma
- Subject cevirisi (RE: yoksa)
- Kisaltma ve imza koruma (CEO, API, [ABC] vb.)
- Kufur filtresi
- 8000 karakter limiti (kullanici onayi)
- Mail thread temizleme (From:, Sent: vb.)
- AES-256 sifreleme

### Bilinen Sorunlar
- App.config'de API key acik metin (GUVENLIK ACIGI — v1.1.0'da duzeltildi)
- Hardcoded sifreleme parolasi (GUVENLIK ACIGI — v1.1.0'da duzeltildi)
- Thread safety sorunlari (race condition — v1.1.0'da duzeltildi)

---

## Yol Haritasi (Roadmap)

### v1.3.0 (Gelecek)
- [ ] Coklu dil destegi (Ispanyolca, Fransizca, Almanca)
- [ ] Kullanici sozlugu (custom translations)
- [ ] Ceviri gecmisi (history)
- [ ] Toplu ceviri (batch translation)
- [ ] Sag tik menu entegrasyonu (context menu)

### v2.0.0 (Uzun Vade)
- [ ] Cevrimdisi mod (cached translations)
- [ ] AI tabanli otomatik dil tespiti
- [ ] Ceviri kalite geri bildirimi
- [ ] Takim sozlugu paylasimi
