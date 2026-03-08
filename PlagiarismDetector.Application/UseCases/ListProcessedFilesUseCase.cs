using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PlagiarismDetector.Application.Interfaces;
using PlagiarismDetector.Domain.Interfaces;

namespace PlagiarismDetector.Application.UseCases;

public class ListProcessedFilesUseCase : IListProcessedFilesUseCase
{
    private readonly IStorageRepository _storageRepository;

    public ListProcessedFilesUseCase(IStorageRepository storageRepository)
    {
        _storageRepository = storageRepository;
    }

    public async Task<IEnumerable<string>> ExecuteAsync(string bucketName, CancellationToken cancellationToken = default)
    {
        return await _storageRepository.ListFilesAsync(bucketName, cancellationToken);
    }
}
