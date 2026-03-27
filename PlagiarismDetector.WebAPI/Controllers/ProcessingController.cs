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
    private readonly IGetRandomPairsUseCase _getRandomPairsUseCase;

    public ProcessingController(IProcessFolderUseCase processFolderUseCase, IListProcessedFilesUseCase listProcessedFilesUseCase, IGetRandomPairsUseCase getRandomPairsUseCase)
    {
        _processFolderUseCase = processFolderUseCase;
        _listProcessedFilesUseCase = listProcessedFilesUseCase;
        _getRandomPairsUseCase = getRandomPairsUseCase;
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

    [HttpGet("random-pairs/{bucketName}")]
    public async Task<IActionResult> GetRandomPairs(string bucketName, [FromQuery] int count = 5, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(bucketName))
        {
            return BadRequest("El nombre de la carpeta (bucket) es requerido.");
        }

        try
        {
            var pairs = await _getRandomPairsUseCase.ExecuteAsync(bucketName, count, cancellationToken);
            
            return Ok(new 
            {
                Bucket = bucketName,
                TotalPairs = pairs.Count(),
                Pairs = pairs
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ocurrió un error al listar los pares: {ex.Message}");
        }
    }

    public class FolderRequest
    {
        public required string FolderPath { get; set; }
    }
}