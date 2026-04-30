using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class AntigravityUISetup : MonoBehaviour
{
    [MenuItem("Antigravity/Görevler UI Otomatik Diz")]
    public static void OtomatikDiz()
    {
        // Objeleri Bul
        RectTransform nameInput = GameObject.Find("NameInput")?.GetComponent<RectTransform>();
        RectTransform durationInput = GameObject.Find("DurationInput")?.GetComponent<RectTransform>();
        RectTransform addTaskButton = GameObject.Find("AddTaskButton")?.GetComponent<RectTransform>();
        RectTransform scrollView = GameObject.Find("Scroll View")?.GetComponent<RectTransform>();
        GameObject contentObj = GameObject.Find("Content");

        // Girdileri ve Butonu Üste Hizala (Top-Center)
        HizalaUst(nameInput, -150f, 800f, 120f);
        HizalaUst(durationInput, -300f, 800f, 120f);
        HizalaUst(addTaskButton, -450f, 400f, 120f);

        // Scroll View'i Tam Ekrana Yay ve Boşluk Bırak (Stretch-Stretch)
        if (scrollView != null)
        {
            scrollView.anchorMin = new Vector2(0, 0);
            scrollView.anchorMax = new Vector2(1, 1);
            scrollView.offsetMin = new Vector2(50, 250); // Sol, Alt
            scrollView.offsetMax = new Vector2(-50, -600); // Sağ, Üst
        }

        // Content İçine Otomatik Dizilim (Layout Group) Ekle
        if (contentObj != null)
        {
            VerticalLayoutGroup vlg = contentObj.GetComponent<VerticalLayoutGroup>();
            if (vlg == null) vlg = contentObj.AddComponent<VerticalLayoutGroup>();
            
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 20;
            
            ContentSizeFitter csf = contentObj.GetComponent<ContentSizeFitter>();
            if (csf == null) csf = contentObj.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        Debug.Log("Antigravity: Kaba kuvvet kullanıldı! Tüm UI elementleri saniyeler içinde jilet gibi hizalandı.");
    }

    private static void HizalaUst(RectTransform rt, float posY, float width, float height)
    {
        if (rt == null) return;
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(width, height);
        rt.anchoredPosition = new Vector2(0, posY);
    }
}
