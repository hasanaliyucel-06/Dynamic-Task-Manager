using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

/// <summary>
/// Görev kartlarının oluşturulması, görüntülenmesi ve yönetiminden sorumlu.
/// Tamamlama, devretme, silme işlemleri ve progress bar bu sınıfta yaşar.
/// </summary>
public class GorevKartiYonetici : MonoBehaviour
{
    private int bugunToplamGorev = 0;
    private int bugunTamamlananGorev = 0;
    private VisualElement progressBarFill;
    private ScrollView gorevListesi;
    private VisualElement emptyGorevler;
    private VisualElement root;

    // Düzenleme durumu
    private string duzenlenenGorevEskiAd = "";
    private string duzenlenenGorevEskiTarih = "";
    private VisualElement gorevDuzenleOverlay;
    private TextField editGorevAdi, editGorevSure, editSaat, editDakika;
    private Button btnDuzenleIptal, btnDuzenleKaydet;

    /// <summary>
    /// UI Toolkit root elementini alarak gerekli UI referanslarını bağlar.
    /// PageNavigator.OnEnable() tarafından çağrılır.
    /// </summary>
    public void Initialize(VisualElement rootElement)
    {
        root = rootElement;
        progressBarFill = root.Q<VisualElement>("progressBarFill");
        gorevListesi = root.Q<ScrollView>("gorevlerScrollView");
        emptyGorevler = root.Q<VisualElement>("emptyGorevler");

        // Düzenleme UI referansları
        gorevDuzenleOverlay = root.Q<VisualElement>("gorevDuzenleOverlay");
        editGorevAdi = root.Q<TextField>("editGorevAdi");
        editGorevSure = root.Q<TextField>("editGorevSure");
        editSaat = root.Q<TextField>("editSaat");
        editDakika = root.Q<TextField>("editDakika");
        btnDuzenleIptal = root.Q<Button>("btnDuzenleIptal");
        btnDuzenleKaydet = root.Q<Button>("btnDuzenleKaydet");

        if (btnDuzenleIptal != null)
        {
            btnDuzenleIptal.clicked += () => { if (gorevDuzenleOverlay != null) gorevDuzenleOverlay.style.display = DisplayStyle.None; };
        }
        if (btnDuzenleKaydet != null)
        {
            btnDuzenleKaydet.clicked += KaydetGorevDuzenle;
        }

        GuncelleProgressBar();
    }

    private void GuncelleProgressBar()
    {
        if (progressBarFill == null) return;
        float yuzde = bugunToplamGorev > 0 ? ((float)bugunTamamlananGorev / bugunToplamGorev) * 100f : 0f;
        progressBarFill.style.width = new Length(yuzde, LengthUnit.Percent);
    }

    public void AktifGorevSayisiniGuncelle()
    {
        if (root == null) return;
        VisualElement pageGorev = root.Q<VisualElement>("Page_Gorevler");
        Label lblGorevSayisi = pageGorev?.Q<Label>(className: "header-status");
        if (lblGorevSayisi != null && gorevListesi != null)
        {
            int sayi = gorevListesi.childCount;
            lblGorevSayisi.text = sayi + " Aktif Görev";
            
            if (emptyGorevler != null)
            {
                emptyGorevler.style.display = sayi > 0 ? DisplayStyle.None : DisplayStyle.Flex;
            }
        }
    }

    public void TumGorevleriTemizle()
    {
        if (gorevListesi != null)
        {
            gorevListesi.Clear();
        }
        bugunToplamGorev = 0;
        bugunTamamlananGorev = 0;
        GuncelleProgressBar();
        AktifGorevSayisiniGuncelle();
    }

