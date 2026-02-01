# OCR & TTS Quick Start Guide

## ?? Quick Setup (5 Minutes)

### Step 1: Verify Packages (Already Done ?)
The following packages have been added to your project:
- Tesseract (v5.2.0)
- SixLabors.ImageSharp (v3.1.6)

### Step 2: Setup Tesseract OCR

1. **Create tessdata folder** in your project root:
   ```
   YourProject/
   ??? tessdata/          <- Create this folder
   ??? Program.cs
   ??? appsettings.json
   ??? ...
   ```

2. **Download language file:**
   - Go to: https://github.com/tesseract-ocr/tessdata/blob/main/eng.traineddata
   - Click "Download" button
   - Save as `eng.traineddata`

3. **Place file in tessdata:**
   ```
   tessdata/
   ??? eng.traineddata    <- Put it here
   ```

4. **Restart your application**

### Step 3: Test OCR

1. Run the application
2. Navigate to **Documents** page
3. Upload an image file (receipt, invoice, etc.)
4. Click the **?? Extract Text** button
5. See the magic! ?

### Step 4: Test TTS

1. Navigate to **Settings > Accessibility**
2. Toggle **"Enable Text-to-Speech"**
3. Type some text in the test box
4. Click **"Speak"**
5. Hear your computer talk! ??

---

## ?? Code Examples

### Example 1: Extract Text from Image

```csharp
@inject IOcrService OcrService

private async Task ProcessReceipt()
{
    using var stream = File.OpenRead("receipt.jpg");
    
    var result = await OcrService.ProcessReceiptAsync(stream);
    
    if (result.Success)
    {
        Console.WriteLine($"Extracted: {result.ExtractedText}");
        
        if (result.ReceiptData != null)
        {
            var merchant = result.ReceiptData.Merchant;
            var total = result.ReceiptData.Total;
            var date = result.ReceiptData.Date;
            
            Console.WriteLine($"Merchant: {merchant}");
            Console.WriteLine($"Total: ${total:N2}");
            Console.WriteLine($"Date: {date:d}");
        }
    }
}
```

### Example 2: Announce Messages with TTS

```csharp
@inject ITextToSpeechService TtsService

private async Task SaveTransaction()
{
    // ... save logic ...
    
    // Announce success
    await TtsService.AnnounceTransactionAsync("Office supplies", 45.67m);
    // Speaks: "Transaction saved. Office supplies for $45.67"
}

private async Task ValidateForm()
{
    if (string.IsNullOrEmpty(description))
    {
        await TtsService.AnnounceErrorAsync("Description is required");
        return;
    }
}
```

### Example 3: Custom OCR Options

```csharp
var options = new OcrOptions
{
    Language = "eng",
    PreprocessImage = true,    // Enhance image quality
    ParseReceipt = true,       // Extract receipt data
    Dpi = 300                  // Image resolution
};

var result = await OcrService.ExtractTextFromImageAsync(stream, options);
```

---

## ?? Common Use Cases

### Use Case 1: Receipt Upload with Auto-Fill

```csharp
private async Task HandleReceiptUpload(IBrowserFile file)
{
    using var stream = file.OpenReadStream();
    var ocrResult = await OcrService.ProcessReceiptAsync(stream);
    
    if (ocrResult.ReceiptData != null)
    {
        var receipt = ocrResult.ReceiptData;
        
        // Auto-fill transaction form
        transactionDescription = receipt.Merchant ?? "";
        transactionAmount = receipt.Total ?? 0;
        transactionDate = receipt.Date ?? DateTime.Today;
        
        // Announce
        await TtsService.AnnounceSuccessAsync(
            $"Receipt processed. {receipt.Items.Count} items found."
        );
    }
}
```

### Use Case 2: Document Scanner Page

```razor
<InputFile OnChange="@HandleFileDrop" accept="image/*" />

@if (isProcessing)
{
    <p>?? Processing with OCR...</p>
}

@if (extractedText != null)
{
    <div class="result">
        <h3>Extracted Text:</h3>
        <pre>@extractedText</pre>
        <button @onclick="ReadAloud">?? Read Aloud</button>
    </div>
}

@code {
    private bool isProcessing;
    private string? extractedText;
    
    private async Task HandleFileDrop(InputFileChangeEventArgs e)
    {
        isProcessing = true;
        
        using var stream = e.File.OpenReadStream(10 * 1024 * 1024);
        var result = await OcrService.ExtractTextFromImageAsync(stream);
        
        extractedText = result.ExtractedText;
        isProcessing = false;
        
        await TtsService.AnnounceSuccessAsync("Text extraction complete");
    }
    
    private async Task ReadAloud()
    {
        if (extractedText != null)
            await TtsService.SpeakAsync(extractedText);
    }
}
```

### Use Case 3: Accessibility Announcements

