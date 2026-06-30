using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text.RegularExpressions;

public class SiberAsistan : MonoBehaviour
{
    [Header("UI Referansları")]
    public ModernAsistanBaglantisi modernUI;
    public LocationManager locManager; // (Bunu Unity Editor'den bağlamayı unutma!)

    [Header("API Ayarları")]
    public string apiKey = ""; 
    private string apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-3.1-flash-lite:generateContent";

    public void ModernArayuzdenMesajAl(string mesaj)
    {
        if (modernUI != null)
        {
            modernUI.EkranaMesajBas(mesaj, true);
        }
        
        string locationContext = "";
        if (locManager != null && locManager.locationReady)
            locationContext = $"[Şu anki konumum: Enlem {locManager.latitude}, Boylam {locManager.longitude}. Aydın.] ";

        string systemDirective = "Sen benim acımasız ve karanlık kişisel asistanımsın. Kısa ve otoriter cevap ver. KULLANICI SENDEN BİR GÖREV OLUŞTURMANI İSTERSE VEYA BİR EYLEMİ ONAYLARSAN, cevabının en sonuna MUTLAKA şu formatta bir görev etiketi ekle: [GOREV:Görev Adı:Süre]. Süre kısmı SADECE DAKİKA CİNSİNDEN TAM SAYI olmalıdır (Örn: 30, 45, 60). Asla metin, boşluk veya soru işareti (?) kullanma. Eğer kullanıcı bir süre belirtmediyse varsayılan olarak 30 yaz. " + locationContext + "Kullanıcı: ";
        
        if (modernUI != null) modernUI.DurumYaziyorYap();
        StartCoroutine(AskSecretary(systemDirective + mesaj));
    }

    IEnumerator AskSecretary(string prompt)
    {
        string cleanKey = apiKey.Trim();
        if (string.IsNullOrEmpty(cleanKey))
        {
            if (modernUI != null) 
            {
                modernUI.EkranaMesajBas("Hata - API Key boş.", false);
                modernUI.DurumCevrimiciYap();
            }
            yield break;
        }

        string jsonData = "{\"contents\":[{\"parts\":[{\"text\":\"" + prompt + "\"}]}]}";

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            // İŞTE BÜTÜN SORUNU ÇÖZEN, SENİN ÇALIŞAN BAĞLANTI YÖNTEMİN
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("x-goog-api-key", cleanKey);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                string hataMetni = "";
                if (request.downloadHandler != null && !string.IsNullOrEmpty(request.downloadHandler.text)) {
                    hataMetni = request.downloadHandler.text;
                } else if (!string.IsNullOrEmpty(request.error)) {
                    hataMetni = request.error;
                }

                Debug.LogError("🚨 GOOGLE API RAW HATA: " + hataMetni);

                if (modernUI != null) {
                    if (hataMetni.Contains("429") || hataMetni.Contains("quota") || hataMetni.Contains("RESOURCE_EXHAUSTED")) {
                        modernUI.EkranaMesajBas("Sistem ağında yoğunluk tespit edildi. Protokollerin soğuması için lütfen 30 saniye bekleyin.", false);
                    } else {
                        modernUI.EkranaMesajBas("Sunucu bağlantısı kurulamadı. Ağ erişimimi kontrol edin.", false);
                    }
                    modernUI.DurumCevrimiciYap();
                }
            }
            else
            {
                string rawResponse = request.downloadHandler.text;
                string temizCevap = CevabiAyikla(rawResponse);

                if (rawResponse.Contains("429") || rawResponse.Contains("RESOURCE_EXHAUSTED") || rawResponse.Contains("quota"))
                {
                    temizCevap = "Sistem ağında yoğunluk tespit edildi. Protokollerin soğuması için lütfen 30 saniye sonra tekrar deneyin.";
                }
                else
                {
                    // Gizli kodları (Görevleri) metin içinden yakala ve sil
                    Match match = Regex.Match(temizCevap, @"\[GOREV:(.*?):(\d+)\]");
                    if (match.Success)
                    {
                        string gorevAdi = match.Groups[1].Value.Trim();
                        string dakika = match.Groups[2].Value;
                        Debug.LogWarning("OTOMATİK GÖREV YAKALANDI: " + gorevAdi + " - " + dakika + " dk");
                        
                        PageNavigator nav = FindFirstObjectByType<PageNavigator>();
                        if (nav != null) {
                            nav.GorevKartiEkle(gorevAdi, dakika, false); // Katı durumu varsa boolean'ı true yap.
                        }

                        // İleride TaskManager'a bağlayacağız, şimdilik ekrandaki yazıdan temizliyoruz
                        temizCevap = temizCevap.Replace(match.Value, "").Trim();
                    }
                }

                if (modernUI != null) 
                {
                    modernUI.EkranaMesajBas(temizCevap, false);
                    modernUI.DurumCevrimiciYap();
                }
            }
        }
    }

    private string CevabiAyikla(string jsonStr)
    {
        try {
            GeminiResponse response = JsonUtility.FromJson<GeminiResponse>(jsonStr);
            if (response != null && response.candidates != null && response.candidates.Length > 0) {
                var content = response.candidates[0].content;
                if (content != null && content.parts != null && content.parts.Length > 0) {
                    return content.parts[0].text.Trim();
                }
            }
        } catch (System.Exception e) {
            Debug.LogError("JSON Parse Hatası: " + e.Message);
        }
        return "Cevap anlaşılamadı...";
    }
}

[System.Serializable]
public class GeminiResponse {
    public GeminiCandidate[] candidates;
}

[System.Serializable]
public class GeminiCandidate {
    public GeminiContent content;
}

[System.Serializable]
public class GeminiContent {
    public GeminiPart[] parts;
}

[System.Serializable]
public class GeminiPart {
    public string text;
}
