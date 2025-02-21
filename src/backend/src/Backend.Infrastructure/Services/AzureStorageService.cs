// Azure.Storage.Blobs v12.13.0
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
// Microsoft.Extensions.Configuration v6.0.0
using Microsoft.Extensions.Configuration;
// Microsoft.Extensions.Logging v6.0.0
using Microsoft.Extensions.Logging;
// System.IO v6.0.0
using System.IO;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Backend.Infrastructure.Services
{
    /// <summary>
    /// Provides enhanced Azure Blob Storage operations with security, monitoring, and performance optimizations.
    /// Handles file storage for inspector documents and equipment-related files.
    /// </summary>
    public class AzureStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ILogger<AzureStorageService> _logger;
        private readonly string _connectionString;
        private readonly BlobClientOptions _clientOptions;
        private const int MaxRetries = 3;
        private const int TimeoutSeconds = 30;

        public AzureStorageService(
            IConfiguration configuration,
            ILogger<AzureStorageService> logger,
            BlobClientOptions clientOptions = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _connectionString = configuration.GetConnectionString("AzureStorage") 
                ?? throw new ArgumentException("Azure Storage connection string not found in configuration.");

            _clientOptions = clientOptions ?? new BlobClientOptions
            {
                Retry = {
                    MaxRetries = MaxRetries,
                    Mode = Azure.Core.RetryMode.Exponential,
                    Delay = TimeSpan.FromSeconds(1)
                },
                Diagnostics = {
                    IsDistributedTracingEnabled = true,
                    IsLoggingEnabled = true
                },
                Networking = {
                    ConnectionTimeout = TimeSpan.FromSeconds(TimeoutSeconds)
                }
            };

            _blobServiceClient = new BlobServiceClient(_connectionString, _clientOptions);
            
            ValidateStorageConnection().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Uploads a file to Azure Blob Storage with concurrent upload support for large files.
        /// </summary>
        public async Task<BlobUploadResult> UploadFileAsync(
            Stream fileStream,
            string containerName,
            string blobName,
            CancellationToken cancellationToken = default,
            Dictionary<string, string> metadata = null)
        {
            try
            {
                _logger.LogInformation("Starting file upload. Container: {Container}, Blob: {Blob}", 
                    containerName, blobName);

                if (fileStream == null || !fileStream.CanRead)
                    throw new ArgumentException("Invalid file stream provided.");

                var container = await GetOrCreateContainerAsync(containerName, cancellationToken);
                var blobClient = container.GetBlobClient(SanitizeBlobName(blobName));

                var options = new BlobUploadOptions
                {
                    Metadata = metadata,
                    TransferOptions = new StorageTransferOptions
                    {
                        MaximumConcurrency = 8,
                        MaximumTransferSize = 4 * 1024 * 1024 // 4MB chunks
                    }
                };

                var response = await blobClient.UploadAsync(fileStream, options, cancellationToken);
                
                _logger.LogInformation("File upload completed successfully. ETag: {ETag}", 
                    response.Value.ETag);

                return new BlobUploadResult
                {
                    Uri = blobClient.Uri,
                    ETag = response.Value.ETag.ToString(),
                    ContentHash = response.Value.ContentHash
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file to blob storage. Container: {Container}, Blob: {Blob}", 
                    containerName, blobName);
                throw;
            }
        }

        /// <summary>
        /// Downloads a file from Azure Blob Storage with retry and progress tracking.
        /// </summary>
        public async Task<BlobDownloadResult> DownloadFileAsync(
            string containerName,
            string blobName,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Starting file download. Container: {Container}, Blob: {Blob}", 
                    containerName, blobName);

                var container = await GetContainerAsync(containerName);
                var blobClient = container.GetBlobClient(SanitizeBlobName(blobName));

                if (!await blobClient.ExistsAsync(cancellationToken))
                    throw new FileNotFoundException($"Blob not found: {blobName}");

                var download = await blobClient.DownloadAsync(cancellationToken);
                var memoryStream = new MemoryStream();
                await download.Value.Content.CopyToAsync(memoryStream, cancellationToken);
                memoryStream.Position = 0;

                return new BlobDownloadResult
                {
                    Content = memoryStream,
                    Metadata = download.Value.Details.Metadata,
                    ContentType = download.Value.Details.ContentType
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file from blob storage. Container: {Container}, Blob: {Blob}", 
                    containerName, blobName);
                throw;
            }
        }

        /// <summary>
        /// Deletes a file from Azure Blob Storage.
        /// </summary>
        public async Task<bool> DeleteFileAsync(
            string containerName,
            string blobName,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var container = await GetContainerAsync(containerName);
                var blobClient = container.GetBlobClient(SanitizeBlobName(blobName));
                
                return await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file from blob storage. Container: {Container}, Blob: {Blob}", 
                    containerName, blobName);
                throw;
            }
        }

        /// <summary>
        /// Lists all files in a container with optional prefix filter.
        /// </summary>
        public async Task<IEnumerable<BlobItem>> ListFilesAsync(
            string containerName,
            string prefix = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var container = await GetContainerAsync(containerName);
                var results = new List<BlobItem>();
                
                await foreach (var blob in container.GetBlobsAsync(
                    prefix: prefix,
                    cancellationToken: cancellationToken))
                {
                    results.Add(blob);
                }
                
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing files in container: {Container}", containerName);
                throw;
            }
        }

        /// <summary>
        /// Generates a secure, time-limited URL for blob access.
        /// </summary>
        public async Task<string> GetFileUrlAsync(
            string containerName,
            string blobName,
            TimeSpan validFor,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var container = await GetContainerAsync(containerName);
                var blobClient = container.GetBlobClient(SanitizeBlobName(blobName));

                if (!await blobClient.ExistsAsync(cancellationToken))
                    throw new FileNotFoundException($"Blob not found: {blobName}");

                var sasBuilder = new BlobSasBuilder
                {
                    StartsOn = DateTimeOffset.UtcNow,
                    ExpiresOn = DateTimeOffset.UtcNow.Add(validFor),
                    BlobContainerName = containerName,
                    BlobName = blobName,
                    Resource = "b"
                };
                
                sasBuilder.SetPermissions(BlobSasPermissions.Read);
                
                return blobClient.GenerateSasUri(sasBuilder).ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating SAS URL. Container: {Container}, Blob: {Blob}", 
                    containerName, blobName);
                throw;
            }
        }

        private async Task<BlobContainerClient> GetOrCreateContainerAsync(
            string containerName,
            CancellationToken cancellationToken = default)
        {
            var container = _blobServiceClient.GetBlobContainerClient(containerName);
            await container.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
            return container;
        }

        private async Task<BlobContainerClient> GetContainerAsync(string containerName)
        {
            var container = _blobServiceClient.GetBlobContainerClient(containerName);
            if (!await container.ExistsAsync())
                throw new DirectoryNotFoundException($"Container not found: {containerName}");
            return container;
        }

        private async Task ValidateStorageConnection()
        {
            try
            {
                await _blobServiceClient.GetPropertiesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate Azure Storage connection");
                throw new InvalidOperationException("Could not connect to Azure Storage", ex);
            }
        }

        private static string SanitizeBlobName(string blobName)
        {
            return Uri.EscapeDataString(blobName)
                .Replace("%2F", "/")
                .Replace("%20", " ")
                .TrimStart('/');
        }
    }

    public class BlobUploadResult
    {
        public Uri Uri { get; set; }
        public string ETag { get; set; }
        public byte[] ContentHash { get; set; }
    }

    public class BlobDownloadResult
    {
        public Stream Content { get; set; }
        public IDictionary<string, string> Metadata { get; set; }
        public string ContentType { get; set; }
    }
}