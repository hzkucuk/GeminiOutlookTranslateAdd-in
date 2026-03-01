# Security Policy

## ?? Güvenlik Politikasý

Bu belge, Gemini Outlook Translate Add-in projesinin güvenlik politikasýný ve güvenlik açýðý bildirimi prosedürlerini içerir.

## Desteklenen Sürümler

Güvenlik güncellemeleri sadece aþaðýdaki sürümler için saðlanýr:

| Sürüm | Destek Durumu |
| ------- | ------------------ |
| 1.1.x   | ? Aktif destek    |
| 1.0.x   | ?? Güvenlik açýðý var (yükseltme önerilir) |
| < 1.0   | ? Destek yok      |

## Bilinen Güvenlik Sorunlarý

### v1.0.0 - KRÝTÝK GÜVENLÝK AÇIKLARI (DÜZELTÝLDÝ v1.1.0'da)

#### 1. Hardcoded Þifreleme Password'ü (CVE-XXXX-XXXX)
- **Ciddiyet:** KRÝTÝK ??
- **Durum:** ? Düzeltildi (v1.1.0)
- **Açýklama:** 
  - v1.0.0'da API key þifreleme password'ü kaynak kodda hardcoded (`"951753"`)
  - Herkes kaynak kodu okuyarak password'ü görebilir
  - Registry'deki þifrelenmiþ key'i decrypt edebilir
- **Çözüm:**
  - v1.1.0'a yükseltin
  - Machine + User + Entropy bazlý þifreleme kullanýlýyor
  - Hardcoded password yok

#### 2. App.config'de Açýk Metin API Key (CVE-XXXX-YYYY)
- **Ciddiyet:** KRÝTÝK ??
- **Durum:** ? Düzeltildi (v1.1.0)
- **Açýklama:**
  - v1.0.0'da API key App.config dosyasýnda açýk metin
  - Git repository'e commit edilmiþ olabilir
  - Herkes okuyabilir
- **Çözüm:**
  - v1.1.0'a yükseltin
  - API key artýk sadece Registry'de (þifreli)
  - App.config'den kaldýrýldý

#### 3. API Key Loglama (CVE-XXXX-ZZZZ)
- **Ciddiyet:** ORTA ??
- **Durum:** ? Düzeltildi (v1.1.0)
- **Açýklama:**
  - v1.0.0'da API key debug loglarýnda açýk görünür
  - Log dosyalarýna yazýlabilir
- **Çözüm:**
  - v1.1.0'da API key maskeleme eklendi
  - Sadece ilk 4 ve son 4 karakter görünür (`AIza...tcmw`)

## Güvenlik Açýðý Bildirimi

### Nasýl Bildirilir?

Güvenlik açýðý bulduysanýz, **lütfen GitHub Issues kullanmayýn!** (açýðý herkes görebilir)

