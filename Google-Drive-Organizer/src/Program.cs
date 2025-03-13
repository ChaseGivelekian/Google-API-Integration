using Google_Drive_Organizer.Models;
using Google_Drive_Organizer.Services;

namespace Google_Drive_Organizer;

public static class Program
{
    public static async Task Main()
    {
        try
        {
            var driveService = await GoogleCredentialsManager.CreateDriveServiceAsync();
            var googleDriveService = new GoogleDriveService(driveService);
            var fileOrganizer = new DriveFileOrganizer(googleDriveService);

            await fileOrganizer.ListAllFilesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Application error: {e.Message}");
            Environment.Exit(1);
        }
    }
}