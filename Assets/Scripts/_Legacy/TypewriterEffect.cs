using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TypewriterEffect : MonoBehaviour
{
    public float yazmaHizi = 0.03f; 
    private TextMeshProUGUI textBileseni;

    void Awake()
    {
        textBileseni = GetComponent<TextMeshProUGUI>();
    }

    public void MetniYazdir(string yeniMetin)
    {
        StopAllCoroutines(); 
        StartCoroutine(HarfHarfYaz(yeniMetin));
    }

    private IEnumerator HarfHarfYaz(string metin)
    {
        textBileseni.text = ""; 
        foreach (char harf in metin.ToCharArray())
        {
            textBileseni.text += harf;
            yield return new WaitForSeconds(yazmaHizi);
        }
    }
}
