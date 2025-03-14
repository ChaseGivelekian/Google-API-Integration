using Google_Drive_Organizer.Interfaces;
using Google;
using Google.Apis.Classroom.v1.Data;

namespace Google_Drive_Organizer.Models;

public class CourseWorkManager(IGoogleClassroomService classroomService)
{
    private readonly IGoogleClassroomService _classroomService = classroomService ?? throw new ArgumentNullException(nameof(classroomService));

    public async Task<IList<CourseWork>> GetCourseWorksAsync(string courseId)
    {
        try
        {
            return await _classroomService.GetCourseWorkAsync(courseId);
        }
        catch (GoogleApiException)
        {
            Console.WriteLine($"Failed to get coursework for course {courseId}.");
            throw;
        }
    }

    public async Task<Dictionary<string, IList<CourseWork>>> GetAllCoursesWorkAsync()
    {
        try
        {
            var courses = await _classroomService.ListCoursesAsync(100);
            var courseWork = new Dictionary<string, IList<CourseWork>>();

            foreach (var course in courses)
            {
                var work = await _classroomService.GetCourseWorkAsync(course.Id);
                courseWork.Add(course.Name, work);
            }

            return courseWork;
        }
        catch (GoogleApiException)
        {
            Console.WriteLine("Failed to get coursework for all courses.");
            throw;
        }
    }
}