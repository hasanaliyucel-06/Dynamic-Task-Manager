using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Görev kartlarının oluşturulması, görüntülenmesi ve yönetiminden sorumlu.
/// Tamamlama, devretme, silme işlemleri, progress bar ve undo sistemi bu sınıfta yaşar.
/// </summary>
public class GorevKartiYonetici : MonoBehaviour
{
    private VisualElement progressBarFill;
    private ScrollView gorevListesi;
    private VisualElement emptyGorevler;
    private VisualElement root;

    // Cache'lenmiş referanslar (FindFirstObjectByType tekrarını önler)
    private ScheduleManager _cachedScheduleManager;

    // Düzenleme durumu
    private string duzenlenenGorevEskiAd = "";
    private string duzenlenenGorevId = "";
    private string duzenlenenGorevEskiTarih = "";
    private int duzenlenenOncelik = 1;
    private VisualElement gorevDuzenleOverlay;
    private TextField editGorevAdi, editGorevSure, editSaat, editDakika, editKategori, editNotlar;
    private Button btnDuzenleIptal, btnDuzenleKaydet;
    private Button btnDuzenleOncelikDusuk, btnDuzenleOncelikOrta, btnDuzenleOncelikYuksek;

    // Geçmiş Görevler
    private VisualElement gecmisGorevlerOverlay;
    private ScrollView scrollGecmisGorevler;
    private Button btnGecmisKapat;
    private Button btnGecmisGorevler;

    // Silme onay overlay
    private VisualElement silmeOnayOverlay;
    private Label lblSilmeOnayMetni;
    private Button btnSilmeOnayEvet, btnSilmeOnayHayir;
    private System.Action onSilmeOnaylandi;

    // Undo sistemi
    private VisualElement undoToast;
    private Label lblUndoMetni;
    private Button btnUndo;
    private System.Action onUndoAction;
    private IVisualElementScheduledItem undoTimer;

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

        // ScheduleManager referansını cache'le
        _cachedScheduleManager = FindFirstObjectByType<ScheduleManager>();

        // Düzenleme UI referansları
        gorevDuzenleOverlay = root.Q<VisualElement>("gorevDuzenleOverlay");
        editGorevAdi = root.Q<TextField>("editGorevAdi");
        editGorevSure = root.Q<TextField>("editGorevSure");
        editSaat = root.Q<TextField>("editSaat");
        editDakika = root.Q<TextField>("editDakika");
        editKategori = root.Q<TextField>("editKategori");
        editNotlar = root.Q<TextField>("editNotlar");
        btnDuzenleIptal = root.Q<Button>("btnDuzenleIptal");
        btnDuzenleKaydet = root.Q<Button>("btnDuzenleKaydet");
        
        btnDuzenleOncelikDusuk = root.Q<Button>("btnDuzenleOncelikDusuk");
        btnDuzenleOncelikOrta = root.Q<Button>("btnDuzenleOncelikOrta");
        btnDuzenleOncelikYuksek = root.Q<Button>("btnDuzenleOncelikYuksek");

        if (btnDuzenleOncelikDusuk != null) btnDuzenleOncelikDusuk.clicked += () => OncelikSec(0, true);
        if (btnDuzenleOncelikOrta != null) btnDuzenleOncelikOrta.clicked += () => OncelikSec(1, true);
        if (btnDuzenleOncelikYuksek != null) btnDuzenleOncelikYuksek.clicked += () => OncelikSec(2, true);

        // Geçmiş Görevler
        gecmisGorevlerOverlay = root.Q<VisualElement>("gecmisGorevlerOverlay");
        scrollGecmisGorevler = root.Q<ScrollView>("scrollGecmisGorevler");
        btnGecmisKapat = root.Q<Button>("btnGecmisKapat");
        btnGecmisGorevler = root.Q<Button>("Btn_GecmisGorevler");

        if (btnGecmisGorevler != null) btnGecmisGorevler.clicked += GecmisGorevleriGoster;
        if (btnGecmisKapat != null) btnGecmisKapat.clicked += () => { if (gecmisGorevlerOverlay != null) gecmisGorevlerOverlay.style.display = DisplayStyle.None; };

