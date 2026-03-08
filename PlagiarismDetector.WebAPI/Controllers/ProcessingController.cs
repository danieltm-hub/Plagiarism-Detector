using PlagiarismDetector.Application.UseCases;
using PlagiarismDetector.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace PlagiarismDetector.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProcessingController : ControllerBase
{
    private readonly IProcessFolderUseCase _processFolderUseCase;
    private readonly IListProcessedFilesUseCase _listProcessedFilesUseCase;

    public ProcessingController(IProcessFolderUseCase processFolderUseCase, IListProcessedFilesUseCase listProcessedFilesUseCase)
    {
        _processFolderUseCase = processFolderUseCase;
        _listProcessedFilesUseCase = listProcessedFilesUseCase;
    }

    [HttpPost("process-folder")]
    public async Task<IActionResult> ProcessFolder([FromBody] FolderRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(request.FolderPath))
        {
            return BadRequest("Folder path is required");
        }

        try
        {
            await _processFolderUseCase.ProcessFolderAsync(request.FolderPath, cancellationToken);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    [HttpGet("list-files/{bucketName}")]
    public async Task<IActionResult> ListFiles(string bucketName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(bucketName))
        {
            return BadRequest("El nombre de la carpeta (bucket) es requerido.");
        }

        try
        {
            var files = await _listProcessedFilesUseCase.ExecuteAsync(bucketName, cancellationToken);
            
            return Ok(new 
            {
                Bucket = bucketName,
                TotalFiles = files.Count(),
                Files = files
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ocurrió un error al listar los archivos: {ex.Message}");
        }
    }

    public class FolderRequest
    {
        public required string FolderPath { get; set; }
    }
}