```csharp
@inject ITextToSpeechService TtsService

protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        await TtsService.AnnounceSuccessAsync("Dashboard loaded");
    }
}

private async Task DeleteItem(int id)
{
    if (!await ConfirmDelete())
        return;
        
    await ItemService.DeleteAsync(id);
    await TtsService.AnnounceSuccessAsync("Item deleted");
    await LoadItems();
}
```

---

## ?? Configuration

### Adjust TTS Settings

```csharp
@inject ITextToSpeechService TtsService

protected override void OnInitialized()
{
    // Enable/disable
    TtsService.IsEnabled = true;
    
    // Speed (0.5x to 2.0x)
    TtsService.SpeechRate = 1.0f;  // Normal
    TtsService.SpeechRate = 0.75f; // Slower
    TtsService.SpeechRate = 1.5f;  // Faster
}
```

### Check OCR Status

```csharp
@inject IOcrService OcrService

protected override void OnInitialized()
{
    if (!OcrService.IsInitialized)
    {
        ShowAlert("OCR not available. Please setup tessdata files.");
    }
}
```

---

## ?? Testing Checklist

### OCR Testing
- [ ] Upload JPG image
- [ ] Upload PNG image
- [ ] Upload receipt image
- [ ] Verify text extraction
- [ ] Check receipt parsing
- [ ] Test confidence scores
- [ ] Try poor quality image
- [ ] Test large image (>2000px)

### TTS Testing
- [ ] Enable/disable toggle
- [ ] Adjust speech rate
- [ ] Test with short text
- [ ] Test with long text
- [ ] Test special characters
- [ ] Test numbers and currency
- [ ] Stop speaking mid-sentence
- [ ] Test in different browsers

---

## ?? Common Issues

### Issue: "OCR engine not initialized"
**Fix:**
```bash
# Verify folder structure
YourProject/
??? tessdata/
?   ??? eng.traineddata   <- Must exist
```

### Issue: "No speech output"
**Fix:**
1. Check browser console for errors
2. Verify system volume is on
3. Try HTTPS instead of HTTP
4. Test in Chrome/Edge first

### Issue: "Low OCR confidence"
**Fix:**
1. Use higher resolution images
2. Ensure good lighting
3. Avoid skewed images
4. Use clear printed text

---

## ?? API Reference

### IOcrService Methods

| Method | Description | Returns |
|--------|-------------|---------|
| `ExtractTextFromImageAsync(Stream)` | Extract text from image stream | `Task<OcrResult>` |
| `ExtractTextFromImageAsync(string)` | Extract text from file path | `Task<OcrResult>` |
| `ProcessReceiptAsync(Stream)` | Process and parse receipt | `Task<OcrResult>` |
| `IsInitialized` | Check if engine is ready | `bool` |

### ITextToSpeechService Methods

| Method | Description | Returns |
|--------|-------------|---------|
| `SpeakAsync(string)` | Speak any text | `Task` |
| `AnnounceTransactionAsync(string, decimal)` | Announce transaction | `Task` |
| `AnnounceReportAsync(string)` | Announce report | `Task` |
| `AnnounceErrorAsync(string)` | Announce error | `Task` |
| `AnnounceSuccessAsync(string)` | Announce success | `Task` |
| `Stop()` | Stop speaking | `void` |

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsEnabled` | `bool` | Enable/disable TTS |
| `SpeechRate` | `float` | Speed (0.5 - 2.0) |
| `IsInitialized` | `bool` | OCR engine status |

---

## ?? UI Components

### Using OcrModal

```razor
<OcrModal DocumentId="@selectedDocId" 
          IsVisible="@showModal" 
          OnClose="@(() => showModal = false)" 
          OnReceiptDetected="@HandleReceipt" />

@code {
    private bool showModal;
    private int selectedDocId;
    
    private void OpenOcr(int documentId)
    {
        selectedDocId = documentId;
        showModal = true;
    }
    
    private void HandleReceipt(ReceiptData receipt)
    {
        // Do something with receipt data
        CreateTransaction(receipt);
    }
}
```

---

## ?? Next Steps

1. ? Setup tessdata folder
2. ? Test OCR on a receipt
3. ? Test TTS announcements
4. ?? Integrate into your workflows
5. ?? Customize for your needs

---

## ?? Full Documentation

See `OCR_TTS_IMPLEMENTATION_GUIDE.md` for:
- Detailed setup instructions
- Advanced configuration
- Troubleshooting guide
- Security considerations
- Performance tips

---

## ?? Pro Tips

1. **OCR**: Always preprocess images for better accuracy
2. **TTS**: Keep announcements short and clear
3. **OCR**: Test with various receipt formats
4. **TTS**: Use appropriate speech rate for your users
5. **Both**: Check initialization status before using

---

**Happy Coding! ??**

Need help? Check the full documentation or open an issue!
