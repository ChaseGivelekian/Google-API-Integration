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
            // var courseLister = new ClassroomCourseLister(googleClassroomService);

            var courses = await new CourseWorkManager(googleClassroomService).GetAllCoursesWorkAsync();
            foreach (var course in courses)
            {
                Console.WriteLine($"Course: {course.Key}");
                foreach (var work in course.Value)
                {
                    if (work is { DueDate: not null, DueTime: not null })
                    {
                        Console.WriteLine(
                            new DateTime((int)work.DueDate.Year!, (int)work.DueDate.Month!, (int)work.DueDate.Day!,
                                (int)work.DueTime.Hours!, (int)work.DueTime.Minutes!, 0) > DateTime.Now
                                ? $"  - {work.Title} (Due: {work.DueDate.Month}-{work.DueDate.Day}-{work.DueDate.Year} {work.DueTime.Hours}:{work.DueTime.Minutes})"
                                : $"  - {work.Title} PAST DUE");
                    }
                    else
                    {
                        Console.WriteLine($"  - {work.Title} (No due date)");
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Application error: {e.Message}");
            Environment.Exit(1);
        }
    }
}