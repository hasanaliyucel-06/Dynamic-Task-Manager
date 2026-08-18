using System.Collections.Generic;

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Paylaşılan Veri Modelleri
// Tüm sınıflar tarafından kullanılan ortak veri yapıları.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

// ── Görev Veri Modeli ──────────────────────────────────
[System.Serializable]
public class GorevData
{
    public string taskDate;
    public string taskTime;
    public string taskName;
    public int durationMinutes;
    public bool isStrictBlock;
    public bool isCompleted;
    public bool isRepeating;
    public string kategori;
    public int hatirlaticiDakikaOnce;
    
    // Faz 2 Yeni Özellikleri
    public int oncelik; // 0: Düşük, 1: Orta, 2: Yüksek
    public string notlar;
    
    // Faz 5 Yeni Özellikleri (Benzersiz ID)
    public string id;

    public GorevData(string date, string time, string name, int duration, bool strictBlock, bool completed = false, bool repeating = false, string kat = "Genel", int hatirlatici = 15, int oncelikDegeri = 1, string notMetni = "", string gorevId = "")
    {
        taskDate = date;
        taskTime = time;
        taskName = name;
        durationMinutes = duration;
        isStrictBlock = strictBlock;
        isCompleted = completed;
        isRepeating = repeating;
        kategori = kat;
        hatirlaticiDakikaOnce = hatirlatici;
        oncelik = oncelikDegeri;
        notlar = notMetni;
        id = string.IsNullOrEmpty(gorevId) ? System.Guid.NewGuid().ToString() : gorevId;
    }
}

// JsonUtility listeleri doğrudan desteklemediği için yardımcı sınıf
[System.Serializable]
public class TaskWrapper
{
    public List<GorevData> tasks;
}

// ── Uzun Vadeli Hedef Modeli ───────────────────────────
[System.Serializable]
public class UzunVadeliHedef
{
    public string id;
    public string hedefAdi;
    public int kalanGun;
    public string baslangicTarihi;

    public UzunVadeliHedef()
    {
        id = System.Guid.NewGuid().ToString();
    }
}

[System.Serializable]
public class HedefListesiWrapper
{
    public List<UzunVadeliHedef> hedefler;
}

// ── Sohbet Veri Modeli ─────────────────────────────────
[System.Serializable]
public class SohbetMesaji
{
    public string metin;
    public bool kullaniciMi;
}

[System.Serializable]
public class SohbetGecmisi
{
    public List<SohbetMesaji> mesajlar = new List<SohbetMesaji>();
}

// ── Gemini API Response Modelleri ──────────────────────
[System.Serializable]
public class GeminiResponse
{
    public GeminiCandidate[] candidates;
}

[System.Serializable]
public class GeminiCandidate
{
    public GeminiContent content;
}

[System.Serializable]
public class GeminiContent
{
    public GeminiPart[] parts;
}

[System.Serializable]
public class GeminiPart
{
    public string text;
}

[System.Serializable]
public class GeminiRequest
{
    public GeminiContent[] contents;
    public GeminiGenerationConfig generationConfig;
}

[System.Serializable]
public class GeminiGenerationConfig
{
    public string responseMimeType;
}

// ── AI Structured Output Modelleri ─────────────────────
[System.Serializable]
public class AIGorev
{
    public string tarih;
    public string saat;
    public string gorevAdi;
    public int sure;
    public string kategori;
    public string notlar;
}

[System.Serializable]
public class AIPlanlamaSonucu
{
    public bool temizle;
    public string mesaj;
    public List<string> yeniBilgiler;
    public List<AIGorev> gorevler;
}

// ── Kullanıcı Profili (Kalıcı Bellek) ──────────────────
[System.Serializable]
public class KullaniciProfili
{
    public List<string> bilgiler = new List<string>();
}
