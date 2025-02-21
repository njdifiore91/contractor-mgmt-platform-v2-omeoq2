using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Backend.Core.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Graph;
using Polly;
using Polly.Retry;
using System.Security;

namespace Backend.Infrastructure.Services
{
    /// <summary>
    /// Enhanced implementation of IOneDriveService interface with security, monitoring, and compliance features.
    /// Provides secure file management for inspector documents using Microsoft Graph API.
    /// </summary>
    public class OneDriveService : IOneDriveService
    {
        private readonly GraphServiceClient _graphClient;
        private readonly IMemoryCache _cache;
        private readonly IAsyncPolicy<DriveItem> _retryPolicy;
        private readonly string _baseFolder;
        private readonly string[] _allowedFileTypes;
        private readonly long _maxFileSize;
        private const int CACHE_DURATION_MINUTES = 60;
        private const string FOLDER_CACHE_KEY_PREFIX = "inspector_folder_";
        
        public OneDriveService(
            IConfiguration configuration,
            GraphServiceClient graphClient,
            IMemoryCache cache)
        {
            _graphClient = graphClient ?? throw new ArgumentNullException(nameof(graphClient));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            
            _baseFolder = configuration["OneDrive:BaseFolder"] 
                ?? throw new InvalidOperationException("OneDrive base folder configuration is missing");
            
            _allowedFileTypes = configuration.GetSection("OneDrive:AllowedFileTypes")
                .Get<string[]>() ?? new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt" };
            
            _maxFileSize = configuration.GetValue<long>("OneDrive:MaxFileSize", 100 * 1024 * 1024); // Default 100MB
            
            // Configure retry policy
            _retryPolicy = Policy<DriveItem>
                .Handle<ServiceException>()
                .WaitAndRetryAsync(3, retryAttempt => 
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
            
            InitializeBaseFolder().GetAwaiter().GetResult();
        }

        private async Task InitializeBaseFolder()
        {
            try
            {
                var root = await _graphClient.Drive.Root
                    .ItemWithPath(_baseFolder)
                    .Request()
                    .GetAsync();
            }
            catch (ServiceException)
            {
                // Create base folder if it doesn't exist
                var folderItem = new DriveItem
                {
                    Name = _baseFolder,
                    Folder = new Folder()
                };

                await _graphClient.Drive.Root
                    .Children
                    .Request()
                    .AddAsync(folderItem);
            }
        }

        /// <inheritdoc/>
        public async Task<string> CreateInspectorFolder(string inspectorId)
        {
            if (string.IsNullOrEmpty(inspectorId))
                throw new ArgumentNullException(nameof(inspectorId));

            string cacheKey = $"{FOLDER_CACHE_KEY_PREFIX}{inspectorId}";

            if (_cache.TryGetValue(cacheKey, out string folderId))
                return folderId;

            try
            {
                var folderPath = Path.Combine(_baseFolder, inspectorId);
                var folder = await _retryPolicy.ExecuteAsync(async () =>
                {
                    try
                    {
                        return await _graphClient.Drive.Root
                            .ItemWithPath(folderPath)
                            .Request()
                            .GetAsync();
                    }
                    catch (ServiceException)
                    {
                        var folderItem = new DriveItem
                        {
                            Name = inspectorId,
                            Folder = new Folder()
                        };

                        return await _graphClient.Drive.Root
                            .ItemWithPath(_baseFolder)
                            .Children
                            .Request()
                            .AddAsync(folderItem);
                    }
                });

                _cache.Set(cacheKey, folder.Id, TimeSpan.FromMinutes(CACHE_DURATION_MINUTES));
                return folder.Id;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create/get inspector folder: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<string> UploadFile(string inspectorId, Stream fileStream, string fileName)
        {
            if (string.IsNullOrEmpty(inspectorId))
                throw new ArgumentNullException(nameof(inspectorId));
            if (fileStream == null)
                throw new ArgumentNullException(nameof(fileStream));
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));

            // Validate file type
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!_allowedFileTypes.Contains(extension))
                throw new SecurityException($"File type {extension} is not allowed");

            // Validate file size
            if (fileStream.Length > _maxFileSize)
                throw new ArgumentException($"File size exceeds maximum allowed size of {_maxFileSize} bytes");

            var folderId = await CreateInspectorFolder(inspectorId);

            try
            {
                var uploadedItem = await _retryPolicy.ExecuteAsync(async () =>
                {
                    return await _graphClient.Drive.Items[folderId]
                        .ItemWithPath(fileName)
                        .Content
                        .Request()
                        .PutAsync<DriveItem>(fileStream);
                });

                return uploadedItem.Id;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to upload file: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<string> GetFileUrl(string inspectorId, string fileName)
        {
            if (string.IsNullOrEmpty(inspectorId))
                throw new ArgumentNullException(nameof(inspectorId));
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));

            try
            {
                var folderId = await CreateInspectorFolder(inspectorId);
                var items = await _graphClient.Drive.Items[folderId]
                    .Children
                    .Request()
                    .Filter($"name eq '{fileName}'")
                    .GetAsync();

                var file = items.FirstOrDefault() 
                    ?? throw new FileNotFoundException($"File {fileName} not found in inspector folder");

                var permission = new Permission
                {
                    Type = "view",
                    Scope = "anonymous",
                    ExpirationDateTime = DateTimeOffset.UtcNow.AddHours(24)
                };

                var shareLink = await _graphClient.Drive.Items[file.Id]
                    .CreateLink(type: "view", scope: "anonymous")
                    .Request()
                    .PostAsync();

                return shareLink.Link.WebUrl;
            }
            catch (ServiceException ex)
            {
                throw new UnauthorizedAccessException($"Access denied: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DriveItem>> ListFiles(string inspectorId)
        {
            if (string.IsNullOrEmpty(inspectorId))
                throw new ArgumentNullException(nameof(inspectorId));

            try
            {
                var folderId = await CreateInspectorFolder(inspectorId);
                var items = await _graphClient.Drive.Items[folderId]
                    .Children
                    .Request()
                    .GetAsync();

                return items.CurrentPage;
            }
            catch (ServiceException ex)
            {
                throw new UnauthorizedAccessException($"Access denied: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteFile(string inspectorId, string fileName)
        {
            if (string.IsNullOrEmpty(inspectorId))
                throw new ArgumentNullException(nameof(inspectorId));
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));

            try
            {
                var folderId = await CreateInspectorFolder(inspectorId);
                var items = await _graphClient.Drive.Items[folderId]
                    .Children
                    .Request()
                    .Filter($"name eq '{fileName}'")
                    .GetAsync();

                var file = items.FirstOrDefault();
                if (file == null)
                    return false;

                await _graphClient.Drive.Items[file.Id]
                    .Request()
                    .DeleteAsync();

                return true;
            }
            catch (ServiceException ex)
            {
                throw new UnauthorizedAccessException($"Access denied: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to delete file: {ex.Message}", ex);
            }
        }
    }
}