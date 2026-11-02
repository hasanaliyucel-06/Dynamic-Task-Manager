using UnityEngine;
using System.Collections;

public class LocationManager : MonoBehaviour
{
    public float latitude;
    public float longitude;
    public bool locationReady = false;

    IEnumerator Start()
    {
#if UNITY_ANDROID
        // Android runtime konum izni kontrolü
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.FineLocation))
        {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.FineLocation);
            yield return new WaitForSeconds(0.5f);
        }
#endif

        // Kullanıcı konum izni vermediyse işlemi durdur
        if (!Input.location.isEnabledByUser)
        {
            Debug.Log("Antigravity: Konum izni verilmedi.");
            yield break;
        }

        Input.location.Start();

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait < 1 || Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("Antigravity: Konum saptanamadı. Bağlantı zayıf veya kapalı.");
            Input.location.Stop(); // Hata durumunda servisi kapat
            yield break;
        }

        // Konum alındıysa değişkenlere ata
        latitude = Input.location.lastData.latitude;
        longitude = Input.location.lastData.longitude;
        locationReady = true;
        
        Debug.Log($"Antigravity: Konum kilitlendi Boss! Enlem: {latitude}, Boylam: {longitude}");

        // Hata 6 FIX: Pil tüketimini önlemek için koordinat alındıktan sonra GPS servisini durdur
        Input.location.Stop();
    }

    void OnDisable()
    {
        // Servis açık kaldıysa kapat
        if (Input.location.status == LocationServiceStatus.Running || Input.location.status == LocationServiceStatus.Initializing)
        {
            Input.location.Stop();
        }
    }
}