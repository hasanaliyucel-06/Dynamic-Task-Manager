using UnityEngine;
using UnityEngine.UIElements;

public class PageNavigator : MonoBehaviour
{
    private UIDocument uiDocument;

    private VisualElement pageSohbet;
    private VisualElement pageGorevler;

    private Button btnSohbet;
    private Button btnGorevler;

    void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null) return;

        var root = uiDocument.rootVisualElement;

        btnSohbet = root.Q<Button>("Btn_TabSohbet");
        btnGorevler = root.Q<Button>("Btn_TabGorevler");

        pageSohbet = root.Q<VisualElement>("Page_Sohbet");
        pageGorevler = root.Q<VisualElement>("Page_Gorevler");

        if (btnSohbet == null) Debug.LogError("PageNavigator: Btn_TabSohbet bulunamadı!");
        if (btnGorevler == null) Debug.LogError("PageNavigator: Btn_TabGorevler bulunamadı!");
        if (pageSohbet == null) Debug.LogError("PageNavigator: Page_Sohbet bulunamadı!");
        if (pageGorevler == null) Debug.LogError("PageNavigator: Page_Gorevler bulunamadı!");

        if (btnSohbet != null)
        {
            btnSohbet.clicked += () => SayfaDegistir(true);
        }

        if (btnGorevler != null)
        {
            btnGorevler.clicked += () => SayfaDegistir(false);
        }

        TextField inputGorev = root.Q<TextField>("Input_YeniGorev");
        Button btnGorevEkle = root.Q<Button>("Btn_GorevEkle");
        ScrollView gorevListesi = root.Q<ScrollView>("GorevScrollView");

        if (btnGorevEkle != null && inputGorev != null && gorevListesi != null)
        {
            btnGorevEkle.clicked += () => {
                if (!string.IsNullOrWhiteSpace(inputGorev.value)) {
                    GorevKartiEkle(inputGorev.value, "", false); // Manuel eklenenler varsayılan
                    inputGorev.value = ""; // Kutuyu temizle
                }
            };
        }

        // Başlangıç durumu
        SayfaDegistir(true);
    }

    private void SayfaDegistir(bool sohbetMi)
    {
        if (pageSohbet != null && pageGorevler != null)
        {
            if (sohbetMi)
            {
                pageSohbet.style.display = DisplayStyle.Flex;
                pageGorevler.style.display = DisplayStyle.None;
            }
            else
            {
                pageSohbet.style.display = DisplayStyle.None;
                pageGorevler.style.display = DisplayStyle.Flex;
            }
        }

        if (btnSohbet != null && btnGorevler != null)
        {
            Color aktifRenk;
            Color pasifRenk;
            ColorUtility.TryParseHtmlString("#005BB5", out aktifRenk); // Aktif sekme mavisi
            ColorUtility.TryParseHtmlString("#1F2937", out pasifRenk); // Pasif sekme koyu grisi

            btnSohbet.style.backgroundColor = new StyleColor(sohbetMi ? aktifRenk : pasifRenk);
            btnGorevler.style.backgroundColor = new StyleColor(sohbetMi ? pasifRenk : aktifRenk);
        }
    }

    public void AktifGorevSayisiniGuncelle() {
        ScrollView gorevListesi = GetComponent<UIDocument>().rootVisualElement.Q<ScrollView>("GorevScrollView");
        VisualElement pageGorev = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("Page_Gorevler");
        Label lblGorevSayisi = pageGorev?.Q<Label>(className: "header-status"); 
        if (lblGorevSayisi != null && gorevListesi != null) {
            int sayi = gorevListesi.childCount;
            lblGorevSayisi.text = sayi + " Aktif Görev";
        }
    }

    public void GorevKartiEkle(string gorevAdi, string sure = "", bool katiMi = false, bool kaydet = true) {
        ScrollView gorevListesi = GetComponent<UIDocument>().rootVisualElement.Q<ScrollView>("GorevScrollView");
        if(gorevListesi == null) return;

        VisualElement kart = new VisualElement();
        kart.AddToClassList("task-card");
        kart.style.flexDirection = FlexDirection.Row; // Öğeleri yan yana dizmek için
        kart.style.alignItems = Align.Center; // İçerikleri dikeyde ortala

        // Katı görevse sol çizgiyi kırmızı yap
        if(katiMi) {
            kart.style.borderLeftColor = new StyleColor(Color.red);
        }

        // Sol panel (Yazı ve Süre)
        VisualElement solPanel = new VisualElement();
        solPanel.style.flexDirection = FlexDirection.Column;
        solPanel.style.flexGrow = 1; 

        Label gorevYazisi = new Label(gorevAdi);
        gorevYazisi.AddToClassList("task-text");
        gorevYazisi.style.whiteSpace = WhiteSpace.Normal;
        solPanel.Add(gorevYazisi);

        // Eğer süre parametresi boş veya '0' DEĞİLSE süre etiketini ekle
        if(!string.IsNullOrEmpty(sure) && sure != "0" && sure != "0 dk") {
            Label sureYazisi = new Label(sure + (sure.Contains("dk") ? "" : " dk"));
            sureYazisi.style.color = new StyleColor(Color.gray);
            sureYazisi.style.fontSize = 12;
            sureYazisi.style.marginTop = 2; // Daha yakın bir boşluk
            solPanel.Add(sureYazisi);
        }

        // Silme Butonu (Sağda)
        Button silButonu = new Button();
        silButonu.text = "X";
        silButonu.style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f, 1f)); // Koyu gri
        silButonu.style.color = new StyleColor(Color.white);
        silButonu.style.width = 30;
        silButonu.style.height = 30;
        silButonu.style.borderTopLeftRadius = 15;
        silButonu.style.borderTopRightRadius = 15;
        silButonu.style.borderBottomLeftRadius = 15;
        silButonu.style.borderBottomRightRadius = 15;

        silButonu.style.borderTopWidth = 0;
        silButonu.style.borderBottomWidth = 0;
        silButonu.style.borderLeftWidth = 0;
        silButonu.style.borderRightWidth = 0;
        silButonu.style.flexShrink = 0;
        
        // Hover efekti
        silButonu.RegisterCallback<MouseEnterEvent>(e => silButonu.style.backgroundColor = new StyleColor(Color.red));
        silButonu.RegisterCallback<MouseLeaveEvent>(e => silButonu.style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f, 1f)));

        silButonu.clicked += () => { 
            gorevListesi.Remove(kart); 
            AktifGorevSayisiniGuncelle();
            
            ScheduleManager sManager = FindFirstObjectByType<ScheduleManager>();
            if (sManager != null) {
                sManager.RemoveTask(gorevAdi);
            }
        }; 

        kart.Add(solPanel);
        kart.Add(silButonu);

        gorevListesi.Insert(0, kart); // Yeni görevi en üste ekle
        AktifGorevSayisiniGuncelle();

        if (kaydet) {
            ScheduleManager sManager = FindFirstObjectByType<ScheduleManager>();
            if (sManager != null) {
                int sureInt = 0;
                int.TryParse(sure, out sureInt);
                sManager.AddTask(gorevAdi, sureInt, katiMi);
            }
        }
    }
}
