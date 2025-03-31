using Google_API_Integration.Interfaces;
using Google.Apis.Classroom.v1.Data;

namespace Google_API_Integration.Models;

public class ClassroomCourseLister(IGoogleClassroomService classroomService)
{
    private readonly IGoogleClassroomService _classroomService =
        classroomService ?? throw new ArgumentNullException(nameof(classroomService));

    /// <summary>
    /// Lists all courses in Google Classroom.
    /// </summary>
    /// <param name="pageSize">Integer for how many objects to be returned per page</param>
    /// <returns>Returns a list of courses</returns>
    public async Task<IList<Course>> ListCoursesAsync(int pageSize)
    {
        try
        {
            var courses = await _classroomService.ListCoursesAsync(pageSize);

            return courses.Count == 0 ? new List<Course>() : courses;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error listing courses: {e.Message}");
            throw;
        }
    }
}