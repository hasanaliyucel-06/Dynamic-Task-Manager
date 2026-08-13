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

        DateTime simdi = DateTime.Now;
        string bugun = simdi.ToString("dd.MM.yyyy");

        foreach (var gorev in scheduleManager.tasks)
        {
            if (!gorev.isCompleted && gorev.taskDate == bugun)
            {
                if (DateTime.TryParseExact(gorev.taskTime, "HH:mm", null, System.Globalization.DateTimeStyles.None, out DateTime gorevSaati))
                {
                    // Görev saatini bugünün tarihine uyarla
                    DateTime gercekGorevSaati = new DateTime(simdi.Year, simdi.Month, simdi.Day, gorevSaati.Hour, gorevSaati.Minute, 0);
                    
                    // Hatırlatıcı dakikasını çıkar
                    DateTime bildirimSaati = gercekGorevSaati.AddMinutes(-gorev.hatirlaticiDakikaOnce);

                    // Eğer şu anki zaman, bildirim saatine eşit veya onu geçmişse (en fazla 1 dakika geçmişse)
                    if (simdi >= bildirimSaati && simdi <= bildirimSaati.AddMinutes(1))
                    {
                        // Zaten bildirim gösterildiyse tekrar etme
                        string gorevKey = gorev.taskName + "_" + gorev.taskDate + "_" + gorev.taskTime;
                        if (!bildirilenGorevler.Contains(gorevKey))
                        {
                            bildirilenGorevler.Add(gorevKey);
                            string mesaj = $"Sıradaki Görev ({gorev.hatirlaticiDakikaOnce} dk kaldı):\n{gorev.taskName}";
                            GosterBildirim(mesaj);
                        }
                    }
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
            
            // 8 saniye sonra otomatik kapat
            bildirimBanner.schedule.Execute(() => {
                bildirimBanner.style.top = -300;
            }).StartingIn(8000);
            
            // Ses ve Titreşim
            if (SesYonetici.Instance != null)
            {
                SesYonetici.Instance.PlayNotification();
                SesYonetici.Instance.Vibrate(true); // Ağır titreşim
            }
        }
    }
}
