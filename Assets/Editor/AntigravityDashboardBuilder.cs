using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class AntigravityDashboardBuilder : MonoBehaviour
{
    [MenuItem("Antigravity/Hedefler (Dashboard) Sayfasını Çiz")]
    public static void BuildDashboard()
    {
        GameObject hedeflerSayfasi = GameObject.Find("Sayfa_Hedefler");
        if (hedeflerSayfasi == null)
        {
            Debug.LogError("Antigravity: Sahnede 'Sayfa_Hedefler' adında bir obje bulunamadı!");
            return;
        }

        // 1. Zemin ve Düzenleyici Ekle
        Image bg = hedeflerSayfasi.GetComponent<Image>();
        if (bg == null) bg = hedeflerSayfasi.AddComponent<Image>();
        ColorUtility.TryParseHtmlString("#050505", out Color zeminSiyahi);
        bg.color = zeminSiyahi;

        VerticalLayoutGroup vlg = hedeflerSayfasi.GetComponent<VerticalLayoutGroup>();
        if (vlg == null) vlg = hedeflerSayfasi.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(40, 40, 80, 40);
        vlg.spacing = 35;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // 2. Mevcut İçeriği Temizle
        while (hedeflerSayfasi.transform.childCount > 0)
        {
            DestroyImmediate(hedeflerSayfasi.transform.GetChild(0).gameObject);
        }

        // 3. İÇERİKLERİ OLUŞTUR
        // Ana Başlık
        CreateText(hedeflerSayfasi.transform, "HEDEFLER VE İSTATİSTİKLER", 45, true, TextAlignmentOptions.Center, "#F0F0F0");

        // Geri Sayım Panosu
        CreateText(hedeflerSayfasi.transform, "Erasmus / Yurtdışı Çıkış Hedefi", 24, false, TextAlignmentOptions.Left, "#A8A8A8");
        CreateText(hedeflerSayfasi.transform, "KALAN SÜRE: 340 GÜN", 40, true, TextAlignmentOptions.Left, "#0044FF");

        // İlerleme Barı 1
        CreateText(hedeflerSayfasi.transform, "C# & Unity Gelişimi", 24, false, TextAlignmentOptions.Left, "#A8A8A8");
        CreateProgressBar(hedeflerSayfasi.transform, "YazilimBar", 0.75f); // %75 dolu

        // İlerleme Barı 2
        CreateText(hedeflerSayfasi.transform, "Elektro Gitar Pratiği", 24, false, TextAlignmentOptions.Left, "#A8A8A8");
        CreateProgressBar(hedeflerSayfasi.transform, "GitarBar", 0.40f); // %40 dolu

        EditorUtility.SetDirty(hedeflerSayfasi);
        Debug.Log("Antigravity: Dashboard sayfası sıfırdan, profesyonel grafiklerle inşa edildi!");
    }

    private static void CreateText(Transform parent, string content, int fontSize, bool isBold, TextAlignmentOptions alignment, string hexColor)
    {
        GameObject txtObj = new GameObject("Text_" + content.Substring(0, Mathf.Min(5, content.Length)));
        txtObj.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.fontStyle = isBold ? FontStyles.Bold : FontStyles.Normal;
        tmp.alignment = alignment;
        ColorUtility.TryParseHtmlString(hexColor, out Color color);
        tmp.color = color;
        
        LayoutElement le = txtObj.AddComponent<LayoutElement>();
        le.minHeight = fontSize + 10;
        le.flexibleWidth = 1;
    }

    private static void CreateProgressBar(Transform parent, string name, float fillAmount)
    {
        GameObject barObj = new GameObject(name);
        barObj.transform.SetParent(parent, false);
        
        LayoutElement le = barObj.AddComponent<LayoutElement>();
        le.minHeight = 45;

        Image bg = barObj.AddComponent<Image>();
        ColorUtility.TryParseHtmlString("#121212", out Color bgColor);
        bg.color = bgColor;

        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(barObj.transform, false);
        Image fill = fillObj.AddComponent<Image>();
        ColorUtility.TryParseHtmlString("#0044FF", out Color fillColor);
        fill.color = fillColor;

        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0, 0);
        fillRect.anchorMax = new Vector2(fillAmount, 1);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
    }
}
