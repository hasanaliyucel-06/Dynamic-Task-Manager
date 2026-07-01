using UnityEngine;
using UnityEngine.UIElements;

public class PageNavigator : MonoBehaviour
{
    public System.Collections.Generic.List<UzunVadeliHedef> aktifHedefler = new System.Collections.Generic.List<UzunVadeliHedef>();

    // Sürükle Bırak (Drag & Drop) Değişkenleri
    private VisualElement draggedCard;
    private bool isDragging = false;
    private Vector2 startMousePosition;
    private int originalIndex;

    private void HedefleriYukle() {
        if (PlayerPrefs.HasKey("AktifHedefler")) {
            string json = PlayerPrefs.GetString("AktifHedefler");
            HedefListesiWrapper wrapper = JsonUtility.FromJson<HedefListesiWrapper>(json);
            if (wrapper != null && wrapper.hedefler != null) {
                aktifHedefler = wrapper.hedefler;
            }
        }
    }

    private UIDocument uiDocument;

    private VisualElement pageSohbet;
    private VisualElement pageGorevler;
    private VisualElement pageDisiplin;
    private VisualElement pageSistem;

    private Button btnSohbet;
    private Button btnGorevler;
    private Button btnDisiplin;
    private Button btnSistem;

    void OnEnable()
    {
        HedefleriYukle();
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null) return;

        var root = uiDocument.rootVisualElement;

        btnSohbet = root.Q<Button>("Btn_TabSohbet");
        btnGorevler = root.Q<Button>("Btn_TabGorevler");
        btnDisiplin = root.Q<Button>("btnDisiplin");
        btnSistem = root.Q<Button>("btnSistem");

        pageSohbet = root.Q<VisualElement>("Page_Sohbet");
        pageGorevler = root.Q<VisualElement>("Page_Gorevler");
        pageDisiplin = root.Q<VisualElement>("pageDisiplin");
        pageSistem = root.Q<VisualElement>("pageSistem");

        if (btnSohbet != null) btnSohbet.clicked += () => SayfaDegistir(0);
        if (btnGorevler != null) btnGorevler.clicked += () => SayfaDegistir(1);
        if (btnDisiplin != null) btnDisiplin.clicked += () => SayfaDegistir(2);
        if (btnSistem != null) btnSistem.clicked += () => SayfaDegistir(3);

        TextField inputGorev = root.Q<TextField>("Input_YeniGorev");
        TextField inputGorevSure = root.Q<TextField>("GorevSureInput");
        Button btnGorevEkle = root.Q<Button>("Btn_GorevEkle");
        ScrollView gorevListesi = root.Q<ScrollView>("gorevlerScrollView");

        if (btnGorevEkle != null && inputGorev != null && gorevListesi != null)
        {
            btnGorevEkle.clicked += () => {
                if (string.IsNullOrWhiteSpace(inputGorev.value)) return;

                string sureDegeri = "0";
                if (inputGorevSure != null && !string.IsNullOrWhiteSpace(inputGorevSure.value))
                {
                    if (int.TryParse(inputGorevSure.value, out int parsedSure))
                    {
                        sureDegeri = parsedSure.ToString();
                    }
                }

                GorevKartiEkle(inputGorev.value, sureDegeri, false);
                
                inputGorev.value = ""; // Kutuyu temizle
                if (inputGorevSure != null) inputGorevSure.value = "";
            };
        }

        TextField inputHedef = root.Q<TextField>("inputHedef");
        Button btnAiPlanla = root.Q<Button>("btnAiPlanla");

        Button btnBaslangic = root.Q<Button>("btnBaslangic");
        Button btnBitis = root.Q<Button>("btnBitis");
        VisualElement takvimPenceresi = root.Q<VisualElement>("takvimPenceresi");
        VisualElement takvimGunler = root.Q<VisualElement>("takvimGunler");
        Label lblTakvimAyYil = root.Q<Label>("lblTakvimAyYil");
        Button btnTakvimKapat = root.Q<Button>("btnTakvimKapat");
        Button btnTakvimOnceki = root.Q<Button>("btnTakvimOnceki");
        Button btnTakvimSonraki = root.Q<Button>("btnTakvimSonraki");

