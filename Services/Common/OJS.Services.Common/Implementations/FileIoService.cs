namespace OJS.Services.Common.Implementations;

using System.IO;
using System.Threading.Tasks;

public class FileIoService(IFileSystemService fileSystem) : IFileIoService
{
    public void CreateDirectory(string directoryPath)
        => Directory.CreateDirectory(directoryPath);

    public string CreateTempDirectory()
    {
        while (true)
        {
            var randomDirectoryName = fileSystem.GetRandomFileName();
            var path = fileSystem.BuildPath(fileSystem.GetTempPath(), randomDirectoryName);
            if (this.DirectoryExists(path))
            {
                continue;
            }

            this.CreateDirectory(path);
            return path;
        }
    }

    public bool DirectoryExists(string directoryPath)
        => Directory.Exists(directoryPath);

    public void SafeDeleteDirectory(string path, bool recursive = false)
    {
        if (!this.DirectoryExists(path))
        {
            return;
        }

        var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        foreach (var fileSystemEntry in Directory.EnumerateFileSystemEntries(path, "*", searchOption))
        {
            File.SetAttributes(fileSystemEntry, FileAttributes.Normal);
        }

        Directory.Delete(path, recursive);
    }

    public async Task SaveFile(string path, byte[] content)
    {
        await using var fileStream = new FileStream(path, FileMode.Create);
        await fileStream.WriteAsync(content);
    }
}