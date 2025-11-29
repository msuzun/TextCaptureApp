# Tesseract Language Data

Bu klasöre Tesseract OCR dil dosyalarını (.traineddata) yerleştirin.

## İndirme Bağlantıları

Dil dosyalarını aşağıdaki GitHub deposundan indirebilirsiniz:

**Tesseract Official Language Data:**
https://github.com/tesseract-ocr/tessdata

### Önerilen Diller

- **eng.traineddata** - İngilizce
- **tur.traineddata** - Türkçe
- **deu.traineddata** - Almanca
- **fra.traineddata** - Fransızca
- **spa.traineddata** - İspanyolca

## Kurulum

1. Yukarıdaki linkten ihtiyacınız olan dil dosyalarını indirin
2. İndirdiğiniz `.traineddata` dosyalarını bu klasöre (`tessdata/`) kopyalayın
3. Uygulamayı çalıştırın

## Örnek Klasör Yapısı

```
tessdata/
├── README.md (bu dosya)
├── eng.traineddata
├── tur.traineddata
└── deu.traineddata
```

## Notlar

- Dil dosyaları büyük olduğu için (her biri ~10-50 MB) git repository'sine dahil edilmemiştir
- OCR özelliğinin çalışması için en az bir dil dosyası gereklidir
- Varsayılan dil İngilizcedir (`eng`)

