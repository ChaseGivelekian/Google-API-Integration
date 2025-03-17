using Google_Drive_Organizer.Interfaces;
using Google_Drive_Organizer.Models;

namespace Google_Drive_Organizer;

public class ClassroomApplication(CourseWorkManager courseWorkManager, IGoogleClassroomService googleClassroomService)
{
    private readonly CourseWorkManager _courseWorkManager = courseWorkManager ?? throw new ArgumentNullException(nameof(courseWorkManager));
    private readonly IGoogleClassroomService _googleClassroomService = googleClassroomService ?? throw new ArgumentNullException(nameof(googleClassroomService));

    public async Task RunAsync()
    {
        await DisplayCourseWorkInformation();
    }

    private async Task DisplayCourseWorkInformation()
    {
        var courses = await _courseWorkManager.GetAllCoursesWorkAsync();

        foreach (var course in courses)
        {
            Console.WriteLine($"Course: {course.Key}");

            foreach (var work in course.Value)
            {
                if (!HasValidDueDate(work) || IsPastDue(work)) continue;

                DisplayCourseWorkDetails(work);
                await DisplaySubmissionsForCourseWork(work);
            }
        }
    }

    private static bool HasValidDueDate(Google.Apis.Classroom.v1.Data.CourseWork work)
    {
        return work is { DueDate: not null, DueTime: not null };
    }

    private static bool IsPastDue(Google.Apis.Classroom.v1.Data.CourseWork work)
    {
        var dueDateTime = new DateTime(
            (int)work.DueDate.Year!,
            (int)work.DueDate.Month!,
            (int)work.DueDate.Day!,
            (int)work.DueTime.Hours!,
            (int)work.DueTime.Minutes!,
            0
        );

        return dueDateTime <= DateTime.Now;
    }

    private static void DisplayCourseWorkDetails(Google.Apis.Classroom.v1.Data.CourseWork work)
    {
        Console.WriteLine($"  - {work.Title} (Due: {work.DueDate.Month}-{work.DueDate.Day}-{work.DueDate.Year} {work.DueTime.Hours}:{work.DueTime.Minutes})");
    }

    private async Task DisplaySubmissionsForCourseWork(Google.Apis.Classroom.v1.Data.CourseWork work)
    {
        var submissions = await _googleClassroomService.GetStudentSubmissionsForSpecificCourseWorkAsync(work.CourseId, work.Id);

        if (!submissions.Any()) return;

        foreach (var submission in submissions)
        {
            Console.WriteLine($"    - {submission.State}");
        }
    }
}
