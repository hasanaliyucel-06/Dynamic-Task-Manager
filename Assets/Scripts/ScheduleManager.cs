using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;


// Bu sınıfı Unity Inspector'da görebilmek için Serializable olarak ekliyoruz
[System.Serializable]
public class Task
{
    public string taskName;
    public int durationMinutes;
    public bool isStrictBlock;
    public bool isCompleted;

    // Basit bir constructor (isteğe bağlı)
    public Task(string name, int duration, bool strictBlock, bool completed = false)
    {
        taskName = name;
        durationMinutes = duration;
        isStrictBlock = strictBlock;
        isCompleted = completed;
    }
}

// JsonUtility listeleri doğrudan desteklemediği için yardımcı sınıf kullanıyoruz.
[System.Serializable]
public class TaskWrapper
{
    public List<Task> tasks;
}

public class ScheduleManager : MonoBehaviour
{
    // Task objelerinin tutulduğu liste
    public List<Task> tasks = new List<Task>();
    
    // Toplam yaşanan gecikmeyi tutar
    public int TotalDelay = 0;

    // Görev listesinde veya gecikmelerde bir değişiklik olduğunda tetiklenir (UI yenilemek için)
    public UnityEvent onScheduleUpdated;

    public TaskListManager taskListManager;

    private void Start()
    {
        LoadTasks();
        
        if (taskListManager != null)
        {
            taskListManager.RefreshList();
        }
        else
        {
            onScheduleUpdated?.Invoke();
        }
    }

