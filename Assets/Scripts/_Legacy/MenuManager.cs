using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject[] sayfalar;
    public Button asistanButonu;
    public Button gorevlerButonu;
    public Button haritaButonu;

    void Start()
    {
        if (asistanButonu != null) asistanButonu.onClick.AddListener(() => SayfaAc(0));
        if (gorevlerButonu != null) gorevlerButonu.onClick.AddListener(() => SayfaAc(1));
        if (haritaButonu != null) haritaButonu.onClick.AddListener(() => SayfaAc(2));
    }

    public void SayfaAc(int sayfaIndex)
    {
        for (int i = 0; i < sayfalar.Length; i++)
        {
            sayfalar[i].SetActive(false);
        }
        sayfalar[sayfaIndex].SetActive(true);
    }
}
