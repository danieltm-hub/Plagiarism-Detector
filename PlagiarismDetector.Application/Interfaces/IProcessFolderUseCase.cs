namespace PlagiarismDetector.Application.Interfaces;

public interface IProcessFolderUseCase
{
    Task ProcessFolderAsync(string folderPath, CancellationToken cancellationToken = default);
}