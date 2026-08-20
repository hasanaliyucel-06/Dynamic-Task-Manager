using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Takvim render, ay navigasyonu, tarih aralığı seçimi ve hedef marker yönetimi.
/// Disiplin sayfasındaki takvim bileşeninin tüm mantığı bu sınıfta yaşar.
/// </summary>
public class TakvimYonetici : MonoBehaviour
{
    public List<UzunVadeliHedef> aktifHedefler = new List<UzunVadeliHedef>();

    private System.DateTime? secilenBaslangic = null;
    private System.DateTime? secilenBitis = null;
    private System.DateTime gosterilenAy;

    private VisualElement takvimGunler;
    private Label lblTakvimAyYil;
    private ScrollView aktifHedeflerListesi;
    
    // BUG 6 FIX: Buton referansları field-level'da (named handler'lar erişebilsin)
    private Button btnTakvimOnceki;
    private Button btnTakvimSonraki;

    // SihirbazYonetici'nin tarih aralığına erişimi için property'ler
    public System.DateTime? SecilenBaslangic { get { return secilenBaslangic; } set { secilenBaslangic = value; } }
    public System.DateTime? SecilenBitis { get { return secilenBitis; } set { secilenBitis = value; } }

    /// <summary>
    /// UI Toolkit root elementini alarak takvim referanslarını bağlar.
    /// PageNavigator.OnEnable() tarafından çağrılır.
    /// </summary>
    public void Initialize(VisualElement root)
    {
        gosterilenAy = new System.DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, 1);

        takvimGunler = root.Q<VisualElement>("takvimGunleriWrap");
        lblTakvimAyYil = root.Q<Label>("lblTakvimAyYil");
        aktifHedeflerListesi = root.Q<ScrollView>("aktifHedeflerListesi");
        btnTakvimOnceki = root.Q<Button>("btnTakvimOnceki");
        btnTakvimSonraki = root.Q<Button>("btnTakvimSonraki");

        // BUG 6 FIX: Named handler pattern — -= / += ile event sızıntısı önleme
        if (btnTakvimOnceki != null)
        {
            btnTakvimOnceki.clicked -= OnTakvimOncekiClicked;
            btnTakvimOnceki.clicked += OnTakvimOncekiClicked;
        }
        if (btnTakvimSonraki != null)
        {
            btnTakvimSonraki.clicked -= OnTakvimSonrakiClicked;
            btnTakvimSonraki.clicked += OnTakvimSonrakiClicked;
        }

