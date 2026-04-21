namespace SmartWorkz.Shared;

using System.IO.Compression;
using System.Text;

/// <summary>
/// Provides utilities for GZip compression and decompression.
/// </summary>
public static class CompressHelper
{
    /// <summary>
    /// Compresses a string using GZip compression.
    /// </summary>
    public static Result<byte[]> CompressString(string text)
    {
        try
        {
            if (string.IsNullOrEmpty(text))
                return Result.Fail<byte[]>("Error.EmptyText", "Text cannot be null or empty");

            return Result.Ok(CompressBytes(Encoding.UTF8.GetBytes(text)));
        }
        catch (Exception ex)
        {
            return Result.Fail<byte[]>("Error.CompressString", ex.Message);
        }
    }

    /// <summary>
    /// Decompresses a GZip-compressed byte array back to a string.
    /// </summary>
    public static Result<string> DecompressString(byte[] compressed)
    {
        try
        {
            if (compressed == null || compressed.Length == 0)
                return Result.Fail<string>("Error.EmptyData", "Compressed data cannot be null or empty");

            var decompressed = DecompressBytes(compressed);
            var text = Encoding.UTF8.GetString(decompressed);
            return Result.Ok(text);
        }
        catch (Exception ex)
        {
            return Result.Fail<string>("Error.DecompressString", ex.Message);
        }
    }

    /// <summary>
    /// Compresses a byte array using GZip compression.
    /// </summary>
    public static byte[] CompressBytes(byte[] data)
    {
        using var output = new MemoryStream();
        using (var gzip = new GZipStream(output, CompressionMode.Compress))
        {
            gzip.Write(data, 0, data.Length);
        }
        return output.ToArray();
    }

    /// <summary>
    /// Decompresses a GZip-compressed byte array.
    /// </summary>
    public static byte[] DecompressBytes(byte[] compressed)
    {
        using var input = new MemoryStream(compressed);
        using var output = new MemoryStream();
        using (var gzip = new GZipStream(input, CompressionMode.Decompress))
        {
            gzip.CopyTo(output);
        }
        return output.ToArray();
    }
}
