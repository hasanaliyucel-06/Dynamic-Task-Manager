using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

/// <summary>
/// Görev ekleme sihirbazı (2 adımlı popup) ve AI hedef planlama mantığı.
/// Sihirbaz adımları: 1) Süre girişi, 2) Saat girişi → Görev oluşturma.
/// AI planlama: Hedef + tarih aralığı → SiberAsistan üzerinden günlük görev üretme.
/// </summary>
public class SihirbazYonetici : MonoBehaviour
{
    private int sihirbazAdimi = 0;
    private string geciciGorevAdi = "";
    private int geciciSure = 0;
    private int secilenOncelik = 1;

    private GorevKartiYonetici gorevKartiYonetici;
    private TakvimYonetici takvimYonetici;
    private SiberAsistan _cachedAsistan;

    /// <summary>
    /// UI Toolkit root elementini ve diğer yönetici referanslarını alır.
    /// Sihirbaz ve AI planlama UI event'lerini bağlar.
    /// PageNavigator.OnEnable() tarafından çağrılır.
    /// </summary>
    public void Initialize(VisualElement root, GorevKartiYonetici gky, TakvimYonetici ty)
    {
        gorevKartiYonetici = gky;
        takvimYonetici = ty;
        _cachedAsistan = FindFirstObjectByType<SiberAsistan>();

        // Görev Sihirbazı UI elementleri
        TextField inputGorev = root.Q<TextField>("Input_YeniGorev");
        Button btnGorevEkle = root.Q<Button>("Btn_GorevEkle");
        VisualElement gorevSihirbaziOverlay = root.Q<VisualElement>("gorevSihirbaziOverlay");
        Label lblSihirbazBaslik = root.Q<Label>("lblSihirbazBaslik");
        TextField inputSihirbazDeger = root.Q<TextField>("inputSihirbazDeger");
        VisualElement timeInputContainer = root.Q<VisualElement>("timeInputContainer");
        TextField inputSaat = root.Q<TextField>("inputSaat");
        TextField inputDakika = root.Q<TextField>("inputDakika");
        Button btnSihirbazOnay = root.Q<Button>("btnSihirbazOnay");
        Button btnSihirbazIptal = root.Q<Button>("btnSihirbazIptal");
        Toggle toggleTekrarla = root.Q<Toggle>("toggleTekrarla");
        TextField inputKategori = root.Q<TextField>("inputKategori");
        TextField inputNotlar = root.Q<TextField>("inputNotlar");
        
        Button btnSihirbazOncelikDusuk = root.Q<Button>("btnSihirbazOncelikDusuk");
        Button btnSihirbazOncelikOrta = root.Q<Button>("btnSihirbazOncelikOrta");
        Button btnSihirbazOncelikYuksek = root.Q<Button>("btnSihirbazOncelikYuksek");

        void OncelikSec(int oncelik)
        {
            secilenOncelik = oncelik;
            if (btnSihirbazOncelikDusuk != null) btnSihirbazOncelikDusuk.style.opacity = oncelik == 0 ? 1f : 0.4f;
            if (btnSihirbazOncelikOrta != null) btnSihirbazOncelikOrta.style.opacity = oncelik == 1 ? 1f : 0.4f;
            if (btnSihirbazOncelikYuksek != null) btnSihirbazOncelikYuksek.style.opacity = oncelik == 2 ? 1f : 0.4f;
        }

        if (btnSihirbazOncelikDusuk != null) btnSihirbazOncelikDusuk.clicked += () => OncelikSec(0);
        if (btnSihirbazOncelikOrta != null) btnSihirbazOncelikOrta.clicked += () => OncelikSec(1);
        if (btnSihirbazOncelikYuksek != null) btnSihirbazOncelikYuksek.clicked += () => OncelikSec(2);

        // Input stilleri
        TextField[] allInputs = { inputSihirbazDeger, inputSaat, inputDakika, inputKategori, inputNotlar };
        foreach (var inp in allInputs)
        {
            if (inp != null)
            {
                VisualElement innerInput = inp.Q(className: "unity-text-field__input");
                if (innerInput != null)
                {
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

        // Input validasyonları
        if (inputSihirbazDeger != null)
        {
            inputSihirbazDeger.RegisterValueChangedCallback(evt => {
                if (string.IsNullOrEmpty(evt.newValue)) return;
                if (sihirbazAdimi == 1)
                {
                    inputSihirbazDeger.maxLength = 3;
                    string filtered = new string(evt.newValue.Where(char.IsDigit).ToArray());
                    if (filtered != evt.newValue)
                    {
                        inputSihirbazDeger.SetValueWithoutNotify(filtered);
                    }
                }
            });
        }

        if (inputSaat != null)
        {
            inputSaat.RegisterValueChangedCallback(evt => {
                if (string.IsNullOrEmpty(evt.newValue)) return;
                string filtered = new string(evt.newValue.Where(char.IsDigit).ToArray());
                if (filtered != evt.newValue) inputSaat.SetValueWithoutNotify(filtered);
                if (!string.IsNullOrEmpty(filtered) && int.TryParse(filtered, out int s))
                {
                    if (s > 23) inputSaat.SetValueWithoutNotify("23");
                }
            });
        }

        if (inputDakika != null)
        {
            inputDakika.RegisterValueChangedCallback(evt => {
                if (string.IsNullOrEmpty(evt.newValue)) return;
                string filtered = new string(evt.newValue.Where(char.IsDigit).ToArray());
                if (filtered != evt.newValue) inputDakika.SetValueWithoutNotify(filtered);
                if (!string.IsNullOrEmpty(filtered) && int.TryParse(filtered, out int d))
                {
                    if (d > 59) inputDakika.SetValueWithoutNotify("59");
                }
            });
        }

        // Görev ekleme butonu → Sihirbaz açma
        if (btnGorevEkle != null && inputGorev != null)
        {
            btnGorevEkle.clicked += () => {
                if (string.IsNullOrWhiteSpace(inputGorev.value)) return;

                geciciGorevAdi = inputGorev.value;
                sihirbazAdimi = 1;

                if (lblSihirbazBaslik != null) lblSihirbazBaslik.text = "Görev Süresi (Dk):";
                if (inputSihirbazDeger != null)
                {
                    inputSihirbazDeger.value = "";
                    inputSihirbazDeger.style.display = DisplayStyle.Flex;
                }
                if (timeInputContainer != null) timeInputContainer.style.display = DisplayStyle.None;
                if (btnSihirbazOnay != null) btnSihirbazOnay.text = "İLERİ";
                OncelikSec(1);
                if (inputNotlar != null) inputNotlar.value = "";
                if (gorevSihirbaziOverlay != null) gorevSihirbaziOverlay.style.display = DisplayStyle.Flex;

                inputGorev.value = ""; // Kutuyu temizle
            };
        }

        // İptal butonu
        if (btnSihirbazIptal != null && gorevSihirbaziOverlay != null)
        {
            btnSihirbazIptal.clicked += () => {
                gorevSihirbaziOverlay.style.display = DisplayStyle.None;
                if (inputSihirbazDeger != null)
                {
                    inputSihirbazDeger.value = "";
                    inputSihirbazDeger.style.display = DisplayStyle.Flex;
                }
                if (timeInputContainer != null) timeInputContainer.style.display = DisplayStyle.None;
                if (inputSaat != null) inputSaat.value = "";
                if (inputDakika != null) inputDakika.value = "";
                if (inputKategori != null) inputKategori.value = "Genel";
                if (inputNotlar != null) inputNotlar.value = "";
                sihirbazAdimi = 0;
            };
        }

        // Onay butonu (2 adımlı akış)
        if (btnSihirbazOnay != null && inputSihirbazDeger != null)
        {
            btnSihirbazOnay.clicked += () => {
                if (sihirbazAdimi == 1)
                {
                    if (int.TryParse(inputSihirbazDeger.value, out geciciSure))
                    {
                        sihirbazAdimi = 2;
                        if (lblSihirbazBaslik != null) lblSihirbazBaslik.text = "Başlangıç Saati:";
                        inputSihirbazDeger.style.display = DisplayStyle.None;
                        if (timeInputContainer != null) timeInputContainer.style.display = DisplayStyle.Flex;
                        if (btnSihirbazOnay != null) btnSihirbazOnay.text = "TAMAMLA";
                        if (inputSaat != null) inputSaat.Focus();
                    }
                }
                else if (sihirbazAdimi == 2)
                {
                    // Saat ve Dakikayı al (boşlarsa 0 varsay)
                    int saat = (inputSaat != null && !string.IsNullOrEmpty(inputSaat.value)) ? int.Parse(inputSaat.value) : 0;
                    int dakika = (inputDakika != null && !string.IsNullOrEmpty(inputDakika.value)) ? int.Parse(inputDakika.value) : 0;

                    // SADECE Başlangıç saatini (HH:MM formatında) birleştir
                    string zamanMetni = $"{saat:D2}:{dakika:D2}";

                    // Görevi ekle
                    string bugununTarihi = System.DateTime.Now.ToString("dd.MM.yyyy");
                    bool isRepeating = false;
                    if (toggleTekrarla != null) isRepeating = toggleTekrarla.value;

                    string kategori = (inputKategori != null && !string.IsNullOrEmpty(inputKategori.value)) ? inputKategori.value : "Genel";
                    string notlar = inputNotlar != null ? inputNotlar.value : "";

                    if (gorevKartiYonetici != null)
                    {
                        gorevKartiYonetici.GorevKartiEkle(bugununTarihi, zamanMetni, geciciGorevAdi, geciciSure.ToString(), false, true, isRepeating, false, kategori, secilenOncelik, notlar);
                    }

                    // Sihirbazı sıfırla ve kapat
                    if (gorevSihirbaziOverlay != null) gorevSihirbaziOverlay.style.display = DisplayStyle.None;
                    if (inputSihirbazDeger != null) inputSihirbazDeger.style.display = DisplayStyle.Flex;
                    if (timeInputContainer != null) timeInputContainer.style.display = DisplayStyle.None;
                    if (inputSihirbazDeger != null) inputSihirbazDeger.value = "";
                    if (inputSaat != null) inputSaat.value = "";
                    if (inputDakika != null) inputDakika.value = "";
                    if (inputKategori != null) inputKategori.value = "Genel";
                    if (inputNotlar != null) inputNotlar.value = "";
                    if (toggleTekrarla != null) toggleTekrarla.value = false;
                    sihirbazAdimi = 0;
                }
            };
        }

        // AI Hedef Planlama (Disiplin sayfası)
        AIPlanlamaKur(root);
    }

    /// <summary>
    /// Disiplin sayfasındaki "Yapay Zekaya Planlat" butonunun event'ini bağlar.
    /// </summary>
    private void AIPlanlamaKur(VisualElement root)
    {
        TextField inputHedef = root.Q<TextField>("inputHedef");
        Button btnAiPlanla = root.Q<Button>("btnAiPlanla");

        if (btnAiPlanla != null && inputHedef != null)
        {
            btnAiPlanla.clicked += () => {
                if (string.IsNullOrWhiteSpace(inputHedef.value)) return;

                if (!takvimYonetici.SecilenBaslangic.HasValue || !takvimYonetici.SecilenBitis.HasValue)
                {
                    Debug.LogWarning("Başlangıç ve Bitiş tarihi seçilmedi!");
                    return;
                }

                int kalanGun = (int)(takvimYonetici.SecilenBitis.Value.Date - takvimYonetici.SecilenBaslangic.Value.Date).TotalDays;

                if (kalanGun < 0)
                {
                    Debug.LogWarning("Bitiş tarihi, Başlangıç tarihinden önce olamaz!");
                    return;
                }

                btnAiPlanla.SetEnabled(false);
                btnAiPlanla.text = "PLANLANIYOR...";

                string hedefAdi = inputHedef.value; // Closure için yakala

                string gizliPrompt = $"Kullanıcının '{hedefAdi}' hedefine ulaşması için {kalanGun} günü var. BUGÜN yapması gereken tek bir spesifik görev üret. Yanıtın KESİNLİKLE sadece şu formatta olmalı: [HEDEF:SAAT:Görev Adı:SÜRE]. SAAT kısmı HH:MM formatında (örn: 09:00) olmalıdır. SÜRE kısmı SADECE ve SADECE rakamlardan (örn: 30) oluşmalıdır. Hiçbir metin ekleme!";

                if (_cachedAsistan == null) _cachedAsistan = FindFirstObjectByType<SiberAsistan>();
                if (_cachedAsistan != null)
                {
                    _cachedAsistan.GizliSorguYap(gizliPrompt, (cevap) => {
                        string temizCevap = cevap.Trim();
                        temizCevap = temizCevap.Replace("[HEDEF:", "").Replace("]", "");

                        string[] parcalar = temizCevap.Split(':');
                        if (parcalar.Length >= 3)
                        {
                            string saat = $"{parcalar[0].Trim()}:{parcalar[1].Trim()}";
                            string gelenSure = parcalar[parcalar.Length - 1].Trim();
                            string sadeceRakamlar = System.Text.RegularExpressions.Regex.Match(gelenSure, @"\d+").Value;
                            if (string.IsNullOrEmpty(sadeceRakamlar))
                            {
                                sadeceRakamlar = "30";
                            }

                            string gorevAdiParsed = string.Join(":", parcalar, 2, parcalar.Length - 3).Trim();

                            string bugun = System.DateTime.Now.ToString("dd.MM.yyyy");
                            if (gorevKartiYonetici != null)
                            {
                                gorevKartiYonetici.GorevKartiEkle(bugun, saat, gorevAdiParsed, sadeceRakamlar, false);
                            }

                            // Hedefi aktif hedefler listesine ekle
                            UzunVadeliHedef yeniHedef = new UzunVadeliHedef();
                            yeniHedef.hedefAdi = hedefAdi;
                            yeniHedef.kalanGun = kalanGun;
                            yeniHedef.baslangicTarihi = takvimYonetici.SecilenBaslangic.Value.ToString("dd.MM.yyyy");
                            takvimYonetici.aktifHedefler.Add(yeniHedef);

                            HedefListesiWrapper wrapper = new HedefListesiWrapper();
                            wrapper.hedefler = takvimYonetici.aktifHedefler;
                            takvimYonetici.HedefleriKaydet();
                        }
                        else
                        {
                            Debug.LogWarning("AI Planlama formatı anlaşılamadı (Eksik parametre): " + cevap);
                        }

                        inputHedef.value = "";
                        takvimYonetici.SecilenBaslangic = null;
                        takvimYonetici.SecilenBitis = null;
                        takvimYonetici.HedefleriYukle();
                        takvimYonetici.TakvimGuncelle();
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
    }
}
