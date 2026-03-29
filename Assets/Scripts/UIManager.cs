using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Referanslar")]
    public ScheduleManager scheduleManager; // Toplam gecikmeyi okuyacağımız sınıf
    public Image stressBar; // UI'daki Image bileşeni (fillAmount değiştireceğiz)
    public TMP_InputField nameInput;
    public TMP_InputField durationInput;

    [Header("Stress Ayarları")]
    public int maxTolerableDelay = 120; // 120 dakika maksimum tolerans
    public Color lowStressColor = new Color(0.3f, 0.3f, 0.3f); // Koyu mat gri
    public Color highStressColor = Color.red; // Parlak kan kırmızı

    private void Start()
    {
        // Başlangıçta StressBar'ı sıfırla
        if (stressBar != null)
        {
            stressBar.fillAmount = 0f;
            stressBar.color = lowStressColor; // Rengi de başlangıç rengine ayarla
        }
        else
        {
            Debug.LogWarning("[UIManager] StressBar Image referansı atanmamış!");
        }

        if (scheduleManager == null)
        {
             Debug.LogWarning("[UIManager] ScheduleManager referansı atanmamış!");
        }
    }

    private void Update()
    {
        // Eğer referanslarımız tamsa her frame güncelleyebiliriz
        // (Performans için ApplyDelay tetiklendiğinde Event ile de yapılabilirdi
        //  ancak en basit yöntem olan Update içinde değeri kontrol ediyoruz)
        if (scheduleManager != null && stressBar != null)
        {
            UpdateStressBarUI();
        }
    }

    /// <summary>
    /// StressBar'ın doluluğunu ve rengini mevcut gecikmeye göre günceller.
    /// </summary>
    private void UpdateStressBarUI()
    {
        // 1. Gecikme Oranını Hesapla (0.0 ile 1.0 arasında)
        // Eğer TotalDelay 30 ise, 30/120 = 0.25 olur
        // Mathf.Clamp01 ile değerin 0 ve 1 dışına çıkmasını engelliyoruz
        float delayRatio = Mathf.Clamp01((float)scheduleManager.TotalDelay / maxTolerableDelay);

        // 2. Barın Doluluğunu (Fill Amount) Güncelle
        stressBar.fillAmount = delayRatio;

        // 3. Barın Rengini (Color.Lerp) Güncelle
        // delayRatio 0 olduğunda lowStressColor, 1 olduğunda highStressColor olur
        stressBar.color = Color.Lerp(lowStressColor, highStressColor, delayRatio);
    }

    public void CreateNewTaskFromUI(string name, string durationString)
    {
        if (int.TryParse(durationString, out int duration))
        {
            if (scheduleManager != null)
            {
                scheduleManager.AddTask(name, duration, false); // şimdilik esnek (isStrict = false)
            }
            else
            {
                Debug.LogWarning("[UIManager] ScheduleManager referansı yok!");
            }
        }
        else
        {
            Debug.LogWarning("[UIManager] Geçersiz süre girişi: " + durationString);
        }
    }

    /// <summary>
    /// TMP_InputField referanslarından gelen verilerle yeni bir görev ekler.
    /// </summary>
    public void AddNewTaskFromUI()
    {
        string taskName = nameInput != null ? nameInput.text : "";
        string durationStr = durationInput != null ? durationInput.text : "";

        // Süreyi çevir, geçersizse 0 kabul et
        if (!int.TryParse(durationStr, out int duration))
        {
            duration = 0;
        }

        if (scheduleManager != null)
        {
            // Görevi şimdilik esnek (false) olarak ekliyoruz
            scheduleManager.AddTask(taskName, duration, false);
        }
        else
        {
            Debug.LogWarning("[UIManager] ScheduleManager referansı yok!");
        }

        // Kutu içlerini temizle
        if (nameInput != null) nameInput.text = "";
        if (durationInput != null) durationInput.text = "";
    }
}
