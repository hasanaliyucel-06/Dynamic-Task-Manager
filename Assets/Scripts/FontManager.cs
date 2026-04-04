using UnityEngine;
using TMPro; // İŞTE EKSİK OLAN SİHİRLİ KELİME BU

public class FontManager : MonoBehaviour
{
    public TMP_FontAsset yeniFont;

    [ContextMenu("Tüm Fontları Değiştir")]
    public void TumFontlariDegistir()
    {
        // Canvas'ın altındaki (gizli olanlar dahil) tüm yazıları bul
        TextMeshProUGUI[] tumYazilar = GetComponentsInChildren<TextMeshProUGUI>(true);

        foreach (TextMeshProUGUI yazi in tumYazilar)
        {
            yazi.font = yeniFont;
        }

        Debug.LogWarning("BÜYÜ ÇALIŞTI: Toplam " + tumYazilar.Length + " adet yazının fontu Orbitron'a çevrildi!");
    }
}