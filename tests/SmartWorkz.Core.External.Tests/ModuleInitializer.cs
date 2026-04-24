using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using QuestPDF.Infrastructure;

namespace SmartWorkz.Core.External.Tests;

/// <summary>
/// Module initializer to configure QuestPDF license before tests run.
/// Note: QuestPDF requires x86 or x64 architecture and does not support ARM64.
/// License configuration will only be applied on supported architectures.
/// </summary>
internal static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        // Only configure license on supported architectures
        // QuestPDF does not support ARM64 (win-arm64)
        var isSupported = RuntimeInformation.ProcessArchitecture is Architecture.X86 or Architecture.X64;

        if (isSupported)
        {
            // Configure QuestPDF Community License for testing
            QuestPDF.Settings.License = LicenseType.Community;
        }
    }
}