        // Silme onay overlay referansları
        silmeOnayOverlay = root.Q<VisualElement>("silmeOnayOverlay");
        lblSilmeOnayMetni = root.Q<Label>("lblSilmeOnayMetni");
        btnSilmeOnayEvet = root.Q<Button>("btnSilmeOnayEvet");
        btnSilmeOnayHayir = root.Q<Button>("btnSilmeOnayHayir");

        // Undo toast referansları
        undoToast = root.Q<VisualElement>("undoToast");
        lblUndoMetni = root.Q<Label>("lblUndoMetni");
        btnUndo = root.Q<Button>("btnUndo");

        if (btnDuzenleIptal != null)
        {
            btnDuzenleIptal.clicked -= DuzenleIptalClicked;
            btnDuzenleIptal.clicked += DuzenleIptalClicked;
        }
        if (btnDuzenleKaydet != null)
        {
            btnDuzenleKaydet.clicked -= KaydetGorevDuzenle;
            btnDuzenleKaydet.clicked += KaydetGorevDuzenle;
        }

        if (btnSilmeOnayEvet != null)
        {
            btnSilmeOnayEvet.clicked -= SilmeOnayEvetClicked;
            btnSilmeOnayEvet.clicked += SilmeOnayEvetClicked;
        }
        if (btnSilmeOnayHayir != null)
        {
            btnSilmeOnayHayir.clicked -= SilmeOnayHayirClicked;
            btnSilmeOnayHayir.clicked += SilmeOnayHayirClicked;
        }

        if (btnUndo != null)
        {
            btnUndo.clicked -= UndoClicked;
            btnUndo.clicked += UndoClicked;
        }

