using System.Runtime.CompilerServices;
using System.Text;
using Anthropic;
using Anthropic.Models.Messages;

namespace SmartWorkz.VoiceAgent.Mvp.Services;

/// <summary>
/// One brain for both text chat and voice: same Claude model, same per-user memory.
/// Voice only changes the reply style (short, no markdown) because it is spoken aloud.
/// </summary>
public class AgentService(AnthropicClient claude, MemoryStore memory, IConfiguration config, ILogger<AgentService> log)
{
    private string Model => config["Anthropic:Model"] ?? "claude-opus-5-5";
    private const int HistoryTurns = 20;

    public async IAsyncEnumerable<string> AskAsync(
        string userId, string userText, bool isVoice,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var mem = await memory.GetAsync(userId);

        var system = new StringBuilder()
            .AppendLine("You are the SmartWorkz assistant. The same assistant serves text chat and voice, with shared memory.")
            .AppendLine(isVoice
                ? "This reply will be spoken aloud: answer in 1-3 short, natural sentences. No markdown, lists, code or emojis."
                : "This reply is shown as text in a chat window. Keep it concise.")
            .AppendLine("What you remember about this user from earlier conversations:")
            .AppendLine(mem.Facts.Count == 0 ? "(nothing yet)" : string.Join("\n", mem.Facts.Select(f => "- " + f)))
            .ToString();

        // Short-term memory: last N turns, starting on a user turn.
        var recent = mem.Turns.TakeLast(HistoryTurns).SkipWhile(t => t.Role != "user");
        var messages = recent.Select(t => new MessageParam
        {
            Role = t.Role == "user" ? Role.User : Role.Assistant,
            Content = t.Text,
        }).ToList();
        messages.Add(new() { Role = Role.User, Content = userText });

        var reply = new StringBuilder();
        await foreach (var ev in claude.Messages.CreateStreaming(new MessageCreateParams
        {
            Model = Model,
            MaxTokens = isVoice ? 1024 : 4096,
            System = system,
            OutputConfig = new OutputConfig { Effort = Effort.Low }, // fast, conversational replies
            Messages = messages,
        }, ct))
        {
            if (ev.TryPickContentBlockDelta(out var delta) && delta.Delta.TryPickText(out var text))
            {
                reply.Append(text.Text);
                yield return text.Text;
            }
        }

        await memory.AddTurnsAsync(userId,
            new ChatTurn("user", userText, DateTime.UtcNow),
            new ChatTurn("assistant", reply.ToString(), DateTime.UtcNow));

        // Long-term memory: extract lasting facts in the background so the reply isn't delayed.
        _ = Task.Run(() => ExtractFactsAsync(userId, userText));
    }

    private async Task ExtractFactsAsync(string userId, string userText)
    {
        try
        {
            var res = await claude.Messages.Create(new MessageCreateParams
            {
                Model = Model,
                MaxTokens = 512,
                OutputConfig = new OutputConfig { Effort = Effort.Low },
                Messages =
                [
                    new()
                    {
                        Role = Role.User,
                        Content = "From the user's message below, extract lasting facts worth remembering about the user " +
                                  "(name, preferences, language, company, goals, IDs). One short fact per line. " +
                                  "If there is nothing worth remembering, reply exactly NONE.\n\nUser message:\n" + userText,
                    },
                ],
            });

            var text = string.Concat(res.Content.Select(b => b.Value).OfType<TextBlock>().Select(b => b.Text)).Trim();
            if (text.Length > 0 && !text.Equals("NONE", StringComparison.OrdinalIgnoreCase))
                await memory.AddFactsAsync(userId, text.Split('\n', StringSplitOptions.RemoveEmptyEntries));
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "Fact extraction failed for {UserId}", userId);
        }
    }
}
