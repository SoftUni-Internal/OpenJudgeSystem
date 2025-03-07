namespace OJS.Services.Common;

using Microsoft.AspNetCore.Http;
using OJS.Services.Infrastructure;

public interface IFileSystemService : IService
{
    string GetFileExtension(IFormFile file);

    string BuildPath(params string[] paths);

    string GetRandomFileName();

    string GetTempPath();

    string GetTempDirectory(string directoryName);

    bool FileExists(string filePath);
}