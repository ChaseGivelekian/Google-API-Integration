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
                    if (work is not { DueDate: not null, DueTime: not null }) continue;
                    if (new DateTime((int)work.DueDate.Year!, (int)work.DueDate.Month!, (int)work.DueDate.Day!,
                            (int)work.DueTime.Hours!, (int)work.DueTime.Minutes!, 0) <= DateTime.Now) continue;
                    Console.WriteLine($"  - {work.Title} (Due: {work.DueDate.Month}-{work.DueDate.Day}-{work.DueDate.Year} {work.DueTime.Hours}:{work.DueTime.Minutes})");

                    // Gets the student submissions for a specific course work
                    var submissions = await googleClassroomService.GetStudentSubmissionsForSpecificCourseWorkAsync(course.Key, work.Id);

                    foreach (var submission in submissions)
                    {
                        Console.WriteLine($"    - {submission.Id} ({submission.State})");
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