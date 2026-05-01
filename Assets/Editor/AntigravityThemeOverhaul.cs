using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class AntigravityThemeOverhaul : MonoBehaviour
{
    [MenuItem("Antigravity/Bükülmeleri Düzelt ve Flat-Ice Kur")]
    public static void ResetAndBuildFlatIce()
    {
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null) return;

        // 1. TÜM BÜKÜLMÜŞ GÖRSELLERİ SIFIRLA (Düz Kareye Dönüş)
        Image[] allImages = canvas.GetComponentsInChildren<Image>(true);
        foreach (var img in allImages) {
            img.sprite = null; 
            img.type = Image.Type.Simple;
        }

        // 2. FLAT-ICE RENK PALETİ (DERİN VE NET)
        GameObject bg = GameObject.Find("ArkaPlan") ?? canvas.gameObject;
        bg.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.07f); // Saf Siyah-Antrasit

        foreach (var img in allImages) {
            string n = img.gameObject.name.ToLower();
            // Paneller ve Input Alanları
            if (n.Contains("area") || n.Contains("panel") || n.Contains("input")) {
                img.color = new Color(0.12f, 0.14f, 0.18f); // Koyu Lacivert-Gri
            } 
            // Butonlar
            else if (n.Contains("btn") || n.Contains("button")) {
                img.color = new Color(0.18f, 0.22f, 0.28f); 
            } 
            // İlerleme Barları
            else if (n.Contains("bar") || n.Contains("fill")) {
                img.color = new Color(0.68f, 0.93f, 1f); // Parlak Buz Mavisi
            }
        }

        // 3. TİPOGRAFİ VE YERLEŞİM (NET BEYAZ YAZILAR)
        TextMeshProUGUI[] allTexts = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var txt in allTexts) {
            txt.color = Color.white;
            if (txt.gameObject.name.Contains("Title")) txt.fontWeight = FontWeight.Bold;
        }

        // 4. LAYOUT DÜZELTME (SIKIŞMALARI ENGELLER)
        VerticalLayoutGroup[] vlgs = canvas.GetComponentsInChildren<VerticalLayoutGroup>(true);
        foreach (var vlg in vlgs) {
            vlg.spacing = 15;
            vlg.padding = new RectOffset(20, 20, 20, 20);
        }

        Debug.Log("Antigravity: 'Bükülme' felaketi temizlendi. Flat-Ice Pro kuruldu Boss!");
    }
}
