using System.Collections;
using UnityEngine;

public class LocationManager : MonoBehaviour
{
    // Butonun tetiklediği ana metot bu (Inspector'da bağlı olan)
    public void CheckLocationPenalty()
    {
        Debug.LogWarning("GPS Sensörleri Uyandırılıyor...");
        StartCoroutine(GetLocationRoutine());
    }

    private IEnumerator GetLocationRoutine()
    {
        // 1. AŞAMA: Eğer kodu bilgisayarda (Unity Editor) çalıştırıyorsak
#if UNITY_EDITOR
            yield return new WaitForSeconds(1.5f); // Uydulara bağlanıyormuş gibi 1.5 saniye bekle
            // Sana özel bir easter egg: Bilgisayardayken Ankara koordinatlarını döndürsün :)
            Debug.LogWarning("GERÇEK KONUM ALINDI (PC Simülasyonu): Enlem 39.92077 - Boylam 32.85411"); 
            yield break; // Metodu burada bitir, telefona özel kısımlara inme
#endif

        // 2. AŞAMA: Eğer kodu Android/iOS telefonda çalıştırıyorsak
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogError("Kullanıcı GPS izni vermemiş veya konum kapalı!");
            yield break;
        }

        Input.location.Start(10f, 10f); // Hassasiyeti ayarlayıp GPS'i başlat

        int maxWait = 20; // Maksimum 20 saniye bekle
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait <= 0)
        {
            Debug.LogError("GPS Zaman Aşımı: Uydular bulunamadı.");
            Input.location.Stop();
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogError("GPS Bağlantı Hatası: Cihaz konumu saptayamadı.");
            yield break;
        }

        // Zafere Ulaştığımız An:
        float latitude = Input.location.lastData.latitude;
        float longitude = Input.location.lastData.longitude;
        Debug.LogWarning("GERÇEK KONUM ALINDI (Mobil): Enlem " + latitude + " - Boylam " + longitude);

        Input.location.Stop(); // Bataryayı emmesin diye GPS'i anında kapat
    }
}