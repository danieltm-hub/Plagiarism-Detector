using PlagiarismDetector.Application.DTOs;

namespace PlagiarismDetector.Application.Interfaces;

public interface IStorageRepository
{
    Task UploadFileAsync(string bucketName, string fileName, FlowGraphs content, CancellationToken cancellationToken = default);

    Task<IEnumerable<FlowGraphResponse>> ListFilesAsync(string bucketName, CancellationToken cancellationToken = default);
}