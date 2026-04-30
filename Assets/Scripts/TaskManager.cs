using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TaskManager : MonoBehaviour
{
    [Header("UI Referansları")]
    public TMP_InputField nameInput;
    public TMP_InputField durationInput;
    public Button addTaskButton;

    [Header("Prefab & Liste")]
    public GameObject taskPrefab;
    public Transform contentPanel;

    void Start()
    {
        if(addTaskButton != null)
        {
            addTaskButton.onClick.AddListener(YeniGorevEkle);
        }
    }

    public void YeniGorevEkle()
    {
        if(string.IsNullOrEmpty(nameInput.text) || string.IsNullOrEmpty(durationInput.text)) return;

        // Prefab'ı Content içine kopyala (Kalıbı bas)
        GameObject yeniGorev = Instantiate(taskPrefab, contentPanel);
        
        // İçindeki yazıları bul (0. Görev Adı, 1. Süre)
        TextMeshProUGUI[] yazilar = yeniGorev.GetComponentsInChildren<TextMeshProUGUI>();
        
        if(yazilar.Length >= 2)
        {
            yazilar[0].text = nameInput.text;
            yazilar[1].text = durationInput.text + " dk";
        }

        // Girdileri temizle
        nameInput.text = "";
        durationInput.text = "";
    }
}
