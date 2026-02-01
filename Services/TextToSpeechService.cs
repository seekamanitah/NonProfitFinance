using Microsoft.JSInterop;

namespace NonProfitFinance.Services;

/// <summary>
/// Text-to-Speech service using browser's Speech Synthesis API
/// This is a Blazor-friendly approach that works cross-platform without native dependencies
/// </summary>
public class TextToSpeechService : ITextToSpeechService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<TextToSpeechService> _logger;
    private bool _isEnabled = true;
    private float _speechRate = 1.0f;

    public bool IsEnabled
    {
        get => _isEnabled;
        set => _isEnabled = value;
    }

    public float SpeechRate
    {
        get => _speechRate;
        set => _speechRate = Math.Clamp(value, 0.5f, 2.0f);
    }

    public TextToSpeechService(IJSRuntime jsRuntime, ILogger<TextToSpeechService> logger)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    public async Task SpeakAsync(string text)
    {
        if (!IsEnabled || string.IsNullOrWhiteSpace(text))
            return;

        try
        {
            await _jsRuntime.InvokeVoidAsync("textToSpeech.speak", text, _speechRate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during text-to-speech");
        }
    }

    public async Task AnnounceTransactionAsync(string description, decimal amount)
    {
        var message = $"Transaction saved. {description} for ${Math.Abs(amount):N2}";
        await SpeakAsync(message);
    }

    public async Task AnnounceReportAsync(string reportName)
    {
        var message = $"Report generated: {reportName}";
        await SpeakAsync(message);
    }


    public async Task AnnounceErrorAsync(string errorMessage)
    {
        var message = $"Error: {errorMessage}";
        await SpeakAsync(message);
    }

    public async Task AnnounceSuccessAsync(string message)
    {
        await SpeakAsync(message);
    }

    public async Task Stop()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("textToSpeech.stop");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping text-to-speech");
        }
    }
}

