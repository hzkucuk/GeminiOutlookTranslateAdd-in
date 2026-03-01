using Microsoft.Office.Tools.Ribbon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Diagnostics;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Microsoft.Office.Interop.Outlook;
using System.Security.Cryptography;
using Microsoft.Win32;
using OutlookException = Microsoft.Office.Interop.Outlook.Exception;
using WordApplication = Microsoft.Office.Interop.Word.Application;
using WordDocument = Microsoft.Office.Interop.Word.Document;


namespace GeminiOutlookTranslateAdd_in
{
    public partial class RibbonCeviri
    {
        private const string TextSeparator = "\n\n---ORIGINAL---\n\n";
        private const string TextSignature = "[Zafer Bilgisayar by Auto-System]";
        private const string HtmlSeparator = "<hr data-auto-translate=\"true\"/>";
        private const string HtmlSignature = "<br/><br/>[Zafer Bilgisayar by Auto-System]";
        private const string GeminiModelSettingKey = "GeminiModel";
        private const int MaxCharacterLimit = 8000;
        private const int MinEmailThreadPosition = 50;
        private const string RegistryPath = @"Software\ZaferBilgisayar\GeminiTranslate";
        private const string RegistryKeyName = "EncryptedApiKey";
        private static volatile string CachedModelName;
        private static System.Threading.CancellationTokenSource GlobalCancellationSource;
        private static readonly object CancellationLock = new object();
        
        private static readonly byte[] Entropy = new byte[] 
        { 
            0x5A, 0x61, 0x66, 0x65, 0x72, 0x42, 0x69, 0x6C, 
            0x67, 0x69, 0x73, 0x61, 0x79, 0x61, 0x72, 0x32 
        };
        private static readonly Regex SegmentRegex = new Regex(@"\[\[SEG(?<id>\d+)\]\](?<text>.*?)\[\[/SEG\k<id>\]\]", RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex ParentheticalRegex = new Regex(@"\s*[\(\[][^\)\]]*[\)\]]", RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex EmailThreadRegex = new Regex(@"(^|\n)(From:|Sent:|To:|Subject:|-----Original Message-----)", RegexOptions.Multiline | RegexOptions.Compiled);
        private static readonly Regex SignatureRegex = new Regex(@"\[([A-Z]{2,10}|[A-Za-z0-9_\-]{2,20})\]", RegexOptions.Compiled);
        private static readonly Regex AbbreviationRegex = new Regex(@"\b[A-Z]{2,5}\b", RegexOptions.Compiled);
        private static readonly HashSet<string> ProfanityList = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "fuck", "shit", "damn", "bitch", "ass", "hell", "bastard", "crap"
        };
        private static readonly HashSet<string> NonTranslatableParents = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "script",
            "style",
            "head",
            "title",
            "meta",
            "link"
        };


        private void RibbonCeviri_Load(object sender, RibbonUIEventArgs e)
        {
            try
            {
                btnTranslate.Image = RibbonIconHelper.CreateTrToEnIcon();
                btnTranslateToTurkish.Image = RibbonIconHelper.CreateEnToTrIcon();
                btnCancelTranslation.Image = RibbonIconHelper.CreateStopIcon();
                btnCancelTranslation.Label = "Durdur";
                btnInfo.Image = RibbonIconHelper.CreateInfoIcon();
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"[RibbonCeviri_Load] İkon oluşturma hatası: {ex.Message}");
            }

            UpdateApiKeyStatus();
        }

        #region Hakkında

        private void btnInfo_Click(object sender, RibbonControlEventArgs e)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;

            string info =
                "Gemini Outlook Translate Add-in\n" +
                "─────────────────────────────\n\n" +
                $"Versiyon: {version.Major}.{version.Minor}.{version.Build}\n" +
                "Geliştirici: Zafer Bilgisayar\n" +
                "E-posta: hzkucuk@hotmail.com\n" +
                "GitHub: github.com/hzkucuk/GeminiOutlookTranslateAdd-in\n\n" +
                "Google Gemini AI destekli Outlook e-posta çeviri eklentisi.\n" +
                "Türkçe ↔ İngilizce otomatik çeviri, imla düzeltme ve\n" +
                "noktalama ekleme özellikleri sunar.\n\n" +
                "© 2026 Zafer Bilgisayar - Tüm hakları saklıdır.";

