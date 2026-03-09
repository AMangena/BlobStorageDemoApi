using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BlobStorageDemoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BlobController : ControllerBase
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    public BlobController(
        BlobServiceClient blobServiceClient,
        IConfiguration config)
    {
        _blobServiceClient = blobServiceClient;
        _containerName = config["AzureBlobStorage:ContainerName"] 
                         ?? throw new InvalidOperationException("ContainerName not configured");
    }

    // GET: api/blob
    [HttpGet]
    public async Task<IActionResult> ListBlobs()
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        
        // Ensure container exists (optional - create if needed)
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob); // or Private, etc.

        var blobs = new List<object>();
        await foreach (var blobItem in containerClient.GetBlobsAsync())
        {
            var blobClient = containerClient.GetBlobClient(blobItem.Name);
            var properties = await blobClient.GetPropertiesAsync();

            blobs.Add(new
            {
                Name = blobItem.Name,
                Size = properties.Value.ContentLength,
                LastModified = properties.Value.LastModified,
                ContentType = properties.Value.ContentType,
                Metadata = properties.Value.Metadata // includes your custom x-ms-meta-*
            });
        }

        return Ok(blobs);
    }

    // POST: api/blob/upload
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file, [FromQuery] string? folder = null)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync();

        // Optional: organize in virtual folder
        var blobName = string.IsNullOrEmpty(folder) 
            ? file.FileName 
            : $"{folder.TrimEnd('/')}/{file.FileName}";

        var blobClient = containerClient.GetBlobClient(blobName);

        // Optional: add custom metadata
        var metadata = new Dictionary<string, string>
        {
            { "uploadedBy", "Ashelly" },
            { "location", "Johannesburg" },
            { "uploadTime", DateTimeOffset.UtcNow.ToString("o") }
        };

        var options = new BlobUploadOptions
        {
            Metadata = metadata,
            HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType }
        };

        await using var stream = file.OpenReadStream();
        await blobClient.UploadAsync(stream, options);

        return Ok(new { Message = "File uploaded", BlobName = blobName, Uri = blobClient.Uri });
    }

    // GET: api/blob/download/{blobName}
    [HttpGet("download/{*blobName}")]
    public async Task<IActionResult> Download(string blobName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        if (!await blobClient.ExistsAsync())
            return NotFound();

        var response = await blobClient.DownloadStreamingAsync();

        return File(response.Value.Content, response.Value.Details.ContentType, blobName);
    }

    // DELETE: api/blob/{blobName}
    [HttpDelete("{*blobName}")]
    public async Task<IActionResult> Delete(string blobName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        if (!await blobClient.ExistsAsync())
            return NotFound();

        await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
        return Ok(new { Message = "Blob deleted", BlobName = blobName });
    }
}