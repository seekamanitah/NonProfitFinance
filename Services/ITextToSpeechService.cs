namespace NonProfitFinance.Services;

/// <summary>
/// Service for Text-to-Speech functionality
/// </summary>
public interface ITextToSpeechService
{
    /// <summary>
    /// Whether TTS is enabled
    /// </summary>
    bool IsEnabled { get; set; }
    
    /// <summary>
    /// Speech rate (0.5 to 2.0, default 1.0)
    /// </summary>
    float SpeechRate { get; set; }

    /// <summary>
    /// Speak text using text-to-speech
    /// </summary>
    Task SpeakAsync(string text);

    /// <summary>
    /// Stop currently speaking text
    /// </summary>
    Task Stop();

    /// <summary>
    /// Announce transaction saved
    /// </summary>
    Task AnnounceTransactionAsync(string description, decimal amount);

    /// <summary>
    /// Announce report generated
    /// </summary>
    Task AnnounceReportAsync(string reportName);

    /// <summary>
    /// Announce validation error
    /// </summary>
    Task AnnounceErrorAsync(string errorMessage);

    /// <summary>
    /// Announce success message
    /// </summary>
    Task AnnounceSuccessAsync(string message);
}
