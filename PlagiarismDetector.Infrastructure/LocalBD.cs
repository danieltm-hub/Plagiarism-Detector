using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PlagiarismDetector.Application.Interfaces;
using PlagiarismDetector.Application.DTOs;

namespace PlagiarismDetector.Infrastructure;

public class LocalFileStorageRepository : IStorageRepository
{
    private readonly string _baseStoragePath;

    public LocalFileStorageRepository(string baseStoragePath)
    {
        _baseStoragePath = baseStoragePath;
    }

    public async Task UploadFileAsync(string bucketName, string fileName, FlowGraphs jsonContent, CancellationToken cancellationToken = default)
    {
        string folderPath = Path.Combine(_baseStoragePath, bucketName);

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, fileName);

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await using FileStream createStream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(createStream, jsonContent, options, cancellationToken);
    }

    public async Task<IEnumerable<FlowGraphResponse>> ListFilesAsync(string bucketName, CancellationToken cancellationToken = default)
    {
        string folderPath = Path.Combine(_baseStoragePath, bucketName);

        if (!Directory.Exists(folderPath))
        {
            return Enumerable.Empty<FlowGraphResponse>();
        }

        return await Task.Run(() =>
        {
            return Directory
                .GetFiles(folderPath)
                .Select(path =>
                {
                    if(!Path.IsPathFullyQualified(path))
                    {
                        path = Path.GetRelativePath("./", path);
                    }
                    var content = File.ReadAllText(path);

                    Console.WriteLine(path);

                    var graphs = JsonSerializer.Deserialize<FlowGraphs>(content);
                    return new FlowGraphResponse 
                    { 
                        filename = Path.GetFileName(path), 
                        flowGraphs = graphs! 
                    };
                });
        }, cancellationToken);
    }
}