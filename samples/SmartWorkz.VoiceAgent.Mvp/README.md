# SmartWorkz Voice Agent – MVP

A quick-test AI agent that serves **text chat and voice with one Claude model and one shared memory**.

```
Text box ───────────────────────────────┐
                                        ▼
Mic ─► speech-to-text ─► text ─► AgentService ─► Claude API (claude-opus-5-5)
       (browser, or local           │   ▲
        Whisper on the server)      ▼   │
Speaker ◄─ browser text-to-speech ◄─┘  MemoryStore (App_Data/memory/<user>.json)
```

- **Same brain:** `/api/chat` is used by both the text box and the mic. Voice mode only asks for short, speakable replies.
- **Memory:**
  - *Short-term* – the last 20 messages for the user are sent with each request.
  - *Long-term* – after each message Claude extracts lasting facts (name, language, preferences…) that are put in the system prompt on every later request.
  - Memory is per **User** value on the page, and is shared across chat and voice.
- **Local voice:**
  - *Speech-to-text:* choose **Local Whisper** to transcribe on your server, offline (Whisper.net, `ggml-base` model, downloaded automatically on first use, ~140 MB). **Browser** mode uses Chrome/Edge speech recognition instead.
  - *Text-to-speech:* the browser's built-in voices, spoken sentence by sentence while the reply streams.

## Run it

Requires the .NET 8 SDK and an Anthropic API key.

```bash
cd samples/SmartWorkz.VoiceAgent.Mvp
export ANTHROPIC_API_KEY=sk-ant-...        # PowerShell: $env:ANTHROPIC_API_KEY="sk-ant-..."
dotnet run
```

Open http://localhost:5080 in Chrome or Edge, allow the microphone, and try:

1. Type: *"Hi, I'm Senthil and I prefer short answers."*
2. Press **🎤 Talk** and say: *"What's my name?"* – it answers from memory, out loud.
3. Open **Memory for this user** to see the saved facts. **Clear memory** resets it.

## Configuration (`appsettings.json`)

| Setting | Default | Purpose |
|---|---|---|
| `Anthropic:Model` | `claude-opus-5-5` | Claude model for chat, voice and fact extraction |
| `Anthropic:ApiKey` | *(env `ANTHROPIC_API_KEY`)* | Optional override |
| `Whisper:ModelPath` | `models/ggml-base.bin` | Local Whisper model file. Use `ggml-small.bin` for better accuracy |
| `Whisper:AutoDownload` | `true` | Download the model on first transcription if missing |

To install the model manually instead, download `ggml-base.bin` from
`https://huggingface.co/ggerganov/whisper.cpp` into `models/`.

## API

| Endpoint | Purpose |
|---|---|
| `POST /api/chat` `{ userId, text, voice }` | Streams the reply as plain text |
| `POST /api/transcribe` (form: `audio` = 16 kHz mono WAV, `language`) | Local Whisper speech-to-text |
| `GET /api/memory/{userId}` / `DELETE /api/memory/{userId}` | View / clear memory |
| `GET /api/status` | Whether local Whisper is available |

## MVP limits / next steps

- No authentication – the user ID comes from the page. Use the logged-in user when moving into the main app.
- JSON-file memory – replace `MemoryStore` with EF Core / SQL Server for production.
- Server-side text-to-speech (e.g. Piper) can replace browser voices if every client must sound the same.
