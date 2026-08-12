using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TaskItemUI : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI durationText;
    public Button deleteButton;

    private Task currentTask;
    private ScheduleManager scheduleManager;

    public void Setup(Task task, ScheduleManager manager)
    {
        currentTask = task;
        scheduleManager = manager;

        UpdateUI(task.taskName, task.durationMinutes, task.isStrictBlock);

        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(DeleteTask);
        }
    }

    /// <summary>
    /// UI üzerinde görev adını ve süresini günceller.
    /// Katı blok ise isminin yanına ' (KİLİTLİ)' ekler.
    /// </summary>
    public void UpdateUI(string taskName, int duration, bool isStrict)
    {
        if (nameText != null)
        {
            nameText.text = isStrict ? $"{taskName} (KİLİTLİ)" : taskName;
        }

        if (durationText != null)
        {
            durationText.text = $"{duration} dk";
        }
    }

    public void DeleteTask()
    {
        if (scheduleManager != null && currentTask != null)
        {
            scheduleManager.tasks.Remove(currentTask);
            scheduleManager.SaveTasks();
            Destroy(gameObject);
        }
    }
}
