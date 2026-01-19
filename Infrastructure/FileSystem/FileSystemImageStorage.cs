namespace Infrastructure.FileSystem;

public class FileSystemImageStorage : IImageStorage
{
    private readonly string _rootPath;
    private readonly string[] _allowedTypes = { "image/png", "image/jpeg", "image/webp" };
    private const long MaxSize = 5 * 1024 * 1024;

    public FileSystemImageStorage(string rootPath)
    {
        _rootPath = rootPath ?? throw new ArgumentNullException(nameof(rootPath));
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<String> Save(Stream imageStream, string fileExtension, string contentType)
    {
        if (!_allowedTypes.Contains(contentType))
            throw new Exception("Invalid iamge type");

        var filename = $"{Guid.NewGuid()}{fileExtension}";
        var fullPath = Path.Combine(_rootPath, filename);

        Directory.CreateDirectory(_rootPath);

        using var fs = new FileStream(fullPath, FileMode.Create);
        await imageStream.CopyToAsync(fs);

        return filename;
    }

    public string GetImage(string filename)
    {
        return Path.Combine(_rootPath, filename);
    }

    public Task Delete(string filename)
    {
        var path = Path.Combine(_rootPath, filename);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        return Task.CompletedTask;
    }
}
