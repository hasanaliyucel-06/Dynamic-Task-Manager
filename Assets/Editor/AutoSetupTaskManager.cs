using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;
using System.Linq;

[InitializeOnLoad]
public class AutoSetupTaskManager
{
    static AutoSetupTaskManager()
    {
        EditorApplication.delayCall += DoSetupOnce;
    }

    static void DoSetupOnce()
    {
        if (EditorPrefs.GetBool("TaskManagerSetupDone", false)) return;

        Canvas canvas = Object.FindObjectOfType<Canvas>(true);
        if (canvas == null) return;

        Transform[] allTransforms = canvas.GetComponentsInChildren<Transform>(true);
        Transform sayfaGorevler = allTransforms.FirstOrDefault(t => t.name == "Sayfa_Gorevler");

        if (sayfaGorevler != null)
        {
            TaskManager taskManager = sayfaGorevler.GetComponent<TaskManager>();
            if (taskManager == null)
            {
                taskManager = sayfaGorevler.gameObject.AddComponent<TaskManager>();
            }

            Transform[] gorevlerTransforms = sayfaGorevler.GetComponentsInChildren<Transform>(true);

            Transform nameInputObj = gorevlerTransforms.FirstOrDefault(t => t.name == "NameInput");
            if (nameInputObj != null) taskManager.nameInput = nameInputObj.GetComponent<TMP_InputField>();

            Transform durationInputObj = gorevlerTransforms.FirstOrDefault(t => t.name == "DurationInput");
            if (durationInputObj != null) taskManager.durationInput = durationInputObj.GetComponent<TMP_InputField>();

            Transform addTaskBtnObj = gorevlerTransforms.FirstOrDefault(t => t.name == "AddTaskButton");
            if (addTaskBtnObj != null) taskManager.addTaskButton = addTaskBtnObj.GetComponent<Button>();

            Transform contentObj = gorevlerTransforms.FirstOrDefault(t => t.name == "Content");
            if (contentObj != null) taskManager.contentPanel = contentObj;

            EditorUtility.SetDirty(taskManager);
            if (sayfaGorevler.gameObject.scene.IsValid())
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(sayfaGorevler.gameObject.scene);
            }

            Debug.Log("TaskManager başarıyla kuruldu ve UI referansları atandı! (taskPrefab hariç)");
            EditorPrefs.SetBool("TaskManagerSetupDone", true);
        }
    }
}
