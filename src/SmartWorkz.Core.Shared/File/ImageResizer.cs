namespace SmartWorkz.Core.Shared.File;

public static class ImageResizer
{
    public static Result<byte[]> ResizeImage(byte[] imageBytes, int width, int height, string outputFormat = "JPEG")
    {
        try
        {
            using var stream = new MemoryStream(imageBytes);
            var bitmap = new System.Drawing.Bitmap(stream);
            var resized = new System.Drawing.Bitmap(width, height);
            using var graphics = System.Drawing.Graphics.FromImage(resized);
            {
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                graphics.DrawImage(bitmap, 0, 0, width, height);
            }
            using var outputStream = new MemoryStream();
            var format = GetImageFormat(outputFormat);
            resized.Save(outputStream, format);
            return Result.Ok(outputStream.ToArray());
        }
        catch (Exception ex) { return Result.Fail<byte[]>(Error.FromException(ex, "IMAGE.RESIZE_FAILED")); }
    }

    public static Result<byte[]> GenerateThumbnail(byte[] imageBytes, int thumbWidth, int thumbHeight)
        => ResizeImage(imageBytes, thumbWidth, thumbHeight, "JPEG");

    public static Result<byte[]> ConvertImageFormat(byte[] imageBytes, string targetFormat)
    {
        try
        {
            using var stream = new MemoryStream(imageBytes);
            var bitmap = new System.Drawing.Bitmap(stream);
            using var outputStream = new MemoryStream();
            var format = GetImageFormat(targetFormat);
            bitmap.Save(outputStream, format);
            return Result.Ok(outputStream.ToArray());
        }
        catch (Exception ex) { return Result.Fail<byte[]>(Error.FromException(ex, "IMAGE.CONVERT_FAILED")); }
    }

    public static Result<(int Width, int Height)> GetImageDimensions(byte[] imageBytes)
    {
        try
        {
            using var stream = new MemoryStream(imageBytes);
            var bitmap = new System.Drawing.Bitmap(stream);
            return Result.Ok((bitmap.Width, bitmap.Height));
        }
        catch (Exception ex) { return Result.Fail<(int, int)>(Error.FromException(ex, "IMAGE.DIMENSIONS_FAILED")); }
    }

    private static System.Drawing.Imaging.ImageFormat GetImageFormat(string format) => format.ToUpperInvariant() switch
    {
        "JPEG" or "JPG" => System.Drawing.Imaging.ImageFormat.Jpeg,
        "PNG" => System.Drawing.Imaging.ImageFormat.Png,
        "BMP" => System.Drawing.Imaging.ImageFormat.Bmp,
        "GIF" => System.Drawing.Imaging.ImageFormat.Gif,
        "TIFF" => System.Drawing.Imaging.ImageFormat.Tiff,
        _ => System.Drawing.Imaging.ImageFormat.Jpeg
    };
}