    /// <summary>
    /// Görev listesine yeni bir kart ekler. Kronolojik sıraya yerleştirir.
    /// ScheduleManager ve SiberAsistan tarafından da (PageNavigator proxy'si üzerinden) çağrılır.
    /// </summary>
    public void GorevKartiEkle(string tarih, string saat, string gorevAdi, string sure = "", bool katiMi = false, bool kaydet = true, bool isRepeating = false)
    {
        bugunToplamGorev++;
        GuncelleProgressBar();

        if (gorevListesi == null) return;

        VisualElement kart = new VisualElement();
        kart.AddToClassList("task-card");

        // Katı görevse sol çizgiyi kırmızı yap
        if (katiMi)
        {
            kart.style.borderLeftColor = new StyleColor(Color.red);
        }

        // Sol panel (Yazı ve Süre)
        VisualElement solPanel = new VisualElement();
        solPanel.style.flexDirection = FlexDirection.Column;
        solPanel.style.flexGrow = 1;
        
        // Düzenleme (Tıklama) Olayı
        solPanel.RegisterCallback<ClickEvent>(evt => {
            AcGorevDuzenle(tarih, saat, gorevAdi, sure, katiMi, isRepeating);
        });


        // Görev Saati (Timeblocking)
        Label lblGorevSaati = new Label(saat);
        lblGorevSaati.name = "lblGorevSaati";
        lblGorevSaati.style.color = new StyleColor(new Color(0f, 1f, 1f)); // Neon Camgöbeği
        lblGorevSaati.style.fontSize = 14;
        lblGorevSaati.style.unityFontStyleAndWeight = FontStyle.Bold;
        lblGorevSaati.style.marginBottom = 2;
        solPanel.Add(lblGorevSaati);

        Label gorevYazisi = new Label(gorevAdi);
        gorevYazisi.AddToClassList("task-text");
        gorevYazisi.style.whiteSpace = WhiteSpace.Normal;
        solPanel.Add(gorevYazisi);

        // Süre etiketi
        if (!string.IsNullOrEmpty(sure) && sure != "0" && sure != "0 dk")
        {
            Label sureYazisi = new Label(sure + (sure.Contains("dk") ? "" : " dk"));
            sureYazisi.style.color = new StyleColor(Color.gray);
            sureYazisi.style.fontSize = 12;
            sureYazisi.style.marginTop = 2;
            solPanel.Add(sureYazisi);
        }

        // Buton Konteyneri (Sağda, yan yana)
        VisualElement butonKutusu = new VisualElement();
        butonKutusu.style.flexDirection = FlexDirection.Row;
        butonKutusu.style.alignItems = Align.Center;
        butonKutusu.style.flexShrink = 0;

        // Tamamlandı Butonu
        Button btnTamamlandi = new Button();
        btnTamamlandi.text = "✓";
        btnTamamlandi.style.backgroundColor = new StyleColor(new Color(0.13f, 0.77f, 0.36f, 0.8f));
        btnTamamlandi.style.color = new StyleColor(Color.white);
        btnTamamlandi.style.width = 30;
        btnTamamlandi.style.height = 30;
        btnTamamlandi.style.borderTopLeftRadius = 15;
        btnTamamlandi.style.borderTopRightRadius = 15;
        btnTamamlandi.style.borderBottomLeftRadius = 15;
        btnTamamlandi.style.borderBottomRightRadius = 15;
        btnTamamlandi.style.borderTopWidth = 0;
        btnTamamlandi.style.borderBottomWidth = 0;
        btnTamamlandi.style.borderLeftWidth = 0;
        btnTamamlandi.style.borderRightWidth = 0;
        btnTamamlandi.style.marginRight = 5;

        bool isCompleted = false;

        btnTamamlandi.clicked += () => {
            if (!isCompleted)
            {
                bugunTamamlananGorev++;
                isCompleted = true;
                kart.style.opacity = 0.5f;
                btnTamamlandi.style.backgroundColor = new StyleColor(Color.gray);
                GuncelleProgressBar();

                AktifGorevSayisiniGuncelle();

                ScheduleManager sManager = FindFirstObjectByType<ScheduleManager>();
                if (sManager != null)
                {
                    sManager.RemoveTask(gorevAdi);
                }
            }
        };

        // Devret Butonu
        Button btnDevret = new Button();
        btnDevret.text = "✖";
        btnDevret.style.backgroundColor = new StyleColor(new Color(0.93f, 0.26f, 0.26f, 0.8f));
        btnDevret.style.color = new StyleColor(Color.white);
        btnDevret.style.width = 30;
        btnDevret.style.height = 30;
        btnDevret.style.borderTopLeftRadius = 15;
        btnDevret.style.borderTopRightRadius = 15;
        btnDevret.style.borderBottomLeftRadius = 15;
        btnDevret.style.borderBottomRightRadius = 15;
        btnDevret.style.borderTopWidth = 0;
        btnDevret.style.borderBottomWidth = 0;
        btnDevret.style.borderLeftWidth = 0;
        btnDevret.style.borderRightWidth = 0;

        btnDevret.clicked += () => {
            ScheduleManager sManager = FindFirstObjectByType<ScheduleManager>();
            if (sManager != null)
            {
                sManager.RemoveTask(gorevAdi);
                
                System.DateTime gorevTarihi;
                if (!System.DateTime.TryParseExact(tarih, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out gorevTarihi))
                {
                    gorevTarihi = System.DateTime.Now;
                }
                
                string yarinTarih = gorevTarihi.AddDays(1).ToString("dd.MM.yyyy");
                int sureInt = 0;
                int.TryParse(sure, out sureInt);
                
                sManager.AddTask(yarinTarih, saat, gorevAdi, sureInt, katiMi, isRepeating);
            }

            // UI'dan kaldır
            kart.RemoveFromHierarchy();
            AktifGorevSayisiniGuncelle();
        };

        // Sil Butonu
        Button btnSil = new Button();
        btnSil.text = "🗑";
        btnSil.style.backgroundColor = new StyleColor(new Color(0.33f, 0.33f, 0.33f, 1f));
        btnSil.style.color = new StyleColor(Color.white);
        btnSil.style.width = 24;
        btnSil.style.height = 24;
        btnSil.style.borderTopLeftRadius = 12;
        btnSil.style.borderTopRightRadius = 12;
        btnSil.style.borderBottomLeftRadius = 12;
        btnSil.style.borderBottomRightRadius = 12;
        btnSil.style.borderTopWidth = 0;
        btnSil.style.borderBottomWidth = 0;
        btnSil.style.borderLeftWidth = 0;
        btnSil.style.borderRightWidth = 0;
        btnSil.style.marginLeft = 5;

        btnSil.clicked += () => {
            kart.RemoveFromHierarchy();
            AktifGorevSayisiniGuncelle();

            ScheduleManager sManager = FindFirstObjectByType<ScheduleManager>();
            if (sManager != null)
            {
                sManager.RemoveTask(gorevAdi);
            }
        };

        butonKutusu.Add(btnTamamlandi);
        butonKutusu.Add(btnDevret);
        butonKutusu.Add(btnSil);

        kart.Add(solPanel);
        kart.Add(butonKutusu);

        // Kronolojik sıralama
        System.TimeSpan yeniSaatTS;
        string parslanacakSaat = saat;
        if (parslanacakSaat.Length >= 5 && parslanacakSaat.Contains("-"))
        {
            parslanacakSaat = parslanacakSaat.Substring(0, 5).Trim();
        }
        bool isParsed = System.TimeSpan.TryParse(parslanacakSaat, out yeniSaatTS);

        int insertIndex = gorevListesi.childCount; // Varsayılan olarak en sona ekle

        if (isParsed)
        {
            for (int i = 0; i < gorevListesi.childCount; i++)
            {
                VisualElement sibling = gorevListesi.ElementAt(i);
                Label lblSibling = sibling.Q<Label>("lblGorevSaati");
                if (lblSibling != null)
                {
                    string sibSaat = lblSibling.text;
                    if (sibSaat.Length >= 5 && sibSaat.Contains("-"))
                    {
                        sibSaat = sibSaat.Substring(0, 5).Trim();
                    }
                    System.TimeSpan siblingTS;
                    if (System.TimeSpan.TryParse(sibSaat, out siblingTS))
                    {
                        if (yeniSaatTS < siblingTS)
                        {
                            insertIndex = i;
                            break;
                        }
                    }
                }
            }
        }

        gorevListesi.Insert(insertIndex, kart); // Kronolojik sıraya göre ekle
        AktifGorevSayisiniGuncelle();

        if (kaydet)
        {
            ScheduleManager sManager = FindFirstObjectByType<ScheduleManager>();
            if (sManager != null)
            {
                int sureInt = 0;
                int.TryParse(sure, out sureInt);
                sManager.AddTask(tarih, saat, gorevAdi, sureInt, katiMi, isRepeating);
            }
        }
    }

