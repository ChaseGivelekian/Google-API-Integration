using Google_Drive_Organizer.Models;
using Google_Drive_Organizer.Services;
using Google_Drive_Organizer.Interfaces;
using Google_Drive_Organizer.Services.Classroom;
using Google_Drive_Organizer.Services.Docs;
using Microsoft.Extensions.DependencyInjection;

namespace Google_Drive_Organizer;

public static class Program
{
    public static async Task Main()
    {
        try
        {
            // Configure services
            var serviceProvider = ConfigureServices();

            // Get the application and run it
            var app = serviceProvider.GetRequiredService<ClassroomApplication>();
            await app.RunAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Application error: {e.Message}");
            Environment.Exit(1);
        }
        // var docsService = GoogleCredentialsManager.CreateDocsServiceAsync().GetAwaiter().GetResult();
        // var document = await new GoogleDocsContentService(docsService).GetDocumentAsync(
        //     "1_a7fdEZFwkzUhGgFkqBvusqtb3L1csfBWJ8CyzhWWKs");
        // var content = await new GoogleDocsContentService(docsService).ExtractDocumentContent(document);
        // Console.WriteLine(content);
    }

    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Register services
        services.AddSingleton(_ =>
            GoogleCredentialsManager.CreateClassroomServiceAsync().GetAwaiter().GetResult());
        services.AddSingleton<IGoogleClassroomService, GoogleClassroomService>();
        services.AddSingleton<CourseWorkManager>();
        services.AddSingleton<ClassroomApplication>();
        services.AddSingleton<GoogleDocsService>();
        services.AddSingleton<GoogleDocsContentService>();

        return services.BuildServiceProvider();
    }
}