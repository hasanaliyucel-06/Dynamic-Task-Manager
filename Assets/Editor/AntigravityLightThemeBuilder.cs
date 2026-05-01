using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class AntigravityLightThemeBuilder : MonoBehaviour
{
    [MenuItem("Antigravity/Light & Clean Temasına Geç")]
    public static void ApplyLightTheme()
    {
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null) return;

        // 1. GENEL ZEMİN VE SAYFALAR (BEYAZ)
        string[] sayfalar = { "ArkaPlan", "Sayfa_Hedefler", "Sayfa_Gorevler", "Sayfa_Asistan" };
        foreach (string isim in sayfalar)
        {
            GameObject obj = GameObject.Find(isim);
            if (obj != null) {
                Image img = obj.GetComponent<Image>() ?? obj.AddComponent<Image>();
                img.color = Color.white;
            }
        }

        // 2. HEDEF EKRANI: YUVARLAK GRADYAN GRAFİKLER
        BuildCircularGoals();

        // 3. METİN RENKLERİNİ DÜZELT (KOYU GRİ/SİYAH)
        TextMeshProUGUI[] allTexts = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var txt in allTexts)
        {
            txt.color = new Color(0.1f, 0.1f, 0.1f); // Koyu Antrasit Yazı
        }

        Debug.Log("Antigravity: Light & Clean teması ve Gradyan Daireler uygulandı Boss!");
    }

    private static void BuildCircularGoals()
    {
        GameObject hedeflerSayfasi = GameObject.Find("Sayfa_Hedefler");
        if (hedeflerSayfasi == null) return;

        // Eski içeriği temizle
        while (hedeflerSayfasi.transform.childCount > 0)
            DestroyImmediate(hedeflerSayfasi.transform.GetChild(0).gameObject);

        GridLayoutGroup glg = hedeflerSayfasi.GetComponent<GridLayoutGroup>() ?? hedeflerSayfasi.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(350, 450);
        glg.spacing = new Vector2(50, 50);
        glg.padding = new RectOffset(50, 50, 100, 50);
        glg.childAlignment = TextAnchor.UpperCenter;

        // HEDEFLERİ OLUŞTUR
        CreateCircularProgress(hedeflerSayfasi.transform, "YAZILIM", 0.85f);
        CreateCircularProgress(hedeflerSayfasi.transform, "GİTAR", 0.45f);
        CreateCircularProgress(hedeflerSayfasi.transform, "FITNESS", 0.65f);
    }

    private static void CreateCircularProgress(Transform parent, string label, float amount)
    {
        GameObject container = new GameObject(label + "_Container");
        container.transform.SetParent(parent, false);
        
        // Dış Gri Halka (Zemin)
        GameObject bgRing = new GameObject("BackgroundRing");
        bgRing.transform.SetParent(container.transform, false);
        Image bgImg = bgRing.AddComponent<Image>();
        bgImg.color = new Color(0.9f, 0.9f, 0.9f); // Açık gri halka
        bgImg.rectTransform.sizeDelta = new Vector2(300, 300);
        
        // İlerleme Halkası (Senin istediğin gradyanı simüle eden dairesel dolum)
        GameObject progressRing = new GameObject("ProgressRing");
        progressRing.transform.SetParent(container.transform, false);
        Image progImg = progressRing.AddComponent<Image>();
        
        // Gradyan Etkisi için: Bu aşamada Unity'nin default sprite'ını dairesel dolum yapıyoruz
        progImg.type = Image.Type.Filled;
        progImg.fillMethod = Image.FillMethod.Radial360;
        progImg.fillAmount = amount;
        progImg.color = new Color(0.1f, 0.8f, 0.4f); // Örnek renk (İleride Sprite ile tam gradyan yapılabilir)
        progImg.rectTransform.sizeDelta = new Vector2(300, 300);

        // Ortadaki Yazı (%)
        GameObject txtObj = new GameObject("PercentText");
        txtObj.transform.SetParent(container.transform, false);
        TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
        tmp.text = "%" + (amount * 100).ToString("0");
        tmp.fontSize = 60;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.black;

        // Alt Etiket
        GameObject labelObj = new GameObject("LabelText");
        labelObj.transform.SetParent(container.transform, false);
        TextMeshProUGUI tmpL = labelObj.AddComponent<TextMeshProUGUI>();
        tmpL.text = label;
        tmpL.fontSize = 30;
        tmpL.alignment = TextAlignmentOptions.Center;
        tmpL.rectTransform.anchoredPosition = new Vector2(0, -180);
    }
}
