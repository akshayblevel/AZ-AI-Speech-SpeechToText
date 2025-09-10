using AZ_AI_Speech_SpeechToText.Interfaces;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using NAudio.Wave;

namespace AZ_AI_Speech_SpeechToText.Services
{
    public class SpeechToTextService : ISpeechToTextService
    {
        private readonly string _subscriptionKey;
        public SpeechToTextService(IConfiguration config)
        {
            _subscriptionKey = config["AzureSpeech:Key"] ?? "";
        }

        public async Task<object> SpeechToTextAsync(IFormFile audioFile)
        {
            await using var stream = audioFile.OpenReadStream();

            using var convertedStream = ConvertToPcm16kMono(stream);

            var speechConfig = SpeechConfig.FromSubscription(_subscriptionKey, "eastus2");
            speechConfig.SetProperty("SPEECH-InitialSilenceTimeoutMs", "10000"); // 10 sec
            speechConfig.SetProperty("SPEECH-EndSilenceTimeoutMs", "3000");

            string resultText = "";

            using var audioInput = AudioConfig.FromStreamInput(new BinaryAudioStreamReader(convertedStream));
            using var recognizer = new SpeechRecognizer(speechConfig, audioInput);

            var result = await recognizer.RecognizeOnceAsync();

            if (result.Reason == ResultReason.RecognizedSpeech)
            {
                resultText = result.Text;
            }
            else if (result.Reason == ResultReason.NoMatch)
            {
                resultText = "Speech could not be recognized.";
            }
            else if (result.Reason == ResultReason.Canceled)
            {
                var cancellation = CancellationDetails.FromResult(result);
                resultText = $"Canceled: {cancellation.Reason}. Error: {cancellation.ErrorDetails}";
            }

            return new { Transcript = resultText };
        }

        public static MemoryStream ConvertToPcm16kMono(Stream inputStream)
        {
            using var reader = new WaveFileReader(inputStream);

            // Desired Azure format: PCM 16kHz mono
            var targetFormat = new WaveFormat(16000, 16, 1);

            using var conversionStream = new MediaFoundationResampler(reader, targetFormat)
            {
                ResamplerQuality = 60
            };

            var outputStream = new MemoryStream();
            WaveFileWriter.WriteWavFileToStream(outputStream, conversionStream);
            outputStream.Position = 0; 
            return outputStream;
        }

        public class BinaryAudioStreamReader : PullAudioInputStreamCallback
        {
            private readonly Stream _stream;
            public BinaryAudioStreamReader(Stream stream) => _stream = stream;

            public override int Read(byte[] buffer, uint size)
            {
                return _stream.Read(buffer, 0, (int)size);
            }

            protected override void Dispose(bool disposing)
            {
                _stream.Dispose();
                base.Dispose(disposing);
            }
        }
    }
}
