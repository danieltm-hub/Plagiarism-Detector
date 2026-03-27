using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PlagiarismDetector.Application.DTOs;
using PlagiarismDetector.Application.Interfaces;

namespace PlagiarismDetector.Application.UseCases;

public class GetRandomPairsUseCase : IGetRandomPairsUseCase
{
    private readonly IStorageRepository _storageRepository;

    public GetRandomPairsUseCase(IStorageRepository storageRepository)
    {
        _storageRepository = storageRepository;
    }

    public async Task<IEnumerable<ProjectPair>> ExecuteAsync(string bucketName, int count = 5, CancellationToken cancellationToken = default)
    {
        var files = (await _storageRepository.ListFilesAsync(bucketName, cancellationToken)).ToList();

        if (files.Count < 2)
        {
            return Enumerable.Empty<ProjectPair>();
        }

        int maxPossiblePairs = files.Count * (files.Count - 1) / 2;
        int actualCount = Math.Min(count, maxPossiblePairs);

        var rng = new Random();
        var selectedIndices = new HashSet<(int, int)>();
        var result = new List<ProjectPair>();

        while (selectedIndices.Count < actualCount)
        {
            int i = rng.Next(files.Count);
            int j = rng.Next(files.Count);

            if (i == j) continue;

            var pair = i < j ? (i, j) : (j, i);

            if (selectedIndices.Add(pair))
            {
                result.Add(new ProjectPair(files[pair.Item1], files[pair.Item2]));
            }
        }

        return result;
    }
}