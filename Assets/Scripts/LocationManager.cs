using System.Collections;
using UnityEngine;
using UnityEngine.Android; // Android izin istemek için gerekli

public class LocationManager : MonoBehaviour
{
    [Header("Mevcut Konum Verileri")]
    public float latitude;
    public float longitude;

    [Header("Debug Hedef Konumu (İsteğe Bağlı)")]
    public float targetLat = 41.0082f; // Örnek: İstanbul (Sultanahmet)
    public float targetLon = 28.9784f;

    [Header("Simülasyon Ayarları (PC / Editör İçin)")]
    public bool isSimulationMode = true;
    public float simLatitude = 37.8444f;
    public float simLongitude = 27.8355f;

    [Header("Sistem Referansları")]
    public ScheduleManager scheduleManager;

    private bool isLocationServiceRunning = false;

    private void Start()
    {
        if (isSimulationMode)
        {
            Debug.Log("[LocationManager] Simülasyon modu aktif! Gerçek GPS başlatılmıyor, sahte veriler kullanılacak.");
            latitude = simLatitude;
            longitude = simLongitude;
            isLocationServiceRunning = true; // Simülasyonda servisin çalıştığını varsayıyoruz
        }
        else
        {
            // Gps servisini başlatmak için Coroutine çalıştırıyoruz
            StartCoroutine(StartLocationService());
        }
    }

    private void Update()
    {
        // Eğer servis çalışıyorsa her frame sadece koordinatları çekip güncelliyoruz
        if (isLocationServiceRunning)
        {
            if (isSimulationMode)
            {
                // Inspector'dan anlık değişimi görebilmek için simülasyon değerlerini okuyoruz
                latitude = simLatitude;
                longitude = simLongitude;
            }
            else
            {
                // Gerçek GPS'ten veriyi alıyoruz
                latitude = Input.location.lastData.latitude;
                longitude = Input.location.lastData.longitude;
            }
        }
    }

    /// <summary>
    /// Mevcut konum ile hedef konum arasındaki mesafeyi kontrol eder.
    /// Eğer hedef konumdan 5 kilometreden daha uzaktaysak ScheduleManager'a
    /// trafik cezası (30 dk) yansıtır.
    /// </summary>
    public void CheckLocationPenalty()
    {
        if (!isLocationServiceRunning)
        {
            Debug.LogWarning("[LocationManager] Konum servisi henüz aktif değil, penalty kontrolü yapılamıyor.");
            return;
        }

        if (scheduleManager == null)
        {
            Debug.LogError("[LocationManager] ScheduleManager referansı atanmamış!");
            return;
        }

        // Kendi yazdığımız GetDistanceTo fonksiyonu ile hedefle aramızdaki km cinsinden mesafeyi buluyoruz
        float distance = GetDistanceTo(targetLat, targetLon);

        if (distance > 5f)
        {
            Debug.Log($"[LocationManager] Hedefe çok uzaksın! (Mesafe: {distance:F2} km). Trafik gecikmesi (30 dk) eklendi.");
            scheduleManager.ApplyDelay(30);
        }
        else
        {
            Debug.Log($"[LocationManager] Hedefe yeterince yakınsın (Mesafe: {distance:F2} km). Gecikme uygulanmadı.");
        }
    }

    /// <summary>
    /// Unity'nin location servisini başlatmayı dener, gerekli izinleri kontrol eder.
    /// </summary>
    private IEnumerator StartLocationService()
    {
        // Önce konum izni verilmiş mi kontrol ediyoruz (Android için)
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            // Kullanıcının izin vermesi için kısa bir süre bekleyebiliriz veya callbacks kullanılabilir.
            // Bu örnekte basitçe bir miktar bekliyoruz.
            yield return new WaitForSeconds(2f); 
        }
#endif

        // Cihazın konum servisleri açık mı?
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogWarning("[LocationManager] Konum servisleri cihazda kapalı. Lütfen ayarlardan açın.");
            yield break;
        }

        // Location servisini başlat (İstenen Doğruluk: 10 metre, Güncelleme Mesafesi: 10 metre)
        Input.location.Start(10f, 10f);

        // Servisin başlatılmasını bekle
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Bekleme süresi aşıldıysa
        if (maxWait < 1)
        {
            Debug.LogWarning("[LocationManager] Konum servisi başlatılamadı (Zaman aşımı).");
            yield break;
        }

        // Bağlantı başarısız olduysa
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogWarning("[LocationManager] Cihazın konumuna ulaşılamıyor.");
            yield break;
        }
        else
        {
            // Servis başarıyla başlatıldı ve çalışıyor
            isLocationServiceRunning = true;
            
            // İlk verileri hemen al
            latitude = Input.location.lastData.latitude;
            longitude = Input.location.lastData.longitude;
            
            Debug.Log($"[LocationManager] Konum servisi başarıyla başlatıldı! İlk Koordinatlar - Enlem: {latitude}, Boylam: {longitude}");
        }
    }

    /// <summary>
    /// Haversine formülünü kullanarak mevcut konum ile verilen hedef konum arasındaki 
    /// kuş uçuşu mesafeyi kilometre (km) cinsinden hesaplar.
    /// </summary>
    /// <param name="targetLatitude">Hedef Enlem</param>
    /// <param name="targetLongitude">Hedef Boylam</param>
    /// <returns>Kilometre cinsinden mesafe</returns>
    public float GetDistanceTo(float targetLatitude, float targetLongitude)
    {
        // Dünya'nın yarıçapı (ortalama) - kilometre
        float R = 6371f;

        // Dereceleri radyana dönüştür
        float lat1Rad = latitude * Mathf.Deg2Rad;
        float lon1Rad = longitude * Mathf.Deg2Rad;
        float lat2Rad = targetLatitude * Mathf.Deg2Rad;
        float lon2Rad = targetLongitude * Mathf.Deg2Rad;

        // Farklar
        float dLat = lat2Rad - lat1Rad;
        float dLon = lon2Rad - lon1Rad;

        // Haversine formülü matematiği
        float a = Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2) +
                  Mathf.Cos(lat1Rad) * Mathf.Cos(lat2Rad) *
                  Mathf.Sin(dLon / 2) * Mathf.Sin(dLon / 2);

        float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));

        // Mesafeyi km olarak döndür
        return R * c;
    }

    /// <summary>
    /// Obje yok edildiğinde veya disable dildiğinde servisi durdurmak iyidir.
    /// </summary>
    private void OnDisable()
    {
        if (isLocationServiceRunning && !isSimulationMode)
        {
            Input.location.Stop();
            isLocationServiceRunning = false;
        }
    }
}
