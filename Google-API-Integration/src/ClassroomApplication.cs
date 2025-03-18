using Google_Drive_Organizer.Interfaces;
using Google_Drive_Organizer.Models;
using Google.Apis.Classroom.v1.Data;

namespace Google_Drive_Organizer;

public class ClassroomApplication(CourseWorkManager courseWorkManager, IGoogleClassroomService googleClassroomService)
{
    private readonly CourseWorkManager _courseWorkManager =
        courseWorkManager ?? throw new ArgumentNullException(nameof(courseWorkManager));

    private readonly IGoogleClassroomService _googleClassroomService =
        googleClassroomService ?? throw new ArgumentNullException(nameof(googleClassroomService));

    public async Task RunAsync()
    {
        await DisplayCourseWorkInformationBatched();
    }

    private async Task DisplayCourseWorkInformationBatched()
    {
        var courses = await _courseWorkManager.GetAllCoursesWorkAsync();
        
        // Group all valid coursework by course ID for batch processing
        var validWorkByCourse = new Dictionary<string, List<(string courseName, CourseWork work)>> ();

        foreach (var (courseName, value) in courses)
        {
            var validWorks = value.Where(work => HasValidDueDate(work) && !IsPastDue(work))
                .Select(work => (courseName, work)).ToList();

            if (validWorks.Count == 0) continue;

            foreach (var (_, work) in validWorks)
            {
                var courseId = work.CourseId;
                if (!validWorkByCourse.TryGetValue(courseId, out var list))
                {
                    list = new List<(string courseName, CourseWork)>();
                    validWorkByCourse[courseId] = list;
                }

                list.Add((courseName, work));
            }
        }
        
        // Process each course in batch
        foreach (var (courseId, workItems) in validWorkByCourse)
        {
            // Get all courseWork IDs for this course
            var courseWorkIds = workItems.Select(w => w.work.Id).ToList();
            
            // Batch fetches all submissions for this course's work items
            var allSubmissions =
                await _googleClassroomService.GetStudentSubmissionsForMultipleCourseWorksAsync(courseId, courseWorkIds);
            
            // Process results
            DisplayBatchedSubmissions(workItems, allSubmissions);
        }
    }
    
    private static void DisplayBatchedSubmissions(
        List<(string courseName, CourseWork work)> workItems, 
        Dictionary<string, IList<StudentSubmission>> submissionsByCourseWorkId)
    {
        foreach (var (courseName, work) in workItems)
        {
            if (!submissionsByCourseWorkId.TryGetValue(work.Id, out var submissions))
                continue;
            
            var courseDisplayed = false;
        
            foreach (var submission in submissions)
            {
                if (!IsActiveSubmission(submission) || !ContainsDocument(submission)) 
                    continue;
                
                if (!courseDisplayed)
                {
                    Console.WriteLine($"Course: {courseName}");
                
                    if (work.DueDate?.Month.HasValue == true && work.DueDate.Day.HasValue && work.DueDate.Year.HasValue &&
                        work.DueTime?.Hours.HasValue == true && work.DueTime.Minutes.HasValue)
                    {
                        Console.WriteLine(
                            $"  - {work.Title} (Due: {work.DueDate.Month.Value}-{work.DueDate.Day.Value}-{work.DueDate.Year.Value} {work.DueTime.Hours.Value}:{work.DueTime.Minutes.Value:D2})");
                    }
                    else
                    {
                        Console.WriteLine($"  - {work.Title} (Due date not fully specified)");
                    }
                
                    courseDisplayed = true;
                }
                Console.WriteLine($"    - {submission.State}");
            }
        }
    }

    private async Task DisplayCourseWorkInformation()
    {
        var courses = await _courseWorkManager.GetAllCoursesWorkAsync();

        foreach (var course in courses)
        {
            foreach (var work in course.Value)
            {
                if (!HasValidDueDate(work) || IsPastDue(work)) continue;

                await DisplaySubmissionsWithDocsForCourseWork(course.Key, work);
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

    private async Task DisplaySubmissionsWithDocsForCourseWork(string courseName, CourseWork work)
    {
        var submissions =
            await _googleClassroomService.GetStudentSubmissionsForSpecificCourseWorkAsync(work.CourseId, work.Id);

        var courseDisplayed = false;

        foreach (var submission in submissions)
        {
            if (!IsActiveSubmission(submission) || !ContainsDocument(submission)) continue;

            if (!courseDisplayed)
            {
                Console.WriteLine($"Course: {courseName}");

                if (work.DueDate?.Month.HasValue == true && work.DueDate.Day.HasValue && work.DueDate.Year.HasValue &&
                    work.DueTime?.Hours.HasValue == true && work.DueTime.Minutes.HasValue)
                {
                    Console.WriteLine(
                        $"  - {work.Title} (Due: {work.DueDate.Month.Value}-{work.DueDate.Day.Value}-{work.DueDate.Year.Value} {work.DueTime.Hours.Value}:{work.DueTime.Minutes.Value:D2})");
                }
                else
                {
                    Console.WriteLine($"  - {work.Title} (Due date not fully specified)");
                }

                courseDisplayed = true;
            }
            Console.WriteLine($"    - {submission.State}");
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
        return driveFile.AlternateLink?.Contains("docs.google.com/document", StringComparison.OrdinalIgnoreCase) ??
               false;
    }
}