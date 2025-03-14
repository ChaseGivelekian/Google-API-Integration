using Google_Drive_Organizer.Interfaces;

namespace Google_Drive_Organizer.Models;

public class ClassroomCourseLister(IGoogleClassroomService classroomService)
{
    private readonly IGoogleClassroomService _classroomService =
        classroomService ?? throw new ArgumentNullException(nameof(classroomService));

    public async Task ListCoursesAsync()
    {
        try
        {
            var courses = await _classroomService.ListCoursesAsync(100);

            if (courses.Count == 0)
            {
                Console.WriteLine("No courses found");
                return;
            }

            Console.WriteLine("Courses:");
            foreach (var course in courses)
            {
                Console.WriteLine($"{course.Name}");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error listing courses: {e.Message}");
            throw;
        }
    }
}