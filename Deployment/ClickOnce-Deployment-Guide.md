# Gemini Outlook Translate Add-in - ClickOnce Deployment Guide

## ?? Quick Start (Developer)

### Visual Studio Publish Steps:

1. **Open Solution in Visual Studio 2022**
   ```
   GeminiOutlookTranslateAdd-in.sln
   ```

2. **Set Build Configuration to Release**
   ```
   Build ? Configuration Manager ? Active solution configuration ? Release
   ```

3. **Right-click on Project ? Publish**
   ```
   Solution Explorer ? GeminiOutlookTranslateAdd-in (right-click) ? Publish
   ```

4. **Configure Publish Settings:**

   | Setting | Value |
   |---------|-------|
   | **Publishing Folder** | `C:\Publish\GeminiTranslate\` |
   | **Installation URL** | `\\server\shared\GeminiTranslate\` (optional) |
   | **Prerequisites** | .NET Framework 4.8, VSTO Runtime |
   | **Update Settings** | Check for updates before start |
   | **Version** | Auto-increment on publish |

5. **Publish**
   ```
   Click "Publish Now" button
   ```

---

## ?? Published Files Structure

```
C:\Publish\GeminiTranslate\
??? setup.exe                          ? USERS RUN THIS!
??? GeminiOutlookTranslateAdd-in.vsto  ? Deployment manifest
??? publish.htm                         ? Auto-generated webpage
??? Application Files\
?   ??? GeminiOutlookTranslateAdd-in_1_0_0_0\
?       ??? GeminiOutlookTranslateAdd-in.dll.deploy
?       ??? GeminiOutlookTranslateAdd-in.dll.manifest
?       ??? HtmlAgilityPack.dll.deploy
?       ??? Newtonsoft.Json.dll.deploy
?       ??? Microsoft.Office.Tools.*.dll.deploy
??? autorun.inf (optional)
```

---

## ?? Code Signing (Recommended for Production)

### Generate Self-Signed Certificate (Testing):

```powershell
# Create test certificate
New-SelfSignedCertificate `
    -Subject "CN=Zafer Bilgisayar" `
    -Type CodeSigningCert `
    -CertStoreLocation "Cert:\CurrentUser\My" `
    -NotAfter (Get-Date).AddYears(5)

# Export to PFX
$cert = Get-ChildItem -Path "Cert:\CurrentUser\My" | Where-Object {$_.Subject -eq "CN=Zafer Bilgisayar"}
Export-PfxCertificate -Cert $cert -FilePath "C:\Certs\ZaferBilgisayar.pfx" -Password (ConvertTo-SecureString -String "YourPassword123" -Force -AsPlainText)
```

### Sign in Visual Studio:

```
Project Properties ? Signing ?
? Sign the ClickOnce manifests
? Select from File: ZaferBilgisayar.pfx
? Password: YourPassword123
```

---

## ?? Network Deployment (Enterprise)

### Share Folder Setup:

```powershell
# Create shared folder
New-Item -ItemType Directory -Path "\\server\shared\GeminiTranslate" -Force

# Copy published files
Copy-Item -Path "C:\Publish\GeminiTranslate\*" -Destination "\\server\shared\GeminiTranslate\" -Recurse -Force

# Set permissions
icacls "\\server\shared\GeminiTranslate" /grant "Domain Users:(OI)(CI)RX"
```

### Update Installation URL in Publish Settings:

```
Publishing folder: C:\Publish\GeminiTranslate\
Installation URL: \\server\shared\GeminiTranslate\
```

---

## ?? Auto-Update Configuration

**Visual Studio ? Project Properties ? Publish ? Updates:**

```
? The application should check for updates
? Before the application starts (Recommended)
? After the application starts

Update location: \\server\shared\GeminiTranslate\

Minimum required version: 1.0.0.0 (optional)
```

**Users will automatically get updates when you publish new versions!**

---

## ?? Pre-Publish Checklist

- [ ] Build in **Release** mode
- [ ] Set version number (Project Properties ? Publish ? Version)
- [ ] Sign manifests with certificate
- [ ] Test locally first
- [ ] Configure auto-update URL
- [ ] Add prerequisites (.NET 4.8, VSTO Runtime)
- [ ] Create README/Installation guide

---

## ?? Testing Deployment

### Test on Clean Machine:

1. **Uninstall existing add-in** (if any)
   ```
   Control Panel ? Programs ? Uninstall
   ```

2. **Run setup.exe**
   ```
   C:\Publish\GeminiTranslate\setup.exe
   ```

3. **Verify installation**
   - Outlook ? File ? Options ? Add-ins
   - Should see "Gemini Translate"

4. **Test functionality**
   - Open a mail
   - Click "Türkçe ? Ýngilizce"
   - Verify translation works

---

## ??? Advanced: Command-Line Publish

```powershell
# Build Release
msbuild GeminiOutlookTranslateAdd-in.csproj /p:Configuration=Release

# Publish (requires Visual Studio MSBuild)
msbuild GeminiOutlookTranslateAdd-in.csproj /t:Publish /p:Configuration=Release /p:PublishDir="C:\Publish\GeminiTranslate\"
```

---

## ?? Version Management

### Auto-increment version on publish:

**Project Properties ? Publish ? Version:**
```
Version: 1.0.0.* (auto-increment)
? Automatically increment revision with each publish
```

### Version History:
- `1.0.0.0` - Initial release
- `1.0.0.1` - Turkish spelling fix + single API call
- `1.0.0.2` - Punctuation preservation fix

---

## ?? Security & Trust

### Trusted Location (Alternative to signing):

```powershell
# Add installation path to Trusted Locations
$trustPath = "HKCU:\Software\Microsoft\Office\16.0\Outlook\Security\Trusted Locations\Location20"
New-Item -Path $trustPath -Force
Set-ItemProperty -Path $trustPath -Name "Path" -Value "$env:LOCALAPPDATA\Apps\2.0\*\GeminiOutlookTranslateAdd-in*"
Set-ItemProperty -Path $trustPath -Name "AllowSubfolders" -Value 1 -Type DWord
```

---

## ?? Distribution Methods

### Method 1: USB/Download (Simple)
```
Zip setup.exe + Application Files folder
Share with users
Users run setup.exe
```

### Method 2: Network Share (Enterprise)
```
\\fileserver\software\GeminiTranslate\setup.exe
Users click to install
Auto-updates from same location
```

### Method 3: Email Link
```
Send email with link to setup.exe
Users click and install
```

---

## ?? Common Issues

### "The application cannot be started"
**Fix:** Install .NET Framework 4.8 + VSTO Runtime

### "Trust not granted"
**Fix:** Sign with certificate OR add to Trusted Locations

### "Application is already installed"
**Fix:** 
```
Control Panel ? Programs ? Uninstall old version
Then run setup.exe again
```

### Setup.exe missing
**Fix:**
```
Project Properties ? Publish ?
? Create setup program to install prerequisites
```

---

## ?? Next Steps

After publishing:

1. ? Test setup.exe on another machine
2. ? Verify auto-update works
3. ? Document API Key setup process
4. ? Create user guide (already done: KURULUM-REHBERÝ.txt)
5. ? Deploy to production

---

**Made with ?? by Zafer Bilgisayar - Auto-System**
