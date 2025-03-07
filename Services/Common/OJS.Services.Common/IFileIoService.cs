namespace OJS.Services.Common;

using OJS.Services.Infrastructure;
using System.Threading.Tasks;

public interface IFileIoService : IService
{
    void CreateDirectory(string directoryPath);

    string CreateTempDirectory();

    bool DirectoryExists(string directoryPath);

    void SafeDeleteDirectory(string path, bool recursive = false);

    Task SaveFile(string path, byte[] content);
}