using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class AntigravityAssistantBuilder : MonoBehaviour
{
    [MenuItem("Antigravity/AI Asistanı Kusursuzlaştır")]
    public static void BuildPerfectAssistant()
    {
        GameObject asistanSayfasi = GameObject.Find("Sayfa_Asistan");
        if (asistanSayfasi == null) return;

        VerticalLayoutGroup vlg = asistanSayfasi.GetComponent<VerticalLayoutGroup>();
        vlg.childControlHeight = true; 
        vlg.childForceExpandHeight = false;
        vlg.childAlignment = TextAnchor.UpperCenter;
        
        // ÇENTİK KORUMASI (Üstten 80 piksel boşluk bırakarak başlığın yutulmasını engeller)
        vlg.padding = new RectOffset(20, 20, 80, 20); 

        while (asistanSayfasi.transform.childCount > 0)
            DestroyImmediate(asistanSayfasi.transform.GetChild(0).gameObject);

        // Başlık
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(asistanSayfasi.transform, false);
        TextMeshProUGUI tmpTitle = titleObj.AddComponent<TextMeshProUGUI>();
        tmpTitle.text = "SİBER ASİSTAN";
        tmpTitle.fontSize = 35;
        tmpTitle.fontStyle = FontStyles.Bold;
        tmpTitle.alignment = TextAlignmentOptions.Center;
        tmpTitle.color = Color.white;
        LayoutElement leTitle = titleObj.AddComponent<LayoutElement>();
        leTitle.minHeight = 50;

        // Sohbet Alanı
        GameObject chatArea = new GameObject("ChatScrollArea");
        chatArea.transform.SetParent(asistanSayfasi.transform, false);
        Image chatBg = chatArea.AddComponent<Image>();
        ColorUtility.TryParseHtmlString("#0A0A0C", out Color chatColor);
        chatBg.color = chatColor;
        LayoutElement leChat = chatArea.AddComponent<LayoutElement>();
        leChat.flexibleHeight = 1; 
        
        ScrollRect scroll = chatArea.AddComponent<ScrollRect>();
        scroll.horizontal = false; scroll.vertical = true;
        
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(chatArea.transform, false);
        viewport.AddComponent<RectTransform>().sizeDelta = Vector2.zero;
        viewport.AddComponent<RectMask2D>();
        
        RectTransform vpRect = viewport.GetComponent<RectTransform>();
        vpRect.anchorMin = Vector2.zero; vpRect.anchorMax = Vector2.one;
        vpRect.offsetMin = Vector2.zero; vpRect.offsetMax = Vector2.zero;

        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1); contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        
        VerticalLayoutGroup contentVlg = content.AddComponent<VerticalLayoutGroup>();
        // Yazıların kenara yapışmasını engelleyen geniş iç boşluk
        contentVlg.padding = new RectOffset(30, 30, 30, 30); 
        contentVlg.spacing = 25;
        contentVlg.childControlWidth = true; contentVlg.childControlHeight = true;
        contentVlg.childForceExpandWidth = true; contentVlg.childForceExpandHeight = false;
        
        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        scroll.viewport = vpRect; scroll.content = contentRect;

        // Girdi Alanı
        GameObject inputArea = new GameObject("InputArea");
        inputArea.transform.SetParent(asistanSayfasi.transform, false);
        HorizontalLayoutGroup inputHlg = inputArea.AddComponent<HorizontalLayoutGroup>();
        inputHlg.spacing = 15;
        inputHlg.childControlWidth = true; inputHlg.childControlHeight = true;
        LayoutElement leInputArea = inputArea.AddComponent<LayoutElement>();
        leInputArea.minHeight = 90; 
        leInputArea.flexibleHeight = 0;

        GameObject inputObj = new GameObject("ChatInput");
        inputObj.transform.SetParent(inputArea.transform, false);
        Image inputBg = inputObj.AddComponent<Image>();
        ColorUtility.TryParseHtmlString("#121212", out Color inputBgColor);
        inputBg.color = inputBgColor;
        LayoutElement leInput = inputObj.AddComponent<LayoutElement>();
        leInput.flexibleWidth = 1;

        GameObject textArea = new GameObject("Text Area");
        textArea.transform.SetParent(inputObj.transform, false);
        RectTransform textAreaRect = textArea.AddComponent<RectTransform>();
        textAreaRect.anchorMin = Vector2.zero; textAreaRect.anchorMax = Vector2.one;
        textAreaRect.offsetMin = new Vector2(25, 10); 
        textAreaRect.offsetMax = new Vector2(-25, -10);
        textArea.AddComponent<RectMask2D>();

        GameObject placeholder = new GameObject("Placeholder");
        placeholder.transform.SetParent(textArea.transform, false);
        TextMeshProUGUI tmpPh = placeholder.AddComponent<TextMeshProUGUI>();
        tmpPh.text = "Asistana komut ver...";
        tmpPh.fontSize = 24;
        tmpPh.alignment = TextAlignmentOptions.MidlineLeft;
        ColorUtility.TryParseHtmlString("#555555", out Color phColor);
        tmpPh.color = phColor;
        
        RectTransform phRect = placeholder.GetComponent<RectTransform>();
        phRect.anchorMin = Vector2.zero; phRect.anchorMax = Vector2.one;
        phRect.offsetMin = Vector2.zero; phRect.offsetMax = Vector2.zero;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(textArea.transform, false);
        TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
        tmpText.fontSize = 24;
        tmpText.alignment = TextAlignmentOptions.MidlineLeft;
        tmpText.color = Color.white;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero; textRect.offsetMax = Vector2.zero;

        TMP_InputField inputField = inputObj.AddComponent<TMP_InputField>();
        inputField.textViewport = textAreaRect;
        inputField.textComponent = tmpText;
        inputField.placeholder = tmpPh;

        GameObject sendBtnObj = new GameObject("SendButton");
        sendBtnObj.transform.SetParent(inputArea.transform, false);
        Image sendBg = sendBtnObj.AddComponent<Image>();
        ColorUtility.TryParseHtmlString("#0044FF", out Color aiColor);
        sendBg.color = aiColor;
        LayoutElement leSend = sendBtnObj.AddComponent<LayoutElement>();
        leSend.minWidth = 140;
        
        Button sendBtn = sendBtnObj.AddComponent<Button>(); 

        GameObject sendTxtObj = new GameObject("Text");
        sendTxtObj.transform.SetParent(sendBtnObj.transform, false);
        TextMeshProUGUI tmpSend = sendTxtObj.AddComponent<TextMeshProUGUI>();
        tmpSend.text = "İLET";
        tmpSend.fontSize = 26;
        tmpSend.fontStyle = FontStyles.Bold;
        tmpSend.alignment = TextAlignmentOptions.Center;
        tmpSend.color = Color.white;

        // Motoru Bağla
        SiberAsistan motor = asistanSayfasi.GetComponent<SiberAsistan>();
        if (motor == null) motor = asistanSayfasi.AddComponent<SiberAsistan>();
        
        motor.inputField = inputField;
        motor.chatContent = content.transform;
        motor.sendButton = sendBtn;
        motor.scrollRect = scroll;

        EditorUtility.SetDirty(asistanSayfasi);
        Debug.Log("Antigravity: Çentik koruması eklendi, gerçek Yapay Zeka motoru hata dedektifiyle bağlandı!");
    }
}
