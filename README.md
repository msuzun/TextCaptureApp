# Text Capture App - OCR and Export

Modern, modular .NET 8 WPF desktop application following SOLID principles.

## 🎯 Features

- **📷 Screen Capture**: Capture full screen or select a region
- **🔍 OCR (Optical Character Recognition)**: Multi-language text extraction using OpenCV for image preprocessing and Tesseract OCR for text recognition
- **💾 Multi-Format Export**: Save in TXT, PDF, DOCX formats
- **🔊 Text-to-Speech**: Convert text to speech (WAV format)

## 🏗️ Architecture

The project has a layered, modular architecture where each layer is designed as a separate class library:

### Projects

```
TextCaptureApp/
├── TextCaptureApp.Core          # Interfaces, DTOs, models
├── TextCaptureApp.ScreenCapture # Screen capture service
├── TextCaptureApp.Ocr           # OCR service (OpenCV + Tesseract)
├── TextCaptureApp.Export        # Export services (PDF, DOCX, TXT)
├── TextCaptureApp.Tts           # Text-to-Speech service
└── TextCaptureApp.UI            # WPF User Interface
```

### Dependencies

- **UI** → Core, Ocr, ScreenCapture, Export, Tts
- **Ocr, ScreenCapture, Export, Tts** → Core
- **Core** → No dependencies

### Technologies

- **.NET 8** (Windows)
- **WPF** (Windows Presentation Foundation)
- **Dependency Injection** (Microsoft.Extensions.Hosting)
- **OpenCV (OpenCvSharp)** - Image preprocessing (resize, denoising, thresholding, morphological operations)
- **Tesseract OCR** - Text recognition engine
- **iTextSharp** - PDF export
- **DocumentFormat.OpenXml** - DOCX export
- **NAudio** - Audio file processing

## 📋 Requirements

- .NET 8 SDK or higher
- Windows 10/11
- Tesseract language data files (tessdata folder)

## 🚀 Installation

### 1. Clone the Project

```bash
git clone <repository-url>
cd app89
```

### 2. Download Tesseract Language Data Files

You need Tesseract language data files for the OCR feature to work:

1. Create a `tessdata` folder in the project root directory
2. Download language data files from the [Tesseract Language Data](https://github.com/tesseract-ocr/tessdata) repository:
   - English: `eng.traineddata`
   - Turkish: `tur.traineddata`
   - German: `deu.traineddata`
   - French: `fra.traineddata`
   - Spanish: `spa.traineddata`

3. Place the downloaded `.traineddata` files in the `tessdata` folder

Folder structure:
```
app89/
├── tessdata/
│   ├── eng.traineddata
│   ├── tur.traineddata
│   └── ...
├── TextCaptureApp.UI/
└── ...
```

### 3. Build and Run

```bash
dotnet build
dotnet run --project TextCaptureApp.UI
```

## 💡 Usage

1. **Capture Screen**
   - "📷 Capture Screen" - Captures the entire screen
   - "🖼️ Capture Region" - Captures the selected region

2. **Extract Text**
   - Select a language (English, Turkish, etc.)
   - Click the "🔍 Extract Text (OCR)" button
   - Extracted text appears in the right panel

3. **Export**
   - "💾 Export TXT" - Save as plain text
   - "📄 Export PDF" - Create PDF document
   - "📝 Export DOCX" - Create Word document

4. **Convert to Speech**
   - "🔊 Generate Speech" - Create WAV audio file

## 🔧 Development Principles

This project strictly adheres to the following software development principles:

### SOLID Principles

- ✅ **Single Responsibility**: Each class has a single responsibility
- ✅ **Open/Closed**: Open for extension, closed for modification
- ✅ **Liskov Substitution**: Interfaces are properly implemented
- ✅ **Interface Segregation**: Small, specialized interfaces
- ✅ **Dependency Inversion**: Dependencies through interfaces

### Other Principles

- ✅ **Separation of Concerns**: Layers are independent from each other
- ✅ **Dependency Injection**: Constructor injection usage
- ✅ **No Static Code**: All services are instance-based
- ✅ **No God Objects**: No excessive responsibility in a single class
- ✅ **Testability**: Unit testable structure

## 🧪 Testing

For unit tests of services:

```bash
# Test projects can be added
dotnet test
```

## 📝 Notes

### About TTS (Text-to-Speech)

The current TTS implementation is a simple placeholder and produces a silent WAV file. 
For real TTS, one of the following options can be used:

- **Azure Cognitive Services Speech SDK**
- **Google Cloud Text-to-Speech**
- **Windows SAPI (System.Speech)** - Windows only

### Platform Support

The project is optimized for the Windows platform. For Linux/macOS support:
- ScreenCapture service requires platform-specific implementation
- System.Drawing.Common alternatives (SkiaSharp, ImageSharp) can be used

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is for educational purposes.

## 👨‍💻 Developer

A modular architecture example using C# and .NET 8, following SOLID principles.

---

**Note**: Don't forget to download Tesseract language data files, otherwise the OCR feature will not work!
