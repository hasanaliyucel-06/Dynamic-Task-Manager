using UnityEngine;
using UnityEngine.UIElements;

public class SistemYonetici : MonoBehaviour
{

    private Button btnSohbetSil;
    private Button btnGorevSil;
    private Label lblVeriDurum;

    // Yedekleme UI
    private Button btnVerileriYedekle;
    private TextField inputYedekJSON;
    private Button btnYedektenDon;
    private Label lblYedekDurum;

    // Faz 3: İstatistik UI
    private Label lblBugunBasari;
    private Label lblHaftaBasari;
    private VisualElement barBugunBasari;
    private VisualElement barHaftaBasari;

    // Faz 4: Ayarlar UI
    private Button btnTemaKoyu;
    private Button btnTemaAcik;
    private Button btnTemaNeon;
    private Toggle toggleSesKapat;
    private VisualElement uiRoot;

    // Cache'lenmiş referanslar
    private ScheduleManager _cachedScheduleManager;
    private GorevKartiYonetici _cachedGorevKartiYonetici;

    public void Initialize(VisualElement root)
    {
        btnSohbetSil = root.Q<Button>("btnSohbetSil");
        btnGorevSil = root.Q<Button>("btnGorevSil");
        lblVeriDurum = root.Q<Label>("lblVeriDurum");

        lblBugunBasari = root.Q<Label>("lblBugunBasari");
        lblHaftaBasari = root.Q<Label>("lblHaftaBasari");
        barBugunBasari = root.Q<VisualElement>("barBugunBasari");
        barHaftaBasari = root.Q<VisualElement>("barHaftaBasari");

        btnVerileriYedekle = root.Q<Button>("btnVerileriYedekle");
        inputYedekJSON = root.Q<TextField>("inputYedekJSON");
        btnYedektenDon = root.Q<Button>("btnYedektenDon");
        lblYedekDurum = root.Q<Label>("lblYedekDurum");

        if (btnVerileriYedekle != null)
        {
            btnVerileriYedekle.clicked -= OnYedekleClicked;
            btnVerileriYedekle.clicked += OnYedekleClicked;
        }

        if (btnYedektenDon != null)
        {
            btnYedektenDon.clicked -= OnYedektenDonClicked;
            btnYedektenDon.clicked += OnYedektenDonClicked;
        }

        btnTemaKoyu = root.Q<Button>("btnTemaKoyu");
        btnTemaAcik = root.Q<Button>("btnTemaAcik");
        btnTemaNeon = root.Q<Button>("btnTemaNeon");
        toggleSesKapat = root.Q<Toggle>("toggleSesKapat");
        uiRoot = root;

        // Cache'le
        _cachedScheduleManager = FindFirstObjectByType<ScheduleManager>();
        _cachedGorevKartiYonetici = FindFirstObjectByType<GorevKartiYonetici>();

        GuncelleIstatistikler();

        // Temayı ve Ses Ayarını Yükle
        string kayitliTema = PlayerPrefs.GetString("SeciliTema", "theme-koyu");
        TemaUygula(kayitliTema);

        if (toggleSesKapat != null)
        {
            toggleSesKapat.value = PlayerPrefs.GetInt("IsMuted", 0) == 1;
            toggleSesKapat.RegisterValueChangedCallback(OnSesToggleChanged);
        }

        // Named handler'lar ile event bağlama (sızıntı önleme)
        if (btnTemaKoyu != null) { btnTemaKoyu.clicked -= OnTemaKoyuClicked; btnTemaKoyu.clicked += OnTemaKoyuClicked; }
        if (btnTemaAcik != null) { btnTemaAcik.clicked -= OnTemaAcikClicked; btnTemaAcik.clicked += OnTemaAcikClicked; }
        if (btnTemaNeon != null) { btnTemaNeon.clicked -= OnTemaNeonClicked; btnTemaNeon.clicked += OnTemaNeonClicked; }

        if (_cachedScheduleManager != null)
        {
            _cachedScheduleManager.onScheduleUpdated.RemoveListener(GuncelleIstatistikler);
            _cachedScheduleManager.onScheduleUpdated.AddListener(GuncelleIstatistikler);
        }

        if (btnSohbetSil != null)
        {
            btnSohbetSil.clicked -= OnSohbetSilClicked;
            btnSohbetSil.clicked += OnSohbetSilClicked;
        }

        if (btnGorevSil != null)
        {
            btnGorevSil.clicked -= OnGorevSilClicked;
            btnGorevSil.clicked += OnGorevSilClicked;
        }
    }

    // Named event handler'lar (event sızıntısını önler)
    private void OnTemaKoyuClicked() => TemaUygula("theme-koyu");
    private void OnTemaAcikClicked() => TemaUygula("theme-acik");
    private void OnTemaNeonClicked() => TemaUygula("theme-neon");

    private void OnSesToggleChanged(ChangeEvent<bool> evt)
    {
        if (SesYonetici.Instance != null)
        {
            SesYonetici.Instance.ToggleMute(evt.newValue);
        }
    }

    private void OnSohbetSilClicked()
    {
        string chatPath = System.IO.Path.Combine(Application.persistentDataPath, "chat.json");
        if (System.IO.File.Exists(chatPath)) System.IO.File.Delete(chatPath);
        GosterDurum(lblVeriDurum, "Sohbet geçmişi silindi! (Yeniden başlatın)", true);
    }

