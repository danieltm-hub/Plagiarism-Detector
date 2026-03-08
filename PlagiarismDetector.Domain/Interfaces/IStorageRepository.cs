namespace PlagiarismDetector.Domain.Interfaces;

public interface IStorageRepository
{
    Task UploadFileAsync(string bucketName, string fileName, string jsonContent, CancellationToken cancellationToken = default);

    Task<IEnumerable<string>> ListFilesAsync(string bucketName, CancellationToken cancellationToken = default);
}