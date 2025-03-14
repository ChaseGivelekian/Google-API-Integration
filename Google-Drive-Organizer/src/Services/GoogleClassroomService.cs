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
}