using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Tarabezah.Infrastructure.Services;

public class CloudixFileUploadService : IFileUploadService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CloudixFileUploadService> _logger;
    public const string ClientName = "CloudixApi";

    public CloudixFileUploadService(
        IHttpClientFactory httpClientFactory, 
        IConfiguration configuration,
        ILogger<CloudixFileUploadService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient(ClientName);
            var endpoint = _configuration["ExternalServices:CloudixApi:MediaEndpoint"] ?? "api/files/Media";

            // Create multipart form content
            var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
            content.Add(fileContent, "file", fileName);

            // Upload to Cloudix API
            var response = await httpClient.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();

            // Parse the response to get the image URL
            var imageUrl = await response.Content.ReadAsStringAsync();
            
            // Return the image URL (trim quotes if they're returned by the API)
            return imageUrl.Trim('"');
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to Cloudix API: {FileName}", fileName);
            throw new ApplicationException($"Error uploading file: {ex.Message}", ex);
        }
    }
} 