using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Linq;

[InitializeOnLoad]
public class AutoSetupMenuManager
{
    static AutoSetupMenuManager()
    {
        EditorApplication.delayCall += DoSetupOnce;
    }

    static void DoSetupOnce()
    {
        if (EditorPrefs.GetBool("MenuManagerAutoSetupDone", false)) return;

        Canvas canvas = Object.FindObjectOfType<Canvas>(true);
        if (canvas == null) return;

        MenuManager menuManager = canvas.GetComponent<MenuManager>();
        if (menuManager == null)
        {
            menuManager = canvas.gameObject.AddComponent<MenuManager>();
        }

        Transform[] allTransforms = canvas.GetComponentsInChildren<Transform>(true);

        GameObject asistan = allTransforms.FirstOrDefault(t => t.name == "Sayfa_Asistan")?.gameObject;
        GameObject gorevler = allTransforms.FirstOrDefault(t => t.name == "Sayfa_Gorevler")?.gameObject;
        GameObject harita = allTransforms.FirstOrDefault(t => t.name == "Sayfa_Harita")?.gameObject;

        menuManager.sayfalar = new GameObject[] { asistan, gorevler, harita };

        Transform altMenu = allTransforms.FirstOrDefault(t => t.name == "AltMenu");
        if (altMenu != null)
        {
            Button[] buttons = altMenu.GetComponentsInChildren<Button>(true);
            foreach (Button b in buttons)
            {
                string name = b.gameObject.name.ToLower();
                if (name.Contains("asistan")) menuManager.asistanButonu = b;
                else if (name.Contains("gorev") || name.Contains("görev")) menuManager.gorevlerButonu = b;
                else if (name.Contains("harita")) menuManager.haritaButonu = b;
            }
        }

        EditorUtility.SetDirty(menuManager);
        if (canvas.gameObject.scene.IsValid())
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        }
        
        EditorPrefs.SetBool("MenuManagerAutoSetupDone", true);
        Debug.Log("MenuManager otomatik kurulumu tamamlandı!");
    }
}
