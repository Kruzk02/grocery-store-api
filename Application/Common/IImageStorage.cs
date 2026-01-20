namespace Application.Common;

public interface IImageStorage
{
    Task<String> Save(Stream imageStream, string fileExtension, string contentType);
    string GetImage(string filename);
    Task Delete(string filename);
}
