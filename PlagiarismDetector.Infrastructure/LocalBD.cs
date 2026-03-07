using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PlagiarismDetector.Domain.Interfaces;

namespace PlagiarismDetector.Infrastructure;

public class LocalFileStorageRepository : IStorageRepository
{
    private readonly string _baseStoragePath;

    public LocalFileStorageRepository(string baseStoragePath)
    {
        _baseStoragePath = baseStoragePath;
    }

    public async Task UploadFileAsync(string bucketName, string fileName, string jsonContent, CancellationToken cancellationToken = default)
    {
        string folderPath = Path.Combine(_baseStoragePath, bucketName);

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, fileName);

        await File.WriteAllTextAsync(filePath, jsonContent, cancellationToken);
    }
}