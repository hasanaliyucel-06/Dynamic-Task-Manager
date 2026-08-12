using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;

/// <summary>
/// Uygulama içi bildirimleri yöneten sınıf.
/// Görev saati geldiğinde UI Toolkit üzerinden banner gösterir.
/// </summary>
public class BildirimYonetici : MonoBehaviour
{
    private VisualElement bildirimBanner;
    private Label lblBildirimMetni;
    private Button btnBildirimKapat;

    private ScheduleManager scheduleManager;
    private HashSet<string> bildirilenGorevler = new HashSet<string>();

    public void Initialize(VisualElement root)
    {
        bildirimBanner = root.Q<VisualElement>("bildirimBanner");
        lblBildirimMetni = root.Q<Label>("lblBildirimMetni");
        btnBildirimKapat = root.Q<Button>("btnBildirimKapat");

        if (btnBildirimKapat != null)
        {
            btnBildirimKapat.clicked += () => {
                if (bildirimBanner != null)
                {
                    bildirimBanner.style.top = -300; // Tamamen Gizle
                }
            };
        }

        scheduleManager = FindFirstObjectByType<ScheduleManager>();

        // Her 10 saniyede bir saat kontrolü yap
        InvokeRepeating("BildirimKontrol", 2f, 10f);
    }

    private void BildirimKontrol()
    {
        if (scheduleManager == null || bildirimBanner == null) return;

        string bugun = DateTime.Now.ToString("dd.MM.yyyy");
        string suAnSaat = DateTime.Now.ToString("HH:mm");

        foreach (var gorev in scheduleManager.tasks)
        {
            if (!gorev.isCompleted && gorev.taskDate == bugun && gorev.taskTime == suAnSaat)
            {
                // Zaten bildirim gösterildiyse tekrar etme
                string gorevKey = gorev.taskName + "_" + gorev.taskDate + "_" + gorev.taskTime;
                if (!bildirilenGorevler.Contains(gorevKey))
                {
                    bildirilenGorevler.Add(gorevKey);
                    GosterBildirim("Sıradaki Görev:\n" + gorev.taskName);
                }
            }
        }
    }

    private void GosterBildirim(string metin)
    {
        if (lblBildirimMetni != null) lblBildirimMetni.text = metin;
        if (bildirimBanner != null)
        {
            // Banner'ı aşağı kaydırarak göster
            bildirimBanner.style.top = 20; 
            
            // Eğer isterseniz burada AudioSource ile bir "Ding!" sesi çalabilirsiniz.
            // GetComponent<AudioSource>().Play();
        }
    }
}
