using UnityEngine;
using UnityEngine.UIElements;

public class PageNavigator : MonoBehaviour
{
    private UIDocument uiDocument;

    private VisualElement pageSohbet;
    private VisualElement pageGorevler;

    private Button btnTabSohbet;
    private Button btnTabGorevler;

    void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null) return;

        var root = uiDocument.rootVisualElement;

        pageSohbet = root.Q<VisualElement>("PageSohbet");
        pageGorevler = root.Q<VisualElement>("PageGorevler");

        btnTabSohbet = root.Q<Button>("btnTabSohbet");
        btnTabGorevler = root.Q<Button>("btnTabGorevler");

        if (btnTabSohbet != null)
        {
            btnTabSohbet.clicked += () => SwitchPage(true);
        }

        if (btnTabGorevler != null)
        {
            btnTabGorevler.clicked += () => SwitchPage(false);
        }

        // Başlangıçta Sohbet sekmesini aç
        SwitchPage(true);
    }

    private void SwitchPage(bool sohbetAktifMi)
    {
        if (pageSohbet == null || pageGorevler == null) return;

        if (sohbetAktifMi)
        {
            pageSohbet.style.display = DisplayStyle.Flex;
            pageGorevler.style.display = DisplayStyle.None;

            if (btnTabSohbet != null) btnTabSohbet.AddToClassList("tab-button-active");
            if (btnTabGorevler != null) btnTabGorevler.RemoveFromClassList("tab-button-active");
        }
        else
        {
            pageSohbet.style.display = DisplayStyle.None;
            pageGorevler.style.display = DisplayStyle.Flex;

            if (btnTabSohbet != null) btnTabSohbet.RemoveFromClassList("tab-button-active");
            if (btnTabGorevler != null) btnTabGorevler.AddToClassList("tab-button-active");
        }
    }
}