        System.DateTime secilenBaslangic = System.DateTime.Now;
        System.DateTime secilenBitis = System.DateTime.MinValue;
        bool isBaslangicSeciliyor = true;

        System.DateTime gosterilenAy = new System.DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, 1);

        System.Action TakvimGuncelle = () => {
            if (takvimGunler == null) return;
            takvimGunler.Clear();
            if (lblTakvimAyYil != null) lblTakvimAyYil.text = gosterilenAy.ToString("MMMM yyyy");
            
            int gunSayisi = System.DateTime.DaysInMonth(gosterilenAy.Year, gosterilenAy.Month);
            for (int i = 1; i <= gunSayisi; i++) {
                int g = i;
                Button gunBtn = new Button();
                gunBtn.text = g.ToString();
                gunBtn.style.width = 35;
                gunBtn.style.height = 35;
                gunBtn.style.marginRight = 2;
                gunBtn.style.marginBottom = 2;
                gunBtn.style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f));
                gunBtn.style.color = new StyleColor(Color.white);
                
                if (gosterilenAy.Year == System.DateTime.Now.Year && gosterilenAy.Month == System.DateTime.Now.Month && g == System.DateTime.Now.Day) {
                    gunBtn.style.backgroundColor = new StyleColor(new Color(0f, 0.5f, 1f));
                }

                gunBtn.clicked += () => {
                    System.DateTime secilenTakvimTarihi = new System.DateTime(gosterilenAy.Year, gosterilenAy.Month, g);
                    
                    if (isBaslangicSeciliyor) {
                        secilenBaslangic = secilenTakvimTarihi;
                        if (btnBaslangic != null) btnBaslangic.text = "Başlangıç: " + secilenBaslangic.ToString("dd.MM.yyyy");
                    } else {
                        secilenBitis = secilenTakvimTarihi;
                        if (btnBitis != null) btnBitis.text = "Bitiş: " + secilenBitis.ToString("dd.MM.yyyy");
                    }

                    takvimPenceresi.style.display = DisplayStyle.None;
                };
                takvimGunler.Add(gunBtn);
            }
        };

        if (takvimPenceresi != null)
        {
            if (btnBaslangic != null) {
                btnBaslangic.text = "Başlangıç: " + System.DateTime.Now.ToString("dd.MM.yyyy");
                btnBaslangic.clicked += () => {
                    isBaslangicSeciliyor = true;
                    gosterilenAy = new System.DateTime(secilenBaslangic.Year, secilenBaslangic.Month, 1);
                    takvimPenceresi.style.display = DisplayStyle.Flex;
                    TakvimGuncelle();
                };
            }

            if (btnBitis != null) {
                btnBitis.clicked += () => {
                    isBaslangicSeciliyor = false;
                    System.DateTime baslangicGosterimi = secilenBitis != System.DateTime.MinValue ? secilenBitis : secilenBaslangic;
                    gosterilenAy = new System.DateTime(baslangicGosterimi.Year, baslangicGosterimi.Month, 1);
                    takvimPenceresi.style.display = DisplayStyle.Flex;
                    TakvimGuncelle();
                };
            }
            
            if (btnTakvimKapat != null) btnTakvimKapat.clicked += () => takvimPenceresi.style.display = DisplayStyle.None;
            
            if (btnTakvimOnceki != null) {
                btnTakvimOnceki.clicked += () => {
                    gosterilenAy = gosterilenAy.AddMonths(-1);
                    TakvimGuncelle();
                };
            }
            if (btnTakvimSonraki != null) {
                btnTakvimSonraki.clicked += () => {
                    gosterilenAy = gosterilenAy.AddMonths(1);
                    TakvimGuncelle();
                };
            }
        }

        if (btnAiPlanla != null && inputHedef != null)
        {
            btnAiPlanla.clicked += () => {
                if (string.IsNullOrWhiteSpace(inputHedef.value)) return;
                
                if (secilenBitis == System.DateTime.MinValue) {
                    Debug.LogWarning("Bitiş tarihi seçilmedi!");
                    return;
                }

                int kalanGun = (int)(secilenBitis.Date - secilenBaslangic.Date).TotalDays;

                if (kalanGun < 0) {
                    Debug.LogWarning("Bitiş tarihi, Başlangıç tarihinden önce olamaz!");
                    return;
                }

                btnAiPlanla.SetEnabled(false);
                btnAiPlanla.text = "PLANLANIYOR...";

                string gizliPrompt = $"Kullanıcının '{inputHedef.value}' hedefine ulaşması için {kalanGun} günü var. BUGÜN yapması gereken tek bir spesifik görev üret. Yanıtın KESİNLİKLE sadece şu formatta olmalı: [HEDEF:Görev Adı:SÜRE]. SÜRE kısmı SADECE ve SADECE rakamlardan (örn: 30) oluşmalıdır. Hiçbir metin ekleme!";

                SiberAsistan asistan = FindFirstObjectByType<SiberAsistan>();
                if (asistan != null)
                {
                    asistan.GizliSorguYap(gizliPrompt, (cevap) => {
                        string temizCevap = cevap.Trim();
                        temizCevap = temizCevap.Replace("[HEDEF:", "").Replace("]", "");
                        
                        string[] parcalar = temizCevap.Split(':');
                        if (parcalar.Length >= 2)
                        {
                            string gelenSure = parcalar[parcalar.Length - 1].Trim();
                            string sadeceRakamlar = System.Text.RegularExpressions.Regex.Match(gelenSure, @"\d+").Value;
                            if (string.IsNullOrEmpty(sadeceRakamlar)) {
                                sadeceRakamlar = "30"; 
                            }

                            string gorevAdi = string.Join(":", parcalar, 0, parcalar.Length - 1).Trim();
                            
                            GorevKartiEkle(gorevAdi, sadeceRakamlar, false);

                            UzunVadeliHedef yeniHedef = new UzunVadeliHedef();
                            yeniHedef.hedefAdi = inputHedef.value;
                            yeniHedef.kalanGun = kalanGun;
                            yeniHedef.baslangicTarihi = System.DateTime.Now.ToString("dd.MM.yyyy");
                            aktifHedefler.Add(yeniHedef);

                            HedefListesiWrapper wrapper = new HedefListesiWrapper();
                            wrapper.hedefler = aktifHedefler;
                            PlayerPrefs.SetString("AktifHedefler", JsonUtility.ToJson(wrapper));
                            PlayerPrefs.Save();
                        }
                        else
                        {
                            Debug.LogWarning("AI Planlama formatı anlaşılamadı (Eksik parametre): " + cevap);
                        }

                        inputHedef.value = "";
                        btnAiPlanla.text = "YAPAY ZEKAYA PLANLAT";
                        btnAiPlanla.SetEnabled(true);
                    });
                }
                else
                {
                    Debug.LogError("SiberAsistan bulunamadı!");
                    btnAiPlanla.text = "YAPAY ZEKAYA PLANLAT";
                    btnAiPlanla.SetEnabled(true);
                }
            };
        }

        SayfaDegistir(0);
    }

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

    public void AktifGorevSayisiniGuncelle() {
        ScrollView gorevListesi = GetComponent<UIDocument>().rootVisualElement.Q<ScrollView>("gorevlerScrollView");
        VisualElement pageGorev = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("Page_Gorevler");
        Label lblGorevSayisi = pageGorev?.Q<Label>(className: "header-status"); 
        if (lblGorevSayisi != null && gorevListesi != null) {
            int sayi = gorevListesi.childCount;
            lblGorevSayisi.text = sayi + " Aktif Görev";
        }
    }

    private void MakeDraggable(VisualElement card) {
        card.RegisterCallback<PointerDownEvent>(evt => {
            isDragging = true;
            draggedCard = card;
            originalIndex = card.parent.IndexOf(card);
            startMousePosition = evt.position;
            card.style.opacity = 0.7f;
            card.BringToFront();
            card.CapturePointer(evt.pointerId);
            
            // ScrollView kaymasını engellemek için event'in yukarı çıkmasını durdur
            evt.StopPropagation();
        });

        card.RegisterCallback<PointerMoveEvent>(evt => {
            if (isDragging && draggedCard == card) {
                float yFarki = evt.position.y - startMousePosition.y;
                card.style.translate = new StyleTranslate(new Translate(0, yFarki, 0));

                VisualElement parent = card.parent;
                int currentIndex = parent.IndexOf(card);
                int newIndex = currentIndex;

                for (int i = 0; i < parent.childCount; i++) {
                    var child = parent.ElementAt(i);
                    if (child != card) {
                        // Eğer sürüklenen kartın merkezi, diğer kartın sınırları içindeyse
                        if (card.worldBound.center.y > child.worldBound.yMin && 
                            card.worldBound.center.y < child.worldBound.yMax) {
                            newIndex = i;
                            break;
                        }
                    }
                }

                if (newIndex != currentIndex) {
                    parent.Insert(newIndex, card);
                    // Hiyerarşi değiştiğinde fiziksel zıplamayı önlemek için fare başlangıç pozisyonunu sıfırla
                    startMousePosition = evt.position;
                    card.style.translate = new StyleTranslate(new Translate(0, 0, 0));
                }
                
                evt.StopPropagation();
            }
        });

        card.RegisterCallback<PointerUpEvent>(evt => {
            if (!isDragging) return;
            isDragging = false;
            card.ReleasePointer(evt.pointerId);
            card.style.translate = new StyleTranslate(new Translate(0, 0, 0));
            card.style.opacity = 1f;
        });

        card.RegisterCallback<PointerCaptureOutEvent>(evt => {
            if (!isDragging) return;
            isDragging = false;
            card.style.translate = new StyleTranslate(new Translate(0, 0, 0));
            card.style.opacity = 1f;
        });
    }

    public void GorevKartiEkle(string gorevAdi, string sure = "", bool katiMi = false, bool kaydet = true) {
        ScrollView gorevListesi = GetComponent<UIDocument>().rootVisualElement.Q<ScrollView>("gorevlerScrollView");
        if(gorevListesi == null) return;

        VisualElement kart = new VisualElement();
        kart.AddToClassList("task-card");

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

        // Buton Konteyneri (Sağda, yan yana)
        VisualElement butonKutusu = new VisualElement();
        butonKutusu.style.flexDirection = FlexDirection.Row;
        butonKutusu.style.alignItems = Align.Center;
        butonKutusu.style.flexShrink = 0;

        // Tamamlandı Butonu
        Button btnTamamlandi = new Button();
        btnTamamlandi.text = "✓";
        btnTamamlandi.style.backgroundColor = new StyleColor(new Color(0.13f, 0.77f, 0.36f, 0.8f)); // Yarı saydam yeşil
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
        
        btnTamamlandi.clicked += () => { 
            kart.RemoveFromHierarchy(); 
            AktifGorevSayisiniGuncelle();
            
            ScheduleManager sManager = FindFirstObjectByType<ScheduleManager>();
            if (sManager != null) {
                sManager.RemoveTask(gorevAdi);
            }
        }; 

        // Devret Butonu
        Button btnDevret = new Button();
        btnDevret.text = "✖";
        btnDevret.style.backgroundColor = new StyleColor(new Color(0.93f, 0.26f, 0.26f, 0.8f)); // Yarı saydam kırmızı
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
            if (!gorevYazisi.text.StartsWith("[DEVRETTİ]")) {
                gorevYazisi.text = "[DEVRETTİ] " + gorevYazisi.text;
                gorevYazisi.style.color = new StyleColor(new Color(0.93f, 0.26f, 0.26f, 1f)); // Metni kırmızı yap
            }
            kart.style.borderLeftColor = new StyleColor(new Color(0.93f, 0.26f, 0.26f, 1f)); // Sol barı kırmızı yap
            kart.style.borderLeftWidth = 4; // Sol barı kalınlaştır
            btnDevret.style.display = DisplayStyle.None; // Tıklandıktan sonra gizle
        };

        // Sil Butonu
        Button btnSil = new Button();
        btnSil.text = "🗑";
        btnSil.style.backgroundColor = new StyleColor(new Color(0.33f, 0.33f, 0.33f, 1f)); // Koyu gri (#555555)
        btnSil.style.color = new StyleColor(Color.white);
        btnSil.style.width = 24; // Daha küçük
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
            if (sManager != null) {
                sManager.RemoveTask(gorevAdi);
            }
        };

        butonKutusu.Add(btnTamamlandi);
        butonKutusu.Add(btnDevret);
        butonKutusu.Add(btnSil);

        kart.Add(solPanel);
        kart.Add(butonKutusu);

        MakeDraggable(kart); // Kartı fiziksel olarak sürüklenebilir yap

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
