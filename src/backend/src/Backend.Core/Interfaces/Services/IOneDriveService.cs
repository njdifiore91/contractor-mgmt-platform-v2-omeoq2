using System.Threading.Tasks;
using System.IO;
using Microsoft.Graph; // v4.0.0

namespace Backend.Core.Interfaces.Services
{
    /// <summary>
    /// Defines the contract for OneDrive service operations to manage inspector-related files and folders.
    /// Provides secure file management capabilities with proper validation and enterprise-grade security controls.
    /// </summary>
    public interface IOneDriveService
    {
        /// <summary>
        /// Creates or retrieves a dedicated folder for an inspector in OneDrive with proper access controls.
        /// </summary>
        /// <param name="inspectorId">The unique identifier of the inspector.</param>
        /// <returns>The unique identifier or path of the created/existing folder in OneDrive.</returns>
        /// <exception cref="ArgumentNullException">Thrown when inspectorId is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when inspector does not exist in the system.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when access to OneDrive is denied.</exception>
        Task<string> CreateInspectorFolder(string inspectorId);

        /// <summary>
        /// Securely uploads a file to an inspector's OneDrive folder with validation and error handling.
        /// </summary>
        /// <param name="inspectorId">The unique identifier of the inspector.</param>
        /// <param name="fileStream">The stream containing the file content to upload.</param>
        /// <param name="fileName">The name of the file to be uploaded.</param>
        /// <returns>The unique identifier of the uploaded file in OneDrive.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown when fileName is invalid or fileStream is empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when upload operation fails.</exception>
        /// <exception cref="SecurityException">Thrown when file type is not allowed or security validation fails.</exception>
        Task<string> UploadFile(string inspectorId, Stream fileStream, string fileName);

        /// <summary>
        /// Generates a secure, time-limited shareable URL for accessing a specific file.
        /// </summary>
        /// <param name="inspectorId">The unique identifier of the inspector.</param>
        /// <param name="fileName">The name of the file to generate URL for.</param>
        /// <returns>A secure, time-limited URL for accessing the file.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when access to file is denied.</exception>
        Task<string> GetFileUrl(string inspectorId, string fileName);

        /// <summary>
        /// Retrieves a paginated list of files in an inspector's folder with metadata.
        /// </summary>
        /// <param name="inspectorId">The unique identifier of the inspector.</param>
        /// <returns>Collection of file information including metadata and access details.</returns>
        /// <exception cref="ArgumentNullException">Thrown when inspectorId is null.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown when inspector folder does not exist.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when access to folder is denied.</exception>
        Task<IEnumerable<DriveItem>> ListFiles(string inspectorId);

        /// <summary>
        /// Securely removes a file from an inspector's folder with proper validation.
        /// </summary>
        /// <param name="inspectorId">The unique identifier of the inspector.</param>
        /// <param name="fileName">The name of the file to delete.</param>
        /// <returns>True if deletion successful, false if file not found or operation failed.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when delete permission is denied.</exception>
        /// <exception cref="InvalidOperationException">Thrown when deletion operation fails.</exception>
        Task<bool> DeleteFile(string inspectorId, string fileName);
    }
}