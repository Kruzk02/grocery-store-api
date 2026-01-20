using Application.Common;

namespace Infrastructure.FileSystem;

public class FileSystemImageStorage : IImageStorage
{
    private static readonly HashSet<string> AllowedExtensions =
    [
        ".png", ".jpg", ".jpeg", ".webp"
    ];

    private readonly string _rootPath;
    private const long MaxSize = 5 * 1024 * 1024;

    public FileSystemImageStorage(string rootPath)
    {
        _rootPath = rootPath ?? throw new ArgumentNullException(nameof(rootPath));
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<String> Save(Stream imageStream, string fileExtension)
    {
        if (!AllowedExtensions.Contains(fileExtension))
            throw new Exception("Invalid iamge type");

        var filename = $"{Guid.NewGuid()}{fileExtension}";
        if (filename.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new Exception("Invalid filename");

        var fullPath = Path.Combine(_rootPath, filename);

        Directory.CreateDirectory(_rootPath);

        using var fs = new FileStream(fullPath, FileMode.Create);
        await imageStream.CopyToAsync(fs);

        return filename;
    }

    public string GetImage(string filename)
    {
        return GetSafePath(filename);
    }

    public Task Delete(string filename)
    {
        var path = GetSafePath(filename);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        return Task.CompletedTask;
    }

    private string GetSafePath(string filename)
    {
        filename = Path.GetFileName(filename);

        var fullPath = Path.GetFullPath(Path.Combine(_rootPath, filename));
        var rootFullPath = Path.GetFullPath(_rootPath);

        if (!fullPath.StartsWith(rootFullPath, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("Invalid file path.");
        }

        return fullPath;
    }
}
