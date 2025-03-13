using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using File = Google.Apis.Drive.v3.Data.File;

namespace Google_Drive_Organizer;

public static class GoogleDriveOrganizer
{
    private static readonly string[] Scopes = [DriveService.Scope.DriveReadonly];

    public static void Main()
    {
        ListFiles();
    }

    private static UserCredential GetCredential()
    {
        try
        {
            using var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read);
            const string credPath = "token.json";
            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(credPath, true)).Result;

            return credential;
        }
        catch (FileNotFoundException e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }

    private static DriveService GetService()
    {
        try
        {
            var credential = GetCredential();
            var service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential
            });

            return service;
        }
        catch (FileNotFoundException e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }

    private static void ListFiles()
    {
        var service = GetService();

        try
        {
            var listRequest = service.Files.List();
            listRequest.PageSize = 1000;
            listRequest.OrderBy = "modifiedByMeTime desc";
            listRequest.Q = "mimeType!='application/vnd.google-apps.folder'";

            IList<File> files = listRequest.Execute().Files;
            Console.WriteLine("Files:");

            // If there are no files found, print this
            if (files == null || files.Count == 0)
            {
                Console.WriteLine("No files found.");
                return;
            }

            // Print the name of each file
            foreach (var file in files)
            {
                Console.WriteLine("{0}", file.Name);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}