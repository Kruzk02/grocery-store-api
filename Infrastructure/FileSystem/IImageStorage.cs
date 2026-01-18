namespace Infrastructure.FileSystem;

public interface IImageStorage
{
    Task<String> Save(Stream imageStream, string fileExtension, string contentType);
    Task Delete(string filename);
}
