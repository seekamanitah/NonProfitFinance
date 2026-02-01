# OCR and Text-to-Speech Implementation Guide

## Overview
This application now includes OCR (Optical Character Recognition) and Text-to-Speech capabilities to enhance accessibility and automate data entry.

## Features

### 1. OCR (Optical Character Recognition)
- Extract text from images (receipts, invoices, documents)
- Automatic receipt parsing with data extraction
- Support for multiple image formats (JPG, PNG, BMP, GIF)
- Pre-processing for better accuracy (grayscale, contrast enhancement)
- Create transactions directly from scanned receipts

### 2. Text-to-Speech (TTS)
- Browser-based speech synthesis (no server dependencies)
- Announces transaction saves
- Reads validation errors
- Announces report generation
- Adjustable speech rate
- Works on all major browsers

## Setup Instructions

### OCR Setup (Tesseract)

#### Step 1: Install NuGet Packages
The following packages have been added to the project:
- `Tesseract` (v5.2.0)
- `SixLabors.ImageSharp` (v3.1.6)

#### Step 2: Download Language Files
1. Create a `tessdata` folder in your application root directory (same level as Program.cs)
2. Download the English language file:
   - Visit: https://github.com/tesseract-ocr/tessdata
   - Download `eng.traineddata`
   - Place it in the `tessdata` folder

Optional: For better number recognition (useful for financial data):
- Also download `equ.traineddata` (equation/math symbols)

#### Step 3: Verify Setup
- Navigate to Settings > Accessibility Settings
- Check if OCR status shows "Initialized"
- If not, restart the application after adding language files

### Text-to-Speech Setup

No additional setup required! TTS uses the browser's built-in Speech Synthesis API.

#### Browser Compatibility:
- ? Chrome/Edge (Best support)
- ? Firefox
- ? Safari
- ?? Older browsers may not support all features

## Usage Guide

### Using OCR

#### From Documents Page:
1. Upload an image document (receipt, invoice, etc.)
2. Click the "Extract Text" button on the image document
3. Wait for OCR processing
4. Review extracted text and data

#### For Receipts:
1. Upload a receipt image
2. Click "Extract Text"
3. If receipt data is detected, you'll see:
   - Merchant name
   - Transaction date
   - Total amount
   - Line items (if detected)
4. Click "Create Transaction" to automatically populate a new transaction

### Using Text-to-Speech

#### Enable/Configure TTS:
1. Go to Settings > Accessibility Settings
2. Toggle "Enable Text-to-Speech"
3. Adjust speech rate (0.5x to 2.0x)
4. Test with sample text

#### Automatic Announcements:
- When saving a transaction: "Transaction saved. [Description] for $[Amount]"
- On validation errors: "Error: [Error message]"
- On report generation: "Report generated: [Report name]"

#### Manual TTS:
- In OCR results, click "Read Aloud" to hear extracted text
- Use keyboard shortcut: Ctrl+Shift+S

## Implementation Details

### Services Created

#### IOcrService / OcrService
- `ExtractTextFromImageAsync(Stream)` - Extract text from image stream
- `ExtractTextFromImageAsync(string)` - Extract text from file path
- `ProcessReceiptAsync(Stream)` - Process and parse receipt
- `IsInitialized` - Check if OCR engine is ready

#### ITextToSpeechService / TextToSpeechService
- `SpeakAsync(string)` - Speak text
- `AnnounceTransactionAsync(string, decimal)` - Announce transaction
- `AnnounceReportAsync(string)` - Announce report
- `AnnounceErrorAsync(string)` - Announce error
- `AnnounceSuccessAsync(string)` - Announce success
- `Stop()` - Stop speaking
- `IsEnabled` - Enable/disable TTS
- `SpeechRate` - Adjust speech speed

### Components Created

#### OcrModal.razor
Reusable modal component for OCR processing:
- Shows OCR progress
- Displays extracted text
- Shows parsed receipt data
- Allows creating transactions from receipts
- Includes TTS integration

#### AccessibilitySettingsPage.razor
Settings page for configuring OCR and TTS:
- TTS enable/disable toggle
- Speech rate slider
- TTS test interface
- OCR status display
- Setup instructions

### Models Created

