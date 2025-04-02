using Google_API_Integration.Interfaces;
using Google_API_Integration.Models;
using Google_API_Integration.Services;
using Google_API_Integration.Services.AttachmentTextExtraction;
using Google_API_Integration.Services.Docs;
using Google.Apis.Classroom.v1.Data;

namespace Google_API_Integration;

public class ClassroomApplication(
    CourseWorkManager courseWorkManager,
    IGoogleClassroomService googleClassroomService,
    GoogleDocsService googleDocsService,
    GoogleDocsContentService googleDocsContentService)
{
    private readonly CourseWorkManager _courseWorkManager =
        courseWorkManager ?? throw new ArgumentNullException(nameof(courseWorkManager));

    private readonly IGoogleClassroomService _googleClassroomService =
        googleClassroomService ?? throw new ArgumentNullException(nameof(googleClassroomService));

    private readonly GoogleDocsService _googleDocsService =
        googleDocsService ?? throw new ArgumentNullException(nameof(googleDocsService));

    private readonly GoogleDocsContentService _googleDocsContentService =
        googleDocsContentService ?? throw new ArgumentNullException(nameof(googleDocsContentService));

    public async Task RunAsync()
    {
        await DisplayCourseWorkInformationBatched();
    }

    private async Task DisplayCourseWorkInformationBatched()
    {
        var courses = await _courseWorkManager.GetAllCoursesWorkAsync();

        // Group all valid coursework by course ID for batch processing
        var validWorkByCourse = new Dictionary<string, List<(string courseName, CourseWork work)>>();

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
        // Collect all submissions first
        Dictionary<string, List<(string courseName, CourseWork work)>> workItemsByCourse = new();
        Dictionary<string, IList<StudentSubmission>> allSubmissionsByCourseWork = new();

        foreach (var (courseId, workItems) in validWorkByCourse)
        {
            // Get all courseWork IDs for this course
            var courseWorkIds = workItems.Select(w => w.work.Id).ToList();

            // Batch fetches all submissions for this course's work items
            var submissions =
                await _googleClassroomService.GetStudentSubmissionsForMultipleCourseWorksAsync(courseId, courseWorkIds);

            // Store work items by course ID for later processing
            workItemsByCourse[courseId] = workItems;

            // Add all submissions to the allSubmissionsByCourseWork dictionary
            foreach (var entry in submissions)
            {
                allSubmissionsByCourseWork[entry.Key] = entry.Value;
            }
        }

        // Process all results at once
        var allWorkItems = workItemsByCourse.Values.SelectMany(items => items).ToList();
        await DisplayBatchedSubmissions(allWorkItems, allSubmissionsByCourseWork);
    }

    private static (int courseNumber,
        List<(List<(string courseName, CourseWork work)> workItems,
            Dictionary<string, IList<StudentSubmission>> submissions)> coursesList,
        List<int> displayedCourseIndices) DisplayAllCourses(
            List<(string courseId,
                List<(string courseName, CourseWork work)> workItems,
                Dictionary<string, IList<StudentSubmission>> submissions)> allDisplayData)
    {
        var courseNumber = 0;
        var displayedCourseIndices = new List<int>();
        var coursesList = new List<(List<(string courseName, CourseWork work)> workItems,
            Dictionary<string, IList<StudentSubmission>> submissions)>();

        foreach (var (_, workItems, submissions) in allDisplayData)
        {
            coursesList.Add((workItems, submissions));

            for (var i = 0; i < workItems.Count; i++)
            {
                var (courseName, work) = workItems[i];

                if (!submissions.TryGetValue(work.Id, out var courseSubmissions))
                    continue;

                var courseDisplayed = true;

                foreach (var submission in courseSubmissions)
                {
                    if (!IsActiveSubmission(submission))
                        continue;

                    if (courseDisplayed)
                    {
                        courseNumber++;
                        displayedCourseIndices.Add(i);

                        Console.WriteLine($"{courseNumber}. Course: {courseName}");

                        if (work.DueDate?.Month.HasValue == true && work.DueDate.Day.HasValue &&
                            work.DueDate.Year.HasValue &&
                            work.DueTime?.Hours.HasValue == true && work.DueTime.Minutes.HasValue)
                        {
                            Console.WriteLine(
                                $"  - {work.Title} (Due: {work.DueDate.Month.Value}-{work.DueDate.Day.Value}-{work.DueDate.Year.Value} {work.DueTime.Hours.Value}:{work.DueTime.Minutes.Value:D2})");
                        }
                        else
                        {
                            Console.WriteLine($"  - {work.Title} (Due date not fully specified)");
                        }

                        courseDisplayed = false;
                    }

                    Console.WriteLine($"    - {submission.State}");
                }
            }
        }

        return (courseNumber, coursesList, displayedCourseIndices);
    }

    private async Task ProcessSelectedCourse(
        List<(List<(string courseName, CourseWork work)> workItems,
            Dictionary<string, IList<StudentSubmission>> submissions)> coursesList,
        List<int> displayedCourseIndices)
    {
        // Get user input on which course to process
        var courseToProcess = UserInputHandler.GetIntegerInput("Which course would you like to process?", 1,
            displayedCourseIndices.Count);

        // Get the correct course list index
        var courseListIndex = courseToProcess - 1;

        // Get the selected work item index
        var selectedWorkItemIndex = displayedCourseIndices[courseToProcess - 1];

        var workItems = coursesList[courseListIndex].workItems;
        var submissions = coursesList[courseListIndex].submissions;

        var workId = submissions[workItems[selectedWorkItemIndex].work.Id];
        var documents = await _googleDocsService.GetGoogleDoc(workId);

        // This gets the chosen course work's description
        var courseWorkDescription = workItems[selectedWorkItemIndex].work.Description;
        Console.WriteLine(courseWorkDescription);

        // This displays the content of the document
        foreach (var document in documents)
        {
            var content = await _googleDocsContentService.ExtractDocumentContent(document);
            Console.WriteLine(content);
        }

        // This gets the text from attachments
        var attachmentText =
            await AttachmentTextExtractor.ExtractTextFromAttachmentsAsync(workItems[selectedWorkItemIndex].work);
        Console.WriteLine("This is the text from the attachments:");
        foreach (var text in attachmentText)
        {
            Console.WriteLine(text);
        }
    }

    private static bool HasValidDueDate(CourseWork work)
    {
        return work is { DueDate: not null, DueTime: not null };
    }

    private static bool IsPastDue(CourseWork work)
    {
        // Ensure all required values are present
        if (!work.DueDate.Year.HasValue || !work.DueDate.Month.HasValue || !work.DueDate.Day.HasValue)
        {
            return false; // Can't determine if it's past due without complete date/time
        }

        // If we have complete date and time information
        if (work.DueTime is { Hours: not null, Minutes: not null })
        {
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

        // If we only have date information without time
        var dueDate = new DateTime(
            work.DueDate.Year.Value,
            work.DueDate.Month.Value,
            work.DueDate.Day.Value
        );
        return dueDate.Date < DateTime.Now.Date ||
               (dueDate.Date == DateTime.Now.Date && DateTime.Now.TimeOfDay > TimeSpan.Zero);
    }

    private static bool IsActiveSubmission(StudentSubmission submission)
    {
        // Check if submission is in one of the active states
        return submission.State is "NEW" or "CREATED" or "RECLAIMED_BY_STUDENT";
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