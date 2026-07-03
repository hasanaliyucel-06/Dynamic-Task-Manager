using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

public class PageNavigator : MonoBehaviour
{
    public System.Collections.Generic.List<UzunVadeliHedef> aktifHedefler = new System.Collections.Generic.List<UzunVadeliHedef>();



    // Takvim Değişkenleri
    // Takvim Değişkenleri
    private System.DateTime? secilenBaslangic = null;
    private System.DateTime? secilenBitis = null;

    // Sihirbaz Değişkenleri
    private int sihirbazAdimi = 0;
    private string geciciGorevAdi = "";
    private int geciciSure = 0;


    
    private int bugunToplamGorev = 0;
    private int bugunTamamlananGorev = 0;
    private VisualElement progressBarFill;

    private void GuncelleProgressBar() {
        if (progressBarFill == null) return;
        float yuzde = bugunToplamGorev > 0 ? ((float)bugunTamamlananGorev / bugunToplamGorev) * 100f : 0f;
        progressBarFill.style.width = new Length(yuzde, LengthUnit.Percent);
    }

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

        progressBarFill = root.Q<VisualElement>("progressBarFill");

        
        // İlk durum güncellemesi
        GuncelleProgressBar();

        if (btnSohbet != null) btnSohbet.clicked += () => SayfaDegistir(0);
        if (btnGorevler != null) btnGorevler.clicked += () => SayfaDegistir(1);
        if (btnDisiplin != null) btnDisiplin.clicked += () => SayfaDegistir(2);
        if (btnSistem != null) btnSistem.clicked += () => SayfaDegistir(3);

        TextField inputGorev = root.Q<TextField>("Input_YeniGorev");
        Button btnGorevEkle = root.Q<Button>("Btn_GorevEkle");
        ScrollView gorevListesi = root.Q<ScrollView>("gorevlerScrollView");

        VisualElement gorevSihirbaziOverlay = root.Q<VisualElement>("gorevSihirbaziOverlay");
        Label lblSihirbazBaslik = root.Q<Label>("lblSihirbazBaslik");
        TextField inputSihirbazDeger = root.Q<TextField>("inputSihirbazDeger");
        VisualElement timeInputContainer = root.Q<VisualElement>("timeInputContainer");
        TextField inputSaat = root.Q<TextField>("inputSaat");
        TextField inputDakika = root.Q<TextField>("inputDakika");
        
        TextField[] allInputs = { inputSihirbazDeger, inputSaat, inputDakika };
        foreach(var inp in allInputs) {
            if (inp != null) {
                VisualElement innerInput = inp.Q(className: "unity-text-field__input");
                if (innerInput != null) {
                    innerInput.style.backgroundColor = new StyleColor(new Color(0.11f, 0.14f, 0.20f));
                    innerInput.style.color = new StyleColor(Color.white);
                    innerInput.style.borderTopColor = new StyleColor(Color.cyan);
                    innerInput.style.borderBottomColor = new StyleColor(Color.cyan);
                    innerInput.style.borderLeftColor = new StyleColor(Color.cyan);
                    innerInput.style.borderRightColor = new StyleColor(Color.cyan);
                    innerInput.style.borderTopWidth = 1;
                    innerInput.style.borderBottomWidth = 1;
                    innerInput.style.borderLeftWidth = 1;
                    innerInput.style.borderRightWidth = 1;
                    innerInput.style.borderTopLeftRadius = 6;
                    innerInput.style.borderTopRightRadius = 6;
                    innerInput.style.borderBottomLeftRadius = 6;
                    innerInput.style.borderBottomRightRadius = 6;
                }
            }
        }

        if (inputSihirbazDeger != null) {
            inputSihirbazDeger.RegisterValueChangedCallback(evt => {
                if (string.IsNullOrEmpty(evt.newValue)) return;
                if (sihirbazAdimi == 1) {
                    inputSihirbazDeger.maxLength = 3; 
                    string filtered = new string(evt.newValue.Where(char.IsDigit).ToArray());
                    if (filtered != evt.newValue) {
                        inputSihirbazDeger.SetValueWithoutNotify(filtered);
                    }
                }
            });
        }
        
