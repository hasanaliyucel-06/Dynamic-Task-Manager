using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

// SohbetMesaji ve SohbetGecmisi artık Models/VeriModelleri.cs dosyasında tanımlıdır.

public class ModernAsistanBaglantisi : MonoBehaviour
{
    private UIDocument uiDocument;
    private Button gonderButonu;
    private bool isRequestActive = false;
    private TextField mesajGirdisi;
    private ScrollView scrollSohbet;
    private Label lblStatus;
    private VisualElement inputContainer;
    private VisualElement emptySohbet;
    private TextField inputSohbetArama;

    public SiberAsistan siberAsistan; 

    private SohbetGecmisi sohbetGecmisi = new SohbetGecmisi();

    void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null) return;

        var root = uiDocument.rootVisualElement;

        gonderButonu = root.Q<Button>("btnGonder");
        mesajGirdisi = root.Q<TextField>("txtMesaj");
        scrollSohbet = root.Q<ScrollView>("scrollSohbet");
        lblStatus = root.Q<Label>(className: "header-status");
        inputContainer = root.Q<VisualElement>(className: "bottom-bar");
        emptySohbet = root.Q<VisualElement>("emptySohbet");

        if (gonderButonu != null)
        {
            gonderButonu.clicked -= MesajGonderildi;
            gonderButonu.clicked += MesajGonderildi;
        }

        inputSohbetArama = root.Q<TextField>("inputSohbetArama");
        if (inputSohbetArama != null)
        {
            inputSohbetArama.RegisterValueChangedCallback(evt => {
                SohbetiFiltrele(evt.newValue);
            });
        }

        SohbetGecmisiniYukle();
    }

    private void SohbetGecmisiniYukle()
    {
        string path = Path.Combine(Application.persistentDataPath, "chat.json");
        string json = "";

        if (PlayerPrefs.HasKey("SohbetGecmisi"))
        {
            json = PlayerPrefs.GetString("SohbetGecmisi");
            try
            {
                File.WriteAllText(path, json);
                PlayerPrefs.DeleteKey("SohbetGecmisi");
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
            sohbetGecmisi = JsonUtility.FromJson<SohbetGecmisi>(json);
        }
        
        if (sohbetGecmisi == null) sohbetGecmisi = new SohbetGecmisi();

        if (sohbetGecmisi.mesajlar != null)
        {
            foreach (var msg in sohbetGecmisi.mesajlar)
            {
                EkranaMesajBas(msg.metin, msg.kullaniciMi, false);
            }
        }
    }

    private void SohbetiFiltrele(string query)
    {
        if (scrollSohbet == null) return;
        
        string kucukQuery = query?.ToLower() ?? "";
        
        foreach (var child in scrollSohbet.Children())
        {
            // Typing indicator'ı gizleme
            if (child.name == "typingIndicator") continue;

            if (child is Label lbl)
            {
                if (string.IsNullOrEmpty(kucukQuery) || lbl.text.ToLower().Contains(kucukQuery))
                {
                    child.style.display = DisplayStyle.Flex;
                }
                else
                {
                    child.style.display = DisplayStyle.None;
                }
            }
        }
    }

    private void SohbetiKaydet()
    {
        // En fazla 50 mesaj tut (bellek taşmasını önle)
        if (sohbetGecmisi.mesajlar.Count > 50)
        {
            sohbetGecmisi.mesajlar.RemoveRange(0, sohbetGecmisi.mesajlar.Count - 50);
        }
        string json = JsonUtility.ToJson(sohbetGecmisi, true);
        string path = Path.Combine(Application.persistentDataPath, "chat.json");
        try
        {
            File.WriteAllText(path, json);
        }
        catch (System.Exception e)
        {
            Debug.LogError("[Sohbet] Kayıt hatası: " + e.Message);
        }
    }

    void Update()
    {
        if (inputContainer == null) return;

        if (TouchScreenKeyboard.visible)
        {
            float keyboardHeight = TouchScreenKeyboard.area.height;
            inputContainer.style.marginBottom = keyboardHeight;
        }
        else
        {
            inputContainer.style.marginBottom = 0;
        }
    }

    void MesajGonderildi()
    {
        if (isRequestActive || mesajGirdisi == null || string.IsNullOrWhiteSpace(mesajGirdisi.value)) return;

        isRequestActive = true;
        if (gonderButonu != null) gonderButonu.SetEnabled(false);

        string gidenMesaj = mesajGirdisi.value;
        Debug.Log("Antigravity: Gönderilen Mesaj: " + gidenMesaj);

        if (siberAsistan != null)
        {
            siberAsistan.ModernArayuzdenMesajAl(gidenMesaj);
        }

        // Gönderdikten sonra kutuyu temizle
        mesajGirdisi.value = "";
    }

    public void EkranaMesajBas(string metin, bool kullaniciMi, bool kaydet = true)
    {
        if (!kullaniciMi) {
            isRequestActive = false;
            if (gonderButonu != null) gonderButonu.SetEnabled(true);
            
            // Yeni gelen bir asistan mesajıysa ses çal
            if (kaydet && SesYonetici.Instance != null)
            {
                SesYonetici.Instance.PlayMessageReceived();
            }
        }

        if (scrollSohbet == null) return;

        if (emptySohbet != null)
        {
            emptySohbet.style.display = DisplayStyle.None;
        }

        Label label = new Label();
        label.text = MarkdownToRichText(metin);
        label.enableRichText = true;
        label.AddToClassList("mesaj-balonu");

        // 1. Tüm mesajlar için ortak stiller
        label.style.color = new StyleColor(Color.white);
        label.style.paddingTop = 15; label.style.paddingBottom = 15;
        label.style.paddingLeft = 20; label.style.paddingRight = 20;
        label.style.borderTopLeftRadius = 15; label.style.borderTopRightRadius = 15;
        label.style.borderBottomLeftRadius = 15; label.style.borderBottomRightRadius = 15;
        label.style.marginBottom = 10;
        label.style.whiteSpace = WhiteSpace.Normal;
        
        if (kullaniciMi)
        {
            label.AddToClassList("mesaj-kullanici");
            // Kullanıcı (Sen) stili — backgroundColor artık USS'den geliyor
            label.style.borderBottomRightRadius = 0;
            label.style.alignSelf = Align.FlexEnd;
        }
        else
        {
            label.AddToClassList("mesaj-asistan");
            // Asistan stili — backgroundColor artık USS'den geliyor
            label.style.borderBottomLeftRadius = 0;
            label.style.alignSelf = Align.FlexStart;
        }

        scrollSohbet.Add(label);

        // UI Toolkit'in elementi çizmesini bekleyip (50ms) en alta kaydırır
        scrollSohbet.schedule.Execute(() => {
            scrollSohbet.ScrollTo(label);
        }).StartingIn(50);

        if (kaydet)
        {
            SohbetMesaji yeniMesaj = new SohbetMesaji();
            yeniMesaj.metin = metin;
            yeniMesaj.kullaniciMi = kullaniciMi;
            sohbetGecmisi.mesajlar.Add(yeniMesaj);
            SohbetiKaydet();
        }
    }

    private string MarkdownToRichText(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        
        // Kalın: **text** -> <b>text</b>
        text = Regex.Replace(text, @"\*\*(.*?)\*\*", "<b>$1</b>");
        // İtalik: *text* -> <i>text</i> (başında/sonunda boşluk veya karakter başı/sonu olacak şekilde)
        text = Regex.Replace(text, @"(?<=^|\s)\*(.*?)\*(?=\s|$)", "<i>$1</i>");
        
        return text;
    }

    // Typing indicator referansı
    private VisualElement typingIndicator;
    private IVisualElementScheduledItem typingAnimTimer;

    public void DurumYaziyorYap() 
    { 
        if(lblStatus != null) 
        { 
            lblStatus.text = "Yazıyor..."; 
            lblStatus.style.color = new StyleColor(Color.yellow); 
        }

        // Typing indicator göster
        GosterTypingIndicator();
    }

    public void DurumCevrimiciYap() {
        if (gonderButonu != null) gonderButonu.SetEnabled(true);

        if(lblStatus != null) {
            lblStatus.text = "Çevrimiçi";
            Color c;
            if (ColorUtility.TryParseHtmlString("#10B981", out c)) {
                lblStatus.style.color = new StyleColor(c);
            } else {
                lblStatus.style.color = new StyleColor(Color.green);
            }
        }

        // Typing indicator kaldır
        GizleTypingIndicator();
    }

    /// <summary>
    /// Animasyonlu typing indicator (3 nokta) oluşturur ve sohbete ekler.
    /// </summary>
    private void GosterTypingIndicator()
    {
        if (scrollSohbet == null) return;
        
        // Zaten varsa tekrar ekleme
        if (typingIndicator != null) return;

        if (emptySohbet != null)
        {
            emptySohbet.style.display = DisplayStyle.None;
        }

        typingIndicator = new VisualElement();
        typingIndicator.AddToClassList("typing-indicator");
        typingIndicator.name = "typingIndicator";

        VisualElement dot1 = new VisualElement();
        dot1.AddToClassList("typing-dot");
        VisualElement dot2 = new VisualElement();
        dot2.AddToClassList("typing-dot");
        VisualElement dot3 = new VisualElement();
        dot3.AddToClassList("typing-dot");
        dot3.style.marginRight = 0;

        typingIndicator.Add(dot1);
        typingIndicator.Add(dot2);
        typingIndicator.Add(dot3);

        scrollSohbet.Add(typingIndicator);

        // Animasyonlu nokta efekti
        int animFrame = 0;
        VisualElement[] dots = { dot1, dot2, dot3 };

        typingAnimTimer = typingIndicator.schedule.Execute(() => {
            for (int i = 0; i < dots.Length; i++)
            {
                dots[i].style.opacity = (i == animFrame % 3) ? 1f : 0.3f;
                dots[i].style.scale = (i == animFrame % 3) 
                    ? new StyleScale(new Scale(new Vector3(1.3f, 1.3f, 1f)))
                    : new StyleScale(new Scale(Vector3.one));
            }
            animFrame++;
        }).Every(400);

        // En alta kaydır
        scrollSohbet.schedule.Execute(() => {
            scrollSohbet.ScrollTo(typingIndicator);
        }).StartingIn(50);
    }

    private void GizleTypingIndicator()
    {
        if (typingIndicator != null)
        {
            typingIndicator.RemoveFromHierarchy();
            typingIndicator = null;
        }
        typingAnimTimer = null;
    }
}
