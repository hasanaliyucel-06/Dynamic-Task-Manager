using UnityEngine;

[CreateAssetMenu(fileName = "SiberAsistanConfig", menuName = "Siber Asistan/Config")]
public class AsistanConfig : ScriptableObject
{
    [Header("API Ayarları")]
    public string apiKey = "";
    public string modelName = "gemini-3.1-flash-lite";

    [Header("Sistem İstemleri")]
    [TextArea(5, 10)]
    public string systemPrompt = "Sen kullanıcının kişisel asistanı Siber Asistan'sın.";
}
