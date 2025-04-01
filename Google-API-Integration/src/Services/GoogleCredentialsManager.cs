using Google.Apis.Auth.OAuth2;
using Google.Apis.Classroom.v1;
using Google.Apis.Docs.v1;
using Google.Apis.Drive.v3;
using Google.Apis.Forms.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;

namespace Google_API_Integration.Services;

public static class GoogleCredentialsManager
{
    private static readonly string[] Scopes =
    [
        DriveService.Scope.Drive,
        ClassroomService.Scope.ClassroomCoursesReadonly,
        ClassroomService.Scope.ClassroomCourseworkMeReadonly,
        YouTubeService.Scope.YoutubeReadonly
    ];

    private const string CredentialsPath = "credentials.json";
    private const string TokenPath = "token.json";

    /// <summary>
    /// Gets the user credential asynchronously.
    /// </summary>
    /// <returns>Returns an authorization for the user.</returns>
    private static async Task<UserCredential> GetUserCredentialAsync()
    {
        await using var stream = new FileStream(CredentialsPath, FileMode.Open, FileAccess.Read);
        var secrets = await GoogleClientSecrets.FromStreamAsync(stream);

        return await GoogleWebAuthorizationBroker.AuthorizeAsync(
            secrets.Secrets,
            Scopes,
            "user",
            CancellationToken.None,
            new FileDataStore(TokenPath, true));
    }

    /// <summary>
    /// Creates a Drive service asynchronously.
    /// </summary>
    /// <returns>Returns a new DriveService</returns>
    public static async Task<DriveService> CreateDriveServiceAsync()
    {
        var credential = await GetUserCredentialAsync();
        return new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential
        });
    }

    /// <summary>
    /// Creates a Classroom service asynchronously.
    /// </summary>
    /// <returns>Returns a new ClassroomService</returns>
    public static async Task<ClassroomService> CreateClassroomServiceAsync()
    {
        var credential = await GetUserCredentialAsync();
        return new ClassroomService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential
        });
    }

    /// <summary>
    /// Creates a Docs service asynchronously.
    /// </summary>
    /// <returns>Returns a new DocService</returns>
    public static async Task<DocsService> CreateDocsServiceAsync()
    {
        var credential = await GetUserCredentialAsync();
        return new DocsService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential
        });
    }

    /// <summary>
    /// Creates a YouTube service asynchronously.
    /// </summary>
    /// <returns>Returns a new YouTubeService</returns>
    public static async Task<YouTubeService> CreateYouTubeServiceAsync()
    {
        var credential = await GetUserCredentialAsync();
        return new YouTubeService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential
        });
    }

    /// <summary>
    /// Creates a Forms service asynchronously.
    /// </summary>
    /// <returns>Returns a new FormsService</returns>
    public static async Task<FormsService> CreateFormsServiceAsync()
    {
        var credential = await GetUserCredentialAsync();
        return new FormsService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential
        });
    }
}