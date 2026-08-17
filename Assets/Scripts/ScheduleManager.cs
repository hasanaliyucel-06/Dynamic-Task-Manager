using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

// GorevData ve TaskWrapper artık Models/VeriModelleri.cs dosyasında tanımlıdır.

public class ScheduleManager : MonoBehaviour
{
    // GorevData objelerinin tutulduğu liste
    public List<GorevData> tasks = new List<GorevData>();

    // Cache'lenmiş referans
    private PageNavigator _cachedNavigator;
    
    // Toplam yaşanan gecikmeyi tutar
    public int TotalDelay = 0;

    // Görev listesinde veya gecikmelerde bir değişiklik olduğunda tetiklenir (UI yenilemek için)
    public UnityEvent onScheduleUpdated;


    private void Start()
    {
        LoadTasks();
        
        onScheduleUpdated?.Invoke();
    }

    private void Update()
    {
        // Yeni Input System ile boşluk tuşu kontrolü (Sistemi yormamak için geçici olarak kapatıldı)
        /*
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("[ScheduleManager] Boşluk tuşuna basıldı! 30 dakika gecikme (trafik) simüle ediliyor...");
            ApplyDelay(30);
            
            // Son durumu görmek için listeyi yazdır
            Debug.Log("--- Güncel Görev Durumu ---");
            PrintTasks();
        }
        */
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

            GorevData currentTask = tasks[i];

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

        // Tüm değişiklikler bittikten sonra UI'ı veya diğer dinleyicileri tetikle
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

    public void RemoveTask(string taskName)
    {
        GorevData taskToRemove = tasks.Find(t => t.taskName == taskName);
        if (taskToRemove != null)
        {
            tasks.Remove(taskToRemove);
            SaveTasks();
        }
    }

    /// <summary>
    /// Görevi tamamlandı olarak işaretler (silmez, kalıcı olarak kaydeder).
    /// </summary>
    public void MarkTaskCompleted(string taskName)
    {
        GorevData task = tasks.Find(t => t.taskName == taskName && !t.isCompleted);
        if (task != null)
        {
            task.isCompleted = true;
            SaveTasks();
            onScheduleUpdated?.Invoke();
        }
    }

    public void UpdateTask(string oldName, string newName, string newTime, int newDuration, string yeniKategori = "Genel", int yeniOncelik = 1, string yeniNotlar = "")
    {
        GorevData taskToUpdate = tasks.Find(t => t.taskName == oldName);
        if (taskToUpdate != null)
        {
            taskToUpdate.taskName = newName;
            taskToUpdate.taskTime = newTime;
            taskToUpdate.durationMinutes = newDuration;
            taskToUpdate.kategori = yeniKategori;
            taskToUpdate.oncelik = yeniOncelik;
            taskToUpdate.notlar = yeniNotlar;
            SaveTasks();
            onScheduleUpdated?.Invoke();
        }
    }

    /// <summary>
    /// Yeni görev ekler. Aynı tarih+saat+isimde görev zaten varsa çift kayıt oluşturmaz.
    /// </summary>
    public void AddTask(string tarih, string time, string taskName, int duration, bool isStrict = false, bool isRepeating = false, string kategori = "Genel", int hatirlatici = 15, int oncelik = 1, string notlar = "")
    {
        // Çift kayıt kontrolü: Aynı tarih, saat ve isimdeki görev zaten varsa ekleme
        bool zatenVar = tasks.Exists(t => t.taskDate == tarih && t.taskTime == time && t.taskName == taskName);
        if (zatenVar)
        {
            Debug.Log($"[ScheduleManager] Görev zaten mevcut, çift kayıt engellendi: {taskName} ({tarih} {time})");
            return;
        }

        GorevData newTask = new GorevData(tarih, time, taskName, duration, isStrict, false, isRepeating, kategori, hatirlatici, oncelik, notlar);
        tasks.Add(newTask);

        onScheduleUpdated?.Invoke();
        
        // Değişikliği anında kaydet
        SaveTasks();
    }

    public void SaveTasks()
    {
        TaskWrapper wrapper = new TaskWrapper();
        wrapper.tasks = this.tasks;
        string json = JsonUtility.ToJson(wrapper, true);
        
        string path = Path.Combine(Application.persistentDataPath, "tasks.json");
        try
        {
            File.WriteAllText(path, json);
        }
        catch (System.Exception e)
        {
            Debug.LogError("[ScheduleManager] Görevler kaydedilemedi: " + e.Message);
        }
    }

    public void LoadTasks()
    {
        string path = Path.Combine(Application.persistentDataPath, "tasks.json");
        string json = "";

        // Migration (PlayerPrefs -> JSON)
        if (PlayerPrefs.HasKey("SavedTasks"))
        {
            json = PlayerPrefs.GetString("SavedTasks");
            try
            {
                File.WriteAllText(path, json);
                PlayerPrefs.DeleteKey("SavedTasks");
                PlayerPrefs.Save();
                Debug.Log("[ScheduleManager] Eski PlayerPrefs verisi JSON dosyasına taşındı.");
            }
            catch (System.Exception e)
            {
                Debug.LogError("[ScheduleManager] Migration hatası: " + e.Message);
            }
        }
        else if (File.Exists(path))
        {
            try
            {
                json = File.ReadAllText(path);
            }
            catch (System.Exception e)
            {
                Debug.LogError("[ScheduleManager] Görevler okunamadı: " + e.Message);
            }
        }

        if (!string.IsNullOrEmpty(json))
        {
            TaskWrapper wrapper = JsonUtility.FromJson<TaskWrapper>(json);
            
            // Veri varsa listeye ata
            if (wrapper != null && wrapper.tasks != null && wrapper.tasks.Count > 0)
            {
                this.tasks = wrapper.tasks;

                // --- TEKRARLAYAN GÖREV KONTROLÜ ---
                string bugunTarih = System.DateTime.Now.ToString("dd.MM.yyyy");
                bool degisiklikVar = false;

                // Sadece bugünden önceki tekrarlayan görevleri bul ve bugüne kopyala
                List<GorevData> eklenecekler = new List<GorevData>();
                foreach (var t in this.tasks)
                {
                    if (t.isRepeating && t.taskDate != bugunTarih)
                    {
                        // Bugün aynı isimde görev var mı kontrol et
                        bool bugunVarMi = this.tasks.Exists(x => x.taskDate == bugunTarih && x.taskName == t.taskName);
                        if (!bugunVarMi)
                        {
                            eklenecekler.Add(new GorevData(bugunTarih, t.taskTime, t.taskName, t.durationMinutes, t.isStrictBlock, false, true, t.kategori, t.hatirlaticiDakikaOnce, t.oncelik, t.notlar));
                            degisiklikVar = true;
                        }
                    }
                }

                if (degisiklikVar)
                {
                    this.tasks.AddRange(eklenecekler);
                    SaveTasks();
                }
                // --- SON ---

                // --- ESKİ GÖREV TEMİZLİĞİ ---
                // Tekrarlanmayan ve 7 günden eski görevleri otomatik sil (veri şişmesini önle)
                System.DateTime yediGunOnce = System.DateTime.Now.Date.AddDays(-7);
                int kaldirilanSayi = this.tasks.RemoveAll(t => {
                    if (t.isRepeating) return false; // Tekrarlayan görevleri silme
                    System.DateTime gorevTarihi;
                    if (System.DateTime.TryParseExact(t.taskDate, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out gorevTarihi))
                    {
                        return gorevTarihi.Date < yediGunOnce;
                    }
                    return false;
                });
                if (kaldirilanSayi > 0)
                {
                    Debug.Log($"[ScheduleManager] {kaldirilanSayi} eski görev otomatik temizlendi.");
                    SaveTasks();
                }
                // --- SON ---

                Debug.Log("[ScheduleManager] Görevler başarıyla yüklendi.");
                
                // SADECE bugünün görevlerini UI'a ekle (Tarih filtresi)
                if (_cachedNavigator == null) _cachedNavigator = FindFirstObjectByType<PageNavigator>();
                if (_cachedNavigator != null) {
                    foreach (var gorev in this.tasks) {
                        if (gorev.taskDate == bugunTarih)
                        {
                            _cachedNavigator.GorevKartiEkle(gorev.taskDate, gorev.taskTime, gorev.taskName, gorev.durationMinutes.ToString(), gorev.isStrictBlock, false, gorev.isRepeating, gorev.isCompleted, gorev.kategori, gorev.oncelik, gorev.notlar);
                        }
                    }
                }
                return;
            }
            else
            {
                // Key var ama liste boşsa (kullanıcı her şeyi silmişse)
                tasks.Clear();
                return;
            }
        }
        
        // İlk açılış (Hiç kayıt yoksa)
        tasks.Clear();
    }

    /// <summary>
    /// Bugün kaç görevin tamamlandığını döndürür.
    /// </summary>
    public int BugununTamamlananSayisi()
    {
        string bugunTarih = System.DateTime.Now.ToString("dd.MM.yyyy");
        int sayi = 0;
        foreach (var t in tasks)
        {
            if (t.taskDate == bugunTarih && t.isCompleted) sayi++;
        }
        return sayi;
    }

    /// <summary>
    /// Bugün toplam kaç görev olduğunu döndürür.
    /// </summary>
    public int BugununToplamSayisi()
    {
        string bugunTarih = System.DateTime.Now.ToString("dd.MM.yyyy");
        int sayi = 0;
        foreach (var t in tasks)
        {
            if (t.taskDate == bugunTarih) sayi++;
        }
        return sayi;
    }

    /// <summary>
    /// Son 7 günün tamamlanma oranını döndürür (Yüzde 0-100)
    /// </summary>
    public float HaftalikBasariOrani()
    {
        System.DateTime bugun = System.DateTime.Now.Date;
        System.DateTime yediGunOnce = bugun.AddDays(-7);
        
        int toplamGorev = 0;
        int bitenGorev = 0;

        foreach (var t in tasks)
        {
            if (System.DateTime.TryParseExact(t.taskDate, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out System.DateTime dt))
            {
                if (dt.Date >= yediGunOnce && dt.Date <= bugun)
                {
                    toplamGorev++;
                    if (t.isCompleted) bitenGorev++;
                }
            }
        }

        if (toplamGorev == 0) return 0f;
        return ((float)bitenGorev / toplamGorev) * 100f;
    }
}

