using Google_Drive_Organizer.Interfaces;
using Google_Drive_Organizer.Models;
using Google.Apis.Classroom.v1.Data;

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

    private static bool HasValidDueDate(CourseWork work)
    {
        return work is { DueDate: not null, DueTime: not null };
    }

    private static bool IsPastDue(CourseWork work)
    {
        // Ensure all required values are present
        if (!work.DueDate.Year.HasValue || !work.DueDate.Month.HasValue || !work.DueDate.Day.HasValue ||
            !work.DueTime.Hours.HasValue || !work.DueTime.Minutes.HasValue)
        {
            return false; // Can't determine if it's past due without complete date/time
        }

        var dueDateTime = new DateTime(
            work.DueDate.Year.Value,
            work.DueDate.Month.Value,
            work.DueDate.Day.Value,
            work.DueTime.Hours.Value,
            work.DueTime.Minutes.Value,
            0
        );

        return dueDateTime <= DateTime.Now;
    }

    private static void DisplayCourseWorkDetails(CourseWork work)
    {
        // Make sure all values are available before formatting
        if (work.DueDate?.Month.HasValue == true && work.DueDate.Day.HasValue && work.DueDate.Year.HasValue &&
            work.DueTime?.Hours.HasValue == true && work.DueTime.Minutes.HasValue)
        {
            Console.WriteLine($"  - {work.Title} (Due: {work.DueDate.Month.Value}-{work.DueDate.Day.Value}-{work.DueDate.Year.Value} {work.DueTime.Hours.Value}:{work.DueTime.Minutes.Value:D2})");
        }
        else
        {
            Console.WriteLine($"  - {work.Title} (Due date not fully specified)");
        }
    }

    private async Task DisplaySubmissionsForCourseWork(CourseWork work)
    {
        var submissions = await _googleClassroomService.GetStudentSubmissionsForSpecificCourseWorkAsync(work.CourseId, work.Id);

        if (!submissions.Any()) return;

        foreach (var submission in submissions)
        {
            Console.WriteLine($"    - {submission.State}");

            if (!IsActiveSubmission(submission)) continue;
            if (ContainsDocument(submission))
            {
                Console.WriteLine("      Contains a Google Docs attachment");
            }
        }
    }

    private static bool IsActiveSubmission(StudentSubmission submission)
    {
        // Check if submission is in one of the active states
        return submission.State is "NEW" or "CREATED";
    }

    private static bool ContainsDocument(StudentSubmission submission)
    {
        // Check if the submission contains a document attachment
        if (submission.AssignmentSubmission?.Attachments == null)
        {
            return false;
        }

        return submission.AssignmentSubmission.Attachments.Any(attachment =>
            attachment.DriveFile != null &&
            IsGoogleDocument(attachment.DriveFile));
    }

    private static bool IsGoogleDocument(DriveFile driveFile)
    {
        return driveFile.AlternateLink?.Contains("docs.google.com/document", StringComparison.OrdinalIgnoreCase) ?? false;
    }
}
