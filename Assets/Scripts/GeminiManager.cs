using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro; // Arayüz kutucuklarının görünmesi için en önemli satır!

public class GeminiManager : MonoBehaviour
{
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
            aiInputField.text = ""; // Gönderdikten sonra yazma kutusunu temizle

            if (aiResponseText != null) aiResponseText.text = "Sekreter düşünüyor..."; // Bekleme mesajı

            StartCoroutine(AskSecretary(mesaj));
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

                Debug.LogWarning("SEKRETER DİYOR Kİ: " + temizCevap);
                if (aiResponseText != null) aiResponseText.text = temizCevap;
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