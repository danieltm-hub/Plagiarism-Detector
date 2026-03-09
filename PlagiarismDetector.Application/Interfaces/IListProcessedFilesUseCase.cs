using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PlagiarismDetector.Application.DTOs;

namespace PlagiarismDetector.Application.Interfaces;
public interface IListProcessedFilesUseCase
{
    Task<IEnumerable<FlowGraphs>> ExecuteAsync(string bucketName, CancellationToken cancellationToken = default);
}