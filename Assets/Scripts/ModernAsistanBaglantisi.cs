using UnityEngine;
using UnityEngine.UIElements;

public class ModernAsistanBaglantisi : MonoBehaviour
{
    private UIDocument uiDocument;
    private Button gonderButonu;
    private TextField mesajGirdisi;
    private ScrollView scrollSohbet;
    private Label lblStatus;

    public SiberAsistan siberAsistan; 

    void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null) return;

        var root = uiDocument.rootVisualElement;

        gonderButonu = root.Q<Button>("btnGonder");
        mesajGirdisi = root.Q<TextField>("txtMesaj");
        scrollSohbet = root.Q<ScrollView>("scrollSohbet");
        lblStatus = root.Q<Label>(className: "header-status");

        if (gonderButonu != null)
        {
            gonderButonu.clicked += MesajGonderildi;
        }
    }

    void MesajGonderildi()
    {
        if (mesajGirdisi == null || string.IsNullOrWhiteSpace(mesajGirdisi.value)) return;

        string gidenMesaj = mesajGirdisi.value;
        Debug.Log("Antigravity: Gönderilen Mesaj: " + gidenMesaj);

        if (siberAsistan != null)
        {
            siberAsistan.ModernArayuzdenMesajAl(gidenMesaj);
        }

        // Gönderdikten sonra kutuyu temizle
        mesajGirdisi.value = "";
    }

    public void EkranaMesajBas(string metin, bool kullaniciMi)
    {
        if (scrollSohbet == null) return;

        Label label = new Label();
        label.text = metin;
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
            // 2. Kullanıcı (Sen) stili
            label.style.backgroundColor = new StyleColor(ColorUtility.TryParseHtmlString("#007AFF", out Color c) ? c : Color.blue);
            label.style.borderBottomRightRadius = 0;
            label.style.alignSelf = Align.FlexEnd;
        }
        else
        {
            label.AddToClassList("mesaj-asistan");
            // 3. Asistan stili
            label.style.backgroundColor = new StyleColor(ColorUtility.TryParseHtmlString("#1E293B", out Color asistanC) ? asistanC : Color.gray);
            label.style.borderBottomLeftRadius = 0;
            label.style.alignSelf = Align.FlexStart;
        }

        scrollSohbet.Add(label);

        // UI Toolkit'in elementi çizmesini bekleyip (50ms) en alta kaydırır
        scrollSohbet.schedule.Execute(() => {
            scrollSohbet.ScrollTo(label);
        }).StartingIn(50);
    }

    public void DurumYaziyorYap() 
    { 
        if(lblStatus != null) 
        { 
            lblStatus.text = "Yazıyor..."; 
            lblStatus.style.color = new StyleColor(Color.yellow); 
        } 
    }

    public void DurumCevrimiciYap() {
        if(lblStatus != null) {
            lblStatus.text = "Çevrimiçi";
            Color c;
            if (ColorUtility.TryParseHtmlString("#10B981", out c)) {
                lblStatus.style.color = new StyleColor(c);
            } else {
                lblStatus.style.color = new StyleColor(Color.green);
            }
        }
    }
}