        GuncelleProgressBar();
    }

    private ScheduleManager GetScheduleManager()
    {
        if (_cachedScheduleManager == null)
            _cachedScheduleManager = FindFirstObjectByType<ScheduleManager>();
        return _cachedScheduleManager;
    }

    // Named event handler metotları (event sızıntısını önler)
    private void DuzenleIptalClicked() { if (gorevDuzenleOverlay != null) gorevDuzenleOverlay.style.display = DisplayStyle.None; }
    private void SilmeOnayEvetClicked() { onSilmeOnaylandi?.Invoke(); if (silmeOnayOverlay != null) silmeOnayOverlay.style.display = DisplayStyle.None; }
    private void SilmeOnayHayirClicked() { if (silmeOnayOverlay != null) silmeOnayOverlay.style.display = DisplayStyle.None; }
    private void UndoClicked() { onUndoAction?.Invoke(); UndoToastGizle(); }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // ANİMASYON SİSTEMİ
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    /// <summary>
    /// Kartı fade-in + slide-up animasyonuyla gösterir.
    /// </summary>
    private void AnimasyonluEkle(VisualElement kart)
    {
        kart.AddToClassList("kart-animasyonlu");
        kart.AddToClassList("kart-silindi"); // Başlangıç durumu (gizli ve sağda)

        // Bir frame sonra silindi sınıfını kaldırarak normal hale (0,0) kaymasını sağla
        kart.schedule.Execute(() => {
            kart.RemoveFromClassList("kart-silindi");
        }).StartingIn(10);
    }

    /// <summary>
    /// Kartı fade-out + slide-left animasyonuyla kaldırır.
    /// Animasyon bittikten sonra RemoveFromHierarchy çağrılır.
    /// </summary>
    private void AnimasyonluKaldir(VisualElement kart, System.Action onComplete = null)
    {
        kart.AddToClassList("kart-animasyonlu");
        kart.AddToClassList("kart-silindi"); // Sağa kayarak yok olur

        // Animasyon süresi (300ms) kadar bekle sonra DOM'dan kaldır
        kart.schedule.Execute(() => {
            kart.RemoveFromHierarchy();
            onComplete?.Invoke();
        }).StartingIn(300);
    }

    /// <summary>
    /// Tamamlama animasyonu: Scale pulse + renk geçişi.
    /// </summary>
    private void TamamlamaAnimasyonu(VisualElement kart, Label gorevYazisi, VisualElement butonKutusu)
    {
        kart.AddToClassList("kart-animasyonlu");
        kart.AddToClassList("kart-tamamlandi"); // Yeşile döner ve kayar

        kart.schedule.Execute(() => {
            kart.style.opacity = 0.4f;
            gorevYazisi.style.color = new StyleColor(new Color(0.6f, 0.6f, 0.6f));

            // Butonları ikon ile değiştir
            butonKutusu.Clear();
            Label tamamIcon = new Label("✓");
            tamamIcon.style.color = new StyleColor(new Color(0.13f, 0.77f, 0.36f));
            tamamIcon.style.fontSize = 24;
            tamamIcon.style.unityFontStyleAndWeight = FontStyle.Bold;
            butonKutusu.Add(tamamIcon);
        }).StartingIn(300);
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // UNDO SİSTEMİ
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    /// <summary>
    /// Geri alma toastını gösterir. 5 saniye sonra otomatik kapanır.
    /// </summary>
    private void UndoToastGoster(string mesaj, System.Action undoCallback)
    {
        if (undoToast == null) return;

        if (lblUndoMetni != null) lblUndoMetni.text = mesaj;
        onUndoAction = undoCallback;

        undoToast.style.display = DisplayStyle.Flex;
        undoToast.style.opacity = 1f;

        // Önceki zamanlayıcıyı iptal et
        // (Yeni bir undo geldiğinde eskisi override edilir)

        // 5 saniye sonra otomatik kapat
        undoTimer = undoToast.schedule.Execute(() => {
            UndoToastGizle();
        }).StartingIn(5000);
    }

    private void UndoToastGizle()
    {
        if (undoToast == null) return;
        undoToast.style.opacity = 0f;
        undoToast.schedule.Execute(() => {
            undoToast.style.display = DisplayStyle.None;
        }).StartingIn(300);
        onUndoAction = null;
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // DİALOGLAR
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    /// <summary>
    /// Silme onay dialogunu gösterir.
    /// </summary>
    private void SilmeOnayiGoster(string gorevAdi, System.Action onConfirm)
    {
        if (silmeOnayOverlay == null)
        {
            onConfirm?.Invoke();
            return;
        }

        if (lblSilmeOnayMetni != null) lblSilmeOnayMetni.text = $"\"{gorevAdi}\" görevini silmek istediğinize emin misiniz?";
        onSilmeOnaylandi = onConfirm;
        silmeOnayOverlay.style.display = DisplayStyle.Flex;
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // PROGRESS BAR
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    private void GuncelleProgressBar()
    {
        if (progressBarFill == null) return;

        ScheduleManager sm = GetScheduleManager();
        int toplam = 0;
        int tamamlanan = 0;

        if (sm != null)
        {
            toplam = sm.BugununToplamSayisi();
            tamamlanan = sm.BugununTamamlananSayisi();
        }

        float yuzde = toplam > 0 ? ((float)tamamlanan / toplam) * 100f : 0f;
        progressBarFill.style.width = new Length(yuzde, LengthUnit.Percent);

        // Renk geçişi: kırmızıdan yeşile
        Color renk;
        if (yuzde < 50f)
        {
            renk = Color.Lerp(new Color(0.93f, 0.26f, 0.26f), new Color(1f, 0.75f, 0f), yuzde / 50f);
        }
        else
        {
            renk = Color.Lerp(new Color(1f, 0.75f, 0f), new Color(0.13f, 0.77f, 0.36f), (yuzde - 50f) / 50f);
        }
        progressBarFill.style.backgroundColor = new StyleColor(renk);
    }

    public void AktifGorevSayisiniGuncelle()
    {
        if (root == null) return;
        VisualElement pageGorev = root.Q<VisualElement>("Page_Gorevler");
        Label lblGorevSayisi = pageGorev?.Q<Label>(className: "header-status");
        if (lblGorevSayisi != null && gorevListesi != null)
        {
            // Tamamlanmamış görevleri say
            int sayi = 0;
            foreach (var child in gorevListesi.Children())
            {
                if (child.resolvedStyle.opacity > 0.5f) sayi++;
            }
            lblGorevSayisi.text = sayi + " Aktif Görev";
            
            if (emptyGorevler != null)
            {
                emptyGorevler.style.display = gorevListesi.childCount > 0 ? DisplayStyle.None : DisplayStyle.Flex;
            }
        }
    }

    public void TumGorevleriTemizle()
    {
        if (gorevListesi != null)
        {
            gorevListesi.Clear();
        }
        GuncelleProgressBar();
        AktifGorevSayisiniGuncelle();
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // GÖREV KARTI OLUŞTURMA
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    /// <summary>
    /// Görev listesine yeni bir kart ekler. Kronolojik sıraya yerleştirir.
    /// </summary>
    public void GorevKartiEkle(string tarih, string saat, string gorevAdi, string sure = "", bool katiMi = false, bool kaydet = true, bool isRepeating = false, bool isAlreadyCompleted = false, string kategori = "Genel", int oncelik = 1, string notlar = "", string gorevId = "")
    {
        if (string.IsNullOrEmpty(gorevId)) gorevId = System.Guid.NewGuid().ToString();

        if (gorevListesi == null) return;

        VisualElement kart = new VisualElement();
        kart.AddToClassList("task-card");

        // Öncelik sınıfı ekleme
        if (oncelik == 0) kart.AddToClassList("oncelik-dusuk");
        else if (oncelik == 1) kart.AddToClassList("oncelik-orta");
        else if (oncelik == 2) kart.AddToClassList("oncelik-yuksek");

        // Katı görevse sol çizgiyi kırmızı yap (önceliği ezer)
        if (katiMi)
        {
            kart.style.borderLeftColor = new StyleColor(Color.red);
        }

        // Sürükle ve Bırak iptal edildi.

        // Tamamlanmış görevse soluk göster (animasyonsuz)
        if (isAlreadyCompleted)
        {
            kart.style.opacity = 0.4f;
        }

        // Sol panel (Yazı ve Süre)
        VisualElement solPanel = new VisualElement();
        solPanel.style.flexDirection = FlexDirection.Column;
        solPanel.style.flexGrow = 1;
        
        // Düzenleme (Tıklama) Olayı
        if (!isAlreadyCompleted)
        {
            solPanel.RegisterCallback<ClickEvent>(evt => {
                AcGorevDuzenle(tarih, saat, gorevAdi, sure, katiMi, isRepeating, kategori, oncelik, notlar, gorevId);
            });
        }

        // Görev Saati
        Label lblGorevSaati = new Label(saat);
        lblGorevSaati.name = "lblGorevSaati";
        lblGorevSaati.AddToClassList("task-saat");
        solPanel.Add(lblGorevSaati);

        Label gorevYazisi = new Label(gorevAdi);
        gorevYazisi.AddToClassList("task-text");
        gorevYazisi.style.whiteSpace = WhiteSpace.Normal;
        if (isAlreadyCompleted)
        {
            gorevYazisi.style.color = new StyleColor(new Color(0.6f, 0.6f, 0.6f));
        }
        solPanel.Add(gorevYazisi);

        // Kategori Rozeti (Badge)
        if (!string.IsNullOrEmpty(kategori) && kategori != "Genel")
        {
            Label lblKategori = new Label(kategori);
            lblKategori.style.fontSize = 10;
            lblKategori.style.color = Color.white;
            lblKategori.style.marginTop = 2;
            lblKategori.style.paddingLeft = 5;
            lblKategori.style.paddingRight = 5;
            lblKategori.style.paddingTop = 1;
            lblKategori.style.paddingBottom = 1;
            lblKategori.style.borderTopLeftRadius = 4;
            lblKategori.style.borderTopRightRadius = 4;
            lblKategori.style.borderBottomLeftRadius = 4;
            lblKategori.style.borderBottomRightRadius = 4;
            lblKategori.style.alignSelf = Align.FlexStart;
            
            // Kategoriye göre renk belirleme
            Color katRenk = new Color(0.3f, 0.3f, 0.3f);
            if (kategori.ToLower().Contains("iş") || kategori.ToLower().Contains("work")) katRenk = new Color(0.1f, 0.4f, 0.8f);
            else if (kategori.ToLower().Contains("spor")) katRenk = new Color(0.8f, 0.3f, 0.1f);
            else if (kategori.ToLower().Contains("kişisel")) katRenk = new Color(0.6f, 0.2f, 0.6f);
            else if (kategori.ToLower().Contains("eğitim")) katRenk = new Color(0.1f, 0.6f, 0.2f);
            
            lblKategori.style.backgroundColor = new StyleColor(katRenk);
            solPanel.Add(lblKategori);
        }

        // Süre etiketi
        if (!string.IsNullOrEmpty(sure) && sure != "0" && sure != "0 dk")
        {
            Label sureYazisi = new Label(sure + (sure.Contains("dk") ? "" : " dk"));
            sureYazisi.style.color = new StyleColor(Color.gray);
            sureYazisi.style.fontSize = 12;
            sureYazisi.style.marginTop = 2;
            solPanel.Add(sureYazisi);
        }

        // Görev Notları
        if (!string.IsNullOrEmpty(notlar))
        {
            Label lblNotlar = new Label(notlar);
            lblNotlar.style.color = new StyleColor(new Color(0.6f, 0.6f, 0.6f));
            lblNotlar.style.fontSize = 11;
            lblNotlar.style.marginTop = 4;
            lblNotlar.style.whiteSpace = WhiteSpace.Normal;
            solPanel.Add(lblNotlar);
        }

        // Tekrarlayan görev etiketi
        if (isRepeating)
        {
            Label tekrarLabel = new Label("🔁 Tekrarlayan");
            tekrarLabel.style.color = new StyleColor(new Color(0.37f, 0.65f, 0.96f));
            tekrarLabel.style.fontSize = 10;
            tekrarLabel.style.marginTop = 2;
            solPanel.Add(tekrarLabel);
        }

        // Buton Konteyneri
        VisualElement butonKutusu = new VisualElement();
        butonKutusu.style.flexDirection = FlexDirection.Row;
        butonKutusu.style.alignItems = Align.Center;
        butonKutusu.style.flexShrink = 0;

        if (!isAlreadyCompleted)
        {
            // ✓ Tamamlandı Butonu
            Button btnTamamlandi = OlusturYuvarlakButon("✓", new Color(0.13f, 0.77f, 0.36f, 0.8f), 30);
            btnTamamlandi.style.marginRight = 5;

            btnTamamlandi.clicked += () => {
                ScheduleManager sManager = GetScheduleManager();
                if (sManager != null)
                {
                    sManager.MarkTaskCompleted(gorevId);
                }

                // Tamamlama animasyonu
                TamamlamaAnimasyonu(kart, gorevYazisi, butonKutusu);

                GuncelleProgressBar();
                AktifGorevSayisiniGuncelle();

                // Ses ve Titreşim
                if (SesYonetici.Instance != null)
                {
                    SesYonetici.Instance.PlaySuccess();
                    SesYonetici.Instance.Vibrate(false);
                }

                // Undo seçeneği göster
                UndoToastGoster($"\"{gorevAdi}\" tamamlandı", () => {
                    // Undo: tamamlamayı geri al
                    if (sManager != null)
                    {
                        var task = sManager.tasks.Find(t => t.id == gorevId && t.isCompleted);
                        if (task != null)
                        {
                            task.isCompleted = false;
                            sManager.SaveTasks();
                        }
                    }
                    ListeyiYenidenCiz();
                });
            };

            // → Devret Butonu
            Button btnDevret = OlusturYuvarlakButon("→", new Color(0.93f, 0.65f, 0.07f, 0.8f), 30);
            btnDevret.style.marginRight = 5;

            btnDevret.clicked += () => {
                ScheduleManager sManager = GetScheduleManager();
                if (sManager != null)
                {
                    sManager.RemoveTask(gorevId);
                    string yarinTarih = System.DateTime.Now.AddDays(1).ToString("dd.MM.yyyy");
                    int sureInt = 0;
                    int.TryParse(sure, out sureInt);
                    sManager.AddTask(yarinTarih, saat, gorevAdi, sureInt, katiMi, isRepeating, kategori, 15, oncelik, notlar, gorevId);
                }

                // Silme animasyonu
                AnimasyonluKaldir(kart, () => {
                    GuncelleProgressBar();
                    AktifGorevSayisiniGuncelle();
                });

                // Ses ve Titreşim
                if (SesYonetici.Instance != null)
                {
                    SesYonetici.Instance.PlayClick();
                }

                // Undo seçeneği
                UndoToastGoster($"\"{gorevAdi}\" yarına devredildi", () => {
                    if (sManager != null)
                    {
                        // Yarınki görevi sil, bugünkü geri ekle
                        string yarinTarih2 = System.DateTime.Now.AddDays(1).ToString("dd.MM.yyyy");
                        var yarinGorev = sManager.tasks.Find(t => t.id == gorevId && t.taskDate == yarinTarih2);
                        if (yarinGorev != null) sManager.tasks.Remove(yarinGorev);
                        
                        int sureInt2 = 0;
                        int.TryParse(sure, out sureInt2);
                        sManager.AddTask(tarih, saat, gorevAdi, sureInt2, katiMi, isRepeating, kategori, 15, oncelik, notlar, gorevId);
                        sManager.SaveTasks();
                    }
                    ListeyiYenidenCiz();
                });
            };

            // 🗑 Sil Butonu
            Button btnSil = OlusturYuvarlakButon("🗑", new Color(0.33f, 0.33f, 0.33f, 1f), 24);
            btnSil.style.marginLeft = 5;

            btnSil.clicked += () => {
                SilmeOnayiGoster(gorevAdi, () => {
                    ScheduleManager sManager = GetScheduleManager();

                    // Silmeden önce verileri sakla (undo için)
                    string silinecekTarih = tarih;
                    string silinecekSaat = saat;
                    string silinecekAd = gorevAdi;
                    string silinecekSure = sure;
                    bool silinecekKati = katiMi;
                    bool silinecekTekrar = isRepeating;
                    int silinecekOncelik = oncelik;
                    string silinecekNotlar = notlar;
                    string silinecekId = gorevId;

                    if (sManager != null)
                    {
                        sManager.RemoveTask(gorevId);
                    }

                    // Silme animasyonu
                    AnimasyonluKaldir(kart, () => {
                        AktifGorevSayisiniGuncelle();
                        GuncelleProgressBar();
                    });

                    // Ses ve Titreşim (Silme)
                    if (SesYonetici.Instance != null)
                    {
                        SesYonetici.Instance.PlayDelete();
                        SesYonetici.Instance.Vibrate(true); // Ağır titreşim
                    }

                    // Undo seçeneği
                    UndoToastGoster($"\"{silinecekAd}\" silindi", () => {
                        if (sManager != null)
                        {
                            int sureInt3 = 0;
                            int.TryParse(silinecekSure, out sureInt3);
                            sManager.AddTask(silinecekTarih, silinecekSaat, silinecekAd, sureInt3, silinecekKati, silinecekTekrar, kategori, 15, silinecekOncelik, silinecekNotlar, silinecekId);
                        }
                        ListeyiYenidenCiz();
                    });
                });
            };

            butonKutusu.Add(btnTamamlandi);
            butonKutusu.Add(btnDevret);
            butonKutusu.Add(btnSil);
        }
        else
        {
            Label tamamIcon = new Label("✓");
            tamamIcon.style.color = new StyleColor(new Color(0.13f, 0.77f, 0.36f));
            tamamIcon.style.fontSize = 20;
            butonKutusu.Add(tamamIcon);
        }

        kart.Add(solPanel);
        kart.Add(butonKutusu);

        // Kronolojik sıralama
        int insertIndex = KronolojikIndexBul(saat);
        gorevListesi.Insert(insertIndex, kart);

        // Ekleme animasyonu (kayıttan yükleme hariç)
        if (kaydet)
        {
            AnimasyonluEkle(kart);
        }

        AktifGorevSayisiniGuncelle();
        GuncelleProgressBar();

        if (kaydet)
        {
            ScheduleManager sManager = GetScheduleManager();
            if (sManager != null)
            {
                int sureInt = 0;
                int.TryParse(sure, out sureInt);
                sManager.AddTask(tarih, saat, gorevAdi, sureInt, katiMi, isRepeating, kategori, 15, oncelik, notlar, gorevId);
            }
        }
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // YARDIMCI METOTLAR
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    /// <summary>
    /// Yuvarlak buton oluşturur (tamamla, devret, sil).
    /// </summary>
    private Button OlusturYuvarlakButon(string text, Color bgColor, int boyut)
    {
        Button btn = new Button();
        btn.text = text;
        btn.style.backgroundColor = new StyleColor(bgColor);
        btn.style.color = new StyleColor(Color.white);
        btn.style.width = boyut;
        btn.style.height = boyut;
        btn.style.borderTopLeftRadius = boyut / 2;
        btn.style.borderTopRightRadius = boyut / 2;
        btn.style.borderBottomLeftRadius = boyut / 2;
        btn.style.borderBottomRightRadius = boyut / 2;
        btn.style.borderTopWidth = 0;
        btn.style.borderBottomWidth = 0;
        btn.style.borderLeftWidth = 0;
        btn.style.borderRightWidth = 0;
        return btn;
    }

    /// <summary>
    /// Kronolojik sıralamada eklenecek index'i bulur.
    /// </summary>
    private int KronolojikIndexBul(string saat)
    {
        System.TimeSpan yeniSaatTS;
        string parslanacakSaat = saat;
        if (parslanacakSaat.Length >= 5 && parslanacakSaat.Contains("-"))
        {
            parslanacakSaat = parslanacakSaat.Substring(0, 5).Trim();
        }
        bool isParsed = System.TimeSpan.TryParse(parslanacakSaat, out yeniSaatTS);

        int insertIndex = gorevListesi.childCount;

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

        return insertIndex;
    }

    /// <summary>
    /// Tüm listeyi temizleyip ScheduleManager'dan bugünün görevlerini yeniden çizer.
    /// Undo işlemlerinden sonra çağrılır.
    /// </summary>
    private void ListeyiYenidenCiz()
    {
        if (gorevListesi == null) return;

        var allCards = gorevListesi.Children().ToList();
        foreach (var c in allCards)
        {
            c.RemoveFromHierarchy();
        }

        string bugununTarihi = System.DateTime.Now.ToString("dd.MM.yyyy");
        ScheduleManager sManager = GetScheduleManager();
        if (sManager != null)
        {
            foreach (var t in sManager.tasks)
            {
                if (t.taskDate == bugununTarihi)
                {
                    GorevKartiEkle(t.taskDate, t.taskTime, t.taskName, t.durationMinutes.ToString(), t.isStrictBlock, false, t.isRepeating, t.isCompleted, t.kategori, t.oncelik, t.notlar, t.id);
                }
            }
        }

        GuncelleProgressBar();
        AktifGorevSayisiniGuncelle();
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // DÜZENLEME
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    private void OncelikSec(int secilen, bool isDuzenle)
    {
        if (isDuzenle)
        {
            duzenlenenOncelik = secilen;
            if (btnDuzenleOncelikDusuk != null) btnDuzenleOncelikDusuk.style.opacity = secilen == 0 ? 1f : 0.4f;
            if (btnDuzenleOncelikOrta != null) btnDuzenleOncelikOrta.style.opacity = secilen == 1 ? 1f : 0.4f;
            if (btnDuzenleOncelikYuksek != null) btnDuzenleOncelikYuksek.style.opacity = secilen == 2 ? 1f : 0.4f;
        }
    }

    private void AcGorevDuzenle(string tarih, string eskiSaat, string eskiAd, string eskiSure, bool katiMi, bool isRepeating, string kategori, int oncelik, string notlar, string gorevId)
    {
        if (gorevDuzenleOverlay == null) return;

        duzenlenenGorevId = gorevId;
        duzenlenenGorevEskiAd = eskiAd;
        duzenlenenGorevEskiTarih = tarih;
        duzenlenenOncelik = oncelik;

        if (editGorevAdi != null) editGorevAdi.value = eskiAd;
        if (editGorevSure != null) editGorevSure.value = eskiSure;
        if (editKategori != null) editKategori.value = kategori;
        if (editNotlar != null) editNotlar.value = notlar;
        
        string[] saatParca = eskiSaat.Split(':');
        if (editSaat != null) editSaat.value = saatParca.Length > 0 ? saatParca[0] : "";
        if (editDakika != null) editDakika.value = saatParca.Length > 1 ? saatParca[1] : "";

        OncelikSec(oncelik, true);

        gorevDuzenleOverlay.style.display = DisplayStyle.Flex;
    }

    private void KaydetGorevDuzenle()
    {
        if (string.IsNullOrWhiteSpace(duzenlenenGorevEskiAd)) return;

        string yeniAd = editGorevAdi != null ? editGorevAdi.value : "";
        string yeniSure = editGorevSure != null ? editGorevSure.value : "0";
        string yeniKategori = editKategori != null ? editKategori.value : "Genel";
        string yeniNotlar = editNotlar != null ? editNotlar.value : "";
        int sureInt = 0;
        int.TryParse(yeniSure, out sureInt);

        string saat = editSaat != null ? editSaat.value : "00";
        string dakika = editDakika != null ? editDakika.value : "00";
        if (saat.Length == 1) saat = "0" + saat;
        if (dakika.Length == 1) dakika = "0" + dakika;
        string yeniZaman = $"{saat}:{dakika}";

        ScheduleManager sManager = GetScheduleManager();
        if (sManager != null)
        {
            sManager.UpdateTask(duzenlenenGorevId, yeniAd, yeniZaman, sureInt, yeniKategori, duzenlenenOncelik, yeniNotlar);
        }

        if (gorevDuzenleOverlay != null) gorevDuzenleOverlay.style.display = DisplayStyle.None;
        
        if (SesYonetici.Instance != null) SesYonetici.Instance.PlayClick();
        ListeyiYenidenCiz();
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // GEÇMİŞ GÖREVLER
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    private void GecmisGorevleriGoster()
    {
        if (gecmisGorevlerOverlay == null || scrollGecmisGorevler == null) return;
        
        scrollGecmisGorevler.Clear();
        ScheduleManager sManager = GetScheduleManager();
        if (sManager != null)
        {
            string bugun = System.DateTime.Now.ToString("dd.MM.yyyy");
            var gecmisList = sManager.tasks.Where(t => t.taskDate != bugun).OrderByDescending(t => t.taskDate).ToList();
            
            if (gecmisList.Count == 0)
            {
                Label bos = new Label("Geçmiş görev bulunmuyor.");
                bos.style.color = Color.white;
                bos.style.marginTop = 20;
                bos.style.unityTextAlign = TextAnchor.MiddleCenter;
                scrollGecmisGorevler.Add(bos);
            }
            else
            {
                string sonTarih = "";
                foreach (var t in gecmisList)
                {
                    if (t.taskDate != sonTarih)
                    {
                        Label baslik = new Label(t.taskDate);
                        baslik.style.color = new StyleColor(new Color(0.37f, 0.65f, 0.96f));
                        baslik.style.unityFontStyleAndWeight = FontStyle.Bold;
                        baslik.style.marginTop = 15;
                        baslik.style.marginBottom = 5;
                        scrollGecmisGorevler.Add(baslik);
                        sonTarih = t.taskDate;
                    }
                    
                    Label gorevItem = new Label($"- {t.taskTime} | {t.taskName} {(t.isCompleted ? "(Tamamlandı)" : "(Atlandı)")}");
                    gorevItem.style.color = t.isCompleted ? new StyleColor(new Color(0.13f, 0.77f, 0.36f)) : new StyleColor(Color.gray);
                    gorevItem.style.marginBottom = 2;
                    scrollGecmisGorevler.Add(gorevItem);
                }
            }
        }
        
        gecmisGorevlerOverlay.style.display = DisplayStyle.Flex;
    }
}