**Güvenli iletiþim kanallarý:**
1. **Email:** security@zaferbilgisayar.com (tercih edilen)
2. **PGP:** [PGP Public Key](https://keybase.io/[your-keybase])
3. **Private GitHub Security Advisory:** (sadece collaborator'lar)

### Bildirim Formatý

Lütfen aþaðýdaki bilgileri ekleyin:

```markdown
## Güvenlik Açýðý Bildirimi

**Baþlýk:** [Kýsa açýklama]

**Ciddiyet:** [Kritik / Yüksek / Orta / Düþük]

**Etkilenen Sürümler:**
- [örn: v1.0.0 - v1.0.5]

**Açýklama:**
[Detaylý açýklama]

**Tekrar Adýmlarý:**
1. [Adým 1]
2. [Adým 2]
3. [Adým 3]

**Etki:**
[Bu açýk ne yapýlmasýna izin veriyor?]

**Önerilen Çözüm:**
[Varsa düzeltme öneriniz]

**Ek Bilgiler:**
- Ýþletim Sistemi: [örn: Windows 11]
- .NET Sürümü: [örn: .NET Framework 4.8]
- Outlook Sürümü: [örn: Microsoft 365]
```

### Yanýt Süresi

- **Ýlk yanýt:** 48 saat içinde
- **Ciddiyet deðerlendirmesi:** 5 iþ günü içinde
- **Düzeltme süresi:** Ciddiyete göre
  - Kritik: 7 gün
  - Yüksek: 30 gün
  - Orta: 90 gün
  - Düþük: 180 gün

### Açýklama Politikasý

- Düzeltme yayýnlanana kadar açýðý açýklamayýn (Responsible Disclosure)
- Patch hazýrlandýktan sonra koordineli açýklama yapýlýr
- Haber verene credit verilir (isim belirtilmek istenirse)

## Güvenlik En Ýyi Uygulamalarý

### Kullanýcýlar Ýçin

#### 1. API Key Güvenliði
- ? **API key'i kimseyle paylaþmayýn**
- ? **Düzenli rotate edin** (3-6 ayda bir)
- ? **Güçlü Google hesabý þifresi kullanýn**
- ? **2FA (Two-Factor Authentication) aktif edin**
- ? **API key'i email ile göndermeyin**
- ? **Public chat/forum'larda paylaþmayýn**

#### 2. Mail Gizliliði
- ?? **Hassas bilgiler içeren mail'leri çevirmeden önce düþünün**
- ?? **Gemini API'ye gönderilen mail'ler Google'a gider**
- ?? **Þirket gizlilik politikalarýna uyun**
- ? **GDPR/KVKK uyumluluðunu kontrol edin**

#### 3. Güncelleme
- ? **En son sürümü kullanýn** (güvenlik yamalarý)
- ? **CHANGELOG.md'yi okuyun** (breaking changes)
- ? **Release notes'u takip edin**

#### 4. Registry Güvenliði
- ? **Bilgisayarýnýzý kilitli tutun**
- ? **Güçlü Windows þifresi kullanýn**
- ? **Ortak kullanýlan bilgisayarlarda kullanmayýn**

### Geliþtiriciler Ýçin

#### 1. Kod Güvenliði
- ? **Asla hardcoded password kullanmayýn**
- ? **Asla API key'i kod içine yazmayýn**
- ? **Environment variables veya secure storage kullanýn**
- ? **Sensitive data'yý loglamayýn** (veya maskeleyin)
- ? **Input validation yapýn** (XSS, injection önleme)
- ? **Output encoding yapýn** (HTML, SQL)

#### 2. Dependency Yönetimi
- ? **NuGet package'larý güncel tutun**
- ? **Bilinen güvenlik açýðý olan package kullanmayýn**
- ? **`dotnet list package --vulnerable` çalýþtýrýn**
- ? **Dependabot veya Snyk kullanýn**

#### 3. Code Review
- ? **Her commit security review'dan geçsin**
- ? **Pull request'lerde güvenlik checklist kullanýn**
- ? **SAST (Static Application Security Testing) toollarý kullanýn**

#### 4. Build ve Release
- ? **Secrets asla Git'e commit etmeyin** (`.gitignore`)
- ? **CI/CD pipeline'da secret scanning yapýn**
- ? **Release artifact'larý sign edin** (code signing)
- ? **Checksum veya hash yayýnlayýn** (integrity check)

## Güvenlik Testleri

### Yapýlan Testler

? **Static Analysis:**
- Visual Studio Code Analysis
- SonarQube
- FxCop

? **Dependency Scanning:**
- NuGet vulnerability scan
- OWASP Dependency-Check

? **Manual Testing:**
- Penetration testing (API key extraction)
- Reverse engineering attempts
- Encryption strength testing

### Yapýlmamýþ Testler (Katký bekleniyor)

? **Dynamic Analysis:**
- DAST (Dynamic Application Security Testing)
- Fuzzing

? **Third-party Audit:**
- Professional security audit
- Penetration testing by security firm

## Þifreleme Detaylarý

### v1.1.0+ (Mevcut)

**Algoritma:** AES-256 (Advanced Encryption Standard)

**Key Derivation:**
```
Input:
  - Machine Name (Environment.MachineName)
  - User Name (Environment.UserName)
  - Static Entropy (16 bytes)

Process:
  Machine Name + User Name + Entropy
  ? UTF8 Encoding
  Byte Array
  ? SHA-256 Hash
  32-byte AES Key (256-bit)

Output:
  - Unique key per machine + user
  - Deterministic (same machine/user = same key)
  - Non-extractable from code
```

**Encryption:**
```
Plain Text (API Key)
  ? UTF8 Encoding
Byte Array
  ? AES-256 CBC Mode
  ? Random IV (16 bytes per encryption)
Encrypted Byte Array
  ? Base64 Encoding
Registry Storage (String)
```

**Güçlü Yönler:**
- ? 256-bit key (military-grade)
- ? Unique key per machine + user
- ? Random IV per encryption
- ? No hardcoded secrets

**Zayýf Yönler:**
- ?? Static entropy in code (gözlemlenebilir)
- ?? Machine/user name predictable (local access)
- ?? Local admin API key'i okuyabilir

**Threat Model:**
- ? Protects against: Remote attackers, casual users, GitHub exposure
- ? Does NOT protect against: Local admin, debugger, memory dump

### v1.0.0 (Eski - GÜVENLÝK AÇIÐI)

**Algoritma:** AES-256

**Key Derivation:**
```
Hardcoded Password: "951753" ?
  ? SHA-256
32-byte Key
```

**Zayýflýklar:**
- ? Password kaynak kodda görünür
- ? Herkes ayný key'i kullanýr
- ? Reverse engineering ile kolayca çözülür

## Threat Model

### Koruma Kapsamý Ýçinde

? **Remote Attackers:**
- Internet üzerinden API key çalma
- GitHub repository'den API key bulma
- Log dosyalarýndan API key okuma

? **Casual Users:**
- Ayný bilgisayar kullanýcýlarý (farklý hesap)
- Registry browser ile okuma
- App.config okuma

? **Accidental Exposure:**
- Git commit
- Screenshot paylaþýmý
- Log dosyasý paylaþýmý

### Koruma Kapsamý DIÞINDA

? **Local Administrator:**
- Registry'den þifreli key'i okuyabilir
- Machine/user name'i bilir
- Entropy'yi reverse engineer edebilir
- API key'i decrypt edebilir

? **Debugger/Memory Dump:**
- Process memory'sinden plain text key çýkarýlabilir
- Debugger ile decryption iþlemi izlenebilir

? **Malware:**
- Keylogger API key'i yakalayabilir
- Memory scanner plain text bulabilir

## Ýletiþim

- **Güvenlik Email:** security@zaferbilgisayar.com
- **Genel Email:** destek@zaferbilgisayar.com
- **GitHub:** [https://github.com/[your-repo]]

## PGP Public Key

```
-----BEGIN PGP PUBLIC KEY BLOCK-----
[PGP Public Key buraya gelecek]
-----END PGP PUBLIC KEY BLOCK-----
```

## Hall of Fame (Security Researchers)

Bu kiþiler/organizasyonlar güvenlik açýðý bildirerek projeye katkýda bulunmuþtur:

- (Henüz kimse bildirmedi - ilk sen ol!)

## Referanslar

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [CWE (Common Weakness Enumeration)](https://cwe.mitre.org/)
- [CVE (Common Vulnerabilities and Exposures)](https://cve.mitre.org/)
- [Microsoft Security Development Lifecycle](https://www.microsoft.com/en-us/securityengineering/sdl/)

---

**Son Güncelleme:** 2025-01-XX