        if (inputSaat != null) {
            inputSaat.RegisterValueChangedCallback(evt => {
                if (string.IsNullOrEmpty(evt.newValue)) return;
                string filtered = new string(evt.newValue.Where(char.IsDigit).ToArray());
                if (filtered != evt.newValue) {
                    inputSaat.SetValueWithoutNotify(filtered);
                }
                if (!string.IsNullOrEmpty(filtered) && int.TryParse(filtered, out int s)) {
                    if (s > 23) inputSaat.SetValueWithoutNotify("23");
                }
            });
        }

        if (inputDakika != null) {
            inputDakika.RegisterValueChangedCallback(evt => {
                if (string.IsNullOrEmpty(evt.newValue)) return;
                string filtered = new string(evt.newValue.Where(char.IsDigit).ToArray());
                if (filtered != evt.newValue) {
                    inputDakika.SetValueWithoutNotify(filtered);
                }
                if (!string.IsNullOrEmpty(filtered) && int.TryParse(filtered, out int d)) {
                    if (d > 59) inputDakika.SetValueWithoutNotify("59");
                }
            });
        }
        Button btnSihirbazOnay = root.Q<Button>("btnSihirbazOnay");
        Button btnSihirbazIptal = root.Q<Button>("btnSihirbazIptal");

        if (btnGorevEkle != null && inputGorev != null)
        {
            btnGorevEkle.clicked += () => {
                if (string.IsNullOrWhiteSpace(inputGorev.value)) return;

                geciciGorevAdi = inputGorev.value;
                sihirbazAdimi = 1;
                
                if(lblSihirbazBaslik != null) lblSihirbazBaslik.text = "Görev Süresi (Dk):";
                if(inputSihirbazDeger != null) {
                    inputSihirbazDeger.value = "";
                    inputSihirbazDeger.style.display = DisplayStyle.Flex;
                }
                if(timeInputContainer != null) timeInputContainer.style.display = DisplayStyle.None;
                
                if(btnSihirbazOnay != null) btnSihirbazOnay.text = "İLERİ";
                if(gorevSihirbaziOverlay != null) gorevSihirbaziOverlay.style.display = DisplayStyle.Flex;
                
                inputGorev.value = ""; // Kutuyu temizle
            };
        }

        if (btnSihirbazIptal != null && gorevSihirbaziOverlay != null) {
            btnSihirbazIptal.clicked += () => {
                gorevSihirbaziOverlay.style.display = DisplayStyle.None;
                if(inputSihirbazDeger != null) {
                    inputSihirbazDeger.value = "";
                    inputSihirbazDeger.style.display = DisplayStyle.Flex;
                }
                if(timeInputContainer != null) timeInputContainer.style.display = DisplayStyle.None;
                if(inputSaat != null) inputSaat.value = "";
                if(inputDakika != null) inputDakika.value = "";
                sihirbazAdimi = 0;
            };
        }

        if (btnSihirbazOnay != null && inputSihirbazDeger != null) {
            btnSihirbazOnay.clicked += () => {
                if (sihirbazAdimi == 1) {
                    if (int.TryParse(inputSihirbazDeger.value, out geciciSure)) {
                        sihirbazAdimi = 2;
                        if(lblSihirbazBaslik != null) lblSihirbazBaslik.text = "Başlangıç Saati:";
                        inputSihirbazDeger.style.display = DisplayStyle.None;
                        if(timeInputContainer != null) timeInputContainer.style.display = DisplayStyle.Flex;
                        if(btnSihirbazOnay != null) btnSihirbazOnay.text = "TAMAMLA";
                        if(inputSaat != null) inputSaat.Focus();
                    }
                }
                else if (sihirbazAdimi == 2) {
                    // Saat ve Dakikayı al (boşlarsa 0 varsay)
                    int saat = (inputSaat != null && !string.IsNullOrEmpty(inputSaat.value)) ? int.Parse(inputSaat.value) : 0;
                    int dakika = (inputDakika != null && !string.IsNullOrEmpty(inputDakika.value)) ? int.Parse(inputDakika.value) : 0;
                    
                    // SADECE Başlangıç saatini (HH:MM formatında) birleştir
                    string zamanMetni = $"{saat:D2}:{dakika:D2}";
                    
                    // Görevi ekle
                    string bugununTarihi = System.DateTime.Now.ToString("dd.MM.yyyy");
                    GorevKartiEkle(bugununTarihi, zamanMetni, geciciGorevAdi, geciciSure.ToString(), false);
                    
                    // Sihirbazı sıfırla ve kapat
                    if(gorevSihirbaziOverlay != null) gorevSihirbaziOverlay.style.display = DisplayStyle.None;
                    if(inputSihirbazDeger != null) inputSihirbazDeger.style.display = DisplayStyle.Flex; 
                    if(timeInputContainer != null) timeInputContainer.style.display = DisplayStyle.None;
                    if(inputSihirbazDeger != null) inputSihirbazDeger.value = "";
                    if(inputSaat != null) inputSaat.value = "";
                    if(inputDakika != null) inputDakika.value = "";
                    sihirbazAdimi = 0;
                }
            };
        }

