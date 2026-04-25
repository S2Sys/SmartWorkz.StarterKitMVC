@echo off
cd /d "%~dp0\.."
echo Generating wiki documentation...
dotnet run --project tools/WikiGenerator -- ^
  --output docs/framework ^
  --config tools/WikiGenerator/config.json ^
  --log-level info
echo Wiki generation complete! Check docs/framework/
