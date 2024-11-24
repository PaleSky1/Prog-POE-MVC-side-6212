namespace ContractClaimSystemMvc.Services
{
    public class FileService
{
    private readonly string _uploadsPath;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileService> _logger;
    
    // Allowed file types and max file size
    private readonly string[] _allowedExtensions = { ".pdf", ".doc", ".docx", ".txt", ".jpg", ".jpeg", ".png" };
    private const int MaxFileSizeInMB = 10;
    
    public FileService(IWebHostEnvironment environment, ILogger<FileService> logger)
    {
        _environment = environment;
        _logger = logger;
        _uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
        EnsureUploadDirectoryExists();
    }
    
    private void EnsureUploadDirectoryExists()
    {
        if (!Directory.Exists(_uploadsPath))
        {
            Directory.CreateDirectory(_uploadsPath);
        }
    }
    
    public async Task<(bool success, string filePath, string errorMessage)> SaveFileAsync(IFormFile file, string subfolder = "")
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return (false, null, "No file was uploaded.");
            }

            // Validate file size
            if (file.Length > MaxFileSizeInMB * 1024 * 1024)
            {
                return (false, null, $"File size exceeds the limit of {MaxFileSizeInMB}MB.");
            }

            // Validate file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                return (false, null, "Invalid file type.");
            }

            // Create safe filename
            var fileName = Path.GetFileNameWithoutExtension(file.FileName);
            var safeFileName = $"{fileName}_{DateTime.UtcNow.Ticks}{extension}"
                .Replace(" ", "_")
                .ToLowerInvariant();

            // Create subfolder if specified
            var targetPath = _uploadsPath;
            if (!string.IsNullOrEmpty(subfolder))
            {
                targetPath = Path.Combine(targetPath, subfolder);
                Directory.CreateDirectory(targetPath);
            }

            var filePath = Path.Combine(targetPath, safeFileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative path for database storage
            var relativePath = Path.Combine("uploads", subfolder, safeFileName).Replace("\\", "/");
            
            _logger.LogInformation($"File saved successfully: {relativePath}");
            return (true, relativePath, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving file");
            return (false, null, "Error saving file. Please try again.");
        }
    }

    public bool DeleteFile(string relativePath)
    {
        try
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                return false;
            }

            var fullPath = Path.Combine(_environment.WebRootPath, relativePath.TrimStart('/'));
            
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation($"File deleted successfully: {relativePath}");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file");
            return false;
        }
    }
}

}
