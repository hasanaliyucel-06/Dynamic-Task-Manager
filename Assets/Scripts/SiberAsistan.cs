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
    private string apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

    public void ModernArayuzdenMesajAl(string mesaj)
    {
        if (modernUI != null)
        {
            modernUI.EkranaMesajBas(mesaj, true);
        }
        
        string locationContext = "";
        if (locManager != null && locManager.locationReady)
            locationContext = $"[Şu anki konumum: Enlem {locManager.latitude}, Boylam {locManager.longitude}. Aydın.] ";

        string systemDirective = "Sen benim acımasız ve karanlık kişisel asistanımsın. Kısa ve otoriter cevap ver. [GOREV:Adı:Dakika] formatını unutma. " + locationContext + "Kullanıcı: ";
        
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
        string aranan = "\"text\": \"";
        int baslangic = jsonStr.IndexOf(aranan);
        if (baslangic != -1)
        {
            baslangic += aranan.Length;
            int bitis = jsonStr.LastIndexOf("\"\n          }");
            if (bitis == -1) bitis = jsonStr.IndexOf("\"", baslangic);

            if (bitis != -1)
            {
                string sonuc = jsonStr.Substring(baslangic, bitis - baslangic);
                return sonuc.Replace("\\n", "\n").Replace("\\\"", "\"").Replace("\\*", "*");
            }
        }
        return "Cevap anlaşılamadı...";
    }
}
