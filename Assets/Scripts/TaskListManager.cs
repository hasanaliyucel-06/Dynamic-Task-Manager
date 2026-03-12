using UnityEngine;

public class TaskListManager : MonoBehaviour
{
    [Header("Referanslar")]
    public ScheduleManager scheduleManager;
    public Transform container;
    public GameObject taskPrefab;

    private void Start()
    {
        // Başlangıçta listeyi ilk kez oluştur
        RefreshList();

        // Herhangi bir gecikme uygulandığında listeyi otomatik güncellemek için Event'e dinleyici ekle
        if (scheduleManager != null)
        {
            scheduleManager.onScheduleUpdated.AddListener(RefreshList);
        }
    }

    /// <summary>
    /// Container içindeki her şeyi siler ve ScheduleManager'daki listeye göre yeniden oluşturur.
    /// </summary>
    public void RefreshList()
    {
        // 1. Önce eski listeyi temizle (container içindeki child objeleri yok et)
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        // Referanslar eksikse devam etme
        if (scheduleManager == null || taskPrefab == null || container == null)
            return;

        // 2. Güncel listeyi döngüye sokup yeni objeler üret (Instantiate)
        foreach (var task in scheduleManager.tasks)
        {
            GameObject newTaskObj = Instantiate(taskPrefab, container);
            
            // Üretilen objenin üzerindeki TaskItemUI bileşenini bul ve değerleri gönder
            TaskItemUI taskUI = newTaskObj.GetComponent<TaskItemUI>();
            if (taskUI != null)
            {
                taskUI.UpdateUI(task.taskName, task.durationMinutes, task.isStrictBlock);
            }
        }
    }
}
