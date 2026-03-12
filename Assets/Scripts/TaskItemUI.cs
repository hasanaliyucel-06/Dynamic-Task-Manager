using UnityEngine;
using TMPro;

public class TaskItemUI : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI durationText;

    /// <summary>
    /// UI üzerinde görev adını ve süresini günceller.
    /// Katı blok ise isminin yanına ' (KİLİTLİ)' ekler.
    /// </summary>
    public void UpdateUI(string taskName, int duration, bool isStrict)
    {
        if (nameText != null)
        {
            nameText.text = isStrict ? $"{taskName} (KİLİTLİ)" : taskName;
        }

        if (durationText != null)
        {
            durationText.text = $"{duration} dk";
        }
    }
}