        HedefleriYukle();
        TakvimGuncelle();
    }

    // BUG 6 FIX: Named event handler metotları
    private void OnTakvimOncekiClicked()
    {
        gosterilenAy = gosterilenAy.AddMonths(-1);
        TakvimGuncelle();
    }

    private void OnTakvimSonrakiClicked()
    {
        gosterilenAy = gosterilenAy.AddMonths(1);
        TakvimGuncelle();
    }

    /// <summary>
    /// PlayerPrefs'ten aktif uzun vadeli hedefleri yükler.
    /// </summary>
    public void HedefleriYukle()
    {
        aktifHedefler.Clear();
        string path = Path.Combine(Application.persistentDataPath, "goals.json");
        string json = "";

        // Migration
        if (PlayerPrefs.HasKey("AktifHedefler"))
        {
            json = PlayerPrefs.GetString("AktifHedefler");
            try
            {
                File.WriteAllText(path, json);
                PlayerPrefs.DeleteKey("AktifHedefler");
                PlayerPrefs.Save();
            }
            catch { }
        }
        else if (File.Exists(path))
        {
            try { json = File.ReadAllText(path); } catch { }
        }

        if (!string.IsNullOrEmpty(json))
        {
            HedefListesiWrapper wrapper = JsonUtility.FromJson<HedefListesiWrapper>(json);
            if (wrapper != null && wrapper.hedefler != null)
            {
                aktifHedefler = wrapper.hedefler;
            }
        }
        HedefListesiniRenderEt();
    }

    private void HedefListesiniRenderEt()
    {
        if (aktifHedeflerListesi == null) return;
        aktifHedeflerListesi.Clear();

        foreach (var hedef in aktifHedefler)
        {
            VisualElement kart = new VisualElement();
            kart.style.backgroundColor = new StyleColor(new Color(0.16f, 0.2f, 0.25f));
            kart.style.borderTopLeftRadius = 8; kart.style.borderTopRightRadius = 8;
            kart.style.borderBottomLeftRadius = 8; kart.style.borderBottomRightRadius = 8;
            kart.style.paddingLeft = 10; kart.style.paddingRight = 10;
            kart.style.paddingTop = 10; kart.style.paddingBottom = 10;
            kart.style.marginBottom = 10;
            kart.style.flexDirection = FlexDirection.Row;
            kart.style.justifyContent = Justify.SpaceBetween;
            kart.style.alignItems = Align.Center;

            VisualElement solTaraf = new VisualElement();
            solTaraf.style.flexDirection = FlexDirection.Column;

            Label lblAd = new Label(hedef.hedefAdi);
            lblAd.style.color = new StyleColor(Color.white);
            lblAd.style.unityFontStyleAndWeight = FontStyle.Bold;

            // Dinamik kalan gün hesaplama
            int dinamikKalanGun = hedef.kalanGun; // Varsayılan
            System.DateTime hBaslangic;
            if (System.DateTime.TryParseExact(hedef.baslangicTarihi, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out hBaslangic))
            {
                System.DateTime hBitis = hBaslangic.AddDays(hedef.kalanGun);
                dinamikKalanGun = (int)(hBitis.Date - System.DateTime.Now.Date).TotalDays;
            }

            string kalanText = dinamikKalanGun >= 0 ? $"Kalan: {dinamikKalanGun} gün" : "Süre doldu!";
            Label lblKalan = new Label(kalanText);
            lblKalan.style.fontSize = 12;
            lblKalan.style.color = new StyleColor(dinamikKalanGun >= 0 ? new Color(0.6f, 0.6f, 0.6f) : new Color(0.93f, 0.26f, 0.26f));

            solTaraf.Add(lblAd);
            solTaraf.Add(lblKalan);

            Button btnSil = new Button();
            btnSil.text = "✖";
            btnSil.style.backgroundColor = new StyleColor(new Color(0.9f, 0.2f, 0.2f));
            btnSil.style.color = new StyleColor(Color.white);
            btnSil.style.borderTopLeftRadius = 15; btnSil.style.borderTopRightRadius = 15;
            btnSil.style.borderBottomLeftRadius = 15; btnSil.style.borderBottomRightRadius = 15;
            btnSil.style.width = 30; btnSil.style.height = 30;
            btnSil.clicked += () => {
                aktifHedefler.RemoveAll(h => h.id == hedef.id);
                HedefleriKaydet();
                HedefListesiniRenderEt();
                TakvimGuncelle();
            };

            kart.Add(solTaraf);
            kart.Add(btnSil);
            
            aktifHedeflerListesi.Add(kart);
        }
    }

    public void HedefleriKaydet()
    {
        HedefListesiWrapper wrapper = new HedefListesiWrapper();
        wrapper.hedefler = aktifHedefler;
        string json = JsonUtility.ToJson(wrapper, true);
        string path = Path.Combine(Application.persistentDataPath, "goals.json");
        string tmpPath = path + ".tmp";
        
        try
        {
            File.WriteAllText(tmpPath, json);
            if (File.Exists(tmpPath))
            {
                if (File.Exists(path)) File.Delete(path);
                File.Move(tmpPath, path);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("[TakvimYonetici] Hedefler kaydedilemedi: " + e.Message);
        }
    }

    /// <summary>
    /// Takvimi yeniden render eder. Günleri, seçimleri ve hedef marker'larını çizer.
    /// </summary>
    public void TakvimGuncelle()
    {
        if (takvimGunler == null) return;
        takvimGunler.Clear();
        if (lblTakvimAyYil != null) lblTakvimAyYil.text = gosterilenAy.ToString("MMMM yyyy");

        int gunSayisi = System.DateTime.DaysInMonth(gosterilenAy.Year, gosterilenAy.Month);

        int firstDayOfWeek = (int)new System.DateTime(gosterilenAy.Year, gosterilenAy.Month, 1).DayOfWeek;
        int boslukSayisi = (firstDayOfWeek == 0) ? 6 : firstDayOfWeek - 1;

        for (int b = 0; b < boslukSayisi; b++)
        {
            VisualElement bosluk = new VisualElement();
            bosluk.style.width = new Length(14.28f, LengthUnit.Percent);
            bosluk.style.height = 35;
            takvimGunler.Add(bosluk);
        }

        for (int i = 1; i <= gunSayisi; i++)
        {
            int g = i;
            System.DateTime iterasyonTarihi = new System.DateTime(gosterilenAy.Year, gosterilenAy.Month, g);

            Button gunBtn = new Button();
            gunBtn.text = g.ToString();
            gunBtn.style.width = new Length(14.28f, LengthUnit.Percent);
            gunBtn.style.height = 35;
            gunBtn.style.borderTopWidth = 0;
            gunBtn.style.borderBottomWidth = 0;
            gunBtn.style.borderLeftWidth = 0;
            gunBtn.style.borderRightWidth = 0;
            gunBtn.style.paddingLeft = 0;
            gunBtn.style.paddingRight = 0;
            gunBtn.style.paddingTop = 0;
            gunBtn.style.paddingBottom = 0;
            gunBtn.style.marginLeft = 0;
            gunBtn.style.marginRight = 0;
            gunBtn.style.marginTop = 0;
            gunBtn.style.marginBottom = 2;

            // Varsayılan Stil
            gunBtn.style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f));
            gunBtn.style.color = new StyleColor(Color.white);

            // Seçim Durumuna Göre Stil
            bool isBaslangic = secilenBaslangic.HasValue && secilenBaslangic.Value.Date == iterasyonTarihi.Date;
            bool isBitis = secilenBitis.HasValue && secilenBitis.Value.Date == iterasyonTarihi.Date;
            bool isArada = secilenBaslangic.HasValue && secilenBitis.HasValue &&
                           iterasyonTarihi.Date > secilenBaslangic.Value.Date &&
                           iterasyonTarihi.Date < secilenBitis.Value.Date;

            if (isBaslangic || isBitis)
            {
                gunBtn.style.backgroundColor = new StyleColor(new Color(0.05f, 0.65f, 0.91f)); // Neon Mavi
                gunBtn.style.color = new StyleColor(Color.white);
            }
            else if (isArada)
            {
                gunBtn.style.backgroundColor = new StyleColor(new Color(0.05f, 0.65f, 0.91f, 0.3f)); // Yarı saydam
            }
            else if (iterasyonTarihi.Date == System.DateTime.Now.Date)
            {
                gunBtn.style.borderTopWidth = 1;
                gunBtn.style.borderBottomWidth = 1;
                gunBtn.style.borderLeftWidth = 1;
                gunBtn.style.borderRightWidth = 1;
                gunBtn.style.borderTopColor = new StyleColor(Color.gray);
                gunBtn.style.borderBottomColor = new StyleColor(Color.gray);
                gunBtn.style.borderLeftColor = new StyleColor(Color.gray);
                gunBtn.style.borderRightColor = new StyleColor(Color.gray);
            }

            // Hedef Marker'ları
            bool hasMarker = false;
            if (aktifHedefler != null)
            {
                foreach (var hedef in aktifHedefler)
                {
                    System.DateTime hBaslangic;
                    if (System.DateTime.TryParseExact(hedef.baslangicTarihi, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out hBaslangic))
                    {
                        System.DateTime hBitis = hBaslangic.AddDays(hedef.kalanGun);
                        if (iterasyonTarihi.Date == hBaslangic.Date || iterasyonTarihi.Date == hBitis.Date)
                        {
                            hasMarker = true;
                            break;
                        }
                    }
                }
            }

            if (hasMarker)
            {
                VisualElement marker = new VisualElement();
                marker.style.position = Position.Absolute;
                marker.style.bottom = 4;
                marker.style.left = 15;
                marker.style.width = 4;
                marker.style.height = 4;
                marker.style.borderTopLeftRadius = 2;
                marker.style.borderTopRightRadius = 2;
                marker.style.borderBottomLeftRadius = 2;
                marker.style.borderBottomRightRadius = 2;
                marker.style.backgroundColor = new StyleColor(new Color(0.97f, 0.45f, 0.08f)); // Turuncu
                gunBtn.Add(marker);
            }

            gunBtn.clicked += () => {
                if (!secilenBaslangic.HasValue || (secilenBaslangic.HasValue && secilenBitis.HasValue))
                {
                    secilenBaslangic = iterasyonTarihi;
                    secilenBitis = null;
                }
                else if (secilenBaslangic.HasValue && !secilenBitis.HasValue)
                {
                    if (iterasyonTarihi.Date >= secilenBaslangic.Value.Date)
                    {
                        secilenBitis = iterasyonTarihi;
                    }
                    else if (iterasyonTarihi.Date < secilenBaslangic.Value.Date)
                    {
                        secilenBaslangic = iterasyonTarihi;
                    }
                }
                TakvimGuncelle();
            };

            takvimGunler.Add(gunBtn);
        }
    }
}
