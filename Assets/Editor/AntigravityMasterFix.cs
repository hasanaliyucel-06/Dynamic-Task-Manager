using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class AntigravityMasterFix : MonoBehaviour
{
    [MenuItem("Antigravity/Sistemi Tamir Et (Kesin Çözüm)")]
    public static void FixEverything()
    {
        // 1. AŞAMA: CONTENT OBJESİNDEKİ KAOSU TEMİZLE
        GameObject content = GameObject.Find("Content");
        if (content != null)
        {
            ContentSizeFitter[] fitters = content.GetComponents<ContentSizeFitter>();
            if(fitters.Length > 1) 
            {
                for(int i = 1; i < fitters.Length; i++) DestroyImmediate(fitters[i]);
            }
            
            ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
            if(fitter != null) 
            {
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            VerticalLayoutGroup vlg = content.GetComponent<VerticalLayoutGroup>();
            if (vlg != null)
            {
                vlg.childAlignment = TextAnchor.UpperCenter;
                vlg.childControlWidth = true;
                vlg.childControlHeight = false; 
                vlg.childForceExpandWidth = true;
                vlg.childForceExpandHeight = false;
                vlg.spacing = 15; 
                vlg.padding = new RectOffset(10, 10, 10, 10);
            }
        }

        // 2. AŞAMA: DOĞRU PREFABI BUL (TaskItemPrefab)
        string[] guids = AssetDatabase.FindAssets("TaskItemPrefab t:Prefab");
        if (guids.Length == 0)
        {
            Debug.LogError("Antigravity: 'TaskItemPrefab' bulunamadı! İsminin tam olarak bu olduğundan emin ol.");
            return;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

        // 3. AŞAMA: PREFABIN İÇ MİMARİSİNİ VE EZİLMEYİ DÜZELT
        HorizontalLayoutGroup hlg = prefab.GetComponent<HorizontalLayoutGroup>();
        if (hlg == null) hlg = prefab.AddComponent<HorizontalLayoutGroup>();

        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlWidth = true;
        hlg.childControlHeight = false; 
        hlg.childForceExpandWidth = false; 
        hlg.childForceExpandHeight = false;
        hlg.padding = new RectOffset(15, 20, 0, 0); 
        hlg.spacing = 15;

        // 4. AŞAMA: İÇERİDEKİ EŞYALARA BOYUT (FLEX) KURALI KOY
        Button btn = prefab.GetComponentInChildren<Button>();
        if (btn != null)
        {
            LayoutElement leBtn = btn.gameObject.GetComponent<LayoutElement>();
            if (leBtn == null) leBtn = btn.gameObject.AddComponent<LayoutElement>();
            leBtn.minWidth = 80; 
            leBtn.flexibleWidth = 0;
        }

        TextMeshProUGUI[] texts = prefab.GetComponentsInChildren<TextMeshProUGUI>();
        if (texts.Length >= 2)
        {
            LayoutElement leName = texts[0].gameObject.GetComponent<LayoutElement>();
            if (leName == null) leName = texts[0].gameObject.AddComponent<LayoutElement>();
            leName.flexibleWidth = 1; 
            texts[0].alignment = TextAlignmentOptions.Left;

            LayoutElement leDur = texts[1].gameObject.GetComponent<LayoutElement>();
            if (leDur == null) leDur = texts[1].gameObject.AddComponent<LayoutElement>();
            leDur.minWidth = 120;
            leDur.flexibleWidth = 0;
            texts[1].alignment = TextAlignmentOptions.Right;
        }

        EditorUtility.SetDirty(prefab);
        AssetDatabase.SaveAssets();
        Debug.Log("Antigravity: Hatalar temizlendi, Prefab onarıldı, liste jilet gibi oldu!");
    }
}
