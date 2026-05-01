using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class AntigravityPrefabFixer : MonoBehaviour
{
    [MenuItem("Antigravity/Görev Kartını Kusursuz Hizala")]
    public static void FixPrefab()
    {
        // Prefab'ı projede bul
        string[] guids = AssetDatabase.FindAssets("GorevKarti t:Prefab");
        if (guids.Length == 0)
        {
            Debug.LogError("Antigravity: GorevKarti prefabı bulunamadı!");
            return;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

        // 1. Ana dizilim ayarlarını düzelt
        HorizontalLayoutGroup hlg = prefab.GetComponent<HorizontalLayoutGroup>();
        if (hlg == null) hlg = prefab.AddComponent<HorizontalLayoutGroup>();

        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlWidth = true;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.padding = new RectOffset(20, 20, 0, 0); // Sol ve sağdan boşluk
        hlg.spacing = 20; // Elemanlar arası boşluk

        // 2. İçerideki elemanların genişlik sınırlarını belirle
        TextMeshProUGUI[] texts = prefab.GetComponentsInChildren<TextMeshProUGUI>();
        Button btn = prefab.GetComponentInChildren<Button>();

        // Kırmızı buton (Sabit boyutta kalsın, sünmesin)
        if (btn != null)
        {
            LayoutElement le = btn.gameObject.GetComponent<LayoutElement>();
            if (le == null) le = btn.gameObject.AddComponent<LayoutElement>();
            le.minWidth = 80; 
            le.flexibleWidth = 0;
        }

        if (texts.Length >= 2)
        {
            // Görev Adı (Kalan tüm boşluğu bu doldursun ve sola yaslansın)
            LayoutElement leName = texts[0].gameObject.GetComponent<LayoutElement>();
            if (leName == null) leName = texts[0].gameObject.AddComponent<LayoutElement>();
            leName.flexibleWidth = 1; 
            texts[0].alignment = TextAlignmentOptions.Left;

            // Süre (Sağ tarafta sabit boyutta kalsın)
            LayoutElement leDur = texts[1].gameObject.GetComponent<LayoutElement>();
            if (leDur == null) leDur = texts[1].gameObject.AddComponent<LayoutElement>();
            leDur.minWidth = 150;
            leDur.flexibleWidth = 0;
            texts[1].alignment = TextAlignmentOptions.Right;
        }

        EditorUtility.SetDirty(prefab);
        AssetDatabase.SaveAssets();
        Debug.Log("Antigravity: Operasyon başarılı! Görev kartı jilet gibi hizalandı.");
    }
}