            System.Windows.Forms.MessageBox.Show(
                info,
                "Hakkında - Gemini Translate",
                System.Windows.Forms.MessageBoxButtons.OK,
                System.Windows.Forms.MessageBoxIcon.Information);
        }

        #endregion

        #region API Key Yönetimi

        private void btnSaveApiKey_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                string apiKey = txtApiKey.Text?.Trim();
                
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    System.Windows.Forms.MessageBox.Show(
                        "Lütfen geçerli bir API Key giriniz.",
                        "Uyarı",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Warning);
                    return;
                }

                SaveEncryptedApiKey(apiKey);
                
                txtApiKey.Text = string.Empty;
                
                UpdateApiKeyStatus();
                
                System.Windows.Forms.MessageBox.Show(
                    "API Key başarıyla kaydedildi ve şifrelendi.",
                    "Başarılı",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Information);
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"API Key kaydedilirken hata oluştu:\n\n{ex.Message}",
                    "Hata",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        private void UpdateApiKeyStatus()
        {
            try
            {
                string apiKey = LoadEncryptedApiKey();
                
                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    lblApiKeyStatus.Label = "✓ API Key kayıtlı";
                }
                else
                {
                    lblApiKeyStatus.Label = "API Key kayıtlı değil";
                }
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"[UpdateApiKeyStatus] Hata: {ex.Message}");
                lblApiKeyStatus.Label = "Durum belirlenemedi";
            }
        }

        private void SaveEncryptedApiKey(string apiKey)
        {
            try
            {
                byte[] plainTextBytes = Encoding.UTF8.GetBytes(apiKey);
                byte[] encryptedData;
                byte[] iv;
                
                // Machine-specific key kullanarak şifreleme (entropy ile güçlendirilmiş)
                using (Aes aes = Aes.Create())
                {
                    aes.Key = DeriveKeyFromMachineEntropy();
                    aes.GenerateIV();
                    iv = aes.IV;
                    
                    using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                    using (var ms = new System.IO.MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            cs.Write(plainTextBytes, 0, plainTextBytes.Length);
                        }
                        encryptedData = ms.ToArray();
                    }
                }
                
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryPath))
                {
                    if (key == null)
                    {
                        throw new InvalidOperationException("Registry anahtarı oluşturulamadı.");
                    }
                    
                    key.SetValue(RegistryKeyName, Convert.ToBase64String(encryptedData), RegistryValueKind.String);
                    key.SetValue("IV", Convert.ToBase64String(iv), RegistryValueKind.String);
                    Debug.WriteLine("[SaveEncryptedApiKey] API Key güvenli şekilde şifrelendi ve kaydedildi");
                }
            }
            catch (CryptographicException ex)
            {
                Debug.WriteLine($"[SaveEncryptedApiKey] Şifreleme hatası: {ex.Message}");
                throw new InvalidOperationException("API Key şifrelenirken hata oluştu.", ex);
            }
        }

        private string LoadEncryptedApiKey()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryPath))
                {
                    if (key == null)
                    {
                        Debug.WriteLine("[LoadEncryptedApiKey] Registry anahtarı bulunamadı");
                        return null;
                    }
                    
                    string encryptedBase64 = key.GetValue(RegistryKeyName) as string;
                    string ivBase64 = key.GetValue("IV") as string;
                    
                    if (string.IsNullOrWhiteSpace(encryptedBase64) || string.IsNullOrWhiteSpace(ivBase64))
                    {
                        Debug.WriteLine("[LoadEncryptedApiKey] Şifreli API Key veya IV bulunamadı");
                        return null;
                    }
                    
                    byte[] encryptedData = Convert.FromBase64String(encryptedBase64);
                    byte[] iv = Convert.FromBase64String(ivBase64);
                    
                    // Machine-specific key ile şifre çözme
                    using (Aes aes = Aes.Create())
                    {
                        aes.Key = DeriveKeyFromMachineEntropy();
                        aes.IV = iv;
                        
                        using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                        using (var ms = new System.IO.MemoryStream(encryptedData))
                        using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            byte[] decryptedData = new byte[encryptedData.Length];
                            int bytesRead = cs.Read(decryptedData, 0, decryptedData.Length);
                            string apiKey = Encoding.UTF8.GetString(decryptedData, 0, bytesRead);
                            Debug.WriteLine($"[LoadEncryptedApiKey] API Key başarıyla yüklendi: {MaskApiKey(apiKey)}");
                            return apiKey;
                        }
                    }
                }
            }
            catch (CryptographicException ex)
            {
                Debug.WriteLine($"[LoadEncryptedApiKey] Şifre çözme hatası: {ex.Message}");
                return null;
            }
            catch (FormatException ex)
            {
                Debug.WriteLine($"[LoadEncryptedApiKey] Format hatası: {ex.Message}");
                return null;
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"[LoadEncryptedApiKey] Beklenmeyen hata: {ex.Message}");
                return null;
            }
        }

        private string MaskApiKey(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return "[EMPTY]";
            }
            
            if (apiKey.Length <= 8)
            {
                return "****";
            }
            
            return $"{apiKey.Substring(0, 4)}...{apiKey.Substring(apiKey.Length - 4)}";
        }

        /// <summary>
        /// Machine ve user bazlı entropy ile AES key türetir.
        /// Hardcoded password yerine makine kimliği kullanılır (daha güvenli).
        /// </summary>
        private byte[] DeriveKeyFromMachineEntropy()
        {
            // Machine name + User name + Static entropy ile unique key oluştur
            string machineId = Environment.MachineName;
            string userName = Environment.UserName;
            byte[] entropyData = Encoding.UTF8.GetBytes(machineId + userName);
            
            // Entropy ile birleştir
            byte[] combined = new byte[Entropy.Length + entropyData.Length];
            Buffer.BlockCopy(Entropy, 0, combined, 0, Entropy.Length);
            Buffer.BlockCopy(entropyData, 0, combined, Entropy.Length, entropyData.Length);
            
            // SHA256 ile 32-byte key türet
            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(combined);
            }
        }

        #endregion

        /// <summary>
        /// Sadece Türkçe imla düzeltmesi yapar (noktalama + yazım hatası).
        /// Çeviri yapmaz, sadece düzeltir.
        /// </summary>
        private async Task<string> FixTurkishSpellingAsync(string text)
        {
            Debug.WriteLine("[FixTurkishSpellingAsync] Başladı");
            
            string instruction = 
                "Görev: Aşağıdaki Türkçe metindeki yazım hatalarını düzelt ve noktalama işaretlerini ekle.\n" +
                "\n" +
                "Kurallar:\n" +
                "1. Soru cümlelerine soru işareti (?) ekle (örnek: nasılsınız → Nasılsınız?)\n" +
                "2. Normal cümlelerin sonuna nokta (.) ekle (örnek: merhaba → Merhaba.)\n" +
                "3. Ünlem cümlelerine ünlem işareti (!) ekle\n" +
                "4. Gerekli yerlere virgül (,) ekle\n" +
                "5. Cümle başlarını büyük harfle yaz\n" +
                "6. Yazım hatalarını düzelt\n" +
                "\n" +
                "ÖNEMLİ: \n" +
                "- SADECE düzeltilmiş Türkçe metni döndür\n" +
                "- Çeviri YAPMA\n" +
                "- Açıklama ekleme\n" +
                "- [[SEG]] işaretlerini aynen koru\n" +
                "- [Parantez] içini değiştirme\n" +
                "- Kısaltmaları (CEO, API, HZK) değiştirme\n" +
                "- {{PLACEHOLDER_0}} gibi yerleri olduğu gibi bırak";

            string response = await SendTranslationRequestAsync(text, instruction);
            
            if (response.StartsWith("HATA"))
            {
                Debug.WriteLine($"[FixTurkishSpellingAsync] Hata: {response}");
                return response;
            }
            
            Debug.WriteLine($"[FixTurkishSpellingAsync] Tamamlandı, sonuç uzunluğu: {response.Length}");
            return response;
        }

        private bool IsTranslatableTextNode(HtmlNode node)
        {
            if (node == null || node.NodeType != HtmlNodeType.Text)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(node.InnerText))
            {
                return false;
            }

            var parent = node.ParentNode;
            if (parent == null)
            {
                return false;
            }

            return !NonTranslatableParents.Contains(parent.Name);
        }

        private async Task<string> TranslateHtmlBodyAsync(string html, string sourceLanguage, string targetLanguage)
        {
            Debug.WriteLine("[TranslateHtmlBodyAsync] Başladı");
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var textNodes = doc.DocumentNode
                .Descendants()
                .Where(IsTranslatableTextNode)
                .ToList();

            Debug.WriteLine($"[TranslateHtmlBodyAsync] Text node sayısı: {textNodes.Count}");

            if (textNodes.Count == 0)
            {
                Debug.WriteLine("[TranslateHtmlBodyAsync] Hiç text node yok, orijinal HTML döndürülüyor");
                return html;
            }

            // Toplam karakter sayısını hesapla
            int totalChars = textNodes.Sum(n => n.InnerText?.Length ?? 0);
            Debug.WriteLine($"[TranslateHtmlBodyAsync] Toplam karakter: {totalChars}");

            // Karakter limiti kontrolü
            if (totalChars > MaxCharacterLimit)
            {
                Debug.WriteLine($"[TranslateHtmlBodyAsync] Karakter limiti aşıldı: {totalChars} > {MaxCharacterLimit}");
                var dialogResult = System.Windows.Forms.MessageBox.Show(
                    $"Çevrilecek metin çok uzun ({totalChars} karakter).\n" +
                    $"İlk {MaxCharacterLimit} karakterlik kısım çevrilecek.\n\nDevam edilsin mi?",
                    "Uyarı",
                    System.Windows.Forms.MessageBoxButtons.YesNo,
                    System.Windows.Forms.MessageBoxIcon.Warning);
                
                if (dialogResult == System.Windows.Forms.DialogResult.No)
                {
                    Debug.WriteLine("[TranslateHtmlBodyAsync] Kullanıcı iptal etti");
                    return "HATA: Çeviri iptal edildi.";
                }

                // Sadece limiti aşmayan node'ları al
                int currentTotal = 0;
                var limitedNodes = new List<HtmlNode>();
                foreach (var node in textNodes)
                {
                    int nodeLength = node.InnerText?.Length ?? 0;
                    if (currentTotal + nodeLength <= MaxCharacterLimit)
                    {
                        limitedNodes.Add(node);
                        currentTotal += nodeLength;
                    }
                    else
                    {
                        break;
                    }
                }
                textNodes = limitedNodes;
                Debug.WriteLine($"[TranslateHtmlBodyAsync] Sınırlandırılmış node sayısı: {textNodes.Count}, toplam: {currentTotal}");
            }

            var builder = new StringBuilder();
            for (int i = 0; i < textNodes.Count; i++)
            {
                builder.AppendLine($"[[SEG{i}]]");
                builder.AppendLine(textNodes[i].InnerText);
                builder.AppendLine($"[[/SEG{i}]]");
            }

            string textToProcess = builder.ToString();

            Debug.WriteLine($"[TranslateHtmlBodyAsync] TEK API ile düzelt + çevir yapılıyor...");

            // Türkçeden çeviriyorsak imla düzeltme + çeviri (TEK API ÇAĞRISI)
            bool fixSpelling = sourceLanguage.Equals("Turkish", StringComparison.OrdinalIgnoreCase);
            
            string instruction = fixSpelling
                ? "Görev: Aşağıdaki Türkçe metinlerdeki yazım hatalarını düzelt, noktalama işaretlerini ekle, sonra İngilizceye çevir.\n" +
                  "\n" +
                  "Adımlar:\n" +
                  "1. Türkçe yazım hatalarını düzelt\n" +
                  "2. Soru cümlelerine '?' ekle (örnek: nasılsın → Nasılsın?)\n" +
                  "3. Normal cümlelere '.' ekle\n" +
                  "4. Büyük harfle başlat\n" +
                  "5. Düzeltilmiş Türkçe metni İngilizceye çevir\n" +
                  "\n" +
                  "ÖNEMLİ: SADECE İngilizce çeviriyi döndür. Türkçe ekleme. Açıklama ekleme. [[SEG]] işaretlerini koru. [Parantez] ve kısaltmaları çevirme."
                : $"Translate the following {sourceLanguage} text segments to {targetLanguage}. " +
                  $"CRITICAL RULES:\n" +
                  $"1. Return ONLY the {targetLanguage} translation\n" +
                  $"2. NEVER include the original {sourceLanguage} text\n" +
                  $"3. NEVER include both languages\n" +
                  $"4. NEVER add explanations or notes\n" +
                  $"5. Keep all segment markers [[SEG]] exactly as-is\n" +
                  $"6. Return ONLY the marked translated segments\n" +
                  $"7. DO NOT translate text inside square brackets [like this]\n" +
                  $"8. DO NOT translate abbreviations (2-5 uppercase letters like HZK, CEO, API)\n" +
                  $"9. DO NOT translate email signatures\n" +
                  $"10. NEVER use profanity or offensive language\n" +
                  $"11. Keep proper nouns and names unchanged";

            string response = await SendTranslationRequestAsync(textToProcess, instruction);
            
            Debug.WriteLine($"[TranslateHtmlBodyAsync] API yanıtı: {(response.StartsWith("HATA") ? response.Substring(0, Math.Min(100, response.Length)) : "OK, uzunluk: " + response.Length)}");
            
            if (response.StartsWith("HATA"))
            {
                return response;
            }

            var matches = SegmentRegex.Matches(response);
            Debug.WriteLine($"[TranslateHtmlBodyAsync] Eşleşen segment sayısı: {matches.Count}");
            
            if (matches.Count == 0)
            {
                Debug.WriteLine("[TranslateHtmlBodyAsync] Hiç segment eşleşmedi!");
                return "HATA: Segmentli çeviri yanıtı çözümlenemedi.";
            }

            var translatedSegments = new Dictionary<int, string>();
            foreach (Match match in matches)
            {
                int id;
                if (!int.TryParse(match.Groups["id"].Value, out id))
                {
                    continue;
                }

                translatedSegments[id] = match.Groups["text"].Value.Trim();
            }

            Debug.WriteLine($"[TranslateHtmlBodyAsync] Text node'lara çeviri uygulanıyor...");
            for (int i = 0; i < textNodes.Count; i++)
            {
                string translatedSegment;
                if (translatedSegments.TryGetValue(i, out translatedSegment))
                {
                    textNodes[i].InnerHtml = HtmlEntity.Entitize(CleanTranslatedText(translatedSegment, sourceLanguage, targetLanguage));
                }
            }

            var bodyNode = doc.DocumentNode.SelectSingleNode("//body");
            string result = bodyNode != null ? bodyNode.InnerHtml : doc.DocumentNode.InnerHtml;
            Debug.WriteLine($"[TranslateHtmlBodyAsync] Tamamlandı, sonuç uzunluğu: {result.Length}");
            return result;
        }

        private async Task<string> SendTranslationRequestAsync(string textToTranslate, string instruction)
        {
            Debug.WriteLine("[SendTranslationRequestAsync] Başladı");
            try
            {
                string apiKey = LoadEncryptedApiKey();
                
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    Debug.WriteLine("[SendTranslationRequestAsync] API key bulunamadı");
                    return "HATA: API Key kayıtlı değil. Lütfen Ribbon üzerinden API Key'i kaydedin.";
                }
                
                Debug.WriteLine($"[SendTranslationRequestAsync] API Key yüklendi: {MaskApiKey(apiKey)}");

                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

                string modelName = await ResolveGeminiModelAsync(apiKey);
                Debug.WriteLine($"[SendTranslationRequestAsync] Model: {modelName}");
                
                if (string.IsNullOrWhiteSpace(modelName))
                {
                    Debug.WriteLine("[SendTranslationRequestAsync] Model bulunamadı");
                    return "HATA: API model listesi alınamadı.";
                }

                string url = $"https://generativelanguage.googleapis.com/v1beta/{modelName}:generateContent?key={apiKey}";
                var request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/json";

                var payload = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = instruction + "\n\n" + textToTranslate }
                            }
                        }
                    }
                };

                string jsonPayload = JsonConvert.SerializeObject(payload);
                byte[] byteArray = Encoding.UTF8.GetBytes(jsonPayload);
                request.ContentLength = byteArray.Length;
                
                Debug.WriteLine($"[SendTranslationRequestAsync] Payload uzunluğu: {byteArray.Length} byte");

                using (var dataStream = await request.GetRequestStreamAsync())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }

                Debug.WriteLine("[SendTranslationRequestAsync] Yanıt bekleniyor...");
                
                using (var response = await request.GetResponseAsync())
                using (var streamReader = new System.IO.StreamReader(response.GetResponseStream()))
                {
                    string jsonString = await streamReader.ReadToEndAsync();
                    Debug.WriteLine($"[SendTranslationRequestAsync] Yanıt alındı: {jsonString.Length} karakter");
                    
                    dynamic result = JsonConvert.DeserializeObject(jsonString);

                    if (result.candidates != null && result.candidates.Count > 0)
                    {
                        string translatedText = result.candidates[0].content.parts[0].text;
                        Debug.WriteLine($"[SendTranslationRequestAsync] Çeviri metni uzunluğu: {translatedText.Length}");
                        return TrimCodeFence(translatedText);
                    }

                    Debug.WriteLine("[SendTranslationRequestAsync] Candidates boş");
                    return "HATA: API'den beklenmedik boş yanıt döndü.";
                }
            }
            catch (System.Net.WebException ex)
            {
                Debug.WriteLine($"[SendTranslationRequestAsync] WebException: {ex.Message}");
                if (ex.Response != null)
                {
                    using (var reader = new System.IO.StreamReader(ex.Response.GetResponseStream()))
                    {
                        string errorDetail = await reader.ReadToEndAsync();
                        Debug.WriteLine($"[SendTranslationRequestAsync] Hata detayı: {errorDetail}");
                        return "HATA Detayı: " + errorDetail;
                    }
                }
                return "HATA: " + ex.Message;
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"[SendTranslationRequestAsync] Exception: {ex.Message}");
                return "HATA: " + ex.Message;
            }
        }

        private async void btnTranslate_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                await TranslateCurrentMailAsync("Turkish", "English");
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"btnTranslate_Click içinde kritik hata:\n\n{ex.GetType().Name}\n{ex.Message}\n\nStackTrace:\n{ex.StackTrace}",
                    "Kritik Hata",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        private async void btnTranslateToTurkish_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                await TranslateCurrentMailAsync("English", "Turkish");
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"btnTranslateToTurkish_Click içinde kritik hata:\n\n{ex.GetType().Name}\n{ex.Message}\n\nStackTrace:\n{ex.StackTrace}",
                    "Kritik Hata",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        private void btnCancelTranslation_Click(object sender, RibbonControlEventArgs e)
        {
            lock (CancellationLock)
            {
                if (GlobalCancellationSource != null && !GlobalCancellationSource.IsCancellationRequested)
                {
                    GlobalCancellationSource.Cancel();
                    Debug.WriteLine("[btnCancelTranslation_Click] İptal isteği gönderildi");
                    System.Windows.Forms.MessageBox.Show("Çeviri iptal ediliyor...");
                }
                else
                {
                    Debug.WriteLine("[btnCancelTranslation_Click] Aktif çeviri yok");
                }
            }
        }

        private async Task TranslateCurrentMailAsync(string sourceLanguage, string targetLanguage)
        {
            var localCancellationSource = new System.Threading.CancellationTokenSource();

            lock (CancellationLock)
            {
                GlobalCancellationSource = localCancellationSource;
            }

            try
            {
                Debug.WriteLine("[TranslateCurrentMailAsync] Başladı");
                Inspector inspector = Globals.ThisAddIn.Application.ActiveInspector();

                if (inspector == null)
                {
                    Debug.WriteLine("[TranslateCurrentMailAsync] Inspector null");
                    System.Windows.Forms.MessageBox.Show("Outlook penceresinde bir mail açık değil.", "Hata", 
                        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                    return;
                }

                if (inspector.CurrentItem is MailItem mailItem)
                {
                    bool isHtmlBody = mailItem.BodyFormat == OlBodyFormat.olFormatHTML &&
                                      !string.IsNullOrWhiteSpace(mailItem.HTMLBody);
                    
                    Debug.WriteLine($"[TranslateCurrentMailAsync] isHtmlBody: {isHtmlBody}");
                    
                    string originalBody = isHtmlBody
                        ? ExtractOriginalHtml(mailItem.HTMLBody)
                        : ExtractOriginalText(mailItem.Body);

                    Debug.WriteLine($"[TranslateCurrentMailAsync] originalBody uzunluğu: {originalBody?.Length ?? 0}");

                    if (string.IsNullOrWhiteSpace(originalBody))
                    {
                        Debug.WriteLine("[TranslateCurrentMailAsync] originalBody boş");
                        System.Windows.Forms.MessageBox.Show("Çevrilecek metin bulunamadı.", "Uyarı",
                            System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                        return;
                    }

                    // Not: İmla düzeltmesi artık TranslateTextAsync/TranslateHtmlBodyAsync içinde yapılıyor

                    // Text body için karakter limiti kontrolü (HTML'de TranslateHtmlBodyAsync içinde yapılıyor)
                    if (!isHtmlBody && originalBody.Length > MaxCharacterLimit)
                    {
                        Debug.WriteLine($"[TranslateCurrentMailAsync] Text body karakter limiti aşıldı: {originalBody.Length}");
                        var dialogResult = System.Windows.Forms.MessageBox.Show(
                            $"Metin çok uzun ({originalBody.Length} karakter).\n" +
                            $"İlk {MaxCharacterLimit} karakter çevrilecek.\n\nDevam edilsin mi?",
                            "Uyarı",
                            System.Windows.Forms.MessageBoxButtons.YesNo,
                            System.Windows.Forms.MessageBoxIcon.Warning);
                        
                        if (dialogResult == System.Windows.Forms.DialogResult.No)
                        {
                            Debug.WriteLine("[TranslateCurrentMailAsync] Kullanıcı iptal etti");
                            return;
                        }
                        
                        originalBody = originalBody.Substring(0, MaxCharacterLimit);
                    }

                    string oldSubject = mailItem.Subject;
                    Debug.WriteLine($"[TranslateCurrentMailAsync] Old Subject: {oldSubject}");
                    mailItem.Subject = oldSubject + " (Çevriliyor...)";

                    localCancellationSource.Token.ThrowIfCancellationRequested();

                    // HTML formatını koruyarak çevir
                    Debug.WriteLine($"[TranslateCurrentMailAsync] Çeviri başlatılıyor: {sourceLanguage} -> {targetLanguage}");
                    
                    string translatedText = isHtmlBody
                        ? await TranslateHtmlBodyAsync(originalBody, sourceLanguage, targetLanguage)
                        : await TranslateTextAsync(originalBody, sourceLanguage, targetLanguage, false);

                    Debug.WriteLine($"[TranslateCurrentMailAsync] Çeviri tamamlandı, sonuç uzunluğu: {translatedText?.Length ?? 0}");

                    if (translatedText.StartsWith("HATA"))
                    {
                        Debug.WriteLine($"[TranslateCurrentMailAsync] Çeviri hatası: {translatedText}");
                        System.Windows.Forms.MessageBox.Show("Çeviri başarısız oldu: " + translatedText);
                        mailItem.Subject = oldSubject;
                        return;
                    }

                    localCancellationSource.Token.ThrowIfCancellationRequested();

                    Debug.WriteLine("[TranslateCurrentMailAsync] Mail body güncelleniyor...");
                    
                    // HTML formatını koru, Text'te basit separator kullan
                    if (isHtmlBody)
                    {
                        Debug.WriteLine("[TranslateCurrentMailAsync] BuildTranslatedHtmlBody çağrılıyor...");
                        string newHtmlBody = BuildTranslatedHtmlBody(originalBody, translatedText);
                        Debug.WriteLine($"[TranslateCurrentMailAsync] newHtmlBody uzunluğu: {newHtmlBody.Length}");
                        Debug.WriteLine("[TranslateCurrentMailAsync] mailItem.HTMLBody set ediliyor...");
                        mailItem.HTMLBody = newHtmlBody;
                        Debug.WriteLine("[TranslateCurrentMailAsync] mailItem.HTMLBody başarıyla set edildi");
                    }
                    else
                    {
                        Debug.WriteLine("[TranslateCurrentMailAsync] mailItem.Body set ediliyor...");
                        mailItem.Body = translatedText + TextSeparator + originalBody + "\n\n" + TextSignature;
                        Debug.WriteLine("[TranslateCurrentMailAsync] mailItem.Body başarıyla set edildi");
                    }

                    // Not: Mail otomatik kaydedilmiyor, kullanıcı manuel kaydetmeli
                    Debug.WriteLine("[TranslateCurrentMailAsync] Mail otomatik kaydedilmiyor (kullanıcı manuel kaydetmeli)");

                    string finalSubject = oldSubject;
                    if (!string.IsNullOrWhiteSpace(oldSubject) &&
                        !oldSubject.TrimStart().StartsWith("RE:", StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.WriteLine("[TranslateCurrentMailAsync] Subject çevriliyor...");
                        localCancellationSource.Token.ThrowIfCancellationRequested();
                        
                        // Subject çevirisi (imla düzeltmesi dahil)
                        string translatedSubject = await TranslateTextAsync(oldSubject, sourceLanguage, targetLanguage, false);
                        if (!translatedSubject.StartsWith("HATA"))
                        {
                            finalSubject = translatedSubject;
                            Debug.WriteLine($"[TranslateCurrentMailAsync] Subject çevrildi: {finalSubject}");
                        }
                    }

                    Debug.WriteLine($"[TranslateCurrentMailAsync] Final Subject set ediliyor: {finalSubject}");
                    mailItem.Subject = finalSubject;
                    Debug.WriteLine("[TranslateCurrentMailAsync] Tamamlandı!");
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Seçili öğe bir mail değil.", "Hata",
                        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                }
            }
            catch (System.OperationCanceledException)
            {
                Inspector inspector = Globals.ThisAddIn.Application.ActiveInspector();
                if (inspector != null && inspector.CurrentItem is MailItem mailItem)
                {
                    if (mailItem.Subject.EndsWith(" (Çevriliyor...)"))
                    {
                        mailItem.Subject = mailItem.Subject.Replace(" (Çevriliyor...)", string.Empty);
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"TranslateCurrentMailAsync içinde beklenmeyen hata:\n\n" +
                    $"Tip: {ex.GetType().Name}\n" +
                    $"Mesaj: {ex.Message}\n\n" +
                    $"StackTrace:\n{ex.StackTrace}",
                    "Kritik Hata",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
            }
            finally
            {
                if (localCancellationSource != null)
                {
                    localCancellationSource.Dispose();
                }

                lock (CancellationLock)
                {
                    if (GlobalCancellationSource == localCancellationSource)
                    {
                        GlobalCancellationSource = null;
                    }
                }
            }
        }

        // Gemini AI API istemcisi
        private async Task<string> TranslateTextAsync(string textToTranslate, string sourceLanguage, string targetLanguage)
        {
            return await TranslateTextAsync(textToTranslate, sourceLanguage, targetLanguage, true);
        }

        private async Task<string> TranslateTextAsync(string textToTranslate, string sourceLanguage, string targetLanguage, bool isHtml)
        {
            // İmzaları ve kısaltmaları korumak için maskeleme
            var maskResult = MaskSignaturesAndAbbreviations(textToTranslate);
            string maskedText = maskResult.Item1;
            var placeholders = maskResult.Item2;

            Debug.WriteLine($"[TranslateTextAsync] TEK API ile düzelt + çevir yapılıyor...");

            // Türkçeden çeviriyorsak imla düzeltme + çeviri (TEK API ÇAĞRISI)
            bool fixSpelling = sourceLanguage.Equals("Turkish", StringComparison.OrdinalIgnoreCase);
            
            string instruction = isHtml
                ? (fixSpelling
                    ? "Görev: Türkçe HTML içindeki yazım hatalarını düzelt, noktalama ekle, sonra İngilizceye çevir.\n" +
                      "Adımlar: 1) Yazım düzelt, 2) Soru için '?' ekle, 3) Cümle için '.' ekle, 4) İngilizceye çevir.\n" +
                      "SADECE İngilizce HTML döndür. HTML etiketlerini koru. {{PLACEHOLDER_0}} değiştirme."
                    : $"Translate the following {sourceLanguage} HTML to {targetLanguage}. " +
                      $"CRITICAL: Return ONLY the {targetLanguage} translation. " +
                      $"NEVER include the {sourceLanguage} text. " +
                      $"NEVER include both languages. " +
                      $"Preserve all HTML tags and structure. " +
                      $"Return ONLY valid {targetLanguage} HTML. " +
                      $"DO NOT translate placeholders like {{PLACEHOLDER_0}}.")
                : (fixSpelling
                    ? "Görev: Türkçe metindeki yazım hatalarını düzelt, noktalama işaretlerini ekle, sonra İngilizceye çevir.\n" +
                      "\n" +
                      "Adımlar:\n" +
                      "1. Yazım hatalarını düzelt\n" +
                      "2. Soru cümlelerine soru işareti (?) ekle (örnek: nasılsın → Nasılsın?)\n" +
                      "3. Normal cümlelere nokta (.) ekle (örnek: merhaba → Merhaba.)\n" +
                      "4. Büyük harfle başlat\n" +
                      "5. Düzeltilmiş Türkçe metni İngilizceye çevir\n" +
                      "\n" +
                      "ÖNEMLİ: SADECE İngilizce çeviriyi döndür. Türkçe ekleme. Açıklama ekleme. [Parantez] ve kısaltmaları çevirme. {{PLACEHOLDER_0}} olduğu gibi bırak."
                    : $"Translate the following {sourceLanguage} text to {targetLanguage}. " +
                      $"CRITICAL: Return ONLY the {targetLanguage} translation. " +
                      $"NEVER include the original {sourceLanguage} text. " +
                      $"NEVER include both languages. " +
                      $"NO explanations. NO notes. ONLY the {targetLanguage} translation. " +
                      $"DO NOT translate text inside square brackets [like this]. " +
                      $"DO NOT translate abbreviations (2-5 uppercase letters). " +
                      $"DO NOT translate placeholders like {{PLACEHOLDER_0}}. " +
                      $"NEVER use profanity or offensive language.");

            string translated = await SendTranslationRequestAsync(maskedText, instruction);
            
            // Placeholder'ları geri yerleştir
            translated = RestorePlaceholders(translated, placeholders);
            
            return CleanTranslatedText(translated, sourceLanguage, targetLanguage);
        }

        private async Task<string> ResolveGeminiModelAsync(string apiKey)
        {
            if (!string.IsNullOrWhiteSpace(CachedModelName))
            {
                return CachedModelName;
            }

            string configuredModel = ConfigurationManager.AppSettings[GeminiModelSettingKey];
            if (!string.IsNullOrWhiteSpace(configuredModel))
            {
                CachedModelName = configuredModel.StartsWith("models/", StringComparison.OrdinalIgnoreCase)
                    ? configuredModel
                    : "models/" + configuredModel;
                return CachedModelName;
            }

            string listUrl = $"https://generativelanguage.googleapis.com/v1beta/models?key={apiKey}";
            var request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(listUrl);
            request.Method = "GET";

            try
            {
                using (var response = await request.GetResponseAsync())
                using (var streamReader = new System.IO.StreamReader(response.GetResponseStream()))
                {
                    string jsonString = await streamReader.ReadToEndAsync();
                    dynamic result = JsonConvert.DeserializeObject(jsonString);

                    if (result?.models == null)
                    {
                        return null;
                    }

                    foreach (var model in result.models)
                    {
                        if (model.supportedGenerationMethods == null)
                        {
                            continue;
                        }

                        foreach (var method in model.supportedGenerationMethods)
                        {
                            if (string.Equals((string)method, "generateContent", StringComparison.OrdinalIgnoreCase))
                            {
                                string name = model.name;
                                if (string.IsNullOrWhiteSpace(name))
                                {
                                    continue;
                                }

                                CachedModelName = name.StartsWith("models/", StringComparison.OrdinalIgnoreCase)
                                    ? name
                                    : "models/" + name;
                                return CachedModelName;
                            }
                        }
                    }

                    return null;
                }
            }
            catch (System.Net.WebException)
            {
                return null;
            }
        }

        private string TrimCodeFence(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            string trimmed = text.Trim();
            if (!trimmed.StartsWith("```", StringComparison.Ordinal))
            {
                return trimmed;
            }

            int firstNewline = trimmed.IndexOf('\n');
            if (firstNewline >= 0)
            {
                trimmed = trimmed.Substring(firstNewline + 1);
            }

            int lastFence = trimmed.LastIndexOf("```", StringComparison.Ordinal);
            if (lastFence >= 0)
            {
                trimmed = trimmed.Substring(0, lastFence);
            }

            return trimmed.Trim();
        }

        private string CleanTranslatedText(string text, string sourceLanguage, string targetLanguage)
        {
            if (string.IsNullOrWhiteSpace(text) || text.StartsWith("HATA"))
            {
                return text;
            }

            string cleaned = text.Trim();

            // Türkçe karakterler
            var turkishChars = new[] { 'ç', 'ğ', 'ı', 'ö', 'ş', 'ü', 'İ', 'Ğ', 'Ö', 'Ş', 'Ü', 'Ç' };

            // 0. Küfür ve uygunsuz içerik filtrele
            cleaned = FilterProfanity(cleaned);

            // 1. Parantez içindeki Türkçe metni temizle: "How are you? (Nasılsınız?)" → "How are you?"
            if (sourceLanguage.Equals("Turkish", StringComparison.OrdinalIgnoreCase))
            {
                // Parantez içinde Türkçe karakter varsa tüm parantezi kaldır
                var parenthesesRegex = new Regex(@"\s*[\(\[][^\)\]]*[çğışüöÇĞİŞÜÖ][^\)\]]*[\)\]]", RegexOptions.IgnoreCase);
                cleaned = parenthesesRegex.Replace(cleaned, string.Empty).Trim();
            }
            else if (targetLanguage.Equals("Turkish", StringComparison.OrdinalIgnoreCase))
            {
                // İngilizceden Türkçeye çeviriyorsak, İngilizce parantezleri temizle
                var parenthesesRegex = new Regex(@"\s*[\(\[][^\)\]]*[a-zA-Z]{3,}[^\)\]]*[\)\]]", RegexOptions.IgnoreCase);
                cleaned = parenthesesRegex.Replace(cleaned, string.Empty).Trim();
            }

            // 2. "Türkçe metin + İngilizce metin" durumunu temizle
            if (sourceLanguage.Equals("Turkish", StringComparison.OrdinalIgnoreCase) && 
                targetLanguage.Equals("English", StringComparison.OrdinalIgnoreCase))
            {
                // "Nasılsınız? How are you?" gibi durumlarda sadece İngilizce kısmı al
                // ÖNEMLİ: Satır satır kontrol et, noktalama işaretlerini KORUYARAK
                var sentences = new System.Collections.Generic.List<string>();
                var currentSentence = new StringBuilder();
                
                for (int i = 0; i < cleaned.Length; i++)
                {
                    char c = cleaned[i];
                    currentSentence.Append(c);
                    
                    // Cümle sonu noktalama işaretleri
                    if (c == '.' || c == '?' || c == '!')
                    {
                        string sentence = currentSentence.ToString().Trim();
                        
                        // Türkçe karakter içermiyorsa al (noktalama ile birlikte)
                        int turkishCharCount = sentence.Count(ch => turkishChars.Contains(ch));
                        if (turkishCharCount == 0 || (turkishCharCount < 3 && sentence.Length > 10))
                        {
                            sentences.Add(sentence);
                        }
                        
                        currentSentence.Clear();
                    }
                }
                
                // Son kalan parça
                if (currentSentence.Length > 0)
                {
                    string sentence = currentSentence.ToString().Trim();
                    int turkishCharCount = sentence.Count(ch => turkishChars.Contains(ch));
                    if (turkishCharCount == 0 || (turkishCharCount < 3 && sentence.Length > 10))
                    {
                        sentences.Add(sentence);
                    }
                }
                
                if (sentences.Count > 0)
                {
                    // Cümleleri birleştir, zaten noktalama var
                    cleaned = string.Join(" ", sentences).Trim();
                }
            }

            // 3. Fazla boşlukları temizle
            cleaned = Regex.Replace(cleaned, @"\s+", " ").Trim();

            return cleaned;
        }

        private string BuildTranslatedHtmlBody(string originalHtml, string translatedHtml)
        {
            Debug.WriteLine($"[BuildTranslatedHtmlBody] originalHtml uzunluğu: {originalHtml.Length}, translatedHtml uzunluğu: {translatedHtml.Length}");
            
            string translationBlock = $"<div>{translatedHtml}</div>{HtmlSeparator}";
            int bodyStart = originalHtml.IndexOf("<body", StringComparison.OrdinalIgnoreCase);
            
            Debug.WriteLine($"[BuildTranslatedHtmlBody] <body> tag pozisyonu: {bodyStart}");
            
            if (bodyStart >= 0)
            {
                int bodyTagEnd = originalHtml.IndexOf('>', bodyStart);
                if (bodyTagEnd >= 0)
                {
                    int bodyClose = originalHtml.IndexOf("</body>", StringComparison.OrdinalIgnoreCase);
                    if (bodyClose >= 0 && bodyClose > bodyTagEnd)
                    {
                        Debug.WriteLine("[BuildTranslatedHtmlBody] Standart yol: <body>...</body> bulundu");
                        string result = originalHtml.Substring(0, bodyTagEnd + 1)
                            + translationBlock
                            + originalHtml.Substring(bodyTagEnd + 1, bodyClose - (bodyTagEnd + 1))
                            + HtmlSignature
                            + originalHtml.Substring(bodyClose);
                        Debug.WriteLine($"[BuildTranslatedHtmlBody] Sonuç uzunluğu: {result.Length}");
                        return result;
                    }

                    Debug.WriteLine("[BuildTranslatedHtmlBody] </body> bulunamadı, <body> sonrasını kullanıyoruz");
                    return originalHtml.Substring(0, bodyTagEnd + 1)
                        + translationBlock
                        + originalHtml.Substring(bodyTagEnd + 1)
                        + HtmlSignature;
                }
            }

            Debug.WriteLine("[BuildTranslatedHtmlBody] <body> tag bulunamadı, başa ekliyoruz");
            return translationBlock + originalHtml + HtmlSignature;
        }

        private string ExtractOriginalText(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                return body;
            }

            Debug.WriteLine($"[ExtractOriginalText] Giriş uzunluğu: {body.Length}");

            // Önce daha önce çevrilmiş içeriği ayır
            int signatureIndex = body.LastIndexOf(TextSignature, StringComparison.OrdinalIgnoreCase);
            if (signatureIndex >= 0)
            {
                body = body.Substring(0, signatureIndex).TrimEnd();
            }

            int separatorIndex = body.IndexOf(TextSeparator, StringComparison.Ordinal);
            if (separatorIndex >= 0)
            {
                body = body.Substring(separatorIndex + TextSeparator.Length);
            }

            // Mail thread'lerini kes (From:, Sent:, -----Original Message----- vb.)
            string result = TrimEmailThread(body);
            Debug.WriteLine($"[ExtractOriginalText] Sonuç uzunluğu: {result.Length}");
            return result;
        }

        private string TrimEmailThread(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            var match = EmailThreadRegex.Match(text);
            if (match.Success && match.Index > MinEmailThreadPosition)
            {
                return text.Substring(0, match.Index).Trim();
            }

            return text.Trim();
        }

        private string ExtractOriginalHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return html;
            }

            Debug.WriteLine($"[ExtractOriginalHtml] Giriş HTML uzunluğu: {html.Length}");

            // Önce daha önce çevrilmiş içeriği ayır
            int signatureIndex = html.LastIndexOf(HtmlSignature, StringComparison.OrdinalIgnoreCase);
            if (signatureIndex >= 0)
            {
                Debug.WriteLine($"[ExtractOriginalHtml] HtmlSignature bulundu, kaldırılıyor");
                html = html.Substring(0, signatureIndex).TrimEnd();
            }

            int separatorIndex = html.IndexOf(HtmlSeparator, StringComparison.OrdinalIgnoreCase);
            if (separatorIndex >= 0)
            {
                Debug.WriteLine($"[ExtractOriginalHtml] HtmlSeparator bulundu, orjinal kısım alınıyor");
                html = html.Substring(separatorIndex + HtmlSeparator.Length);
            }
            else
            {
                int legacySeparatorIndex = html.IndexOf("<hr/>", StringComparison.OrdinalIgnoreCase);
                if (legacySeparatorIndex >= 0)
                {
                    Debug.WriteLine($"[ExtractOriginalHtml] Legacy <hr/> bulundu");
                    html = html.Substring(legacySeparatorIndex + "<hr/>".Length);
                }
            }

            // HTML'i olduğu gibi döndür (formatı koru)
            Debug.WriteLine($"[ExtractOriginalHtml] Sonuç HTML uzunluğu: {html.Length}");
            return html;
        }

        private string ExtractPlainTextFromHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return html;
            }

            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // Script, style, head vb. etiketleri kaldır
                var nodesToRemove = doc.DocumentNode.Descendants()
                    .Where(n => NonTranslatableParents.Contains(n.Name))
                    .ToList();

                foreach (var node in nodesToRemove)
                {
                    node.Remove();
                }

                // Sadece görünür metni al
                var plainText = doc.DocumentNode.InnerText;

                // HTML entity'leri decode et
                plainText = HtmlEntity.DeEntitize(plainText);

                // Fazla boşlukları temizle
                plainText = Regex.Replace(plainText, @"\s+", " ");
                plainText = Regex.Replace(plainText, @"\n\s+\n", "\n\n");

                return plainText.Trim();
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"[ExtractPlainTextFromHtml] HTML parse hatası: {ex.Message}");
                return html;
            }
        }

        /// <summary>
        /// İmzaları, kısaltmaları ve hassas ifadeleri maskeleyerek çeviriye korur.
        /// </summary>
        private Tuple<string, Dictionary<string, string>> MaskSignaturesAndAbbreviations(string text)
        {
            var placeholders = new Dictionary<string, string>();
            int placeholderIndex = 0;

            // [HZK], [CEO] gibi köşeli parantez içindeki ifadeleri maskele
            text = SignatureRegex.Replace(text, match =>
            {
                string placeholder = $"{{{{PLACEHOLDER_{placeholderIndex}}}}}";
                placeholders[placeholder] = match.Value;
                placeholderIndex++;
                return placeholder;
            });

            // Büyük harfli kısaltmaları maskele (CEO, API, HZK vb.)
            text = AbbreviationRegex.Replace(text, match =>
            {
                string placeholder = $"{{{{PLACEHOLDER_{placeholderIndex}}}}}";
                placeholders[placeholder] = match.Value;
                placeholderIndex++;
                return placeholder;
            });

            return Tuple.Create(text, placeholders);
        }

        /// <summary>
        /// Çeviri sonrası placeholder'ları orijinal değerlerle değiştirir.
        /// </summary>
        private string RestorePlaceholders(string text, Dictionary<string, string> placeholders)
        {
            if (placeholders == null || placeholders.Count == 0)
            {
                return text;
            }

            foreach (var kvp in placeholders)
            {
                text = text.Replace(kvp.Key, kvp.Value);
            }

            return text;
        }

        /// <summary>
        /// Küfür ve uygunsuz içeriği filtreler.
        /// </summary>
        private string FilterProfanity(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            // Kelime bazlı küfür kontrolü
            var words = text.Split(new[] { ' ', '\n', '\t', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var word in words)
            {
                if (ProfanityList.Contains(word.Trim()))
                {
                    Debug.WriteLine($"[FilterProfanity] Uygunsuz kelime tespit edildi: {word}");
                    // Küfür bulunursa, kelimeyi [FILTERED] ile değiştir
                    text = Regex.Replace(text, $@"\b{Regex.Escape(word)}\b", "[FILTERED]", RegexOptions.IgnoreCase);
                }
            }

            return text;
        }
    }
}
