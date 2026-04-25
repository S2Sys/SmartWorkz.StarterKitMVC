# WikiGenerator

Generates wiki documentation from C# DLL XML comments.

## Usage

```bash
dotnet run -- --output docs --config tools/WikiGenerator/config.json --log-level info
```

## Options

- `--output, -o`: Output directory (default: docs)
- `--config, -c`: Config file path (default: tools/WikiGenerator/config.json)
- `--log-level`: Logging level (info, warning, error; default: info)

## Configuration

See `config.json` for project mappings and feature patterns.
