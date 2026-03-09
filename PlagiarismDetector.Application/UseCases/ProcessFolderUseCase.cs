using PlagiarismDetector.Application.Interfaces;
using PlagiarismDetector.Application.Services;
using PlagiarismDetector.Domain.Enums;

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
        var storeData = UploadProjectsFolder.ExtractFlowGraphs(folderPath, ImportType.Solution);

        foreach (var toStore in storeData)
            await _storageRepository.UploadFileAsync("plagiarism-detector", $"{toStore.filename}.json", toStore.flowGraphs);
    }
}