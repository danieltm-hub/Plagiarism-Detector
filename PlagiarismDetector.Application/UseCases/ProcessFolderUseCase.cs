using PlagiarismDetector.Domain.Interfaces;
using PlagiarismDetector.Application.Interfaces;

namespace PlagiarismDetector.Application.UseCases;

public class ProcessFolderUseCase : IProcessFolderUseCase
{
    private readonly IStorageRepository _storageRepository;

    public ProcessFolderUseCase(IStorageRepository storageRepository)
    {
        _storageRepository = storageRepository;
    }

    public async Task ProcessFolderAsync(string folderPath, CancellationToken cancellationToken = default)
    {
        string[] filenames = new string[0];

        var files = Directory.GetFiles(folderPath);
        string filename = folderPath.Split("\\").Last();

        foreach (var file in files)
        {
            filenames.Append(file.Split("\\").Last());
        }

        string mockContent = $"content: {string.Join(',', filenames)}";

        await _storageRepository.UploadFileAsync("plagiarism-detector", $"{filename}.json", mockContent);
    }
}