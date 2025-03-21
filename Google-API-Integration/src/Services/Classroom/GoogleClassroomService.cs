using Google_Drive_Organizer.Interfaces;
using Google.Apis.Classroom.v1;
using Google.Apis.Classroom.v1.Data;

namespace Google_Drive_Organizer.Services.Classroom;

public class GoogleClassroomService(ClassroomService classroomService) : IGoogleClassroomService
{
    private readonly ClassroomService _classroomService =
        classroomService ?? throw new ArgumentNullException(nameof(classroomService));

    /// <summary>
    /// Gets a list of courses.
    /// </summary>
    /// <param name="pageSize">How many objects will be returned for each page</param>
    /// <returns>Returns a list of courses</returns>
    public async Task<IList<Course>> ListCoursesAsync(int pageSize)
    {
        var request = _classroomService.Courses.List();
        request.PageSize = pageSize;

        var result = await request.ExecuteAsync();
        return result.Courses;
    }

    /// <summary>
    /// Gets a specific course by its ID.
    /// </summary>
    /// <param name="courseId">Identifier for the course</param>
    /// <returns>Returns a course object</returns>
    public async Task<Course> GetCourseAsync(string courseId)
    {
        if (string.IsNullOrEmpty(courseId))
        {
            throw new ArgumentNullException(nameof(courseId));
        }

        var request = _classroomService.Courses.Get(courseId);
        return await request.ExecuteAsync();
    }

    /// <summary>
    /// Gets the coursework for a specific course.
    /// </summary>
    /// <param name="courseId">
    /// The ID of the course for which to get coursework.
    /// </param>
    /// <returns>
    /// Returns a list of coursework for the specified course.
    /// </returns>
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

    /// <summary>
    /// Gets the submissions for a specific course and coursework and returns them as a list.
    /// </summary>
    /// <param name="courseId">Identifier for the course/class</param>
    /// <param name="courseWorkId">Identifier for the coursework</param>
    /// <returns>Returns a list of student submissions.</returns>
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

    /// <summary>
    /// Gets student submissions for multiple course works in a single course.
    /// </summary>
    /// <param name="courseId">Identifier for the course/class</param>
    /// <param name="courseWorkIds">List of course work identifiers</param>
    /// <returns>Dictionary mapping course work IDs to their respective student submissions</returns>
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

    /// <summary>
    /// Gets the description of a course work.
    /// </summary>
    /// <param name="work">The Work object</param>
    /// <returns>Returns a string with the course work description</returns>
    public Task<string> GetCourseWorkDescriptionAsync(CourseWork work)
    {
        ArgumentNullException.ThrowIfNull(work);

        return Task.FromResult(work.Description);
    }
}