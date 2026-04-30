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
    private string apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

    [Header("Arayüz Bağlantıları")]
    [SerializeField] private TMP_Text aiResponseText;
    [SerializeField] private TMP_InputField aiInputField;

    // Gönder butonuna basıldığında tetiklenecek metot
    public void SendMessageToAI()
    {
        if (aiInputField != null && !string.IsNullOrEmpty(aiInputField.text))
        {
            string mesaj = aiInputField.text;
            
            string systemDirective = "Sen benim acımasız ve karanlık kişisel asistanımsın. Bana daima gotik, otoriter ve havalı bir tonda kısa cevaplar ver. EĞER senden yeni bir görev/iş/plan eklemeni istersem, bana normal cevabını verdikten sonra cümlenin EN SONUNA tam olarak şu formatta gizli bir kod ekle: [GOREV:GörevAdı:Dakika]. Örneğin: [GOREV:Elektro Gitar:60]. Eğer benden bir süre belirtilmediyse varsayılan olarak 30 dakika yaz. Eğer görev eklememi istemiyorsam kod ekleme. Kullanıcının mesajı: ";
            string promptToSend = systemDirective + mesaj;

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

                // Regex ile [GOREV:GörevAdı:Dakika] kontrolü
                Match match = Regex.Match(temizCevap, @"\[GOREV:(.*?):(\d+)\]");
                if (match.Success)
                {
                    string gorevAdi = match.Groups[1].Value.Trim();
                    string dakika = match.Groups[2].Value;
                    Debug.LogWarning("OTOMATİK GÖREV YAKALANDI: " + gorevAdi + " - " + dakika + " dk");

                    if (taskManager != null && taskManager.scheduleManager != null)
                    {
                        if (int.TryParse(dakika, out int sure))
                        {
                            taskManager.scheduleManager.AddTask(gorevAdi, sure, false);
                            Debug.Log("Görev başarıyla listeye eklendi!");
                        }
                    }
                    else if (taskManager == null)
                    {
                        Debug.LogWarning("GeminiManager üzerinde TaskListManager referansı eksik!");
                    }

                    // Kodu metinden sil
                    temizCevap = temizCevap.Replace(match.Value, "").Trim();
                }

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