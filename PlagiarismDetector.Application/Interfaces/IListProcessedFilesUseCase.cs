using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PlagiarismDetector.Application.Interfaces;
public interface IListProcessedFilesUseCase
{
    Task<IEnumerable<string>> ExecuteAsync(string bucketName, CancellationToken cancellationToken = default);
}