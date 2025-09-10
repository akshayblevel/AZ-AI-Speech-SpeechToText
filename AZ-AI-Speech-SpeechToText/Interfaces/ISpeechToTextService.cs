namespace AZ_AI_Speech_SpeechToText.Interfaces
{
    public interface ISpeechToTextService
    {
        Task<object> SpeechToTextAsync(IFormFile audioFile);
    }
}
