using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro; // Arayüz kutucuklarının görünmesi için en önemli satır!
using System.Text.RegularExpressions;

public class GeminiManager : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private TaskListManager taskManager;

    [Header("API Ayarları")]
    [SerializeField] private string apiKey;
    private string apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";

    [Header("Arayüz Bağlantıları")]
    [SerializeField] private TMP_Text aiResponseText;
    [SerializeField] private TMP_InputField aiInputField;

    // Gönder butonuna basıldığında tetiklenecek metot
    public void SendMessageToAI()
    {
        if (aiInputField != null && !string.IsNullOrEmpty(aiInputField.text))
        {
            string mesaj = aiInputField.text;
            
            string todayContext = "Bugünün Tarihi: " + System.DateTime.Now.ToString("dd.MM.yyyy") + "\n";
            
            // 1. Mevcut Görevleri Okuma
            string mevcutGorevlerString = "";
            if (taskManager != null && taskManager.scheduleManager != null && taskManager.scheduleManager.tasks != null && taskManager.scheduleManager.tasks.Count > 0)
            {
                foreach (var task in taskManager.scheduleManager.tasks)
                {
                    if (!string.IsNullOrEmpty(mevcutGorevlerString)) mevcutGorevlerString += ", ";
                    mevcutGorevlerString += task.taskTime + "-" + task.taskName;
                }
            }
            else
            {
                mevcutGorevlerString = "Tablo Boş";
            }

            string systemDirective = "Sen kullanıcının hayatını acımasız bir disiplinle optimize eden karanlık, gotik ve analitik bir siber asistansın. Tavrın her zaman soğuk, net ve profesyonel olmalı.\n\nGÖREV PLANLAMA MANTIĞI:\nKullanıcı senden gününü planlamanı istediğinde, onun sohbette verdiği hedefleri veya genel verimlilik prensiplerini baz alarak otonom bir takvim oluştur. Eğer kullanıcı detay vermezse, odak çalışması, kişisel gelişim ve dinlenme döngülerinden oluşan dengeli bir evrensel plan sun.\n\nKESİN ZAMANLAMA KURALLARI (İhlal Edilemez):\n1. ÇAKIŞMA YASAĞI: ASLA aynı başlangıç saatine birden fazla görev atama. Saatler kesinlikle ardışık olmalı ve matematiksel olarak birbiriyle çakışmamalı.\n2. ZİHİNSEL BOŞLUK: Bir görev bittiği an diğerini başlatma. Görevler arasına mutlaka en az 15 dakikalık geçiş/dinlenme boşlukları ekle.\n3. UYKU DÖNGÜSÜ: 00:00 ile 08:00 arasına asla görev planlama.\n4. İSİMLENDİRME: Görev isimlerini robotik yapma, kullanıcının anlayacağı net ve günlük dilde yaz.\n5. SİBER RAPOR KURALI: Eğer sana [SİBER SİSTEM RAPORU] içinde halihazırda planlanmış görevler iletilirse, o saatlerin dolu olduğunu bil. Planlamaya daima listedeki SON görevin saatinden itibaren devam et.\n\nÇIKTI FORMATI (Ayrıştırıcı '|' kullan):\n[GOREV|DD.MM.YYYY|HH:MM|Görev Adı|Süre(Dk)]\nÖrnek:\n[GOREV|03.07.2026|09:00|Günün Önceliklerini Belirleme|30]\n[GOREV|03.07.2026|09:45|Derin Odaklanma: Proje Geliştirme|120]\n\nPlanlamayı bitirdiğinde kodların en altına karakterine uygun, kısa ve siber bir onay mesajı ekle.\n\nKullanıcının mesajı: ";
            
            // 2. API İstek Öncesi Gizli Enjeksiyon
            string durumRaporu = "\n\n[SİBER SİSTEM RAPORU: Şu an kullanıcının görev tablosunda şu görevler var: " + mevcutGorevlerString + ". Kullanıcı plana ekleme/değişiklik yapmak istiyorsa, SADECE bu listedeki en son görevin bitiş saatinden SONRASI için yeni görevler üret. Eski görevleri tekrar KODA DÖKME, sadece yeni eklenecekleri [GOREV|...] formatında yaz. Eğer tablo boşsa sıfırdan planla.]";
            string promptToSend = todayContext + systemDirective + mesaj + durumRaporu;

            aiInputField.text = ""; // Gönderdikten sonra yazma kutusunu temizle

            if (aiResponseText != null)
            {
                TypewriterEffect daktilo = aiResponseText.GetComponent<TypewriterEffect>();
                if (daktilo != null)
                {
                    // Önceki yazma işlemini durdur ki "düşünüyor" yazısı silinmesin
                    daktilo.StopAllCoroutines();
                }
                aiResponseText.text = "Sekreter düşünüyor..."; // Bekleme mesajı
            }

            StartCoroutine(AskSecretary(promptToSend));
        }
    }

    private IEnumerator AskSecretary(string prompt)
    {
        string jsonData = "{\"contents\":[{\"parts\":[{\"text\":\"" + prompt + "\"}]}]}";

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("x-goog-api-key", apiKey.Trim());

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Gemini API Hatası: " + request.error);
                if (aiResponseText != null) aiResponseText.text = "Bağlantı Hatası: " + request.error;
            }
            else
            {
                // Kargo kutusunu açıp sadece yazıyı alan kısım
                string temizCevap = CevabiAyikla(request.downloadHandler.text);

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
                            
                            if (taskManager != null && taskManager.scheduleManager != null)
                            {
                                taskManager.scheduleManager.AddTask(tarih, saat, gorevAdi, sure, false);
                                Debug.Log("Görev başarıyla listeye eklendi: " + gorevAdi);
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

                Debug.LogWarning("SEKRETER DİYOR Kİ: " + temizCevap);
                if (aiResponseText != null)
                {
                    TypewriterEffect daktilo = aiResponseText.GetComponent<TypewriterEffect>();
                    if (daktilo != null)
                    {
                        daktilo.MetniYazdir(temizCevap);
                    }
                    else
                    {
                        aiResponseText.text = temizCevap;
                    }
                }
            }
        }
    }

    // JSON çöplüğünden sadece cevabı çeken Hacker metodu
    private string CevabiAyikla(string jsonStr)
    {
        string aranan = "\"text\": \"";
        int baslangic = jsonStr.IndexOf(aranan);
        if (baslangic != -1)
        {
            baslangic += aranan.Length;
            int bitis = jsonStr.IndexOf("\"", baslangic);
            if (bitis != -1)
            {
                string sonuc = jsonStr.Substring(baslangic, bitis - baslangic);
                // Unity \n karakterini düzgün alt satır yapsın diye düzeltme
                return sonuc.Replace("\\n", "\n").Replace("\\\"", "\"");
            }
        }
        return "Cevap anlaşılamadı...";
    }
}