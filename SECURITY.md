# Guvenlik Politikasi (Security Policy)

## Desteklenen Surumler

| Surum | Destek Durumu |
|-------|---------------|
| 1.2.x | Aktif destek |
| 1.1.x | Guvenlik yamalari |
| 1.0.x | Guvenlik acigi var — yukseltme onerilir |
| < 1.0 | Destek yok |

## Bilinen Guvenlik Sorunlari

### v1.0.0 — Kritik Guvenlik Aciklari (v1.1.0'da Duzeltildi)

#### 1. Hardcoded Sifreleme Parolasi
- **Ciddiyet:** KRITIK
- **Durum:** Duzeltildi (v1.1.0)
- **Aciklama:** API key sifreleme parolasi kaynak kodda acik metin (`"951753"`)
- **Cozum:** v1.1.0'a yukseltin — Machine + User + Entropy bazli sifreleme

#### 2. App.config'de Acik Metin API Key
- **Ciddiyet:** KRITIK
- **Durum:** Duzeltildi (v1.1.0)
- **Aciklama:** API key App.config dosyasinda acik metin olarak saklaniyordu
- **Cozum:** v1.1.0'a yukseltin — API key artik sadece Registry'de (sifreli)

#### 3. API Key Loglama
- **Ciddiyet:** ORTA
- **Durum:** Duzeltildi (v1.1.0)
- **Aciklama:** API key debug loglarinda acik gorunuyordu
- **Cozum:** Maskeleme eklendi (`AIza...tcmw`)

---

## Guvenlik Acigi Bildirimi

### Nasil Bildirilir?

Guvenlik acigi bulduysaniz **lutfen GitHub Issues kullanmayin** (acik herkes gorebilir).

**Guvenli iletisim kanallari:**
1. **Email:** security@zaferbilgisayar.com (tercih edilen)
2. **GitHub Security Advisory:** Repository > Security > Advisories

### Bildirim Formati

```
Baslik:            [Kisa aciklama]
Ciddiyet:          [Kritik / Yuksek / Orta / Dusuk]
Etkilenen Surum:   [Orn: v1.0.0 - v1.0.5]
Aciklama:          [Detayli aciklama]
Tekrar Adimlari:   [1. ... 2. ... 3. ...]
Etki:              [Bu acik ne yapilmasina izin veriyor?]
Onerilen Cozum:    [Varsa duzeltme oneriniz]
```

### Yanit Suresi

| Asama | Sure |
|-------|------|
| Ilk yanit | 48 saat |
| Ciddiyet degerlendirmesi | 5 is gunu |
| Kritik duzeltme | 7 gun |
| Yuksek duzeltme | 30 gun |
| Orta duzeltme | 90 gun |

---

## Guvenlik En Iyi Uygulamalari

### Kullanicilar Icin

**API Key Guvenligi:**
- API anahtarinizi kimseyle paylasmayin
- Duzenli olarak degistirin (3-6 ayda bir)
- Guclu Google hesabi sifresi kullanin
- 2FA (Iki Faktorlu Dogrulama) aktif edin
- API anahtarini e-posta ile gondermeyin

**Mail Gizliligi:**
- Hassas bilgiler iceren mailleri cevirmeden once dusunun
- Gemini API'ye gonderilen mailler Google'a gider
- Sirket gizlilik politikalarina uyun
- GDPR/KVKK uyumlulugunu kontrol edin

**Guncelleme:**
- En son surumu kullanin (guvenlik yamalari)
- CHANGELOG.md'yi okuyun (breaking changes)

### Gelistiriciler Icin

**Kod Guvenligi:**
- Asla hardcoded parola kullanmayin
- Asla API key'i kod icine yazmayin
- Sensitive data'yi loglamayin (veya maskeleyin)

**Dependency Yonetimi:**
- NuGet paketlerini guncel tutun
- Bilinen guvenlik acigi olan paket kullanmayin

**Build ve Release:**
- Secrets asla Git'e commit etmeyin (`.gitignore`)
- Release artifact'lari imzalayin (code signing)

---

## Sifreleme Detaylari (v1.1.0+)

**Algoritma:** AES-256

**Key Derivation:**
```
Girdi:
  - Machine Name (Environment.MachineName)
  - User Name (Environment.UserName)
  - Static Entropy (16 bytes)
      |
      v  SHA-256
  32-byte AES Key (her makine+kullanici icin benzersiz)
      |
      v  AES-256 + Rastgele IV
  Sifreli Veri (Base64) -> Registry
```

**Depolama:** `HKEY_CURRENT_USER\Software\ZaferBilgisayar\GeminiTranslate`
- `EncryptedApiKey`: Sifreli API key (Base64)
- `IV`: Initialization Vector (Base64)
