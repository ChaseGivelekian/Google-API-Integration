using Google_Drive_Organizer.Models;
using Google_Drive_Organizer.Services;

namespace Google_Drive_Organizer;

public static class Program
{
    public static async Task Main()
    {
        try
        {
            // Code to list all files in Google Drive
            // var driveService = await GoogleCredentialsManager.CreateDriveServiceAsync();
            // var googleDriveService = new GoogleDriveService(driveService);
            // var fileOrganizer = new DriveFileOrganizer(googleDriveService);
            //
            // await fileOrganizer.ListAllFilesAsync();


            // Code to list all courses in Google Classroom
            var classroomService = await GoogleCredentialsManager.CreateClassroomServiceAsync();
            var googleClassroomService = new GoogleClassroomService(classroomService);
            var courseLister = new ClassroomCourseLister(googleClassroomService);

            var courses = await new CourseWorkManager.Get
        }
        catch (Exception e)
        {
            Console.WriteLine($"Application error: {e.Message}");
            Environment.Exit(1);
        }
    }
}