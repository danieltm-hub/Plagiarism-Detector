using PlagiarismDetector.Application.UseCases;
using PlagiarismDetector.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace PlagiarismDetector.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProcessingController : ControllerBase
{
    private readonly IProcessFolderUseCase _processFolderUseCase;

    public ProcessingController(IProcessFolderUseCase processFolderUseCase)
    {
        _processFolderUseCase = processFolderUseCase;
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

    public class FolderRequest
    {
        public required string FolderPath { get; set; }
    }
}