    private void OnGorevSilClicked()
    {
        string tasksPath = System.IO.Path.Combine(Application.persistentDataPath, "tasks.json");
        string goalsPath = System.IO.Path.Combine(Application.persistentDataPath, "goals.json");
        if (System.IO.File.Exists(tasksPath)) System.IO.File.Delete(tasksPath);
        if (System.IO.File.Exists(goalsPath)) System.IO.File.Delete(goalsPath);
        
        if (_cachedScheduleManager == null) _cachedScheduleManager = FindFirstObjectByType<ScheduleManager>();
        if (_cachedScheduleManager != null)
        {
            _cachedScheduleManager.tasks.Clear();
            _cachedScheduleManager.SaveTasks();
        }

        if (_cachedGorevKartiYonetici == null) _cachedGorevKartiYonetici = FindFirstObjectByType<GorevKartiYonetici>();
        if (_cachedGorevKartiYonetici != null)
        {
            _cachedGorevKartiYonetici.TumGorevleriTemizle();
        }

        TakvimYonetici ty = FindFirstObjectByType<TakvimYonetici>();
        if (ty != null)
        {
            ty.aktifHedefler.Clear();
            ty.HedefleriKaydet();
            ty.HedefleriYukle();
        }

        GosterDurum(lblVeriDurum, "Tüm görevler silindi!", true);
    }

    private void OnYedekleClicked()
    {
        SistemYedek yedek = new SistemYedek();
        string tasksPath = System.IO.Path.Combine(Application.persistentDataPath, "tasks.json");
        string goalsPath = System.IO.Path.Combine(Application.persistentDataPath, "goals.json");
        string chatPath = System.IO.Path.Combine(Application.persistentDataPath, "chat.json");

        if (System.IO.File.Exists(tasksPath)) yedek.tasksJson = System.IO.File.ReadAllText(tasksPath);
        if (System.IO.File.Exists(goalsPath)) yedek.goalsJson = System.IO.File.ReadAllText(goalsPath);
        if (System.IO.File.Exists(chatPath)) yedek.chatJson = System.IO.File.ReadAllText(chatPath);

        string masterJson = JsonUtility.ToJson(yedek);
        GUIUtility.systemCopyBuffer = masterJson;
        GosterDurum(lblYedekDurum, "Veriler panoya kopyalandı!", true);
    }

    private void OnYedektenDonClicked()
    {
        if (inputYedekJSON == null || string.IsNullOrWhiteSpace(inputYedekJSON.value))
        {
            GosterDurum(lblYedekDurum, "Hata: JSON verisi boş!", false);
            return;
        }

        try
        {
            SistemYedek yedek = JsonUtility.FromJson<SistemYedek>(inputYedekJSON.value);
            if (yedek == null) throw new System.Exception("Geçersiz JSON formatı.");

            string tasksPath = System.IO.Path.Combine(Application.persistentDataPath, "tasks.json");
            string goalsPath = System.IO.Path.Combine(Application.persistentDataPath, "goals.json");
            string chatPath = System.IO.Path.Combine(Application.persistentDataPath, "chat.json");

            if (!string.IsNullOrEmpty(yedek.tasksJson)) System.IO.File.WriteAllText(tasksPath, yedek.tasksJson);
            if (!string.IsNullOrEmpty(yedek.goalsJson)) System.IO.File.WriteAllText(goalsPath, yedek.goalsJson);
            if (!string.IsNullOrEmpty(yedek.chatJson)) System.IO.File.WriteAllText(chatPath, yedek.chatJson);

            GosterDurum(lblYedekDurum, "Başarılı! Yeniden başlatın.", true);
            inputYedekJSON.value = "";
        }
        catch (System.Exception e)
        {
            GosterDurum(lblYedekDurum, "Hata: " + e.Message, false);
        }
    }

    private void GosterDurum(Label lbl, string mesaj, bool basarili)
    {
        if (lbl == null) return;
        lbl.text = mesaj;
        lbl.style.color = new StyleColor(basarili ? new Color(0.1f, 0.73f, 0.5f) : new Color(0.93f, 0.26f, 0.26f));
        
        // 3 saniye sonra yazıyı sil
        lbl.schedule.Execute(() => {
            lbl.text = "";
        }).StartingIn(3000);
    }

    public void GuncelleIstatistikler()
    {
        if (_cachedScheduleManager == null) _cachedScheduleManager = FindFirstObjectByType<ScheduleManager>();
        if (_cachedScheduleManager == null) return;

        // Günlük İstatistik
        int bugunToplam = _cachedScheduleManager.BugununToplamSayisi();
        int bugunTamamlanan = _cachedScheduleManager.BugununTamamlananSayisi();
        float bugunYuzde = bugunToplam > 0 ? ((float)bugunTamamlanan / bugunToplam) * 100f : 0f;

        if (lblBugunBasari != null) lblBugunBasari.text = $"%{Mathf.RoundToInt(bugunYuzde)}";
        if (barBugunBasari != null) barBugunBasari.style.width = new Length(bugunYuzde, LengthUnit.Percent);

        // Haftalık İstatistik
        float haftaYuzde = _cachedScheduleManager.HaftalikBasariOrani();
        if (lblHaftaBasari != null) lblHaftaBasari.text = $"%{Mathf.RoundToInt(haftaYuzde)}";
        if (barHaftaBasari != null) barHaftaBasari.style.width = new Length(haftaYuzde, LengthUnit.Percent);
    }

    private void TemaUygula(string temaAdi)
    {
        if (uiRoot == null) return;

        uiRoot.RemoveFromClassList("theme-koyu");
        uiRoot.RemoveFromClassList("theme-acik");
        uiRoot.RemoveFromClassList("theme-neon");
        
        uiRoot.AddToClassList(temaAdi);

        PlayerPrefs.SetString("SeciliTema", temaAdi);
        PlayerPrefs.Save();
        
        if (SesYonetici.Instance != null)
        {
            SesYonetici.Instance.PlayClick();
        }
    }
}

[System.Serializable]
public class SistemYedek
{
    public string tasksJson;
    public string goalsJson;
    public string chatJson;
}
