using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class AntigravityMenuStyler : MonoBehaviour
{
    [MenuItem("Antigravity/Alt Menüyü Karanlık Temaya Çevir")]
    public static void StyleBottomMenu()
    {
        Color koyuArkaPlan; ColorUtility.TryParseHtmlString("#0A0A0C", out koyuArkaPlan); // En koyu siyah
        Color yaziRengi; ColorUtility.TryParseHtmlString("#A8A8A8", out yaziRengi); // Pasif gri yazı

        // Alt menü taşıyıcısını bul
        GameObject altMenu = GameObject.Find("AltMenu");
        if (altMenu != null)
        {
            Image menuBg = altMenu.GetComponent<Image>();
            if (menuBg == null) menuBg = altMenu.AddComponent<Image>();
            menuBg.color = koyuArkaPlan;
        }

        // Hiyerarşindeki eski isimlere göre butonları bulup baştan yaratıyoruz
        StyleButton("HaritaBtn", "HEDEFLER", koyuArkaPlan, yaziRengi);
        StyleButton("GorevlerBtn", "GÖREVLER", koyuArkaPlan, yaziRengi);
        StyleButton("AsistanBtn", "ASİSTAN", koyuArkaPlan, yaziRengi);

        Debug.Log("Antigravity: Alt menü karanlık temaya uyarlandı ve yazılar eklendi!");
    }

    static void StyleButton(string btnName, string text, Color bg, Color txtColor)
    {
        GameObject btnObj = GameObject.Find(btnName);
        if (btnObj != null)
        {
            // Buton arka planı
            Image img = btnObj.GetComponent<Image>();
            if (img != null) img.color = bg;

            // Yazı ayarları
            TextMeshProUGUI tmp = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = text;
                tmp.color = txtColor;
                tmp.fontStyle = FontStyles.Bold;
                tmp.fontSize = 30; // Yazı boyutu
            }
            EditorUtility.SetDirty(btnObj);
        }
        else
        {
            Debug.LogWarning("Bulunamadı: " + btnName + ". İsimleri kontrol et Patron.");
        }
    }
}
