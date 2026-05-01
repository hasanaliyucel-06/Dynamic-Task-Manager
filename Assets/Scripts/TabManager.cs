using UnityEngine;
using UnityEngine.UI;

public class TabManager : MonoBehaviour
{
    [Header("Menü Butonları")]
    public Button btnHedefler;
    public Button btnGorevler;
    public Button btnAsistan;

    [Header("Sayfalar")]
    public GameObject sayfaHedefler;
    public GameObject sayfaGorevler;
    public GameObject sayfaAsistan;

    [Header("Buton Renkleri")]
    public Color aktifRenk = new Color(0f, 0.26f, 1f, 1f); // Elektrik Mavisi
    public Color pasifRenk = new Color(0.15f, 0.15f, 0.15f, 1f); // Koyu Gri

    void Start()
    {
        // Butonlara tıklama olaylarını bağla
        if (btnHedefler != null) btnHedefler.onClick.AddListener(() => SekmeDegistir(sayfaHedefler, btnHedefler));
        if (btnGorevler != null) btnGorevler.onClick.AddListener(() => SekmeDegistir(sayfaGorevler, btnGorevler));
        if (btnAsistan != null) btnAsistan.onClick.AddListener(() => SekmeDegistir(sayfaAsistan, btnAsistan));

        // Başlangıçta Görevler sayfasını aç
        SekmeDegistir(sayfaGorevler, btnGorevler);
    }

    public void SekmeDegistir(GameObject aktifSayfa, Button aktifButon)
    {
        // Tüm sayfaları kapat
        if (sayfaHedefler != null) sayfaHedefler.SetActive(false);
        if (sayfaGorevler != null) sayfaGorevler.SetActive(false);
        if (sayfaAsistan != null) sayfaAsistan.SetActive(false);

        // Tüm butonları pasif renge boya
        if (btnHedefler != null) btnHedefler.GetComponent<Image>().color = pasifRenk;
        if (btnGorevler != null) btnGorevler.GetComponent<Image>().color = pasifRenk;
        if (btnAsistan != null) btnAsistan.GetComponent<Image>().color = pasifRenk;

        // İstenen sayfayı aç ve butonunu parlat
        if (aktifSayfa != null) aktifSayfa.SetActive(true);
        if (aktifButon != null) aktifButon.GetComponent<Image>().color = aktifRenk;
    }
}
