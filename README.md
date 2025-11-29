# Text Capture App - OCR and Export

Modern, modüler ve SOLID prensiplere uygun .NET 8 WPF masaüstü uygulaması.

## 🎯 Özellikler

- **📷 Ekran Görüntüsü Yakalama**: Tam ekran veya bölge seçerek görüntü yakala
- **🔍 OCR (Optik Karakter Tanıma)**: Tesseract OCR ile çok dilli metin çıkarma
- **💾 Çoklu Format Export**: TXT, PDF, DOCX formatlarında kaydetme
- **🔊 Text-to-Speech**: Metni sese dönüştürme (WAV formatı)

## 🏗️ Mimari

Proje katmanlı, modüler bir mimariye sahiptir ve her katman ayrı bir class library olarak tasarlanmıştır:

### Projeler

```
TextCaptureApp/
├── TextCaptureApp.Core          # Interface'ler, DTO'lar, modeller
├── TextCaptureApp.ScreenCapture # Ekran görüntüsü alma servisi
├── TextCaptureApp.Ocr           # OCR servisi (Tesseract)
├── TextCaptureApp.Export        # Export servisleri (PDF, DOCX, TXT)
├── TextCaptureApp.Tts           # Text-to-Speech servisi
└── TextCaptureApp.UI            # WPF Kullanıcı Arayüzü
```

### Bağımlılıklar

- **UI** → Core, Ocr, ScreenCapture, Export, Tts
- **Ocr, ScreenCapture, Export, Tts** → Core
- **Core** → Hiçbir şeye bağımlı değil

### Teknolojiler

- **.NET 8** (Windows)
- **WPF** (Windows Presentation Foundation)
- **Dependency Injection** (Microsoft.Extensions.Hosting)
- **Tesseract OCR** - Metin tanıma
- **iTextSharp** - PDF export
- **DocumentFormat.OpenXml** - DOCX export
- **NAudio** - Ses dosyası işleme

## 📋 Gereksinimler

- .NET 8 SDK veya üzeri
- Windows 10/11
- Tesseract dil dosyaları (tessdata klasörü)

## 🚀 Kurulum

### 1. Projeyi Klonlayın

```bash
git clone <repository-url>
cd app89
```

### 2. Tesseract Dil Dosyalarını İndirin

OCR özelliğinin çalışması için Tesseract dil dosyalarına ihtiyacınız var:

1. Proje kök dizininde `tessdata` klasörü oluşturun
2. [Tesseract Language Data](https://github.com/tesseract-ocr/tessdata) deposundan dil dosyalarını indirin:
   - İngilizce: `eng.traineddata`
   - Türkçe: `tur.traineddata`
   - Almanca: `deu.traineddata`
   - Fransızca: `fra.traineddata`
   - İspanyolca: `spa.traineddata`

3. İndirilen `.traineddata` dosyalarını `tessdata` klasörüne koyun

Klasör yapısı:
```
app89/
├── tessdata/
│   ├── eng.traineddata
│   ├── tur.traineddata
│   └── ...
├── TextCaptureApp.UI/
└── ...
```

### 3. Build ve Çalıştır

```bash
dotnet build
dotnet run --project TextCaptureApp.UI
```

## 💡 Kullanım

1. **Ekran Görüntüsü Al**
   - "📷 Capture Screen" - Tüm ekranı yakalar
   - "🖼️ Capture Region" - Seçili bölgeyi yakalar

2. **Metin Çıkar**
   - Dil seçin (İngilizce, Türkçe, vb.)
   - "🔍 Extract Text (OCR)" butonuna tıklayın
   - Çıkarılan metin sağ panelde görünür

3. **Export Et**
   - "💾 Export TXT" - Düz metin olarak kaydet
   - "📄 Export PDF" - PDF belgesi oluştur
   - "📝 Export DOCX" - Word belgesi oluştur

4. **Sese Çevir**
   - "🔊 Generate Speech" - WAV ses dosyası oluştur

## 🔧 Geliştirme Prensipleri

Bu proje aşağıdaki yazılım geliştirme prensiplerine sıkı sıkıya uygundur:

### SOLID Prensipleri

- ✅ **Single Responsibility**: Her sınıf tek bir sorumluluğa sahip
- ✅ **Open/Closed**: Genişlemeye açık, değişikliğe kapalı
- ✅ **Liskov Substitution**: Interface'ler doğru implement edilmiş
- ✅ **Interface Segregation**: Küçük, özelleşmiş interface'ler
- ✅ **Dependency Inversion**: Bağımlılıklar interface'ler üzerinden

### Diğer Prensipler

- ✅ **Separation of Concerns**: Katmanlar birbirinden bağımsız
- ✅ **Dependency Injection**: Constructor injection kullanımı
- ✅ **No Static Code**: Tüm servisler instance-based
- ✅ **No God Objects**: Tek bir sınıfta aşırı sorumluluk yok
- ✅ **Testability**: Unit test edilebilir yapı

## 🧪 Test

Servislerin unit test'leri için:

```bash
# Test projeleri eklenebilir
dotnet test
```

## 📝 Notlar

### TTS (Text-to-Speech) Hakkında

Mevcut TTS implementasyonu basit bir placeholder'dır ve sessiz WAV dosyası üretir. 
Gerçek TTS için aşağıdaki seçeneklerden biri kullanılabilir:

- **Azure Cognitive Services Speech SDK**
- **Google Cloud Text-to-Speech**
- **Windows SAPI (System.Speech)** - Sadece Windows

### Platform Desteği

Proje Windows platformu için optimize edilmiştir. Linux/macOS desteği için:
- ScreenCapture servisi platform-specific implementasyon gerektirir
- System.Drawing.Common alternatifi (SkiaSharp, ImageSharp) kullanılabilir

## 🤝 Katkıda Bulunma

1. Fork yapın
2. Feature branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Commit yapın (`git commit -m 'Add amazing feature'`)
4. Push edin (`git push origin feature/amazing-feature`)
5. Pull Request açın

## 📄 Lisans

Bu proje eğitim amaçlıdır.

## 👨‍💻 Geliştirici

C# ve .NET 8 ile SOLID prensiplere uygun, modüler mimari örneği.

---

**Not**: Tesseract dil dosyalarını indirmeyi unutmayın, aksi takdirde OCR özelliği çalışmayacaktır!

