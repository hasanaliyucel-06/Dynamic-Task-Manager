using UnityEngine;
using UnityEditor;
using TMPro;

[InitializeOnLoad]
public class AutoSetupTypewriterEffect
{
    static AutoSetupTypewriterEffect()
    {
        EditorApplication.delayCall += DoSetupOnce;
    }

    static void DoSetupOnce()
    {
        if (EditorPrefs.GetBool("TypewriterEffectSetupDone", false)) return;

        GeminiManager gm = Object.FindObjectOfType<GeminiManager>(true);
        if (gm != null)
        {
            SerializedObject so = new SerializedObject(gm);
            SerializedProperty prop = so.FindProperty("aiResponseText");
            if (prop != null && prop.objectReferenceValue != null)
            {
                TMP_Text aiText = prop.objectReferenceValue as TMP_Text;
                if (aiText != null)
                {
                    if (aiText.GetComponent<TypewriterEffect>() == null)
                    {
                        aiText.gameObject.AddComponent<TypewriterEffect>();
                        EditorUtility.SetDirty(aiText.gameObject);
                        if (aiText.gameObject.scene.IsValid())
                        {
                            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(aiText.gameObject.scene);
                        }
                        Debug.Log("TypewriterEffect başarıyla aiResponseText objesine eklendi!");
                        EditorPrefs.SetBool("TypewriterEffectSetupDone", true);
                    }
                }
            }
        }
    }
}
