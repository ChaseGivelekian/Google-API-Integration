using File = Google.Apis.Drive.v3.Data.File;

namespace Google_API_Integration.Interfaces;

public interface IGoogleDriveService
{
    Task<IList<File>> ListFilesAsync(string query, int pageSize, string orderBy);
}