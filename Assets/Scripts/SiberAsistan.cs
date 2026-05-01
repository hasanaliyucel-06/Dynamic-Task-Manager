using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Text.RegularExpressions;

public class SiberAsistan : MonoBehaviour
{
    [Header("UI Referansları")]
    public TMP_InputField inputField;
    public Transform chatContent;
    public Button sendButton;
    public ScrollRect scrollRect;

    [Header("API Ayarları")]
    public string apiKey = ""; 
    private string apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

    void Start()
    {
        if (sendButton != null) sendButton.onClick.AddListener(MesajGonder);
    }

    void MesajGonder()
    {
        if (string.IsNullOrWhiteSpace(inputField.text)) return;
        string kullaniciMesaji = inputField.text;
        inputField.text = ""; 

        // Ekrana kullanıcının mesajını yazdır
        YeniMesaj("Sen: " + kullaniciMesaji, Color.white, TextAlignmentOptions.TopRight);
        
        // Senin harika direktifin ve görev motorun
        string systemDirective = "Sen benim acımasız ve karanlık kişisel asistanımsın. Bana daima gotik, otoriter ve havalı bir tonda kısa cevaplar ver. EĞER senden yeni bir görev/iş/plan eklemeni istersem, bana normal cevabını verdikten sonra cümlenin EN SONUNA tam olarak şu formatta gizli bir kod ekle: [GOREV:GörevAdı:Dakika]. Örneğin: [GOREV:Elektro Gitar:60]. Eğer benden bir süre belirtilmediyse varsayılan olarak 30 dakika yaz. Eğer görev eklememi istemiyorsam kod ekleme. Kullanıcının mesajı: ";
        string promptToSend = systemDirective + kullaniciMesaji;

        StartCoroutine(AskSecretary(promptToSend));
    }

    IEnumerator AskSecretary(string prompt)
    {
        Color siberMavi; ColorUtility.TryParseHtmlString("#0044FF", out siberMavi);
        GameObject beklemeMesaji = YeniMesaj("Siber Asistan: Düşünüyor...", siberMavi, TextAlignmentOptions.TopLeft);

        string cleanKey = apiKey.Trim();
        if (string.IsNullOrEmpty(cleanKey))
        {
            beklemeMesaji.GetComponent<TextMeshProUGUI>().text = "Hata: API Key boş. Lütfen Inspector'dan girin.";
            beklemeMesaji.GetComponent<TextMeshProUGUI>().color = Color.red;
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
                string hataDetayi = request.downloadHandler != null ? request.downloadHandler.text : request.error;
                beklemeMesaji.GetComponent<TextMeshProUGUI>().text = "Bağlantı Hatası:\n" + hataDetayi;
                beklemeMesaji.GetComponent<TextMeshProUGUI>().color = Color.red;
            }
            else
            {
                string temizCevap = CevabiAyikla(request.downloadHandler.text);

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

                beklemeMesaji.GetComponent<TextMeshProUGUI>().text = "Siber Asistan: " + temizCevap;
            }
            
            Canvas.ForceUpdateCanvases();
            if(scrollRect != null) scrollRect.verticalNormalizedPosition = 0f;
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

    GameObject YeniMesaj(string metin, Color renk, TextAlignmentOptions hizalama)
    {
        GameObject msgObj = new GameObject("Mesaj");
        msgObj.transform.SetParent(chatContent, false);
        
        TextMeshProUGUI tmp = msgObj.AddComponent<TextMeshProUGUI>();
        tmp.text = metin;
        tmp.fontSize = 24;
        tmp.color = renk;
        tmp.alignment = hizalama;
        tmp.enableWordWrapping = true;
        
        tmp.margin = new Vector4(10, 0, 40, 0); 
        
        LayoutElement le = msgObj.AddComponent<LayoutElement>();
        le.minHeight = 45;
        le.flexibleWidth = 1;

        Canvas.ForceUpdateCanvases();
        if(scrollRect != null) scrollRect.verticalNormalizedPosition = 0f;

        return msgObj;
    }
}
