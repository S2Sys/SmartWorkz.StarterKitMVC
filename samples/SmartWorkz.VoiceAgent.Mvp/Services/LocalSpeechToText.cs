using System.Text;
using Whisper.net;
using Whisper.net.Ggml;

namespace SmartWorkz.VoiceAgent.Mvp.Services;

/// <summary>
/// Local, offline speech-to-text with Whisper (whisper.cpp via Whisper.net).
/// Expects 16 kHz mono 16-bit WAV, which the browser page records and sends.
/// </summary>
public class LocalSpeechToText(IConfiguration config, IWebHostEnvironment env, ILogger<LocalSpeechToText> log)
{
    private WhisperFactory? _factory;
    private readonly SemaphoreSlim _init = new(1, 1);

    private string ModelPath => Path.Combine(env.ContentRootPath, config["Whisper:ModelPath"] ?? "models/ggml-base.bin");

    public bool IsAvailable => File.Exists(ModelPath) || config.GetValue("Whisper:AutoDownload", true);

    public async Task<string> TranscribeAsync(Stream wav, string language, CancellationToken ct)
    {
        var factory = await GetFactoryAsync(ct);
        using var processor = factory.CreateBuilder().WithLanguage(language).Build();

        var sb = new StringBuilder();
        await foreach (var segment in processor.ProcessAsync(wav, ct))
            sb.Append(segment.Text);
        return sb.ToString().Trim();
    }

    private async Task<WhisperFactory> GetFactoryAsync(CancellationToken ct)
    {
        if (_factory is not null) return _factory;
        await _init.WaitAsync(ct);
        try
        {
            if (_factory is not null) return _factory;
            if (!File.Exists(ModelPath))
            {
                // First run: download the Whisper "base" model (~140 MB) once.
                log.LogInformation("Downloading Whisper model to {Path} ...", ModelPath);
                Directory.CreateDirectory(Path.GetDirectoryName(ModelPath)!);
                var tmp = ModelPath + ".download";
                await using (var model = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(GgmlType.Base, cancellationToken: ct))
                await using (var file = File.Create(tmp))
                    await model.CopyToAsync(file, ct);
                File.Move(tmp, ModelPath, overwrite: true); // only a complete download becomes the model
            }
            return _factory = WhisperFactory.FromPath(ModelPath);
        }
        finally { _init.Release(); }
    }
}
