using Google_API_Integration.Interfaces;
using Google;
using Google.Apis.Classroom.v1.Data;

namespace Google_API_Integration.Models;

public class CourseWorkManager(IGoogleClassroomService classroomService)
{
    private readonly IGoogleClassroomService _classroomService = classroomService ?? throw new ArgumentNullException(nameof(classroomService));

    /// <summary>
    /// Gets the coursework for a specific course.
    /// </summary>
    /// <param name="courseId">ID used to identify the course</param>
    /// <returns>Coursework for specified course</returns>
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

    /// <summary>
    /// Gets all course work for all courses.
    /// </summary>
    /// <returns>Returns a dictionary of the course name and its work</returns>
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