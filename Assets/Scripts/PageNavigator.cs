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

    void Awake()
    {
        // Hata 7 FIX: Mobil cihazlarda varsayılan 30 FPS kısıtını kaldırıp pürüzsüz 60 FPS akıcılık sağla
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }

    void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null) return;

        var root = uiDocument.rootVisualElement;

        // BUG 14 FIX: Safe Area Desteği (Dinamik çentik/notch ve orientation takibi)
        GuncelleSafeArea();

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
        if (btnSohbet != null) btnSohbet.clicked += OnSohbetClicked;
        if (btnGorevler != null) btnGorevler.clicked += OnGorevlerClicked;
        if (btnDisiplin != null) btnDisiplin.clicked += OnDisiplinClicked;
        if (btnSistem != null) btnSistem.clicked += OnSistemClicked;

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

        // BUG 2 FIX: Görevler sayfası klavye desteği için bottom-bar referansı
        if (pageGorevler != null)
        {
            gorevlerInputContainer = pageGorevler.Q<VisualElement>(className: "bottom-bar");
        }

        // BUG 8 FIX: Overlay referansları
        gorevSihirbaziOverlay = root.Q<VisualElement>("gorevSihirbaziOverlay");
        gorevDuzenleOverlay = root.Q<VisualElement>("gorevDuzenleOverlay");

        // Swipe gesture'ları kur
        SwipeKur(root);

        // Faz 2: Onboarding ve Günlük Özet Kontrolleri
        KontrolOnboarding(root);
        KontrolGunlukOzet(root);
    }

    // BUG 11 FIX: Onboarding overlay referansı (named handler erişimi için)
    private VisualElement onboardingOverlay;

    private void KontrolOnboarding(VisualElement root)
    {
        if (PlayerPrefs.GetInt("OnboardingDone", 0) == 0)
        {
            onboardingOverlay = root.Q<VisualElement>("onboardingOverlay");
            if (onboardingOverlay != null) onboardingOverlay.style.display = DisplayStyle.Flex;

            var btnIleri = root.Q<Button>("btnOnboardingIleri");
            if (btnIleri != null)
            {
                btnIleri.clicked -= OnOnboardingIleriClicked;
                btnIleri.clicked += OnOnboardingIleriClicked;
            }
        }
    }

    private void OnOnboardingIleriClicked()
    {
        if (onboardingOverlay != null) onboardingOverlay.style.display = DisplayStyle.None;
        PlayerPrefs.SetInt("OnboardingDone", 1);
        PlayerPrefs.Save();
    }

    // Günlük özet overlay referansı (named handler erişimi için)
    private VisualElement gunlukOzetOverlay;

    private void KontrolGunlukOzet(VisualElement root)
    {
        string bugun = System.DateTime.Now.ToString("dd.MM.yyyy");
        if (System.DateTime.Now.Hour >= 20 && PlayerPrefs.GetString("SonOzetTarihi", "") != bugun)
        {
            gunlukOzetOverlay = root.Q<VisualElement>("gunlukOzetOverlay");
            if (gunlukOzetOverlay != null)
            {
                gunlukOzetOverlay.style.display = DisplayStyle.Flex;
                PlayerPrefs.SetString("SonOzetTarihi", bugun);
                PlayerPrefs.Save();
                
                // Tamamlanan görev sayısını yazdır
                var scheduleManager = FindFirstObjectByType<ScheduleManager>();
                var lblAdet = root.Q<Label>("lblOzetAdet");
                if (scheduleManager != null && lblAdet != null)
                {
                    lblAdet.text = scheduleManager.BugununTamamlananSayisi().ToString();
                }

                var btnKapat = root.Q<Button>("btnOzetKapat");
                if (btnKapat != null)
                {
                    btnKapat.clicked -= OnGunlukOzetKapatClicked;
                    btnKapat.clicked += OnGunlukOzetKapatClicked;
                }
            }
        }
    }

    private void OnGunlukOzetKapatClicked()
    {
        if (gunlukOzetOverlay != null) gunlukOzetOverlay.style.display = DisplayStyle.None;
    }

    void OnApplicationPause(bool pauseStatus)
    {
        // Hata 7 FIX: Uygulama gün içinde arka plandan öne geldiğinde (resume) akşam özeti kontrolü yap
        if (!pauseStatus && uiDocument != null && uiDocument.rootVisualElement != null)
        {
            KontrolGunlukOzet(uiDocument.rootVisualElement);
        }
    }

    void OnDisable()
    {
        // Event sızıntısını önle: tüm listener'ları temizle
        if (btnSohbet != null) btnSohbet.clicked -= OnSohbetClicked;
        if (btnGorevler != null) btnGorevler.clicked -= OnGorevlerClicked;
        if (btnDisiplin != null) btnDisiplin.clicked -= OnDisiplinClicked;
        if (btnSistem != null) btnSistem.clicked -= OnSistemClicked;

        // BUG 12 FIX: Swipe callback temizliği
        if (swipeRoot != null)
        {
            if (swipeDownCallback != null) swipeRoot.UnregisterCallback(swipeDownCallback);
            if (swipeUpCallback != null) swipeRoot.UnregisterCallback(swipeUpCallback);
        }
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // KLAVYE VE SAFE AREA YÖNETİMİ (BUG 2 & BUG 14 FIX)
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    private Rect lastSafeArea;
    private Vector2Int lastScreenSize;
    private VisualElement cachedMainContainer;

    private void GuncelleSafeArea()
    {
        if (cachedMainContainer == null && uiDocument != null && uiDocument.rootVisualElement != null)
        {
            cachedMainContainer = uiDocument.rootVisualElement.Q<VisualElement>(className: "main-container");
        }

        if (cachedMainContainer != null && cachedMainContainer.panel != null)
        {
            Rect safeArea = Screen.safeArea;
            Vector2Int screenSize = new Vector2Int(Screen.width, Screen.height);
            if (safeArea == lastSafeArea && screenSize == lastScreenSize) return;

            lastSafeArea = safeArea;
            lastScreenSize = screenSize;

            var panel = cachedMainContainer.panel;
            Vector2 screenTop = RuntimePanelUtils.ScreenToPanel(panel, new Vector2(0, Screen.height));
            Vector2 safeAreaTop = RuntimePanelUtils.ScreenToPanel(panel, new Vector2(0, safeArea.yMax));
            Vector2 screenBottom = RuntimePanelUtils.ScreenToPanel(panel, new Vector2(0, 0));
            Vector2 safeAreaBottom = RuntimePanelUtils.ScreenToPanel(panel, new Vector2(0, safeArea.yMin));

            float topPadding = Mathf.Abs(screenTop.y - safeAreaTop.y);
            float bottomPadding = Mathf.Abs(screenBottom.y - safeAreaBottom.y);

            cachedMainContainer.style.paddingTop = topPadding > 0 ? topPadding : 0;
            cachedMainContainer.style.paddingBottom = bottomPadding > 0 ? bottomPadding : 0;
        }
    }

    void Update()
    {
        // BUG 14 FIX: Oryantasyon veya çözünürlük değişiminde safe area'yı güncelle
        GuncelleSafeArea();

        bool isKeyboardVisible = TouchScreenKeyboard.visible;

        if (isKeyboardVisible)
        {
            float kbHeight = GetKeyboardHeightPanel();
            if (kbHeight > 0)
            {
                if (gorevlerInputContainer != null) gorevlerInputContainer.style.marginBottom = kbHeight;
                // BUG 8 FIX: Overlay açıkken klavye kadar paddingBottom ekle
                if (gorevSihirbaziOverlay != null && gorevSihirbaziOverlay.resolvedStyle.display == DisplayStyle.Flex)
                    gorevSihirbaziOverlay.style.paddingBottom = kbHeight;
                if (gorevDuzenleOverlay != null && gorevDuzenleOverlay.resolvedStyle.display == DisplayStyle.Flex)
                    gorevDuzenleOverlay.style.paddingBottom = kbHeight;
            }
        }
        else
        {
            if (gorevlerWasKeyboardVisible)
            {
                if (gorevlerInputContainer != null) gorevlerInputContainer.style.marginBottom = 0;
                if (gorevSihirbaziOverlay != null) gorevSihirbaziOverlay.style.paddingBottom = 0;
                if (gorevDuzenleOverlay != null) gorevDuzenleOverlay.style.paddingBottom = 0;
            }
        }

        gorevlerWasKeyboardVisible = isKeyboardVisible;
    }

    private float GetKeyboardHeightPanel()
    {
        float screenKbHeight = TouchScreenKeyboard.area.height;
        if (screenKbHeight <= 0) return 0f;

        if (uiDocument == null) return screenKbHeight;
        var r = uiDocument.rootVisualElement;
        if (r == null || r.panel == null) return screenKbHeight;

        var panel = r.panel;
        Vector2 bottom = RuntimePanelUtils.ScreenToPanel(panel, new Vector2(0, 0));
        Vector2 kbTop = RuntimePanelUtils.ScreenToPanel(panel, new Vector2(0, screenKbHeight));
        return Mathf.Abs(kbTop.y - bottom.y);
    }

    // Named event handler metotları (OnDisable'da temizlenebilir)
    private void OnSohbetClicked() => SayfaDegistir(0);
    private void OnGorevlerClicked() => SayfaDegistir(1);
    private void OnDisiplinClicked() => SayfaDegistir(2);
    private void OnSistemClicked() => SayfaDegistir(3);

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // SWIPE GESTURE SİSTEMİ
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    private int aktifSayfaIndex = 0;
    private const int TOPLAM_SAYFA = 4;
    private Vector2 swipeBaslangic;
    private bool swipeAktif = false;
    private bool swipeInsideScrollView = false;
    private const float SWIPE_ESIK = 80f; // Minimum swipe mesafesi (piksel)

    // Görevler sayfası klavye desteği (BUG 2 FIX)
    private VisualElement gorevlerInputContainer;
    private bool gorevlerWasKeyboardVisible = false;

    // Overlay klavye desteği (BUG 8 FIX)
    private VisualElement gorevSihirbaziOverlay;
    private VisualElement gorevDuzenleOverlay;

    // Swipe cleanup referansları (BUG 12 FIX)
    private EventCallback<PointerDownEvent> swipeDownCallback;
    private EventCallback<PointerUpEvent> swipeUpCallback;
    private VisualElement swipeRoot;

    private void SwipeKur(VisualElement root)
    {
        swipeRoot = root;

        swipeDownCallback = evt => {
            swipeBaslangic = evt.position;
            swipeAktif = true;

            // BUG 3 FIX: ScrollView içinden başlayan swipe'ları işaretle
            swipeInsideScrollView = false;
            VisualElement target = evt.target as VisualElement;
            while (target != null && target != root)
            {
                if (target is ScrollView) { swipeInsideScrollView = true; break; }
                target = target.parent;
            }
        };

        swipeUpCallback = evt => {
            if (!swipeAktif) return;
            swipeAktif = false;

            Vector2 swipeBitis = evt.position;
            float deltaX = swipeBitis.x - swipeBaslangic.x;
            float deltaY = swipeBitis.y - swipeBaslangic.y;

            // BUG 3 FIX: ScrollView içindeyse daha yüksek eşik ve oran uygula
            float esik = swipeInsideScrollView ? Mathf.Max(SWIPE_ESIK, Screen.width * 0.2f) : SWIPE_ESIK;
            float oran = swipeInsideScrollView ? 2.5f : 1.5f;

            // Yatay swipe'ın dikey swipe'dan belirgin olması gerekir
            if (Mathf.Abs(deltaX) > esik && Mathf.Abs(deltaX) > Mathf.Abs(deltaY) * oran)
            {
                if (deltaX < 0)
                {
                    // Sola kaydırma → Sonraki sayfa
                    int yeniIndex = Mathf.Min(aktifSayfaIndex + 1, TOPLAM_SAYFA - 1);
                    if (yeniIndex != aktifSayfaIndex) SayfaDegistir(yeniIndex);
                }
                else
                {
                    // Sağa kaydırma → Önceki sayfa
                    int yeniIndex = Mathf.Max(aktifSayfaIndex - 1, 0);
                    if (yeniIndex != aktifSayfaIndex) SayfaDegistir(yeniIndex);
                }
            }
        };

        root.RegisterCallback(swipeDownCallback);
        root.RegisterCallback(swipeUpCallback);
    }

    /// <summary>
    /// Belirtilen indeksteki sayfayı gösterir, diğerlerini gizler.
    /// Tab butonlarının aktif/pasif renklerini günceller.
    /// </summary>
    private void SayfaDegistir(int sayfaIndex)
    {
        if (aktifSayfaIndex != sayfaIndex && SesYonetici.Instance != null)
        {
            SesYonetici.Instance.PlayClick();
        }

        aktifSayfaIndex = sayfaIndex;
        if (pageSohbet != null) pageSohbet.style.display = (sayfaIndex == 0) ? DisplayStyle.Flex : DisplayStyle.None;
        if (pageGorevler != null) pageGorevler.style.display = (sayfaIndex == 1) ? DisplayStyle.Flex : DisplayStyle.None;
        if (pageDisiplin != null) pageDisiplin.style.display = (sayfaIndex == 2) ? DisplayStyle.Flex : DisplayStyle.None;
        if (pageSistem != null) pageSistem.style.display = (sayfaIndex == 3) ? DisplayStyle.Flex : DisplayStyle.None;

        Color aktifRenk = new Color(0f, 0.5f, 1f);
        Color pasifRenk = new Color(0.6f, 0.6f, 0.6f);

        // Önce hepsinden animasyon classını çıkar
        if (btnSohbet != null) btnSohbet.RemoveFromClassList("tab-button-active");
        if (btnGorevler != null) btnGorevler.RemoveFromClassList("tab-button-active");
        if (btnDisiplin != null) btnDisiplin.RemoveFromClassList("tab-button-active");
        if (btnSistem != null) btnSistem.RemoveFromClassList("tab-button-active");

        if (btnSohbet != null)
        {
            if (sayfaIndex == 0) btnSohbet.AddToClassList("tab-button-active");
            btnSohbet.style.unityBackgroundImageTintColor = new StyleColor((sayfaIndex == 0) ? aktifRenk : pasifRenk);
            btnSohbet.style.backgroundColor = new StyleColor(Color.clear);
        }
        if (btnGorevler != null)
        {
            if (sayfaIndex == 1) btnGorevler.AddToClassList("tab-button-active");
            btnGorevler.style.unityBackgroundImageTintColor = new StyleColor((sayfaIndex == 1) ? aktifRenk : pasifRenk);
            btnGorevler.style.backgroundColor = new StyleColor(Color.clear);
        }
        if (btnDisiplin != null)
        {
            if (sayfaIndex == 2) btnDisiplin.AddToClassList("tab-button-active");
            btnDisiplin.style.unityBackgroundImageTintColor = new StyleColor((sayfaIndex == 2) ? aktifRenk : pasifRenk);
            btnDisiplin.style.backgroundColor = new StyleColor(Color.clear);
        }
        if (btnSistem != null)
        {
            if (sayfaIndex == 3) btnSistem.AddToClassList("tab-button-active");
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
    public void GorevKartiEkle(string tarih, string saat, string gorevAdi, string sure = "", bool katiMi = false, bool kaydet = true, bool isRepeating = false, bool isAlreadyCompleted = false, string kategori = "Genel", int oncelik = 1, string notlar = "", string id = "")
    {
        if (gorevKartiYonetici != null)
        {
            gorevKartiYonetici.GorevKartiEkle(tarih, saat, gorevAdi, sure, katiMi, kaydet, isRepeating, isAlreadyCompleted, kategori, oncelik, notlar, id);
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

// UzunVadeliHedef ve HedefListesiWrapper artık Models/VeriModelleri.cs dosyasında tanımlıdır.