    private void Update()
    {
        // Yeni Input System ile boşluk tuşu kontrolü
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("[ScheduleManager] Boşluk tuşuna basıldı! 30 dakika gecikme (trafik) simüle ediliyor...");
            ApplyDelay(30);
            
            // Son durumu görmek için listeyi yazdır
            Debug.Log("--- Güncel Görev Durumu ---");
            PrintTasks();
        }
    }

    /// <summary>
    /// Verilen gecikme süresini katı olmayan (esnek) ilk görevden düşer.
    /// Eğer o görevin süresi yetmezse, kalan borcu sonraki esnek görevlere aktarır.
    /// Toplam yaşanan gecikmeyi TotalDelay değişkeninde saklar.
    /// </summary>
    /// <param name="delayMinutes">Uygulanacak gecikme süresi (kaç dakika olduğu)</param>
    public void ApplyDelay(int delayMinutes)
    {
        TotalDelay += delayMinutes; // Toplam gecikmeyi artır

        Debug.Log($"[ScheduleManager] ApplyDelay çağrıldı: {delayMinutes} dakika gecikme uygulanacak. Toplam Gecikme: {TotalDelay} dk");
        
        int remainingDebt = delayMinutes; // Dağıtılması gereken kalan gecikme

        for (int i = 0; i < tasks.Count; i++)
        {
            // Eğer borcumuz kalmadıysa döngüden çıkabiliriz
            if (remainingDebt <= 0)
            {
                Debug.Log("[ScheduleManager] Tüm gecikme başarıyla düşüldü.");
                break;
            }

            Task currentTask = tasks[i];

            // Eğer görev zaten tamamlandıysa, ona dokunmayalım
            if (currentTask.isCompleted)
            {
                continue;
            }

            // Eğer görev KATİYSA (Strict Block), bu görevden süre düşemeyiz
            if (currentTask.isStrictBlock)
            {
                Debug.Log($"[ScheduleManager] '{currentTask.taskName}' görevi katı bir blok. Süresi değiştirilmeden geçiliyor.");
                continue;
            }

            // Geriye esnek ve tamamlanmamış görev kaldı
            Debug.Log($"[ScheduleManager] Sıradaki esnek görev bulundu: '{currentTask.taskName}', Mevcut Süresi: {currentTask.durationMinutes} dakika, Kalan Borç: {remainingDebt} dakika.");

            // Esnek görevin süresi borcu tek başına karşılayabiliyorsa (veya tam yetiyorsa)
            if (currentTask.durationMinutes >= remainingDebt)
            {
                currentTask.durationMinutes -= remainingDebt;
                Debug.Log($"[ScheduleManager] '{currentTask.taskName}' görevinin süresinden {remainingDebt} dakika düşüldü. Yeni süresi: {currentTask.durationMinutes} dakika.");
                remainingDebt = 0; // Borcu sıfırladık
            }
            else
            {
                // Esnek görevin süresi borcu kapatmaya yetmiyorsa
                Debug.Log($"[ScheduleManager] '{currentTask.taskName}' görevinin süresi ({currentTask.durationMinutes} dk) borcu ({remainingDebt} dk) tamamen kaldırmaya yetmiyor!");
                
                remainingDebt -= currentTask.durationMinutes; 
                currentTask.durationMinutes = 0; // Kalan tüm süreyi tükettik

                Debug.Log($"[ScheduleManager] '{currentTask.taskName}' görevi tamamen tüketildi (Süresi 0 yapıldı). Sonraki esnek göreve devreden borç: {remainingDebt} dakika.");
                
                // İsteğe bağlı olarak süresi biten bir görevi tamamlandı işaretleyebilirsiniz. 
                // Biz sadece süresini 0 yaptık.
            }
        }

        // Döngü bittiğinde hala borcumuz varsa, yeterli esnek süremiz yok demektir.
        if (remainingDebt > 0)
        {
            Debug.LogWarning($"[ScheduleManager] UYARI: Bütün esnek görevler tüketildi ancak hala \n{remainingDebt} dakika gecikme borcu kaldı! Sisteme yeni esnek zaman eklenmesi gerekebilir.");
        }

        // Tüm değişiklikler bittikten sonra UI'ı veya diğer dinleyicileri tetikle (Örn: TaskListManager)
        onScheduleUpdated?.Invoke();
        
        // Değişikliği anında kaydet
        SaveTasks();
    }

    /// <summary>
    /// Listedeki tüm görevleri konsola yazdırarak kontrol etmeyi kolaylaştırır.
    /// </summary>
    public void PrintTasks()
    {
        foreach (var task in tasks)
        {
            Debug.Log($"Görev: {task.taskName} | Süre: {task.durationMinutes} dk | Katı Mı: {task.isStrictBlock} | Tamamlandı Mı: {task.isCompleted}");
        }
    }

    public void AddTask(string taskName, int duration, bool isStrict)
    {
        Task newTask = new Task(taskName, duration, isStrict);
        tasks.Add(newTask);

        if (taskListManager != null)
        {
            taskListManager.RefreshList();
        }
        else
        {
            onScheduleUpdated?.Invoke();
        }
        
        // Değişikliği anında kaydet
        SaveTasks();
    }

    public void SaveTasks()
    {
        TaskWrapper wrapper = new TaskWrapper();
        wrapper.tasks = this.tasks;
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString("SavedTasks", json);
        PlayerPrefs.Save();
        Debug.Log("[ScheduleManager] Görevler kaydedildi.");
    }

    public void LoadTasks()
    {
        if (PlayerPrefs.HasKey("SavedTasks"))
        {
            string json = PlayerPrefs.GetString("SavedTasks");
            TaskWrapper wrapper = JsonUtility.FromJson<TaskWrapper>(json);
            
            // Veri varsa listeye ata
            if (wrapper != null && wrapper.tasks != null && wrapper.tasks.Count > 0)
            {
                this.tasks = wrapper.tasks;
                Debug.Log("[ScheduleManager] Görevler başarıyla yüklendi.");
                return;
            }
        }
        
        // Hiç kayıt yoksa (veya liste boşsa/bozulmuşsa) varsayılan görevleri kullan
        Debug.Log("[ScheduleManager] Kayıt bulunamadı, varsayılan görevler oluşturuluyor.");
        tasks.Clear();
        tasks.Add(new Task("Hazırlık ve E-postalar", 30, true));
        tasks.Add(new Task("Programlama", 120, false));
        tasks.Add(new Task("Öğle Yemeği", 60, true));
        tasks.Add(new Task("Proje Tasarımı", 90, false));
        SaveTasks();
    }
}