    private void AcGorevDuzenle(string tarih, string eskiSaat, string eskiAd, string eskiSure, bool katiMi, bool isRepeating)
    {
        if (gorevDuzenleOverlay == null) return;

        duzenlenenGorevEskiAd = eskiAd;
        duzenlenenGorevEskiTarih = tarih; // Düzenleme şu anki liste içinde olacağı için tarih aynı kalır.

        if (editGorevAdi != null) editGorevAdi.value = eskiAd;
        if (editGorevSure != null) editGorevSure.value = eskiSure;
        
        string[] saatParca = eskiSaat.Split(':');
        if (editSaat != null) editSaat.value = saatParca.Length > 0 ? saatParca[0] : "";
        if (editDakika != null) editDakika.value = saatParca.Length > 1 ? saatParca[1] : "";

        gorevDuzenleOverlay.style.display = DisplayStyle.Flex;
    }

    private void KaydetGorevDuzenle()
    {
        if (string.IsNullOrWhiteSpace(duzenlenenGorevEskiAd)) return;

        string yeniAd = editGorevAdi != null ? editGorevAdi.value : "";
        string yeniSure = editGorevSure != null ? editGorevSure.value : "0";
        int sureInt = 0;
        int.TryParse(yeniSure, out sureInt);

        string saat = editSaat != null ? editSaat.value : "00";
        string dakika = editDakika != null ? editDakika.value : "00";
        // Formatı düzelt (örnek: "9" -> "09")
        if (saat.Length == 1) saat = "0" + saat;
        if (dakika.Length == 1) dakika = "0" + dakika;
        string yeniZaman = $"{saat}:{dakika}";

        ScheduleManager sManager = FindFirstObjectByType<ScheduleManager>();
        if (sManager != null)
        {
            sManager.UpdateTask(duzenlenenGorevEskiAd, yeniAd, yeniZaman, sureInt);
        }

        if (gorevDuzenleOverlay != null) gorevDuzenleOverlay.style.display = DisplayStyle.None;
        
        // UI listesini komple yenilemek için tüm listeyi siliyoruz. onScheduleUpdated olayı bunu geri dolduracak ama OnEnable tetiklenmeli.
        // Aslında en temizi, PageNavigator üzerinden listeyi yeniden yükletmektir. Veya ScheduleManager'daki onScheduleUpdated'i buraya bağlayıp her seferinde listeyi boşaltıp baştan çizmektir.
        // Şimdilik sayfayı yenilemek için hızlı bir hile:
        var allCards = gorevListesi.Children().ToList();
        foreach (var c in allCards)
        {
            if (c.ClassListContains("task-card")) c.RemoveFromHierarchy();
        }
        bugunTamamlananGorev = 0;
        bugunToplamGorev = 0;
        if (sManager != null)
        {
            // Sadece bugünün görevlerini tekrar yükle
            string bugununTarihi = System.DateTime.Now.ToString("dd.MM.yyyy");
            foreach(var t in sManager.tasks)
            {
                if (t.taskDate == bugununTarihi)
                {
                    GorevKartiEkle(t.taskDate, t.taskTime, t.taskName, t.durationMinutes.ToString(), t.isStrictBlock, false, t.isRepeating);
                    if (t.isCompleted) {
                        // Eğer görev tamamlanmışsa, tamamlandı olarak işaretle (bu mantık burada karmaşıklaşabilir, idealde listeyi tam baştan çizen bir render metodu olmalı)
                        // Görevin basit düzenleme işlemi sonrası yeniden çizildiğini varsayıyoruz.
                    }
                }
            }
        }
    }
}
