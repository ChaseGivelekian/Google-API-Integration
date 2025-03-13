using Google_Drive_Organizer.Interfaces;
using Google.Apis.Drive.v3;
using File = Google.Apis.Drive.v3.Data.File;

namespace Google_Drive_Organizer.Services;

public class GoogleDriveService(DriveService driveService) : IGoogleDriveService
{
    private readonly DriveService _driveService = driveService ?? throw new ArgumentNullException(nameof(driveService));

    public async Task<IList<File>> ListFilesAsync(string query, int pageSize, string orderBy)
    {
        var listRequest = _driveService.Files.List();
        listRequest.PageSize = pageSize;
        listRequest.OrderBy = orderBy;
        listRequest.Q = query;

        var result = await listRequest.ExecuteAsync();
        return result.Files;
    }
}