#### OcrResult
- `ExtractedText` - Full text from image
- `Confidence` - OCR confidence score (0-100)
- `Success` - Processing status
- `ErrorMessage` - Error details (if any)
- `ReceiptData` - Parsed receipt data

#### ReceiptData
- `Merchant` - Vendor/merchant name
- `Date` - Transaction date
- `Total` - Total amount
- `Items` - Line items
- `ReceiptConfidence` - Receipt detection confidence

### API Extensions

#### IDocumentService
- Added `ProcessDocumentOcrAsync(int documentId)` method
- Returns `OcrResult` with extracted text and receipt data

## Performance Considerations

### OCR Performance
- Processing time: 1-5 seconds per image (depends on image size/quality)
- Runs synchronously (blocks during processing)
- Consider adding progress indicators for large images
- Image preprocessing improves accuracy but adds processing time

### Memory Usage
- OCR engine is singleton (loaded once)
- Each image processing creates temporary memory
- Language files: ~30-50 MB per language
- Consider implementing image size limits (currently 2000px max dimension)

## Troubleshooting

### OCR Not Working

**Problem:** "OCR engine not initialized" error

**Solutions:**
1. Verify `tessdata` folder exists in application root
2. Check `eng.traineddata` file is present
3. Restart the application
4. Check file permissions on tessdata folder
5. Review application logs for initialization errors

**Problem:** Low accuracy/confidence scores

**Solutions:**
1. Use higher resolution images (300 DPI recommended)
2. Ensure good lighting in original photo
3. Avoid skewed/rotated images
4. Use clear, printed text (handwriting not supported well)

### TTS Not Working

**Problem:** No speech output

**Solutions:**
1. Check browser compatibility (use Chrome/Edge for best results)
2. Verify TTS is enabled in Settings > Accessibility Settings
3. Check system volume settings
4. Try with HTTPS (required in some browsers)
5. Check browser console for JavaScript errors

**Problem:** Speech is too fast/slow

**Solution:**
- Adjust speech rate in Settings > Accessibility Settings (0.5x to 2.0x)

## Future Enhancements

### Potential OCR Improvements:
- [ ] Support for PDF OCR
- [ ] Batch processing multiple images
- [ ] Custom training for organization-specific terms
- [ ] Invoice parsing (not just receipts)
- [ ] Multi-language support
- [ ] Handwriting recognition

### Potential TTS Improvements:
- [ ] Voice selection (male/female)
- [ ] Custom phrases/shortcuts
- [ ] Read entire forms/reports
- [ ] Notification announcements
- [ ] SSML support for better pronunciation

## Security Considerations

### OCR
- Images are processed server-side (sensitive data never sent to third parties)
- All processing is local (offline capable)
- Original images remain in secure storage
- Consider implementing image encryption at rest

### TTS
- Uses browser API (no data sent to servers)
- Text is processed locally in browser
- No audio is recorded or stored
- Fully offline capable

## Cost & Licensing

### Tesseract OCR
- **License:** Apache 2.0 (Free, open-source)
- **Cost:** $0
- **Commercial Use:** ? Yes

### SixLabors.ImageSharp
- **License:** Apache 2.0 for non-commercial
- **Commercial License:** Required for commercial use (see pricing)
- **Note:** Verify licensing for your use case

### Browser Speech Synthesis
- **Cost:** $0 (built into browsers)
- **No API keys required**
- **No rate limits**

## Testing Recommendations

### OCR Testing:
1. Test with various receipt formats
2. Test with different image qualities
3. Test with different lighting conditions
4. Verify confidence scores
5. Test receipt parsing accuracy

### TTS Testing:
1. Test in different browsers
2. Verify announcements at key points
3. Test with various speech rates
4. Check for interruptions/conflicts
5. Test keyboard shortcuts

## Support Resources

### Tesseract OCR:
- Documentation: https://tesseract-ocr.github.io/
- Language files: https://github.com/tesseract-ocr/tessdata
- Issue tracker: https://github.com/tesseract-ocr/tesseract/issues

### Web Speech API:
- MDN Docs: https://developer.mozilla.org/en-US/docs/Web/API/SpeechSynthesis
- Browser support: https://caniuse.com/speech-synthesis

## Contact & Feedback

For issues or feature requests related to OCR and TTS functionality, please open an issue on the project repository.
