using Google_API_Integration.Interfaces;
using Google.Apis.Classroom.v1;
using Google.Apis.Classroom.v1.Data;

namespace Google_API_Integration.Services.Classroom;

public class GoogleClassroomService(ClassroomService classroomService) : IGoogleClassroomService
{
    private readonly ClassroomService _classroomService =
        classroomService ?? throw new ArgumentNullException(nameof(classroomService));

    public async Task<IList<Course>> ListCoursesAsync(int pageSize)
    {
        var request = _classroomService.Courses.List();
        request.PageSize = pageSize;

        var result = await request.ExecuteAsync();
        return result.Courses;
    }

    public async Task<Course> GetCourseAsync(string courseId)
    {
        if (string.IsNullOrEmpty(courseId))
        {
            throw new ArgumentNullException(nameof(courseId));
        }

        var request = _classroomService.Courses.Get(courseId);
        return await request.ExecuteAsync();
    }

    public async Task<IList<CourseWork>> GetCourseWorkAsync(string courseId)
    {
        if (string.IsNullOrEmpty(courseId))
        {
            throw new ArgumentNullException(nameof(courseId));
        }

        var request = _classroomService.Courses.CourseWork.List(courseId);
        var result = await request.ExecuteAsync();
        return result.CourseWork ?? new List<CourseWork>();
    }


    public async Task<IList<StudentSubmission>> GetStudentSubmissionsForSpecificCourseWorkAsync(string courseId,
        string courseWorkId)
    {
        if (string.IsNullOrEmpty(courseWorkId))
        {
            throw new ArgumentNullException(nameof(courseWorkId));
        }

        var request = _classroomService.Courses.CourseWork.StudentSubmissions.List(courseId, courseWorkId);
        var result = await request.ExecuteAsync();
        return result.StudentSubmissions ?? new List<StudentSubmission>();
    }

    public async Task<Dictionary<string, IList<StudentSubmission>>> GetStudentSubmissionsForMultipleCourseWorksAsync(
        string courseId, IList<string> courseWorkIds)
    {
        if (string.IsNullOrEmpty(courseId))
        {
            throw new ArgumentNullException(nameof(courseId));
        }

        if (courseWorkIds == null || !courseWorkIds.Any())
        {
            throw new ArgumentException("Course work IDs cannot be null or empty", nameof(courseWorkIds));
        }

        var result = new Dictionary<string, IList<StudentSubmission>>();

        // Fetches each course work's submissions separate but concurrently
        var tasks = courseWorkIds.Select(async courseWorkId =>
        {
            var submissions = await GetStudentSubmissionsForSpecificCourseWorkAsync(courseId, courseWorkId);
            return (courseWorkId, submissions);
        });

        // Wait for all requests to complete
        var results = await Task.WhenAll(tasks);

        // Build the result dictionary
        foreach (var (courseWorkId, submissions) in results)
        {
            result[courseWorkId] = submissions;
        }

        return result;
    }

    public Task<string> GetCourseWorkDescriptionAsync(CourseWork work)
    {
        ArgumentNullException.ThrowIfNull(work);

        return Task.FromResult(work.Description);
    }

    public Task<bool> SubmitAssignmentAsync(string courseId, string courseWorkId, string assignmentId)
    {
        try
        {
            _classroomService.Courses.CourseWork.StudentSubmissions.TurnIn(new TurnInStudentSubmissionRequest(),
                courseId, courseWorkId, assignmentId);
            return Task.FromResult(true);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return Task.FromResult(false);
        }
    }
}