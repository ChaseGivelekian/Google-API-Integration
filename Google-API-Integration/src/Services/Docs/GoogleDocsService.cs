using Google_API_Integration.Exceptions;
using Google_API_Integration.Interfaces;
using Google_API_Integration.Services.Docs.DocsContentProcessing;
using Google;
using Google.Apis.Classroom.v1.Data;
using Google.Apis.Docs.v1;
using Google.Apis.Docs.v1.Data;

namespace Google_API_Integration.Services.Docs;

public class GoogleDocsService
{
    private readonly DocsService _docsService =
        GoogleCredentialsManager.CreateDocsServiceAsync().GetAwaiter().GetResult() ?? throw new NullReferenceException();

    public async Task<List<Document>> GetGoogleDoc(IList<StudentSubmission> work)
    {
        var results = new List<Document>();

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
                 results.Add(await _docsService.Documents.Get(driveFileId).ExecuteAsync());
            }
            catch (GoogleApiException e) when (e.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new DocumentNotFoundException(driveFileId, e);
            }
        }

        return results;
    }
}