global using System.Text.Json;
global using Microsoft.Extensions.Logging;

#if !WINDOWS
global using System.Reactive;
#endif

global using SmartWorkz.Mobile;
global using SmartWorkz.Shared;
// Note: SmartWorkz.Shared.ILogger conflicts with Microsoft.Extensions.Logging.ILogger
// Files using Microsoft's ILogger must use the alias: using ILogger = Microsoft.Extensions.Logging.ILogger;

