using Anthropic;
using SmartWorkz.VoiceAgent.Mvp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(sp =>
{
    // Reads ANTHROPIC_API_KEY from the environment; appsettings "Anthropic:ApiKey" is an optional override.
    var key = sp.GetRequiredService<IConfiguration>()["Anthropic:ApiKey"];
    return string.IsNullOrWhiteSpace(key) ? new AnthropicClient() : new AnthropicClient { ApiKey = key };
});
builder.Services.AddSingleton<MemoryStore>();
builder.Services.AddSingleton<AgentService>();
builder.Services.AddSingleton<LocalSpeechToText>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/status", (LocalSpeechToText stt) => new { localWhisper = stt.IsAvailable });

// Text and voice both call the same endpoint; "voice" only switches to short, speakable replies.
app.MapPost("/api/chat", async (ChatRequest req, AgentService agent, HttpContext http) =>
{
    if (string.IsNullOrWhiteSpace(req.UserId) || string.IsNullOrWhiteSpace(req.Text))
    {
        http.Response.StatusCode = StatusCodes.Status400BadRequest;
        return;
    }

    http.Response.ContentType = "text/plain; charset=utf-8";
    try
    {
        await foreach (var chunk in agent.AskAsync(req.UserId, req.Text, req.Voice, http.RequestAborted))
        {
            await http.Response.WriteAsync(chunk, http.RequestAborted);
            await http.Response.Body.FlushAsync(http.RequestAborted);
        }
    }
    catch (Exception ex) when (ex is not OperationCanceledException)
    {
        app.Logger.LogError(ex, "Chat request failed");
        await http.Response.WriteAsync("\n[Error: " + ex.Message + "]");
    }
});

// Local, offline speech-to-text (Whisper). The browser posts a 16 kHz mono WAV.
app.MapPost("/api/transcribe", async (HttpRequest request, LocalSpeechToText stt, CancellationToken ct) =>
{
    var form = await request.ReadFormAsync(ct);
    var audio = form.Files["audio"];
    if (audio is null || audio.Length == 0) return Results.BadRequest("Missing 'audio' file.");

    var language = form["language"].FirstOrDefault() ?? "auto";
    try
    {
        await using var stream = audio.OpenReadStream();
        var text = await stt.TranscribeAsync(stream, language, ct);
        return Results.Ok(new { text });
    }
    catch (Exception ex) when (ex is not OperationCanceledException)
    {
        app.Logger.LogError(ex, "Local transcription failed");
        return Results.Problem("Local Whisper transcription failed: " + ex.Message);
    }
}).DisableAntiforgery();

app.MapGet("/api/memory/{userId}", async (string userId, MemoryStore memory) => await memory.GetAsync(userId));
app.MapDelete("/api/memory/{userId}", async (string userId, MemoryStore memory) =>
{
    await memory.ClearAsync(userId);
    return Results.NoContent();
});

app.Run();

public record ChatRequest(string UserId, string Text, bool Voice);
