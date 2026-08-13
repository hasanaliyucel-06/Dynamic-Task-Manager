using UnityEngine;

/// <summary>
/// Uygulama içi ses efektlerini ve mobil cihaz titreşimlerini yönetir.
/// Singleton yapısındadır.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SesYonetici : MonoBehaviour
{
    public static SesYonetici Instance { get; private set; }

    [Header("Ses Efektleri (Inspector'dan atayın)")]
    public AudioClip clickSound;
    public AudioClip successSound;
    public AudioClip messageSound;
    public AudioClip notificationSound;
    public AudioClip deleteSound;

    private AudioSource audioSource;
    private bool isMuted = false;

    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        
        Instance = this;
        // Eğer sahneler arası geçiş yoksa DontDestroyOnLoad şart değil, 
        // ama genel yönetici olduğu için eklemekte fayda var.
        DontDestroyOnLoad(this.gameObject);
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
        }

        // Mute ayarını yükle
        isMuted = PlayerPrefs.GetInt("IsMuted", 0) == 1;
    }

    public void ToggleMute(bool muteStatus)
    {
        isMuted = muteStatus;
        PlayerPrefs.SetInt("IsMuted", isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    public bool IsMuted()
    {
        return isMuted;
    }

    // --- SES ÇALMA FONKSİYONLARI ---

    private void PlaySound(AudioClip clip)
    {
        if (isMuted || clip == null || audioSource == null) return;
        audioSource.PlayOneShot(clip);
    }

    public void PlayClick()
    {
        PlaySound(clickSound);
    }

    public void PlaySuccess()
    {
        PlaySound(successSound);
    }

    public void PlayMessageReceived()
    {
        PlaySound(messageSound);
    }

    public void PlayNotification()
    {
        PlaySound(notificationSound);
    }

    public void PlayDelete()
    {
        PlaySound(deleteSound);
    }

    // --- TİTREŞİM (HAPTIC) ---

    public void Vibrate(bool heavy = false)
    {
        if (isMuted) return; // Mute iken titreşimi de kapatıyoruz.
        
        // Sadece mobil cihazlarda titreşimi çalıştırır
        #if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
        #endif
    }
}