        TextField inputHedef = root.Q<TextField>("inputHedef");
        Button btnAiPlanla = root.Q<Button>("btnAiPlanla");

        VisualElement takvimGunler = root.Q<VisualElement>("takvimGunleriWrap");
        Label lblTakvimAyYil = root.Q<Label>("lblTakvimAyYil");
        Button btnTakvimOnceki = root.Q<Button>("btnTakvimOnceki");
        Button btnTakvimSonraki = root.Q<Button>("btnTakvimSonraki");

        System.DateTime gosterilenAy = new System.DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, 1);

        System.Action TakvimGuncelle = null;
        TakvimGuncelle = () => {
            if (takvimGunler == null) return;
            takvimGunler.Clear();
            if (lblTakvimAyYil != null) lblTakvimAyYil.text = gosterilenAy.ToString("MMMM yyyy");
            
            int gunSayisi = System.DateTime.DaysInMonth(gosterilenAy.Year, gosterilenAy.Month);
            
            int firstDayOfWeek = (int)new System.DateTime(gosterilenAy.Year, gosterilenAy.Month, 1).DayOfWeek;
            int boslukSayisi = (firstDayOfWeek == 0) ? 6 : firstDayOfWeek - 1;
            
            for (int b = 0; b < boslukSayisi; b++) {
                VisualElement bosluk = new VisualElement();
                bosluk.style.width = 35;
                bosluk.style.height = 35;
                bosluk.style.marginRight = 2;
                bosluk.style.marginBottom = 2;
                takvimGunler.Add(bosluk);
            }

            for (int i = 1; i <= gunSayisi; i++) {
                int g = i;
                System.DateTime iterasyonTarihi = new System.DateTime(gosterilenAy.Year, gosterilenAy.Month, g);
                
                Button gunBtn = new Button();
                gunBtn.text = g.ToString();
                gunBtn.style.width = 35;
                gunBtn.style.height = 35;
                gunBtn.style.marginRight = 2;
                gunBtn.style.marginBottom = 2;
                gunBtn.style.borderTopWidth = 0;
                gunBtn.style.borderBottomWidth = 0;
                gunBtn.style.borderLeftWidth = 0;
                gunBtn.style.borderRightWidth = 0;
                gunBtn.style.paddingLeft = 0;
                gunBtn.style.paddingRight = 0;
                gunBtn.style.paddingTop = 0;
                gunBtn.style.paddingBottom = 0;
                
                // Varsayılan Stil
                gunBtn.style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f));
                gunBtn.style.color = new StyleColor(Color.white);
                
                // Seçim Durumuna Göre Stil
                bool isBaslangic = secilenBaslangic.HasValue && secilenBaslangic.Value.Date == iterasyonTarihi.Date;
                bool isBitis = secilenBitis.HasValue && secilenBitis.Value.Date == iterasyonTarihi.Date;
                bool isArada = secilenBaslangic.HasValue && secilenBitis.HasValue && 
                               iterasyonTarihi.Date > secilenBaslangic.Value.Date && 
                               iterasyonTarihi.Date < secilenBitis.Value.Date;

                if (isBaslangic || isBitis) {
                    gunBtn.style.backgroundColor = new StyleColor(new Color(0.05f, 0.65f, 0.91f)); // Neon Mavi
                    gunBtn.style.color = new StyleColor(Color.white);
                } else if (isArada) {
                    gunBtn.style.backgroundColor = new StyleColor(new Color(0.05f, 0.65f, 0.91f, 0.3f)); // Yarı saydam neon mavi
                } else if (iterasyonTarihi.Date == System.DateTime.Now.Date) {
                    gunBtn.style.borderTopWidth = 1;
                    gunBtn.style.borderBottomWidth = 1;
                    gunBtn.style.borderLeftWidth = 1;
                    gunBtn.style.borderRightWidth = 1;
                    gunBtn.style.borderTopColor = new StyleColor(Color.gray);
                    gunBtn.style.borderBottomColor = new StyleColor(Color.gray);
                    gunBtn.style.borderLeftColor = new StyleColor(Color.gray);
                    gunBtn.style.borderRightColor = new StyleColor(Color.gray);
                }

                // Marker Ekleme
                bool hasMarker = false;
                if (aktifHedefler != null) {
                    foreach(var hedef in aktifHedefler) {
                        System.DateTime hBaslangic;
                        if(System.DateTime.TryParseExact(hedef.baslangicTarihi, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out hBaslangic)) {
                            System.DateTime hBitis = hBaslangic.AddDays(hedef.kalanGun);
                            if(iterasyonTarihi.Date == hBaslangic.Date || iterasyonTarihi.Date == hBitis.Date) {
                                hasMarker = true;
                                break;
                            }
                        }
                    }
                }

                if(hasMarker) {
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
                    marker.style.backgroundColor = new StyleColor(new Color(0.97f, 0.45f, 0.08f)); // Turuncu/Kırmızı
                    gunBtn.Add(marker);
                }

                gunBtn.clicked += () => {
                    if (!secilenBaslangic.HasValue || (secilenBaslangic.HasValue && secilenBitis.HasValue)) {
                        secilenBaslangic = iterasyonTarihi;
                        secilenBitis = null;
                    } else if (secilenBaslangic.HasValue && !secilenBitis.HasValue) {
                        if (iterasyonTarihi.Date >= secilenBaslangic.Value.Date) {
                            secilenBitis = iterasyonTarihi;
                        } else if (iterasyonTarihi.Date < secilenBaslangic.Value.Date) {
                            secilenBaslangic = iterasyonTarihi;
                        }
                    }
                    TakvimGuncelle();
                };
                
                takvimGunler.Add(gunBtn);
            }
        };

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

        TakvimGuncelle();

        if (btnAiPlanla != null && inputHedef != null)
        {
            btnAiPlanla.clicked += () => {
                if (string.IsNullOrWhiteSpace(inputHedef.value)) return;
                
                if (!secilenBaslangic.HasValue || !secilenBitis.HasValue) {
                    Debug.LogWarning("Başlangıç ve Bitiş tarihi seçilmedi!");
                    return;
                }

                int kalanGun = (int)(secilenBitis.Value.Date - secilenBaslangic.Value.Date).TotalDays;

                if (kalanGun < 0) {
                    Debug.LogWarning("Bitiş tarihi, Başlangıç tarihinden önce olamaz!");
                    return;
                }

                btnAiPlanla.SetEnabled(false);
                btnAiPlanla.text = "PLANLANIYOR...";

                string gizliPrompt = $"Kullanıcının '{inputHedef.value}' hedefine ulaşması için {kalanGun} günü var. BUGÜN yapması gereken tek bir spesifik görev üret. Yanıtın KESİNLİKLE sadece şu formatta olmalı: [HEDEF:SAAT:Görev Adı:SÜRE]. SAAT kısmı HH:MM formatında (örn: 09:00) olmalıdır. SÜRE kısmı SADECE ve SADECE rakamlardan (örn: 30) oluşmalıdır. Hiçbir metin ekleme!";

                SiberAsistan asistan = FindFirstObjectByType<SiberAsistan>();
                if (asistan != null)
                {
                    asistan.GizliSorguYap(gizliPrompt, (cevap) => {
                        string temizCevap = cevap.Trim();
                        temizCevap = temizCevap.Replace("[HEDEF:", "").Replace("]", "");
                        
                        string[] parcalar = temizCevap.Split(':');
                        if (parcalar.Length >= 3)
                        {
                            string saat = $"{parcalar[0].Trim()}:{parcalar[1].Trim()}";
                            string gelenSure = parcalar[parcalar.Length - 1].Trim();
                            string sadeceRakamlar = System.Text.RegularExpressions.Regex.Match(gelenSure, @"\d+").Value;
                            if (string.IsNullOrEmpty(sadeceRakamlar)) {
                                sadeceRakamlar = "30"; 
                            }

                            string gorevAdi = string.Join(":", parcalar, 2, parcalar.Length - 3).Trim();
                            
                            string bugun = System.DateTime.Now.ToString("dd.MM.yyyy");
                            GorevKartiEkle(bugun, saat, gorevAdi, sadeceRakamlar, false);

                            UzunVadeliHedef yeniHedef = new UzunVadeliHedef();
                            yeniHedef.hedefAdi = inputHedef.value;
                            yeniHedef.kalanGun = kalanGun;
                            yeniHedef.baslangicTarihi = secilenBaslangic.Value.ToString("dd.MM.yyyy");
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
                        secilenBaslangic = null;
                        secilenBitis = null;
                        TakvimGuncelle();
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
        // Sürükle-bırak motoru iptal edildi.
    }

    public void GorevKartiEkle(string tarih, string saat, string gorevAdi, string sure = "", bool katiMi = false, bool kaydet = true) {
        bugunToplamGorev++;
        GuncelleProgressBar();
        
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

        // Görev Saati (Timeblocking)
        Label lblGorevSaati = new Label(saat);
        lblGorevSaati.name = "lblGorevSaati";
        lblGorevSaati.style.color = new StyleColor(new Color(0f, 1f, 1f)); // Neon Camgöbeği #00FFFF
        lblGorevSaati.style.fontSize = 14;
        lblGorevSaati.style.unityFontStyleAndWeight = FontStyle.Bold;
        lblGorevSaati.style.marginBottom = 2;
        solPanel.Add(lblGorevSaati);

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
        
        bool isCompleted = false;
        
        btnTamamlandi.clicked += () => { 
            if (!isCompleted) {
                bugunTamamlananGorev++;
                isCompleted = true;
                kart.style.opacity = 0.5f;
                btnTamamlandi.style.backgroundColor = new StyleColor(Color.gray);
                GuncelleProgressBar();
                
                AktifGorevSayisiniGuncelle();
                
                ScheduleManager sManager = FindFirstObjectByType<ScheduleManager>();
                if (sManager != null) {
                    sManager.RemoveTask(gorevAdi);
                }
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



        System.TimeSpan yeniSaatTS;
        string parslanacakSaat = saat;
        if (parslanacakSaat.Length >= 5 && parslanacakSaat.Contains("-")) {
            parslanacakSaat = parslanacakSaat.Substring(0, 5).Trim();
        }
        bool isParsed = System.TimeSpan.TryParse(parslanacakSaat, out yeniSaatTS);
        
        int insertIndex = gorevListesi.childCount; // Varsayılan olarak en sona ekle
        
        if (isParsed) {
            for (int i = 0; i < gorevListesi.childCount; i++) {
                VisualElement sibling = gorevListesi.ElementAt(i);
                Label lblSibling = sibling.Q<Label>("lblGorevSaati");
                if (lblSibling != null) {
                    string sibSaat = lblSibling.text;
                    if (sibSaat.Length >= 5 && sibSaat.Contains("-")) {
                        sibSaat = sibSaat.Substring(0, 5).Trim();
                    }
                    System.TimeSpan siblingTS;
                    if (System.TimeSpan.TryParse(sibSaat, out siblingTS)) {
                        if (yeniSaatTS < siblingTS) {
                            insertIndex = i;
                            break;
                        }
                    }
                }
            }
        }
        
        gorevListesi.Insert(insertIndex, kart); // Kronolojik sıraya göre ekle
        AktifGorevSayisiniGuncelle();

        if (kaydet) {
            ScheduleManager sManager = FindFirstObjectByType<ScheduleManager>();
            if (sManager != null) {
                int sureInt = 0;
                int.TryParse(sure, out sureInt);
                sManager.AddTask(tarih, saat, gorevAdi, sureInt, katiMi);
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
