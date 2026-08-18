using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text.RegularExpressions;

public class SiberAsistan : MonoBehaviour
{
    [Header("UI Referansları")]
    public ModernAsistanBaglantisi modernUI;
    public LocationManager locManager; // (Bunu Unity Editor'den bağlamayı unutma!)

    private string aktifProjelerMetni = "";

    // Cache'lenmiş referanslar
    private ScheduleManager _cachedScheduleManager;
    private PageNavigator _cachedNavigator;
    private GorevKartiYonetici _cachedGorevKartiYonetici;

    private KullaniciProfili _aktifProfil = new KullaniciProfili();

    void Start()
    {
        _cachedScheduleManager = FindFirstObjectByType<ScheduleManager>();
        _cachedNavigator = FindFirstObjectByType<PageNavigator>();
        _cachedGorevKartiYonetici = FindFirstObjectByType<GorevKartiYonetici>();
        AktifProjeleriOku();
        KullaniciProfiliniYukle();
    }

    // ── Kalıcı Siber Bellek ──────────────────────────────
    public void KullaniciProfiliniYukle()
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, "kullanici_profili.json");
        if (System.IO.File.Exists(path))
        {
            try {
                string json = System.IO.File.ReadAllText(path);
                _aktifProfil = JsonUtility.FromJson<KullaniciProfili>(json);
            } catch { }
        }
        if (_aktifProfil == null) _aktifProfil = new KullaniciProfili();
        if (_aktifProfil.bilgiler == null) _aktifProfil.bilgiler = new System.Collections.Generic.List<string>();
    }

    public void KullaniciProfiliniKaydet()
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, "kullanici_profili.json");
        try {
            string json = JsonUtility.ToJson(_aktifProfil, true);
            System.IO.File.WriteAllText(path + ".tmp", json);
            if (System.IO.File.Exists(path)) System.IO.File.Replace(path + ".tmp", path, path + ".bak");
            else System.IO.File.Move(path + ".tmp", path);
        } catch(System.Exception e) {
            Debug.LogError("Profil kaydetme hatası: " + e.Message);
        }
    }

    /// <summary>
    /// Aktif uzun vadeli hedefleri PlayerPrefs'ten okur.
    /// Bitiş tarihi geçmemiş hedefleri filtreler ve aktifProjelerMetni string'ini günceller.
    /// Hem uygulama açılışında (Start) hem de her planlama isteğinde çağrılır.
    /// </summary>
    public void AktifProjeleriOku()
    {
        aktifProjelerMetni = "";
        string path = System.IO.Path.Combine(Application.persistentDataPath, "goals.json");
        if (System.IO.File.Exists(path))
        {
            string json = "";
            try { json = System.IO.File.ReadAllText(path); } catch { }
            
            if (!string.IsNullOrEmpty(json))
            {
                HedefListesiWrapper wrapper = JsonUtility.FromJson<HedefListesiWrapper>(json);
                if (wrapper != null && wrapper.hedefler != null)
                {
                    foreach (var hedef in wrapper.hedefler)
                    {
                        System.DateTime baslangic;
                        if (System.DateTime.TryParseExact(hedef.baslangicTarihi, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out baslangic))
                        {
                            System.DateTime bitisTarihi = baslangic.AddDays(hedef.kalanGun);
                            // Bitiş tarihi henüz geçmemişse hedef AKTİF
                            if (bitisTarihi.Date >= System.DateTime.Now.Date)
                            {
                                int kalanGun = (int)(bitisTarihi.Date - System.DateTime.Now.Date).TotalDays;
                                if (!string.IsNullOrEmpty(aktifProjelerMetni)) aktifProjelerMetni += " | ";
                                aktifProjelerMetni += $"Hedef: {hedef.hedefAdi}, Kalan Gün: {kalanGun}";
                            }
                        }
                    }
                }
            }
        }
        Debug.Log("[SiberAsistan] Aktif Projeler Güncellendi: " + (string.IsNullOrEmpty(aktifProjelerMetni) ? "YOK" : aktifProjelerMetni));
    }

    [Header("API Ayarları")]
    [SerializeField] private AsistanConfig config;

    public void ModernArayuzdenMesajAl(string mesaj)
    {
        if (modernUI != null)
        {
            modernUI.EkranaMesajBas(mesaj, true);
        }

        // Her planlama isteğinde aktif projeleri tazele
        AktifProjeleriOku();
        
        string locationContext = "";
        if (locManager != null && locManager.locationReady)
            locationContext = $"[Şu anki konumum: Enlem {locManager.latitude}, Boylam {locManager.longitude}.] ";

        string todayContext = "Bugünün Tarihi: " + System.DateTime.Now.ToString("dd.MM.yyyy") + "\n";
        // 1. Mevcut Görevleri Okuma
        string mevcutGorevlerString = "";
        if (_cachedScheduleManager == null) _cachedScheduleManager = FindFirstObjectByType<ScheduleManager>();
        if (_cachedScheduleManager != null && _cachedScheduleManager.tasks != null && _cachedScheduleManager.tasks.Count > 0)
        {
            foreach (var task in _cachedScheduleManager.tasks)
            {
                if (!string.IsNullOrEmpty(mevcutGorevlerString)) mevcutGorevlerString += ", ";
                mevcutGorevlerString += task.taskTime + "-" + task.taskName;
            }
        }
        else
        {
            mevcutGorevlerString = "Tablo Boş";
        }

        string profilMetni = "";
        if (_aktifProfil != null && _aktifProfil.bilgiler != null && _aktifProfil.bilgiler.Count > 0)
        {
            profilMetni = "\n\n[KALICI SİBER BELLEK - KULLANICI PROFİLİ]: Planlama yaparken aşağıdaki kurallara/alışkanlıklara KESİNLİKLE uymak zorundasın:\n- " + string.Join("\n- ", _aktifProfil.bilgiler);
        }

        string systemDirective = "Senin adın Siber Asistan. Kullanıcıdan gelen mesajı yanıtlamadan önce ŞU DURUMLARI DİKKATE AL:\n\nDURUM 1 (GÜNLÜK SOHBET):\nKullanıcı basit bir sohbet mesajı atıyorsa KESİNLİKLE takvim analizi yapma! Sadece kısa ve siber-gotik bir mesaj ver.\n\nDURUM 2 (BİLGİ ÖĞRENME):\nEğer kullanıcı kendi alışkanlıkları, rutinleri veya tercihleriyle ilgili (örn: yemek saati, uyku saati, sevdiği şeyler) kalıcı bir bilgi verirse, bunu KESİNLİKLE 'yeniBilgiler' dizisine ekle.\n\nDURUM 3 (PLANLAMA TALEBİ):\nKullanıcı açıkça planlama istiyorsa arka plandaki SİBER SİSTEM RAPORU'nu ve KALICI SİBER BELLEK'i kullanarak görevleri optimize et.\n\nKESİN ZAMANLAMA KURALLARI:\n1. ÇAKIŞMA YASAĞI: Aynı saate birden fazla görev atama.\n2. ZİHİNSEL BOŞLUK: Görevler arasına geçiş boşlukları ekle.\n3. UYKU DÖNGÜSÜ: Profilde aksi belirtilmedikçe 00:00 ile 08:00 arasına görev planlama.\n\nÇIKTI FORMATI: YANITINI MUTLAKA VE SADECE AŞAĞIDAKİ JSON FORMATINDA VER!\n{\n  \"temizle\": false,\n  \"mesaj\": \"Kullanıcıya gösterilecek kısa, siber gotik yanıt mesajı.\",\n  \"yeniBilgiler\": [],\n  \"gorevler\": [\n    {\n      \"tarih\": \"DD.MM.YYYY\",\n      \"saat\": \"HH:MM\",\n      \"gorevAdi\": \"Örn: Kodlama Çalışması\",\n      \"sure\": 60,\n      \"kategori\": \"Eğitim\",\n      \"notlar\": \"\"\n    }\n  ]\n}\n\n" + locationContext + profilMetni + "\n\nPatron: ";
        
        // 2. API İstek Öncesi Gizli Enjeksiyon
        string suAnkiSaat = System.DateTime.Now.ToString("HH:mm");
        string durumRaporu = $"\n\n[SİBER SİSTEM RAPORU: Şu anki gerçek dünya saati: {suAnkiSaat}. Kullanıcının görev tablosundaki mevcut görevler: {mevcutGorevlerString}. KESİN KURAL: Planlama yaparken KESİNLİKLE {suAnkiSaat} saatinden ÖNCESİNE (geçmişe) görev atama! Eğer tablo boşsa, planlamayı doğrudan şu anki saatten veya sonrasından başlat. Eğer tabloda görevler varsa, en son görevin bitiş saatini referans al.]";
        string apiGidecekMesaj = mesaj + durumRaporu;

        // 3. Aktif Proje Enjeksiyonu (Uzun Vadeli Hedefler)
        string projeKomutu = "";
        if (!string.IsNullOrEmpty(aktifProjelerMetni)) {
            projeKomutu = $"\n\n[SİBER SİSTEM RAPORU - AKTİF PROJELER]: Kullanıcının devam eden uzun vadeli hedefleri var: '{aktifProjelerMetni}'. Bugünü planlarken, mevcut günlük rutinleri koruyarak, GÜNÜN UYGUN BİR BOŞLUĞUNA bu uzun vadeli hedeflerin bugünkü adımını yeni bir JSON gorev objesi olarak ekle.";
        }
        apiGidecekMesaj += projeKomutu;

        if (modernUI != null) modernUI.DurumYaziyorYap();
        StartCoroutine(AskSecretary(todayContext + systemDirective + apiGidecekMesaj));
    }

    IEnumerator AskSecretary(string prompt)
    {
        if (config == null) config = Resources.Load<AsistanConfig>("SiberAsistanConfig");
        
        string cleanKey = config != null ? config.apiKey.Trim() : "";
        string cleanModelName = config != null ? config.modelName.Trim() : "gemini-1.5-pro";

        if (string.IsNullOrEmpty(cleanKey))
        {
            if (modernUI != null) 
            {
                modernUI.EkranaMesajBas("Hata - API Key boş. Lütfen AsistanConfig objesini ayarlayın.", false);
                modernUI.DurumCevrimiciYap();
            }
            yield break;
        }

        GeminiRequest requestObj = new GeminiRequest();
        requestObj.contents = new GeminiContent[1];
        requestObj.contents[0] = new GeminiContent();
        requestObj.contents[0].parts = new GeminiPart[1];
        requestObj.contents[0].parts[0] = new GeminiPart();
        requestObj.contents[0].parts[0].text = prompt;
        requestObj.generationConfig = new GeminiGenerationConfig { responseMimeType = "application/json" };

        string jsonData = JsonUtility.ToJson(requestObj);

        string temizDomain = "https://generativelanguage.googleapis.com";
        string modelEndpoint = $"/v1beta/models/{cleanModelName}:generateContent?key=";
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
                    try
                    {
                        AIPlanlamaSonucu aiSonuc = JsonUtility.FromJson<AIPlanlamaSonucu>(temizCevap);
                        
                        if (aiSonuc.temizle)
                        {
                            if (_cachedScheduleManager == null) _cachedScheduleManager = FindFirstObjectByType<ScheduleManager>();
                            if (_cachedScheduleManager != null) {
                                _cachedScheduleManager.tasks.Clear();
                                _cachedScheduleManager.SaveTasks();
                            }
                            if (_cachedGorevKartiYonetici == null) _cachedGorevKartiYonetici = FindFirstObjectByType<GorevKartiYonetici>();
                            if (_cachedGorevKartiYonetici != null) _cachedGorevKartiYonetici.TumGorevleriTemizle();
                        }

                        if (aiSonuc.gorevler != null && aiSonuc.gorevler.Count > 0)
                        {
                            if (_cachedNavigator == null) _cachedNavigator = FindFirstObjectByType<PageNavigator>();
                            if (_cachedNavigator != null) {
                                foreach(var g in aiSonuc.gorevler)
                                {
                                    _cachedNavigator.GorevKartiEkle(g.tarih, g.saat, g.gorevAdi, g.sure.ToString(), false);
                                }
                            }
                        }

                        temizCevap = string.IsNullOrEmpty(aiSonuc.mesaj) ? "Planlama işlemi tamamlandı." : aiSonuc.mesaj;

                        // Kalıcı Bellek Güncellemesi
                        if (aiSonuc.yeniBilgiler != null && aiSonuc.yeniBilgiler.Count > 0)
                        {
                            bool yeniBilgiEklendi = false;
                            foreach (string bilgi in aiSonuc.yeniBilgiler)
                            {
                                if (!_aktifProfil.bilgiler.Contains(bilgi))
                                {
                                    _aktifProfil.bilgiler.Add(bilgi);
                                    yeniBilgiEklendi = true;
                                }
                            }
                            if (yeniBilgiEklendi)
                            {
                                KullaniciProfiliniKaydet();
                                temizCevap += "\n\n<color=#60A5FA>[SİSTEM]: Yeni alışkanlık/bilgi Siber Belleğe kaydedildi.</color>";
                            }
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("Yapay Zeka JSON ayrıştırma hatası: " + e.Message + "\nRAW JSON: " + temizCevap);
                        temizCevap = "Sistem iletişim hatası. AI beklenen formatta yanıt vermedi.";
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

    public void GizliSorguYap(string prompt, System.Action<string> callback)
    {
        StartCoroutine(GizliSorguCoroutine(prompt, callback));
    }

    IEnumerator GizliSorguCoroutine(string prompt, System.Action<string> callback)
    {
        if (config == null) config = Resources.Load<AsistanConfig>("SiberAsistanConfig");
        
        string cleanKey = config != null ? config.apiKey.Trim() : "";
        string cleanModelName = config != null ? config.modelName.Trim() : "gemini-3.1-flash-lite";

        if (string.IsNullOrEmpty(cleanKey))
        {
            callback?.Invoke("Hata: API Key boş. Lütfen AsistanConfig objesini ayarlayın.");
            yield break;
        }

        GeminiRequest requestObj = new GeminiRequest();
        requestObj.contents = new GeminiContent[1];
        requestObj.contents[0] = new GeminiContent();
        requestObj.contents[0].parts = new GeminiPart[1];
        requestObj.contents[0].parts[0] = new GeminiPart();
        requestObj.contents[0].parts[0].text = prompt;

        string jsonData = JsonUtility.ToJson(requestObj);

        string temizDomain = "https://generativelanguage.googleapis.com";
        string modelEndpoint = $"/v1beta/models/{cleanModelName}:generateContent?key=";
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

// GeminiResponse, GeminiCandidate, GeminiContent, GeminiPart
// artık Models/VeriModelleri.cs dosyasında tanımlıdır.
