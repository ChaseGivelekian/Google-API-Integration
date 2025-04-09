using Google.Apis.Classroom.v1.Data;

namespace Google_API_Integration.Interfaces;

public interface IGoogleClassroomService
{
    /// <summary>
    /// Gets a list of courses.
    /// </summary>
    /// <param name="pageSize">How many objects will be returned for each page</param>
    /// <returns>Returns a list of courses</returns>
    Task<IList<Course>> ListCoursesAsync(int pageSize);

    /// <summary>
    /// Gets a specific course by its ID.
    /// </summary>
    /// <param name="courseId">Identifier for the course</param>
    /// <returns>Returns a course object</returns>
    Task<Course> GetCourseAsync(string courseId);

    /// <summary>
    /// Gets the coursework for a specific course.
    /// </summary>
    /// <param name="courseId">
    /// The ID of the course for which to get coursework.
    /// </param>
    /// <returns>
    /// Returns a list of coursework for the specified course.
    /// </returns>
    Task<IList<CourseWork>> GetCourseWorkAsync(string courseId);

    /// <summary>
    /// Gets the submissions for a specific course and coursework and returns them as a list.
    /// </summary>
    /// <param name="courseId">Identifier for the course/class</param>
    /// <param name="courseWorkId">Identifier for the coursework</param>
    /// <returns>Returns a list of student submissions.</returns>
    Task<IList<StudentSubmission>> GetStudentSubmissionsForSpecificCourseWorkAsync(string courseId, string courseWorkId);

    /// <summary>
    /// Gets student submissions for multiple course works in a single course.
    /// </summary>
    /// <param name="courseId">Identifier for the course/class</param>
    /// <param name="courseWorkIds">List of course work identifiers</param>
    /// <returns>Dictionary mapping course work IDs to their respective student submissions</returns>
    Task<Dictionary<string, IList<StudentSubmission>>> GetStudentSubmissionsForMultipleCourseWorksAsync(
        string courseId, IList<string> courseWorkIds);

    /// <summary>
    /// Gets the description of a course work.
    /// </summary>
    /// <param name="work">The Work object</param>
    /// <returns>Returns a string with the course work description</returns>
    Task<string> GetCourseWorkDescriptionAsync(CourseWork work);

    /// <summary>
    /// Turns in a student submission for a specific assignment.
    /// </summary>
    /// <param name="courseId">Identifier of the course. This Identifier can be either the Classroom-assigned identifier or an alias.</param>
    /// <param name="courseWorkId">Identifier of the course work.</param>
    /// <param name="assignmentId">Identifier of the student submission.</param>
    /// <returns>Returns a boolean for success.</returns>
    Task<bool> SubmitAssignmentAsync(string courseId, string courseWorkId, string assignmentId);
}