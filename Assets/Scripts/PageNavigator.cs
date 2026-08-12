using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Uygulamanın ana sayfa navigasyonu.
/// Alt yöneticileri (GorevKartiYonetici, TakvimYonetici, SihirbazYonetici) başlatır
/// ve tab butonlarıyla sayfa geçişlerini yönetir.
/// 
/// ScheduleManager ve SiberAsistan geriye dönük uyumluluk için
/// hâlâ bu sınıfı FindFirstObjectByType ile bulup GorevKartiEkle() çağırır.
/// Bu çağrılar proxy metotları üzerinden GorevKartiYonetici'ye yönlendirilir.
/// </summary>
public class PageNavigator : MonoBehaviour
{
    private UIDocument uiDocument;

    // Sayfa referansları
    private VisualElement pageSohbet;
    private VisualElement pageGorevler;
    private VisualElement pageDisiplin;
    private VisualElement pageSistem;

    // Tab buton referansları
    private Button btnSohbet;
    private Button btnGorevler;
    private Button btnDisiplin;
    private Button btnSistem;

    // Alt yöneticiler
    private GorevKartiYonetici gorevKartiYonetici;
    private TakvimYonetici takvimYonetici;
    private SihirbazYonetici sihirbazYonetici;
    private SistemYonetici sistemYonetici;
    private BildirimYonetici bildirimYonetici;

    void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null) return;

        var root = uiDocument.rootVisualElement;

        // Sayfa referansları
        pageSohbet = root.Q<VisualElement>("Page_Sohbet");
        pageGorevler = root.Q<VisualElement>("Page_Gorevler");
        pageDisiplin = root.Q<VisualElement>("pageDisiplin");
        pageSistem = root.Q<VisualElement>("pageSistem");

        // Tab butonları
        btnSohbet = root.Q<Button>("Btn_TabSohbet");
        btnGorevler = root.Q<Button>("Btn_TabGorevler");
        btnDisiplin = root.Q<Button>("btnDisiplin");
        btnSistem = root.Q<Button>("btnSistem");

        // Tab event'leri
        if (btnSohbet != null) btnSohbet.clicked += () => SayfaDegistir(0);
        if (btnGorevler != null) btnGorevler.clicked += () => SayfaDegistir(1);
        if (btnDisiplin != null) btnDisiplin.clicked += () => SayfaDegistir(2);
        if (btnSistem != null) btnSistem.clicked += () => SayfaDegistir(3);

        // Alt yöneticileri bul veya oluştur, sonra başlat
        gorevKartiYonetici = GetComponent<GorevKartiYonetici>();
        if (gorevKartiYonetici == null) gorevKartiYonetici = gameObject.AddComponent<GorevKartiYonetici>();
        gorevKartiYonetici.Initialize(root);

        takvimYonetici = GetComponent<TakvimYonetici>();
        if (takvimYonetici == null) takvimYonetici = gameObject.AddComponent<TakvimYonetici>();
        takvimYonetici.Initialize(root);

        sihirbazYonetici = GetComponent<SihirbazYonetici>();
        if (sihirbazYonetici == null) sihirbazYonetici = gameObject.AddComponent<SihirbazYonetici>();
        sihirbazYonetici.Initialize(root, gorevKartiYonetici, takvimYonetici);

        sistemYonetici = GetComponent<SistemYonetici>();
        if (sistemYonetici == null) sistemYonetici = gameObject.AddComponent<SistemYonetici>();
        sistemYonetici.Initialize(root);

        bildirimYonetici = GetComponent<BildirimYonetici>();
        if (bildirimYonetici == null) bildirimYonetici = gameObject.AddComponent<BildirimYonetici>();
        bildirimYonetici.Initialize(root);

        // İlk sayfa: Sohbet
        SayfaDegistir(0);
    }

    /// <summary>
    /// Belirtilen indeksteki sayfayı gösterir, diğerlerini gizler.
    /// Tab butonlarının aktif/pasif renklerini günceller.
    /// </summary>
    private void SayfaDegistir(int sayfaIndex)
    {
        if (pageSohbet != null) pageSohbet.style.display = (sayfaIndex == 0) ? DisplayStyle.Flex : DisplayStyle.None;
        if (pageGorevler != null) pageGorevler.style.display = (sayfaIndex == 1) ? DisplayStyle.Flex : DisplayStyle.None;
        if (pageDisiplin != null) pageDisiplin.style.display = (sayfaIndex == 2) ? DisplayStyle.Flex : DisplayStyle.None;
        if (pageSistem != null) pageSistem.style.display = (sayfaIndex == 3) ? DisplayStyle.Flex : DisplayStyle.None;

        Color aktifRenk = new Color(0f, 0.5f, 1f);
        Color pasifRenk = new Color(0.6f, 0.6f, 0.6f);

        if (btnSohbet != null)
        {
            btnSohbet.style.unityBackgroundImageTintColor = new StyleColor((sayfaIndex == 0) ? aktifRenk : pasifRenk);
            btnSohbet.style.backgroundColor = new StyleColor(Color.clear);
        }
        if (btnGorevler != null)
        {
            btnGorevler.style.unityBackgroundImageTintColor = new StyleColor((sayfaIndex == 1) ? aktifRenk : pasifRenk);
            btnGorevler.style.backgroundColor = new StyleColor(Color.clear);
        }
        if (btnDisiplin != null)
        {
            btnDisiplin.style.unityBackgroundImageTintColor = new StyleColor((sayfaIndex == 2) ? aktifRenk : pasifRenk);
            btnDisiplin.style.backgroundColor = new StyleColor(Color.clear);
        }
        if (btnSistem != null)
        {
            btnSistem.style.unityBackgroundImageTintColor = new StyleColor((sayfaIndex == 3) ? aktifRenk : pasifRenk);
            btnSistem.style.backgroundColor = new StyleColor(Color.clear);
        }
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // Geriye Dönük Uyumluluk Proxy Metotları
    // ScheduleManager.LoadTasks() ve SiberAsistan.AskSecretary()
    // hâlâ FindFirstObjectByType<PageNavigator>() ile bu metotları çağırıyor.
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    /// <summary>
    /// Proxy: Görev kartı ekleme isteğini GorevKartiYonetici'ye yönlendirir.
    /// </summary>
    public void GorevKartiEkle(string tarih, string saat, string gorevAdi, string sure = "", bool katiMi = false, bool kaydet = true)
    {
        if (gorevKartiYonetici != null)
        {
            gorevKartiYonetici.GorevKartiEkle(tarih, saat, gorevAdi, sure, katiMi, kaydet);
        }
    }

    /// <summary>
    /// Proxy: Aktif görev sayısı güncelleme isteğini GorevKartiYonetici'ye yönlendirir.
    /// </summary>
    public void AktifGorevSayisiniGuncelle()
    {
        if (gorevKartiYonetici != null)
        {
            gorevKartiYonetici.AktifGorevSayisiniGuncelle();
        }
    }
}

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Paylaşılan Veri Modelleri
// SiberAsistan.cs, TakvimYonetici.cs ve SihirbazYonetici.cs
// tarafından kullanılır.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[System.Serializable]
public class UzunVadeliHedef {
    public string hedefAdi;
    public int kalanGun;
    public string baslangicTarihi;
}

[System.Serializable]
public class HedefListesiWrapper {
    public System.Collections.Generic.List<UzunVadeliHedef> hedefler;
}
