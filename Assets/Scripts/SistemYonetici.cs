using UnityEngine;
using UnityEngine.UIElements;

public class SistemYonetici : MonoBehaviour
{

    private Button btnSohbetSil;
    private Button btnGorevSil;
    private Label lblVeriDurum;

    public void Initialize(VisualElement root)
    {
        btnSohbetSil = root.Q<Button>("btnSohbetSil");
        btnGorevSil = root.Q<Button>("btnGorevSil");
        lblVeriDurum = root.Q<Label>("lblVeriDurum");

        if (btnSohbetSil != null)
        {
            btnSohbetSil.clicked += () => {
                PlayerPrefs.DeleteKey("SohbetGecmisi");
                PlayerPrefs.Save();
                GosterDurum(lblVeriDurum, "Sohbet geçmişi silindi! (Yeniden başlatın)", true);
            };
        }

        if (btnGorevSil != null)
        {
            btnGorevSil.clicked += () => {
                PlayerPrefs.DeleteKey("SavedTasks");
                PlayerPrefs.DeleteKey("AktifHedefler");
                PlayerPrefs.Save();
                
                ScheduleManager sManager = FindFirstObjectByType<ScheduleManager>();
                if (sManager != null)
                {
                    sManager.tasks.Clear();
                }

                GorevKartiYonetici gManager = FindFirstObjectByType<GorevKartiYonetici>();
                if (gManager != null)
                {
                    gManager.TumGorevleriTemizle();
                }

                GosterDurum(lblVeriDurum, "Tüm görevler silindi!", true);
            };
        }
    }

    private void GosterDurum(Label lbl, string mesaj, bool basarili)
    {
        if (lbl == null) return;
        lbl.text = mesaj;
        lbl.style.color = new StyleColor(basarili ? new Color(0.1f, 0.73f, 0.5f) : new Color(0.93f, 0.26f, 0.26f));
        
        // 3 saniye sonra yazıyı sil
        lbl.schedule.Execute(() => {
            lbl.text = "";
        }).StartingIn(3000);
    }
}
