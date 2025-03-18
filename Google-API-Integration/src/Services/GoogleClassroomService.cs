using Google_Drive_Organizer.Interfaces;
using Google.Apis.Classroom.v1;
using Google.Apis.Classroom.v1.Data;

namespace Google_Drive_Organizer.Services;

public class GoogleClassroomService(ClassroomService classroomService) : IGoogleClassroomService
{
    private readonly ClassroomService _classroomService = classroomService ?? throw new ArgumentNullException(nameof(classroomService));

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
    /// <param name="courseWorkId">Identifier for the coursework.</param>
    /// <returns>Returns a list of student submissions.</returns>
    public async Task<IList<StudentSubmission>> GetStudentSubmissionsForSpecificCourseWorkAsync(string courseId, string courseWorkId)
    {
        if (string.IsNullOrEmpty(courseWorkId))
        {
            throw new ArgumentNullException(nameof(courseWorkId));
        }

        var request = _classroomService.Courses.CourseWork.StudentSubmissions.List(courseId, courseWorkId);
        var result = await request.ExecuteAsync();
        return result.StudentSubmissions ?? new List<StudentSubmission>();
    }
}