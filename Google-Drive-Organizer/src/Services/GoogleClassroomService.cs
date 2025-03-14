using Google_Drive_Organizer.Interfaces;
using Google.Apis.Classroom.v1;
using Google.Apis.Classroom.v1.Data;

namespace Google_Drive_Organizer.Services;

public class GoogleClassroomService(ClassroomService classroomService) : IGoogleClassroomService
{
    private readonly ClassroomService _classroomService = classroomService ?? throw new ArgumentNullException(nameof(classroomService));

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
}