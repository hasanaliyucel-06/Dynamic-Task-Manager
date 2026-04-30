using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class AntigravityModernTheme : MonoBehaviour
{
    [MenuItem("Antigravity/Siber Mavi Temayı Uygula")]
    public static void TemaUygula()
    {
        // --- RENK PALETİMİZ ---
        Color zeminSiyahi; ColorUtility.TryParseHtmlString("#050505", out zeminSiyahi); 
        Color kutuSiyahi; ColorUtility.TryParseHtmlString("#121212", out kutuSiyahi); 
        Color elektrikMavisi; ColorUtility.TryParseHtmlString("#0044FF", out elektrikMavisi); 
        Color yaziRengi; ColorUtility.TryParseHtmlString("#F0F0F0", out yaziRengi); 
        Color silikYazi; ColorUtility.TryParseHtmlString("#888888", out silikYazi); 

        // 1. ANA ZEMİNİ BOYAMA
        Image arkaPlan = GameObject.Find("Sayfa_Gorevler")?.GetComponent<Image>();
        if (arkaPlan != null) arkaPlan.color = zeminSiyahi;

        // 2. GİRDİ KUTULARINI (INPUTLAR) BOYAMA VE DÜZENLEME
        TMP_InputField nameInput = GameObject.Find("NameInput")?.GetComponent<TMP_InputField>();
        TMP_InputField durationInput = GameObject.Find("DurationInput")?.GetComponent<TMP_InputField>();
        
        StyleInputField(nameInput, kutuSiyahi, yaziRengi, silikYazi);
        StyleInputField(durationInput, kutuSiyahi, yaziRengi, silikYazi);

        // 3. GÖREV EKLE BUTONUNA NEON EFEKTİ VE RENK
        Button addButton = GameObject.Find("AddTaskButton")?.GetComponent<Button>();
        if (addButton != null)
        {
            Image btnImage = addButton.GetComponent<Image>();
            if (btnImage != null) btnImage.color = elektrikMavisi;

            Shadow glow = addButton.gameObject.GetComponent<Shadow>();
            if (glow == null) glow = addButton.gameObject.AddComponent<Shadow>();
            glow.effectColor = new Color(0f, 0.26f, 1f, 0.6f); 
            glow.effectDistance = new Vector2(0, -5); 

            TextMeshProUGUI btnText = addButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.color = Color.white; 
                btnText.fontStyle = FontStyles.Bold;
            }
        }

        // 4. STRES BARINI NEON MAVİ YAPMA
        GameObject stressBarObj = GameObject.Find("StressBar");
        if (stressBarObj != null)
        {
            Image barBg = stressBarObj.GetComponent<Image>();
            if (barBg != null) barBg.color = kutuSiyahi;

            Transform fillObj = stressBarObj.transform.Find("Fill"); 
            if (fillObj != null)
            {
                Image barFill = fillObj.GetComponent<Image>();
                if (barFill != null) barFill.color = elektrikMavisi;
            }
            else if (barBg != null && barBg.type == Image.Type.Filled)
            {
                 barBg.color = elektrikMavisi;
            }
        }

        // 5. LİSTE ALANINI TEMİZLEME (ŞEFFAF YAPMA)
        Image scrollViewBg = GameObject.Find("Scroll View")?.GetComponent<Image>();
        if (scrollViewBg != null)
        {
            scrollViewBg.color = new Color(0, 0, 0, 0); 
        }

        Debug.Log("Antigravity: Siber Mavi Tema başarıyla uygulandı! Odaya elektrik verildi.");
    }

    private static void StyleInputField(TMP_InputField input, Color bgColor, Color txtColor, Color placeholderColor)
    {
        if (input == null) return;

        Image bg = input.GetComponent<Image>();
        if (bg != null) bg.color = bgColor;

        if (input.textComponent != null) input.textComponent.color = txtColor;
        if (input.placeholder != null) input.placeholder.color = placeholderColor;
    }
}
