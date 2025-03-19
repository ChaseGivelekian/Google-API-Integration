using Google.Apis.Classroom.v1.Data;
using Google.Apis.Docs.v1;

namespace Google_Drive_Organizer.Services;

public class GoogleDocsService
{
    private readonly DocsService _docsService =
        GoogleCredentialsManager.CreateDocsServiceAsync().GetAwaiter().GetResult() ?? throw new NullReferenceException();

    public async Task GetGoogleDoc(IList<StudentSubmission> work)
    {
        foreach (var studentSubmission in work)
        {
            // Get the attachment or link from the submission
            var attachment = studentSubmission.AssignmentSubmission?.Attachments?.FirstOrDefault();
            if (attachment?.DriveFile == null) continue;

            // Extract the Drive file ID which can be used with Docs API
            var driveFileId = attachment.DriveFile.Id;
            if (string.IsNullOrEmpty(driveFileId)) continue;

            try
            {
                var result = await _docsService.Documents.Get(driveFileId).ExecuteAsync();
                Console.WriteLine($"Retrieved document: {result.Title}");
            }
            catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Console.WriteLine($"Document not found for submission: {studentSubmission.Id}");
            }
        }
    }
}