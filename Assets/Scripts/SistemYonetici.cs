using UnityEngine;
using UnityEngine.UIElements;

public class SistemYonetici : MonoBehaviour
{

    private Button btnSohbetSil;
    private Button btnGorevSil;
    private Label lblVeriDurum;

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

    public void Initialize(VisualElement root)
    {
        btnSohbetSil = root.Q<Button>("btnSohbetSil");
        btnGorevSil = root.Q<Button>("btnGorevSil");
        lblVeriDurum = root.Q<Label>("lblVeriDurum");

        lblBugunBasari = root.Q<Label>("lblBugunBasari");
        lblHaftaBasari = root.Q<Label>("lblHaftaBasari");
        barBugunBasari = root.Q<VisualElement>("barBugunBasari");
        barHaftaBasari = root.Q<VisualElement>("barHaftaBasari");

        btnTemaKoyu = root.Q<Button>("btnTemaKoyu");
        btnTemaAcik = root.Q<Button>("btnTemaAcik");
        btnTemaNeon = root.Q<Button>("btnTemaNeon");
        toggleSesKapat = root.Q<Toggle>("toggleSesKapat");
        uiRoot = root;

        GuncelleIstatistikler();

        // Temayı ve Ses Ayarını Yükle
        string kayitliTema = PlayerPrefs.GetString("SeciliTema", "theme-koyu");
        TemaUygula(kayitliTema);

        if (toggleSesKapat != null)
        {
            toggleSesKapat.value = PlayerPrefs.GetInt("IsMuted", 0) == 1;
            toggleSesKapat.RegisterValueChangedCallback(evt => {
                if (SesYonetici.Instance != null)
                {
                    SesYonetici.Instance.ToggleMute(evt.newValue);
                }
            });
        }

        if (btnTemaKoyu != null) btnTemaKoyu.clicked += () => TemaUygula("theme-koyu");
        if (btnTemaAcik != null) btnTemaAcik.clicked += () => TemaUygula("theme-acik");
        if (btnTemaNeon != null) btnTemaNeon.clicked += () => TemaUygula("theme-neon");

        ScheduleManager sm = FindFirstObjectByType<ScheduleManager>();
        if (sm != null)
        {
            sm.onScheduleUpdated.RemoveListener(GuncelleIstatistikler);
            sm.onScheduleUpdated.AddListener(GuncelleIstatistikler);
        }

        if (btnSohbetSil != null)
        {
            btnSohbetSil.clicked += () => {
                PlayerPrefs.DeleteKey("SohbetGecmisi");
                PlayerPrefs.Save();
                GosterDurum(lblVeriDurum, "Sohbet geçmişi silindi! (Yeniden başlatın)", true);
            };
        }

        if (btnGorevSil != null)
        {
            btnGorevSil.clicked += () => {
                PlayerPrefs.DeleteKey("SavedTasks");
                PlayerPrefs.DeleteKey("AktifHedefler");
                PlayerPrefs.Save();
                
                ScheduleManager sManager = FindFirstObjectByType<ScheduleManager>();
                if (sManager != null)
                {
                    sManager.tasks.Clear();
                    sManager.SaveTasks();
                }

                GorevKartiYonetici gManager = FindFirstObjectByType<GorevKartiYonetici>();
                if (gManager != null)
                {
                    gManager.TumGorevleriTemizle();
                }

                GosterDurum(lblVeriDurum, "Tüm görevler silindi!", true);
            };
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
        ScheduleManager sManager = FindFirstObjectByType<ScheduleManager>();
        if (sManager == null) return;

        // Günlük İstatistik
        int bugunToplam = sManager.BugununToplamSayisi();
        int bugunTamamlanan = sManager.BugununTamamlananSayisi();
        float bugunYuzde = bugunToplam > 0 ? ((float)bugunTamamlanan / bugunToplam) * 100f : 0f;

        if (lblBugunBasari != null) lblBugunBasari.text = $"%{Mathf.RoundToInt(bugunYuzde)}";
        if (barBugunBasari != null) barBugunBasari.style.width = new Length(bugunYuzde, LengthUnit.Percent);

        // Haftalık İstatistik
        float haftaYuzde = sManager.HaftalikBasariOrani();
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
