namespace SmartWorkz.Shared;

public static class FileHelper
{
    public static async Task<Result<string>> ReadTextAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!System.IO.File.Exists(filePath))
                return Result.Fail<string>(Error.NotFound("File", filePath));
            var content = await System.IO.File.ReadAllTextAsync(filePath, cancellationToken);
            return Result.Ok(content);
        }
        catch (Exception ex) { return Result.Fail<string>(Error.FromException(ex, "FILE.READ_FAILED")); }
    }

    public static async Task<Result<byte[]>> ReadBytesAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!System.IO.File.Exists(filePath))
                return Result.Fail<byte[]>(Error.NotFound("File", filePath));
            var content = await System.IO.File.ReadAllBytesAsync(filePath, cancellationToken);
            return Result.Ok(content);
        }
        catch (Exception ex) { return Result.Fail<byte[]>(Error.FromException(ex, "FILE.READ_FAILED")); }
    }

    public static async Task<Result> WriteTextAsync(string filePath, string content, CancellationToken cancellationToken = default)
    {
        try
        {
            var directory = System.IO.Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
                System.IO.Directory.CreateDirectory(directory);
            await System.IO.File.WriteAllTextAsync(filePath, content, cancellationToken);
            return Result.Ok();
        }
        catch (Exception ex) { return Result.Fail(Error.FromException(ex, "FILE.WRITE_FAILED")); }
    }

    public static async Task<Result> WriteBytesAsync(string filePath, byte[] content, CancellationToken cancellationToken = default)
    {
        try
        {
            var directory = System.IO.Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
                System.IO.Directory.CreateDirectory(directory);
            await System.IO.File.WriteAllBytesAsync(filePath, content, cancellationToken);
            return Result.Ok();
        }
        catch (Exception ex) { return Result.Fail(Error.FromException(ex, "FILE.WRITE_FAILED")); }
    }

    public static async Task<Result> AppendTextAsync(string filePath, string content, CancellationToken cancellationToken = default)
    {
        try
        {
            var directory = System.IO.Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
                System.IO.Directory.CreateDirectory(directory);
            await System.IO.File.AppendAllTextAsync(filePath, content, cancellationToken);
            return Result.Ok();
        }
        catch (Exception ex) { return Result.Fail(Error.FromException(ex, "FILE.APPEND_FAILED")); }
    }

    public static Result DeleteFile(string filePath)
    {
        try
        {
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
            return Result.Ok();
        }
        catch (Exception ex) { return Result.Fail(Error.FromException(ex, "FILE.DELETE_FAILED")); }
    }

    public static bool Exists(string filePath) => System.IO.File.Exists(filePath);

    public static Result<long> GetFileSize(string filePath)
    {
        try
        {
            if (!System.IO.File.Exists(filePath))
                return Result.Fail<long>(Error.NotFound("File", filePath));
            var info = new System.IO.FileInfo(filePath);
            return Result.Ok(info.Length);
        }
        catch (Exception ex) { return Result.Fail<long>(Error.FromException(ex, "FILE.SIZE_FAILED")); }
    }

    public static Result CopyFile(string sourcePath, string destinationPath, bool overwrite = false)
    {
        try
        {
            if (!System.IO.File.Exists(sourcePath))
                return Result.Fail(Error.NotFound("File", sourcePath));
            System.IO.File.Copy(sourcePath, destinationPath, overwrite);
            return Result.Ok();
        }
        catch (Exception ex) { return Result.Fail(Error.FromException(ex, "FILE.COPY_FAILED")); }
    }

    public static Result MoveFile(string sourcePath, string destinationPath, bool overwrite = false)
    {
        try
        {
            if (!System.IO.File.Exists(sourcePath))
                return Result.Fail(Error.NotFound("File", sourcePath));
            System.IO.File.Move(sourcePath, destinationPath, overwrite);
            return Result.Ok();
        }
        catch (Exception ex) { return Result.Fail(Error.FromException(ex, "FILE.MOVE_FAILED")); }
    }

    public static string GetSafeFileName(string fileName)
    {
        var invalidChars = new string(System.IO.Path.GetInvalidFileNameChars()) + new string(System.IO.Path.GetInvalidPathChars());
        return invalidChars.Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
    }
}
