using Google.Apis.Classroom.v1.Data;

namespace Google_Drive_Organizer.Interfaces;

public interface IGoogleClassroomService
{
    Task<IList<Course>> ListCoursesAsync(int pageSize);
    Task<Course> GetCourseAsync(string courseId);
    Task<IList<CourseWork>> GetCourseWorkAsync(string courseId);
    Task<IList<StudentSubmission>> GetStudentSubmissionsForSpecificCourseWorkAsync(string courseId, string courseWorkId);
    Task<Dictionary<string, IList<StudentSubmission>>> GetStudentSubmissionsForMultipleCourseWorksAsync(
        string courseId, IList<string> courseWorkIds);
}