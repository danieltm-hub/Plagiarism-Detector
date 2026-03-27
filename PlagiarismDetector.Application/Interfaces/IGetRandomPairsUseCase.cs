using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PlagiarismDetector.Application.DTOs;

namespace PlagiarismDetector.Application.Interfaces;

public interface IGetRandomPairsUseCase
{
    Task<IEnumerable<ProjectPair>> ExecuteAsync(string bucketName, int count = 5, CancellationToken cancellationToken = default);
}