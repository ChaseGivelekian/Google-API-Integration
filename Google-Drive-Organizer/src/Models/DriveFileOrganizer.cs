using Google_Drive_Organizer.Interfaces;

namespace Google_Drive_Organizer.Models;

public class DriveFileOrganizer(IGoogleDriveService driveService)
{
    private readonly IGoogleDriveService _driveService = driveService ?? throw new ArgumentNullException(nameof(driveService));

    public async Task ListAllFilesAsync()
    {
        try
        {
            var files = await _driveService.ListFilesAsync(
                "mimeType != 'application/vnd.google-apps.folder'",
                1000,
                "modifiedByMeTime desc");

            if (files.Count == 0)
            {
                Console.WriteLine("No files found");
                return;
            }

            Console.WriteLine("Files:");
            foreach (var file in files)
            {
                Console.WriteLine($"{file.Name}");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error listing files: {e.Message}");
            throw;
        }
    }
}