using UnityEngine;
using UnityEngine.UIElements;

public class ModernAsistanBaglantisi : MonoBehaviour
{
    private UIDocument uiDocument;
    private Button gonderButonu;
    private TextField mesajGirdisi;

    public SiberAsistan siberAsistan; 

    void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null) return;

        var root = uiDocument.rootVisualElement;

        gonderButonu = root.Q<Button>("btnGonder");
        mesajGirdisi = root.Q<TextField>("txtMesaj");

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
}
