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

    public void ModernArayuzdenMesajAl(string mesaj)
    {
        if (modernUI != null)
        {
            modernUI.EkranaMesajBas(mesaj, true);
        }
        
        string locationContext = "";
        if (locManager != null && locManager.locationReady)
            locationContext = $"[Şu anki konumum: Enlem {locManager.latitude}, Boylam {locManager.longitude}. Aydın.] ";

        string todayContext = "Bugünün Tarihi: " + System.DateTime.Now.ToString("dd.MM.yyyy") + "\n";
        // 1. Mevcut Görevleri Okuma
        string mevcutGorevlerString = "";
        ScheduleManager scheduleManager = FindFirstObjectByType<ScheduleManager>();
        if (scheduleManager != null && scheduleManager.tasks != null && scheduleManager.tasks.Count > 0)
        {
            foreach (var task in scheduleManager.tasks)
            {
                if (!string.IsNullOrEmpty(mevcutGorevlerString)) mevcutGorevlerString += ", ";
                mevcutGorevlerString += task.taskTime + "-" + task.taskName;
            }
        }
        else
        {
            mevcutGorevlerString = "Tablo Boş";
        }

        string systemDirective = "Sen kullanıcının hayatını acımasız bir disiplinle optimize eden karanlık, gotik ve analitik bir siber asistansın. Tavrın her zaman soğuk, net ve profesyonel olmalı.\n\nGÖREV PLANLAMA MANTIĞI:\nKullanıcı senden gününü planlamanı istediğinde, onun sohbette verdiği hedefleri veya genel verimlilik prensiplerini baz alarak otonom bir takvim oluştur. Eğer kullanıcı detay vermezse, odak çalışması, kişisel gelişim ve dinlenme döngülerinden oluşan dengeli bir evrensel plan sun.\n\nKESİN ZAMANLAMA KURALLARI (İhlal Edilemez):\n1. ÇAKIŞMA YASAĞI: ASLA aynı başlangıç saatine birden fazla görev atama. Saatler kesinlikle ardışık olmalı ve matematiksel olarak birbiriyle çakışmamalı.\n2. ZİHİNSEL BOŞLUK: Bir görev bittiği an diğerini başlatma. Görevler arasına mutlaka en az 15 dakikalık geçiş/dinlenme boşlukları ekle.\n3. UYKU DÖNGÜSÜ: 00:00 ile 08:00 arasına asla görev planlama.\n4. İSİMLENDİRME: Görev isimlerini robotik yapma, kullanıcının anlayacağı net ve günlük dilde yaz.\n5. SİBER RAPOR KURALI: Eğer sana [SİBER SİSTEM RAPORU] içinde halihazırda planlanmış görevler iletilirse, o saatlerin dolu olduğunu bil. Planlamaya daima listedeki SON görevin saatinden itibaren devam et.\n\nÇIKTI FORMATI (Ayrıştırıcı '|' kullan):\n[GOREV|DD.MM.YYYY|HH:MM|Görev Adı|Süre(Dk)]\nÖrnek:\n[GOREV|03.07.2026|09:00|Günün Önceliklerini Belirleme|30]\n[GOREV|03.07.2026|09:45|Derin Odaklanma: Proje Geliştirme|120]\n\nPlanlamayı bitirdiğinde kodların en altına karakterine uygun, kısa ve siber bir onay mesajı ekle.\n\n" + locationContext + "Patron: ";
        
        // 2. API İstek Öncesi Gizli Enjeksiyon
        string durumRaporu = "\n\n[SİBER SİSTEM RAPORU: Şu an kullanıcının görev tablosunda şu görevler var: " + mevcutGorevlerString + ". Kullanıcı plana ekleme/değişiklik yapmak istiyorsa, SADECE bu listedeki en son görevin bitiş saatinden SONRASI için yeni görevler üret. Eski görevleri tekrar KODA DÖKME, sadece yeni eklenecekleri [GOREV|...] formatında yaz. Eğer tablo boşsa sıfırdan planla.]";
        string apiGidecekMesaj = mesaj + durumRaporu;

        if (modernUI != null) modernUI.DurumYaziyorYap();
        StartCoroutine(AskSecretary(todayContext + systemDirective + apiGidecekMesaj));
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

        string temizDomain = "https://generativelanguage.googleapis.com";
        string modelEndpoint = "/v1beta/models/gemini-3.1-flash-lite:generateContent?key=";
        string apiUrl = temizDomain + modelEndpoint + cleanKey;

        // TEŞHİS İÇİN KONSOL YAZDIRMASI (Bunu kesinlikle ekle):
        UnityEngine.Debug.Log("<color=cyan>[SİBER ASİSTAN AĞ DENETİMİ] Hedef URL: </color>" + apiUrl.Replace(cleanKey, "GİZLİ_API_ANAHTARI"));

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
                        modernUI.EkranaMesajBas("Bağlantı reddedildi. Sistem şu an aşırı yük altında (Rate Limit).", false);
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
                    string cleanChatText = "";
                    string[] lines = temizCevap.Split('\n'); 

                    foreach (string line in lines) {
                        string trimmedLine = line.Trim();
                        if (trimmedLine.StartsWith("[GOREV")) {
                            // Köşeli parantezleri temizle ve '|' ile böl
                            string data = trimmedLine.Replace("[", "").Replace("]", "");
                            string[] parts = data.Split('|');
                            
                            if (parts.Length >= 5) {
                                string tarih = parts[1].Trim();
                                string saat = parts[2].Trim();
                                string gorevAdi = parts[3].Trim();
                                int sure = 0;
                                int.TryParse(parts[4].Trim(), out sure);
                                
                                PageNavigator nav = FindFirstObjectByType<PageNavigator>();
                                if (nav != null) {
                                    nav.GorevKartiEkle(tarih, saat, gorevAdi, sure.ToString(), false);
                                }
                            }
                        } else if (!string.IsNullOrEmpty(trimmedLine)) {
                            // [GOREV olmayan normal metinleri sohbet balonunda göstermek için sakla
                            cleanChatText += trimmedLine + "\n";
                        }
                    }

                    cleanChatText = cleanChatText.Trim();
                    // Eğer yapay zeka sadece kod gönderdiyse, sohbet ekranı boş kalmasın diye şık bir mesaj ekle
                    if (string.IsNullOrEmpty(cleanChatText)) {
                        cleanChatText = "Planlaman jilet gibi hazır Patron. Görevler sekmesinden takvimi kontrol edebilirsin.";
                    }
                    temizCevap = cleanChatText;
                }

                if (modernUI != null) 
                {
                    modernUI.EkranaMesajBas(temizCevap, false);
                    modernUI.DurumCevrimiciYap();
                }
            }
        }
    }

    public void GizliSorguYap(string prompt, System.Action<string> callback)
    {
        StartCoroutine(GizliSorguCoroutine(prompt, callback));
    }

    IEnumerator GizliSorguCoroutine(string prompt, System.Action<string> callback)
    {
        string cleanKey = apiKey.Trim();
        if (string.IsNullOrEmpty(cleanKey))
        {
            callback?.Invoke("Hata: API Key boş.");
            yield break;
        }

        string jsonData = "{\"contents\":[{\"parts\":[{\"text\":\"" + prompt + "\"}]}]}";

        string temizDomain = "https://generativelanguage.googleapis.com";
        string modelEndpoint = "/v1beta/models/gemini-3.1-flash-lite:generateContent?key=";
        string apiUrl = temizDomain + modelEndpoint + cleanKey;

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("x-goog-api-key", cleanKey);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                callback?.Invoke("Hata: " + request.error);
            }
            else
            {
                string rawResponse = request.downloadHandler.text;
                string temizCevap = CevabiAyikla(rawResponse);
                callback?.Invoke(temizCevap);
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
