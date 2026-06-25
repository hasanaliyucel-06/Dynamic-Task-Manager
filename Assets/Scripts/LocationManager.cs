using UnityEngine;
using System.Collections;

public class LocationManager : MonoBehaviour
{
    public float latitude;
    public float longitude;
    public bool locationReady = false;

    IEnumerator Start()
    {
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
            yield break;
        }

        // Konum alındıysa değişkenlere ata
        latitude = Input.location.lastData.latitude;
        longitude = Input.location.lastData.longitude;
        locationReady = true;
        
        Debug.Log($"Antigravity: Konum kilitlendi Boss! Enlem: {latitude}, Boylam: {longitude}");
    }
}