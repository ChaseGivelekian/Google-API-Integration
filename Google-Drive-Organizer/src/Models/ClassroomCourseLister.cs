using Google_Drive_Organizer.Interfaces;
using Google.Apis.Classroom.v1.Data;

namespace Google_Drive_Organizer.Models;

public class ClassroomCourseLister(IGoogleClassroomService classroomService)
{
    private readonly IGoogleClassroomService _classroomService =
        classroomService ?? throw new ArgumentNullException(nameof(classroomService));

    public async Task<IList<Course>> ListCoursesAsync()
    {
        try
        {
            var courses = await _classroomService.ListCoursesAsync(100);

            return courses.Count == 0 ? new List<Course>() : courses;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error listing courses: {e.Message}");
            throw;
        }
    }
}