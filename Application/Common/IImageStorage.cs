namespace Application.Common;

public interface IImageStorage
{
    Task<String> Save(Stream imageStream, string fileExtension);
    string GetImage(string filename);
    Task Delete(string filename);
}
